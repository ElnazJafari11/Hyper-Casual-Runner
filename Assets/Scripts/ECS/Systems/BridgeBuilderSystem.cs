using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct BridgeBuilderSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<LevelStateComponent>(out var levelState) || levelState.CurrentState != GameState.Playing)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

            foreach (var (builder, transform, runStats) in SystemAPI.Query<RefRW<BridgeBuilderComponent>, RefRO<LocalTransform>, RefRW<CurrentRunStats>>().WithAll<PlayerComponent>())
            {
                float3 playerPos = transform.ValueRO.Position;
                bool isOverGap = false;

                // Check against all GapZones
                foreach (var gap in SystemAPI.Query<RefRO<GapZoneComponent>>())
                {
                    if (playerPos.x >= gap.ValueRO.MinBounds.x && playerPos.x <= gap.ValueRO.MaxBounds.x &&
                        playerPos.z >= gap.ValueRO.MinBounds.z && playerPos.z <= gap.ValueRO.MaxBounds.z)
                    {
                        isOverGap = true;
                        break;
                    }
                }

                if (isOverGap)
                {
                    if (builder.ValueRO.LastPlankZ < -9000f)
                    {
                        builder.ValueRW.LastPlankZ = playerPos.z;
                    }

                    float distSinceLastPlank = playerPos.z - builder.ValueRO.LastPlankZ;
                    float distStep = math.max(0.5f, builder.ValueRO.DistancePerPlank);

                    if (distSinceLastPlank >= distStep)
                    {
                        if (runStats.ValueRO.CurrentGold >= 1.0)
                        {
                            runStats.ValueRW.CurrentGold -= 1.0;
                            builder.ValueRW.LastPlankZ = playerPos.z;

                            if (builder.ValueRO.BridgeTilePrefab != Entity.Null)
                            {
                                var plankEntity = ecb.Instantiate(builder.ValueRO.BridgeTilePrefab);
                                var plankTransform = LocalTransform.FromPositionRotation(
                                    new float3(playerPos.x, 0.05f, playerPos.z),
                                    quaternion.identity
                                );
                                ecb.SetComponent(plankEntity, plankTransform);
                            }

                            // Trigger placement sound
                            var audioEntity = ecb.CreateEntity();
                            ecb.AddComponent(audioEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                        }
                        else
                        {
                            // Ran out of planks while over a gap -> Defeat
                            foreach (var stateComp in SystemAPI.Query<RefRW<LevelStateComponent>>())
                            {
                                stateComp.ValueRW.CurrentState = GameState.Defeat;
                            }
                        }
                    }
                }
                else
                {
                    // Reset when safe on ground
                    builder.ValueRW.LastPlankZ = -9999f;
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
