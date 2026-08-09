using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct AirborneSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<LevelStateComponent>(out var levelState) || levelState.CurrentState != GameState.Playing)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);
            float dt = SystemAPI.Time.DeltaTime;

            foreach (var (airborne, transform, runStats) in SystemAPI.Query<RefRW<AirborneComponent>, RefRW<LocalTransform>, RefRW<CurrentRunStats>>().WithAll<PlayerComponent>())
            {
                float3 pos = transform.ValueRO.Position;

                // 1. Check Ramp Collision if not airborne
                if (!airborne.ValueRO.IsAirborne)
                {
                    foreach (var (rampTransform, ramp) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<RampComponent>>())
                    {
                        float distSq = math.distancesq(pos, rampTransform.ValueRO.Position);
                        float radSq = ramp.ValueRO.CollisionRadius * ramp.ValueRO.CollisionRadius;

                        if (distSq < radSq)
                        {
                            airborne.ValueRW.IsAirborne = true;
                            airborne.ValueRW.VerticalVelocity = ramp.ValueRO.JumpForce;
                            airborne.ValueRW.Gravity = -25f;

                            // Sound effect
                            var audioEntity = ecb.CreateEntity();
                            ecb.AddComponent(audioEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                            break;
                        }
                    }
                }

                // 2. Process Airborne Flight Motion
                if (airborne.ValueRO.IsAirborne)
                {
                    pos.y += airborne.ValueRO.VerticalVelocity * dt;
                    airborne.ValueRW.VerticalVelocity += airborne.ValueRO.Gravity * dt;
                    transform.ValueRW.Position = pos;

                    // Landing Check
                    if (pos.y <= 1.0f)
                    {
                        pos.y = 1.0f;
                        transform.ValueRW.Position = pos;
                        airborne.ValueRW.IsAirborne = false;
                        airborne.ValueRW.VerticalVelocity = 0f;

                        // Check if landed in Water Zone
                        foreach (var water in SystemAPI.Query<RefRO<WaterZoneComponent>>())
                        {
                            if (pos.x >= water.ValueRO.MinBounds.x && pos.x <= water.ValueRO.MaxBounds.x &&
                                pos.z >= water.ValueRO.MinBounds.z && pos.z <= water.ValueRO.MaxBounds.z)
                            {
                                runStats.ValueRW.CurrentGold *= water.ValueRO.BonusMultiplier;

                                var audioEntity = ecb.CreateEntity();
                                ecb.AddComponent(audioEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Victory });
                                break;
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
