using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public struct InputComponent : IComponentData
    {
        public float SwerveDelta;
        public float SwipeDeltaX;
        public bool IsInteracting;
    }
}
