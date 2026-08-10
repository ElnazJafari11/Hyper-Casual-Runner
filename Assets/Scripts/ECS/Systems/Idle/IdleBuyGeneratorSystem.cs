using Unity.Entities;
using Unity.Collections;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    /// <summary>Buy generators / businesses / dimensions / shafts / hero DPS.</summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct IdleBuyGeneratorSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IdleBuyGeneratorEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            Entity sole = IdleEventTarget.FindSoleSlice(em);

            foreach (var (buyEvt, evtEntity) in SystemAPI.Query<RefRO<IdleBuyGeneratorEvent>>().WithEntityAccess())
            {
                int genId = buyEvt.ValueRO.GeneratorId;
                int amount = buyEvt.ValueRO.Amount <= 0 ? 1 : buyEvt.ValueRO.Amount;
                Entity sliceEntity = IdleEventTarget.Resolve(em, evtEntity, buyEvt.ValueRO.TargetSlice, sole);

                if (sliceEntity != Entity.Null && em.HasComponent<IdleSliceState>(sliceEntity))
                {
                    var slice = em.GetComponentData<IdleSliceState>(sliceEntity);
                    bool bought = false;

                    if (em.HasComponent<BuyableGenerator>(sliceEntity))
                    {
                        var gen = em.GetComponentData<BuyableGenerator>(sliceEntity);
                        if (genId == 0 || gen.GeneratorId == genId)
                        {
                            Purchase(ref gen, ref slice, amount);
                            em.SetComponentData(sliceEntity, gen);
                            bought = true;
                        }
                    }

                    if (!bought)
                        PurchaseHeroDpsFallback(ref slice, amount);

                    ApplyCombatHeroBoost(ref slice, em, sliceEntity);
                    em.SetComponentData(sliceEntity, slice);
                    IdleEventTarget.SyncPairedRunGold(em, sliceEntity, slice.PrimaryCurrency);
                }

                SystemAPI.SetComponentEnabled<IdleBuyGeneratorEvent>(evtEntity, false);
                IdleEventTarget.DestroyIfEphemeral(em, ecb, evtEntity, sliceEntity);
            }

            ecb.Playback(em);
            ecb.Dispose();
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

                // PassiveRate = BaseCps * Owned only — GlobalMultiplier applied once in simulation
                bool auto = !gen.RequiresManager || gen.IsAutomated;
                if (auto)
                    slice.PassiveRate = IdlePrestigeMath.ComputePassiveRate(gen.BaseCps, gen.OwnedCount);
            }
        }

        private static bool PurchaseHeroDpsFallback(ref IdleSliceState slice, int amount)
        {
            bool any = false;
            for (int n = 0; n < amount; n++)
            {
                double cost = 20.0 * System.Math.Pow(1.2, slice.OwnedGenerators);
                if (slice.PrimaryCurrency < cost) break;
                slice.PrimaryCurrency -= cost;
                slice.OwnedGenerators += 1;
                slice.PassiveRate += 1.0; // mult applied in sim / combat, not here
                slice.ClickPower += 0.5;
                any = true;
            }
            return any;
        }

        private static void ApplyCombatHeroBoost(ref IdleSliceState slice, EntityManager em, Entity sliceEntity)
        {
            if (slice.Archetype != IdleArchetype.ClickerHeroes &&
                slice.Archetype != IdleArchetype.TapTitans2 &&
                slice.Archetype != IdleArchetype.IdleHeroes)
                return;

            if (slice.PassiveRate < slice.OwnedGenerators)
                slice.PassiveRate = System.Math.Max(slice.PassiveRate, slice.OwnedGenerators);

            if (em.HasComponent<IdleCombatState>(sliceEntity))
            {
                var combat = em.GetComponentData<IdleCombatState>(sliceEntity);
                // Single DPS field: PassiveRate / owned heroes — first buy must raise a bootstrap floor of 1.
                combat.HeroDps = System.Math.Max(slice.PassiveRate, 1.0 + slice.OwnedGenerators);
                combat.TapDamage = slice.ClickPower > 0 ? slice.ClickPower : combat.TapDamage;
                em.SetComponentData(sliceEntity, combat);
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
            var em = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            Entity sole = IdleEventTarget.FindSoleSlice(em);

            foreach (var (hire, evtEntity) in SystemAPI.Query<RefRO<IdleHireManagerEvent>>().WithEntityAccess())
            {
                int targetId = hire.ValueRO.TargetGeneratorId;
                Entity sliceEntity = IdleEventTarget.Resolve(em, evtEntity, hire.ValueRO.TargetSlice, sole);

                if (sliceEntity != Entity.Null &&
                    em.HasComponent<IdleSliceState>(sliceEntity) &&
                    em.HasComponent<IdleManager>(sliceEntity))
                {
                    var slice = em.GetComponentData<IdleSliceState>(sliceEntity);
                    var manager = em.GetComponentData<IdleManager>(sliceEntity);

                    if (!manager.IsHired &&
                        (targetId == 0 || manager.TargetGeneratorId == targetId) &&
                        slice.PrimaryCurrency >= manager.HireCost)
                    {
                        slice.PrimaryCurrency -= manager.HireCost;
                        manager.IsHired = true;
                        slice.ManagersHired += 1;
                        em.SetComponentData(sliceEntity, manager);

                        if (em.HasComponent<BuyableGenerator>(sliceEntity))
                        {
                            var gen = em.GetComponentData<BuyableGenerator>(sliceEntity);
                            if (targetId == 0 || gen.GeneratorId == manager.TargetGeneratorId)
                            {
                                // Keep RequiresManager so prestige/phase can restore the gate.
                                gen.IsAutomated = true;
                                slice.PassiveRate = System.Math.Max(
                                    slice.PassiveRate,
                                    IdlePrestigeMath.ComputePassiveRate(gen.BaseCps, gen.OwnedCount));
                                em.SetComponentData(sliceEntity, gen);
                            }
                        }

                        em.SetComponentData(sliceEntity, slice);
                        IdleEventTarget.SyncPairedRunGold(em, sliceEntity, slice.PrimaryCurrency);
                    }
                }

                SystemAPI.SetComponentEnabled<IdleHireManagerEvent>(evtEntity, false);
                IdleEventTarget.DestroyIfEphemeral(em, ecb, evtEntity, sliceEntity);
            }

            ecb.Playback(em);
            ecb.Dispose();
        }
    }
}
