## 2026-07-22T10:53:03Z

<USER_REQUEST>
You are teamwork_preview_worker for Milestone 4: Multi-Level Progression Loader & UI Integration in Hyper-Casual Runner Toolkit.
Your working directory is d:\Git\Hyper-Casual-Runner\.agents\worker_m4_1. Create this directory if it does not exist.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

TASK SCOPE:
Implement Milestone 4 based on the technical design specification in `d:\Git\Hyper-Casual-Runner\.agents\orchestrator\m4_design.md` and Explorer reports:
1. `Assets/Scripts/ECS/Components/LevelProgressionComponents.cs`:
   - `LevelTransitionState` enum (`Idle`, `PendingNext`, `TeardownCurrent`, `SpawningNext`, `Failed`)
   - `SlicePrefabBufferElement` (`IBufferElementData` with `[InternalBufferCapacity(21)]` holding `Entity PrefabEntity`)
   - `LevelSequenceComponent` (`IComponentData` storing `CurrentLevelIndex`, `MaxLevels`, `UnlockedLevelIndex`, `LevelTransitionState`, `Entity CurrentSliceInstance`, `bool LoopSequencing`, `float AutoTransitionTimer`)
   - `SliceEntityTag` (`IComponentData` tag)
   - `MetaProgressionComponent` (`IComponentData` singleton)
   - `SaveProgressEventComponent` (`IComponentData`, `IEnableableComponent` event)
2. `Assets/Scripts/ECS/Authoring/LevelSequenceAuthoring.cs`:
   - Authoring component with `GameObject[] SlicePrefabs` and `Baker` converting prefabs to `SlicePrefabBufferElement` buffer elements.
3. `Assets/Scripts/ECS/Systems/LevelProgressionSystem.cs`:
   - Pure DOTS `ISystem` in `SimulationSystemGroup` managing slice loading, `LinkedEntityGroup` recursive teardown, `SliceEntityTag` query cleanup, and level index updates.
4. `Assets/Scripts/ECS/Systems/MetaProgressionSaveSystem.cs`:
   - Main-thread presentation system handling `SaveProgressEventComponent` and atomic `GameProgressData.Save()`.
5. `Assets/Scripts/GameProgressData.cs`:
   - Extend with `CurrentLevelIndex`, `UnlockedLevelIndex`, `GetLevelStars`, `SetLevelStars`, `SaveLevelCompletion`, and `Save()`.
6. UI Toolkit Documents & Styles (`Assets/UI/`):
   - Create UXML and USS files (`GameHUD.uxml`, `GameHUD.uss`, `LevelSelect.uxml`, `LevelSelect.uss`, `VictoryScreen.uxml`, `VictoryScreen.uss`, `DefeatScreen.uxml`, `DefeatScreen.uss`, `LevelCardItem.uxml`). Strict rule: ONLY Unity UI Toolkit (`UnityEngine.UIElements`). Zero legacy uGUI/Canvas!
7. UI Controller (`Assets/Scripts/UI/UIManagerSystem.cs` or `UIManager.cs` updates):
   - Bind ECS simulation state (`LevelStateComponent`, `PlayerCoinRunnerComponent`, Z positions) to UI Toolkit elements. Include Level progress bar, live coin count, 1-21 Level Select grid with stars, Victory screen with 1.5x-5.0x multiplier wheel ticker, and Defeat screen.
8. Verification:
   - Run compilation checks and tests to verify everything compiles clean with 0 errors.

Write a detailed handoff report in `d:\Git\Hyper-Casual-Runner\.agents\worker_m4_1\handoff.md` detailing all modified/created files, compilation results, and verification steps. Send a message to parent when complete.
</USER_REQUEST>
