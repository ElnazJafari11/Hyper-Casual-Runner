# Round 03 Implement 08 — UI Toolkit (Idle)

**Agent:** implement 8/10  
**Source review:** `round_03/review_08_ui.md`  
**Architecture:** UI Toolkit only  
**Commit:** `bd8d4fc` local only (no push)

---

## ASSUMPTIONS

1. Live idle HUD remains `IdleSliceUIController` (Option B) — confidence: **high** — 19/19 Idle prefabs; 0 `IdleGameHudBinder`.
2. Hyper-Casual-Runner interactive editor not on MCP bridge — confidence: **high** — `doctor` → no HCR registered; discovered editor is `thepcgtoolkit`; HCR used batchmode only this pass.
3. UI3-05 stays deferred (binder unused) rather than reviving sandbox currency — confidence: **high** — review guidance + 0 binder instances.
4. Optional UI3-06 hub label refresh is in scope when touching hub docs — confidence: **high** — review recommended if touching hub anyway.

---

## Changes

| File | Change |
| --- | --- |
| `Assets/Scripts/UI/ToolkitHubManager.cs` | UI3-06: `RefreshAllSkinButtons` after equip; document skins 0–2 hub list (index 3 Idle Cosmetics–only); STUB dual-economy policy retained |
| `Assets/Scripts/UI/IdleUIManagerSystem.cs` | UI3-05: document runner-shaped sandbox currency STUB; do not revive without `IdleSliceState` labels |
| `Assets/Scripts/UI/IdleGameHudBinder.cs` | UI3-05: binder summary requires IdleSliceState currency switch before attachment; live instance count stays 0 |
| `Assets/UI/IdleGameHUD.uxml` | UI3-05: banner documents deferred currency gate before binder revival |

No live HUD / prefab / smoke-test source changes.

---

## Acceptance criteria

| ID | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| UI3-01 | Play Mode cosmetics smoke | **UNVERIFIED** | MCP `doctor`: HCR not registered; live transport pinned to `thepcgtoolkit`. No Play Mode capture. Static Cosmetics tab + `CosmeticPurchaseEventComponent` unchanged. |
| UI3-02 | Toolkit-only + sole-HUD regression | **PASS** | 0 `FindObjectOfType*UIDocument` under `Assets/Scripts`; 0 `using UnityEngine.UI;` under `Assets/Scripts/UI`; Idle prefabs **19/19** `IdleSliceUIController`, **0** `IdleGameHudBinder` |
| UI3-03 | Hub equip-only holds | **PASS** | STUB comment retained; click path equip-only; `UnlockSkin` appears only in STUB prose (no gold unlock call) |
| UI3-04 | EditMode smoke stays green | **PASS** | `IdleSliceHudSmokeTests` → `result=Passed passed=1 failed=0` (`Logs/IdleHud-TestResults.xml`, log `Logs/IdleHud-impl08-r3.log`, exit 0) |
| UI3-05 | Sandbox stay deferred or fix currency | **N/A (PASS)** | Binder instance count **0**; deferred currency STUB documented on system + UXML + binder |
| UI3-06 | Hub label refresh (optional) | **PASS (static)** | Equip click invokes `RefreshAllSkinButtons()` before `ReturnToHub()` |

R01 UI-01..07 and R02 UI2-02..06 retained as regression — sole live HUD, hub equip-only, binder unused, unbind, Toolkit-only unchanged in intent.

---

## Static leftovers closed / documented

| R03 finding | Disposition |
| --- | --- |
| Hub button labels stale after equip | **Closed** — UI3-06 refresh |
| Hub lists skins 0–2 only | **Documented** — intentional asymmetry STUB |
| Deferred sandbox `CurrentRunStats` gold | **Documented** — stay deferred (UI3-05); revive gate written |
| Play Mode cosmetics | **Left UNVERIFIED** — no HCR interactive editor |

Not in scope this pass: insufficient-funds UX, event entity destroy, full USS migration, LevelSelect idle browser.

---

## Verification

```
# MCP doctor (HCR)
editors discovered: thepcgtoolkit (+ unrelated batchmode projects)
HCR interactive / registered: none → UI3-01 UNVERIFIED

# Regression greps
Idle/*_Slice.prefab IdleSliceUIController: 19
Idle/*_Slice.prefab IdleGameHudBinder: 0
FindObjectOfType*UIDocument in Assets/Scripts: 0
using UnityEngine.UI; in Assets/Scripts/UI: 0

# EditMode HUD smoke (batchmode)
Unity 6000.5.5f1 -batchmode -nographics -projectPath D:\Git\Hyper-Casual-Runner
  -runTests -testPlatform EditMode
  -testFilter HyperCasualRunner.Tests.IdleSliceHudSmokeTests
→ Test run completed. Exiting with code 0 (Ok).
→ result=Passed total=1 passed=1 failed=0 (start-time 2026-08-10 00:34:09Z)
```

**Play Mode cosmetics:** **UNVERIFIED**.  
To verify: open any `Assets/ToolkitExamples/Idle/*_Slice.prefab` in an HCR interactive editor on MCP → Enter Play → Cosmetics tab → buy with funded prestige → `GameProgressData.CurrentSkinIndex` updates; attach capture/log.

---

## STATUS

**PARTIAL VERIFIED** — UI3-02..06 static/EditMode **VERIFIED**; UI3-01 Play Mode cosmetics **UNVERIFIED**.
