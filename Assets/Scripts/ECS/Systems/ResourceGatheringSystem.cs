using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct ResourceGatheringSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // Note: In Arcade Idle, player stops moving to harvest
            foreach (var (playerTransform, playerComp, runStats) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<PlayerComponent>, RefRW<CurrentRunStats>>())
            {
                bool isHarvesting = false;

                foreach (var (nodeTransform, node, nodeEntity) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<ResourceNodeComponent>>().WithEntityAccess())
                {
                    if (node.ValueRO.IsDepleted) continue;

                    float distSq = math.distancesq(playerTransform.ValueRO.Position, nodeTransform.ValueRO.Position);
                    
                    if (distSq < node.ValueRO.CollisionRadius * node.ValueRO.CollisionRadius)
                    {
                        isHarvesting = true;
                        
                        // Increment progress
                        node.ValueRW.CurrentProgress += SystemAPI.Time.DeltaTime;
                        
                        if (node.ValueRO.CurrentProgress >= node.ValueRO.HarvestingTimeRequired)
                        {
                            // Yield resources
                            runStats.ValueRW.CurrentGold += node.ValueRO.YieldAmount; // Using Gold as generic generic resource
                            node.ValueRW.IsDepleted = true;

                            // Play Pickup Sound
                            Entity audioEvent = ecb.CreateEntity();
                            ecb.AddComponent(audioEvent, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                        }
                    }
                }

                // If player is harvesting a node, halt forward speed
                if (isHarvesting)
                {
                    playerComp.ValueRW.ForwardSpeed = 0f;
                }
                else
                {
                    // MVP Hack: Restore base speed when not harvesting. 
                    // Realistically, base speed would be stored separately.
                    playerComp.ValueRW.ForwardSpeed = math.max(playerComp.ValueRO.ForwardSpeed, 5f);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
