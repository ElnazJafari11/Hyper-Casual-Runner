# Round 01 Review 08 — UI Toolkit (Idle)

**Agent:** examine/analyze/review 8/10  
**Scope:** `IdleSliceUIController`, `IdleGameHUD` (+ cosmetics tab), LevelSelect ↔ idle interaction, UXML/USS  
**Architecture rule:** UI Toolkit only (no legacy uGUI / Canvas for game UI)  
**Mode:** Review only — no implementation, no push

---

## Verdict

**CONDITIONAL FAIL for idle UI cohesion** (prototype bar still partially met).

Idle MVP slices ship a working UI Toolkit control surface via code-built `IdleSliceUIController`. Separately, `IdleGameHUD.uxml` + `IdleUIManagerSystem` implement upgrades/cosmetics tabs that are **not wired into idle slice prefabs**. LevelSelect is runner-level navigation (21 slices / `LevelSequenceComponent`) and has **no idle-archetype routing**. Architecture rule is honored on the idle path (UI Toolkit only); structural gaps block a single coherent idle HUD story.

---

## Inventory (inspected)

| Asset / Type | Path | Role |
| --- | --- | --- |
| Controller | `Assets/Scripts/UI/IdleSliceUIController.cs` | Per-slice adaptive HUD (code UI) |
| ECS UI binder | `Assets/Scripts/UI/IdleUIManagerSystem.cs` | Binds `IdleGameHUD` names; cosmetics / prestige / buy |
| UXML | `Assets/UI/IdleGameHUD.uxml` | Sandbox HUD + Upgrades/Cosmetics tabs |
| USS | *(none)* | No `IdleGameHUD.uss`; all styles inline in UXML or C# |
| Level select | `Assets/Scripts/UI/LevelSelectScreenController.cs` | 21-level grid; `GameProgressData` + `LevelSequenceComponent` |
| Level select UXML/USS | `Assets/UI/LevelSelect.uxml`, `LevelSelectScreen.uxml`, `LevelSelect.uss`, `LevelCardItem.uxml` | Proper Toolkit + stylesheet |
| Runner HUD | `Assets/UI/GameHUD.uxml` + `GameHUD.uss` + `UIManagerSystem.cs` | Runner meta; Level Select entry |
| Cosmetics sim | `Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs` | Prestige → unlock/equip skins |
| Slice generator | `Assets/Scripts/Editor/IdleToolkitSliceGenerator.cs` | Adds `UIDocument` + `IdleSliceUIController` only |
| Prefabs | `Assets/ToolkitExamples/Idle/*_Slice.prefab` (19) | `sourceAsset: {fileID: 0}` — empty VisualTree |

Also noted (adjacent, not idle-slice primary): `ToolkitHubManager` builds hub + gold-priced Skin Shop in code (UI Toolkit compliant; economy conflicts with prestige cosmetics).

---

## Architecture compliance (UI Toolkit only)

| Check | Result |
| --- | --- |
| Idle slice HUD uses `UIDocument` + UIElements | **PASS** — `IdleSliceUIController`, generator, all 19 idle prefabs |
| Cosmetics / IdleGameHUD use UIElements | **PASS** — UXML buttons + `IdleUIManagerSystem` queries |
| LevelSelect uses UIElements + USS | **PASS** — UXML/USS/controller; tests under `LevelSelectScreenTests` |
| No uGUI on idle UI path | **PASS** — no `UnityEngine.UI` in idle UI scripts |
| Hybrid presentation pattern for UI | **PARTIAL FAIL** — `IdleUIManagerSystem` lives in `SimulationSystemGroup` and does managed UI every frame (should be Presentation-oriented / MonoBehaviour bind once) |

Out of scope but present: Archive/`TextMesh Pro`/Ads sample Canvas assets — not used by idle toolkit slices.

---

## Findings

### Critical

1. **Dual idle HUD stacks, only one shipped on slices**  
   - Live path: `IdleSliceUIController` clears `UIDocument` root and builds labels/buttons in C# (`EnsureUi`). Prefabs have `sourceAsset: 0`.  
   - Dead/orphan path: `IdleGameHUD.uxml` (Gold/Prestige, Upgrades/Cosmetics tabs, skin rows) + `IdleUIManagerSystem` (`Q("PrestigeButton")`, `CosmeticsTabButton`, `EquipSkin0Button`, …).  
   - Grep: `IdleGameHUD` is **not referenced** by any `.prefab` / `.unity` / `.cs` assigner. Generator never assigns the VisualTree.  
   - **Impact:** Cosmetics tab UI exists on disk but never appears in idle MVP play. Implementers must pick one stack and wire it.

2. **`IdleUIManagerSystem` document discovery is unsafe**  
   - `Object.FindObjectOfType<UIDocument>()` takes the first document in the scene.  
   - Idle slices already own a `UIDocument` driven by `IdleSliceUIController` (clears root). If both systems run together, binding targets the wrong tree or a wiped tree; `_buttonBound` latches true even when queries return null.  
   - No unbind on destroy; tab lambdas capture fields permanently.

### Major

3. **LevelSelect has zero idle interaction**  
   - `_totalLevels = 21` maps runner toolkit slices via `GameProgressData.CurrentLevelIndex` / `UnlockedLevelIndex` and `LevelSequenceComponent.TransitionState = TeardownCurrent`.  
   - Idle matrix has **19** archetypes under `ToolkitExamples/Idle/`; no mapping from level index → `IdleArchetype` / idle prefab.  
   - `GameHUD` / `UIManagerSystem` Level Select opens runner overlay; idle slices do not host LevelSelect.  
   - **Impact:** “LevelSelect interaction with idle” is currently a **gap**, not a bug in LevelSelect itself.

4. **Cosmetics economy / surface inconsistency**  
   - `IdleGameHUD` + `IdleUIManagerSystem` + `CosmeticsShopSystem`: prestige costs (5 / 15 / 30), event `CosmeticPurchaseEventComponent`.  
   - `ToolkitHubManager.AddShopItem`: gold costs (200 / 1000), mutates `GameProgressData` **directly** (bypasses ECS shop). Skin 3 (Emerald Neon) only on IdleGameHUD.  
   - Locked-skin buy button color not restored in `UpdateSkinButtonState` when still locked (text updates to `BUY (N P)` but prior unlocked blue may stick after state flips).

5. **UXML without USS for idle sandbox HUD**  
   - `IdleGameHUD.uxml`: large inline `style=` blocks; no `<ui:Style src="…"/>`.  
   - Contrast: `GameHUD.uxml` + `GameHUD.uss`, `LevelSelect*.uxml` + `LevelSelect.uss` — correct Toolkit separation.  
   - `IdleSliceUIController` duplicates the anti-pattern (all inline `style.*` in C#), with `// TODO: [STUB]` runtime `PanelSettings`.

6. **Per-frame UI query cost in `IdleUIManagerSystem.OnUpdate`**  
   - After bind, still `root.Q<Label>("GoldLabel")` / `PrestigeLabel` every frame inside `foreach` queries. Should cache labels once with buttons.

### Minor / Notice

7. **`IdleSliceUIController` multi-slice ambiguity** — `CreateEntityQuery(typeof(IdleSliceState))` then `arr[0]`; first entity wins if multiple slices exist.  
8. **Event entity hygiene** — Click/buy/etc. spawn one-shot entities; no UI-side cleanup (depends on sim systems consuming them). Prestige fires **both** `IdlePhaseShiftEvent` and `PrestigeEventComponent` (`FirePrestige`).  
9. **Duplicate LevelSelect UXML** — `LevelSelect.uxml` and `LevelSelectScreen.uxml` are identical; editor falls back between them. Harmless but confusing.  
10. **Code UI vs asset UI** — Adaptive archetype buttons are a reasonable MVP; long-term bar prefers UXML templates + USS classes + thin binders (match LevelSelect pattern).  
11. **Tab a11y / state** — Cosmetics tab toggles `display` only; no selected-tab USS class / disabled sibling feedback.  
12. **`IEnableableComponent` on cosmetic events** — Shop disables components after handle; UI creates new entities each click (OK) but disabled entities may accumulate if never destroyed.

---

## Component deep-dives

### IdleSliceUIController

**Strengths**
- Clear archetype → verb mapping aligned with `docs/idle_mechanics_matrix.md` / generator HowTo strings.
- Event-driven ECS bridge (`IdleClickEvent`, `IdleBuyGeneratorEvent`, …) keeps MonoBehaviour thin.
- `RequireComponent(UIDocument + IdleSliceBootstrap)`; PanelSettings stub avoids blank panels.

**Weaknesses**
- No VisualTreeAsset; regenerating layout requires code change.
- Stats string is a kitchen-sink dump (not per-archetype).
- Buttons always enabled (no affordance for insufficient currency / max workers).
- `Update()` rebuilds gate + full query every frame after build (stats only needed).

### IdleGameHUD.uxml + cosmetics tab

**Strengths**
- Named elements match `IdleUIManagerSystem` contracts (`GoldLabel`, `PrestigeLabel`, tab/container names, skin buttons 0–3).
- Cosmetics in `ScrollView`; default tab Upgrades visible, Cosmetics `display: none` — correct initial state.
- Skin labels match hub naming for 0–2; prestige pricing matches tests (`CosmeticsShopTests` uses 5 P for skin 1).

**Weaknesses**
- Orphaned asset (see Critical #1).
- No USS; hardcoded purple prestige / green buy — fine for prototype, poor reuse.
- No disabled state / insufficient-funds styling in UXML or binder.
- No link to `IdleSliceState` primary currency (shows `CurrentRunStats.CurrentGold` instead) — wrong data model for idle MVP slices.

### IdleUIManagerSystem

**Strengths**
- Correct event emission for prestige, shop purchase, cosmetics.
- Skin button text reflects equipped / unlocked / buy via `GameProgressData`.

**Weaknesses**
- Simulation-group managed UI + FindObjectOfType (Critical #2 / Major #6).
- Assumes `IdleGameHUD` element names; silent no-op on idle slice documents.
- Does not drive or coexist with `IdleSliceUIController`.

### LevelSelect ↔ idle

**LevelSelect quality (runner):** solid — UXML + USS, card template, lock/play/star states, fallback builder, EditMode tests, editor auto-assign.

**Idle bridge:** missing — no idle hub entry, no archetype list, no load of `ToolkitExamples/Idle/*_Slice.prefab`, no save-slot awareness beyond runner `GameProgressData` level indices. Idle entry today is generator prefabs / optional `ToolkitHubManager` list, not LevelSelect.

---

## Acceptance criteria (for Round 01 implementers)

Machine-checkable where possible:

| ID | Criterion | Pass if |
| --- | --- | --- |
| UI-01 | Single idle HUD authority | Exactly one of: (A) idle prefabs reference `IdleGameHUD.uxml` and drop conflicting root.Clear UI, or (B) cosmetics/upgrades folded into `IdleSliceUIController` and `IdleGameHUD` marked deferred/deleted |
| UI-02 | Cosmetics reachable in play | Enter Play on any idle slice → Cosmetics tab or equivalent skin controls visible; buy/equip updates `GameProgressData.CurrentSkinIndex` |
| UI-03 | No FindObjectOfType UI bind | `IdleUIManagerSystem` (or replacement) resolves `UIDocument` via explicit ref / singleton / authoring — grep shows no `FindObjectOfType<UIDocument>` for idle HUD |
| UI-04 | Toolkit-only idle path | Idle UI scripts remain UIElements-only; no new `UnityEngine.UI` / Canvas HUD |
| UI-05 | Styles extracted or stubbed | Either `IdleGameHUD.uss` exists and is linked, or intentional `// TODO: [STUB]` on inline styles with count greppable |
| UI-06 | LevelSelect idle policy explicit | Doc or code comment + stub: LevelSelect is runner-only **or** idle entries added (19 archetypes) with load path — no silent “assumes it works” |
| UI-07 | Label queries cached | Gold/Prestige (or idle stats) labels bound once; no per-frame `root.Q` for those names |

---

## Recommended implement order (guidance only)

1. Decide HUD stack (UI-01) — prefer wiring `IdleGameHUD.uxml` onto sandbox / shared idle hub **or** keep code HUD and port cosmetics into it; do not leave both live.  
2. Fix document binding (UI-03) and Presentation placement.  
3. Align currency labels with `IdleSliceState` / `PersistentPlayerStats` (not runner `CurrentRunStats` alone).  
4. Add `IdleGameHUD.uss` (UI-05).  
5. Record LevelSelect policy (UI-06); if idle browser needed, prefer extending hub over overloading 21-level runner select.

---

## Out of scope / deferred

- Visual skin mesh application (`SkinApplicatorSystem`) — consumes `CurrentSkinIndex`; not reviewed in depth here.  
- Full per-archetype UXML variants (matrix pillar “second axis”) — beyond prototype bar.  
- Cleaning Archive uGUI / TMP samples.

---

## Evidence summary

- Idle prefabs: 19 × `IdleSliceUIController`, `UIDocument.sourceAsset = 0`.  
- `IdleGameHUD.uxml` exists; zero scene/prefab references found.  
- `IdleUIManagerSystem`: `FindObjectOfType<UIDocument>`, `SimulationSystemGroup`, cosmetics tab display toggle, prestige-priced skins.  
- LevelSelect: 21 levels → `LevelSequenceComponent`; no idle archetype symbols.  
- Architecture: idle UI path is UI Toolkit; cohesion between assets and runtime is not.

**STATUS:** Review complete — **UNVERIFIED** play-mode (no Play Mode / screenshot this pass). Static file + grep evidence only.
