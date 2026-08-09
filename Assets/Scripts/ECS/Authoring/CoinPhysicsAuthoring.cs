using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class CoinPhysicsAuthoring : MonoBehaviour
    {
        public float SpreadAngle = 60.0f;
        public float ImpulseSpeed = 8.0f;
        public float StackHeightOffset = 0.5f;
        public float MaxLifetime = 3.0f;
        public float GravityMultiplier = 2.5f;
        public int CoinCount = 1;

        class Baker : Baker<CoinPhysicsAuthoring>
        {
            public override void Bake(CoinPhysicsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CoinSplitPhysicsComponent
                {
                    CurrentVelocity = float3.zero,
                    SpreadAngle = authoring.SpreadAngle,
                    ImpulseSpeed = authoring.ImpulseSpeed,
                    StackHeightOffset = authoring.StackHeightOffset,
                    Lifetime = authoring.MaxLifetime,
                    MaxLifetime = authoring.MaxLifetime,
                    GravityMultiplier = authoring.GravityMultiplier,
                    CoinCount = authoring.CoinCount,
                    IsGrounded = false,
                    IsCollectible = false,
                    ReservedPadding = 0
                });
                AddComponent<CoinTag>(entity);
            }
        }
    }
}
