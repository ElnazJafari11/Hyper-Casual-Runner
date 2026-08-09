using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class PlayerAuthoring : MonoBehaviour
    {
        public float ForwardSpeed = 10f;
        public float SwerveSpeed = 5f;
        public float MaxSwerveDistance = 4f;

        class Baker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new PlayerComponent
                {
                    ForwardSpeed = authoring.ForwardSpeed,
                    SwerveSpeed = authoring.SwerveSpeed,
                    MaxSwerveDistance = authoring.MaxSwerveDistance
                });

                AddComponent(entity, new InputComponent
                {
                    SwipeDeltaX = 0f
                });

                AddComponent(entity, new AirborneComponent
                {
                    IsAirborne = false,
                    VerticalVelocity = 0f,
                    Gravity = -25f
                });

                AddComponent(entity, new CurrentRunStats
                {
                    CurrentGold = 0.0
                });
            }
        }
    }
}
