# Round 06 Review 08 — UI Toolkit (Idle) Re-Audit

**Agent:** examine/analyze/review 8/10  
**Scope:** Post-`impl_07` (R5) idle UI — live HUD, hub equip-only + label refresh, deferred sandbox policy, LevelSelect, EditMode surface + cosmetics wiring + shop-outcome bridge  
**Prior:** `round_05/review_08_ui.md` → `round_05/impl_07_ui.md` (commit `7f4a071`)  
**Architecture rule:** UI Toolkit only (no legacy uGUI / Canvas for game UI)  
**Mode:** Review only — no implementation, no push

---

## Verdict

**PASS** for idle UI cohesion at the prototype / toolkit quality bar.

Round 05 implement closed the optional shop-outcome gap (UI5-06): fund prestige → Cosmetics BuySkin1 click → tick `CosmeticsShopSystem` → prestige debit + `GameProgressData.CurrentSkinIndex`. Sole live authority remains `IdleSliceUIController` on all 19 idle prefabs; deferred sandbox stays unused; hub remains equip-only with in-session label refresh. Live HUD / prefab / hub / sandbox sources were unchanged by R5 UI impl (tests + receipt only; LoM Farm hide is peer gacha on HEAD).

Retained open item (not cohesion-blocking): **UI5-01 / UI6-01 Play Mode cosmetics still BLOCKED / UNVERIFIED** — Hyper-Casual-Runner has no interactive editor on the MCP bridge this pass (`doctor` → registered sessions empty; discovered interactive editor is `thepcgtoolkit@96e3a310`; HCR appears only as batchmode competitor).

---

## Delta vs Round 05

| R05 finding / UI5 criterion | Post-impl_07 (R5) status |
| --- | --- |
| UI5-01 Play Mode cosmetics smoke | **Still BLOCKED / UNVERIFIED** — no HCR interactive editor / capture |
| UI5-02 Toolkit-only + sole-HUD | **HOLD PASS** — 19/19 live controller; 0 binder; 0 `FindObjectOfType*UIDocument`; 0 `using UnityEngine.UI;` under `Assets/Scripts/UI` |
| UI5-03 Hub equip-only + refresh | **HOLD PASS** — STUB + equip-only click; `RefreshAllSkinButtons`; `UnlockSkin` only in STUB prose |
| UI5-04 EditMode smoke stays green | **HOLD PASS** — fixture now **7** tests, all Passed (`Logs/IdleHud-impl07-r5-TestResults.xml`, `Logs/IdleHud-impl07-r5b.log` exit 0) |
| UI5-05 Sandbox deferred / currency | **N/A PASS** — binder count 0; UI3-05 revive gate still on binder / system / UXML |
| UI5-06 Shop-outcome EditMode bridge | **RESOLVED** — prestige 20→15, unlock skin 1, `CurrentSkinIndex=1`; also asserts `TargetSlice` |
| Major: Play Mode cosmetics | **Still open** — carry as UI6-01 |
| Minor: event-only smoke gap (R5 #4) | **Closed** via UI5-06 |

---

## Inventory (inspected)

| Asset / Type | Path | Role (post-R5) |
| --- | --- | --- |
| Live controller | `Assets/Scripts/UI/IdleSliceUIController.cs` | Sole idle-slice HUD; `EnsureHudBuilt` + `RootVisualElement`; cosmetics → `CosmeticPurchaseEventComponent` + `TargetSlice`; LoM Farm hide at Stage≥1 (peer) |
| HUD smoke | `Assets/Scripts/Editor/Tests/IdleSliceHudSmokeTests.cs` | Named tabs/skin buttons + UI4-06 wiring + UI5-06 shop bridge + combat HUD + LoM Farm peers |
| Binder | `Assets/Scripts/UI/IdleGameHudBinder.cs` | Explicit sandbox registry (`Active`); revive requires IdleSliceState currency |
| Sandbox ECS binder | `Assets/Scripts/UI/IdleUIManagerSystem.cs` | Deferred; unbind + Presentation; runner-gold STUB |
| UXML / USS | `Assets/UI/IdleGameHUD.uxml` + `.uss` | Deferred sandbox + UI3-05 banner |
| Level select | `Assets/Scripts/UI/LevelSelectScreenController.cs` | Runner-only 21; idle policy + STUB |
| Hub | `Assets/Scripts/UI/ToolkitHubManager.cs` | Equip-only Skin Shop; refreshers + skins 0–2 |
| Cosmetics sim | `Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs` | Prestige spend via events (UI consumer) |
| Prefabs | `Assets/ToolkitExamples/Idle/*_Slice.prefab` (**19**/19) | `IdleSliceUIController`; `sourceAsset: 0` |

---

## Round 01–05 criteria re-score

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
| UI2-01 | Play Mode cosmetics smoke | **UNVERIFIED** | Merges into UI6-01; MCP doctor: no HCR registered |
| UI2-02 | EditMode HUD smoke | **PASS** | Surface test still green (fixture total=7) |
| UI2-03 | Hub vs prestige policy | **PASS** | Hub equip-only; gold unlock removed |
| UI2-04 | Sandbox currency or stay deferred | **N/A (PASS)** | 0 `IdleGameHudBinder` on prefabs/scenes |
| UI2-05 | Button unbind | **PASS (static)** | `UnbindButtons` on clear / rebind / `OnStopRunning` |
| UI2-06 | Toolkit-only regression | **PASS** | Same as UI-03/UI-04 |

### R03 UI3-01..06

| ID | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| UI3-01 | Play Mode cosmetics smoke | **UNVERIFIED** | Merges into UI6-01 |
| UI3-02 | Toolkit-only + sole-HUD | **PASS** | Prefab counts + greps (this pass) |
| UI3-03 | Hub equip-only holds | **PASS** | Click path: unlocked → set index; locked → no `UnlockSkin` |
| UI3-04 | EditMode smoke stays green | **PASS** | `Logs/IdleHud-impl07-r5-TestResults.xml` total=7 passed=7 |
| UI3-05 | Sandbox stay deferred or fix currency | **N/A (PASS)** | Binder 0; revive gate documented |
| UI3-06 | Hub label refresh | **PASS (static)** | `_skinButtonRefreshers` + `RefreshAllSkinButtons` before show hub |

### R04 UI4-01..06

| ID | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| UI4-01 | Play Mode cosmetics smoke | **BLOCKED / UNVERIFIED** | Merges into UI6-01 |
| UI4-02 | Toolkit-only + sole-HUD | **PASS** | 19/19 controller; 0 binder; 0 FindObjectOfType; 0 uGUI in UI |
| UI4-03 | Hub equip-only + refresh | **PASS** | STUB retained; equip-only; `RefreshAllSkinButtons` on click |
| UI4-04 | EditMode smoke stays green | **PASS** | Fixture green under UI5-04 / UI6-04 |
| UI4-05 | Sandbox stay deferred or fix currency | **N/A (PASS)** | Binder instance count 0 |
| UI4-06 | Cosmetics wiring EditMode smoke | **PASS** | BuySkin1 → 1× event (TargetSkinIndex=1, PrestigeCost=5.0) |

### R05 UI5-01..06

| ID | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| UI5-01 | Play Mode cosmetics smoke | **BLOCKED / UNVERIFIED** | No HCR interactive MCP; transport pinned to `thepcgtoolkit@96e3a310`; HCR batchmode only |
| UI5-02 | Toolkit-only + sole-HUD | **PASS** | 19/19 controller; 0 binder; 0 FindObjectOfType; 0 uGUI in UI |
| UI5-03 | Hub equip-only + refresh | **PASS** | STUB; equip-only; `RefreshAllSkinButtons` |
| UI5-04 | EditMode smoke stays green | **PASS** | `IdleSliceHudSmokeTests` Passed total=7 (`impl07-r5`) |
| UI5-05 | Sandbox stay deferred or fix currency | **N/A (PASS)** | Binder instance count 0 |
| UI5-06 | Shop-outcome EditMode bridge | **PASS** | Prestige 20→15; unlock skin 1; `CurrentSkinIndex=1`; `TargetSlice` asserted |

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
| EditMode cosmetics click → shop spend / skin index | **PASS** (UI5-06 / post-`7f4a071`) |

---

## Findings (Round 06)

### Critical

*None.* Dual-stack, FindObjectOfType, hub dual-economy, stale hub equip labels, thin named-only smoke, and event-only cosmetics gap remain closed.

### Major

1. **Play Mode cosmetics path still not evidenced (carry UI5-01 → UI6-01)**  
   - EditMode surface + UI4-06 + UI5-06 prove named UI, click → event, and click → shop debit / `CurrentSkinIndex`. They do **not** prove Cosmetics tab visibility in Play, live prestige spend UX, or mesh/`SkinApplicatorSystem` apply after buy.  
   - This pass: MCP `doctor` on `D:\Git\Hyper-Casual-Runner` → editors discovered (`thepcgtoolkit` interactive, `TheCheckout` batchmode, HCR batchmode pid 56912); **registered_sessions empty**; live transport socket pinned to `thepcgtoolkit@96e3a310`, not HCR interactive.  
   - **Impact:** Cannot claim end-to-end cosmetics UX VERIFIED. Does not block static cohesion PASS.

### Minor / Notice

2. **Insufficient-funds UX** — locked BUY remains clickable; silent no-op when prestige &lt; cost (live + deferred). No dim/disable. Unchanged; prototype-acceptable.

3. **Deferred sandbox still runner-shaped** — `IdleUIManagerSystem` writes `GoldLabel` from `CurrentRunStats.CurrentGold`. Documented STUB; **must** switch to `IdleSliceState` if sandbox revived (UI3-05 / UI5-05 / UI6-05 gate holds).

4. **UI5-06 is EditMode, not Play** — closes the event-only gap relative to shop ledger; gap relative to UI6-01 (Play visibility / capture) remains intentional.

5. **Smoke tolerates `UI Toolkit.meta` Error** — runtime `PanelSettings` CreateInstance may Error in batchmode; test uses `LogAssert.Expect` / `ignoreFailingMessages`. Residual environment smell: `Assets/UI Toolkit/` present on disk.

6. **Live HUD remains code + inline styles** — 5 greppable STUBs; intentional prototype.

7. **One-shot cosmetic event entities** — shop disables enableable component, does not destroy (accumulation). Sim concern, not Toolkit rule.

8. **Duplicate LevelSelect UXML** — `LevelSelect.uxml` ≈ `LevelSelectScreen.uxml` (harmless).

9. **`IdleGameHudBinder.Active` last-OnEnable-wins** — edge case; zero instances today.

10. **`RefreshSkinButtons` every `Update`** — cheap for 4 buttons; dirty-check optional.

11. **Hub `ReturnToHub` destroys all ECS entities** — aggressive world wipe when leaving a loaded level; adjacent to UI, not an idle-HUD cohesion failure. Note if Play Mode cosmetics smoke is run via hub load path.

12. **HUD smoke fixture grew via peers** — combat SoT (CH/TT2/IH) + LoM Farm hide share `IdleSliceHudSmokeTests` (7 cases). Cohesion-positive; UI6-04 must keep the full fixture green, not only cosmetics tests.

13. **UI4-06 still omits `TargetSlice` assert** — UI5-06 covers it on the shop-outcome path; wiring-only test remains thinner. Low risk; optional tighten later.

14. **No new R6 cohesion regressions** — post-`7f4a071` surface is EditMode shop bridge + UI5-01 blocked documentation; live controller / prefabs / hub / sandbox source unchanged by UI impl.

---

## Component deep-dives (post-R5)

### IdleSliceUIController (live)

**Strengths**
- Sole-authority docs; Actions / Cosmetics tabs; prestige single-event; cosmetics emit `TargetSlice`.
- `EnsureHudBuilt` / `RootVisualElement` enable EditMode surface + wiring + shop asserts.
- Prefers bootstrap slice entity for stats + resolve; singleton fallback when count == 1.
- Stub markers greppable (PanelSettings + 4 style sites).
- Peer: combat SoT lines + LoM Farm hide at Stage≥1.

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

`IdleEventTarget` + `TargetSlice` path aligns with UI emitters. Shop tests cover TargetSlice spend; UI5-06 now drives shop from HUD click path (EditMode).

### IdleSliceHudSmokeTests (post-R5)

**Strengths**
- Seven-test fixture: structural names + Cosmetics tab display + BuySkin1 → event + shop spend/`CurrentSkinIndex`/`TargetSlice` + combat HUD (3 arches) + LoM Farm hide.
- Documents UI5-01 blocked in class summary.
- Isolated ECS test world SetUp/TearDown for wiring / shop tests.

**Weaknesses**
- Reflection `SimulateClick` (EditMode Clickable quirk) — brittle if Unity renames `clicked` backing field.
- No PlayMode companion.

---

## Acceptance criteria (for Round 06 implementers)

| ID | Criterion | Pass if |
| --- | --- | --- |
| UI6-01 | Play Mode cosmetics smoke | Enter Play on ≥1 idle slice prefab → Cosmetics tab visible; buy skin with funded prestige → `GameProgressData.CurrentSkinIndex` matches; log/screenshot or MCP capture attached |
| UI6-02 | Toolkit-only + sole-HUD regression | Idle UI remains UIElements-only; 0 `FindObjectOfType<UIDocument>`; 19/19 idle prefabs still `IdleSliceUIController` without `IdleGameHudBinder` |
| UI6-03 | Hub equip-only + refresh holds | Hub Skin Shop still cannot gold-unlock shared skins; STUB comment remains; locked rows do not call `UnlockSkin`; equip click still refreshes button labels via `RefreshAllSkinButtons` (or equivalent) |
| UI6-04 | EditMode smoke stays green | `IdleSliceHudSmokeTests` Pass with **all** fixture tests (surface + UI4-06 wiring + UI5-06 shop bridge + peer combat/LoM if still present; batchmode acceptable) |
| UI6-05 | Sandbox stay deferred or fix currency | Either binder instance count stays 0, **or** any revived binder reads `IdleSliceState` for primary/prestige labels (not `CurrentRunStats` gold alone) |

Retain R01 UI-01..07, R02 UI2-02..06, R03 UI3-02..06, R04 UI4-02..06, and R05 UI5-02..06 as regression checks (must stay PASS / N/A). UI5-01 merges into UI6-01. UI5-06 is now regression under UI6-04.

No new optional EditMode substitute this round — UI5-06 already closed the shop-outcome static gap. Next implement value is almost entirely UI6-01 (Play evidence) unless a cohesion regression appears.

---

## Recommended implement order (guidance only)

1. UI6-01 Play Mode evidence (open HCR interactive editor on MCP, or batchmode PlayMode/prefab smoke with log assert).  
2. UI6-02 / UI6-03 / UI6-04 / UI6-05 quick regression greps + existing seven-test smoke (cheap).  
3. Do **not** invent new optional EditMode cosmetics bridges while UI6-01 remains the only open major — avoid gold-plating past the bar.

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
- Combat HUD / LoM Farm verb policy (owned by combat/gacha lanes; hold as shared smoke only).

---

## Evidence summary

- Commit `7f4a071`: UI5-06 Cosmetics shop-outcome EditMode bridge; UI5-01 documented BLOCKED; no live HUD/prefab/hub/sandbox source churn.  
- 19/19 idle prefabs: `IdleSliceUIController`, `sourceAsset: 0`; **0** `IdleGameHudBinder` refs across Assets prefabs/scenes.  
- `IdleSliceHudSmokeTests`: **Passed** total=7 passed=7 failed=0 (`Logs/IdleHud-impl07-r5-TestResults.xml`, start-time 2026-08-10 02:00:20Z; `Logs/IdleHud-impl07-r5b.log` exit 0).  
- Hub: `// TODO: [STUB]` dual-economy policy; equip-only; `RefreshAllSkinButtons`; no gold unlock path.  
- Grep: 0 `FindObjectOfType*UIDocument` calls; 0 `using UnityEngine.UI;` in `Assets/Scripts/UI`.  
- MCP `doctor` (this pass): HCR not connected as interactive; discovered/registered interactive editor is `thepcgtoolkit@96e3a310`; HCR only batchmode — Play Mode cosmetics **BLOCKED / UNVERIFIED**.

**STATUS:** Review complete — static re-audit **VERIFIED**; Play Mode cosmetics **UNVERIFIED**. No implementation. No push.
