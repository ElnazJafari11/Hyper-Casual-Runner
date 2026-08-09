using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    /// <summary>Buy generators / businesses / dimensions / shafts. Cost = base * growth^owned.</summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct IdleBuyGeneratorSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IdleBuyGeneratorEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (buyEvt, evtEntity) in SystemAPI.Query<RefRO<IdleBuyGeneratorEvent>>().WithEntityAccess())
            {
                int genId = buyEvt.ValueRO.GeneratorId;
                int amount = buyEvt.ValueRO.Amount <= 0 ? 1 : buyEvt.ValueRO.Amount;

                // Prefer co-located BuyableGenerator + IdleSliceState
                bool handled = false;
                foreach (var (gen, slice) in SystemAPI.Query<RefRW<BuyableGenerator>, RefRW<IdleSliceState>>())
                {
                    if (genId != 0 && gen.ValueRO.GeneratorId != genId) continue;
                    handled = true;
                    Purchase(ref gen.ValueRW, ref slice.ValueRW, amount);
                }

                if (!handled)
                {
                    BuyOnSeparateEntities(ref state, genId, amount);
                }

                foreach (var run in SystemAPI.Query<RefRW<CurrentRunStats>>())
                {
                    foreach (var slice in SystemAPI.Query<RefRO<IdleSliceState>>())
                    {
                        run.ValueRW.CurrentGold = slice.ValueRO.PrimaryCurrency;
                        break;
                    }
                }

                SystemAPI.SetComponentEnabled<IdleBuyGeneratorEvent>(evtEntity, false);
            }
        }

        private static void Purchase(ref BuyableGenerator gen, ref IdleSliceState slice, int amount)
        {
            for (int n = 0; n < amount; n++)
            {
                float growth = gen.CostGrowth <= 1f ? 1.15f : gen.CostGrowth;
                double cost = gen.BaseCost * System.Math.Pow(growth, gen.OwnedCount);
                if (slice.PrimaryCurrency < cost) break;

                slice.PrimaryCurrency -= cost;
                gen.OwnedCount += 1;
                slice.OwnedGenerators = gen.OwnedCount;

                bool auto = !gen.RequiresManager || gen.IsAutomated;
                if (auto)
                {
                    slice.PassiveRate = gen.BaseCps * gen.OwnedCount * slice.GlobalMultiplier;
                }
            }
        }

        private static void BuyOnSeparateEntities(ref SystemState state, int genId, int amount)
        {
            foreach (var slice in SystemAPI.Query<RefRW<IdleSliceState>>())
            {
                foreach (var gen in SystemAPI.Query<RefRW<BuyableGenerator>>())
                {
                    if (genId != 0 && gen.ValueRO.GeneratorId != genId) continue;
                    Purchase(ref gen.ValueRW, ref slice.ValueRW, amount);
                }
            }
        }
    }

    /// <summary>AdVenture Capitalist managers: hire once → automate matching generator.</summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct IdleManagerHireSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IdleHireManagerEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (hire, evtEntity) in SystemAPI.Query<RefRO<IdleHireManagerEvent>>().WithEntityAccess())
            {
                int targetId = hire.ValueRO.TargetGeneratorId;

                foreach (var slice in SystemAPI.Query<RefRW<IdleSliceState>>())
                {
                    foreach (var manager in SystemAPI.Query<RefRW<IdleManager>>())
                    {
                        if (manager.ValueRO.IsHired) continue;
                        if (targetId != 0 && manager.ValueRO.TargetGeneratorId != targetId) continue;
                        if (slice.ValueRO.PrimaryCurrency < manager.ValueRO.HireCost) continue;

                        slice.ValueRW.PrimaryCurrency -= manager.ValueRO.HireCost;
                        manager.ValueRW.IsHired = true;
                        slice.ValueRW.ManagersHired += 1;

                        foreach (var gen in SystemAPI.Query<RefRW<BuyableGenerator>>())
                        {
                            if (targetId != 0 && gen.ValueRO.GeneratorId != manager.ValueRO.TargetGeneratorId)
                                continue;

                            gen.ValueRW.IsAutomated = true;
                            gen.ValueRW.RequiresManager = false;
                            slice.ValueRW.PassiveRate = System.Math.Max(
                                slice.ValueRO.PassiveRate,
                                gen.ValueRO.BaseCps * System.Math.Max(1, gen.ValueRO.OwnedCount) *
                                slice.ValueRO.GlobalMultiplier);
                        }
                    }
                }

                SystemAPI.SetComponentEnabled<IdleHireManagerEvent>(evtEntity, false);
            }
        }
    }
}
