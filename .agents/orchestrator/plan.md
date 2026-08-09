# Orchestration Plan: Hyper-Casual Runner Toolkit Extension

## Plan Contract & Breakdown

### Item 1: Explore Codebase & Existing Architecture [required]
- Description: Investigate existing `GameProgressData.cs`, `PersistentPlayerStats`, UI Toolkit setup (`IdleGameHUD.uxml`, `IdleUIManagerSystem.cs`), DOTS Systems and Authoring patterns in `Assets/Scripts/ECS/`.
- Acceptance Criteria:
  1. `analysis.md` created in `.agents/explorer_1/` documenting exact classes, namespaces, existing obstacle patterns, UI Toolkit conventions, and persistence API.
  2. Map of exact file paths and signatures for Level Select integration, Obstacle Authoring/Systems, and Cosmetics Shop.

### Item 2: Milestone 1 — UI Toolkit Level Select Screen [required]
- Description: Implement `Assets/UI/LevelSelectScreen.uxml` and controller script (`LevelSelectScreenController.cs`) with dynamic grid population based on `GameProgressData.cs`, along with Editor script or Scene prefab / Mono script wiring.
- Acceptance Criteria:
  1. `Assets/UI/LevelSelectScreen.uxml` exists and contains a grid container for level items.
  2. `LevelSelectScreenController.cs` (or equivalent UI controller) dynamically generates level buttons reading unlocked state from `GameProgressData.cs` and handles level selection.
  3. No compilation errors exist across the project.

### Item 3: Milestone 2 — Advanced Obstacle Variants (DOTS ECS) [required]
- Description: Implement 3 new advanced obstacle variants (Moving Walls, Pendulum Swings, Splitting Hazards) using pure DOTS `ISystem` / `IComponentData` and Authoring MonoBehaviours.
- Acceptance Criteria:
  1. 3 new Authoring scripts exist in `Assets/Scripts/ECS/Authoring/` (e.g. `MovingWallAuthoring.cs`, `PendulumObstacleAuthoring.cs`, `SplittingHazardAuthoring.cs`).
  2. 3 new DOTS ECS Systems exist in `Assets/Scripts/ECS/Systems/` (e.g. `MovingWallSystem.cs`, `PendulumObstacleSystem.cs`, `SplittingHazardSystem.cs`).
  3. All systems compile without errors and use proper SystemAPI / EntityCommandBuffer patterns.

### Item 4: Milestone 3 — Cosmetics Shop Extension [required]
- Description: Extend `IdleGameHUD.uxml` and UI controller/system (`IdleUIManagerSystem.cs`) to include a cosmetics tab/button, allowing players to spend `PrestigeCurrency` from `PersistentPlayerStats` to unlock cosmetics.
- Acceptance Criteria:
  1. `IdleGameHUD.uxml` contains a Cosmetics tab or button.
  2. Cosmetic purchase logic deducts `PrestigeCurrency` from `PersistentPlayerStats` and persists purchase state.
  3. No compilation errors exist across the project.

### Item 5: Review & Audit Gate [required]
- Description: Perform independent code review, empirical verification, and forensic audit of all implemented features.
- Acceptance Criteria:
  1. Reviewers confirm clean implementation adhering to project standards.
  2. Forensic Auditor reports CLEAN verdict (0 integrity violations / 0 fake code).
  3. Zero compilation errors reported.
