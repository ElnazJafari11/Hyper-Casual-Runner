using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct CombatSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // Simple N^2 collision check for MVP combat
            // Check all Player/Swarm entities against all Enemy entities
            foreach (var (playerTransform, playerEntity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerComponent>().WithEntityAccess())
            {
                foreach (var (enemyTransform, enemy, enemyEntity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<EnemyComponent>>().WithEntityAccess())
                {
                    float distSq = math.distancesq(playerTransform.ValueRO.Position, enemyTransform.ValueRO.Position);
                    float combinedRadius = 0.5f + enemy.ValueRO.CollisionRadius; // hardcoded player radius for MVP
                    
                    if (distSq < combinedRadius * combinedRadius)
                    {
                        // Mutual destruction (1-to-1 swarm combat)
                        // Instead of destroying immediately, we tag them so the VFXSystem can play particles
                        ecb.AddComponent<DestroyEventComponent>(playerEntity);
                        ecb.AddComponent<DestroyEventComponent>(enemyEntity);
                        
                        // Trigger Explosion Audio Event
                        Entity audioEvent = ecb.CreateEntity();
                        ecb.AddComponent(audioEvent, new PlaySoundEventComponent { SoundToPlay = SoundType.Explosion });
                        
                        // We break so this player entity doesn't try to tag multiple enemies in one frame
                        break;
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
