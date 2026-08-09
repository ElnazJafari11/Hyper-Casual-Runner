using Unity.Entities;
using Unity.Mathematics;

namespace HyperCasualRunner.ECS.Components
{
    public struct PendulumSwingComponent : IComponentData
    {
        public float MaxAngleDegrees;
        public float Speed;
        public float PhaseOffset;
        public float3 SwingAxis;
        public quaternion InitialRotation;
        public float CollisionRadius;
        public float DamageAmount;
    }
}
