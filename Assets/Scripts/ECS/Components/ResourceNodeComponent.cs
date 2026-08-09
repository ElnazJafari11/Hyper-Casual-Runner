using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public struct ResourceNodeComponent : IComponentData
    {
        public float HarvestingTimeRequired;
        public float CurrentProgress;
        public int YieldAmount;
        public float CollisionRadius;
        public bool IsDepleted;
    }
}
