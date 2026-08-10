# Round 03 Review 08 — UI Toolkit (Idle) Re-Audit

**Agent:** examine/analyze/review 8/10  
**Scope:** Post-`impl_08` (R2) idle UI — live HUD, hub cosmetics policy, deferred sandbox, LevelSelect, smoke coverage  
**Prior:** `round_02/review_08_ui.md` → `round_02/impl_08_ui.md` (commit `9ffe8f3`)  
**Architecture rule:** UI Toolkit only (no legacy uGUI / Canvas for game UI)  
**Mode:** Review only — no implementation, no push

---

## Verdict

**PASS** for idle UI cohesion at the prototype / toolkit quality bar.

Round 02 implement closed the remaining cohesion majors that blocked a clean static PASS: EditMode HUD surface smoke (`IdleSliceHudSmokeTests`), hub Skin Shop dual-economy removal (equip-only + STUB policy), sandbox binder unbind, and Toolkit-only regression. Sole live authority remains `IdleSliceUIController` on all 19 idle prefabs; deferred sandbox stays unused.

Retained open item (not cohesion-blocking): **UI2-01 / UI3-01 Play Mode cosmetics still UNVERIFIED** — Hyper-Casual-Runner has no interactive editor on the MCP bridge this pass (`doctor` → registered sessions empty; discovered editor is `thepcgtoolkit`, not HCR).

---

## Delta vs Round 02

| R02 finding / UI2 criterion | Post-impl_08 status |
| --- | --- |
| UI2-01 Play Mode cosmetics smoke | **Still UNVERIFIED** — no HCR interactive editor / capture |
| UI2-02 EditMode HUD smoke | **RESOLVED** — `IdleSliceHudSmokeTests` Passed (`Logs/IdleHud-TestResults.xml`) |
| UI2-03 Hub vs prestige dual economy | **RESOLVED** — hub equip-only; gold `UnlockSkin` removed; STUB + player hint |
| UI2-04 Sandbox currency / binder | **N/A PASS** — **0** `IdleGameHudBinder` on prefabs/scenes |
| UI2-05 Button unbind | **RESOLVED** — named handlers + `UnbindButtons` on clear / rebind / `OnStopRunning` |
| UI2-06 Toolkit-only regression | **PASS** — 0 `FindObjectOfType<UIDocument>`; 0 `using UnityEngine.UI;` under `Assets/Scripts/UI` |
| Major: no live HUD surface test | **CLOSED** by UI2-02 |
| Major: hub gold Skin Shop | **CLOSED** by UI2-03 |
| Minor: sandbox click stack | **CLOSED** by UI2-05 |
| Deferred sandbox `CurrentRunStats` gold | **Still present** — acceptable while binder unused |

---

## Inventory (inspected)

| Asset / Type | Path | Role (post-R2) |
| --- | --- | --- |
| Live controller | `Assets/Scripts/UI/IdleSliceUIController.cs` | Sole idle-slice HUD; `EnsureHudBuilt` + `RootVisualElement` for EditMode |
| HUD smoke | `Assets/Scripts/Editor/Tests/IdleSliceHudSmokeTests.cs` | Named tabs + skin buttons without Play |
| Binder | `Assets/Scripts/UI/IdleGameHudBinder.cs` | Explicit sandbox registry (`Active`) |
| Sandbox ECS binder | `Assets/Scripts/UI/IdleUIManagerSystem.cs` | Deferred; unbind + `TargetSlice` on cosmetics |
| UXML / USS | `Assets/UI/IdleGameHUD.uxml` + `.uss` | Deferred sandbox + banner |
| Level select | `Assets/Scripts/UI/LevelSelectScreenController.cs` | Runner-only 21; idle policy + STUB |
| Hub | `Assets/Scripts/UI/ToolkitHubManager.cs` | Equip-only Skin Shop; unlock via Idle Cosmetics |
| Cosmetics sim | `Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs` | Prestige spend via `IdleEventTarget` + `TargetSlice` |
| Prefabs | `Assets/ToolkitExamples/Idle/*_Slice.prefab` (**19**/19) | `IdleSliceUIController`; `sourceAsset: 0` |

---

## Round 01 + Round 02 criteria re-score

### R01 UI-01..07 (regression)

| ID | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| UI-01 | Single idle HUD authority | **PASS** | Option B docs; 19/19 prefabs live controller; sandbox deferred + unused |
| UI-02 | Cosmetics reachable in play | **PASS (static) / UNVERIFIED (play)** | Cosmetics tab + `FireCosmetic` → `CosmeticPurchaseEventComponent`; no Play capture |
| UI-03 | No FindObjectOfType UI bind | **PASS** | 0 matches under `Assets/Scripts` |
| UI-04 | Toolkit-only idle path | **PASS** | UIElements only on idle UI scripts |
| UI-05 | Styles extracted or stubbed | **PASS** | Sandbox USS linked; live HUD **5×** `// TODO: [STUB]` |
| UI-06 | LevelSelect idle policy explicit | **PASS** | Class summary + STUB idle browser |
| UI-07 | Label queries cached | **PASS** | Sandbox `BindOnce`; live `_stats` / `_skinButtons` |

### R02 UI2-01..06

| ID | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| UI2-01 | Play Mode cosmetics smoke | **UNVERIFIED** | MCP `doctor`: no HCR registered; live transport pinned to `thepcgtoolkit` |
| UI2-02 | EditMode HUD smoke | **PASS** | `IdleSliceHudSmokeTests` → passed=1 failed=0 (`Logs/IdleHud-TestResults.xml`) |
| UI2-03 | Hub vs prestige policy | **PASS** | `ToolkitHubManager` STUB + equip-only; no gold unlock |
| UI2-04 | Sandbox currency or stay deferred | **N/A (PASS)** | 0 binder instances in Assets prefabs/scenes |
| UI2-05 | Button unbind | **PASS (static)** | `UnbindButtons` wired; cosmetic events set `TargetSlice` |
| UI2-06 | Toolkit-only regression | **PASS** | Same as UI-03/UI-04 |

---

## Architecture compliance (UI Toolkit only)

| Check | Result |
| --- | --- |
| Idle slice HUD uses `UIDocument` + UIElements | **PASS** |
| Cosmetics on live path use UIElements | **PASS** |
| Sandbox HUD UXML + USS | **PASS** (deferred) |
| LevelSelect UIElements + USS | **PASS** |
| No uGUI on idle UI path | **PASS** |
| Presentation-oriented sandbox binder | **PASS** (`PresentationSystemGroup` + explicit binder) |
| Hub Skin Shop not a second unlock economy | **PASS** (post-impl_08) |

---

## Findings (Round 03)

### Critical

*None.* Dual-stack, FindObjectOfType, and hub dual-economy criticals remain closed.

### Major

1. **Play Mode cosmetics path still not evidenced (carry UI2-01)**  
   - EditMode smoke proves named UI exists; it does **not** prove Cosmetics tab visibility in Play, prestige spend, or `GameProgressData.CurrentSkinIndex` update after buy.  
   - This pass: no Hyper-Casual-Runner editor process; MCP bridge not connected to HCR.  
   - **Impact:** Cannot claim end-to-end cosmetics UX VERIFIED. Does not block static cohesion PASS.

### Minor / Notice

2. **Hub Skin Shop button labels do not refresh after click**  
   - `updateBtnUI` runs once at construction; click path sets `CurrentSkinIndex` then `ReturnToHub()` without re-invoking `updateBtnUI`.  
   - Equipped / Idle Cosmetics labels can stale until hub rebuild (`OnEnable`). Prototype-acceptable.

3. **Hub lists skins 0–2 only** — Emerald Neon (index 3) exists on idle Cosmetics + sandbox UXML but not hub equip list. Intentional asymmetry; document if product wants parity.

4. **Insufficient-funds UX** — locked BUY remains clickable; silent no-op when prestige &lt; cost (live + deferred). No dim/disable.

5. **Deferred sandbox still runner-shaped** — `IdleUIManagerSystem` writes `GoldLabel` from `CurrentRunStats.CurrentGold`. Fine while binder unused; **must** switch to `IdleSliceState` if sandbox revived.

6. **EditMode smoke is structural only** — asserts element names; does not click tabs, fire `CosmeticPurchaseEventComponent`, or cover archetype button sets. Acceptable for UI2-02; thin for wiring regressions.

7. **Smoke tolerates `UI Toolkit.meta` Error** — runtime `PanelSettings` CreateInstance may Error in batchmode; test uses `LogAssert.Expect` / `ignoreFailingMessages`. Residual environment smell (`Assets/UI Toolkit/` present as untracked in git status).

8. **Live HUD remains code + inline styles** — 5 greppable STUBs; intentional prototype.

9. **One-shot cosmetic event entities** — shop disables enableable component, does not destroy (accumulation). Sim concern, not Toolkit rule.

10. **Duplicate LevelSelect UXML** — `LevelSelect.uxml` ≈ `LevelSelectScreen.uxml` (harmless).

11. **`IdleGameHudBinder.Active` last-OnEnable-wins** — edge case; zero instances today.

12. **`RefreshSkinButtons` every `Update`** — cheap for 4 buttons; dirty-check optional.

---

## Component deep-dives (post-R2)

### IdleSliceUIController (live)

**Strengths**
- Sole-authority docs; Actions / Cosmetics tabs; prestige single-event; cosmetics emit `TargetSlice`.
- `EnsureHudBuilt` / `RootVisualElement` enable EditMode surface asserts.
- Prefers bootstrap slice entity for stats + resolve; singleton fallback when count == 1.
- Stub markers greppable (PanelSettings + 4 style sites).

**Weaknesses**
- Code tree + empty `sourceAsset` on all 19 prefabs (by design).
- Kitchen-sink stats; action buttons always enabled; no insufficient-funds feedback on BUY.
- Play Mode path still uninstrumented.

### ToolkitHubManager (adjacent)

**Strengths**
- Dual economy closed: STUB policy, hint label, equip-only clicks, no gold `UnlockSkin`.

**Weaknesses**
- Stale button labels after equip; skin 3 omitted; still inline code UI (fine for hub chrome).

### IdleGameHUD + IdleGameHudBinder + IdleUIManagerSystem (deferred)

**Strengths**
- Explicit opt-in; latch only if Gold/Prestige names present; USS separation; unbind complete; cosmetics set `TargetSlice`.

**Weaknesses**
- Orphan until intentionally revived; runner gold model if revived without currency fix.

### LevelSelect ↔ idle

Policy unchanged and explicit: runner 21 only; idle via Idle prefabs / ToolkitHub. R01 Major #3 stays closed.

### CosmeticsShopSystem (UI consumer)

`IdleEventTarget` + `TargetSlice` path aligns with UI emitters. Shop tests cover TargetSlice spend (`CosmeticsShopTests`); UI smoke does not exercise the shop.

---

## Acceptance criteria (for Round 03 implementers)

| ID | Criterion | Pass if |
| --- | --- | --- |
| UI3-01 | Play Mode cosmetics smoke | Enter Play on ≥1 idle slice prefab → Cosmetics tab visible; buy skin with funded prestige → `GameProgressData.CurrentSkinIndex` matches; log/screenshot or MCP capture attached |
| UI3-02 | Toolkit-only + sole-HUD regression | Idle UI remains UIElements-only; 0 `FindObjectOfType<UIDocument>`; 19/19 idle prefabs still `IdleSliceUIController` without `IdleGameHudBinder` |
| UI3-03 | Hub equip-only holds | Hub Skin Shop still cannot gold-unlock shared skins; STUB comment remains; locked rows do not call `UnlockSkin` |
| UI3-04 | EditMode smoke stays green | `IdleSliceHudSmokeTests` Pass (batchmode acceptable) |
| UI3-05 | Sandbox stay deferred or fix currency | Either binder instance count stays 0, **or** any revived binder reads `IdleSliceState` for primary/prestige labels (not `CurrentRunStats` gold alone) |
| UI3-06 | (Optional) Hub label refresh | After equip click, hub skin buttons reflect EQUIPPED / EQUIP without requiring scene reload |

Retain R01 UI-01..07 and R02 UI2-02..06 as regression checks (must stay PASS / N/A). UI2-01 merges into UI3-01.

---

## Recommended implement order (guidance only)

1. UI3-01 Play Mode evidence (open HCR interactive editor on MCP, or batchmode PlayMode/prefab smoke with log assert).  
2. UI3-02 / UI3-03 / UI3-04 quick regression greps + existing smoke (cheap).  
3. UI3-06 only if touching hub anyway.  
4. UI3-05 only if intentionally reviving sandbox.

---

## Out of scope / deferred

- Full USS migration of live `IdleSliceUIController` (already STUB-marked).  
- Per-archetype UXML variants / second-axis polish.  
- Visual mesh skin application depth (`SkinApplicatorSystem`).  
- Archive / sample Canvas assets.  
- Idle browser inside LevelSelect (already STUB).

---

## Evidence summary

- Commit `9ffe8f3`: EditMode HUD smoke, hub equip-only, sandbox unbind.  
- 19/19 idle prefabs: `IdleSliceUIController`, `sourceAsset: 0`; **0** `IdleGameHudBinder` refs.  
- `IdleSliceHudSmokeTests`: **Passed** (`Logs/IdleHud-TestResults.xml`, 2026-08-10).  
- Hub: `// TODO: [STUB]` dual-economy policy; no gold unlock path.  
- Grep: 0 `FindObjectOfType<UIDocument>`; 0 `using UnityEngine.UI;` in `Assets/Scripts/UI`.  
- MCP `doctor` (this pass): HCR not connected; discovered/registered editor is `thepcgtoolkit` — Play Mode cosmetics **UNVERIFIED**.

**STATUS:** Review complete — static re-audit **VERIFIED**; Play Mode cosmetics **UNVERIFIED**. No implementation. No push.
