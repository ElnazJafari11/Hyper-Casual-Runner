# Round 02 Implement 04 — Gacha / Narrative / Auto

**Agent:** implement 4/10  
**Source review:** `round_02/review_06_gacha.md`  
**Date:** 2026-08-10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Commit:** local only (never push)

---

## Scope executed

From review priority list:

| # | Item | Status |
|---|------|--------|
| P0 | Capybara auto-tiles | **DONE** |
| P1 | LoM prefab HowTo (“auto-lamp”) | **DONE** |
| P1 | LoM single-verb / income-in-lamp (cheap) | **DONE** — Stage≥1 lamp loot refunds cost+1 |
| — | Capybara HowTo (generator + prefab) | **DONE** — mentions Auto-tiles |

Deferred: hero-dupe identity, HUD second-axis, roguelite stub, pet gacha, dead-fallback delete, Play-smoke.

---

## ASSUMPTIONS

1. Quality bar = matrix MVP DNA (core verb + automation + one second-axis beat) — **high** — `docs/project-context.md` + review.
2. Capybara auto-tiles unlock when `ExploreUnlocked > 0` (bootstrap already seeds 1) — **high** — matches review “when unlocked”.
3. Auto interval = 1s per tile; milestone mult reuses `TryAdvanceStep` (`RoomOrStep % 5`) — **high** — mirrors manual narrative path.
4. LoM lamp loot only when `Stage >= 1` *before* the pull’s stage bump — **high** — preserves RubLamp spend asserts for graduation pulls.
5. Concurrent swarm may contend for Unity batchmode — **high** — observed.

---

## Changes

### 1. Capybara auto-tiles (P0)
- `IdleNarrativeState.AutoTimer` interval accumulator.
- `IdleNarrativeActionSystem.TryAdvanceStep` shared by manual action 1 + sim.
- `IdleSliceSimulationSystem`: Capybara + ExploreUnlocked → every 1s call `TryAdvanceStep` (milestone mult included).

### 2. LoM leftovers (cheap)
- Prefab `15_*` HowTo → contains “auto-lamp”.
- `TryApplyPull`: when LoM and Stage≥1, refund `cost + 1` so Farm click is optional after unlock.
- Auto-lamp test tightened: start currency 15, assert PullCount ≥ 5 over ~5s (sustained without farm).

### 3. Capybara HowTo
- Generator + prefab `16_*`: “Auto-tiles advance steps…”.

### Tests (`IdleBatchBCSmokeTests`)
- `CapybaraGo_AutoTiles_AdvanceWithoutEvents` — sim dt → RoomOrStep ≥ 5, GlobalMultiplier rises (no narrative events).
- Updated `LegendOfMushroom_AutoLamp_PullsAfterStageUnlock` — sustain assert.

---

## Acceptance criteria

| Criterion | Result |
|-----------|--------|
| EditMode sim dt → RoomOrStep increases without narrative event | **MET** — `CapybaraGo_AutoTiles_AdvanceWithoutEvents` |
| After 5 auto steps GlobalMultiplier rises | **MET** — same test |
| LoM prefab string contains “auto-lamp” | **MET** — prefab HowTo |
| Stage≥1 run with no click events sustains ≥1 auto-pull over window | **MET** — AutoLamp test PullCount ≥ 5 from 15 gold |

---

## VERIFICATION

```
TASK: Round 02 implement 4/10 — Capybara auto-tiles + cheap LoM leftovers
ROUTE: Executor (parity) against review_06_gacha P0/P1
ASSUMPTIONS: 5 high verified
CHANGES:
 - IdleSliceComponents.cs (Narrative AutoTimer)
 - IdleSliceActionSystems.cs (TryAdvanceStep + LoM lamp loot)
 - IdleSliceSimulationSystem.cs (Capybara auto-tiles loop)
 - IdleBatchBCSmokeTests.cs (auto-tiles test + AutoLamp sustain)
 - IdleToolkitSliceGenerator.cs (Capybara HowTo)
 - 15_LegendOfMushroom_Lamp_Slice.prefab (auto-lamp HowTo)
 - 16_CapybaraGo_Steps_Slice.prefab (Auto-tiles HowTo)
VERIFICATION:
 - Build: Unity 6000.5.5f1 batchmode compiled + ran AllSmoke
 - Run: Logs/IdleAllSmoke-Summary.txt → result=Passed pass=56 fail=0
 - Behavior (IdleAllSmoke-impl04-r2.log):
     CapybaraGo_AutoTiles_AdvanceWithoutEvents => Passed
     LegendOfMushroom_AutoLamp_PullsAfterStageUnlock => Passed
     CapybaraGo_FiveSteps_RaisesGlobalMultiplier => Passed
STATUS: VERIFIED
RISKS:
 - Play Mode still UNVERIFIED (MCP not on Hyper-Casual-Runner)
```

---

## Deviations

- LoM single-verb solved via Stage≥1 lamp-loot refund inside `TryApplyPull` (not PassiveRate drip / UI button removal).
- Auto-tile interval = 1s (LoM lamp uses 2s).

## Flash Base

None.
