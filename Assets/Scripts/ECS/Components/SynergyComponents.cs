using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public struct SynergyTagComponent : IComponentData
    {
        public int SynergyId;
    }

    public struct SynergyBuffComponent : IComponentData
    {
        public int RequiredSynergyId;
        public double MultiplierBonus;
    }
}
