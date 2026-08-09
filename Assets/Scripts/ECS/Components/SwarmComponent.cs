using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public enum GateOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }

    public struct MathGateComponent : IComponentData
    {
        public GateOperation Operation;
        public int Value;
        public float Radius;
    }

    public struct SwarmComponent : IComponentData
    {
        public int TargetCount;
        public int CurrentCount;
        public Entity SwarmMemberPrefab;
    }
}
