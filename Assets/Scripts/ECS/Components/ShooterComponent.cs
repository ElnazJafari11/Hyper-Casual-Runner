using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public struct ShooterComponent : IComponentData
    {
        public Entity ProjectilePrefab;
        public float FireRate; // Time between shots in seconds
        public float Timer;
        public int SpreadCount; // Number of projectiles fired at once
        public float SpreadAngle; // Angle in degrees between projectiles
        public int Pierce; // Number of enemies the projectile can pierce
    }
}
