using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public struct TransformationComponent : IComponentData
    {
        public float TargetHeight;
        public float TargetWidth;
    }
}
