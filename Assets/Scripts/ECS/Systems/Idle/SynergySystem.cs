using Unity.Entities;
using Unity.Collections;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(IdleProductionSystem))]
    public partial struct SynergySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ProducerComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var synergyCounts = new NativeParallelHashMap<int, int>(16, Allocator.Temp);

            foreach (var tag in SystemAPI.Query<RefRO<SynergyTagComponent>>())
            {
                int id = tag.ValueRO.SynergyId;
                if (synergyCounts.ContainsKey(id))
                {
                    synergyCounts[id] = synergyCounts[id] + 1;
                }
                else
                {
                    synergyCounts.TryAdd(id, 1);
                }
            }

            foreach (var (producer, buff) in SystemAPI.Query<RefRW<ProducerComponent>, RefRO<SynergyBuffComponent>>())
            {
                int reqId = buff.ValueRO.RequiredSynergyId;
                double baseMultiplier = 1.0;
                if (synergyCounts.TryGetValue(reqId, out int count) && count > 0)
                {
                    baseMultiplier += buff.ValueRO.MultiplierBonus * count;
                }
                producer.ValueRW.Multiplier = baseMultiplier;
            }

            synergyCounts.Dispose();
        }
    }
}
