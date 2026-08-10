# Round 4 IMPLEMENT 9/10 — Batch A verify-green + P2 persist asserts

**Agent:** implement 9/10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Date:** 2026-08-10  
**Source:** `round_04/review_02_batchA.md`  
**Commit:** (local only, no push — hash filled after commit)

---

## ASSUMPTIONS

1. No P0/P1 Batch A correctness work this round — **high** — review_02 fix list (Play only if claiming playable; P1 none).  
2. Optional P2#2 (Managers/Phase/MgrHired round-trip) is cheap enough to land — **high** — long Save/Load overloads already exist; only short-overload asserts were missing.  
3. Play Mode R4-A4 stays deferred — **high** — MCP instance is `pcg-toolkit`, not HCR; checklist still open.  
4. R3 OL-sync / hire income / FirePrestige contracts remain intact — **high** — bootstrap D25 order + hire `RequiresManager` + UI prestige-only still on disk.

---

## Changes

| Area | Change |
|------|--------|
| `IdleBatchASmokeTests.cs` | **P2 / R4-A3:** `GameProgressData_IdleSlice_RoundTrip_ManagersPhaseMgrHired_AdvCapAndPaperclips` — AdvCap Managers=1 + MgrHired=true; Paperclips Phase=3 + MgrHired=false; Clear AdvCap/Paperclips in SetUp/TearDown |
| Gameplay / bootstrap / UI | **None** (no P0/P1) |

Deferred (explicit): Play Mode 01/03/04; live bootstrap fixture; Cookie multi-tier; Paperclips phase identity depth.

---

## Acceptance (review R4-A1…A5)

| # | Criterion | Result |
|---|-----------|--------|
| R4-A1 | Existing Batch A suite (A1–A6 + zero-currency + AngelReset + phase exclusivity + AdvCap hire→sim) still PASS | **PASS** — suite `pass=23` (prior 21 + persist fixture + concurrent suite growth) |
| R4-A2 | Bootstrap Attach Sync **before** `ApplyPersistedElapsed`; `A6_…` gain **6±0.05** / PassiveRate **3** | **PASS** — bootstrap D25 comment+order; `A6_… => Passed` |
| R4-A3 | Round-trip asserts ManagersHired / PhaseIndex / MgrHired for AdvCap + Paperclips | **PASS** — new fixture Passed |
| R4-A4 | Play Mode checklist 01/03/04 | **Deferred** |
| R4-A5 | `FirePrestige` no `FirePhase`; hire keeps `RequiresManager` | **PASS** — UI source; hire comment+flag keep in `IdleBuyGeneratorSystem` |

---

## Verification evidence

`Logs/IdleBatchA-Summary.txt`:

```
result=Passed pass=23 fail=0 skip=0 inconclusive=0 duration=1.0767781
```

From `Logs/IdleBatchA-impl09-r4.log`:

```
A1_Cookie_SimCps_AppliesGlobalMultiplierOnce => Passed
A5_Reload_OwnedCountSynced_NextBuyUsesGrowthCost => Passed
A6_LoadCatchUp_UsesRawPassiveNotStaleMultSquared => Passed
AdventureCapitalist_BuyThenHireManager_AutomatesPassiveRate => Passed
GameProgressData_IdleSlice_RoundTrip_ManagersPhaseMgrHired_AdvCapAndPaperclips => Passed
FirePrestige_DoesNotAlsoCreatePhaseEvent_PaperclipsPhaseOnly => Passed
result=Passed pass=23 fail=0 skip=0 inconclusive=0
```

---

## STATUS

**VERIFIED** for R4-A1, R4-A2, R4-A3, R4-A5 via EditMode batchmode log above.  
**UNVERIFIED / deferred:** R4-A4 Play Mode.  
**RISKS:** Swarm agents concurrently mutate BC/kernel/test trees; HCR Editor MCP not connected (verification via batchmode only). Historical Mult³ Primary-on-disk still accepted MVP (review note).
