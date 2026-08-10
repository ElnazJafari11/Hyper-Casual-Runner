# Round 07 Review 08 — UI Toolkit (Idle) Re-Audit

**Agent:** examine/analyze/review 8/10  
**Scope:** Post-`impl_10` (R6) idle UI — live HUD, hub equip-only + label refresh, deferred sandbox policy, LevelSelect, EditMode surface + cosmetics wiring + shop-outcome bridge  
**Prior:** `round_06/review_08_ui.md` → `round_06/impl_10_ui.md` (commit `c8d31a1`)  
**Peer delta on live HUD (not UI impl):** `8319c9b` (+4 stats lines IH Hero/Dupes + LoM GearSlot)  
**Architecture rule:** UI Toolkit only (no legacy uGUI / Canvas for game UI)  
**Mode:** Review only — no implementation, no push  
**Date:** 2026-08-10

---

## ASSUMPTIONS (reviewer)

1. Quality bar = prototype / toolkit MVP cohesion — high — `docs/project-context.md`.
2. R6 UI impl scope was documentation + smoke header only (no live HUD/prefab/hub/sandbox source in `c8d31a1`) — high — `impl_10_ui.md` + `git show c8d31a1 --stat`.
3. Sole live idle HUD remains `IdleSliceUIController` Option B — high — 19/19 Idle prefabs this pass.
4. UI6-01 Play Mode cosmetics still requires HCR interactive MCP — high — this-pass `doctor` (HCR not discovered/registered).
5. Peer gacha identity HUD lines (`8319c9b`) are gacha-lane owned display, not a dual-stack / Toolkit regression — high — diff is `_stats.text` only under existing `IdleGachaState` block.

---

## Verdict

**PASS** for idle UI cohesion at the prototype / toolkit quality bar.

Round 06 implement held static/EditMode criteria (UI6-02..05) and documented UI6-01 BLOCKED after one doctor — no live controller / prefab / hub / sandbox churn in the UI commit. Sole live authority remains `IdleSliceUIController` on all 19 idle prefabs; deferred sandbox stays unused; hub remains equip-only with in-session label refresh; EditMode fixture (surface + UI4-06 + UI5-06 + combat/LoM peers) last green at 7/7.

Retained open item (not cohesion-blocking): **UI6-01 / UI7-01 Play Mode cosmetics still BLOCKED / UNVERIFIED** — Hyper-Casual-Runner has no interactive editor on the MCP bridge this pass (`doctor` → `registered_sessions` empty; discovered interactive = `thepcgtoolkit@96e3a310`; HCR not among discovered instances; live transport socket pinned to thepcgtoolkit).

Peer notice (not a UI cohesion failure): post-`c8d31a1`, gacha commit `8319c9b` appended IH/LoM identity fields to live HUD stats. Cosmetics / sole-HUD / Toolkit policy unchanged.

---

## Delta vs Round 06

| R06 finding / UI6 criterion | Post-impl_10 (R6) + HEAD status |
| --- | --- |
| UI6-01 Play Mode cosmetics smoke | **Still BLOCKED / UNVERIFIED** — no HCR interactive editor / capture (this-pass doctor) |
| UI6-02 Toolkit-only + sole-HUD | **HOLD PASS** — 19/19 live controller; 0 binder on Idle prefabs; 0 `FindObjectOfType*UIDocument`; 0 `using UnityEngine.UI;` under `Assets/Scripts/UI` |
| UI6-03 Hub equip-only + refresh | **HOLD PASS** — STUB + equip-only click; `RefreshAllSkinButtons`; no `UnlockSkin(` call site |
| UI6-04 EditMode smoke stays green | **HOLD PASS** — fixture **7** tests Passed (`Logs/IdleHud-impl10-r6-TestResults.xml`, `Logs/IdleHud-impl10-r6-a9.log` exit 0). No newer HUD XML after peer `8319c9b`; display-only stats delta — reconfirm cheap for implementer |
| UI6-05 Sandbox deferred / currency | **N/A PASS** — binder instance count 0; UI3-05 revive gate still on binder / system / UXML |
| Major: Play Mode cosmetics | **Still open** — carry as UI7-01 |
| R6 impl value | Documentation-only UI commit (UI6-01 blocked wording + UI6-04 fixture header) — correct non-gold-plate |
| Peer gacha HUD identity lines | **Notice** — IH `HeroId`/`DupeCount`, LoM `GearSlot` on `_stats`; owned by gacha lane |

---

## Inventory (inspected)

| Asset / Type | Path | Role (post-R6) |
| --- | --- | --- |
| Live controller | `Assets/Scripts/UI/IdleSliceUIController.cs` | Sole idle-slice HUD; `EnsureHudBuilt` + `RootVisualElement`; cosmetics → `CosmeticPurchaseEventComponent` + `TargetSlice`; combat SoT + LoM Farm hide; peer IH/LoM identity stats |
| HUD smoke | `Assets/Scripts/Editor/Tests/IdleSliceHudSmokeTests.cs` | Named tabs/skin buttons + UI4-06 wiring + UI5-06 shop bridge + combat HUD + LoM Farm; docs UI6-01 blocked / UI6-04 |
| Binder | `Assets/Scripts/UI/IdleGameHudBinder.cs` | Explicit sandbox registry (`Active`); revive requires IdleSliceState currency |
| Sandbox ECS binder | `Assets/Scripts/UI/IdleUIManagerSystem.cs` | Deferred; unbind + Presentation; runner-gold STUB |
| UXML / USS | `Assets/UI/IdleGameHUD.uxml` + `.uss` | Deferred sandbox + UI3-05 banner |
| Level select | `Assets/Scripts/UI/LevelSelectScreenController.cs` | Runner-only 21; idle policy + STUB |
| Hub | `Assets/Scripts/UI/ToolkitHubManager.cs` | Equip-only Skin Shop; refreshers + skins 0–2 |
| Cosmetics sim | `Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs` | Prestige spend via events (UI consumer) |
| Prefabs | `Assets/ToolkitExamples/Idle/*_Slice.prefab` (**19**/19) | `IdleSliceUIController`; `sourceAsset: 0` |

---

## Round 01–06 criteria re-score

### R01 UI-01..07 (regression)

| ID | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| UI-01 | Single idle HUD authority | **PASS** | Option B docs; 19/19 prefabs live controller; sandbox deferred + unused |
| UI-02 | Cosmetics reachable in play | **PASS (static + EditMode wiring + shop) / UNVERIFIED (play)** | Cosmetics tab + `FireCosmetic` → event; UI4-06 + UI5-06; no Play capture |
| UI-03 | No FindObjectOfType UI bind | **PASS** | 0 matches under `Assets/Scripts` |
| UI-04 | Toolkit-only idle path | **PASS** | UIElements only on idle UI scripts |
| UI-05 | Styles extracted or stubbed | **PASS** | Sandbox USS linked; live HUD **5×** `// TODO: [STUB]` |
| UI-06 | LevelSelect idle policy explicit | **PASS** | Class summary + STUB idle browser |
| UI-07 | Label queries cached | **PASS** | Sandbox `BindOnce`; live `_stats` / `_skinButtons` |

### R02 UI2-01..06

| ID | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| UI2-01 | Play Mode cosmetics smoke | **UNVERIFIED** | Merges into UI7-01; MCP doctor: no HCR registered |
| UI2-02 | EditMode HUD smoke | **PASS** | Surface test still green (fixture total=7) |
| UI2-03 | Hub vs prestige policy | **PASS** | Hub equip-only; gold unlock removed |
| UI2-04 | Sandbox currency or stay deferred | **N/A (PASS)** | 0 `IdleGameHudBinder` on Idle prefabs |
| UI2-05 | Button unbind | **PASS (static)** | `UnbindButtons` on clear / rebind / `OnStopRunning` |
| UI2-06 | Toolkit-only regression | **PASS** | Same as UI-03/UI-04 |

### R03 UI3-01..06

| ID | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| UI3-01 | Play Mode cosmetics smoke | **UNVERIFIED** | Merges into UI7-01 |
| UI3-02 | Toolkit-only + sole-HUD | **PASS** | Prefab counts + greps (this pass) |
| UI3-03 | Hub equip-only holds | **PASS** | Click path: unlocked → set index; locked → no `UnlockSkin` |
| UI3-04 | EditMode smoke stays green | **PASS** | `Logs/IdleHud-impl10-r6-TestResults.xml` total=7 passed=7 |
| UI3-05 | Sandbox stay deferred or fix currency | **N/A (PASS)** | Binder 0; revive gate documented |
| UI3-06 | Hub label refresh | **PASS (static)** | `_skinButtonRefreshers` + `RefreshAllSkinButtons` before show hub |

### R04 UI4-01..06

| ID | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| UI4-01 | Play Mode cosmetics smoke | **BLOCKED / UNVERIFIED** | Merges into UI7-01 |
| UI4-02 | Toolkit-only + sole-HUD | **PASS** | 19/19 controller; 0 binder; 0 FindObjectOfType; 0 uGUI in UI |
| UI4-03 | Hub equip-only + refresh | **PASS** | STUB retained; equip-only; `RefreshAllSkinButtons` on click |
| UI4-04 | EditMode smoke stays green | **PASS** | Fixture green under UI6-04 / UI7-04 |
| UI4-05 | Sandbox stay deferred or fix currency | **N/A (PASS)** | Binder instance count 0 |
| UI4-06 | Cosmetics wiring EditMode smoke | **PASS** | BuySkin1 → 1× event (TargetSkinIndex=1, PrestigeCost=5.0) |

### R05 UI5-01..06

| ID | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| UI5-01 | Play Mode cosmetics smoke | **BLOCKED / UNVERIFIED** | Merges into UI7-01 |
| UI5-02 | Toolkit-only + sole-HUD | **PASS** | Same greps / prefab counts |
| UI5-03 | Hub equip-only + refresh | **PASS** | STUB; equip-only; `RefreshAllSkinButtons` |
| UI5-04 | EditMode smoke stays green | **PASS** | Held under UI6-04 evidence |
| UI5-05 | Sandbox stay deferred or fix currency | **N/A (PASS)** | Binder instance count 0 |
| UI5-06 | Shop-outcome EditMode bridge | **PASS** | Prestige debit + unlock + `CurrentSkinIndex` + `TargetSlice` under UI6-04 fixture |

### R06 UI6-01..05

| ID | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| UI6-01 | Play Mode cosmetics smoke | **BLOCKED / UNVERIFIED** | This-pass MCP `doctor`: no HCR interactive; pin `thepcgtoolkit@96e3a310`; `registered_sessions=[]` |
| UI6-02 | Toolkit-only + sole-HUD | **PASS** | 19/19 controller; 0 Idle binder; 0 FindObjectOfType; 0 uGUI in UI |
| UI6-03 | Hub equip-only + refresh | **PASS** | STUB; no `UnlockSkin(`; `RefreshAllSkinButtons` |
| UI6-04 | EditMode smoke stays green | **PASS** | `IdleSliceHudSmokeTests` Passed total=7 (`impl10-r6`); smoke class docs UI6-01 / UI6-04 |
| UI6-05 | Sandbox stay deferred or fix currency | **N/A (PASS)** | Binder instance count 0 |

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
| Hub Skin Shop not a second unlock economy | **PASS** |
| Hub equip labels refresh in-session | **PASS** |
| EditMode cosmetics click → purchase event | **PASS** (UI4-06) |
| EditMode cosmetics click → shop spend / skin index | **PASS** (UI5-06 / held under UI6-04) |
| R6 UI commit avoided gold-plate EditMode bridges | **PASS** (docs only while UI6-01 blocked) |

---

## Findings (Round 07)

### Critical

*None.* Dual-stack, FindObjectOfType, hub dual-economy, stale hub equip labels, thin named-only smoke, and event-only cosmetics gap remain closed.

### Major

1. **Play Mode cosmetics path still not evidenced (carry UI6-01 → UI7-01)**  
   - EditMode surface + UI4-06 + UI5-06 prove named UI, click → event, and click → shop debit / `CurrentSkinIndex`. They do **not** prove Cosmetics tab visibility in Play, live prestige spend UX, or mesh/`SkinApplicatorSystem` apply after buy.  
   - This pass: MCP `doctor` on `D:\Git\Hyper-Casual-Runner` → editors: `thepcgtoolkit` interactive, `TheCheckout` batchmode, plus anonymous pid with `project_path=null`; **registered_sessions empty**; discovered interactive = `thepcgtoolkit@96e3a310`; HCR **not** in discovered instances; live transport socket pinned to thepcgtoolkit.  
   - **Impact:** Cannot claim end-to-end cosmetics UX VERIFIED. Does not block static cohesion PASS.

### Minor / Notice

2. **Insufficient-funds UX** — locked BUY remains clickable; silent no-op when prestige &lt; cost (live + deferred). Unchanged; prototype-acceptable.

3. **Deferred sandbox still runner-shaped** — `IdleUIManagerSystem` writes `GoldLabel` from `CurrentRunStats.CurrentGold`. Documented STUB; **must** switch to `IdleSliceState` if sandbox revived (UI3-05 / UI6-05 / UI7-05 gate holds).

4. **UI5-06 is EditMode, not Play** — closes the event-only gap relative to shop ledger; gap relative to UI7-01 (Play visibility / capture) remains intentional.

5. **Smoke tolerates `UI Toolkit.meta` Error** — runtime `PanelSettings` CreateInstance may Error in batchmode; test uses `LogAssert.Expect` / `ignoreFailingMessages`. Residual environment smell: `Assets/UI Toolkit/` present on disk.

6. **Live HUD remains code + inline styles** — 5 greppable STUBs; intentional prototype.

7. **One-shot cosmetic event entities** — shop disables enableable component, does not destroy (accumulation). Sim concern, not Toolkit rule.

8. **Duplicate LevelSelect UXML** — `LevelSelect.uxml` ≈ `LevelSelectScreen.uxml` (harmless).

9. **`IdleGameHudBinder.Active` last-OnEnable-wins** — edge case; zero instances today.

10. **`RefreshSkinButtons` every `Update`** — cheap for 4 buttons; dirty-check optional.

11. **Hub `ReturnToHub` destroys all ECS entities** — aggressive world wipe when leaving a loaded level; adjacent to UI, not an idle-HUD cohesion failure. Note if Play Mode cosmetics smoke is run via hub load path.

12. **HUD smoke fixture grew via peers** — combat SoT (CH/TT2/IH) + LoM Farm hide share `IdleSliceHudSmokeTests` (7 cases). Cohesion-positive; UI7-04 must keep the full fixture green, not only cosmetics tests.

13. **UI4-06 still omits `TargetSlice` assert** — UI5-06 covers it on the shop-outcome path; wiring-only test remains thinner. Low risk; optional tighten later.

14. **No new R7 cohesion regressions from UI impl** — post-`c8d31a1` UI surface is blocked documentation + smoke header; live controller / prefabs / hub / sandbox **source unchanged by UI impl**.

15. **Peer gacha identity HUD lines (`8319c9b`)** — IH shows `HeroId`/`DupeCount`, LoM shows `GearSlot` on stats. Does not open dual HUD / binder / uGUI paths. Last EditMode HUD XML predates this commit; implementer should cheap-reconfirm UI7-04 if any doubt.

---

## Component deep-dives (post-R6)

### IdleSliceUIController (live)

**Strengths**
- Sole-authority docs; Actions / Cosmetics tabs; prestige single-event; cosmetics emit `TargetSlice`.
- `EnsureHudBuilt` / `RootVisualElement` enable EditMode surface + wiring + shop asserts.
- Prefers bootstrap slice entity for stats + resolve; singleton fallback when count == 1.
- Stub markers greppable (PanelSettings + 4 style sites).
- Peer: combat SoT lines + LoM Farm hide + IH/LoM identity stats.

**Weaknesses**
- Code tree + empty `sourceAsset` on all 19 prefabs (by design).
- Kitchen-sink stats; action buttons always enabled; no insufficient-funds feedback on BUY.
- Play Mode path still uninstrumented.

### ToolkitHubManager (adjacent)

**Strengths**
- Dual economy closed: STUB policy, hint label, equip-only clicks, no gold `UnlockSkin`.
- UI3-06 hold: `_skinButtonRefreshers` + `RefreshAllSkinButtons` updates EQUIPPED / EQUIP / Idle Cosmetics labels.
- Skins 0–2 asymmetry documented (index 3 Idle Cosmetics–only).

**Weaknesses**
- Still inline code UI (fine for hub chrome).
- Skin 3 omitted from hub list (documented intentional).

### IdleGameHUD + IdleGameHudBinder + IdleUIManagerSystem (deferred)

**Strengths**
- Explicit opt-in; latch only if Gold/Prestige names present; USS separation; unbind complete; cosmetics set `TargetSlice`.
- UI3-05 revive gate written on binder summary, system STUB, and UXML banner.

**Weaknesses**
- Orphan until intentionally revived; runner gold model if revived without currency fix.

### LevelSelect ↔ idle

Policy unchanged and explicit: runner 21 only; idle via Idle prefabs / ToolkitHub. R01 Major #3 stays closed.

### CosmeticsShopSystem (UI consumer)

`IdleEventTarget` + `TargetSlice` path aligns with UI emitters. Shop tests cover TargetSlice spend; UI5-06 drives shop from HUD click path (EditMode).

### IdleSliceHudSmokeTests (post-R6)

**Strengths**
- Seven-test fixture: structural names + Cosmetics tab display + BuySkin1 → event + shop spend/`CurrentSkinIndex`/`TargetSlice` + combat HUD (3 arches) + LoM Farm hide.
- Documents UI6-01 blocked + UI6-04 in class summary (post-`c8d31a1`).
- Isolated ECS test world SetUp/TearDown for wiring / shop tests.

**Weaknesses**
- Reflection `SimulateClick` (EditMode Clickable quirk) — brittle if Unity renames `clicked` backing field.
- No PlayMode companion.
- Latest green XML is R6 impl10; peer identity HUD text not asserted (fine).

---

## Acceptance criteria (for Round 07 implementers)

| ID | Criterion | Pass if |
| --- | --- | --- |
| UI7-01 | Play Mode cosmetics smoke | Enter Play on ≥1 idle slice prefab → Cosmetics tab visible; buy skin with funded prestige → `GameProgressData.CurrentSkinIndex` matches; log/screenshot or MCP capture attached |
| UI7-02 | Toolkit-only + sole-HUD regression | Idle UI remains UIElements-only; 0 `FindObjectOfType<UIDocument>`; 19/19 idle prefabs still `IdleSliceUIController` without `IdleGameHudBinder` |
| UI7-03 | Hub equip-only + refresh holds | Hub Skin Shop still cannot gold-unlock shared skins; STUB comment remains; locked rows do not call `UnlockSkin`; equip click still refreshes button labels via `RefreshAllSkinButtons` (or equivalent) |
| UI7-04 | EditMode smoke stays green | `IdleSliceHudSmokeTests` Pass with **all** fixture tests (surface + UI4-06 wiring + UI5-06 shop bridge + peer combat/LoM if still present; batchmode acceptable). Reconfirm after peer HUD churn if no post-`8319c9b` XML exists |
| UI7-05 | Sandbox stay deferred or fix currency | Either binder instance count stays 0, **or** any revived binder reads `IdleSliceState` for primary/prestige labels (not `CurrentRunStats` gold alone) |

Retain R01 UI-01..07, R02 UI2-02..06, R03 UI3-02..06, R04 UI4-02..06, R05 UI5-02..06, and R06 UI6-02..05 as regression checks (must stay PASS / N/A). UI6-01 merges into UI7-01. UI5-06 remains regression under UI7-04.

No new optional EditMode substitute this round — UI5-06 already closed the shop-outcome static gap; R6 correctly refused further EditMode bridges. Next implement value is almost entirely UI7-01 (Play evidence) unless a cohesion regression appears.

---

## Recommended implement order (guidance only)

1. UI7-01 Play Mode evidence (open HCR interactive editor on MCP, or batchmode PlayMode/prefab smoke with log assert).  
2. UI7-02 / UI7-03 / UI7-04 / UI7-05 quick regression greps + existing seven-test smoke (cheap; refresh UI7-04 XML if peer churn since last green).  
3. Do **not** invent new optional EditMode cosmetics bridges while UI7-01 remains the only open major — avoid gold-plating past the bar.

---

## Out of scope / deferred

- Full USS migration of live `IdleSliceUIController` (already STUB-marked).  
- Per-archetype UXML variants / second-axis polish.  
- Visual mesh skin application depth (`SkinApplicatorSystem`).  
- Archive / sample Canvas assets.  
- Idle browser inside LevelSelect (already STUB).  
- Insufficient-funds dim/disable UX (prototype-acceptable).  
- Hub world-wipe behavior on `ReturnToHub` (adjacent, not idle HUD stack).  
- Asserting `TargetSlice` in UI4-06 wiring-only test (optional; already covered by UI5-06).  
- Combat HUD / LoM Farm / gacha identity verb policy (owned by combat/gacha lanes; hold as shared smoke / peer display only).

---

## Evidence summary

- Commit `c8d31a1`: UI6-01 Play cosmetics BLOCKED documented after one doctor; UI6-04 fixture header; no live HUD/prefab/hub/sandbox source churn.  
- Peer `8319c9b`: +4 lines IH/LoM identity on live HUD `_stats` (gacha-owned; Toolkit cohesion unchanged).  
- 19/19 idle prefabs: `IdleSliceUIController`, `sourceAsset: 0`; **0** `IdleGameHudBinder` on Idle prefabs.  
- `IdleSliceHudSmokeTests`: **Passed** total=7 passed=7 failed=0 (`Logs/IdleHud-impl10-r6-TestResults.xml`, start-time 2026-08-10 02:23:23Z; `Logs/IdleHud-impl10-r6-a9.log` exit 0).  
- Hub: `// TODO: [STUB]` dual-economy policy; equip-only; `RefreshAllSkinButtons`; no `UnlockSkin(` call.  
- Grep: 0 `FindObjectOfType*UIDocument` calls; 0 `using UnityEngine.UI;` in `Assets/Scripts/UI`.  
- MCP `doctor` (this pass): HCR not connected as interactive; discovered/registered interactive editor is `thepcgtoolkit@96e3a310`; `registered_sessions=[]` — Play Mode cosmetics **BLOCKED / UNVERIFIED**.

**STATUS:** Review complete — static re-audit **VERIFIED**; Play Mode cosmetics **UNVERIFIED**. No implementation. No push.
