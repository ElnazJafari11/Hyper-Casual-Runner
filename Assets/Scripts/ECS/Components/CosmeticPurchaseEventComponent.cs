using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public struct CosmeticPurchaseEventComponent : IComponentData, IEnableableComponent
    {
        public Entity TargetSlice;
        public int TargetSkinIndex;
        public double PrestigeCost;
    }
}
