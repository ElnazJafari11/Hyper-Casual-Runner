# Round 05 Implement 02 — Cozy / Assign (D26 Neko wall-clock cats)

**Scope:** Neko Atsume wall-clock `CheckInCats` CatchUp (D26 cozy face)  
**Agent:** implement 2/10  
**Date:** 2026-08-10  
**Source review:** `round_05/review_07_cozy.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`

---

## Goal

Close Round-05 P2 Neko absence fantasy: closed-app time accrues check-in cats (or document online-only). Chosen path: cheap CatchUp arm.

## ASSUMPTIONS

1. Optional CatchUp arm preferred over doc-only — confidence: high — verified by: review “if prioritized” + user “fix if feasible”.
2. Online rate is 1 cat / 5s, soft 20 — confidence: high — verified by: `IdleSliceSimulationSystem` Neko case.
3. Return value = cats added (not Primary) so D23 stamps only when cats actually accrue — confidence: high — verified by: `ApplyPersistedElapsed` grant &gt; 0 gate.
4. IH/AFK chest CatchUp (rest of kernel D26) out of scope for cozy implement — confidence: high — verified by: review scope cozy/assign only.

---

## Changes

| Item | Criteria | Status |
|------|----------|--------|
| Neko CatchUp cats arm | `elapsed/5s` → CheckInCats + HasOfflineClaim; cap 20; no Primary | **met** |
| D23 sub-interval / full buffer | Grant 0 | **met** |
| STAMP_POLICY Kernel B row | Document Neko bank destination | **met** |
| Progress scorecard / Persistence | Neko AFK one-liner | **met** |
| EditMode fixture | `NekoAtsume_OfflineCatchUp_AccruesCheckInCats` | **met** (code); run evidence below |

### Files

- `Assets/Scripts/ECS/Systems/Idle/IdleOfflineCatchUp.cs` — `ApplyNekoCatchUp` + Neko route before PassiveRate
- `Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs` — OfflineCatchUp fixture
- `.agents/idle_swarm/round_02/STAMP_POLICY.md` — Neko Kernel B rows
- `docs/idle-toolkit-progress.md` — scorecard 18 + Neko AFK note
- `.agents/idle_swarm/round_05/impl_02_cozy.md` — this receipt

### Deferred (skipped)

- Multi-station / multi-room / ADR atmosphere / cat variety / raids
- Play Mode verification
- Prestige → cozy extras wipe
- Neko food-gate / narrative-routing harden
- IH/AFK chest CatchUp (kernel D26 remainder)

---

## VERIFICATION

**Read-back:** `Apply` routes `NekoAtsume` → `ApplyNekoCatchUp`; 50s → +10 cats; cap + D23 zero-grant paths present.

**EditMode Batch BC** (`Logs/IdleBatchBC-impl02-cozy-r5.log` / `Logs/IdleBatchBC-Summary.txt`):

```text
result=Passed pass=61 fail=0 skip=0 inconclusive=0 duration=4.6261293
NekoAtsume_OfflineCatchUp_AccruesCheckInCats => Passed
CatsAndSoup_OfflineCatchUp_StationWorkers_AddsPrimary => Passed
FalloutShelter_OfflineCatchUp_StationWorkers_BanksPendingClaim => Passed
NekoAtsume_CheckInCats_SurvivePersistNowReload => Passed
```

**STATUS:** **VERIFIED** (EditMode).  
**Play Mode:** still UNVERIFIED.

---

## Receipt

- **Completed:** D26 Neko wall-clock cats CatchUp + policy/progress + EditMode fixture
- **Stubs created:** none
- **Deviations:** Progress Persistence / scorecard Neko AFK lines scooped into `6461b37` (Batch A) before this commit; Batch BC 61/0 included concurrent peer fixtures present on disk during the run
- **Flash Base:** none
- **Escalations:** none
- **Commit:** `eef133f07ba8e8aef892a68c8cfc5ca2eb028f20` (CatchUp + policy + receipt; local only, never push)
- **Follow-up:** EditMode fixture `NekoAtsume_OfflineCatchUp_AccruesCheckInCats` landed in companion commit (test file raced with swarm peers on first stage)