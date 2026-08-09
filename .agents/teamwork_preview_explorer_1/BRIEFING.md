# BRIEFING — 2026-07-22T19:55:00Z

## Mission
Comprehensive read-only exploration of the Hyper-Casual Runner codebase for 3 milestone areas:
1. UI Toolkit Level Select Screen
2. Advanced Obstacle Variants (Moving Walls, Pendulum Swings, Splitting Hazards)
3. Cosmetics Shop Extension (IdleGameHUD tab & PrestigeCurrency transaction logic)

## 🔒 My Identity
- Archetype: Explorer
- Roles: Codebase analysis, architectural investigation, technical design specification
- Working directory: d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_explorer_1
- Original parent: 77518b05-575e-4a6f-8a64-beed938515a6
- Milestone: Preview Exploration (M1, M2, M3 Analysis)

## 🔒 Key Constraints
- Read-only investigation — do NOT modify application source code
- Strictly write outputs to d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_explorer_1/
- Follow project UI Toolkit constraint (UI Toolkit for all UI)
- Follow DOTS ECS Hybrid Architecture for VFX/Audio tag components where applicable

## Current Parent
- Conversation ID: 77518b05-575e-4a6f-8a64-beed938515a6
- Updated: 2026-07-22T19:55:00Z

## Investigation State
- **Explored paths**:
  - `Assets/UI/` (`LevelSelect.uxml`, `LevelCardItem.uxml`, `LevelSelect.uss`, `IdleGameHUD.uxml`, etc.)
  - `Assets/Scripts/UI/` (`UIManagerSystem.cs`, `IdleUIManagerSystem.cs`, `UIManager.cs`)
  - `Assets/Scripts/GameProgressData.cs`
  - `Assets/Scripts/ECS/Components/` (`LevelProgressionComponents.cs`, `IdleComponents.cs`, `CollisionComponents.cs`, `MathTweenComponent.cs`, `ShopPurchaseEventComponent.cs`, `PlaySoundEventComponent.cs`, `DestroyEventComponent.cs`)
  - `Assets/Scripts/ECS/Authoring/` (`HurdleWallAuthoring.cs`, `LaneObstacleAuthoring.cs`, `MathTweenAuthoring.cs`)
  - `Assets/Scripts/ECS/Systems/` (`LevelProgressionSystem.cs`, `CollisionSystem.cs`, `MathTweenSystem.cs`, `Idle/PrestigeSystem.cs`, `Idle/IdleShopSystem.cs`, `SkinApplicatorSystem.cs`, `AudioManagerSystem.cs`, `VFXManagerSystem.cs`)
  - `Assets/Scripts/Editor/Tests/` (`LevelProgressionTests.cs`, `CoinSystemTests.cs`, `Milestone4StressTests.cs`, etc.)
- **Key findings**:
  - M1 Level Select Screen UXML, USS, and grid logic in `UIManagerSystem.cs` fully mapped to `GameProgressData.cs` static storage and `LevelSequenceComponent` DOTS level slice loading.
  - M2 Advanced Obstacles designed following `MathTweenSystem` / `CollisionSystem` patterns (`MovingWallSystem`, `PendulumSwingSystem`, `SplittingHazardSystem`).
  - M3 Cosmetics Shop designed combining `IdleGameHUD.uxml` tab view, `CosmeticsShopSystem` deducting `PersistentPlayerStats.PrestigeCurrency`, updating `GameProgressData` skin bitmasks, and driving `SkinApplicatorSystem.cs`.
  - Batchmode Unity CLI & `unityMCP` test runner fully documented.
- **Unexplored areas**: None. All target requirements investigated.

## Key Decisions Made
- Produced detailed technical design specs in `analysis.md` and structured 5-component handoff report in `handoff.md`.

## Artifact Index
- ORIGINAL_REQUEST.md — Original request log
- BRIEFING.md — Working briefing index
- progress.md — Heartbeat and progress log
- analysis.md — Full technical exploration and design analysis report
- handoff.md — Self-contained 5-component handoff report
