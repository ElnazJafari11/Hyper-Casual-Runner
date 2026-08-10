# Round 07 Implement 04 — Gacha / Narrative / Auto

**Agent:** implement 4/10  
**Source review:** `round_07/review_06_gacha.md`  
**Date:** 2026-08-10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Commit:** local only (never push) — pending hash after commit

---

## Scope executed

From review priority list:

| # | Item | Status |
|---|------|--------|
| P2 #1 | Capybara roguelite stub (Run + binary choice) | **DONE** |
| P2 #2 | Pet/gear gacha on Capybara (Lucky Find → PetId) | **DONE** |
| P2 #3 | Optional PersistNow→entity identity fixture | **DEFERRED** |
| P3 | Dead-fallback delete / Play-smoke / AllSmoke reclaim | **DEFERRED** |

IH/LoM minimal identity DNA (R6) re-verified via EditMode filter (see VERIFICATION).

---

## ASSUMPTIONS

1. Quality bar = matrix MVP second-axis DNA beat (scalar run/choice/pet), not full roguelite inventory — **high** — review P2 wording + project-context.
2. Cheap stub may share `IdleNarrativeState` with ADR if Capybara-only fields stay 0 on ADR — **high** — prior IH/LoM identity pattern.
3. Lucky Find must not remain `FireClick(2f)` — **high** — review acceptance.
4. Unity MCP pinned to other project; verify via batchmode EditMode — **high** — doctor.
5. Persist of Run/Pet/Choice is optional this turn — **high** — review P2 acceptance is EditMode + HowTo only.

---

## Changes

### 1. Capybara DNA fields on `IdleNarrativeState`
- `RunId` (starts 1; +1 every 10 steps)
- `PendingChoice` / `LastChoice` (fork every 5 steps; Fight=1 / Safe=2)
- `PetId` (Lucky Find cycles 1..3)

### 2. Narrative actions
- Advance: pending fork @ %5; run bump @ %10 (keeps milestone mult)
- Action 3/4: resolve Fight/Safe when pending
- Action 5: Lucky Find → PetId + soft/passive (not click gold)

### 3. UI + HowTo honesty
- Buttons: Fight Path / Safe Path; Lucky Find → `FireNarrative(5)`
- HUD: `Run | Choice | Pet`
- Generator HowTo + prefab HowTo + play-smoke checklist row synced

### 4. EditMode fixtures
- `CapybaraGo_FiveSteps_SetsPendingChoiceAndRunId`
- `CapybaraGo_PathChoice_MutatesLastChoice`
- `CapybaraGo_TenSteps_IncrementsRunId`
- `CapybaraGo_LuckyFind_SetsPetId`

---

## Acceptance criteria

| Criterion (review_06_gacha P2) | Result |
|--------------------------------|--------|
| EditMode asserts run field / choice mutates state | **MET** — 4/4 Capybara DNA Passed |
| HowTo mentions run beat | **MET** — generator + prefab + checklist |
| Lucky Find asserts non-click identity / side-effect | **MET** — PetId + PassiveRate |

---

## VERIFICATION

```
TASK: Round 07 implement 4/10 — cheap Capybara roguelite/pet DNA
ROUTE: Executor (parity) against review_06_gacha P2
ASSUMPTIONS: 5 high verified
CHANGES:
 - IdleSliceComponents.cs (RunId/PendingChoice/LastChoice/PetId)
 - IdleSliceActionSystems.cs (fork/run/lucky-find helpers)
 - IdleSliceBootstrap.cs (Capybara RunId=1 cold-start)
 - IdleSliceUIController.cs (path buttons + HUD + Lucky Find)
 - IdleToolkitSliceGenerator.cs + prefab + play-smoke checklist
 - IdleBatchBCSmokeTests.cs (4 Capybara DNA fixtures)
 - this receipt
VERIFICATION:
 - Build: Unity 6000.5.5f1 batchmode compiled (no CS errors in filter run)
 - Run: Logs/IdleGacha-impl04-r7-TestResults.xml
     testcasecount=8 result=Passed passed=8 failed=0
     CapybaraGo_FiveSteps_SetsPendingChoiceAndRunId => Passed
     CapybaraGo_PathChoice_MutatesLastChoice => Passed
     CapybaraGo_TenSteps_IncrementsRunId => Passed
     CapybaraGo_LuckyFind_SetsPetId => Passed
     IdleHeroes_GachaPull_SetsHeroIdAndDupes => Passed
     LegendOfMushroom_GachaPull_SetsGearSlot => Passed
     GachaIdentity_SurvivePersistPrefsRoundTrip => Passed
     CapybaraGo_FiveSteps_RaisesGlobalMultiplier => Passed
STATUS: VERIFIED (P2 Capybara DNA + IH/LoM re-check)
RISKS:
 - Play Mode still UNVERIFIED
 - Run/Pet/Choice prefs persist deferred
 - Dead fallbacks / AllSmoke reclaim deferred (P3)
```

---

## Deviations

- Did not add PersistNow→entity identity fixture (optional P2 #3).
- Did not delete dead fallbacks / Play-smoke / AllSmoke reclaim (P3).

## Flash Base

None.
