# Handoff Report: Explorer 1 - Milestone 3 (14_MoneyRush_Coins)

## 1. Observation

- **Inspected Files**:
  - `Assets/Scripts/ECS/Components/CollisionComponents.cs` (lines 1-17): Found standard collision struct pattern (`ObstacleComponent`, `CollectibleComponent`).
  - `Assets/Scripts/ECS/Components/RunnerComponents.cs` (lines 1-12): Found `PlayerComponent`.
  - `Assets/Scripts/ECS/Components/SwarmComponent.cs` (lines 5-18): Found `GateOperation` enum (`Add`, `Subtract`, `Multiply`, `Divide`) and `MathGateComponent`.
  - `Assets/Scripts/ECS/Components/DestroyEventComponent.cs` (lines 1-10): Found frame-local event component using `IEnableableComponent`.
  - `Assets/Scripts/ECS/Components/PlaySoundEventComponent.cs` (lines 1-17): Found presentation audio event component pattern.
  - `Assets/Scripts/ECS/Authoring/MathGateAuthoring.cs` (lines 1-28): Found baker implementation for math gates (`TransformUsageFlags.Dynamic`).
  - `Assets/Scripts/ECS/Systems/ShooterGateSystem.cs` (lines 20-53): Found distance-based gate collision check and `DestroyEventComponent` ECB pattern.
  - `Assets/ToolkitExamples/14_MoneyRush_Coins_Slice.prefab`: Verified presence of slice 14 prefab with `MathGate`, `PlayerEntity`, `EndZone`, `LevelManager`.
  - `docs/project-context.md` (lines 1-20): Identified quality bar (Playable 60FPS DOTS runner mechanics across 21 core slices) and critical-lane domains (DOTS ECS Systems, Math Gate / Numerics logic).

## 2. Logic Chain

1. **Existing Patterns**: Examination of `SwarmComponent.cs` and `ShooterGateSystem.cs` showed that existing gate mechanisms use simple enums, radius/width floats, and direct distance/AABB calculations during the `SimulationSystemGroup` update pass.
2. **Component Requirements for Slice 14**:
   - `CoinMultiplierGateComponent`: Needs to distinguish additive (`+X`) vs multiplicative (`xX`) operations, track execution value, lateral width, longitudinal trigger depth, and a `bool IsTriggered` state flag to prevent multi-triggering across ticks.
   - `CoinSplitPhysicsComponent`: Needs to model parabolic burst trajectories with `float3 CurrentVelocity`, `SpreadAngle`, `ImpulseSpeed`, `StackHeightOffset`, `Lifetime`, `GravityMultiplier`, and grounded/collectible flags.
   - Auxiliary tags & buffers: Needs `CoinTag`, `CoinMultiplierGateTag`, `CoinMultiplierEventComponent`, `CoinSplitEventComponent`, `PlayerCoinRunnerComponent`, and dynamic buffer `CoinStackElement` to cleanly separate simulation, presentation, and dynamic coin stack rendering.
3. **Memory Layout Alignment**:
   - Arranged primitive data types in all component structs to align on 4-byte and 8-byte boundaries without implicit compiler padding holes.
   - Kept struct sizes compact (20B for gate component, 44B for split physics component, 24B for player runner component, 32B for event components) to maximize DOTS chunk density and L1/L2 CPU cache hit rates.
4. **Collision & Trigger Detection Architecture**:
   - Primary: Transverse bounding box check in `CoinMultiplierGateSystem` matching current repo patterns. Enables zero-dependency, ultra-fast 60FPS execution.
   - Alternative: Unity Physics `ITriggerEventsJob` using `PhysicsWorldSingleton` for projects utilizing `com.unity.physics`.

## 3. Caveats

- Investigation is purely read-only per agent constraints; source code files under `Assets/Scripts/ECS/` have not been modified or added yet.
- Visual rendering of the coin stack (whether using GPU instancing, child entities, or dynamic buffer mesh deformation) will depend on the Implementer's presentation system choices.

## 4. Conclusion

The component data models, memory layout specifications, and multiplier gate collision architecture for Slice 14 (`14_MoneyRush_Coins`) are fully designed and documented in `d:\Git\Hyper-Casual-Runner\.agents\explorer_m3_1\analysis.md`. The design aligns perfectly with existing project ECS patterns and DOTS Entities 1.0+ standards.

## 5. Verification Method

To independently verify the investigation findings and proposed architecture:
1. View `d:\Git\Hyper-Casual-Runner\.agents\explorer_m3_1\analysis.md` and inspect Section 2 (Component Definitions) and Section 3 (Memory Layout Table).
2. Compare struct definitions against `Assets/Scripts/ECS/Components/SwarmComponent.cs` and `Assets/Scripts/ECS/Components/CollisionComponents.cs` to confirm naming and structural consistency.
3. Invalidation condition: If any struct contains unaligned fields causing implicit compiler padding bytes or fails to compile with `Unity.Entities 1.0+`.
