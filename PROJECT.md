# Project: Hyper-Casual Runner Toolkit

## Architecture
- **Engine**: Unity 2022.3+ / 6000.x with Unity DOTS Entities 1.0+
- **Simulation**: Pure DOTS ECS Systems (SimulationSystemGroup), ComponentData (IComponentData, IBufferElementData), and MonoBehaviour Bakers.
- **Presentation Layer**: Hybrid ECS pattern (Tags -> Presentation Systems reading AudioSource / ParticleSystem / UI Elements).
- **UI Architecture**: Unity UI Toolkit (UI Elements) for HUD, Level Select, Victory, Defeat screens.
- **Persistence**: `GameProgressData.cs` (PlayerPrefs wrapper) for meta-progression across multi-level slice transitions.

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| M1 | Grid Pathfinder & Maze Collector | Implementation of Grid Tile Pathfinder, Maze Collector system, authoring component, baker, and slice integration (`12_StackyDash_Grid`) | None | DONE |
| M2 | Snake Follower Chain & Collision | Implementation of Snake Follower chain movement, follower spawning, collision handling, authoring component, baker, and slice integration (`5_JoinClash_Snake`) | None | DONE |
| M3 | Coin Multiplier & Splitting Physics | Implementation of Coin Multiplier, Splitting Physics system, authoring component, baker, and slice integration (`14_MoneyRush_Coins`) | None | DONE |
| M4 | Multi-Level Progression Loader & UI Integration | Implementation of `LevelProgressionSystem.cs`, `LevelSequenceComponent.cs`, seamless scene/prefab transitions, UI Toolkit meta-game integration, and persistence via `GameProgressData.cs` | M1, M2, M3 | DONE |
| M5 | Automated Verification Suite & E2E Validation | Run `RunVerificationSuite` across all 21 slice prefabs in `Assets/ToolkitExamples/`, verify zero compilation errors, component bindings, and tier coverage | M4 | DONE |

## Interface Contracts
### Level Progression ↔ Game Progress Data
- `LevelProgressionSystem` reads end-zone reach events from `LevelStateComponent` or collision systems.
- Updates `GameProgressData.Coins`, `GameProgressData.CurrentLevel`, `GameProgressData.Save()`.
- Requests slice transition via EntityCommandBuffer / Scene System.

### Simulation ↔ Hybrid Presentation (VFX & Audio)
- `PlaySoundEventComponent` (Entity tag with AudioType enum) -> `AudioManagerSystem` triggers `AudioSource.PlayOneShot()`.
- `DestroyEventComponent` or `SpawnVFXEventComponent` -> `VFXManagerSystem` instantiates/plays `ParticleSystem`.

## Code Layout
- `Assets/Scripts/ECS/Components/`: Component structs (`IComponentData`, `IBufferElementData`)
- `Assets/Scripts/ECS/Authoring/`: Authoring scripts (`MonoBehaviour` + `Baker<T>`)
- `Assets/Scripts/ECS/Systems/`: Pure DOTS Systems (`ISystem`, `SystemBase`)
- `Assets/Scripts/UI/`: UI Toolkit Controllers (`UIManagerSystem`, `UIManager`, UXML/USS files)
- `Assets/Scripts/Editor/`: Editor tools, generators (`ToolkitExampleGenerator.cs`), and verification tests
- `Assets/ToolkitExamples/`: 21 archetype slice prefabs (`1_SubwaySurfers_Meta_Slice.prefab` ... `21_WeaponMaster_Shooter_Slice.prefab`)
