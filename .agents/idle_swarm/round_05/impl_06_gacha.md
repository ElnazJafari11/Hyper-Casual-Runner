# Round 05 Implement 06 — Gacha / Narrative / Auto

**Agent:** implement 6/10  
**Source review:** `round_05/review_06_gacha.md`  
**Date:** 2026-08-10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Commit:** local only (never push) — `f07ee0c`

---

## Scope executed

From review priority list (cheap P1s only):

| # | Item | Status |
|---|------|--------|
| P1 #2 | LoM single-verb UI — hide `Farm Stage Gold` after Stage≥1 | **DONE** |
| P1 #3 | Sync play-smoke checklist to generator verbs | **DONE** (already on HEAD; reconfirmed) |

Deferred (not cheap this turn): minimal identity split (P1 #1), Capybara roguelite/pet gacha (P2), dead-fallback delete (P3), Play-smoke MCP (P3).

---

## ASSUMPTIONS

1. Quality bar = matrix MVP DNA; Farm hide at Stage≥1 closes single-verb control-surface gap without identity DNA — **high** — review P1 #2 acceptance OR.
2. Checked-in `docs/idle-play-smoke-checklist.md` must match `WritePlaySmokeChecklist` (not generator alone) — **high** — review finding J; HEAD already had LoM auto-lamp + Capybara unlocks auto-tiles rows (from prior sync commit).
3. Peer combat R5-F1 HUD rewrite of `RefreshStats` stays intact; Farm hide only adds Stage≥1 display toggle — **high** — re-applied atop `e016b13` combat HUD.
4. Unity MCP not pinned to HCR; verify via batchmode EditMode — **high** — doctor/instances.

---

## Changes

### 1. LoM Farm hide (P1 #2)
- `IdleSliceUIController`: keep `Farm Stage Gold` for Stage 0 graduation; hide (`DisplayStyle.None`) when `IdleGachaState.Stage >= 1`.
- Named button `FarmStageGoldButton`; `Btn` now returns `Button`.
- EditMode: `LegendOfMushroom_FarmButton_HidesAfterStageUnlock`.

### 2. Play-smoke checklist (P1 #3)
- No code change required this turn — HEAD already matches generator:
  - LoM: `Rub Lamp → auto-lamp at Stage≥1`
  - Capybara: `Take Step unlocks auto-tiles → Next Step → Lucky Find`
- Compose row already notes Farm only until Stage≥1.

---

## Acceptance criteria

| Criterion (from review_06_gacha P1 #2–#3) | Result |
|-------------------------------------------|--------|
| Stage≥1 UI lacks Farm **or** Farm clearly secondary (disabled/hidden) | **MET** — hidden at Stage≥1; EditMode Passed |
| Checklist has auto-lamp + unlocks auto-tiles; no peer “Farm Stage Gold” as primary LoM verb | **MET** — grep on `docs/idle-play-smoke-checklist.md` |

---

## VERIFICATION

```
TASK: Round 05 implement 6/10 — LoM Farm hide + checklist sync confirm
ROUTE: Executor (parity) against review_06_gacha P1 #2 + #3
ASSUMPTIONS: 4 high verified
CHANGES:
 - IdleSliceUIController.cs (Farm hide at Stage≥1)
 - IdleSliceHudSmokeTests.cs (LegendOfMushroom_FarmButton_HidesAfterStageUnlock)
 - this receipt
VERIFICATION:
 - Build: Unity 6000.5.5f1 batchmode EditMode compiled
 - Run: Logs/IdleHud-impl06-r5-TestResults.xml
     testcasecount=1 result=Passed passed=1 failed=0
     LegendOfMushroom_FarmButton_HidesAfterStageUnlock => Passed
 - Checklist grep: auto-lamp + unlocks auto-tiles present; no Farm Stage Gold primary verb
STATUS: VERIFIED
RISKS:
 - Play Mode still UNVERIFIED
 - Hero-dupe / gear / roguelite DNA still FAIL (deferred)
 - Concurrent swarm agents touch shared HUD files — re-applied atop combat R5-F1
```

---

## Deviations

- Checklist was already regenerated/synced on HEAD before this turn; receipt still greps acceptance closed.
- Did not run full AllSmoke (project lock contention); targeted Farm hide EditMode fixture green.
- Feature commit hash recorded as `f07ee0c` (same tree as transient `15dd7df` during swarm rewrite).

## Flash Base

None.
