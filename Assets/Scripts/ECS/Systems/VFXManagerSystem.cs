using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using HyperCasualRunner.ECS.Authoring;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class VFXManagerSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            
            // Only run if we have a VFX prefab assigned in the scene
            if (!SystemAPI.ManagedAPI.HasSingleton<VFXManagerComponent>()) return;
            
            var vfxPrefab = SystemAPI.ManagedAPI.GetSingleton<VFXManagerComponent>().HitVFXPrefab;
            if (vfxPrefab == null) return;

            foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<DestroyEventComponent>().WithEntityAccess())
            {
                // 1. Spawn the Unity Particle System at the ECS position
                GameObject vfx = Object.Instantiate(vfxPrefab, transform.ValueRO.Position, Quaternion.identity);
                
                // Ensure it cleans itself up after 1 second
                Object.Destroy(vfx, 1f);

                // 2. Actually destroy the ECS entity
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
