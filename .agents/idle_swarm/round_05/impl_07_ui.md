# Round 05 Implement 07 — UI Toolkit (Idle)

**Agent:** implement 7/10 (retry after stalled relaunch `12eb30a3`)  
**Source review:** `round_05/review_08_ui.md`  
**Architecture:** UI Toolkit only  
**Commit:** _(filled after local commit)_  
**Push:** never (local commit only)  
**Play:** no thrash — UI5-01 documented BLOCKED only

---

## ASSUMPTIONS

1. Live idle HUD remains `IdleSliceUIController` (Option B) — confidence: **high** — verified 19/19 Idle prefabs; 0 `IdleGameHudBinder`.
2. Hyper-Casual-Runner interactive editor not on MCP bridge — confidence: **high** — `doctor` → registered_sessions empty; discovered interactive editor is `thepcgtoolkit@96e3a310`; HCR appears only as competing batchmode.
3. UI5-01 Play Mode cosmetics remains blocked without HCR interactive MCP — confidence: **high** — same doctor evidence as review; user: no Play thrash.
4. UI5-06 EditMode shop-outcome bridge is the approved static substitute while UI5-01 is blocked — confidence: **high** — review recommended order item 3.
5. Concurrent swarm may touch HUD smoke / LoM Farm — confidence: **high** — UI5-06 re-applied after peer overwrite; commit scopes only this receipt + UI5-06 test delta.

---

## Changes

| File | Change |
| --- | --- |
| `Assets/Scripts/Editor/Tests/IdleSliceHudSmokeTests.cs` | UI5-06: fund prestige → Cosmetics BuySkin1 → tick `CosmeticsShopSystem` → assert debit + `CurrentSkinIndex`; class summary documents UI5-01 blocked |
| `.agents/idle_swarm/round_05/impl_07_ui.md` | This receipt |

No live HUD / prefab / hub / sandbox source changes. LoM Farm hide already on HEAD (`f07ee0c`); smoke fixture also carries that peer test.

---

## Acceptance criteria

| ID | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| UI5-01 | Play Mode cosmetics smoke | **BLOCKED / UNVERIFIED** | MCP `doctor`: HCR not registered; live transport pinned to `thepcgtoolkit@96e3a310`; discovered interactive = `thepcgtoolkit`; HCR only batchmode competitors. No Play enter / Cosmetics capture / live `CurrentSkinIndex` after buy. |
| UI5-02 | Toolkit-only + sole-HUD regression | **PASS** | Idle prefabs **19/19** `IdleSliceUIController`, **0** `IdleGameHudBinder`; 0 `FindObjectOfType*UIDocument` under `Assets/Scripts`; 0 `using UnityEngine.UI;` under `Assets/Scripts/UI` |
| UI5-03 | Hub equip-only + refresh holds | **PASS** | STUB dual-economy comment retained; `UnlockSkin` only in STUB prose; `RefreshAllSkinButtons()` still called on equip click |
| UI5-04 | EditMode smoke stays green | **PASS** | `IdleSliceHudSmokeTests` → surface + cosmetics-wiring + UI5-06 shop bridge green (see Verification); fixture also carries R5 combat HUD + LoM Farm peers |
| UI5-05 | Sandbox stay deferred or fix currency | **N/A (PASS)** | Binder instance count **0** on Idle prefabs |
| UI5-06 | (Optional) Shop-outcome EditMode bridge | **PASS** | `IdleSliceUIController_CosmeticsBuy_ShopSpendsPrestigeAndSetsCurrentSkin`: prestige 20→15, unlock skin 1, `CurrentSkinIndex=1` after shop tick |

R01 UI-01..07, R02 UI2-02..06, R03 UI3-02..06, R04 UI4-02..06 retained as regression — sole live HUD, hub equip-only + refresh, binder unused, Toolkit-only, UI4-06 wiring unchanged.

---

## Static leftovers closed / documented

| R05 finding / item | Disposition |
| --- | --- |
| UI5-01 Play Mode cosmetics | **Documented BLOCKED** — no HCR interactive editor on MCP; no Play thrash |
| UI4-06 event-only gap (finding #4) | **Closed via UI5-06** — click → shop tick → prestige debit + `CurrentSkinIndex` |
| Hub equip-only + refresh / sole-HUD / Toolkit-only / deferred sandbox | **Hold PASS** — regression greps + existing smoke |
| Insufficient-funds UX, USS migration, LevelSelect idle browser, event entity destroy | Out of scope (per review) |

---

## UI5-01 blocked — how to verify later

Open Hyper-Casual-Runner in an interactive editor registered on MCP (or local Play), then:

1. Enter Play on any `Assets/ToolkitExamples/Idle/*_Slice.prefab`
2. Open Cosmetics tab
3. Buy a locked skin with funded prestige
4. Assert `GameProgressData.CurrentSkinIndex` matches; attach MCP `capture` / log

Until then: EditMode UI5-06 proves click → shop spend / skin index (not Play Mode UX / mesh apply).

---

## Verification

```
# MCP doctor (HCR) — UI5-01 blocker
editors discovered: thepcgtoolkit (interactive) + TheCheckout (batchmode) + HCR batchmode competitors
HCR interactive / registered: none → UI5-01 BLOCKED / UNVERIFIED
live transport: socket pinned to thepcgtoolkit@96e3a310

# Regression greps
Idle/*_Slice.prefab IdleSliceUIController: 19
Idle/*_Slice.prefab IdleGameHudBinder: 0
FindObjectOfType*UIDocument in Assets/Scripts: 0
using UnityEngine.UI; in Assets/Scripts/UI: 0
ToolkitHubManager UnlockSkin: STUB prose only
ToolkitHubManager RefreshAllSkinButtons: present (call + method)

# EditMode HUD smoke (batchmode)
Unity 6000.5.5f1 -batchmode -nographics -projectPath D:\Git\Hyper-Casual-Runner
  -runTests -testPlatform EditMode
  -testFilter HyperCasualRunner.Tests.IdleSliceHudSmokeTests
  -testResults Logs/IdleHud-impl07-r5-TestResults.xml
  -logFile Logs/IdleHud-impl07-r5b.log
→ Test run completed. Exiting with code 0 (Ok).
→ result=Passed total=7 passed=7 failed=0 (start-time 2026-08-10 02:00:20Z)
→ IdleSliceUIController_CosmeticsBuy_ShopSpendsPrestigeAndSetsCurrentSkin => Passed
→ IdleSliceUIController_CosmeticsBuy_EmitsCosmeticPurchaseEvent => Passed
→ IdleSliceUIController_EnsureHudBuilt_CreatesNamedTabsAndSkinButtons => Passed
```

**Play Mode cosmetics:** **BLOCKED / UNVERIFIED**.

---

## STATUS

**PARTIAL VERIFIED** — UI5-02..06 static/EditMode **VERIFIED**; UI5-01 Play Mode cosmetics **BLOCKED / UNVERIFIED**.
