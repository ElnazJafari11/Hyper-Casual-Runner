# BRIEFING — 2026-07-22T19:55:13Z

## Mission
Milestone 3: Cosmetics Shop Extension - Extend UI, implementing DOTS transaction logic for cosmetic purchases with PrestigeCurrency, updating GameProgressData skin state, writing tests, and running Unity tests.

## 🔒 My Identity
- Archetype: implementer / qa / specialist
- Roles: implementer, qa, specialist
- Working directory: d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_worker_m3/
- Original parent: 77518b05-575e-4a6f-8a64-beed938515a6
- Milestone: Milestone 3 - Cosmetics Shop Extension

## 🔒 Key Constraints
- UI Architecture: UI Toolkit (UI Elements) for all user interfaces.
- Persistence: GameProgressData.cs & PersistentPlayerStats for PrestigeCurrency and skin unlocking/equipping.
- Architecture: DOTS Component & System for cosmetics purchase event processing.
- Genuine implementation required: No hardcoded test results or facade implementations.
- Write handoff to `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_worker_m3/handoff.md`, update `progress.md`, and report back via `send_message`.

## Current Parent
- Conversation ID: 77518b05-575e-4a6f-8a64-beed938515a6
- Updated: 2026-07-22T19:55:13Z

## Task Summary
- **What to build**: Cosmetics Shop UI tab/container in UI Toolkit UXML, DOTS `CosmeticPurchaseEventComponent` and system handling cosmetics purchase with `PrestigeCurrency` deduction, updating `PersistentPlayerStats` and `GameProgressData` (`UnlockSkin`, `CurrentSkinIndex`), unit tests in `CosmeticsShopTests.cs`, and verify with Unity CLI tests.
- **Success criteria**:
  1. `Assets/UI/IdleGameHUD.uxml` includes Cosmetics tab/button & cosmetics view container.
  2. `CosmeticPurchaseEventComponent` & `IdleUIManagerSystem.cs` / `CosmeticsShopSystem.cs` processes cosmetics purchases.
  3. `PrestigeCurrency` deducted from `PersistentPlayerStats`, skin unlocked in `GameProgressData` (`UnlockSkin(index)`), equipped skin set (`CurrentSkinIndex`).
  4. Unit tests in `Assets/Scripts/Editor/Tests/CosmeticsShopTests.cs` pass.
  5. Unity CLI test run succeeds with 0 failures.
  6. Handoff report and progress updated, completion message sent to parent.

## Change Tracker
- **Files modified**: None yet
- **Build status**: TBD
- **Pending issues**: None

## Quality Status
- **Build/test result**: TBD
- **Lint status**: TBD
- **Tests added/modified**: None yet

## Loaded Skills
- None

## Artifact Index
- `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_worker_m3/ORIGINAL_REQUEST.md` — Original request log
- `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_worker_m3/BRIEFING.md` — Working memory index
