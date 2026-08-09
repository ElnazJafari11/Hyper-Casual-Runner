using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public struct ShopPurchaseEventComponent : IComponentData, IEnableableComponent
    {
        public int TargetProducerId;
        public double Cost;
    }
}
