# Round 02 Implement 08 — UI Toolkit (Idle)

**Agent:** implement 8/10  
**Source review:** `review_08_ui.md`  
**Architecture:** UI Toolkit only  
**Commit:** `09034e9` local only (no push)

---

## ASSUMPTIONS

1. Live idle HUD remains `IdleSliceUIController` (Option B from R01) — confidence: **high** — prefabs + binder inventory unchanged.
2. Hyper-Casual-Runner interactive editor not on MCP bridge this pass — confidence: **high** — `doctor` targets `thepcgtoolkit`; HCR used batchmode only.
3. Zero `IdleGameHudBinder` prefab/scene instances → UI2-04 N/A — confidence: **high** — Assets grep.

---

## Changes

| File | Change |
| --- | --- |
| `Assets/Scripts/UI/IdleSliceUIController.cs` | `EnsureHudBuilt()` + `RootVisualElement` for EditMode smoke; PanelSettings helpers |
| `Assets/Scripts/Editor/Tests/IdleSliceHudSmokeTests.cs` | **New** — EditMode asserts named Actions/Cosmetics tabs + `EquipSkin0` / `BuySkin{1..3}` |
| `Assets/Scripts/UI/ToolkitHubManager.cs` | Hub Skin Shop STUB: equip-only for unlocked skins; no gold `UnlockSkin` dual economy |
| `Assets/Scripts/UI/IdleUIManagerSystem.cs` | UI2-05 click unbind on binder clear / rebind / `OnStopRunning`; cosmetic events set `TargetSlice` |

---

## Acceptance criteria

| ID | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| UI2-01 | Play Mode cosmetics smoke | **UNVERIFIED** | No HCR interactive editor on MCP; no Play Mode capture. Static path still builds Cosmetics tab + `CosmeticPurchaseEventComponent`. |
| UI2-02 | EditMode HUD smoke | **PASS** | `IdleSliceHudSmokeTests` → `result=Passed passed=1 failed=0` (`Logs/IdleHud-TestResults.xml`, log `Logs/IdleHud-impl08e.log`, exit 0) |
| UI2-03 | Hub vs prestige policy | **PASS** | Hub documented `// TODO: [STUB]`; gold unlock removed; equip-only; idle Cosmetics remains unlock authority |
| UI2-04 | Sandbox currency or stay deferred | **N/A (PASS)** | Grep: **0** `IdleGameHudBinder` on prefabs/scenes; binder stays unused |
| UI2-05 | Button unbind | **PASS (static)** | `UnbindButtons()` on binder null, document change, failed latch, `OnStopRunning`; named `Action` handlers |
| UI2-06 | Toolkit-only regression | **PASS** | No `FindObjectOfType<UIDocument>` under `Assets/Scripts`; idle UI scripts UIElements-only (`using UnityEngine.UI` absent) |

R01 UI-01..07 retained as regression (static) — sole live HUD, binder gate, LevelSelect policy, cached labels unchanged in intent.

---

## Verification

```
# EditMode HUD smoke (batchmode)
Unity 6000.5.5f1 -batchmode -nographics -projectPath D:\Git\Hyper-Casual-Runner
  -runTests -testPlatform EditMode
  -testFilter HyperCasualRunner.Tests.IdleSliceHudSmokeTests
→ Exiting with code 0 (Ok). result=Passed passed=1 failed=0
→ Assembly-CSharp.dll contains EnsureHudBuilt: True
```

**Play Mode cosmetics:** **UNVERIFIED**.  
To verify: open any `Assets/ToolkitExamples/Idle/*_Slice.prefab`, Enter Play → Cosmetics tab → buy with funded prestige → `GameProgressData.CurrentSkinIndex` updates.

---

## STATUS

**PARTIAL VERIFIED** — UI2-02 EditMode smoke **VERIFIED**; UI2-03/04/05/06 static **VERIFIED**; UI2-01 Play Mode cosmetics **UNVERIFIED**.
