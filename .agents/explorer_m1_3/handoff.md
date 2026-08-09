# Explorer 3 Handoff Report: Milestone 1 DOTS ECS Safety, ECB Guidelines & Architecture

## 1. Observation

### 1.1 Source Code & File Structure
- **PROJECT.md & project-context.md**:
  - `PROJECT.md` (lines 10-14, 25-28): Defines Milestone 1 (Grid Pathfinder & Maze Collector for `12_StackyDash_Grid`). Specifies Simulation as pure DOTS ECS Systems in `SimulationSystemGroup`, Presentation layer as Hybrid ECS (Tags -> Presentation Systems reading `AudioSource`/`ParticleSystem`), and UI as UI Toolkit.
  - `docs/project-context.md` (lines 16-20): Identifies DOTS ECS Systems (Simulation Group, SystemAPI Query, EntityCommandBuffer safety) as a Critical-Lane domain.
- **Existing Collision Handling (`Assets/Scripts/ECS/Systems/CollisionSystem.cs`)**:
  - Lines 8-10: `[UpdateInGroup(typeof(SimulationSystemGroup))] public partial struct CollisionSystem : ISystem`
  - Lines 13-14: `var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);`
  - Lines 35-51: Iterates `SystemAPI.Query<RefRO<LocalTransform>, RefRO<CollectibleComponent>>().WithEntityAccess()` using CPU distance-squared check (`math.distancesq(playerPos, transform.ValueRO.Position)`). On collision, nested `foreach (var runStats in SystemAPI.Query<RefRW<CurrentRunStats>>())` updates gold, and calls `ecb.DestroyEntity(entity)`.
  - Line 66: Synchronously plays back commands on `state.EntityManager`: `ecb.Playback(state.EntityManager); ecb.Dispose();`
  - **Burst compilation status**: `CollisionSystem` currently lacks `[BurstCompile]` attributes.
- **Existing Bridge Builder Handling (`Assets/Scripts/ECS/Systems/BridgeBuilderSystem.cs`)**:
  - Lines 18, 55, 64-65: Uses `Allocator.TempJob` for ECB creation, instantiates tile prefabs, creates audio event tags (`ecb.AddComponent(audioEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup })`), and plays back synchronously.
- **Existing Audio Presentation (`Assets/Scripts/ECS/Systems/AudioManagerSystem.cs`)**:
  - Lines 8-10: `[UpdateInGroup(typeof(PresentationSystemGroup))] public partial class AudioManagerSystem : SystemBase`
  - Lines 27-44: Queries `PlaySoundEventComponent` entities, plays audio clip via `AudioSource`, and destroys event entities via `ecb.DestroyEntity(entity)`.
- **Existing Stack Visual System (`Assets/Scripts/ECS/Systems/StackVisualSystem.cs`)**:
  - Lines 8-10: `[UpdateInGroup(typeof(SimulationSystemGroup))] public partial struct StackVisualSystem : ISystem`
  - Lines 27-37: Spawns visual stack entities under player feet when `StackComponent.CurrentCount` increases.

---

## 2. Logic Chain

1. **System Execution Order & Determinism**:
   - `CollisionSystem`, `PlayerMovementSystem`, `BridgeBuilderSystem`, and `StackVisualSystem` are all annotated with `[UpdateInGroup(typeof(SimulationSystemGroup))]` without explicit `[UpdateBefore]` or `[UpdateAfter]` ordering attributes.
   - For `GridPathfinderSystem` and `MazeCollectorSystem`, execution must occur after player movement updates `LocalTransform`, but before visual updates (`StackVisualSystem`) and win condition checks (`WinConditionSystem`).
   - *Reasoning*: If `GridPathfinderSystem` runs before `PlayerMovementSystem`, tile stepping and path calculations reflect the previous frame's player position, introducing a 1-frame latency in stack tile collection and path validation.

2. **ECB Operations & Memory Safety**:
   - `CollisionSystem` and `BridgeBuilderSystem` create local `EntityCommandBuffer(Allocator.Temp)` instance inside `OnUpdate` and play back synchronously via `ecb.Playback(state.EntityManager)`.
   - Structural changes (such as `DestroyEntity` or `Instantiate`) immediately alter `EntityManager` archetypes and chunk layout.
   - If `MazeCollectorSystem` and `CollisionSystem` were both to query generic `CollectibleComponent` entities, both systems could issue `ecb.DestroyEntity()` for the same entity in the same frame. Playing back double-destroys or operating on destroyed entities results in runtime invalidation errors.
   - *Reasoning*: Grid tiles and maze pickups must be isolated under specific `GridTileComponent` and `MazeCollectorComponent` structs. Furthermore, immediate in-memory state mutation (`RefRW<GridTileComponent>.ValueRW.IsCollected = true`) must be set during query iteration before buffering `ecb.DestroyEntity(entity)`, guaranteeing idempotency across systems.

3. **Burst Compilation & Unmanaged Struct Constraints (Entities 1.0+)**:
   - Pure DOTS ECS requires `ISystem` structs tagged with `[BurstCompile]`.
   - Managed references (such as `class`, `GameObject`, `string`, or `SystemAPI.ManagedAPI`) cause Burst compilation failures.
   - *Reasoning*: `GridPathfinderSystem` and `MazeCollectorSystem` must be written as unmanaged `public partial struct` with `[BurstCompile]` on both struct definition and `OnUpdate`/`OnCreate` methods. All components (`GridTileComponent`, `MazeCollectorComponent`, `GridPathfinderComponent`) must be unmanaged `IComponentData` structs containing blittable types (`int2`, `float3`, `bool`, `Entity`).

4. **Grid Spatial Querying vs Physics Triggers**:
   - `CollisionSystem.cs` uses simple CPU sphere overlap checks (`math.distancesq`).
   - Hyper-casual grid pathfinding (Stacky Dash style) operates on discrete grid cell coordinates `int2(x, z)`.
   - *Reasoning*: A cell-coordinate matching approach (`int2 playerCell = (int2)math.floor((playerPos - gridOrigin) / tileSize)`) is $O(1)$ per frame, deterministic, and avoids Unity Physics trigger overhead and CPU distance loops across non-grid entities.

---

## 3. Caveats

- **No Source Code Edits**: Per prompt directive, no source files under `Assets/` were modified during this investigation.
- **Unity Physics Integration**: The project currently uses custom math distance overlap queries instead of Unity Physics (`PhysicsWorldHistory` / `TriggerEventJob`). All recommendations assume continuation of custom math/grid cell queries. If Unity Physics triggers are introduced in future milestones, `StatefulTriggerEvent` or `ITriggerEventsJob` pattern will be required.

---

## 4. Conclusion & System Architecture Guidelines

### 4.1 Recommended System Architecture for Milestone 1

#### A. Component Definitions (`Assets/Scripts/ECS/Components/GridComponents.cs`)
```csharp
using Unity.Entities;
using Unity.Mathematics;

namespace HyperCasualRunner.ECS.Components
{
    // Grid Tile representation
    public struct GridTileComponent : IComponentData
    {
        public int2 GridPosition;      // X, Z cell coordinate
        public bool IsWalkable;        // Whether tile can be traversed
        public bool HasTileItem;       // Has stackable tile item to pick up
        public bool IsCollected;       // Flag to prevent double collection
        public Entity TilePrefab;      // Prefab reference for visual spawning
    }

    // Grid Pathfinder configuration attached to Player
    public struct GridPathfinderComponent : IComponentData
    {
        public float3 GridOrigin;      // World space position of (0,0) cell
        public float TileSize;         // Width/length of each grid cell (e.g. 1.0f)
        public int2 CurrentCell;       // Player's current grid cell
        public int2 TargetCell;        // Current movement target cell
        public bool IsMoving;          // Active path movement status
    }

    // Maze Collector state attached to Player
    public struct MazeCollectorComponent : IComponentData
    {
        public int CollectedTileCount; // Number of tiles currently stacked under player feet
        public float StackHeightOffset;// Vertical offset per stacked tile
    }
}
```

#### B. Execution Grouping & Dependencies
Both systems must reside in `SimulationSystemGroup` with explicit order attributes:
- `GridPathfinderSystem`: `[UpdateInGroup(typeof(SimulationSystemGroup))]` `[UpdateAfter(typeof(PlayerMovementSystem))]`
- `MazeCollectorSystem`: `[UpdateInGroup(typeof(SimulationSystemGroup))]` `[UpdateAfter(typeof(GridPathfinderSystem))]` `[UpdateBefore(typeof(StackVisualSystem))]` `[UpdateBefore(typeof(WinConditionSystem))]`

#### C. System Implementations & Burst/ECB Safety Pattern

```csharp
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PlayerMovementSystem))]
    public partial struct GridPathfinderSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerComponent>();
            state.RequireForUpdate<GridPathfinderComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float3 playerPos = float3.zero;
            foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerComponent>())
            {
                playerPos = transform.ValueRO.Position;
                break;
            }

            foreach (var pathfinder in SystemAPI.Query<RefRW<GridPathfinderComponent>>())
            {
                // Calculate discrete cell index from world position
                float3 relPos = playerPos - pathfinder.ValueRO.GridOrigin;
                int2 cell = new int2(
                    (int)math.floor((relPos.x + pathfinder.ValueRO.TileSize * 0.5f) / pathfinder.ValueRO.TileSize),
                    (int)math.floor((relPos.z + pathfinder.ValueRO.TileSize * 0.5f) / pathfinder.ValueRO.TileSize)
                );

                pathfinder.ValueRW.CurrentCell = cell;
            }
        }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GridPathfinderSystem))]
    [UpdateBefore(typeof(StackVisualSystem))]
    public partial struct MazeCollectorSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerComponent>();
            state.RequireForUpdate<MazeCollectorComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Acquire ECB for structural mutations
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            float3 playerPos = float3.zero;
            bool playerFound = false;

            foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerComponent>())
            {
                playerPos = transform.ValueRO.Position;
                playerFound = true;
                break;
            }

            if (!playerFound)
            {
                ecb.Dispose();
                return;
            }

            // Query uncollected grid tiles
            foreach (var (transform, tile, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<GridTileComponent>>().WithEntityAccess())
            {
                if (tile.ValueRO.IsCollected || !tile.ValueRO.HasTileItem)
                    continue;

                // Cell overlap check
                float distSq = math.distancesq(playerPos, transform.ValueRO.Position);
                if (distSq < 0.64f) // Threshold matching tile radius (e.g. 0.8m)
                {
                    // 1. Immediately mutate flag in memory to prevent duplicate collection in same frame
                    tile.ValueRW.IsCollected = true;

                    // 2. Increment collector stack count
                    foreach (var collector in SystemAPI.Query<RefRW<MazeCollectorComponent>>())
                    {
                        collector.ValueRW.CollectedTileCount++;
                    }

                    // 3. Trigger Pickup SFX presentation event
                    var soundEntity = ecb.CreateEntity();
                    ecb.AddComponent(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });

                    // 4. Destroy collected tile entity
                    ecb.DestroyEntity(entity);
                }
            }

            // Playback structural changes synchronously on main thread
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
```

### 4.2 Summary of Safety Rules for Milestone 1 Implementers
1. **Unmanaged `ISystem` + `[BurstCompile]`**: Always use `public partial struct SystemName : ISystem` with `[BurstCompile]` attributes. Avoid `SystemAPI.ManagedAPI` and managed objects inside simulation systems.
2. **Explicit System Ordering**: Always specify `[UpdateInGroup(typeof(SimulationSystemGroup))]`, `[UpdateAfter(...)]`, and `[UpdateBefore(...)]` to avoid 1-frame latency or ordering race conditions.
3. **Idempotent In-Memory State Flags**: Mutate component flags (e.g. `tile.ValueRW.IsCollected = true`) immediately in memory prior to recording `ecb.DestroyEntity()`.
4. **Isolate Component Types**: Keep `GridTileComponent` independent from generic `CollectibleComponent` to prevent multi-system double destruction.
5. **Synchronous ECB Cleanup**: If using local `EntityCommandBuffer(Allocator.Temp)`, ensure `ecb.Dispose()` is called on all code paths (including early returns).

---

## 5. Verification Method

To verify these guidelines and architecture implementation during execution:
1. **Compilation & Burst Validation**:
   - Ensure `Assets/Scripts/ECS/Components/GridComponents.cs` and `Assets/Scripts/ECS/Systems/GridPathfinderSystem.cs` compile with zero errors and zero Burst compiler warnings.
2. **Execution Order Inspection**:
   - Open Unity Systems Window (`Window > DOTS > Systems`) while running `12_StackyDash_Grid_Slice.prefab`.
   - Verify `GridPathfinderSystem` executes after `PlayerMovementSystem` and `MazeCollectorSystem` executes before `StackVisualSystem`.
3. **ECB & Memory Safety Check**:
   - Run Unity Editor with `Leak Detection` set to Full. Verify zero memory leaks or `EntityDoesNotExistException` during tile collection and path traversal.
