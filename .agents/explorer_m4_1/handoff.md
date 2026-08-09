# Handoff Report: Milestone 4 Multi-Level Progression Loader Architecture

## 1. Observation
* **Game State Management**: `LevelStateComponent` (`Assets/Scripts/ECS/Components/LevelStateComponent.cs`, lines 13-16) contains `public GameState CurrentState` where `GameState` is `Pregame`, `Playing`, `Victory`, or `Defeat`.
* **State Initialization**: `LevelManagerAuthoring` (`Assets/Scripts/ECS/Authoring/EndZoneAuthoring.cs`, lines 22-32) bakes `LevelStateComponent` with `CurrentState = GameState.Pregame`.
* **Victory Triggers**: `WinConditionSystem` (`Assets/Scripts/ECS/Systems/WinConditionSystem.cs`, lines 40-45) sets `CurrentState = GameState.Victory` upon player reaching `EndZoneComponent`.
* **Defeat Triggers**: `BridgeBuilderSystem`, `GridPathfinderSystem`, `LaneSystem`, `SnakeCollisionSystem`, and `StiltsSystem` set `CurrentState = GameState.Defeat` on failure conditions.
* **Current Level Loading & Teardown**: `ToolkitHubManager` (`Assets/Scripts/UI/ToolkitHubManager.cs`, lines 162-181) uses `GameObject.Instantiate` and wipes all entities via `EntityManager.DestroyEntity(allEntities)`.
* **21 Playable Slices**: `ToolkitExampleGenerator` (`Assets/Scripts/Editor/ToolkitExampleGenerator.cs`, lines 11-19) defines 21 slice prefabs in `Assets/ToolkitExamples/`.
* **Meta Progression Data**: `GameProgressData` (`Assets/Scripts/GameProgressData.cs`, lines 11-39) manages `TotalGold`, `SwarmLevel`, `IncomeLevel`, and `CurrentSkinIndex` via `PlayerPrefs`.

## 2. Logic Chain
1. *Observation*: Existing simulation systems (14+) halt when `LevelStateComponent.CurrentState != GameState.Playing`.
2. *Observation*: Level completion transitions `CurrentState` to `GameState.Victory`, while level failure transitions to `GameState.Defeat`.
3. *Deduction*: A pure DOTS progression system can monitor `LevelStateComponent` and drive automatic sequence transitions without interrupting presentation layer UI listeners.
4. *Observation*: Standard unmanaged `IComponentData` cannot hold arrays of `Entity` prefabs directly, but `IBufferElementData` can hold prefab entity handles converted during baking via `Baker.GetEntity(authoringPrefab, TransformUsageFlags.Dynamic)`.
5. *Deduction*: `LevelSequenceComponent` (singleton struct) combined with `SlicePrefabBufferElement` (`IBufferElementData` array of 21 prefab entities) provides a zero-allocation, robust DOTS architecture for level sequencing.
6. *Observation*: Instantiating a prefab creates a `LinkedEntityGroup` buffer on the root entity; destroying the root entity automatically cleans up all baked child entities.
7. *Deduction*: By destroying `CurrentSliceInstance` via `EntityCommandBuffer` and querying `SliceEntityTag` for runtime-spawned objects, clean slice entity teardown is achieved without destroying persistent manager entities.

## 3. Caveats
* **SubScene vs Prefab Instantiation**: This architecture assumes slice prefabs are baked and instantiated via DOTS `EntityCommandBuffer.Instantiate(prefabEntity)`. If SubScenes are used in future milestones, `SceneSystem.LoadSceneAsync` can be plugged into the same `LevelTransitionState` state machine.
* **Persistent Manager Singletons**: Any persistent manager entities (e.g. audio event listener, VFX manager) must NOT carry `SliceEntityTag` so they survive level transitions.

## 4. Conclusion
The multi-level progression loader and level sequencing system architecture is fully designed and documented in `d:\Git\Hyper-Casual-Runner\.agents\explorer_m4_1\analysis.md`. The design comprises:
1. `LevelSequenceComponent` & `SlicePrefabBufferElement` for managing 21 slice prefabs.
2. `LevelSequenceAuthoring` for baking 21 slice GameObject prefabs into Entity prefabs.
3. `LevelProgressionSystem` for state-driven level sequencing, ECB teardown, and next slice instantiation.
4. `SliceEntityTag` for clean runtime entity teardown.
5. Extended `GameProgressData.CurrentLevelIndex` for persistent PlayerPrefs level saving.

## 5. Verification Method
1. **Inspect Documentation**: Review `d:\Git\Hyper-Casual-Runner\.agents\explorer_m4_1\analysis.md` for complete specification, state machine flowcharts, and code blueprints.
2. **Code Compilation Test**: Run Unity test runner or compilation check (`Assets/Scripts/Editor/Tests/`) to verify existing test suite continues passing.
3. **Data Structure Verification**: Verify `SlicePrefabBufferElement` uses `[InternalBufferCapacity(21)]` and `LevelSequenceComponent` manages `LevelTransitionState` cleanly.
