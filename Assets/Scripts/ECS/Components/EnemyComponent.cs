using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public struct EnemyComponent : IComponentData
    {
        public float CollisionRadius;
        public float Health;
    }
}
