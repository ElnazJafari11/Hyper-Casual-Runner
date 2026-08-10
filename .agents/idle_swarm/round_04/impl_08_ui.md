# Round 04 Implement 08 — UI Toolkit (Idle)

**Agent:** implement 8/10  
**Source review:** `round_04/review_08_ui.md`  
**Architecture:** UI Toolkit only  
**Commit:** local only (no push)

---

## ASSUMPTIONS

1. Live idle HUD remains `IdleSliceUIController` (Option B) — confidence: **high** — verified 19/19 Idle prefabs; 0 `IdleGameHudBinder`.
2. Hyper-Casual-Runner interactive editor not on MCP bridge — confidence: **high** — `doctor` → registered_sessions empty; discovered interactive editor is `thepcgtoolkit`; HCR only appears as competing batchmode jobs this pass.
3. UI4-01 Play Mode cosmetics remains blocked without HCR interactive MCP — confidence: **high** — same doctor evidence as R3/R4 review.
4. UI4-06 EditMode wiring smoke is the approved substitute while UI4-01 is blocked — confidence: **high** — review recommended order item 3.
5. No further static cohesion leftovers beyond regression holds + UI4-06 — confidence: **high** — R4 review verdict PASS; R3 closed hub refresh / sandbox revive gate.

---

## Changes

| File | Change |
| --- | --- |
| `Assets/Scripts/Editor/Tests/IdleSliceHudSmokeTests.cs` | UI4-06: Cosmetics tab + BuySkin1 wiring smoke → asserts `CosmeticPurchaseEventComponent` (skin 1 / cost 5); documents UI4-01 blocked; ECS test world SetUp/TearDown |

No live HUD / prefab / hub / sandbox source changes.

---

## Acceptance criteria

| ID | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| UI4-01 | Play Mode cosmetics smoke | **BLOCKED / UNVERIFIED** | MCP `doctor`: HCR not registered; live transport pinned to `thepcgtoolkit@96e3a310`; discovered editors = `thepcgtoolkit` (interactive) + unrelated `TheCheckout` batchmode. No Play Mode enter / Cosmetics capture / `GameProgressData.CurrentSkinIndex` after buy. |
| UI4-02 | Toolkit-only + sole-HUD regression | **PASS** | Idle prefabs **19/19** `IdleSliceUIController`, **0** `IdleGameHudBinder`; 0 `FindObjectOfType*UIDocument` under `Assets/Scripts`; 0 `using UnityEngine.UI;` under `Assets/Scripts/UI` |
| UI4-03 | Hub equip-only + refresh holds | **PASS** | STUB dual-economy comment retained; `UnlockSkin` only in STUB prose; `RefreshAllSkinButtons()` still called on equip click |
| UI4-04 | EditMode smoke stays green | **PASS** | `IdleSliceHudSmokeTests` → `result=Passed total=2 passed=2 failed=0` (`Logs/IdleHud-TestResults.xml`, log `Logs/IdleHud-impl08-r4.log`, exit 0) |
| UI4-05 | Sandbox stay deferred or fix currency | **N/A (PASS)** | Binder instance count **0** on Idle prefabs |
| UI4-06 | (Optional) Stronger cosmetics wiring smoke | **PASS** | New test `IdleSliceUIController_CosmeticsBuy_EmitsCosmeticPurchaseEvent`: Cosmetics tab show + BuySkin1 → 1× `CosmeticPurchaseEventComponent` (TargetSkinIndex=1, PrestigeCost=5.0) |

R01 UI-01..07, R02 UI2-02..06, R03 UI3-02..06 retained as regression — sole live HUD, hub equip-only + refresh, binder unused, Toolkit-only unchanged.

---

## Static leftovers closed / documented

| R04 finding / item | Disposition |
| --- | --- |
| UI4-01 Play Mode cosmetics | **Documented BLOCKED** — no HCR interactive editor on MCP; cannot claim end-to-end cosmetics UX VERIFIED |
| Thin EditMode smoke (finding #4) | **Closed via UI4-06** — wiring beyond named-element surface checks |
| Hub equip-only + refresh / sole-HUD / Toolkit-only / deferred sandbox | **Hold PASS** — regression greps + existing smoke |
| Insufficient-funds UX, USS migration, LevelSelect idle browser, event entity destroy | Out of scope (per review) |

---

## UI4-01 blocked — how to verify later

Open Hyper-Casual-Runner in an interactive editor registered on MCP (or local Play), then:

1. Enter Play on any `Assets/ToolkitExamples/Idle/*_Slice.prefab`
2. Open Cosmetics tab
3. Buy a locked skin with funded prestige
4. Assert `GameProgressData.CurrentSkinIndex` matches; attach MCP `capture` / log

Until then: EditMode UI4-06 proves click → `CosmeticPurchaseEventComponent` only (not shop spend / skin index).

---

## Verification

```
# MCP doctor (HCR)
editors discovered: thepcgtoolkit (interactive) + TheCheckout (batchmode, unrelated)
HCR interactive / registered: none → UI4-01 BLOCKED / UNVERIFIED
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
→ Test run completed. Exiting with code 0 (Ok).
→ result=Passed total=2 passed=2 failed=0 (start-time 2026-08-10 01:04:33Z)
```

**Play Mode cosmetics:** **BLOCKED / UNVERIFIED**.

---

## STATUS

**PARTIAL VERIFIED** — UI4-02..06 static/EditMode **VERIFIED**; UI4-01 Play Mode cosmetics **BLOCKED / UNVERIFIED**.
