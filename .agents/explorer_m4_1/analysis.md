# Milestone 4 - Multi-Level Progression Loader & Level Sequencing Architecture

## 1. Executive Summary
This analysis details the architectural design for the **Multi-Level Progression Loader and Level Sequencing System** in the Hyper-Casual Runner Toolkit. The objective is to enable seamless sequencing, clean teardown, and instantiation across all 21 playable slice prefabs (`1_SubwaySurfers_Meta_Slice.prefab` through `21_WeaponMaster_Shooter_Slice.prefab`) using pure Unity Entities 1.0+ (DOTS) mechanics, while integrating with the existing UI Toolkit presentation layer and PlayerPrefs meta-progression persistence.

---

## 2. Existing System Inspection (`Assets/Scripts/ECS/` & `Assets/Scripts/UI/`)

### 2.1 Game State Architecture
* **`LevelStateComponent`** (`Assets/Scripts/ECS/Components/LevelStateComponent.cs`):
  ```csharp
  public enum GameState { Pregame, Playing, Victory, Defeat }

  public struct LevelStateComponent : IComponentData
  {
      public GameState CurrentState;
  }
  ```
* **`LevelManagerAuthoring`** (`Assets/Scripts/ECS/Authoring/EndZoneAuthoring.cs`):
  Bakes `LevelStateComponent` onto an entity, initializing `CurrentState = GameState.Pregame`.

### 2.2 System Gating & State Transitions
* **Simulation Gating**: 14+ core simulation systems (e.g. `PlayerMovementSystem`, `LaneSystem`, `BridgeBuilderSystem`, `CoinPhysicsSystem`, `GridPathfinderSystem`, `SnakeCollisionSystem`) check `LevelStateComponent.CurrentState == GameState.Playing` before executing updates. If the state is `Pregame`, `Victory`, or `Defeat`, game simulation is paused.
* **Victory Conditions**: `WinConditionSystem` checks proximity of entities with `PlayerComponent` to entities with `EndZoneComponent`. When `distanceSq < TriggerRadius^2`, `CurrentState` is transitioned to `GameState.Victory`.
* **Defeat Conditions**: Multiple hazard systems (`BridgeBuilderSystem`, `GridPathfinderSystem`, `LaneSystem`, `SnakeCollisionSystem`, `StiltsSystem`) switch `CurrentState` to `GameState.Defeat` when loss triggers fire (e.g., falling in gap, obstacle collision, head death).
* **UI State Sync**: `UIManagerSystem` (MonoBehaviour UI Toolkit controller) reads `LevelStateComponent` every frame to present Pregame, Defeat, or Victory panels and handle user inputs (e.g., "TAP TO START", "RETURN TO HUB").

### 2.3 Existing Level Loading Limitations
* Currently, `ToolkitHubManager` loads slice prefabs manually via `GameObject.Instantiate(prefab)` in MonoBehaviour presentation space.
* Teardown currently relies on calling `World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(allEntities)` to wipe all entities when returning to the Hub.
* There is currently no pure DOTS multi-level progression manager that holds entity prefabs of all 21 slices, handles slice-to-slice entity transitions, and updates player level index dynamically.

---

## 3. Data Structures & Authoring Design

To achieve pure DOTS level sequencing without destroying persistent manager entities, we introduce three core data structures and an authoring component:

### 3.1 `LevelTransitionState` Enum
Defines the state of the level loader state machine:
```csharp
namespace HyperCasualRunner.ECS.Components
{
    public enum LevelTransitionState
    {
        Idle,            // Level active / playing or waiting in pregame
        PendingNext,     // Victory achieved, awaiting UI sequence or delay
        TeardownCurrent, // ECB destroying current slice entities
        SpawningNext,    // ECB instantiating next slice prefab entity
        Failed           // Defeat state, awaiting restart or retry
    }
}
```

### 3.2 `SlicePrefabBufferElement` (`IBufferElementData`)
Dynamic buffer element storing entity prefab handles for all 21 slices:
```csharp
using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    [InternalBufferCapacity(21)]
    public struct SlicePrefabBufferElement : IBufferElementData
    {
        public Entity PrefabEntity;
    }
}
```

### 3.3 `LevelSequenceComponent` (`IComponentData`)
Singleton component storing progression state and sequence parameters:
```csharp
using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public struct LevelSequenceComponent : IComponentData
    {
        public int CurrentLevelIndex;       // 0 to MaxLevels - 1
        public int MaxLevels;               // Total slice prefabs (21)
        public LevelTransitionState TransitionState;
        public Entity CurrentSliceInstance; // Entity handle to root slice instance
        public bool LoopSequencing;         // True: wrap around after level 21
        public float AutoTransitionTimer;   // Optional timer before auto-transitioning
    }

    public struct SliceEntityTag : IComponentData {}
}
```

### 3.4 `LevelSequenceAuthoring` (`MonoBehaviour` + Baker)
Authoring script attached to `LevelManager` in master scene/hub:
```csharp
using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class LevelSequenceAuthoring : MonoBehaviour
    {
        public GameObject[] SlicePrefabs; // Array of 21 GameObject prefabs
        public bool LoopSequencing = true;

        class Baker : Baker<LevelSequenceAuthoring>
        {
            public override void Bake(LevelSequenceAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                int prefabCount = authoring.SlicePrefabs != null ? authoring.SlicePrefabs.Length : 0;
                
                AddComponent(entity, new LevelSequenceComponent
                {
                    CurrentLevelIndex = GameProgressData.CurrentLevelIndex,
                    MaxLevels = prefabCount,
                    TransitionState = LevelTransitionState.Idle,
                    CurrentSliceInstance = Entity.Null,
                    LoopSequencing = authoring.LoopSequencing,
                    AutoTransitionTimer = 0f
                });

                var buffer = AddBuffer<SlicePrefabBufferElement>(entity);
                if (authoring.SlicePrefabs != null)
                {
                    foreach (var slicePrefab in authoring.SlicePrefabs)
                    {
                        if (slicePrefab != null)
                        {
                            // Register prefab dependency & get DOTS Entity prefab handle
                            DependsOn(slicePrefab);
                            Entity prefabEntity = GetEntity(slicePrefab, TransformUsageFlags.Dynamic);
                            buffer.Add(new SlicePrefabBufferElement { PrefabEntity = prefabEntity });
                        }
                    }
                }
            }
        }
    }
}
```

### 3.5 Meta-Progression Extension (`GameProgressData.cs`)
Extend `GameProgressData` to store `CurrentLevelIndex`:
```csharp
public static int CurrentLevelIndex
{
    get => PlayerPrefs.GetInt("HCR_CurrentLevelIndex", 0);
    set { PlayerPrefs.SetInt("HCR_CurrentLevelIndex", value); PlayerPrefs.Save(); }
}
```

---

## 4. `LevelProgressionSystem` Architecture & Lifecycle

`LevelProgressionSystem` is a pure DOTS system executing in `SimulationSystemGroup`.

```
           +--------------------------------------------------+
           |                      IDLE                        |
           |   (LevelStateComponent.CurrentState == Playing)  |
           +------------------------+-------------------------+
                                    |
            +-----------------------+-----------------------+
            |                                               |
  [GameState == Victory]                           [GameState == Defeat]
            |                                               |
            v                                               v
+-----------------------+                       +-----------------------+
|  PENDING_NEXT LEVEL   |                       |        FAILED         |
|  CurrentLevelIndex++  |                       | Wait for Retry Input  |
+-----------+-----------+                       +-----------+-----------+
            |                                               |
            +-----------------------+-----------------------+
                                    | (Trigger Teardown)
                                    v
                       +-------------------------+
                       |    TEARDOWN_CURRENT     |
                       |  Destroy Slice Entities |
                       +------------+------------+
                                    |
                                    v
                       +-------------------------+
                       |      SPAWNING_NEXT      |
                       |  Instantiate Next Prefab|
                       +------------+------------+
                                    |
                                    v
                       +-------------------------+
                       |          IDLE           |
                       | Set GameState.Pregame   |
                       +-------------------------+
```

### 4.1 State Machine Operations in `OnUpdate`

1. **`LevelTransitionState.Idle`**:
   * Inspects `LevelStateComponent.CurrentState`.
   * If `GameState.Victory`: Sets `TransitionState = LevelTransitionState.PendingNext`.
   * If `GameState.Defeat`: Sets `TransitionState = LevelTransitionState.Failed`.

2. **`LevelTransitionState.PendingNext`**:
   * Increments level index:
     `sequence.ValueRW.CurrentLevelIndex = (sequence.ValueRO.CurrentLevelIndex + 1) % sequence.ValueRO.MaxLevels`.
   * Updates `GameProgressData.CurrentLevelIndex`.
   * Sets `sequence.ValueRW.TransitionState = LevelTransitionState.TeardownCurrent`.

3. **`LevelTransitionState.TeardownCurrent`**:
   * Requests an `EntityCommandBuffer` from `EndSimulationEntityCommandBufferSystem.Singleton`.
   * **Root Cleanup**: If `sequence.ValueRO.CurrentSliceInstance != Entity.Null` and exists, executes `ecb.DestroyEntity(sequence.ValueRO.CurrentSliceInstance)`.
     *(Note: In Entities 1.0+, instantiating a prefab creates a `LinkedEntityGroup` buffer on the root entity. Destroying the root entity automatically destroys all child entities in its baked hierarchy!)*
   * **Loose Entity Cleanup**: Executes `ecb.DestroyEntity(sliceEntityQuery)` where `sliceEntityQuery` selects entities with `SliceEntityTag` or level runtime items (coins, projectiles, particle events) to prevent entity leakage.
   * Sets `sequence.ValueRW.TransitionState = LevelTransitionState.SpawningNext`.

4. **`LevelTransitionState.SpawningNext`**:
   * Requests `EntityCommandBuffer`.
   * Reads `SlicePrefabBufferElement` at `CurrentLevelIndex`.
   * Instantiates prefab:
     `Entity newInstance = ecb.Instantiate(buffer[currentIndex].PrefabEntity);`.
   * Saves `sequence.ValueRW.CurrentSliceInstance = newInstance`.
   * Resets `LevelStateComponent.CurrentState = GameState.Pregame`.
   * Sets `sequence.ValueRW.TransitionState = LevelTransitionState.Idle`.

---

## 5. Clean Slice Entity Teardown Mechanics

A major risk in multi-level runner progression is entity leakage (e.g. orphan coins, projectiles, or particles remaining from previous levels).

### 5.1 Hierarchical Cleanup (`LinkedEntityGroup`)
When a GameObject prefab containing authoring scripts is baked, Unity automatically builds a `LinkedEntityGroup` dynamic buffer on the primary root entity. When `ecb.DestroyEntity(rootInstance)` is called, the entity manager recursively destroys every child entity listed in `LinkedEntityGroup`.

### 5.2 Dynamic Runtime Entity Tagging (`SliceEntityTag`)
For entities created dynamically at runtime during gameplay (e.g., coins spawned by `CoinSpawnerSystem`, projectiles created by `ShooterSystem`, sound event tags created by audio system):
1. Spawn systems attach `SliceEntityTag` to created runtime entities.
2. `LevelProgressionSystem` executes a query-based cleanup:
   ```csharp
   var sliceQuery = SystemAPI.QueryBuilder().WithAll<SliceEntityTag>().Build();
   ecb.DestroyEntity(sliceQuery, EntityQueryCaptureMode.AtPlayback);
   ```
3. Persistent singleton entities (such as `LevelSequenceComponent`, `LevelStateComponent`, `VFXManagerComponent`, `AudioManagerComponent`) do NOT have `SliceEntityTag` and remain safely intact across level transitions.

---

## 6. System Design Specification Code Blueprints

### 6.1 `LevelProgressionSystem.cs` Draft Specification
```csharp
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct LevelProgressionSystem : ISystem
    {
        private EntityQuery _sliceQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LevelSequenceComponent>();
            state.RequireForUpdate<LevelStateComponent>();
            _sliceQuery = state.GetEntityQuery(ComponentType.ReadOnly<SliceEntityTag>());
        }

        public void OnUpdate(ref SystemState state)
        {
            var sequenceRW = SystemAPI.GetSingletonRW<LevelSequenceComponent>();
            var levelStateRW = SystemAPI.GetSingletonRW<LevelStateComponent>();
            var buffer = SystemAPI.GetSingletonBuffer<SlicePrefabBufferElement>();

            ref var sequence = ref sequenceRW.ValueRW;
            ref var levelState = ref levelStateRW.ValueRW;

            switch (sequence.TransitionState)
            {
                case LevelTransitionState.Idle:
                    if (levelState.CurrentState == GameState.Victory)
                    {
                        sequence.TransitionState = LevelTransitionState.PendingNext;
                    }
                    break;

                case LevelTransitionState.PendingNext:
                    if (sequence.MaxLevels > 0)
                    {
                        sequence.CurrentLevelIndex = (sequence.CurrentLevelIndex + 1) % sequence.MaxLevels;
                        GameProgressData.CurrentLevelIndex = sequence.CurrentLevelIndex;
                    }
                    sequence.TransitionState = LevelTransitionState.TeardownCurrent;
                    break;

                case LevelTransitionState.TeardownCurrent:
                    var ecbSystemTeardown = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
                    var ecbTeardown = ecbSystemTeardown.CreateCommandBuffer(state.WorldUnmanaged);

                    if (sequence.CurrentSliceInstance != Entity.Null)
                    {
                        ecbTeardown.DestroyEntity(sequence.CurrentSliceInstance);
                        sequence.CurrentSliceInstance = Entity.Null;
                    }

                    if (!_sliceQuery.IsEmptyIgnoreFilter)
                    {
                        ecbTeardown.DestroyEntity(_sliceQuery, EntityQueryCaptureMode.AtPlayback);
                    }

                    sequence.TransitionState = LevelTransitionState.SpawningNext;
                    break;

                case LevelTransitionState.SpawningNext:
                    if (buffer.Length > 0 && sequence.CurrentLevelIndex < buffer.Length)
                    {
                        var ecbSystemSpawn = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
                        var ecbSpawn = ecbSystemSpawn.CreateCommandBuffer(state.WorldUnmanaged);

                        Entity prefab = buffer[sequence.CurrentLevelIndex].PrefabEntity;
                        Entity newInstance = ecbSpawn.Instantiate(prefab);

                        sequence.CurrentSliceInstance = newInstance;
                        levelState.CurrentState = GameState.Pregame;
                    }
                    sequence.TransitionState = LevelTransitionState.Idle;
                    break;

                case LevelTransitionState.Failed:
                    // Defeat state handled by UI restart button pushing transition to TeardownCurrent
                    break;
            }
        }
    }
}
```

---

## 7. Verification Plan & Test Matrix

To ensure stability during implementation:
1. **Compilation & Assembly Verification**: Validate that `LevelSequenceComponent`, `SlicePrefabBufferElement`, `LevelSequenceAuthoring`, and `LevelProgressionSystem` compile cleanly without namespace collisions or unmanaged memory violations.
2. **Buffer Capacity Check**: Verify that 21 prefabs fit in `SlicePrefabBufferElement` dynamic buffer without allocation overhead (`[InternalBufferCapacity(21)]`).
3. **Teardown Memory Audit**: Test sequential slice loading from slice 0 to slice 20 and verify entity counts before and after teardown using `EntityManager.Debug.GetEntityCount()`.

---
*Report compiled by Explorer 1 for Milestone 4.*
