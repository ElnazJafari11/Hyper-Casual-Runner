# Codebase Exploration & Technical Design Analysis Report

**Project**: Hyper-Casual Runner Toolkit (`d:/Git/Hyper-Casual-Runner`)  
**Author**: Explorer Agent (`teamwork_preview_explorer_1`)  
**Date**: 2026-07-22  
**Status**: Completed  

---

## 1. Executive Summary

This report provides a comprehensive, read-only architectural investigation of the Hyper-Casual Runner Toolkit codebase across three primary target requirement areas:
1. **UI Toolkit Level Select Screen** (`Assets/UI/LevelSelect.uxml`, `Assets/Scripts/UI/UIManagerSystem.cs`, `GameProgressData.cs`, and DOTS level progression mechanics).
2. **Advanced Obstacle Variants** (DOTS ECS component, authoring, and system patterns in `Assets/Scripts/ECS/` for obstacles and hazards).
3. **Cosmetics Shop Extension** (`Assets/UI/IdleGameHUD.uxml`, `IdleUIManagerSystem.cs`, `PersistentPlayerStats`, and PrestigeCurrency transaction logic).

Additionally, this report documents the available build, compilation, and automated test execution tooling (`unityMCP`, Unity CLI batchmode testing).

---

## 2. Requirement 1: UI Toolkit Level Select Screen & Progression Mechanics

### 2.1 File Map & References

| File Path | Namespace | Class / Type | Role / Key Members |
| :--- | :--- | :--- | :--- |
| `Assets/UI/LevelSelect.uxml` | N/A (UXML) | Visual Element Hierarchy | Overlay modal containing `Header` (`title-label`, `CloseBtn`), `ScrollView` (`grid-scroll-view`), and `VisualElement` (`LevelGridContainer`). |
| `Assets/UI/LevelCardItem.uxml` | N/A (UXML) | Visual Element Template | Template button `CardButton` with `LevelNumberLabel`, `StarsContainer` (3 stars), and `StatusLabel`. |
| `Assets/UI/LevelSelect.uss` | N/A (USS) | Style Rules | Styling for overlay, modal, headers, grid container (`.level-grid-container`), level cards (`.level-card-button`), status states (`.card-locked`, `.card-playing`), and stars (`.star-active`, `.star-inactive`). |
| `Assets/Scripts/UI/UIManagerSystem.cs` | `HyperCasualRunner.UI` | `public class UIManagerSystem : MonoBehaviour` | Primary UI controller attached to `UIDocument`. Queries UXML elements, binds callbacks, builds level cards in `BuildLevelGrid()`, handles level selection in `SelectLevel(int levelIndex)`, and communicates state changes to DOTS `LevelSequenceComponent`. |
| `Assets/Scripts/GameProgressData.cs` | `HyperCasualRunner` | `public static class GameProgressData` | Static persistence manager wrapping `PlayerPrefs`. Key properties: `TotalGold`, `CurrentLevelIndex`, `UnlockedLevelIndex`, `GetLevelStars(int)`, `SetLevelStars(int, int)`, `SaveLevelCompletion(int, int, int)`. |
| `Assets/Scripts/ECS/Components/LevelProgressionComponents.cs` | `HyperCasualRunner.ECS.Components` | `LevelSequenceComponent`, `SlicePrefabBufferElement`, `LevelTransitionState`, `SliceEntityTag` | Core ECS progression data. `LevelSequenceComponent` manages `CurrentLevelIndex`, `MaxLevels`, `UnlockedLevelIndex`, `TransitionState`, and `CurrentSliceInstance`. `SlicePrefabBufferElement` dynamic buffer holds level slice prefabs. |
| `Assets/Scripts/ECS/Systems/LevelProgressionSystem.cs` | `HyperCasualRunner.ECS.Systems` | `public partial struct LevelProgressionSystem : ISystem` | Unmanaged ECS system handling level slice transitions (`Idle` -> `PendingNext` -> `TeardownCurrent` -> `SpawningNext` -> `Idle`). |

### 2.2 Existing UI Toolkit & Scene Connection Patterns

- **UIDocument Integration**: `UIManagerSystem` inherits from `MonoBehaviour` and requires `UIDocument`. In `Awake()` / `InitializeUI()`, it retrieves `UIDocument.rootVisualElement` and queries elements via `.Q<VisualElement>("ElementName")` or `.Q<Button>("ButtonName")`.
- **Programmatic Fallback**: `UIManagerSystem` includes a `BuildFallbackUI()` method that programmatically constructs UI elements if UXML elements are missing or if no UXML asset is bound to `UIDocument`.
- **Level Grid Construction**: `BuildLevelGrid()` loops over levels (0 to 20), queries `GameProgressData.UnlockedLevelIndex`, `GameProgressData.CurrentLevelIndex`, and `GameProgressData.GetLevelStars(i)`. It constructs level cards dynamically, applying appropriate USS classes (`level-card-button`, `card-locked`, `card-playing`).
- **Bridge to DOTS ECS**: When a user selects an unlocked level, `UIManagerSystem.SelectLevel(int levelIndex)`:
  1. Sets `GameProgressData.CurrentLevelIndex = levelIndex`.
  2. Queries `World.DefaultGameObjectInjectionWorld.EntityManager` for the `LevelSequenceComponent` singleton entity.
  3. Updates `seq.CurrentLevelIndex = levelIndex` and sets `seq.TransitionState = LevelTransitionState.TeardownCurrent`.
  4. `LevelProgressionSystem` intercepts `TeardownCurrent`, destroys all entities tagged with `SliceEntityTag` and `CurrentSliceInstance`, and instantiates `SlicePrefabBufferElement[levelIndex].PrefabEntity` in `SpawningNext`.

### 2.3 Technical Design for M1 Refinement

1. **UXML Template Instantiation**:
   - Enhance `UIManagerSystem.BuildLevelGrid()` to clone `LevelCardItem.uxml` (`VisualTreeAsset.Instantiate()`) when available, binding elements (`CardButton`, `LevelNumberLabel`, `StarsContainer`, `StatusLabel`) directly to support custom styling while retaining fallback button creation.
2. **Lock State & Selection Enforcement**:
   - Enforce check `i <= GameProgressData.UnlockedLevelIndex`. Locked cards register no click handler and render with `.card-locked` opacity.
3. **Star Rendering & Completion Feedback**:
   - Synchronize star count (0 to 3) with `GameProgressData.GetLevelStars(levelIndex)` and update star CSS classes (`star-active` / `star-inactive`).

---

## 3. Requirement 2: Advanced Obstacle Variants

### 3.1 Existing DOTS ECS Hazard Architecture

Inspection of `Assets/Scripts/ECS/Authoring/` and `Assets/Scripts/ECS/Systems/` reveals the established project conventions for obstacles and hazards:
- **Authoring Pattern**: MonoBehaviour class in `HyperCasualRunner.ECS.Authoring` with nested `Baker<TAuthoring>` class. Uses `GetEntity(TransformUsageFlags.Dynamic)` and adds structs implementing `IComponentData`.
- **Component Pattern**: Simple unmanaged structs in `HyperCasualRunner.ECS.Components`. Obstacle hitboxes use `ObstacleComponent` (`CollisionRadius`, `DamageAmount`).
- **Movement / Motion Pattern**: Demonstrated in `MathTweenSystem.cs` and `MathTweenComponent.cs` (`TweenProperty`, `Amplitude`, `Speed`, `BaseValue`), updating `LocalTransform` positions/rotations within `SimulationSystemGroup` or `PresentationSystemGroup`.
- **Collision Pattern**: Processed in `CollisionSystem.cs` via distance checks (`math.distancesq`) between player `LocalTransform` and obstacle `LocalTransform`.
- **Hybrid VFX/SFX Presentation Pattern**: Systems create transient tag entities (`PlaySoundEventComponent`, `DestroyEventComponent`) which presentation systems (`AudioManagerSystem`, `VFXManagerSystem`) consume in `PresentationSystemGroup` to play Unity `AudioSource` clips or spawn `ParticleSystem` prefabs before destroying the tag entity.

### 3.2 Detailed Technical Design for 3 Advanced Obstacle Variants

#### Obstacle 1: Moving Walls (`MovingWall`)
- **Behavior**: Horizontal or vertical oscillating wall segment along a configurable axis vector.
- **Component Struct**:
  ```csharp
  namespace HyperCasualRunner.ECS.Components
  {
      public struct MovingWallComponent : IComponentData
      {
          public float3 MovementAxis;  // e.g., math.right() or math.up()
          public float MoveDistance;    // Half-amplitude of travel
          public float Speed;           // Oscillation frequency multiplier
          public float3 InitialPosition;// Center point around which movement occurs
          public float CollisionRadius; // Radius for collision detection
          public float DamageAmount;    // Damage dealt on collision
      }
  }
  ```
- **Authoring Class**: `MovingWallAuthoring.cs` in `HyperCasualRunner.ECS.Authoring`:
  - Fields: `Vector3 MovementAxis`, `float MoveDistance`, `float Speed`, `float CollisionRadius`, `float DamageAmount`.
  - Baker adds `MovingWallComponent` and `ObstacleComponent` to `TransformUsageFlags.Dynamic` entity.
- **System**: `MovingWallSystem.cs` (`public partial struct MovingWallSystem : ISystem` in `SimulationSystemGroup`):
  - Updates position using `transform.ValueRW.Position = wall.ValueRO.InitialPosition + wall.ValueRO.MovementAxis * (math.sin(time * wall.ValueRO.Speed) * wall.ValueRO.MoveDistance)`.

#### Obstacle 2: Pendulum Swings (`PendulumSwing`)
- **Behavior**: Rotational swinging obstacle (blade or wrecking ball) oscillating around an upper pivot axis.
- **Component Struct**:
  ```csharp
  namespace HyperCasualRunner.ECS.Components
  {
      public struct PendulumSwingComponent : IComponentData
      {
          public float MaxAngleDegrees; // Swing arc angle limit
          public float Speed;           // Frequency of swing
          public float PhaseOffset;     // Phase shift for staggered traps
          public float3 SwingAxis;      // Axis of rotation (e.g. forward/Z)
          public float CollisionRadius;
          public float DamageAmount;
      }
  }
  ```
- **Authoring Class**: `PendulumSwingAuthoring.cs` in `HyperCasualRunner.ECS.Authoring`:
  - Fields: `float MaxAngleDegrees`, `float Speed`, `float PhaseOffset`, `Vector3 SwingAxis`, `float CollisionRadius`, `float DamageAmount`.
- **System**: `PendulumSwingSystem.cs` (`public partial struct PendulumSwingSystem : ISystem` in `SimulationSystemGroup`):
  - Computes `float angle = math.sin((time * pendulum.ValueRO.Speed) + pendulum.ValueRO.PhaseOffset) * math.radians(pendulum.ValueRO.MaxAngleDegrees)`.
  - Sets `transform.ValueRW.Rotation = quaternion.AxisAngle(pendulum.ValueRO.SwingAxis, angle)`.

#### Obstacle 3: Splitting Hazards (`SplittingHazard`)
- **Behavior**: Medium/large obstacle that splits into 2 or 3 smaller child hazard entities upon taking damage, timer expiration, or player proximity.
- **Component Struct**:
  ```csharp
  namespace HyperCasualRunner.ECS.Components
  {
      public struct SplittingHazardComponent : IComponentData
      {
          public int SplitCount;        // Number of child hazards to spawn (e.g. 2 or 3)
          public Entity ChildPrefab;     // Prefab entity of the child hazard
          public float ImpulseForce;     // Lateral ejection velocity for child hazards
          public float TriggerDistance;  // Distance threshold for splitting
          public bool HasSplit;          // Split flag to prevent duplicate triggers
          public float CollisionRadius;
          public float DamageAmount;
      }
  }
  ```
- **Authoring Class**: `SplittingHazardAuthoring.cs` in `HyperCasualRunner.ECS.Authoring`:
  - Fields: `int SplitCount`, `GameObject ChildPrefab`, `float ImpulseForce`, `float TriggerDistance`, `float CollisionRadius`, `float DamageAmount`.
  - Baker converts `ChildPrefab` using `GetEntity(authoring.ChildPrefab, TransformUsageFlags.Dynamic)`.
- **System**: `SplittingHazardSystem.cs` (`public partial struct SplittingHazardSystem : ISystem` in `SimulationSystemGroup`):
  - Queries player `LocalTransform`. When player distance < `TriggerDistance` and `!HasSplit`:
    1. Spawns `SplitCount` child entities via `EntityCommandBuffer.Instantiate(hazard.ChildPrefab)`.
    2. Sets child positions at parent position + offset vector along lateral axis.
    3. Spawns `PlaySoundEventComponent` (Explosion/Split SFX) and `DestroyEventComponent` (VFX particle trigger).
    4. Destroys parent hazard entity via ECB.

---

## 4. Requirement 3: Cosmetics Shop Extension & PrestigeCurrency Transaction Logic

### 4.1 Existing Idle, Persistence & Skin Architecture

- **`Assets/UI/IdleGameHUD.uxml`**:
  - Displays HUD for Idle mode containing `GoldLabel`, `PrestigeLabel`, `BuyUpgradeButton`, and `PrestigeButton`.
- **`Assets/Scripts/UI/IdleUIManagerSystem.cs`**:
  - `SystemBase` in `SimulationSystemGroup`. Queries `CurrentRunStats` for `CurrentGold` and `PersistentPlayerStats` for `PrestigeCurrency`. Binds `PrestigeButton` to create `PrestigeEventComponent` entity.
- **`PersistentPlayerStats` Component** (`Assets/Scripts/ECS/Components/IdleComponents.cs`):
  ```csharp
  public struct PersistentPlayerStats : IComponentData
  {
      public double PrestigeCurrency;
      public float PermanentDamageMultiplier;
      public float PermanentGoldMultiplier;
  }
  ```
- **`PrestigeSystem.cs`** (`Assets/Scripts/ECS/Systems/Idle/PrestigeSystem.cs`):
  - Increments `persistentStats.ValueRW.PrestigeCurrency += 1.0` on `PrestigeEventComponent`.
  - Resets `CurrentRunStats` (`CurrentGold = 0`, `CurrentDistance = 0`).
  - Clears `ResourceWallet` buffers and resets `ProducerComponent` timers.
- **`GameProgressData.cs`** Skin API:
  - `CurrentSkinIndex`: `PlayerPrefs.GetInt("HCR_SkinIndex", 0)`
  - `UnlockedSkins`: Bitmask stored in `PlayerPrefs.GetInt("HCR_UnlockedSkins", 1)` (bit 0 set by default = skin 0 unlocked).
  - `IsSkinUnlocked(int index)`: Returns `(UnlockedSkins & (1 << index)) != 0`.
  - `UnlockSkin(int index)`: `UnlockedSkins |= (1 << index); Save();`
- **`SkinApplicatorSystem.cs`**:
  - Runs in `InitializationSystemGroup`. Checks `GameProgressData.CurrentSkinIndex` and applies material color overrides (`float4 targetColor`) to `PlayerComponent` and `SwarmComponent` entities.

### 4.2 Technical Design for Cosmetics Shop & Prestige Transaction Logic

#### 1. UXML Layout Extension (`IdleGameHUD.uxml` / `CosmeticsShopTab`)
Add a tab control to `IdleGameHUD.uxml`:
- Header navigation buttons: `UpgradesTabBtn` and `ShopTabBtn`.
- Main content container with two child views: `UpgradesContainer` and `CosmeticsShopContainer`.
- `CosmeticsShopContainer` includes a `ScrollView` containing cards for available skins:
  - **Skin 0: Classic Blue** — Cost: 0 Prestige (Default / Unlocked)
  - **Skin 1: Crimson Red** — Cost: 5 Prestige
  - **Skin 2: Solid Gold** — Cost: 15 Prestige
  - **Skin 3: Emerald Neon** — Cost: 30 Prestige
- Each card element features a preview box, title label, price label (`"5 Prestige"`), and action button (`"BUY"`, `"EQUIP"`, or `"EQUIPPED"`).

#### 2. PrestigeCurrency Transaction Event & System Logic
- **Component Struct**:
  ```csharp
  namespace HyperCasualRunner.ECS.Components
  {
      public struct CosmeticPurchaseEventComponent : IComponentData, IEnableableComponent
      {
          public int TargetSkinIndex;
          public double PrestigeCost;
      }
  }
  ```
- **Transaction Processing System (`CosmeticsShopSystem.cs`)**:
  ```csharp
  namespace HyperCasualRunner.ECS.Systems
  {
      [UpdateInGroup(typeof(SimulationSystemGroup))]
      public partial struct CosmeticsShopSystem : ISystem
      {
          public void OnCreate(ref SystemState state)
          {
              state.RequireForUpdate<CosmeticPurchaseEventComponent>();
          }

          public void OnUpdate(ref SystemState state)
          {
              foreach (var (purchaseEvent, entity) in SystemAPI.Query<RefRO<CosmeticPurchaseEventComponent>>().WithEntityAccess())
              {
                  int skinIndex = purchaseEvent.ValueRO.TargetSkinIndex;
                  double cost = purchaseEvent.ValueRO.PrestigeCost;

                  foreach (var stats in SystemAPI.Query<RefRW<PersistentPlayerStats>>())
                  {
                      if (stats.ValueRO.PrestigeCurrency >= cost && !GameProgressData.IsSkinUnlocked(skinIndex))
                      {
                          stats.ValueRW.PrestigeCurrency -= cost;
                          GameProgressData.UnlockSkin(skinIndex);
                          GameProgressData.CurrentSkinIndex = skinIndex;
                          
                          // Trigger audio/visual confirmation
                          var soundEntity = state.EntityManager.CreateEntity();
                          state.EntityManager.AddComponentData(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                      }
                  }

                  SystemAPI.SetComponentEnabled<CosmeticPurchaseEventComponent>(entity, false);
              }
          }
      }
  }
  ```

#### 3. Equip & Skin Switching Logic
- If `GameProgressData.IsSkinUnlocked(skinIndex)` is already `true`:
  - Clicking `"EQUIP"` updates `GameProgressData.CurrentSkinIndex = skinIndex`.
  - UI updates button state to `"EQUIPPED"`.
  - Next level spawn or domain reload triggers `SkinApplicatorSystem` to apply the selected skin color/material.

---

## 5. Build, Test, and Verification Tooling Overview

### 5.1 Unity Editor CLI / Batchmode Testing

The project supports direct execution of edit-mode unit tests via Unity CLI batchmode:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe" -batchmode -nographics -projectPath "d:\Git\Hyper-Casual-Runner" -runTests -testPlatform EditMode -testResults "d:\Git\Hyper-Casual-Runner\test_results.xml" -logFile "d:\Git\Hyper-Casual-Runner\unity_test.log"
```

- **Installed Unity Version**: Unity `6000.3.20f1` (Unity 6).
- **Existing Test Suite Locations**:
  - `Assets/Scripts/Editor/Tests/CoinSystemTests.cs`
  - `Assets/Scripts/Editor/Tests/LevelProgressionTests.cs`
  - `Assets/Scripts/Editor/Tests/Milestone4StressTests.cs`
  - `Assets/Scripts/Editor/Tests/ToolkitGeneratorTests.cs`

### 5.2 unityMCP Server Tools

The workspace includes configured MCP integration (`unityMCP` server) providing lazy tool execution for:
- `mcp_unityMCP_run_tests`: Executing NUnit test suites within Unity.
- `mcp_unityMCP_validate_script`: Performing script compilation validation.
- `mcp_unityMCP_batch_execute`: Running editor menu commands or batch scripts.
- `mcp_unityMCP_manage_build`: Triggering builds and asset bundle generation.

---

## 6. Implementation Roadmap for Subsequent Milestones

1. **M1 (Level Select Screen & Controller)**:
   - Wire `LevelCardItem.uxml` cloning in `UIManagerSystem.cs`.
   - Validate button event callbacks to DOTS `LevelSequenceComponent`.
   - Add unit tests verifying `GameProgressData` star/unlock updates and grid construction.

2. **M2 (Advanced Obstacle Variants)**:
   - Implement `MovingWallComponent`, `MovingWallAuthoring`, and `MovingWallSystem`.
   - Implement `PendulumSwingComponent`, `PendulumSwingAuthoring`, and `PendulumSwingSystem`.
   - Implement `SplittingHazardComponent`, `SplittingHazardAuthoring`, and `SplittingHazardSystem`.
   - Add unit tests verifying transform motion math, splitting logic, and collision tags.

3. **M3 (Cosmetics Shop Extension & Prestige Logic)**:
   - Extend `IdleGameHUD.uxml` with cosmetics tab container.
   - Implement `CosmeticPurchaseEventComponent` and `CosmeticsShopSystem`.
   - Wire `IdleUIManagerSystem.cs` to handle skin purchase and equip actions.
   - Add unit tests verifying PrestigeCurrency deduction, skin unlocking bitmask, and skin selection.
