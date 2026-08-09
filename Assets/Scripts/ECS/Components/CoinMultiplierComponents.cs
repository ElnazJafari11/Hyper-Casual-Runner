using Unity.Entities;
using Unity.Mathematics;

namespace HyperCasualRunner.ECS.Components
{
    public enum MultiplierType : byte
    {
        Additive = 0,
        Multiplicative = 1
    }

    public struct CoinMultiplierGateComponent : IComponentData
    {
        public MultiplierType GateType;   // Offset 0 (1 byte)
        public bool IsTriggered;          // Offset 1 (1 byte)
        public ushort ReservedPadding;    // Offset 2 (2 bytes)
        public float Value;               // Offset 4 (4 bytes)
        public float GateWidth;           // Offset 8 (4 bytes)
        public float TriggerDepth;        // Offset 12 (4 bytes)
        public float MinimumOutput;       // Offset 16 (4 bytes)
    }

    public struct CoinSplitPhysicsComponent : IComponentData
    {
        public float3 CurrentVelocity;    // Offset 0 (12 bytes)
        public float SpreadAngle;         // Offset 12 (4 bytes)
        public float ImpulseSpeed;        // Offset 16 (4 bytes)
        public float StackHeightOffset;   // Offset 20 (4 bytes)
        public float Lifetime;            // Offset 24 (4 bytes)
        public float MaxLifetime;         // Offset 28 (4 bytes)
        public float GravityMultiplier;   // Offset 32 (4 bytes)
        public int CoinCount;             // Offset 36 (4 bytes)
        public bool IsGrounded;           // Offset 40 (1 byte)
        public bool IsCollectible;        // Offset 41 (1 byte)
        public ushort ReservedPadding;    // Offset 42 (2 bytes)
    }

    public struct PlayerCoinRunnerComponent : IComponentData
    {
        public Entity CoinVisualPrefab;   // Offset 0 (8 bytes)
        public int CurrentCoinCount;      // Offset 8 (4 bytes)
        public float StackSpacing;       // Offset 12 (4 bytes)
        public float MaxStackHeight;     // Offset 16 (4 bytes)
        public float SwerveSensitivity;  // Offset 20 (4 bytes)
    }

    public struct CoinTag : IComponentData {}
    public struct CoinMultiplierGateTag : IComponentData {}

    public struct CoinMultiplierEventComponent : IComponentData, IEnableableComponent
    {
        public Entity GateEntity;         // Offset 0 (8 bytes)
        public Entity InstigatorEntity;   // Offset 8 (8 bytes)
        public MultiplierType GateType;   // Offset 16 (1 byte)
        public byte Padding0;             // Offset 17 (1 byte)
        public ushort Padding1;           // Offset 18 (2 bytes)
        public float MultiplierValue;     // Offset 20 (4 bytes)
        public int CoinsBefore;           // Offset 24 (4 bytes)
        public int CoinsAfter;            // Offset 28 (4 bytes)
    }

    public struct CoinSplitEventComponent : IComponentData, IEnableableComponent
    {
        public float3 SpawnPosition;      // Offset 0 (12 bytes)
        public float SpreadAngle;         // Offset 12 (4 bytes)
        public float ImpulseSpeed;        // Offset 16 (4 bytes)
        public int QuantityToSpawn;       // Offset 20 (4 bytes)
        public Entity CoinPrefab;         // Offset 24 (8 bytes)
    }

    public struct CoinStackElement : IBufferElementData
    {
        public Entity CoinVisualEntity;  // Offset 0 (8 bytes)
        public float LocalYOffset;       // Offset 8 (4 bytes)
        public int ValueMultiplier;      // Offset 12 (4 bytes)
    }
}
