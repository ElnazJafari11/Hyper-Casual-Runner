using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public struct StackComponent : IComponentData
    {
        public int CurrentCount;
        public float ItemHeightOffset;
        public Entity StackItemPrefab;
    }
}
