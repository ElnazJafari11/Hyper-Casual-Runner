using Unity.Entities;
using Unity.Collections;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct PrestigeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PrestigeEventComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (_, entity) in SystemAPI.Query<RefRO<PrestigeEventComponent>>().WithEntityAccess())
            {
                // Increment PrestigeCurrency for all entities with PersistentPlayerStats
                foreach (var persistentStats in SystemAPI.Query<RefRW<PersistentPlayerStats>>())
                {
                    persistentStats.ValueRW.PrestigeCurrency += 1.0;
                }

                // Reset CurrentGold and CurrentDistance for all entities with CurrentRunStats
                foreach (var currentRunStats in SystemAPI.Query<RefRW<CurrentRunStats>>())
                {
                    currentRunStats.ValueRW.CurrentGold = 0.0;
                    currentRunStats.ValueRW.CurrentDistance = 0;
                }

                // Clear all ResourceWallet dynamic buffers across all entities
                foreach (var walletBuffer in SystemAPI.Query<DynamicBuffer<ResourceWallet>>())
                {
                    walletBuffer.Clear();
                }

                // Reset Timer = 0 and Multiplier = 1 for all ProducerComponent entities
                foreach (var producer in SystemAPI.Query<RefRW<ProducerComponent>>())
                {
                    producer.ValueRW.Timer = 0f;
                    producer.ValueRW.Multiplier = 1.0;
                }

                // Idle toolkit slices: convert run currency → prestige mult, reset generators
                foreach (var slice in SystemAPI.Query<RefRW<IdleSliceState>>())
                {
                    double converted = System.Math.Floor(System.Math.Sqrt(System.Math.Max(0, slice.ValueRO.PrimaryCurrency) / 50.0));
                    if (converted < 1 && slice.ValueRO.PrimaryCurrency >= 25) converted = 1;
                    if (converted < 1) converted = 1;

                    slice.ValueRW.PrestigeCurrency += converted;
                    slice.ValueRW.PrimaryCurrency = 0;
                    slice.ValueRW.OwnedGenerators = 0;
                    slice.ValueRW.PassiveRate = 0;
                    slice.ValueRW.ManagersHired = 0;
                    slice.ValueRW.AssignedWorkers = 0;
                    slice.ValueRW.ProgressionLevel = 0;
                    slice.ValueRW.EnemyHp = 0;
                    slice.ValueRW.EnemyMaxHp = 0;
                    slice.ValueRW.ClickPower = 1 + slice.ValueRO.PrestigeCurrency * 0.5;
                    slice.ValueRW.GlobalMultiplier = 1f + (float)slice.ValueRO.PrestigeCurrency * 0.1f;
                }

                foreach (var gen in SystemAPI.Query<RefRW<BuyableGenerator>>())
                {
                    gen.ValueRW.OwnedCount = 0;
                }

                foreach (var manager in SystemAPI.Query<RefRW<IdleManager>>())
                {
                    manager.ValueRW.IsHired = false;
                }

                // Fire massive hybrid Audio and VFX to celebrate the Prestige!
                var soundEntity = ecb.CreateEntity();
                ecb.AddComponent(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Victory });
                
                // Disable PrestigeEventComponent on the triggering entity
                SystemAPI.SetComponentEnabled<PrestigeEventComponent>(entity, false);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
