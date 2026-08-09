# Handoff Report — Milestone 1: Grid Tile Pathfinder & Maze Collector (12_StackyDash_Grid)

## 1. Observation

### Codebase & Prefab Findings
- **Prefab Inspection (`Assets/ToolkitExamples/12_StackyDash_Grid_Slice.prefab`)**:
  - `PlayerEntity` (Lines 800–929): Capsule mesh with `PlayerAuthoring` (`ForwardSpeed = 10`, `SwerveSpeed = 5`, `MaxSwerveDistance = 4`) and `StackingAuthoring` (`ItemHeightOffset = 0.5`).
  - Grid / Collectibles (Lines 65–789, 930–1400): Cubes positioned at various `(x, y, z)` grid coordinates with `CollectibleAuthoring` (`GoldValue = 1`, `CollisionRadius = 0.8`) and `MathTweenAuthoring`.
  - Level Management (Lines 1089–1198): `LevelManagerAuthoring`, `UIDocument`, `UIManagerSystem`, `AudioManagerAuthoring`, `VFXManagerAuthoring`.
- **Existing Component Patterns**:
  - `StackComponent.cs` (`Assets/Scripts/ECS/Components/StackComponent.cs:5-10`): Contains `CurrentCount` (int), `ItemHeightOffset` (float), `StackItemPrefab` (Entity).
  - `BridgeBuilderComponent.cs` (`Assets/Scripts/ECS/Components/BridgeBuilderComponent.cs:6-18`): `BridgeTilePrefab` (Entity), `DistancePerPlank` (float), `LastPlankZ` (float), `IsBuilding` (bool), plus `GapZoneComponent` with `MinBounds`/`MaxBounds` (float3).
  - `CollisionComponents.cs` (`Assets/Scripts/ECS/Components/CollisionComponents.cs:11-15`): `CollectibleComponent` with `CollisionRadius` (float), `GoldValue` (double).
  - `RunnerComponents.cs` (`Assets/Scripts/ECS/Components/RunnerComponents.cs:5-10`): `PlayerComponent` with `ForwardSpeed`, `SwerveSpeed`, `MaxSwerveDistance`.
- **Existing System Patterns**:
  - `StackVisualSystem.cs` (`Assets/Scripts/ECS/Systems/StackVisualSystem.cs:8-44`): Runs in `SimulationSystemGroup`. Queries `LocalTransform`, `StackComponent`, `CurrentRunStats`. Instantiates `StackItemPrefab` at `yOffset = 1.0f + (index * ItemHeightOffset)`.
  - `PlayerMovementSystem.cs` (`Assets/Scripts/ECS/Systems/PlayerMovementSystem.cs:8-57`): Runs in `SimulationSystemGroup`. Checks `LevelStateComponent.CurrentState == GameState.Playing`. Updates `LocalTransform.Position.z` (forward run) and `Position.x` (swerve input).
  - `BridgeBuilderSystem.cs` (`Assets/Scripts/ECS/Systems/BridgeBuilderSystem.cs:9-87`): Runs in `SimulationSystemGroup`. Checks player bounding against `GapZoneComponent`. Planks placed when distance >= `DistancePerPlank` and `CurrentGold >= 1.0`. Triggers `PlaySoundEventComponent` or sets `GameState.Defeat` if out of planks.

---

## 2. Logic Chain

1. **System & Component Architecture**:
   - StackyDash mechanics require three core concepts in DOTS ECS:
     1. **Grid Floor Tile Collectibles**: Grid tiles spread across a maze layout (`GridTileComponent`) that get collected when player steps over them.
     2. **Player Tile Stacker**: A stack manager (`MazeCollectorComponent`) attached to the player entity tracking collected tiles, visual stack height, and stack item prefabs.
     3. **Gap Pathfinder / Paving**: A path builder (`GridPathfinderComponent`) that automatically detects when player is over gap/water zones (`GapZoneComponent` / `WaterZoneComponent`) and consumes 1 tile per `StepDistance` to pave a bridge tile under player's feet.

2. **Data Structure Definitions**:
   - `GridTileComponent` (`IComponentData`):
     ```csharp
     namespace HyperCasualRunner.ECS.Components
     {
         public struct GridTileComponent : IComponentData
         {
             public float TileSize;        // Cell dimension (default 1.0f)
             public float PickupRadius;    // Collection distance threshold (default 0.8f)
             public bool IsCollected;      // State flag to prevent double-processing
             public int TileValue;         // Number of stack items given (default 1)
         }
     }
     ```
   - `MazeCollectorComponent` (`IComponentData`):
     ```csharp
     namespace HyperCasualRunner.ECS.Components
     {
         public struct MazeCollectorComponent : IComponentData
         {
             public int StackedTiles;         // Current count of tiles in player's stack
             public float TileHeightOffset;   // Vertical height per tile in visual stack (default 0.2f)
             public Entity StackVisualPrefab; // Prefab entity for visual tile stack
             public float CollectionRadius;  // Player pickup radius (default 0.8f)
             public int TotalCollected;       // Lifetime total collected in run
         }
     }
     ```
   - `GridPathfinderComponent` (`IComponentData`):
     ```csharp
     namespace HyperCasualRunner.ECS.Components
     {
         public struct GridPathfinderComponent : IComponentData
         {
             public Entity PathTilePrefab;    // Entity prefab instantiated when paving gaps
             public float StepDistance;      // Distance interval between path tiles (default 1.0f)
             public float3 LastTilePosition;  // Grid/World position of last placed path tile
             public bool IsCrossingGap;       // True if currently over a water/gap zone
             public float PathYHeight;        // Target Y level for paved floor tiles (default 0.0f)
         }
     ```
   - Optional Dynamic Buffer `StackedTileElement` (`IBufferElementData`):
     ```csharp
     namespace HyperCasualRunner.ECS.Components
     {
         public struct StackedTileElement : IBufferElementData
         {
             public Entity VisualEntity;
         }
     }
     ```

3. **Authoring Bakers Design**:
   - `GridTileAuthoring.cs` (`MonoBehaviour` + `Baker<GridTileAuthoring>`): Bakes `GridTileComponent` onto maze ground tile GameObjects using `TransformUsageFlags.Dynamic`.
   - `MazeCollectorAuthoring.cs` (`MonoBehaviour` + `Baker<MazeCollectorAuthoring>`): Bakes `MazeCollectorComponent` and `AddBuffer<StackedTileElement>()` onto `PlayerEntity`. Converts `StackVisualPrefab` GameObject to Entity.
   - `GridPathfinderAuthoring.cs` (`MonoBehaviour` + `Baker<GridPathfinderAuthoring>`): Bakes `GridPathfinderComponent` onto `PlayerEntity`. Converts `PathTilePrefab` GameObject to Entity.

4. **System Execution Design**:
   - `MazeCollectorSystem.cs` (`ISystem` in `SimulationSystemGroup`):
     - `OnCreate`: `state.RequireForUpdate<PlayerComponent>();`
     - `OnUpdate`: Checks `LevelStateComponent.CurrentState == GameState.Playing`.
     - Queries player entity `(transform, collector)`.
     - Queries all `(tileEntity, tileTransform, tileComp)` where `!tileComp.IsCollected`.
     - Distance check: `math.distance(playerPos.xz, tilePos.xz) <= (collector.CollectionRadius + tileComp.PickupRadius)`.
     - Upon pickup:
       - Set `tileComp.IsCollected = true`.
       - `collector.StackedTiles += tileComp.TileValue`.
       - ECB destroys `tileEntity`.
       - ECB creates event entity `PlaySoundEventComponent { SoundToPlay = SoundType.Pickup }` (Hybrid ECS presentation).
   - `GridPathfinderSystem.cs` (`ISystem` in `SimulationSystemGroup`):
     - `OnCreate`: `state.RequireForUpdate<PlayerComponent>();`
     - `OnUpdate`: Checks `LevelStateComponent.CurrentState == GameState.Playing`.
     - Queries player entity `(transform, collector, pathfinder)`.
     - Checks player `(X, Z)` against all `GapZoneComponent` / `WaterZoneComponent` AABB bounds.
     - If inside gap:
       - Set `pathfinder.IsCrossingGap = true`.
       - If `pathfinder.LastTilePosition.z < -9000f`, initialize to `playerPos`.
       - Calculate distance: `math.distance(playerPos.xz, pathfinder.LastTilePosition.xz)`.
       - If `dist >= pathfinder.StepDistance`:
         - If `collector.StackedTiles > 0`:
           - Decrement `collector.StackedTiles--`.
           - Update `pathfinder.LastTilePosition = playerPos`.
           - Instantiate `pathfinder.PathTilePrefab` at `float3(playerPos.x, pathfinder.PathYHeight, playerPos.z)` via ECB.
           - Spawn `PlaySoundEventComponent { SoundToPlay = SoundType.Pickup }`.
         - Else (`collector.StackedTiles == 0` while crossing gap):
           - Trigger defeat: `levelState.CurrentState = GameState.Defeat`.
     - Else: Reset `pathfinder.IsCrossingGap = false`, `pathfinder.LastTilePosition = float3(-9999f)`.

---

## 3. Caveats

1. **Stack Visual Rendering**: The visual representation of tiles stacked under/behind the player can either be managed by a separate visual updater system (similar to `StackVisualSystem`) or via hierarchy parenting (`LocalToParent`). The primary state relies on `MazeCollectorComponent.StackedTiles`.
2. **Existing Prefab Integration**: `12_StackyDash_Grid_Slice.prefab` currently uses standard `CollectibleAuthoring` and `StackingAuthoring`. To fully enable M1 features, `ToolkitExampleGenerator.cs` or the prefab itself will need `GridTileAuthoring`, `MazeCollectorAuthoring`, `GridPathfinderAuthoring`, and `GapZoneAuthoring` attached.
3. **No Code Edits Executed**: As an Explorer agent, no files under `Assets/` were modified during this investigation phase.

---

## 4. Conclusion

The designed Grid Tile Pathfinder and Maze Collector DOTS ECS components, authoring bakers, and systems cleanly extend the existing toolkit conventions (`BridgeBuilderSystem`, `StackVisualSystem`, `CollisionComponents`). 

### Proposed File Map for Implementer:
1. `Assets/Scripts/ECS/Components/GridPathfinderComponents.cs` (Contains `GridTileComponent`, `MazeCollectorComponent`, `GridPathfinderComponent`, `StackedTileElement`)
2. `Assets/Scripts/ECS/Authoring/GridTileAuthoring.cs`
3. `Assets/Scripts/ECS/Authoring/MazeCollectorAuthoring.cs`
4. `Assets/Scripts/ECS/Authoring/GridPathfinderAuthoring.cs`
5. `Assets/Scripts/ECS/Systems/MazeCollectorSystem.cs`
6. `Assets/Scripts/ECS/Systems/GridPathfinderSystem.cs`
7. Update `ToolkitExampleGenerator.cs` & `12_StackyDash_Grid_Slice.prefab` to assign authoring components to tiles, player, and gap zones.

---

## 5. Verification Method

1. **Compilation & Assembly Verification**:
   - Ensure all new scripts are created in `Assets/Scripts/ECS/Components/`, `Assets/Scripts/ECS/Authoring/`, `Assets/Scripts/ECS/Systems/` under `HyperCasualRunner.ECS.*` namespaces.
2. **Editor Menu Verification**:
   - Execute Unity menu command `IdleToolkit/Generate 21 Playable Slices`.
   - Run `IdleToolkit/Run Verification Suite`.
   - Verify `12_StackyDash_Grid_Slice.prefab` passes check and generates cleanly without errors.
