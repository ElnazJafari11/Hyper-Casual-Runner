using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct CollisionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // Get the player position (assuming single player entity for simplicity)
            float3 playerPos = float3.zero;
            bool playerFound = false;

            foreach (var (transform, player) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<PlayerComponent>>())
            {
                playerPos = transform.ValueRO.Position;
                playerFound = true;
                break;
            }

            if (!playerFound)
            {
                ecb.Dispose();
                return;
            }

            float playerRadius = 1.0f; // Simplified player radius

            // Check Collectibles
            foreach (var (transform, collectible, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<CollectibleComponent>>().WithEntityAccess())
            {
                float distSq = math.distancesq(playerPos, transform.ValueRO.Position);
                float radSum = playerRadius + collectible.ValueRO.CollisionRadius;
                
                if (distSq < radSum * radSum)
                {
                    // Collision occurred! Add gold to run stats
                    foreach (var runStats in SystemAPI.Query<RefRW<CurrentRunStats>>())
                    {
                        runStats.ValueRW.CurrentGold += collectible.ValueRO.GoldValue;
                    }
                    
                    // Destroy collectible
                    ecb.DestroyEntity(entity);
                }
            }

            // Check Obstacles
            foreach (var (transform, obstacle, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<ObstacleComponent>>().WithEntityAccess())
            {
                float distSq = math.distancesq(playerPos, transform.ValueRO.Position);
                float radSum = playerRadius + obstacle.ValueRO.CollisionRadius;
                
                if (distSq < radSum * radSum)
                {
                    // Handle damage/death logic here
                    ecb.DestroyEntity(entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
