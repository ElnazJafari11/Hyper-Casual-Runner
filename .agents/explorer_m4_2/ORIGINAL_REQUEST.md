## 2026-07-22T07:51:02Z
You are Explorer 2 for Milestone 4: Multi-Level Progression Loader & UI Integration.
Your working directory is d:\Git\Hyper-Casual-Runner\.agents\explorer_m4_2.

Task:
Investigate UI Toolkit (UI Elements) architecture for HUD, Level Select, Victory, and Defeat screens.
1. MANDATORY RULE: Use Unity UI Toolkit (UI Elements) for all game UI. Never use legacy uGUI/Canvas.
2. Inspect existing UI scripts in `Assets/Scripts/UI/`, UXML documents, and USS style sheets.
3. Design UI Toolkit layout and controller architecture:
   - HUD (Level progress bar, Coin count badge, Settings button)
   - Level Select screen (Grid of unlocked/locked level buttons 1–21)
   - Victory screen (Stars earned, coins collected, Next Level button, Multiplier wheel bonus)
   - Defeat screen (Retry button, Level reached display)
4. Design `UIManagerSystem.cs` to bind DOTS entity events (`LevelStateComponent`, `PlayerCoinRunnerComponent`) to UI Toolkit VisualElements.
5. Document UI hierarchy, UXML/USS specs, and controller logic in `d:\Git\Hyper-Casual-Runner\.agents\explorer_m4_2\analysis.md`.
6. Deliver handoff report in `d:\Git\Hyper-Casual-Runner\.agents\explorer_m4_2\handoff.md` and notify parent.
