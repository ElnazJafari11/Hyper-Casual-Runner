# Milestone 3 Investigation Analysis: Coin Multiplier & Splitting Physics (`14_MoneyRush_Coins`)

**Author**: Explorer 3  
**Date**: 2026-07-22  
**Target Slice**: `14_MoneyRush_Coins` (`Assets/ToolkitExamples/14_MoneyRush_Coins_Slice.prefab`)  

---

## 1. Slice Prefab Structure Analysis

### 1.1 Existing Prefab Asset Overview
- **Path**: `Assets/ToolkitExamples/14_MoneyRush_Coins_Slice.prefab`
- **Root GameObject**: `14_MoneyRush_Coins_Slice` (Transform position: `(0, 0, 0)`)
- **Generator Origin**: Built by `ToolkitExampleGenerator.cs` (lines 89–120).

### 1.2 Hierarchy & Component Breakdown

| GameObject Name | Transform Position / Scale | Core Authoring & Components | Purpose / Functionality |
| :--- | :--- | :--- | :--- |
| `14_MoneyRush_Coins_Slice` | `Pos: (0,0,0)` `Scale: (1,1,1)` | Root Transform | Parent slice container |
| `Plane` | `Pos: (0,0,0)` `Scale: (2,1,10)` | MeshFilter (Plane), MeshRenderer, MeshCollider | Ground track environment (100 units long) |
| `PlayerEntity` | `Pos: (0,1,-40)` `Scale: (1,1,1)` | `PlayerAuthoring`<br>`SwarmMechanicsAuthoring` (StartingCount: 1)<br>`MathTweenAuthoring` (ScaleUniform, Amp: 0.2, Speed: 15)<br>MeshFilter (Capsule), CapsuleCollider, MeshRenderer | Lead coin swarm entity moving forward via swerve input |
| `MathGate` (x3) | Gate 1: `(0, 1, -20)`<br>Gate 2: `(-2, 1, 0)`<br>Gate 3: `(2, 1, 20)` | `MathGateAuthoring`<br>- Gate 1: `Add +10`<br>- Gate 2: `Multiply x2`<br>- Gate 3: `Subtract -5`<br>MeshFilter (Cube), BoxCollider, MeshRenderer | Math multiplier/divider gates modulating coin swarm volume |
| `EnemyEntity` (x7) | Grid positions: `X: -2..2, Z: 30..35` | `EnemyAuthoring` (CollisionRadius: 0.5, Health: 1)<br>MeshFilter (Sphere), SphereCollider, MeshRenderer | Obstacles/hazards that reduce coin count on collision |
| `LevelManager` | `Pos: (0,0,0)` `Scale: (1,1,1)` | `LevelManagerAuthoring`<br>`UIDocument`<br>`UIManagerSystem`<br>`AudioManagerAuthoring` (`PickupSFX`, `ExplosionSFX`, `VictorySFX`)<br>`MockAdsManager`<br>`VFXManagerAuthoring` (`HitVFXPrefab`) | System manager singletons & presentation layer anchors |
| `EndZone` | `Pos: (0,0,40)` `Scale: (10,1,5)` | MeshFilter (Cube), BoxCollider, MeshRenderer | Finish line triggering victory state |
| `HitVFX_Template` | `Pos: (0,0,0)` `Scale: (1,1,1)` | ParticleSystem, MeshRenderer | Template prefab for collision/destruction visual effects |

### 1.3 Material and Asset References
- **Meshes**: Standard Unity primitives (`Capsule`, `Cube`, `Sphere`, `Plane`).
- **Materials**: Standard default materials assigned to MeshRenderers.
- **Audio Clips & VFX Prefabs**: Referenced on `AudioManagerAuthoring` and `VFXManagerAuthoring`.

---

## 2. Verification Suite Inspection & Rules

### 2.1 Current Verification Architecture
- **Location**: `Assets/Scripts/Editor/ToolkitExampleGenerator.cs` (method `RunVerificationSuite()`).
- **Test Suite**: `Assets/Scripts/Editor/Tests/ToolkitGeneratorTests.cs`.
- **Report Output**: Writes empirical report to `verification_report.txt` in project root and logs to Unity Console.

### 2.2 Validation Rules for Slice 14 (`14_MoneyRush_Coins`)
1. **Asset Integrity**: `14_MoneyRush_Coins_Slice.prefab` must exist at `Assets/ToolkitExamples/` and load as a valid `GameObject`.
2. **Entity Structure**:
   - `PlayerEntity` child transform must exist under slice root.
   - `PlayerEntity` must hold `SwarmMechanicsAuthoring` (or dedicated coin multiplier/splitting authoring component).
3. **Multiplier Gates**:
   - Prefab must contain `MathGateAuthoring` instances with valid operations (`Add`, `Multiply`, `Subtract`, `Divide`).
4. **Verification Report Output**:
   - `RunVerificationSuite()` checks child count, logs component configuration, and tallies `passed/total` count.

---

## 3. Hybrid Presentation (VFX/Audio) Integration

In accordance with project rules for DOTS Hybrid ECS Architecture:
- **Rule**: Presentation logic (Audio/VFX) MUST be decoupled from simulation logic. Tag entities are spawned during simulation updates, and Presentation layer systems read these tags to trigger standard Unity `AudioSource` or `ParticleSystem` components.

### 3.1 Audio Integration Pattern (`AudioManagerSystem`)
1. **Authoring / Singleton**: `AudioManagerAuthoring` on `LevelManager` bakes `AudioManagerComponent` (holding `AudioClip` references: `PickupSFX`, `ExplosionSFX`, `VictorySFX`).
2. **Simulation Event Trigger**: When a coin is collected, multiplied, or split, the simulation system spawns a tag entity with:
   ```csharp
   ecb.CreateEntity(state.EntityManager.CreateArchetype(typeof(PlaySoundEventComponent)));
   ecb.SetComponent(new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
   ```
3. **Presentation System**: `AudioManagerSystem` runs in `[UpdateInGroup(typeof(PresentationSystemGroup))]`, queries `PlaySoundEventComponent`, invokes `PlayOneShot()`, and destroys the event entity via ECB.

### 3.2 VFX Integration Pattern (`VFXManagerSystem`)
1. **Authoring / Singleton**: `VFXManagerAuthoring` on `LevelManager` bakes `VFXManagerComponent` (holding `HitVFXPrefab`).
2. **Simulation Event Trigger**: When coins or enemies are collected/destroyed, entities are tagged with `DestroyEventComponent`.
3. **Presentation System**: `VFXManagerSystem` runs in `[UpdateInGroup(typeof(PresentationSystemGroup))]`, queries entities with `LocalTransform` and `DestroyEventComponent`, instantiates `HitVFXPrefab`, schedules cleanup (`Object.Destroy(vfx, 1f)`), and destroys the ECS entity.

---

## 4. Milestone 3 Verification Strategy & Test Criteria

### 4.1 Pass/Fail Test Criteria

| # | Criterion | Machine-Checkable Verification Method | Target Expected Result |
| :---: | :--- | :--- | :--- |
| **C1** | **Prefab File Existence** | `AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ToolkitExamples/14_MoneyRush_Coins_Slice.prefab")` | Non-null `GameObject` loaded |
| **C2** | **Player Entity Configuration** | Inspect `prefab.transform.Find("PlayerEntity")` for `PlayerAuthoring` and `SwarmMechanicsAuthoring` | `SwarmMechanicsAuthoring` present, `StartingCount >= 1` |
| **C3** | **Coin Multiplier Gate Count** | Count children containing `MathGateAuthoring` with `Operation == Multiply` | At least 1 `Multiply` gate present, total gates >= 3 |
| **C4** | **Hybrid Event Tag Compliance** | Inspect simulation code for `PlaySoundEventComponent` and `DestroyEventComponent` creation | Events created in `SimulationSystemGroup`, consumed in `PresentationSystemGroup` |
| **C5** | **Verification Suite Execution** | Execute `ToolkitExampleGenerator.RunVerificationSuite()` | `verification_report.txt` includes `[PASS] 14_MoneyRush_Coins` |

### 4.2 Verification Execution Commands
1. **Editor Menu Verification**: Run `IdleToolkit -> Run Verification Suite` or `IdleToolkit -> Generate and Verify All`.
2. **NUnit Test Execution**: Execute NUnit tests in `ToolkitGeneratorTests.cs` via Unity Test Runner.
