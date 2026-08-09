# Handoff Report: Explorer 3 — Milestone 3 (Coin Multiplier & Splitting Physics `14_MoneyRush_Coins`)

## 1. Observation
- **Slice Prefab File**: `Assets/ToolkitExamples/14_MoneyRush_Coins_Slice.prefab` exists on disk (7,107 lines, 183KB).
  - Root GameObject `14_MoneyRush_Coins_Slice` (line 681).
  - Contains `PlayerEntity` (line 398) with `PlayerAuthoring`, `SwarmMechanicsAuthoring` (StartingCount: 1), and `MathTweenAuthoring`.
  - Contains 3 `MathGate` objects (lines 859, 1625, 1770) configured with `GateOperation.Add`, `Multiply`, `Subtract`.
  - Contains 7 `EnemyEntity` objects (lines 142, 269, 558, 1003, 1130, 1257, 1497) with `EnemyAuthoring`.
  - Contains `LevelManager` (line 19) with `LevelManagerAuthoring`, `UIDocument`, `UIManagerSystem`, `AudioManagerAuthoring`, `MockAdsManager`, `VFXManagerAuthoring`.
  - Contains `EndZone` (line 732) and `HitVFX_Template` (line 2168).
- **Verification Suite**:
  - `ToolkitExampleGenerator.cs` (lines 577–649) defines `RunVerificationSuite()` which verifies prefabs on disk and checks specific authoring components per slice.
  - `Assets/Scripts/Editor/Tests/ToolkitGeneratorTests.cs` (lines 1–115) contains NUnit tests asserting prefab existence and component composition.
- **Hybrid Presentation Layer (VFX/Audio)**:
  - `AudioManagerSystem.cs` (lines 27–44) queries `PlaySoundEventComponent` in `PresentationSystemGroup`, triggers `PlayOneShot()`, and destroys event entities.
  - `VFXManagerSystem.cs` (lines 22–32) queries `DestroyEventComponent` and `LocalTransform` in `PresentationSystemGroup`, instantiates `HitVFXPrefab`, and destroys ECS entities.

## 2. Logic Chain
1. **Prefab Composition**: `14_MoneyRush_Coins_Slice.prefab` is currently generated using `ToolkitExampleGenerator.cs` shared generator logic (`Swarm` / `Coins` / `Cannons`). It contains the core baseline hierarchy (`PlayerEntity`, `MathGate`, `LevelManager`, `EndZone`, `EnemyEntity`).
2. **Verification Suite Validation**: `RunVerificationSuite()` checks that `14_MoneyRush_Coins_Slice.prefab` exists and has valid child nodes. Adding explicit verification assertions for `CoinMultiplier` / `CoinSplitting` properties in `RunVerificationSuite()` and `ToolkitGeneratorTests.cs` ensures Milestone 3 criteria are strictly enforced in CI/Editor builds.
3. **Decoupled Hybrid Presentation**: Coin collection and splitting logic MUST NOT directly call Unity Audio or Particle APIs in ECS simulation jobs. Instead, simulation systems create `PlaySoundEventComponent` and `DestroyEventComponent` tag entities, allowing `AudioManagerSystem` and `VFXManagerSystem` to execute cleanly in `PresentationSystemGroup`.

## 3. Caveats
- `ToolkitExampleVerification.cs` as a standalone file does not currently exist in `Assets/Scripts/Editor/`; verification logic is embedded within `ToolkitExampleGenerator.cs`.
- Splitting physics trajectory parameters (impulse vector, spread angle, boundary collision) will be defined during implementation, building on the existing `MathGate` and `SwarmSystem` DOTS architecture.

## 4. Conclusion
Slice 14 (`14_MoneyRush_Coins`) has a fully functional prefab foundation in `Assets/ToolkitExamples/14_MoneyRush_Coins_Slice.prefab` with `PlayerEntity`, `MathGate` objects, and hybrid audio/VFX managers (`AudioManagerAuthoring`, `VFXManagerAuthoring`). The verification suite strategy and test criteria are defined, machine-checkable, and ready for Milestone 3 implementation.

## 5. Verification Method
1. Inspect prefab file at `Assets/ToolkitExamples/14_MoneyRush_Coins_Slice.prefab` via `view_file` or Unity Editor Inspector.
2. Execute `ToolkitExampleGenerator.RunVerificationSuite()` via `IdleToolkit -> Run Verification Suite` menu item in Unity Editor.
3. Verify test execution in `Assets/Scripts/Editor/Tests/ToolkitGeneratorTests.cs` using Unity Test Runner.
