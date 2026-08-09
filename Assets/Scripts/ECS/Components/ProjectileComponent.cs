using Unity.Entities;
using Unity.Mathematics;

namespace HyperCasualRunner.ECS.Components
{
    public struct ProjectileComponent : IComponentData
    {
        public float Speed;
        public float Damage;
        public float3 Direction;
        public float Lifetime;
        public float CollisionRadius;
        public int PierceCount;
    }
}
