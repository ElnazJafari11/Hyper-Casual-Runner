using Unity.Entities;
using Unity.Mathematics;

namespace HyperCasualRunner.ECS.Components
{
    public struct MovingWallComponent : IComponentData
    {
        public float3 MovementAxis;
        public float MoveDistance;
        public float Speed;
        public float3 InitialPosition;
        public float CollisionRadius;
        public float DamageAmount;
    }
}
