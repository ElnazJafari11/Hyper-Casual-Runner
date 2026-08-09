using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct StackVisualSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // Collect logic (for MVP, we just use the global Gold as the proxy for the stack count)
            // Realistically, you would collide with individual stackable resources on the ground.
            foreach (var (transform, stack, runStats) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<StackComponent>, RefRO<CurrentRunStats>>())
            {
                int desiredCount = (int)runStats.ValueRO.CurrentGold;

                // If collected a new item, spawn visual
                if (stack.ValueRO.CurrentCount < desiredCount)
                {
                    int toSpawn = desiredCount - stack.ValueRO.CurrentCount;
                    for (int i = 0; i < toSpawn; i++)
                    {
                        Entity e = ecb.Instantiate(stack.ValueRO.StackItemPrefab);
                        
                        // Calculate Y position based on how many items we already have in stack
                        int index = stack.ValueRO.CurrentCount + i;
                        float yOffset = 1.0f + (index * stack.ValueRO.ItemHeightOffset); 
                        
                        // Parent to the player (Requires Parent component, using MVP float3 relative position here)
                        // In DOTS, you usually add Parent/LocalToParent. We will just teleport it above player in late update for MVP
                        ecb.SetComponent(e, LocalTransform.FromPosition(transform.ValueRO.Position + new float3(0, yOffset, 0)));
                    }
                    stack.ValueRW.CurrentCount = desiredCount;
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
