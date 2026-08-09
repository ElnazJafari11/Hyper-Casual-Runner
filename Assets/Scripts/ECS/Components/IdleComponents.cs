using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    // The persistent stats that survive a Prestige
    public struct PersistentPlayerStats : IComponentData
    {
        public double PrestigeCurrency;
        public float PermanentDamageMultiplier;
        public float PermanentGoldMultiplier;
    }

    // The transient stats that reset on Prestige
    public struct CurrentRunStats : IComponentData
    {
        public int CurrentDistance;
        public double CurrentGold;
        public float BaseDamage;
    }

    public enum UpgradeType
    {
        Damage,
        GoldRate,
        OfflineRate
    }

    // Component attached to upgrade buttons/nodes
    public struct UpgradeNode : IComponentData
    {
        public UpgradeType Type;
        public double BaseCost;
        public float CostGrowthFactor;
        public int CurrentLevel;
    }

    // Buffer element representing resources held in a wallet
    public struct ResourceWallet : IBufferElementData
    {
        public int ResourceId;
        public double Amount;
    }

    // Component for passive generator/producer entities
    public struct ProducerComponent : IComponentData
    {
        public int ResourceId;
        public double BaseProductionRate;
        public float ProductionInterval;
        public float Timer;
        public double Multiplier;
        public bool IsAutomated;
        public Entity TargetWalletEntity;
    }

    // Component event to trigger a Prestige reset (TargetSlice scopes idle mutations)
    public struct PrestigeEventComponent : IComponentData, IEnableableComponent
    {
        public Entity TargetSlice;
    }
}
