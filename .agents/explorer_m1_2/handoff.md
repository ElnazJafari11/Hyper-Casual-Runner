# Handoff Report: Explorer 2 - Grid Tile Pathfinder & Maze Collector (12_StackyDash_Grid)

## 1. Observation

Direct code and prefab observations from the project codebase:

1. **Slice Prefab Current State (`Assets/ToolkitExamples/12_StackyDash_Grid_Slice.prefab`)**:
   - Inspected `12_StackyDash_Grid_Slice.prefab` (Lines 1–200).
   - Currently contains a generic `PlayerEntity` with `PlayerAuthoring` and `StackingAuthoring`, along with generic collectible cubes (`CollectibleAuthoring` and `MathTweenAuthoring`).
   - Reason: `ToolkitExampleGenerator.cs` line 162 uses `else if (prefabName.Contains("Stack") || prefabName.Contains("Snake"))`, which inadvertently matched `"12_StackyDash_Grid"` due to `"Stack"` substring matching.

2. **Prefab Generation System (`Assets/Scripts/Editor/ToolkitExampleGenerator.cs`)**:
   - `ToolkitExampleGenerator.cs` (Lines 10–18): `ExampleNames` array includes `"12_StackyDash_Grid"`.
   - Lines 32–383: `GenerateExamples()` constructs root GameObject `<Name>_Slice`, ground plane, player entity, level manager, audio/VFX managers, and end zone before saving via `PrefabUtility.SaveAsPrefabAsset`.
   - Lines 440–498: `RunVerificationSuite()` verifies slice prefabs on disk and checks slice-specific authoring components on `PlayerEntity` or sub-objects (currently checks `SubwaySurfers`, `Shooter`, `Planks`, `Vertical`, `Skip`, but lacks a specific clause for `12_StackyDash_Grid`).

3. **Existing Authoring & Hybrid ECS Architecture**:
   - `Assets/Scripts/ECS/Authoring/PlayerAuthoring.cs`: Bakes `PlayerComponent`, `InputComponent`, `AirborneComponent`, `CurrentRunStats`. Uses `Baker<T>` pattern with `GetEntity(TransformUsageFlags.Dynamic)`.
   - `Assets/Scripts/ECS/Authoring/StackingAuthoring.cs`: Bakes `StackComponent` with `GetEntity(authoring.StackItemPrefab, TransformUsageFlags.Dynamic)`.
   - `Assets/Scripts/ECS/Authoring/BridgeBuilderAuthoring.cs`: Bakes `BridgeBuilderComponent` holding bridge tile prefab entity reference.
   - `Assets/Scripts/ECS/Authoring/AudioManagerAuthoring.cs` & `VFXManagerAuthoring.cs`: Managed components (`IComponentData` with `AudioClip` / `GameObject`) used by presentation systems (`AudioManagerSystem`, `VFXManagerSystem`) running in `PresentationSystemGroup`.
   - `Assets/Scripts/ECS/Components/PlaySoundEventComponent.cs` & `DestroyEventComponent.cs`: Tag components spawned in simulation group to trigger presentation audio/VFX.

---

## 2. Logic Chain

1. **Problem Identification**:
   - `12_StackyDash_Grid_Slice.prefab` requires dedicated Grid Tile Pathfinder & Maze Collector mechanics (`MazeCollectorAuthoring`, `GridTileAuthoring`, `GridMapAuthoring`).
   - Because `ToolkitExampleGenerator.cs` matches `12_StackyDash_Grid` to the generic `Stack` condition, `12_StackyDash_Grid_Slice.prefab` does not contain maze grid tiles, gap cells, or maze collector components.

2. **Generator & Prefab Integration Logic**:
   - A dedicated branch must be added to `ToolkitExampleGenerator.cs` before `prefabName.Contains("Stack")`:
     `else if (prefabName.Contains("StackyDash") || prefabName.Contains("Grid"))`.
   - The generator for `12_StackyDash_Grid` must spawn:
     a) `PlayerEntity` with `PlayerAuthoring`, `SwerveInputSystem` / Grid input reader, and `MazeCollectorAuthoring`.
     b) A Grid layout GameObject (`GridMap`) with `GridMapAuthoring` (specifying grid dimensions X x Z, cell size e.g. 1.0f, and origin float3).
     c) A matrix of Grid Cell GameObjects with `GridTileAuthoring`:
        - `Pickup` tiles (cubes with collectible visual, value=1).
        - `Gap` tiles (recessed planes/cubes representing water/pit gaps needing tiles).
        - `Wall` tiles (border obstacle blocks preventing movement).
     d) An inactive `StackTileTemplate` prefab GameObject (child of `LevelManager` or root) passed into `MazeCollectorAuthoring.TilePrefab`.
     e) `LevelManager` with `LevelManagerAuthoring`, `UIDocument`, `UIManagerSystem`, `AudioManagerAuthoring`, `VFXManagerAuthoring`, `MockAdsManager`.
     f) `EndZone` with `EndZoneAuthoring` at the exit cell of the maze.

3. **Baking Strategy Logic**:
   - `MazeCollectorAuthoring`:
     - Bakes `MazeCollectorComponent`: `CurrentTileCount` (int), `TileHeightOffset` (float), `TilePrefab` (Entity from `GetEntity(authoring.TilePrefab)`), `MoveSpeed` (float), `IsSliding` (bool), `TargetGridPos` (int2), `SlideDirection` (int2).
     - Bakes `GridPathfinderComponent`: `CurrentGridPos` (int2), `GridOrigin` (float3), `CellSize` (float2).
     - Bakes `DynamicBuffer<StackedTileElement>`: Holds stacked visual tile entities attached to player.
   - `GridTileAuthoring`:
     - Bakes `GridTileComponent`: `TileType` (enum: Pickup, Gap, PlacedBridge, Wall, Empty), `GridPosition` (int2), `IsCollected` (bool), `IsFilled` (bool), `TileValue` (int).
   - `GridMapAuthoring`:
     - Bakes `GridMapComponent`: `Width` (int), `Height` (int), `CellSize` (float), `Origin` (float3).
     - Bakes `DynamicBuffer<GridCellElement>`: Flattened grid index buffer (`index = z * Width + x`) referencing cell tile entities for O(1) runtime lookup during sliding path calculation.

4. **Verification Suite Logic**:
   - `RunVerificationSuite()` in `ToolkitExampleGenerator.cs` must include a specific verification block for `StackyDash`:
     ```csharp
     else if (sliceName.Contains("StackyDash"))
     {
         var m = player.GetComponent<MazeCollectorAuthoring>();
         report += $"       - MazeCollectorAuthoring verified: TileHeightOffset={m?.TileHeightOffset}, StartingTiles={m?.StartingTiles}\n";
     }
     ```
   - This ensures `RunVerificationSuite` validates `12_StackyDash_Grid_Slice.prefab` as a PASS with non-null component attributes.

---

## 3. Caveats

1. **Read-Only Investigation Scope**:
   - No source code or prefab files in `Assets/` were modified during this investigation. Implementation will be executed by the designated Implementer agent.
2. **Editor Execution Dependency**:
   - Prefab generation and `RunVerificationSuite()` require running Unity Editor or calling the editor menu methods (`IdleToolkit/Generate 21 Playable Slices` / `IdleToolkit/Run Verification Suite`).
3. **Grid Coordinate Conventions**:
   - Assumes Grid X maps to World X (width), Grid Z maps to World Z (length/depth), with Y as height offset. Cell coordinates use zero-indexed `int2(x, z)`.

---

## 4. Conclusion

To achieve complete validation for `12_StackyDash_Grid_Slice.prefab` and pass `RunVerificationSuite`:

1. **Modify `ToolkitExampleGenerator.cs`**:
   - Insert `else if (prefabName.Contains("StackyDash"))` before `Contains("Stack")`.
   - Build a structured maze grid in the slice generator featuring:
     - 10x15 grid of cells (Pickup tiles, Gap bridge areas, Wall borders).
     - `PlayerEntity` configured with `MazeCollectorAuthoring` and `TilePrefab` template.
     - `GridMap` with `GridMapAuthoring`.
   - Update `RunVerificationSuite()` to inspect `MazeCollectorAuthoring` on `12_StackyDash_Grid_Slice.prefab`.

2. **Authoring & Baking Component Requirements**:
   - `MazeCollectorAuthoring`: Configures starting tiles (0), tile height offset (0.2f), move speed (15f), and tile prefab reference. Bakes `MazeCollectorComponent`, `GridPathfinderComponent`, and `DynamicBuffer<StackedTileElement>`.
   - `GridTileAuthoring`: Configures tile type (`Pickup`, `Gap`, `Wall`), grid position `int2(x, z)`, and collection status. Bakes `GridTileComponent`.
   - `GridMapAuthoring`: Configures grid bounds and cell size. Bakes `GridMapComponent` and cell lookup buffer.

3. **Edge Case Resolution Specifications**:

   - **Grid Boundary Navigation**:
     - `GridPathfinderSystem` checks cell bounds (`0 <= x < Width` and `0 <= z < Height`).
     - Sliding raycast stops at grid bounds or wall cells (`TileType == Wall`).
     - Mid-slide swipe inputs are ignored while `IsSliding == true` to preserve grid alignment.

   - **Tile Depletion Handling**:
     - Player entering a `Gap` cell with `CurrentTileCount > 0` decrements tile count by 1, marks cell `PlacedBridge`, instantiates bridge tile at `gapPos`, and destroys top stacked tile entity.
     - Player entering a `Gap` cell with `CurrentTileCount == 0` stops slide immediately at gap boundary. If pushed onto gap, triggers defeat state in `LevelStateComponent`.
     - Visual stack height interpolates smoothly via `math.lerp` to avoid visual popping.

   - **Grid Tile Spawning & Despawning**:
     - Fast multi-cell slide movement checks all grid cells along line segment `[previousGridPos, currentGridPos]`.
     - For each `Pickup` cell crossed, `IsCollected = true`, `CurrentTileCount += TileValue`, tile renderer/entity disabled or destroyed, and audio tag entity created.

   - **Hybrid Audio / VFX Tag Triggering**:
     - Tile Pickup: Spawns `PlaySoundEventComponent(SoundType.Pickup)` and `DestroyEventComponent` on pickup entity for particle burst.
     - Tile Placement: Spawns placement sound/VFX event at bridge gap coordinates.
     - `AudioManagerSystem` processes audio tags once per frame and destroys tag entities in `PresentationSystemGroup` to prevent clip distortion and memory leaks.

---

## 5. Verification Method

To verify the strategy once implemented by the implementer:

1. **Prefab Inspection**:
   - Inspect `Assets/ToolkitExamples/12_StackyDash_Grid_Slice.prefab` in Unity Editor or view YAML structure.
   - Verify `PlayerEntity` possesses `PlayerAuthoring` and `MazeCollectorAuthoring`.
   - Verify grid child objects possess `GridTileAuthoring` with `GridTileType` set.

2. **Automated Verification Suite Execution**:
   - Execute menu item `IdleToolkit/Run Verification Suite` in Unity Editor.
   - Open `verification_report.txt` at project root.
   - Verify `12_StackyDash_Grid` reports `[PASS]` and displays non-null `MazeCollectorAuthoring` properties.

3. **Compilation & Unit Test Verification**:
   - Run NUnit tests via `ToolkitGeneratorTests.cs` to ensure generator executes without exceptions.
