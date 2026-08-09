# Handoff Report: Meta-Progression Persistence & `GameProgressData.cs` Integration

**Agent**: Explorer 3  
**Milestone**: Milestone 4 — Multi-Level Progression Loader & UI Integration  
**Working Directory**: `d:\Git\Hyper-Casual-Runner\.agents\explorer_m4_3`  
**Date**: 2026-07-22

---

## 1. Observation

1. **`GameProgressData.cs` Inspection**:
   - Location: `Assets/Scripts/GameProgressData.cs:1-99`
   - Contains static properties wrapping `PlayerPrefs`:
     - `TotalGold` (`"HCR_TotalGold"`, default `0`) (lines 7, 11-15)
     - `SwarmLevel` (`"HCR_SwarmLevel"`, default `1`) (lines 8, 17-21)
     - `IncomeLevel` (`"HCR_IncomeLevel"`, default `1`) (lines 9, 23-27)
     - `CurrentSkinIndex` (`"HCR_SkinIndex"`, default `0`) (lines 29-33)
     - `UnlockedSkins` (`"HCR_UnlockedSkins"`, default `1`) (lines 35-39)
     - `LastLoginDate` (`"HCR_LastLoginDate"`) & `LoginStreak` (`"HCR_LoginStreak"`) (lines 51-61)
   - Lacks level progress tracking keys (`CurrentLevelIndex`, `UnlockedLevelIndex`, per-level star ratings).
   - Property setters call `PlayerPrefs.Save()` individually without a bulk/atomic level completion save wrapper method.

2. **Existing UI & ECS Binding**:
   - `UIManagerSystem.cs` (`Assets/Scripts/UI/UIManagerSystem.cs:210-221`) checks `state.CurrentState == GameState.Victory` and manually increments `GameProgressData.TotalGold += runGold`.
   - `ToolkitHubManager.cs` (`Assets/Scripts/UI/ToolkitHubManager.cs:24`) reads `GameProgressData.TotalGold` for title UI display.
   - `CoinMultiplierComponents.cs` (`Assets/Scripts/ECS/Components/CoinMultiplierComponents.cs:38-45`) defines `PlayerCoinRunnerComponent` holding `CurrentCoinCount`.

3. **Project Guidelines & Rules (`.agents/AGENTS.md`)**:
   - Rule 1: UI Architecture Constraint: UI Toolkit (UI Elements) for all game UI.
   - Rule 2: Hybrid ECS Architecture Pattern for Audio, VFX, and presentation layer systems.
   - Rule 3: Meta-Progression Modularity: Keep meta-progression modular and lightweight. Use `GameProgressData.cs` (PlayerPrefs wrapper) for persistence. Avoid bloated universal economy systems.

---

## 2. Logic Chain

1. **Observation 1** shows that `GameProgressData.cs` currently manages currency (`TotalGold`) and skin/upgrades, but lacks fields for current level index, unlocked level index, level stars, or explicit atomic save methods.
2. **Observation 2** shows that `UIManagerSystem.cs` currently accesses `GameProgressData.TotalGold` directly inside `Update()`, which mixes presentation logic with persistence operations instead of using an event-driven ECS bridge system.
3. Combining **Observations 1, 2, and 3**, pure DOTS simulation running on worker threads cannot call `PlayerPrefs` directly. Therefore, an ECS Data Binding & Persistence Bridge is required:
   - `MetaProgressionComponent` singleton storing runtime ECS state (`TotalGold`, `CurrentLevelIndex`, `UnlockedLevelIndex`).
   - `SaveProgressEventComponent` event tag for signaling level completion.
   - `MetaProgressionBootstrapSystem` (in `InitializationSystemGroup`) reading `PlayerPrefs` via `GameProgressData` into ECS singletons on startup.
   - `MetaProgressionSaveSystem` (in `PresentationSystemGroup`, main thread) intercepting completion events, updating `GameProgressData`, and calling `GameProgressData.Save()`.
4. This architecture fulfills all milestone requirements, provides clean UI binding to UI Toolkit (`UIManagerSystem`), and complies fully with project modularity constraints.

---

## 3. Caveats

1. **No Source Implementation**: As an Explorer agent (read-only investigation), code changes were not applied to `Assets/Scripts/GameProgressData.cs` or `Assets/Scripts/ECS/`. Full implementation will be performed by the Implementer agent based on `analysis.md`.
2. **Multi-Level Prefab Integration**: Level index persistence assumes a 0-based slice indexing scheme (`0..20` for 21 levels) corresponding to Explorer 1's `LevelSequenceComponent` design.

---

## 4. Conclusion

The meta-progression persistence architecture and `GameProgressData.cs` integration for Milestone 4 is fully designed and documented in `d:\Git\Hyper-Casual-Runner\.agents\explorer_m4_3\analysis.md`. The design cleanly decouples PlayerPrefs storage (`GameProgressData.cs`), ECS singletons (`MetaProgressionComponent`), event-driven save triggers (`SaveProgressEventComponent`), and UI Toolkit visualization (`UIManagerSystem`).

---

## 5. Verification Method

1. **Inspect Analysis Report**:
   - File: `d:\Git\Hyper-Casual-Runner\.agents\explorer_m4_3\analysis.md`
   - Verify presence of PlayerPrefs Key Matrix, ECS component layouts, system specifications (`MetaProgressionBootstrapSystem`, `MetaProgressionSaveSystem`), startup/victory sequence diagrams, and code blueprints.

2. **Automated Unit Testing Plan**:
   - Run existing project tests:
     `dotnet test` or Unity Test Runner via `Editor/Tests/CoinSystemTests.cs`.
   - Implementer will add `MetaProgressionTests.cs` testing startup reading from `GameProgressData`, `SaveProgressEventComponent` triggering atomic PlayerPrefs updates, and `UnlockedLevelIndex` progression.

3. **Invalidation Conditions**:
   - Any direct `PlayerPrefs` calls from within Jobified pure DOTS systems.
   - Non-atomic updates where `UnlockedLevelIndex` or `TotalGold` fail to write on level completion.
   - Modifying `GameProgressData.cs` in a way that breaks existing usages in `ToolkitHubManager` or `SwarmMechanicsAuthoring`.
