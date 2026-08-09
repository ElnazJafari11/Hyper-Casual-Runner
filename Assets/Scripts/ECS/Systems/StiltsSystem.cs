using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct StiltsSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<LevelStateComponent>(out var levelState) || levelState.CurrentState != GameState.Playing)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

            foreach (var (stilts, transform, runStats) in SystemAPI.Query<RefRW<StiltsComponent>, RefRW<LocalTransform>, RefRW<CurrentRunStats>>().WithAll<PlayerComponent>())
            {
                // Sync current heels from run stats
                int currentHeels = (int)runStats.ValueRO.CurrentGold;
                stilts.ValueRW.CurrentHeels = currentHeels;

                // Adjust player position Y based on heel height
                float targetY = 1.0f + (currentHeels * stilts.ValueRO.HeelHeight);
                float3 pos = transform.ValueRO.Position;
                pos.y = math.lerp(pos.y, targetY, SystemAPI.Time.DeltaTime * 10f);
                transform.ValueRW.Position = pos;

                // Check collision with HurdleWalls
                foreach (var (wallTransform, wall, wallEntity) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<HurdleWallComponent>>().WithEntityAccess())
                {
                    if (wall.ValueRO.Cleared) continue;

                    float distSq = math.distancesq(pos, wallTransform.ValueRO.Position);
                    float thresholdSq = wall.ValueRO.CollisionRadius * wall.ValueRO.CollisionRadius;

                    if (distSq < thresholdSq)
                    {
                        float currentTotalHeight = currentHeels * stilts.ValueRO.HeelHeight;

                        if (currentTotalHeight >= wall.ValueRO.RequiredHeight)
                        {
                            // Pass over hurdle -> Consume heels required
                            int consumedHeels = (int)math.ceil(wall.ValueRO.RequiredHeight / math.max(0.01f, stilts.ValueRO.HeelHeight));
                            runStats.ValueRW.CurrentGold = math.max(0, runStats.ValueRO.CurrentGold - consumedHeels);
                            stilts.ValueRW.CurrentHeels = (int)runStats.ValueRO.CurrentGold;
                            wall.ValueRW.Cleared = true;

                            // Sound effect
                            var audioEntity = ecb.CreateEntity();
                            ecb.AddComponent(audioEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                        }
                        else
                        {
                            // Failed to clear hurdle -> Defeat
                            foreach (var stateComp in SystemAPI.Query<RefRW<LevelStateComponent>>())
                            {
                                stateComp.ValueRW.CurrentState = GameState.Defeat;
                            }
                        }
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
