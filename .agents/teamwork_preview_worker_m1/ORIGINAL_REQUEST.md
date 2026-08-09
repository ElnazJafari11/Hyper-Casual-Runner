## 2026-07-22T19:55:12Z
You are the Worker agent for Milestone 1: UI Toolkit Level Select Screen.
Your working directory is `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_worker_m1/`.
Read `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_explorer_1/analysis.md` and `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_explorer_1/handoff.md` for context.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

Requirements for Milestone 1:
1. Create/Ensure `Assets/UI/LevelSelectScreen.uxml` exists in `Assets/UI/` (can incorporate or copy/adapt `LevelSelect.uxml` and `LevelCardItem.uxml`).
2. Provide a UI controller script (e.g. `LevelSelectScreenController.cs` or enhance `UIManagerSystem.cs`) and an Editor script (`Assets/Scripts/Editor/LevelSelectScreenEditor.cs`) or scene prefab that wires the `.uxml` to the controller script.
3. Dynamically populate a grid of levels based on `GameProgressData.cs` (`UnlockedLevelIndex`, `CurrentLevelIndex`, `GetLevelStars(index)`). Clicking an unlocked level updates `GameProgressData.CurrentLevelIndex` and sends transition command to `LevelSequenceComponent` / `LevelProgressionSystem`.
4. Ensure pure UI Toolkit (UI Elements) is used exclusively.
5. Create or run edit-mode unit tests for Level Select grid construction and progress data binding. Run compilation check via Unity CLI or test suite and include exact test output.
6. Write your handoff report to `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_worker_m1/handoff.md` and update `progress.md`. Send completion message back to orchestrator.
