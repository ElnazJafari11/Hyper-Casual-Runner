using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct ShooterSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<LevelStateComponent>(out var levelState) || levelState.CurrentState != GameState.Playing)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);
            float dt = SystemAPI.Time.DeltaTime;

            foreach (var (shooter, transform) in SystemAPI.Query<RefRW<ShooterComponent>, RefRO<LocalTransform>>())
            {
                if (shooter.ValueRO.ProjectilePrefab == Entity.Null) continue;

                shooter.ValueRW.Timer += dt;
                if (shooter.ValueRO.Timer >= shooter.ValueRO.FireRate)
                {
                    shooter.ValueRW.Timer = 0f;
                    
                    int count = math.max(1, shooter.ValueRO.SpreadCount);
                    float angleStep = shooter.ValueRO.SpreadAngle;
                    float startAngle = -((count - 1) * angleStep) / 2f;
                    
                    bool hasProjData = SystemAPI.HasComponent<ProjectileComponent>(shooter.ValueRO.ProjectilePrefab);
                    ProjectileComponent baseProj = default;
                    if (hasProjData) baseProj = SystemAPI.GetComponent<ProjectileComponent>(shooter.ValueRO.ProjectilePrefab);
                    
                    for (int i = 0; i < count; i++)
                    {
                        var projectileEntity = ecb.Instantiate(shooter.ValueRO.ProjectilePrefab);
                        
                        // Calculate direction with spread
                        float currentAngle = startAngle + (i * angleStep);
                        quaternion rotation = quaternion.Euler(0, math.radians(currentAngle), 0);
                        float3 forward = math.mul(rotation, transform.ValueRO.Forward());
                        
                        // Spawn the projectile slightly in front of the shooter
                        var projTransform = LocalTransform.FromPositionRotation(transform.ValueRO.Position + (forward * 1.5f), rotation);
                        ecb.SetComponent(projectileEntity, projTransform);
                        
                        // Set the Projectile properties
                        if (hasProjData)
                        {
                            var projComp = baseProj;
                            projComp.Direction = forward;
                            projComp.PierceCount = shooter.ValueRO.Pierce;
                            ecb.SetComponent(projectileEntity, projComp);
                        }
                    }
                    
                    // Trigger sound (using Pickup as a "pew" sound for MVP)
                    var audioEntity = ecb.CreateEntity();
                    ecb.AddComponent(audioEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
