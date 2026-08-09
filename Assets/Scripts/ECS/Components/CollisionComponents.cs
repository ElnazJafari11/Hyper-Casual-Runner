using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public struct ObstacleComponent : IComponentData
    {
        public float CollisionRadius;
        public float DamageAmount;
    }

    public struct CollectibleComponent : IComponentData
    {
        public float CollisionRadius;
        public double GoldValue;
    }
}
