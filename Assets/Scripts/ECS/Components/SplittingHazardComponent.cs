using Unity.Entities;
using Unity.Mathematics;

namespace HyperCasualRunner.ECS.Components
{
    public struct SplittingHazardComponent : IComponentData
    {
        public int SplitCount;
        public Entity ChildPrefab;
        public float ImpulseForce;
        public float TriggerDistance;
        public bool HasSplit;
        public float CollisionRadius;
        public float DamageAmount;
    }
}
