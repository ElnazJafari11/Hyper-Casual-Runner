# Round 01 Implement 07 — Idle Kernel P0/P1

**Agent:** implement 7/10  
**Source review:** `.agents/idle_swarm/round_01/review_01_kernel.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Date:** 2026-08-10  
**Commit:** `6f86cf1e56ea2e9a42b63b38da367a0bcfda6b49` (local only — never push)  
**Push:** never

---

## ASSUMPTIONS

1. Quality bar = MVP slice correctness (no full clones) — high — `docs/project-context.md`.
2. Peers would land overlapping prestige/CPS/combat/production P0s in parallel — high — confirmed via `impl_01_batchA`, `impl_03_production`, `impl_05_combat` commits already on branch.
3. Remaining unique kernel work after peer landings = cosmetics ledger sync + dedicated kernel correctness fixture + receipt — high — `git status` showed only `CosmeticsShopSystem.cs` dirty among ECS + new `IdleKernelCorrectnessTests.cs`.

---

## Peer conflicts / already-landed (verify, do not re-author)

| Review item | Peer landing | Evidence |
|-------------|--------------|----------|
| P0#1 Prestige gate + scope | `57910f7` Batch A / prestige (`IdlePrestigeMath`, `PrestigeSystem` TargetSlice) | `PrestigeSystem_ZeroCurrency_DoesNotAward` Passed |
| P0#2 Phase gate + Paperclips/AD only | same + phase system | `Paperclips_*`, `Antimatter_PhaseShift_*` Passed |
| P0#3 Event→entity scoping | `IdleEventTarget` + `TargetSlice` on all idle events; UI `ResolveTargetSlice` | AllSmoke event tests Passed |
| P0#4 Mult single-site (no CPS²) | `ComputePassiveRate` raw; sim applies Mult once | `A1_Cookie_SimCps_AppliesGlobalMultiplierOnce` (peer) / kernel fixture |
| P0#5 Single combat HP authority | `b6e5b37` combat impl | CH/TT2/IH skip `TickHeroDps` when `IdleCombatState` present |
| P0#6 Persist OwnedCount/managers + extras | `GameProgressData` extended schema + bootstrap `_loadedGens` | `A5_Reload_OwnedCountSynced_*` / round-trip |
| P0#7 Offline catchup on IdleSliceState | `OfflineSimulationSystem` PendingClaim + 8h cap | Melvor/EggInc offline tests Passed |
| P0#8 Honest claim (no demo grant) | `IdleClaimOfflineSystem` no-op without claim evidence | IH/Melvor/Neko claim gates Passed |
| P1#9 Melvor single XP clock | production `1e2d9df` — skill node only | `MelvorIdle_GrindSkillNode_*` Passed |
| P1#10 Cats/Fallout single pay path | station-only (no continuous AssignedWorkers) | Cats/Fallout assign+sim Passed |
| P1#11 InvariantCulture parse | `TryParseInvariantDouble` in HEAD | GameProgress round-trip Passed |
| P1#12 Bootstrap destroy/cleanup | `_destroyed` + `DestroyEntity` on OnDestroy | present in `IdleSliceBootstrap` |
| P1#14 Paired CurrentRunStats | `IdleEventTarget.SyncPairedRunGold` / per-entity run sync | present in action/click/sim paths |

**Conflict note:** Concurrent agents briefly broke `OfflineSimulationSystem` (SystemAPI in static helpers → CS0120/EA0006). Restored to OnUpdate-only SystemAPI. Mid-query `CreateEntityQuery` in `IdleEventTarget.Resolve` caused EditMode `Failed:Error` until `FindSoleSlice` moved **before** event foreach — fixed during this agent's session; peers adapted.

---

## Items completed by this agent (7/10)

| ID | Status | Notes |
|----|--------|-------|
| P0 1–8 | **met (peer + verify)** | Verified in HEAD + AllSmoke; not re-landed |
| P1 9–12, 14 | **met (peer + verify)** | Verified in HEAD + AllSmoke |
| P1#13 Cosmetics single prestige ledger | **met (this agent)** | Spend `IdleSliceState.PrestigeCurrency` when co-located; sync `PersistentPlayerStats`; runner-only path unchanged |
| Kernel correctness fixture | **met (this agent)** | `IdleKernelCorrectnessTests.cs` — zero prestige, mult-once, multi-slice click isolation, save extras, offline PendingClaim, claim no-op |
| Event targeting safety | **met (this agent / shared)** | `FindSoleSlice` before event loops; ECB destroys |

### Explicitly deferred (out of bar / P2)

- P2#15–17 NGU rebirth DNA, AFK LastIdleUpdateTime chest while closed, Fallout neglect timer  
- P2#18 full test matrix beyond AllSmoke + kernel fixture  
- Nested AD layers, Realm spells, Miner elevator, real gacha RNG

---

## Acceptance criteria (review_01 P0/P1)

| Criterion | Result |
|-----------|--------|
| Prestige with 0 currency does nothing | **PASS** (Batch A + kernel fixture) |
| Mult not squared | **PASS** (A1 + kernel fixture) |
| Two slices + one click only mutates target | **PASS** (kernel fixture; AllSmoke single-slice covers happy path) |
| Save/load restores OwnedCount | **PASS** (A5 / bootstrap) |
| Offline catchup without ProducerComponent | **PASS** (Melvor/EggInc offline + kernel fixture) |
| Honest claim no free demo gold | **PASS** (IH/Melvor claim tests) |
| Cosmetics spends idle prestige when present | **PASS** (code path; CosmeticsShopTests still exercise runner-only ledger) |

---

## Verification evidence

Primary AllSmoke (this agent, `Logs/IdleKernel-impl07.log` / `Logs/IdleAllSmoke-Summary.txt`):

```
result=Passed pass=46 fail=0 skip=0 inconclusive=0 duration=0.968998
```

Dedicated filter (`Logs/IdleKernel-TestResults.xml` / `IdleKernel-correctness.log`):

```
total="11" passed="11" failed="0"
CosmeticsShopTests: 5/5 Passed
IdleKernelCorrectnessTests: 6/6 Passed
  - Prestige_ZeroCurrency_IsNoOp
  - BuyWithMult2_PassiveRateIsRaw_SimAppliesOnce
  - TwoSlices_OneClick_OnlyMutatesTarget
  - SaveLoad_RestoresOwnedGeneratorsField
  - OfflineCatchup_IdleSlice_WithoutProducer_SetsPendingClaim
  - Claim_WithoutEvidence_IsNoOp
```

---

## Files touched (this agent's final commit)

- `Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs` — P1#13 ledger sync  
- `Assets/Scripts/Editor/Tests/IdleKernelCorrectnessTests.cs` (+ `.meta`)  
- `.agents/idle_swarm/round_01/impl_07_kernel.md` — this receipt  

Shared kernel files already committed by peers (`IdleEventTarget`, `IdlePrestigeMath`, systems, `GameProgressData`, bootstrap, UI TargetSlice) — not re-committed unless dirty.

---

## Deviations

- Did not expand into P2 mechanic honesty (NGU rebirth DNA, neglect timer).  
- Did not push.  
- ToolkitExamples prefab churn / Logs left unstaged (unrelated / noisy).

---

## STATUS

**ROUTE:** Executor (plan = review_01 fix list)  
**VERIFIED:** AllSmoke 46/46 + Cosmetics+KernelCorrectness 11/11 EditMode  
**REMAINING RISK:** P2 mechanic honesty (NGU rebirth DNA, neglect timer) still deferred; Play Mode 0/19.
