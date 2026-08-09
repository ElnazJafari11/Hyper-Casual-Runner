## 2026-07-22T19:55:13Z
You are the Worker agent for Milestone 3: Cosmetics Shop Extension.
Your working directory is `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_worker_m3/`.
Read `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_explorer_1/analysis.md` and `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_explorer_1/handoff.md` for context.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

Requirements for Milestone 3:
1. Extend `Assets/UI/IdleGameHUD.uxml` to include a Cosmetics tab or cosmetics button and cosmetics scroll view / view container.
2. Extend `IdleUIManagerSystem.cs` (or implement `CosmeticsShopSystem.cs`) and `CosmeticPurchaseEventComponent` to process cosmetics purchases using `PrestigeCurrency` from `PersistentPlayerStats`.
3. Ensure buying a cosmetic subtracts `PrestigeCurrency` from `PersistentPlayerStats`, unlocks the skin in `GameProgressData` (`UnlockSkin(index)`), and allows equipping skins (`CurrentSkinIndex`).
4. Create unit tests verifying cosmetics purchase transactions and PrestigeCurrency deduction in `Assets/Scripts/Editor/Tests/CosmeticsShopTests.cs`.
5. Run compilation check via Unity CLI or test suite and include exact test output.
6. Write your handoff report to `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_worker_m3/handoff.md` and update `progress.md`. Send completion message back to orchestrator.
