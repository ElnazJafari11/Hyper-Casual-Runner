using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class CameraFollowSystem : SystemBase
    {
        private Camera _mainCamera;
        private float3 _offset = new float3(0, 5, -8); // Standard isometric-ish runner view

        protected override void OnStartRunning()
        {
            _mainCamera = Camera.main;
        }

        protected override void OnUpdate()
        {
            if (_mainCamera == null) return;

            // Find the player entity to follow
            foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerComponent>())
            {
                float3 targetPos = transform.ValueRO.Position + _offset;
                
                // Smooth follow (Lerp)
                Vector3 currentPos = _mainCamera.transform.position;
                _mainCamera.transform.position = Vector3.Lerp(currentPos, targetPos, SystemAPI.Time.DeltaTime * 10f);
                
                // Always look at the player
                _mainCamera.transform.LookAt(transform.ValueRO.Position);
                
                // Only follow the first player found (relevant for Swarm games where we might have many units)
                break;
            }
        }
    }
}
