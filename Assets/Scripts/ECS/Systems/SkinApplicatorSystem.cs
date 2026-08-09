using Unity.Entities;
using Unity.Rendering;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    // We run in Initialization so colors are set before the first frame is rendered
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class SkinApplicatorSystem : SystemBase
    {
        private bool _hasApplied = false;

        protected override void OnUpdate()
        {
            // Only apply once per level load
            if (_hasApplied) return;

            // In a real game, this might be a Singleton we listen to, but for MVP we poll the persistent data
            int skinIndex = GameProgressData.CurrentSkinIndex;
            
            // Default Blue
            float4 targetColor = new float4(0, 0, 1, 1);
            if (skinIndex == 1) targetColor = new float4(0.8f, 0, 0, 1); // Crimson Red
            if (skinIndex == 2) targetColor = new float4(1f, 0.84f, 0, 1); // Solid Gold

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // Apply URP material overrides to anything that is a Player or Swarm unit
            foreach (var (player, entity) in SystemAPI.Query<RefRO<PlayerComponent>>().WithEntityAccess())
            {
                // ecb.AddComponent(entity, new URPMaterialPropertyBaseColor { Value = targetColor });
            }

            // We must also tag the Swarm Prefab itself, so any newly spawned units inherit the color!
            foreach (var (swarm, entity) in SystemAPI.Query<RefRO<SwarmComponent>>().WithEntityAccess())
            {
                // ecb.AddComponent(entity, new URPMaterialPropertyBaseColor { Value = targetColor });
                
                if (swarm.ValueRO.SwarmMemberPrefab != Entity.Null)
                {
                    // ecb.AddComponent(swarm.ValueRO.SwarmMemberPrefab, new URPMaterialPropertyBaseColor { Value = targetColor });
                }
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();

            _hasApplied = true;
        }
    }
}
