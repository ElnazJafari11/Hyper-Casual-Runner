# Project: Hyper-Casual Runner Toolkit Extension

## Architecture
- **Engine**: Unity 2022/6000 with DOTS (Entities 1.0+) & UI Toolkit (UI Elements).
- **Architecture Pattern**: Hybrid ECS (DOTS Entities for simulation logic, UI Toolkit for UI, Mono Presentation for Audio/VFX).
- **Persistence**: `GameProgressData.cs` (PlayerPrefs wrapper) and `PersistentPlayerStats`.

## Milestones

| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| M1 | UI Toolkit Level Select Screen | `LevelSelectScreen.uxml`, LevelSelectScreenController, LevelSelectScreenEditor, dynamic grid populating level buttons from `GameProgressData.cs` | None | DONE (Verified CLEAN) |
| M2 | Advanced Obstacle Variants | 3 pure DOTS obstacle systems (Moving Walls, Pendulums, Splitting Hazards) with `IComponentData`, `ISystem`, and MonoBakers in `Assets/Scripts/ECS/` | None | DONE (Verified CLEAN) |
| M3 | Cosmetics Shop Extension | `IdleGameHUD.uxml` cosmetics tab/button, `IdleUIManagerSystem.cs`, `CosmeticsShopSystem.cs` ECB playback pattern, spending `PrestigeCurrency` on `PersistentPlayerStats` | None | DONE (Verified CLEAN) |

## Interface Contracts
### GameProgressData ↔ LevelSelectScreen
- `GameProgressData` provides total level count, unlocked levels count, current level index, and level completion status.
- LevelSelectScreen dynamically creates visual UI elements per level, styling unlocked vs locked levels, and setting click handlers to load the selected level.

### PersistentPlayerStats ↔ Cosmetics Shop
- `PersistentPlayerStats` (or related meta data) holds `PrestigeCurrency` and unlocked cosmetics state.
- Purchasing a cosmetic validates available `PrestigeCurrency`, deducts cost, marks cosmetic unlocked, and updates `PersistentPlayerStats`.

## Code Layout
- UI Documents: `Assets/UI/`
- UI Controllers / MonoScripts: `Assets/Scripts/UI/` or `Assets/Scripts/`
- DOTS ECS Authoring: `Assets/Scripts/ECS/Authoring/`
- DOTS ECS Systems: `Assets/Scripts/ECS/Systems/`
- DOTS ECS Components: `Assets/Scripts/ECS/Components/`
