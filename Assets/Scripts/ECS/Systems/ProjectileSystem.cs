using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ShooterSystem))]
    public partial struct ProjectileSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<LevelStateComponent>(out var levelState) || levelState.CurrentState != GameState.Playing)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);
            float dt = SystemAPI.Time.DeltaTime;

            foreach (var (projectile, transform, entity) in SystemAPI.Query<RefRW<ProjectileComponent>, RefRW<LocalTransform>>().WithEntityAccess())
            {
                // Move forward
                transform.ValueRW.Position += projectile.ValueRO.Direction * projectile.ValueRO.Speed * dt;

                projectile.ValueRW.Lifetime -= dt;
                if (transform.ValueRO.Position.z > 500f || projectile.ValueRO.Lifetime <= 0f) // Simple cleanup
                {
                    ecb.DestroyEntity(entity);
                    continue;
                }

                // Check collision against enemies
                bool hit = false;
                foreach (var (enemyTransform, enemyComp, enemyEntity) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<EnemyComponent>>().WithEntityAccess())
                {
                    float distSq = math.distancesq(transform.ValueRO.Position, enemyTransform.ValueRO.Position);
                    float radSq = (projectile.ValueRO.CollisionRadius + enemyComp.ValueRO.CollisionRadius) * (projectile.ValueRO.CollisionRadius + enemyComp.ValueRO.CollisionRadius);
                    
                    if (distSq < radSq)
                    {
                        hit = true;
                        enemyComp.ValueRW.Health -= projectile.ValueRO.Damage;
                        
                        if (enemyComp.ValueRO.Health <= 0)
                        {
                            ecb.AddComponent<DestroyEventComponent>(enemyEntity);
                            
                            var audioEntity = ecb.CreateEntity();
                            ecb.AddComponent(audioEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Explosion });
                        }
                        
                        break; // Process one enemy at a time per frame per projectile
                    }
                }

                if (hit)
                {
                    projectile.ValueRW.PierceCount--;
                    if (projectile.ValueRO.PierceCount < 0)
                    {
                        ecb.DestroyEntity(entity);
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
