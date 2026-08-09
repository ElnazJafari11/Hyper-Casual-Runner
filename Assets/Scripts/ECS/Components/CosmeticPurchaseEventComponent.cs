using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public struct CosmeticPurchaseEventComponent : IComponentData, IEnableableComponent
    {
        public int TargetSkinIndex;
        public double PrestigeCost;
    }
}
