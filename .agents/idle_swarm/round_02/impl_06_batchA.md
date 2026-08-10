# Round 2 IMPLEMENT 6/10 — Batch A fixture harden (A1 buy→sim / A5 real reload)

**Agent:** implement 6/10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Date:** 2026-08-10  
**Source:** `round_02/review_02_batchA.md`  
**Commit:** local only (no push)

---

## ASSUMPTIONS

1. P1 harden only (A1 buy→sim, A5 Save→reload); Play Mode A7 out of scope — **high** — review fix list.  
2. Shared `SyncGeneratorOwnedCountFromState` is the bootstrap load contract tests must call — **high** — review T1/T2 + bootstrap OwnedCount=`_loadedGens`.  
3. Paperclips phase identity (verb swap / compute) only if cheap — **skipped** (UI verb rebuild not cheap) — **high**.  
4. Optional P1#4 rebuild PassiveRate on load is in scope via the sync helper — **high**.

---

## Changes

| Area | Change |
|------|--------|
| `IdlePrestigeMath.cs` | Added `SyncGeneratorOwnedCountFromState` — OwnedCount sync + rebuild raw PassiveRate when automated |
| `IdleSliceBootstrap.cs` | CPS attach path uses sync helper (guards stale Mult² Passive on load) |
| `IdleBatchASmokeTests.cs` | **A1** buy→sim (Mult=2, buy 1, 1s → Primary≈2, PassiveRate=1); **A5** SaveIdleSlice → clear world → TryLoad → sync helper → cost `15*1.15^3` |
| Paperclips identity | **Deferred** — not cheap |

---

## Acceptance (review R2-A1…A5)

| # | Criterion | Result |
|---|-----------|--------|
| R2-A1 | Existing A1–A5 + zero-currency + AngelReset + Paperclips phase exclusivity still PASS | **PASS** — Batch A `pass=20 fail=0` |
| R2-A2 | Buy path Mult=2 → 1s sim → Primary **2±0.05**, PassiveRate **1** | **PASS** — `A1_Cookie_SimCps_AppliesGlobalMultiplierOnce` |
| R2-A3 | Save Gens=3 → reload → next buy `15*1.15^3` | **PASS** — `A5_Reload_OwnedCountSynced_NextBuyUsesGrowthCost` |
| R2-A4 | Play Mode 01/03/04 | **Deferred** (P0 playability; not this ticket) |
| R2-A5 | `FirePrestige` has no `FirePhase` call | **PASS** — source review (`FirePrestige` creates `PrestigeEventComponent` only) |

---

## Verification evidence

`Logs/IdleBatchA-Summary.txt`:

```
result=Passed pass=20 fail=0 skip=0 inconclusive=0 duration=1.10442
```

From `Logs/IdleBatchA-impl06.log`:

```
A1_Cookie_SimCps_AppliesGlobalMultiplierOnce => Passed
A2_Cookie_Prestige_SingleConversion_NoForcedExtra => Passed
A3_AdvCap_AfterPrestige_RequiresManager_BuyLeavesPassiveZero => Passed
A4_AdvCap_HireWithZeroOwned_LeavesPassiveZero => Passed
A5_Reload_OwnedCountSynced_NextBuyUsesGrowthCost => Passed
PrestigeSystem_ZeroCurrency_DoesNotAward => Passed
AdventureCapitalist_AngelReset_ConvertsAndResetsManagers => Passed
FirePrestige_DoesNotAlsoCreatePhaseEvent_PaperclipsPhaseOnly => Passed
Paperclips_ManufactureThenPhaseShift_ConvertsToPrestige => Passed
```

---

## STATUS

**VERIFIED** for R2-A1 / R2-A2 / R2-A3 / R2-A5 via Batch A EditMode log above.  
**UNVERIFIED / deferred:** R2-A4 Play Mode; Paperclips identity (P2).  
**RISKS:** Multi-agent batchmode lock contention on HCR; A5 exercises the shared sync helper (same contract bootstrap uses) rather than spawning a live `IdleSliceBootstrap` MonoBehaviour.
