# Analysis & Architecture Report: Milestone 3 - Coin Multiplier & Splitting Physics (Slice 14)

**Target Slice**: `14_MoneyRush_Coins_Slice.prefab`  
**Scope**: Pure DOTS Entities 1.0+ Component Data Models, Gate Multiplier Architecture, Coin Splitting Physics, Memory Layout Alignment, and Collision/Trigger Detection Systems.

---

## 1. Existing Architecture & Pattern Inspection

An inspection of `Assets/Scripts/ECS/Components/` and `Assets/Scripts/ECS/Systems/` revealed the following core architectural patterns in the codebase:

1. **Namespace & Structural Conventions**:
   - All ECS components reside in `HyperCasualRunner.ECS.Components`.
   - Core state uses unmanaged C# structs implementing `IComponentData`.
   - Visual references and prefab references use `Entity` handles (baked by MonoBehaviours using `Baker<T>`).
   - Frame-specific tags/events implement `IEnableableComponent` or empty structs (`IComponentData`).
   - Dynamic length collections use `IBufferElementData` (e.g. `StackedTileElement` in `GridPathfinderComponents.cs`).

2. **Gate & Collision Handling Patterns**:
   - Existing gates (e.g. `MathGateComponent` in `SwarmComponent.cs` & `MathGateAuthoring.cs`) use explicit operation enums (`GateOperation.Add`, `GateOperation.Multiply`, etc.) and float parameters (`Radius`, `Value`).
   - Gate processing systems (such as `ShooterGateSystem.cs` and `SnakeCollisionSystem.cs`) check player/gate distance, apply modifications to runner stats, and issue structural changes (e.g. attaching `DestroyEventComponent` and creating a `PlaySoundEventComponent` entity).

3. **Slice 14 Inspection**:
   - `14_MoneyRush_Coins_Slice.prefab` contains `LevelManager`, `PlayerEntity`, `MathGate`, `EnemyEntity`, `EndZone`, and visual VFX templates.
   - Milestone 3 requires extending these base patterns into dedicated, highly optimized Money Rush components: `CoinMultiplierGateComponent` and `CoinSplitPhysicsComponent`.

---

## 2. Component Data Models (`IComponentData`, `IBufferElementData`, Tags)

### 2.1 Enums

```csharp
namespace HyperCasualRunner.ECS.Components
{
    /// <summary>
    /// Type of arithmetic operation performed by a Money Rush multiplier gate.
    /// </summary>
    public enum MultiplierType : byte
    {
        Additive = 0,       // Adds fixed amount to current count/value (+N or -N)
        Multiplicative = 1  // Multiplies current count/value (xN or ÷N)
    }
}
```

### 2.2 Gate Component: `CoinMultiplierGateComponent`

```csharp
namespace HyperCasualRunner.ECS.Components
{
    /// <summary>
    /// Component attached to Money Rush gate entities defining multiplier math, width, and trigger state.
    /// </summary>
    public struct CoinMultiplierGateComponent : IComponentData
    {
        public MultiplierType GateType;   // Offset 0 (1 byte): Additive (+X) or Multiplicative (xX)
        public bool IsTriggered;          // Offset 1 (1 byte): Prevents re-triggering across frames
        public ushort ReservedPadding;    // Offset 2 (2 bytes): Alignment padding
        public float Value;               // Offset 4 (4 bytes): Arithmetic operand (e.g., +10.0f, 2.0f, -5.0f)
        public float GateWidth;           // Offset 8 (4 bytes): Transverse X-axis trigger width
        public float TriggerDepth;        // Offset 12 (4 bytes): Longitudinal Z-axis trigger thickness
        public float MinimumOutput;       // Offset 16 (4 bytes): Result floor (prevents negative counts)
    }
}
```

### 2.3 Coin Splitting Physics: `CoinSplitPhysicsComponent`

```csharp
namespace HyperCasualRunner.ECS.Components
{
    /// <summary>
    /// Component driving 3D parabolic dispersal physics for split coins ejected from gates or obstacles.
    /// </summary>
    public struct CoinSplitPhysicsComponent : IComponentData
    {
        public Unity.Mathematics.float3 CurrentVelocity; // Offset 0 (12 bytes): 3D velocity vector (m/s)
        public float SpreadAngle;                       // Offset 12 (4 bytes): Arc spread in degrees (e.g. 60 deg)
        public float ImpulseSpeed;                      // Offset 16 (4 bytes): Initial ejection speed magnitude (m/s)
        public float StackHeightOffset;                 // Offset 20 (4 bytes): Vertical ejection origin offset
        public float Lifetime;                          // Offset 24 (4 bytes): Remaining simulation lifetime (seconds)
        public float MaxLifetime;                       // Offset 28 (4 bytes): Total initial lifetime for interpolation
        public float GravityMultiplier;                 // Offset 32 (4 bytes): Downward acceleration factor (e.g., 2.5g)
        public int CoinCount;                           // Offset 36 (4 bytes): Coin batch count represented by this entity
        public bool IsGrounded;                         // Offset 40 (1 byte): Ground hit state flag
        public bool IsCollectible;                      // Offset 41 (1 byte): Magnetism / pickup eligibility flag
        public ushort ReservedPadding;                  // Offset 42 (2 bytes): Alignment padding
    }
}
```

### 2.4 Runner Entity State: `PlayerCoinRunnerComponent`

```csharp
namespace HyperCasualRunner.ECS.Components
{
    /// <summary>
    /// Attached to the main runner entity tracking current coin count and visual stack parameters.
    /// </summary>
    public struct PlayerCoinRunnerComponent : IComponentData
    {
        public Entity CoinVisualPrefab;   // Offset 0 (8 bytes): Prefab entity for stacked visual coins
        public int CurrentCoinCount;      // Offset 8 (4 bytes): Total active coin score/count
        public float StackSpacing;       // Offset 12 (4 bytes): Vertical spacing per coin tier
        public float MaxStackHeight;     // Offset 16 (4 bytes): Visual stack height cap before column splitting
        public float SwerveSensitivity;  // Offset 20 (4 bytes): Lateral movement multiplier
    }
}
```

### 2.5 Tags & Event Components

```csharp
namespace HyperCasualRunner.ECS.Components
{
    /// <summary>
    /// Empty tag component identifying coin entities.
    /// </summary>
    public struct CoinTag : IComponentData {}

    /// <summary>
    /// Empty tag component identifying multiplier gate entities.
    /// </summary>
    public struct CoinMultiplierGateTag : IComponentData {}

    /// <summary>
    /// Frame-local event component fired when player triggers a multiplier gate.
    /// </summary>
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

    /// <summary>
    /// Event component requesting coin split physics spawning.
    /// </summary>
    public struct CoinSplitEventComponent : IComponentData, IEnableableComponent
    {
        public Unity.Mathematics.float3 SpawnPosition; // Offset 0 (12 bytes)
        public float SpreadAngle;                       // Offset 12 (4 bytes)
        public float ImpulseSpeed;                      // Offset 16 (4 bytes)
        public int QuantityToSpawn;                      // Offset 20 (4 bytes)
        public Entity CoinPrefab;                       // Offset 24 (8 bytes)
    }

    /// <summary>
    /// Dynamic buffer element tracking individual coin visual entities attached to the runner stack.
    /// </summary>
    public struct CoinStackElement : IBufferElementData
    {
        public Entity CoinVisualEntity;  // Offset 0 (8 bytes)
        public float LocalYOffset;       // Offset 8 (4 bytes)
        public int ValueMultiplier;      // Offset 12 (4 bytes)
    }
}
```

---

## 3. Detailed Memory Layout & Alignment Table

| Component Struct | Field Name | Type | Size (Bytes) | Field Offset | Struct Total Size | Alignment |
|---|---|---|---|---|---|---|
| **CoinMultiplierGateComponent** | `GateType` | `MultiplierType` (enum byte) | 1 | 0 | 20 Bytes | 4-byte aligned |
| | `IsTriggered` | `bool` | 1 | 1 | | |
| | `ReservedPadding` | `ushort` | 2 | 2 | | |
| | `Value` | `float` | 4 | 4 | | |
| | `GateWidth` | `float` | 4 | 8 | | |
| | `TriggerDepth` | `float` | 4 | 12 | | |
| | `MinimumOutput` | `float` | 4 | 16 | | |
| **CoinSplitPhysicsComponent** | `CurrentVelocity` | `float3` | 12 | 0 | 44 Bytes | 4-byte aligned |
| | `SpreadAngle` | `float` | 4 | 12 | | |
| | `ImpulseSpeed` | `float` | 4 | 16 | | |
| | `StackHeightOffset` | `float` | 4 | 20 | | |
| | `Lifetime` | `float` | 4 | 24 | | |
| | `MaxLifetime` | `float` | 4 | 28 | | |
| | `GravityMultiplier` | `float` | 4 | 32 | | |
| | `CoinCount` | `int` | 4 | 36 | | |
| | `IsGrounded` | `bool` | 1 | 40 | | |
| | `IsCollectible` | `bool` | 1 | 41 | | |
| | `ReservedPadding` | `ushort` | 2 | 42 | | |
| **PlayerCoinRunnerComponent** | `CoinVisualPrefab` | `Entity` | 8 | 0 | 24 Bytes | 8-byte aligned |
| | `CurrentCoinCount` | `int` | 4 | 8 | | |
| | `StackSpacing` | `float` | 4 | 12 | | |
| | `MaxStackHeight` | `float` | 4 | 16 | | |
| | `SwerveSensitivity` | `float` | 4 | 20 | | |
| **CoinMultiplierEventComponent**| `GateEntity` | `Entity` | 8 | 0 | 32 Bytes | 8-byte aligned |
| | `InstigatorEntity` | `Entity` | 8 | 8 | | |
| | `GateType` | `MultiplierType` | 1 | 16 | | |
| | `Padding0` | `byte` | 1 | 17 | | |
| | `Padding1` | `ushort` | 2 | 18 | | |
| | `MultiplierValue` | `float` | 4 | 20 | | |
| | `CoinsBefore` | `int` | 4 | 24 | | |
| | `CoinsAfter` | `int` | 4 | 28 | | |
| **CoinSplitEventComponent** | `SpawnPosition` | `float3` | 12 | 0 | 32 Bytes | 8-byte aligned |
| | `SpreadAngle` | `float` | 4 | 12 | | |
| | `ImpulseSpeed` | `float` | 4 | 16 | | |
| | `QuantityToSpawn` | `int` | 4 | 20 | | |
| | `CoinPrefab` | `Entity` | 8 | 24 | | |
| **CoinStackElement** | `CoinVisualEntity` | `Entity` | 8 | 0 | 16 Bytes | 8-byte aligned |
| | `LocalYOffset` | `float` | 4 | 8 | | |
| | `ValueMultiplier` | `int` | 4 | 12 | | |

---

## 4. Multiplier Gate Collision & Trigger Detection Architecture in Entities 1.0+

### 4.1 Primary Design Pattern: Pure DOTS Transverse Box Trigger System (`CoinMultiplierGateSystem`)

In track-based hyper-casual runners, gates span across lanes on the transverse X-axis and have a thin longitudinal Z-axis depth. Utilizing pure DOTS mathematical bounds eliminates Unity Physics overhead while maintaining 100% deterministic, 60+ FPS execution.

#### Detection Algorithm:
1. **Query**: Iterate over all non-triggered gates matching `RefRW<CoinMultiplierGateComponent>`, `RefRO<LocalTransform>`, `WithAll<CoinMultiplierGateTag>()`.
2. **Runner Position Query**: Obtain `LocalTransform` and `RefRW<PlayerCoinRunnerComponent>` of the runner entity.
3. **Bounding Check (AABB / OBB Overlap in Local Track Space)**:
   $$\Delta Z = |P_z - G_z| \le \frac{\text{TriggerDepth}}{2}$$
   $$\Delta X = |P_x - G_x| \le \frac{\text{GateWidth}}{2}$$
4. **Trigger Execution**:
   - Calculate output coins:
     - **Additive**: $\text{NewCount} = \max(\text{MinimumOutput}, \text{CurrentCount} + \text{Value})$
     - **Multiplicative**: $\text{NewCount} = \max(\text{MinimumOutput}, \lfloor\text{CurrentCount} \times \text{Value}\rfloor)$
   - Set `gate.ValueRW.IsTriggered = true` (or disable `IEnableableComponent`).
   - Issue `CoinMultiplierEventComponent` to inform presentation/audio/VFX systems.
   - Enqueue coin spawning/splitting via `EntityCommandBuffer`.

```csharp
// Logic sketch for CoinMultiplierGateSystem
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct CoinMultiplierGateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingleton<LevelStateComponent>(out var levelState) || levelState.CurrentState != GameState.Playing)
            return;

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (runner, playerTransform, playerEntity) in SystemAPI.Query<RefRW<PlayerCoinRunnerComponent>, RefRO<LocalTransform>>().WithEntityAccess())
        {
            float3 pPos = playerTransform.ValueRO.Position;

            foreach (var (gate, gateTransform, gateEntity) in SystemAPI.Query<RefRW<CoinMultiplierGateComponent>, RefRO<LocalTransform>>().WithAll<CoinMultiplierGateTag>().WithEntityAccess())
            {
                if (gate.ValueRO.IsTriggered) continue;

                float3 gPos = gateTransform.ValueRO.Position;
                float halfWidth = gate.ValueRO.GateWidth * 0.5f;
                float halfDepth = gate.ValueRO.TriggerDepth * 0.5f;

                if (math.abs(pPos.z - gPos.z) <= halfDepth && math.abs(pPos.x - gPos.x) <= halfWidth)
                {
                    gate.ValueRW.IsTriggered = true;
                    int coinsBefore = runner.ValueRO.CurrentCoinCount;
                    int coinsAfter = coinsBefore;

                    if (gate.ValueRO.GateType == MultiplierType.Additive)
                    {
                        coinsAfter = (int)math.max(gate.ValueRO.MinimumOutput, coinsBefore + gate.ValueRO.Value);
                    }
                    else if (gate.ValueRO.GateType == MultiplierType.Multiplicative)
                    {
                        coinsAfter = (int)math.max(gate.ValueRO.MinimumOutput, coinsBefore * gate.ValueRO.Value);
                    }

                    int delta = coinsAfter - coinsBefore;
                    runner.ValueRW.CurrentCoinCount = coinsAfter;

                    // Emit event & audio tag
                    var eventEntity = ecb.CreateEntity();
                    ecb.AddComponent(eventEntity, new CoinMultiplierEventComponent
                    {
                        GateEntity = gateEntity,
                        InstigatorEntity = playerEntity,
                        GateType = gate.ValueRO.GateType,
                        MultiplierValue = gate.ValueRO.Value,
                        CoinsBefore = coinsBefore,
                        CoinsAfter = coinsAfter
                    });

                    var audioEntity = ecb.CreateEntity();
                    ecb.AddComponent(audioEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });

                    // Spawn physical splitting coins if delta > 0
                    if (delta > 0)
                    {
                        var splitEntity = ecb.CreateEntity();
                        ecb.AddComponent(splitEntity, new CoinSplitEventComponent
                        {
                            SpawnPosition = gPos + new float3(0, 0.5f, 0),
                            SpreadAngle = 60.0f,
                            ImpulseSpeed = 8.0f,
                            QuantityToSpawn = math.min(delta, 20), // visual burst cap
                            CoinPrefab = runner.ValueRO.CoinVisualPrefab
                        });
                    }

                    // Schedule gate visual feedback / destroy
                    ecb.AddComponent<DestroyEventComponent>(gateEntity);
                }
            }
        }
    }
}
```

### 4.2 Alternative Design Pattern: Unity Physics `ITriggerEventsJob`

If Unity Physics package (`com.unity.physics`) is active on gate colliders:

1. **Authoring**:
   - Gate authoring assigns `Physics Shape` (Trigger = True, Collision Filter: BelongsTo = Gate, CollidesWith = Player).
2. **Job Implementation**:
   - `CoinGateTriggerJob` implements `ITriggerEventsJob`.
   - Filters pairs using `ComponentLookup<CoinMultiplierGateComponent>` and `ComponentLookup<PlayerCoinRunnerComponent>`.
   - Enqueues gate trigger processing via `EntityCommandBuffer.ParallelWriter`.

---

## 5. Coin Splitting Physics System Architecture (`CoinSplitPhysicsSystem`)

When coins burst out of a gate or obstacle hit:
1. **Spawn Phase**: `CoinSplitEventComponent` triggers spawning $N$ entities with `CoinSplitPhysicsComponent` and `LocalTransform`.
2. **Initial Velocity Calculation**:
   - Uniform radial dispersion across `SpreadAngle` ($\theta \in [-\frac{\text{SpreadAngle}}{2}, +\frac{\text{SpreadAngle}}{2}]$):
     $$V_x = \text{ImpulseSpeed} \cdot \sin(\theta)$$
     $$V_y = \text{ImpulseSpeed} \cdot \cos(\theta) \cdot 0.5 + \text{UpwardBonus}$$
     $$V_z = \text{ImpulseSpeed} \cdot \cos(\theta)$$
3. **Simulation Phase (`CoinSplitPhysicsSystem`)**:
   - Updates `CurrentVelocity.y += Gravity.y * GravityMultiplier * deltaTime`.
   - Updates `Position += CurrentVelocity * deltaTime`.
   - Rotates coin on Y/Z axes for spinning visual feel.
   - Ground check: when `Position.y <= GroundY`, set `IsGrounded = true`, damp velocity ($V = V \times -0.3$), and flag `IsCollectible = true`.
   - Lifetime decrement: when `Lifetime <= 0`, recycle / destroy entity.

---

## 6. Verification Plan & Next Steps

1. **Component Compilation & Validation**:
   - Implementer creates component files in `Assets/Scripts/ECS/Components/`.
   - Implementer creates `CoinMultiplierGateAuthoring.cs` and `CoinSplitPhysicsAuthoring.cs` in `Assets/Scripts/ECS/Authoring/`.
2. **Slice Integration**:
   - Attach authoring components to `MathGate` GameObjects in `14_MoneyRush_Coins_Slice.prefab`.
3. **Execution Verification**:
   - Verify gate triggering updates `PlayerCoinRunnerComponent.CurrentCoinCount`.
   - Verify splitting physics burst visual coins upon passing positive multiplier gates.
