# Technical Specification: Milestone 3 — Coin Multiplier & Splitting Physics (`14_MoneyRush_Coins`)

## 1. Overview & Architecture
Milestone 3 implements pure DOTS Entities 1.0+ coin multiplier gates (+N, xN) and 3D coin splitting physics for slice 14 (`14_MoneyRush_Coins_Slice.prefab`). The design follows the project's hybrid ECS pattern and memory layout constraints.

## 2. Component Data Models (`Assets/Scripts/ECS/Components/CoinMultiplierComponents.cs`)

```csharp
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
        public Unity.Mathematics.float3 CurrentVelocity; // Offset 0 (12 bytes)
        public float SpreadAngle;                       // Offset 12 (4 bytes)
        public float ImpulseSpeed;                      // Offset 16 (4 bytes)
        public float StackHeightOffset;                 // Offset 20 (4 bytes)
        public float Lifetime;                          // Offset 24 (4 bytes)
        public float MaxLifetime;                       // Offset 28 (4 bytes)
        public float GravityMultiplier;                 // Offset 32 (4 bytes)
        public int CoinCount;                           // Offset 36 (4 bytes)
        public bool IsGrounded;                         // Offset 40 (1 byte)
        public bool IsCollectible;                      // Offset 41 (1 byte)
        public ushort ReservedPadding;                  // Offset 42 (2 bytes)
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
        public Entity GateEntity;
        public Entity InstigatorEntity;
        public MultiplierType GateType;
        public byte Padding0;
        public ushort Padding1;
        public float MultiplierValue;
        public int CoinsBefore;
        public int CoinsAfter;
    }

    public struct CoinSplitEventComponent : IComponentData, IEnableableComponent
    {
        public Unity.Mathematics.float3 SpawnPosition;
        public float SpreadAngle;
        public float ImpulseSpeed;
        public int QuantityToSpawn;
        public Entity CoinPrefab;
    }
}
```

## 3. Authoring Components (`Assets/Scripts/ECS/Authoring/`)
- `CoinGateAuthoring.cs` (Bakes `CoinMultiplierGateComponent` & `CoinMultiplierGateTag`)
- `CoinPhysicsAuthoring.cs` (Bakes `CoinSplitPhysicsComponent` & `CoinTag`)
- `CoinSpawnerAuthoring.cs` (Bakes `PlayerCoinRunnerComponent`)

## 4. Pure DOTS Systems (`Assets/Scripts/ECS/Systems/`)
1. `CoinMultiplierSystem`:
   - `[UpdateInGroup(typeof(SimulationSystemGroup))]`
   - `[UpdateBefore(typeof(CoinPhysicsSystem))]`
   - Detects player-gate bounding overlap ($|P_z - G_z| \le \frac{Depth}{2}$ and $|P_x - G_x| \le \frac{Width}{2}$).
   - Updates `CurrentCoinCount`, marks gate triggered, emits `CoinMultiplierEventComponent`, `PlaySoundEventComponent`, and `CoinSplitEventComponent` for bursts.
2. `CoinPhysicsSystem`:
   - Executes 3-phase trajectory (Airborne parabolic -> Ground bounce -> Magnetic collection towards player).
   - Collects split coins when within collection radius ($d < R_{collect}$).

## 5. Generator Integration (`Assets/Scripts/Editor/ToolkitExampleGenerator.cs`)
- Programmatically populates `14_MoneyRush_Coins_Slice.prefab` with `CoinSpawnerAuthoring`, `CoinGateAuthoring` (+2, x3, +10, x2, x4), and split coin templates.
- Updates `RunVerificationSuite()` to assert `14_MoneyRush_Coins_Slice.prefab` validity.

## 6. Hybrid Presentation Layer
- Emits `PlaySoundEventComponent` for audio rendering via `AudioManagerSystem`.
- Emits `DestroyEventComponent` for visual effect instantiation via `VFXManagerSystem`.
