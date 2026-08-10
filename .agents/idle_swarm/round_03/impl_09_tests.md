# Round 03 Implement 09 — Idle EditMode Tests Soft-Green + Doc Residue

**Agent:** implement 9/10  
**Review:** `.agents/idle_swarm/round_03/review_09_tests.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Commit:** (see git tip after local commit) — local only (no push)

---

## ASSUMPTIONS

1. Stale checklist `EditMode-verified: **19/19**` overclaims verb closure — confidence: **high** — verified by: review_09 §6 + generator already honest.  
2. AdvCap post-hire `PumpSim` may already be on HEAD via batchA agent — confidence: **high** — verified by: `git show HEAD:…IdleBatchASmokeTests.cs` contains Post-hire PumpSim assert.  
3. Idle Miner post-hire income tick still missing until this impl — confidence: **high** — verified by: BC fixture lacked PumpSim until re-applied (concurrent BC agent once overwrote).  
4. Softening matrix “EditMode-verified MVP slices” blurb is in scope (review AC#5) — confidence: **high** — verified by: review_09 recommended next #5.

---

## Changes

| Area | What |
|------|------|
| `IdleBatchBCSmokeTests.cs` | After Miner hire: `PumpSim` ≥1s + assert `PrimaryCurrency` rises — landed on HEAD via concurrent commit `578a474` after this agent authored/re-applied it |
| `IdleBatchASmokeTests.cs` | AdvCap post-hire PumpSim — **already on HEAD** (batchA); not re-committed |
| `docs/idle-play-smoke-checklist.md` | Replace stale `EditMode-verified: **19/19**` with fixtures-present + ~17/19 scorecard pointer |
| `docs/idle_mechanics_matrix.md` | Soften “EditMode-verified MVP slices” → fixtures present + progress pointer |
| `docs/idle-toolkit-progress.md` | R3 impl_09 evidence; AdvCap/Miner scorecard note post-hire income; AllSmoke **65/65** |
| Compile unblock (not owned) | Kernel `AreNotEqual(..., 0.001)` CS1503 fixed earlier; landed via sibling commit |

Deferred (explicit in review): fold Kernel into AllSmoke; AFK combat path; claim mid-gate; multi-slice TargetSlice.

---

## Acceptance criteria → status

| # | Criterion | Status |
|---|-----------|--------|
| 1 | AdvCap after hire: `PumpSim` + `PrimaryCurrency` rises | **MET** (HEAD + AllSmoke green) |
| 2 | Idle Miner after hire: `PumpSim` + `PrimaryCurrency` rises | **MET** (this commit) |
| 3 | Checklist no longer markets `EditMode-verified: **19/19**` as verb closure | **MET** |
| 4 | Matrix blurb softened; progress scorecard notes income ticks | **MET** |

---

## VERIFICATION

```
ROUTE: Executor (round_03 review_09 ACs 1 + 5; AdvCap already by batchA)
ASSUMPTIONS: 4 verified, 0 unverified

Logs/IdleAllSmoke-Summary.txt:
result=Passed pass=65 fail=0 skip=0 inconclusive=0 duration=1.8156812

Logs/IdleAllSmoke-impl09-r3c.log:
AdventureCapitalist_BuyThenHireManager_AutomatesPassiveRate => Passed
IdleMiner_BuyShaftThenHireManager_AutomatesShaft => Passed
[IdleToolkit] result=Passed pass=65 fail=0 …

STATUS: VERIFIED
```

**RISKS:** Concurrent swarm agents may rewrite BC fixtures (Miner assert was once clobbered). Play Mode still 0/19. Antimatter + AFK remain PARTIAL. Kernel still outside AllSmoke.

---

## Flash Base

None.
