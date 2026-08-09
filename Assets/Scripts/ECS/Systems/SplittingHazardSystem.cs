using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct SplittingHazardSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
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
                return;
            }

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var (transform, hazard, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<SplittingHazardComponent>>().WithEntityAccess())
            {
                if (hazard.ValueRO.HasSplit)
                {
                    continue;
                }

                float distSq = math.distancesq(playerPos, transform.ValueRO.Position);
                float trigDistSq = hazard.ValueRO.TriggerDistance * hazard.ValueRO.TriggerDistance;

                if (distSq <= trigDistSq)
                {
                    hazard.ValueRW.HasSplit = true;

                    if (hazard.ValueRO.ChildPrefab != Entity.Null && hazard.ValueRO.SplitCount > 0)
                    {
                        float3 parentPos = transform.ValueRO.Position;
                        int count = hazard.ValueRO.SplitCount;
                        float impulse = hazard.ValueRO.ImpulseForce;

                        for (int i = 0; i < count; i++)
                        {
                            Entity childEntity = ecb.Instantiate(hazard.ValueRO.ChildPrefab);

                            // Evenly space child hazards along lateral X axis
                            float offsetFactor = count > 1 ? ((float)i / (count - 1)) - 0.5f : 0f;
                            float3 childPos = parentPos + new float3(offsetFactor * impulse * 2.0f, 0f, 0f);

                            ecb.SetComponent(childEntity, LocalTransform.FromPosition(childPos));
                        }
                    }

                    // Create audio event entity
                    Entity soundEntity = ecb.CreateEntity();
                    ecb.AddComponent(soundEntity, new PlaySoundEventComponent
                    {
                        SoundToPlay = SoundType.Explosion
                    });

                    // Add VFX tag and destroy parent entity
                    ecb.AddComponent(entity, new DestroyEventComponent());
                    ecb.DestroyEntity(entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
