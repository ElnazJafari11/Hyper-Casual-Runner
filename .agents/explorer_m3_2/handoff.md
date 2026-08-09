# Handoff Report: Coin Multiplier & Splitting Physics Architecture (Milestone 3 - Slice 14)

**From**: Explorer 2 (`explorer_m3_2`)  
**To**: Parent / Implementer  
**Date**: 2026-07-22  
**Task**: System execution logic, coin splitting physics, authoring bakers, and generator integration for `14_MoneyRush_Coins`.  

---

## 1. Observation

1. **Existing Systems & Authoring Inspection**:
   - `Assets/Scripts/ECS/Systems/CollisionSystem.cs`: Queries `LocalTransform`, `CollectibleComponent`, `ObstacleComponent` and `CurrentRunStats`. Uses `EntityCommandBuffer(Allocator.Temp)` for destruction/stats updates.
   - `Assets/Scripts/ECS/Systems/SwarmSystem.cs`: Queries `MathGateComponent` for `GateOperation` (`Add`, `Subtract`, `Multiply`, `Divide`) and computes target counts.
   - `Assets/Scripts/ECS/Authoring/MathGateAuthoring.cs`: Uses `Baker<MathGateAuthoring>` converting MonoBehaviours to `MathGateComponent` with `TransformUsageFlags.Dynamic`.
   - `Assets/Scripts/Editor/ToolkitExampleGenerator.cs`: Lines 89-120 group `Coins` with `Swarm` and `Cannons`. Generates primitive track elements, player caps, and gates.

2. **Generator Current Behavior**:
   - Currently, `14_MoneyRush_Coins_Slice.prefab` uses `SwarmMechanicsAuthoring` and generic `MathGateAuthoring`. It lacks dedicated `CoinGateAuthoring`, `CoinPhysicsAuthoring`, and `CoinSpawnerAuthoring`.

---

## 2. Logic Chain

1. **System Execution Order**:
   - `CoinMultiplierSystem` must update in `SimulationSystemGroup` **before** `CoinPhysicsSystem` so that newly spawned split coins generated during gate collisions in frame $N$ can immediately have their positions and initial impulse integrated by `CoinPhysicsSystem` in frame $N$.
2. **Physics Trajectory Phases**:
   - Split coins transition from airborne parabolic motion ($y > y_{ground}$, gravity acceleration $g = 25.0 \text{ m/s}^2$) to ground bounce ($e = 0.4$, friction $\mu = 5.0$), and finally magnetic collection towards the player position ($v = v_{base} + \frac{k}{d}$).
   - This 3-phase physics model guarantees visually appealing coin spread without entities getting stuck permanently on track.
3. **Baker Alignment**:
   - Using standard DOTS `Baker<T>` pattern with `TransformUsageFlags.Dynamic` aligns with Unity Entities 1.0+ and existing authoring scripts in `Assets/Scripts/ECS/Authoring/`.
4. **Generator Integration**:
   - Updating `ToolkitExampleGenerator.cs` to explicitly handle `Coins` / `MoneyRush` ensures `14_MoneyRush_Coins_Slice.prefab` is auto-generated with multiplier gates (`+2`, `x3`, `+10`, `x2`, `x4`) and player coin spawner authoring without manual Editor tweaking.

---

## 3. Caveats

- **Audio/VFX Presentation Layer**: `CoinMultiplierSystem` and `CoinPhysicsSystem` emit tag components (`PlaySoundEventComponent`) which rely on presentation layer systems (`AudioManagerSystem`, `VFXManagerSystem`) for actual GameObject audio/particle rendering (following hybrid ECS design rule).
- **Player Entity Query Assumption**: Systems assume a single active Player entity with `PlayerComponent` and `LocalTransform` in the world during simulation.

---

## 4. Conclusion

The pure DOTS system design for `CoinMultiplierSystem` and `CoinPhysicsSystem`, authoring bakers (`CoinGateAuthoring`, `CoinPhysicsAuthoring`, `CoinSpawnerAuthoring`), exact 3D physics formulas, and generator update blueprints for `ToolkitExampleGenerator.cs` are fully defined and documented in `d:\Git\Hyper-Casual-Runner\.agents\explorer_m3_2\analysis.md`.

---

## 5. Verification Method

1. **File Inspection**:
   - Inspect `d:\Git\Hyper-Casual-Runner\.agents\explorer_m3_2\analysis.md` for full C# signatures, Burst attributes, 3D physics formulas, baker logic, and generator code snippets.
2. **Implementation Verification**:
   - Verify `CoinMultiplierSystem` has `[UpdateInGroup(typeof(SimulationSystemGroup))]` and `[UpdateBefore(typeof(CoinPhysicsSystem))]`.
   - Verify `CoinPhysicsSystem` calculates 3D parabolic spread trajectories, ground bounce damping, and magnetic target attraction.
   - Verify `ToolkitExampleGenerator.cs` populates `14_MoneyRush_Coins_Slice.prefab` with `CoinSpawnerAuthoring`, `CoinGateAuthoring`, and `CoinPhysicsAuthoring`.
