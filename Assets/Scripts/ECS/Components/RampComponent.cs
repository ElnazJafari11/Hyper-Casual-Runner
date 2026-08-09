using Unity.Entities;
using Unity.Mathematics;

namespace HyperCasualRunner.ECS.Components
{
    public struct RampComponent : IComponentData
    {
        public float JumpForce;
        public float SpeedBoost;
        public float CollisionRadius;
    }

    public struct AirborneComponent : IComponentData
    {
        public bool IsAirborne;
        public float VerticalVelocity;
        public float Gravity;
    }

    public struct WaterZoneComponent : IComponentData
    {
        public float BonusMultiplier;
        public float3 MinBounds;
        public float3 MaxBounds;
    }
}
