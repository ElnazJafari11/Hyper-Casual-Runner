# Handoff Report — Preview Exploration

**Working Directory**: `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_explorer_1`  
**Author**: Explorer Agent (`teamwork_preview_explorer_1`)  
**Recipient**: Orchestrator / Parent (`77518b05-575e-4a6f-8a64-beed938515a6`)  
**Date**: 2026-07-22  

---

## 1. Observation

Directly observed files, line ranges, and structures across the project:

### UI Toolkit & Progression Files
- `Assets/UI/LevelSelect.uxml` (lines 1–24): UXML layout containing `#LevelSelectOverlay`, `#LevelSelectModal`, `#Header` (`#CloseBtn`), and `#LevelGridScrollView` containing `#LevelGridContainer`.
- `Assets/UI/LevelCardItem.uxml` (lines 1–17): UXML element containing `#CardButton`, `#LevelNumberLabel`, `#StarsContainer` (`#Star1`, `#Star2`, `#Star3`), and `#StatusLabel`.
- `Assets/UI/LevelSelect.uss` (lines 1–107): USS rules defining styles `.level-card-button`, `.card-locked`, `.card-playing`, `.star-active`, `.star-inactive`, and `.card-status-label`.
- `Assets/Scripts/UI/UIManagerSystem.cs` (lines 468–525): `BuildLevelGrid()` constructs 21 level buttons, querying `GameProgressData.UnlockedLevelIndex`, `CurrentLevelIndex`, and `GetLevelStars(i)`. `SelectLevel(int levelIndex)` (lines 585–604) updates `GameProgressData.CurrentLevelIndex` and sets `seq.CurrentLevelIndex = levelIndex` and `seq.TransitionState = LevelTransitionState.TeardownCurrent` on `LevelSequenceComponent`.
- `Assets/Scripts/GameProgressData.cs` (lines 31–73, 75–96): Static class wrapping PlayerPrefs. Properties: `CurrentLevelIndex`, `UnlockedLevelIndex`, `GetLevelStars(int)`, `SetLevelStars(int, int)`, `CurrentSkinIndex`, `UnlockedSkins`, `IsSkinUnlocked(int)`, `UnlockSkin(int)`.
- `Assets/Scripts/ECS/Components/LevelProgressionComponents.cs` (lines 5–29): Defines `LevelTransitionState` enum (`Idle`, `PendingNext`, `TeardownCurrent`, `SpawningNext`, `Failed`), `SlicePrefabBufferElement` dynamic buffer, and `LevelSequenceComponent` struct.
- `Assets/Scripts/ECS/Systems/LevelProgressionSystem.cs` (lines 62–95): Handles `TeardownCurrent` by destroying `CurrentSliceInstance` and `SliceEntityTag` entities, then instantiates `buffer[CurrentLevelIndex].PrefabEntity` in `SpawningNext`.

### Obstacle & ECS Hazard Files
- `Assets/Scripts/ECS/Components/CollisionComponents.cs` (lines 5–9): Defines `ObstacleComponent` struct (`CollisionRadius`, `DamageAmount`).
- `Assets/Scripts/ECS/Authoring/HurdleWallAuthoring.cs` (lines 7–25) & `LaneObstacleAuthoring.cs` (lines 7–24): MonoBehaviour authoring classes using nested `Baker<T>` to bake components to entities with `TransformUsageFlags.Dynamic`.
- `Assets/Scripts/ECS/Components/MathTweenComponent.cs` (lines 5–18) & `MathTweenSystem.cs` (lines 9–37): System updating `LocalTransform` positions/rotations using `math.sin` and time-based trigonometric motion in `PresentationSystemGroup`.
- `Assets/Scripts/ECS/Systems/CollisionSystem.cs` (lines 9–69): System in `SimulationSystemGroup` performing distance checks between player `LocalTransform` and obstacle `LocalTransform`.
- `Assets/Scripts/ECS/Systems/AudioManagerSystem.cs` (lines 27–44) & `VFXManagerSystem.cs` (lines 22–33): Presentation systems consuming `PlaySoundEventComponent` and `DestroyEventComponent` tag entities to play AudioSource clips or spawn particle prefabs.

### Idle & Cosmetics Files
- `Assets/UI/IdleGameHUD.uxml` (lines 8–22): UXML containing `GoldLabel`, `PrestigeLabel`, `BuyUpgradeButton`, and `PrestigeButton`.
- `Assets/Scripts/UI/IdleUIManagerSystem.cs` (lines 9–81): `SystemBase` in `SimulationSystemGroup` binding UI buttons and updating `GoldLabel` (`CurrentRunStats.CurrentGold`) and `PrestigeLabel` (`PersistentPlayerStats.PrestigeCurrency`).
- `Assets/Scripts/ECS/Components/IdleComponents.cs` (lines 6–11, 57): `PersistentPlayerStats` struct (`PrestigeCurrency`, `PermanentDamageMultiplier`, `PermanentGoldMultiplier`) and `PrestigeEventComponent`.
- `Assets/Scripts/ECS/Systems/Idle/PrestigeSystem.cs` (lines 16–51): Listens for `PrestigeEventComponent`, increments `persistentStats.ValueRW.PrestigeCurrency += 1.0`, resets `CurrentRunStats` and wallet buffers.
- `Assets/Scripts/ECS/Systems/SkinApplicatorSystem.cs` (lines 9–50): Reads `GameProgressData.CurrentSkinIndex` and applies material color overrides (`float4 targetColor`) to player/swarm entities.

### Tooling Verification
- Log file `d:\Git\Hyper-Casual-Runner\.agents\auditor_m3_1\unity_test.log` (lines 10–24) demonstrates batchmode Unity CLI test command:
  `"C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe" -batchmode -nographics -projectPath d:\Git\Hyper-Casual-Runner -runTests -testPlatform EditMode -testResults d:\Git\Hyper-Casual-Runner\test_results.xml -logFile <path>`

---

## 2. Logic Chain

1. **Level Select Screen (M1)**:
   - Observation: `LevelSelect.uxml`, `LevelCardItem.uxml`, and `LevelSelect.uss` exist in `Assets/UI/`. `UIManagerSystem.cs` contains `BuildLevelGrid()` and `SelectLevel(int)`.
   - Deduction: The UI Toolkit assets and data binding logic in `UIManagerSystem.cs` are already established. Selecting a level sets `GameProgressData.CurrentLevelIndex` and sends a `TeardownCurrent` transition command to the `LevelSequenceComponent` singleton entity, triggering `LevelProgressionSystem` to reload level slice prefabs.
   - Refinement: `UIManagerSystem.cs` should clone `LevelCardItem.uxml` for clean UI Toolkit template rendering while preserving selection guard checks against `UnlockedLevelIndex`.

2. **Advanced Obstacle Variants (M2)**:
   - Observation: Existing obstacles use `ObstacleComponent`, authored via `Baker<T>` in `Assets/Scripts/ECS/Authoring/`, moved via `MathTweenSystem.cs`, checked for collision in `CollisionSystem.cs`, and triggering hybrid audio/VFX via tag entities (`PlaySoundEventComponent`, `DestroyEventComponent`).
   - Deduction: To implement Moving Walls, Pendulum Swings, and Splitting Hazards:
     - Moving Walls need `MovingWallComponent`, `MovingWallAuthoring`, and `MovingWallSystem` (sine wave translation along configurable axis).
     - Pendulum Swings need `PendulumSwingComponent`, `PendulumSwingAuthoring`, and `PendulumSwingSystem` (rotational oscillation around pivot).
     - Splitting Hazards need `SplittingHazardComponent`, `SplittingHazardAuthoring`, and `SplittingHazardSystem` (instantiating child hazard prefabs via ECB on trigger and destroying parent with VFX/SFX).

3. **Cosmetics Shop & Prestige Transactions (M3)**:
   - Observation: `IdleGameHUD.uxml` displays Prestige & Gold. `IdleUIManagerSystem.cs` handles clicks. `PersistentPlayerStats` holds `double PrestigeCurrency`. `GameProgressData` manages skin bitmasks (`UnlockedSkins`, `CurrentSkinIndex`). `SkinApplicatorSystem.cs` applies skin colors to DOTS entities.
   - Deduction: Extending `IdleGameHUD.uxml` with a Cosmetics Shop tab allowing skin purchases via `CosmeticPurchaseEventComponent` cleanly integrates with `PersistentPlayerStats.PrestigeCurrency`. `CosmeticsShopSystem` deducts `PrestigeCurrency` and unlocks/equips skins via `GameProgressData.UnlockSkin(index)`, triggering visual updates in `SkinApplicatorSystem`.

---

## 3. Caveats

- **Unity Editor State**: Investigation was strictly read-only. Scripts were verified by inspect-and-trace; runtime behavior should be validated via automated batchmode NUnit edit-mode tests (`unity_test.log` command).
- **URP Shader Material Properties**: `SkinApplicatorSystem.cs` currently has commented placeholder code for `URPMaterialPropertyBaseColor`. Ensure URP material property structs exist or use material property blocks when executing skin color updates in M3.

---

## 4. Conclusion

The codebase architecture is clean, modular, and fully aligned with Unity 6, DOTS ECS 1.x, and UI Toolkit best practices.
- M1 (Level Select Screen) is fully designed around `UIManagerSystem.cs`, `GameProgressData.cs`, and `LevelProgressionSystem.cs`.
- M2 (3 Advanced Obstacles) is fully designed using `MovingWallSystem`, `PendulumSwingSystem`, and `SplittingHazardSystem` following the existing `MathTweenSystem` / `CollisionSystem` patterns.
- M3 (Cosmetics Shop Extension) is fully designed connecting `IdleGameHUD.uxml`, `CosmeticsShopSystem`, `PersistentPlayerStats.PrestigeCurrency`, and `GameProgressData` skin bitmasks.

Detailed technical specs and component layouts are recorded in `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_explorer_1/analysis.md`.

---

## 5. Verification Method

To independently verify this exploration report:
1. **Inspect Analysis Report**: View `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_explorer_1/analysis.md`.
2. **Inspect Code Files**:
   - `Assets/UI/LevelSelect.uxml`, `LevelCardItem.uxml`, `LevelSelect.uss`
   - `Assets/Scripts/UI/UIManagerSystem.cs`, `IdleUIManagerSystem.cs`
   - `Assets/Scripts/GameProgressData.cs`
   - `Assets/Scripts/ECS/Components/LevelProgressionComponents.cs`, `IdleComponents.cs`, `CollisionComponents.cs`, `MathTweenComponent.cs`
   - `Assets/Scripts/ECS/Systems/LevelProgressionSystem.cs`, `CollisionSystem.cs`, `MathTweenSystem.cs`, `Idle/PrestigeSystem.cs`, `SkinApplicatorSystem.cs`
3. **Run Automated EditMode Test Suite**:
   ```powershell
   & "C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe" -batchmode -nographics -projectPath "d:\Git\Hyper-Casual-Runner" -runTests -testPlatform EditMode -testResults "d:\Git\Hyper-Casual-Runner\test_results.xml" -logFile "d:\Git\Hyper-Casual-Runner\.agents\teamwork_preview_explorer_1\unity_test.log"
   ```
