# Round 3 IMPLEMENT 6/10 — Batch A OL-sync proof + AdvCap/Miner post-hire income

**Agent:** implement 6/10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Date:** 2026-08-10  
**Sources:** `round_03/review_02_batchA.md` (+ `round_03/review_09_tests.md` ranked leftover #1)  
**Commit:** `0936ffa` (local only, no push)

---

## ASSUMPTIONS

1. P1 OL-sync is in scope for Batch A; Play Mode R3-A4 deferred — **high** — review_02 fix list.  
2. AdvCap/Miner post-hire `PumpSim` is ranked (review_09 #1 leftover) → include — **high**.  
3. Optional persist Managers/Phase/MgrHired asserts (R3-A3) stay optional — **skipped**.  
4. Production CatchUp-after-Sync (D25) lands via kernel peer — **high** — verified `bd06da8` on disk.

---

## Changes

| Area | Change | Landing |
|------|--------|---------|
| `IdleBatchASmokeTests.cs` | **AdvCap** hire → `PumpSim` 1s + Primary rises; **A6** Sync→CatchUp Mult² stale → gain **6±0.05**, PassiveRate **3** | In tree (also present in `763a323` working-tree bundle) |
| `IdleBatchBCSmokeTests.cs` | **Miner** hire → `PumpSim` 1s + Primary rises | In tree (`578a474` / BC peer keep) |
| `IdleSliceBootstrap.cs` | CatchUp **after** Attach Sync (D25 / R3-A2 production) | **`bd06da8`** kernel peer |

---

## Acceptance (review R3-A1…A5 + ranked income)

| # | Criterion | Result |
|---|-----------|--------|
| R3-A1 | Existing Batch A A1–A5 + zero-currency + AngelReset + Paperclips phase exclusivity still PASS | **PASS** — Batch A `pass=21` |
| R3-A2 | Stale Mult² Passive → Sync then catch-up 1s → Primary gain **6±0.05**, PassiveRate **3** | **PASS** — `A6_LoadCatchUp_UsesRawPassiveNotStaleMultSquared` |
| R3-A3 | Persist Managers/Phase/MgrHired round-trip asserts | **Deferred** (optional) |
| R3-A4 | Play Mode 01/03/04 | **Deferred** |
| R3-A5 | `FirePrestige` no `FirePhase`; Sync still used from bootstrap CPS attach | **PASS** — source + Attach Sync still present; CatchUp after Sync via `bd06da8` |
| Ranked | AdvCap + Miner post-hire income tick | **PASS** — AdvCap Batch A + Miner BC PumpSim |

---

## Verification evidence

`Logs/IdleBatchA-Summary.txt`:

```
result=Passed pass=21 fail=0 skip=0 inconclusive=0 duration=0.5740529
```

From `Logs/IdleBatchA-impl06-r3.log`:

```
A1_Cookie_SimCps_AppliesGlobalMultiplierOnce => Passed
A5_Reload_OwnedCountSynced_NextBuyUsesGrowthCost => Passed
A6_LoadCatchUp_UsesRawPassiveNotStaleMultSquared => Passed
AdventureCapitalist_BuyThenHireManager_AutomatesPassiveRate => Passed
result=Passed pass=21 fail=0 skip=0 inconclusive=0
```

Miner (Batch BC, earlier same session — `Logs/IdleBatchBC-impl02-r3.log`):

```
IdleMiner_BuyShaftThenHireManager_AutomatesShaft => Passed
```

---

## STATUS

**VERIFIED** for R3-A1, R3-A2, R3-A5, and ranked AdvCap/Miner post-hire income via EditMode logs above.  
**UNVERIFIED / deferred:** R3-A3 persist key asserts; R3-A4 Play Mode.  
**RISKS:** A6 locks Sync→Apply contract (same order as bootstrap D25) without spawning live `IdleSliceBootstrap` MonoBehaviour; HCR batchmode lock contention among swarm agents.
