# Original User Request

## 2026-07-22T06:52:19Z

Build a high-performance, production-ready Unity DOTS (Entities 1.0+) Hyper-Casual Runner Toolkit containing 21 fully functional, playable archetype slices, a unified multi-level progression loader, and UI Toolkit meta-game integration.

Working directory: d:\Git\Hyper-Casual-Runner
Integrity mode: development

## Requirements

### R1. Complete Archetype DOTS ECS Implementation
Implement dedicated DOTS ECS Systems, Component Data, and MonoBehaviour Bakers for all remaining unrefined hyper-casual runner slices:
- Grid Tile Pathfinder / Maze Collector (`12_StackyDash_Grid`)
- Snake Follower Chain & Collision (`5_JoinClash_Snake`)
- Coin Multiplier & Splitting Physics (`14_MoneyRush_Coins`)

### R2. Unified Multi-Level Progression Loader
Build a DOTS Level Management system (`LevelProgressionSystem.cs` & `LevelSequenceComponent.cs`) that seamlessly transitions players between level slices upon reaching the EndZone (Level 1 -> Level 2 -> Level 3), persisting player currency and upgrades across levels via `GameProgressData.cs`.

### R3. Hybrid ECS Presentation Architecture
Maintain strict hybrid separation: Unity DOTS Entities 1.0+ for simulation logic, standard Unity UI Toolkit for UI Elements (HUD, Level Select, Victory, Defeat), and Presentation Systems for Audio and VFX tags.

### R4. Automated Verification Suite
Ensure all 21 template slice prefabs in `Assets/ToolkitExamples/` pass automated Editor verification (`RunVerificationSuite`) with zero compilation errors, verified child object hierarchies, and validated authoring component bindings.

## Acceptance Criteria

### Mechanics & Systems
- [ ] Custom DOTS ECS systems created for Grid Tile Pathfinder, Snake Chain, and Coin Multipliers.
- [ ] LevelProgressionSystem handles seamless multi-level scene/prefab transitions.

### Verification & Asset Quality
- [ ] All 21 Playable Slices in `Assets/ToolkitExamples/` pass `RunVerificationSuite` with 0 compiler errors.
- [ ] All UI features strictly use Unity UI Toolkit (UI Elements).

## Follow-up — 2026-07-22T19:52:29Z

# Teamwork Project Prompt

> Goal: Complete the remainder of the Hyper-Casual Runner Toolkit (Level Select, Cosmetics, Advanced Obstacles).

The Hyper-Casual Runner Toolkit is a DOTS (Entities 1.0+) project. The core runner mechanics and the Idle Sandbox are complete. We need to finalize the remaining UI and Level Design features.

Working directory: d:/Git/Hyper-Casual-Runner
Integrity mode: development

## Requirements

### R1. UI Toolkit Level Select Screen
Implement a Level Select screen using Unity UI Toolkit (`UIDocument`). It must dynamically populate a grid of levels based on `GameProgressData.cs`. It must use exclusive UI Elements (no uGUI).

### R2. Advanced Obstacle Variants
Create 3 new advanced obstacle variants (e.g., Moving Walls, Pendulums, Splitting Hazards) using pure DOTS `ISystem` and `IComponentData` in the `Assets/Scripts/ECS/` folders. Provide Authoring MonoBehaviours for them.

### R3. Cosmetics Shop Extension
Extend the `IdleGameHUD.uxml` and `IdleUIManagerSystem.cs` to include a cosmetics shop (e.g., unlocking new player colors/trails using PrestigeCurrency).

## Acceptance Criteria

### UI Toolkit Level Select
- [ ] `LevelSelectScreen.uxml` exists in `Assets/UI/`.
- [ ] An Editor script or Scene prefab exists that wires the `.uxml` to a controller script.
- [ ] No compilation errors (`unityMCP read_console` returns 0 errors).

### Advanced Obstacles
- [ ] 3 new Authoring scripts exist in `Assets/Scripts/ECS/Authoring/`.
- [ ] 3 new DOTS Systems exist in `Assets/Scripts/ECS/Systems/`.
- [ ] No compilation errors.

### Cosmetics Shop
- [ ] `IdleGameHUD.uxml` contains a Cosmetics tab or button.
- [ ] Buying a cosmetic subtracts `PrestigeCurrency` from `PersistentPlayerStats`.
- [ ] No compilation errors.

