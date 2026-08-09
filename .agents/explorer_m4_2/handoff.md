# Handoff Report: UI Toolkit Architecture & DOTS Binding (Milestone 4)

## 1. Observation
- **Mandatory Constraint**: 100% Unity UI Toolkit (`UnityEngine.UIElements`, UXML, USS, `UIDocument`). **No legacy uGUI/Canvas is permitted** as per project rules (`AGENTS.md`).
- **Existing Files Audited**:
  - `Assets/UI/GameHUD.uxml` and `Assets/UI/GameHUD.uss`: Prototype HUD layout.
  - `Assets/Scripts/UI/UIManagerSystem.cs`: Legacy prototype MonoBehaviour generating UI controls programmatically in C# rather than separating into UXML templates.
  - `Assets/Scripts/UI/ToolkitHubManager.cs`: Hub manager controlling level spawning and shop mechanics.
  - `Assets/Scripts/GameProgressData.cs`: `PlayerPrefs` persistence wrapper for coins, upgrades, and skins.
  - `Assets/Scripts/ECS/Components/LevelStateComponent.cs`: ECS component defining `GameState` (`Pregame`, `Playing`, `Victory`, `Defeat`).
  - `Assets/Scripts/ECS/Components/CoinMultiplierComponents.cs`: Defines `PlayerCoinRunnerComponent` holding `CurrentCoinCount`.
- **Target Deliverables**: Detailed UI Toolkit layout, UXML/USS specifications, controller design for HUD, Level Select (1–21 grid), Victory (stars & multiplier wheel), Defeat, and `UIManagerSystem.cs` DOTS binding.

---

## 2. Logic Chain
1. **Transition to Declarative UI Toolkit**:
   - The current `UIManagerSystem.cs` constructs UI elements imperatively via C# instantiation. Moving to UXML documents (`GameHUD.uxml`, `LevelSelect.uxml`, `VictoryScreen.uxml`, `DefeatScreen.uxml`) and USS stylesheets ensures clean separation of visual presentation, styling, and game logic.
2. **Level Progression Model**:
   - Level Select screen requires 21 interactive level cards. `GameProgressData` must be extended with `HighestUnlockedLevel` and `GetLevelStars(level)` to track progression and star completion (0–3 stars) across runs.
3. **DOTS Entity Binding Strategy**:
   - `UIManagerSystem` acts as the managed presentation controller attached to a `UIDocument`. On each frame / update tick, it queries singleton entities (`LevelStateComponent`, `PlayerCoinRunnerComponent`, `PlayerComponent`, `LocalTransform`, `EndZoneComponent`).
   - UI screen visibility is reactively toggled (`DisplayStyle.Flex` vs `DisplayStyle.None`) when `LevelStateComponent.CurrentState` changes.
   - The Level Progress Bar is bound to `(PlayerPosition.z - StartZ) / (EndZonePosition.z - StartZ)`.
4. **Victory Multiplier Wheel**:
   - A ping-pong animation curve moves a UI Toolkit indicator across a multiplier track (1.5x–5.0x). Clicking the claim button calculates bonus gold and updates `GameProgressData.TotalGold`.

---

## 3. Caveats
- **Mock Ads Integration**: Ad reward callbacks (`MockAdsManager.cs`) are mocked in the editor environment.
- **Scene / Level Loader Coordination**: Scene loading or prefab instantiation for Levels 1–21 is handled in parallel by the Level Loader system (Explorer 1 / Implementer).

---

## 4. Conclusion
The UI architecture for Milestone 4 has been fully designed and documented in `d:\Git\Hyper-Casual-Runner\.agents\explorer_m4_2\analysis.md`. The design adheres strictly to Unity UI Toolkit standards, eliminates programmatic UI layout hacks, establishes clean DOTS entity bindings (`LevelStateComponent`, `PlayerCoinRunnerComponent`), and provides complete UXML, USS, and C# controller specifications.

---

## 5. Verification Method
1. Inspect `d:\Git\Hyper-Casual-Runner\.agents\explorer_m4_2\analysis.md` for full UXML structures, USS classes, and DOTS binding code contracts.
2. Verify zero usage of `UnityEngine.UI` or `Canvas` across all proposed scripts and UI assets.
3. Validate that `GameProgressData.cs` extensions provide persistent level unlocking (Levels 1–21) and star ratings.
4. Perform Unity Play Mode verification once Implementer builds the UXML/USS files and `UIManagerSystem.cs`.
