# Round 08 Implement 10 — UI Toolkit (Idle)

**Agent:** implement 10/10  
**Source review:** `round_08/review_08_ui.md`  
**Architecture:** UI Toolkit only  
**Push:** never (local commit only)  
**Play:** no thrash — UI8-01 one `doctor` max, then documented BLOCKED

---

## ASSUMPTIONS

1. Live idle HUD remains `IdleSliceUIController` (Option B) — confidence: **high** — verified 19/19 Idle prefabs; 0 `IdleGameHudBinder` on Idle prefabs.
2. Hyper-Casual-Runner interactive editor not on MCP bridge — confidence: **high** — single `doctor` this pass (`project_path=D:\Git\Hyper-Casual-Runner`): `registered_sessions=[]`; discovered interactive = `thepcgtoolkit@96e3a310`; HCR present only as batchmode pid 50380 (`RunAllIdleSmokeAndExit`); live transport socket pinned to `thepcgtoolkit`.
3. UI8-01 Play Mode cosmetics remains blocked without HCR interactive MCP — confidence: **high** — doctor evidence; user: one doctor max, no Play thrash.
4. No new optional EditMode cosmetics bridge this round — confidence: **high** — review: avoid gold-plating past UI5-06 while UI8-01 is the only open major.
5. Optional UI8-04 Capybara reconfirm cannot start a second HCR batchmode while peer AllSmoke holds the project lock — confidence: **high** — pid 50380 + import worker on HCR; prior R7 abort pattern.

---

## Changes

| File | Change |
| --- | --- |
| `.agents/idle_swarm/round_08/impl_10_ui.md` | This receipt (implement 10/10) |

Already on HEAD via peer `812da1d` (LLM/UI compose seat, same session): `IdleSliceHudSmokeTests.cs` UI8-01/UI8-04 wording; `review_08_ui.md`. No live HUD / prefab / hub / sandbox source changes in this commit.

---

## Acceptance criteria

| ID | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| UI8-01 | Play Mode cosmetics smoke | **BLOCKED / UNVERIFIED** | One MCP `doctor` (`project_path=D:\Git\Hyper-Casual-Runner`): `registered_sessions=[]`; discovered interactive `thepcgtoolkit@96e3a310`; HCR not interactive on bridge (batchmode pid 50380 only; remediations cite none REGISTERED + contention). No Play enter / Cosmetics capture / live `CurrentSkinIndex`. |
| UI8-02 | Toolkit-only + sole-HUD regression | **PASS** | Idle prefabs **19/19** `IdleSliceUIController`, **0** `IdleGameHudBinder`; 0 `FindObjectOfType*UIDocument` under `Assets/Scripts`; 0 `using UnityEngine.UI;` under `Assets/Scripts/UI` |
| UI8-03 | Hub equip-only + refresh holds | **PASS** | STUB dual-economy comment retained; `UnlockSkin(` call sites in hub: **none** (STUB prose only); equip click still calls `RefreshAllSkinButtons()` |
| UI8-04 | EditMode smoke stays green | **PASS (prior green + static Capybara reconfirm)** | Fixture last fully green: `IdleSliceHudSmokeTests` total=7 passed=7 (`Logs/IdleHud-impl10-r6-TestResults.xml`, pre-`09b9b72`). Fresh HUD batchmode **not started** — HCR locked by peer `IdleAllSmoke-impl01-r7` (pid 50380). Optional Capybara cheap reconfirm: static hold on live controller — Fight Path / Safe Path / Lucky Find → `FireNarrative(3/4/5)`; stats `RunId` / Choice / `PetId`; Capybara prefab still `IdleSliceUIController` only. No post-`09b9b72` HUD XML this seat. |
| UI8-05 | Sandbox stay deferred or fix currency | **N/A (PASS)** | Binder instance count **0** on Idle prefabs |

R01 UI-01..07, R02 UI2-02..06, R03 UI3-02..06, R04 UI4-02..06, R05 UI5-02..06, R06 UI6-02..05, R07 UI7-02..05 retained as regression — sole live HUD, hub equip-only + refresh, binder unused, Toolkit-only, UI4-06 + UI5-06 EditMode paths under prior green fixture.

---

## UI8-01 blocked — how to verify later

Open Hyper-Casual-Runner in an interactive editor registered on MCP (dismiss any Safe Mode / recovery dialog first), then:

1. Enter Play on any `Assets/ToolkitExamples/Idle/*_Slice.prefab`
2. Open Cosmetics tab
3. Buy a locked skin with funded prestige
4. Assert `GameProgressData.CurrentSkinIndex` matches; attach MCP `capture` / log

Until then: EditMode UI4-06 + UI5-06 prove click → event → shop spend / skin index (not Play Mode UX / mesh apply).

---

## Deviations

- Optional UI8-04 fresh EditMode XML after peer Capybara HUD churn (`09b9b72`) skipped — peer AllSmoke batchmode holds HCR project lock. Did not thrash Play; did not run a second doctor. Relied on R6 green XML + static greps + Capybara controller/prefab static reconfirm.

---

## Verification

```
# MCP doctor (HCR) — UI8-01 one-shot
editors: thepcgtoolkit (interactive) + TheCheckout (batchmode) + HCR (batchmode pid 50380 RunAllIdleSmokeAndExit)
HCR interactive / registered: none → UI8-01 BLOCKED / UNVERIFIED
live transport: socket pinned to thepcgtoolkit@96e3a310
registered_sessions: []
machine: editor_count=3, cpu ~94–100%

# Regression greps
Idle/*_Slice.prefab count: 19
Idle/*_Slice.prefab IdleSliceUIController: 19
Idle/*_Slice.prefab IdleGameHudBinder: 0
FindObjectOfType*UIDocument in Assets/Scripts: 0
using UnityEngine.UI; in Assets/Scripts/UI: 0
ToolkitHubManager UnlockSkin(: none (STUB prose only)
ToolkitHubManager RefreshAllSkinButtons: present (call + method)

# Optional Capybara static reconfirm (post-09b9b72)
IdleSliceUIController CapybaraGo: Fight Path/Safe Path/Lucky Find → FireNarrative(3/4/5)
IdleSliceUIController stats DNA: RunId | Choice | PetId
16_CapybaraGo_Steps_Slice.prefab: IdleSliceUIController present; binder absent

# EditMode HUD smoke
Prior green: Logs/IdleHud-impl10-r6-TestResults.xml → total=7 passed=7
Fresh r8 HUD batchmode: not run (HCR project lock / peer AllSmoke)
```

**Play Mode cosmetics:** **BLOCKED / UNVERIFIED**.

---

## STATUS

**PARTIAL VERIFIED** — UI8-02..05 static / prior EditMode + Capybara static reconfirm **VERIFIED**; UI8-01 Play Mode cosmetics **BLOCKED / UNVERIFIED** (one doctor, documented, no thrash).

---

## Receipt

- **Completed:** UI8-01 one doctor → BLOCKED documented; UI8-02/03/05 static PASS; UI8-04 held via prior green + greps + optional Capybara static reconfirm; smoke header UI8 wording
- **Stubs created:** none
- **Deviations:** fresh HUD batchmode skipped on peer AllSmoke lock (documented)
- **Flash Base:** none
- **Escalations:** none
- **Commit:** (local only, never push) — see git log after commit
