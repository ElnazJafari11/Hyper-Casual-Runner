# Round 02 Implement 01 — Idle Kernel Offline (D14/D15/D16)

**Agent:** implement 1/10  
**Sources:** `.agents/idle_swarm/round_02/review_01_kernel.md`, `review_05_production.md`, `STAMP_POLICY.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Date:** 2026-08-10  
**Commit:** `8d2407c2002994119a6a82ef8f2b6913d239d6a0` (local only — never push)  
**Push:** never

---

## ASSUMPTIONS

1. Quality bar = MVP toolkit slice correctness — high — `docs/project-context.md`.
2. Shared stamp contract in `round_02/STAMP_POLICY.md` is binding for Kernel + Production — high — file present before/during this landing.
3. Kernel B sole writer = `IdleOfflineCatchUp` via bootstrap; Kernel A = `OfflineSimulationSystem` Producer wallets only — high — STAMP_POLICY + reviews.
4. D14/D15/D16 are in kernel scope this round; D17–D18 deferred to other agents — high — user prompt + ranked fix list.

---

## Items completed

| ID | Status | Notes |
|----|--------|-------|
| **D14** stamp-before-slices | **met** | `OfflineSimulationSystem` stamps `LastIdleUpdateTime` **only if** ≥1 automated producer catch-up applied; empty Init leaves T0 for bootstrap |
| **D15** dual catch-up unify | **met** | Removed IdleSliceState loop from OS (no PendingClaim / AfkChest on slices); Kernel B = `IdleOfflineCatchUp` only |
| **D16** claim double-pay | **met** | `IdleClaimOfflineSystem`: if `PendingClaim > 0` pay pending only; else chest/cats; never both on one click |
| Tests | **met** | Kernel fixture expanded: empty-OS stamp preserve, OS-then-CatchUp Melvor accrual, claim conservation |
| D17 cosmetics TargetSlice | **deferred** | Out of this implement slot |
| D18 persist extras / PassiveRate recompute | **deferred** | Concurrent bootstrap hunks may land elsewhere; not claimed here |

---

## Acceptance criteria

| Criterion | Result | Evidence |
|-----------|--------|----------|
| OS with no slices/producers + past stamp → timestamp unchanged | **PASS** | `OfflineSimulation_EmptyWorld_DoesNotStampLastIdleUpdateTime` |
| Then Melvor CatchUp still accrues PendingClaim | **PASS** | `OfflineSimulation_ThenBootstrapCatchUp_StillAccruesPendingClaim` |
| Melvor catch-up does not bump AfkChestSeconds | **PASS** | `OfflineCatchup_IdleSlice_WithoutProducer_UsesKernelBCatchUp` |
| Claim with Pending+Chest pays pending only | **PASS** | `Claim_PendingClaim_DoesNotAlsoPayAfkChest` |
| Prior kernel gates still green | **PASS** | Prestige/mult/click/save/claim-noop |

---

## Verification

```
IdleKernelCorrectnessTests: total=9 passed=9 failed=0 result=Passed
  BuyWithMult2_PassiveRateIsRaw_SimAppliesOnce
  Claim_PendingClaim_DoesNotAlsoPayAfkChest
  Claim_WithoutEvidence_IsNoOp
  OfflineCatchup_IdleSlice_WithoutProducer_UsesKernelBCatchUp
  OfflineSimulation_EmptyWorld_DoesNotStampLastIdleUpdateTime
  OfflineSimulation_ThenBootstrapCatchUp_StillAccruesPendingClaim
  Prestige_ZeroCurrency_IsNoOp
  SaveLoad_RestoresOwnedGeneratorsField
  TwoSlices_OneClick_OnlyMutatesTarget
```

Log: `Logs/IdleKernel-impl01.log`  
Results: `Logs/IdleKernel-TestResults.xml` (2026-08-10 ~02:58)

Play Mode relaunch AFK: **UNVERIFIED** in-editor (EditMode ordering mirror covers the race). Risks: none known for stamp policy; live Play still needs one PersistNow → stop → play smoke if disputed.

---

## Files touched (this agent)

- `Assets/Scripts/ECS/Systems/Idle/OfflineSimulationSystem.cs` — Producer-only + stamp-only-on-apply
- `Assets/Scripts/ECS/Systems/Idle/IdleOfflineCatchUp.cs` — docs aligned to sole Kernel B authority
- `Assets/Scripts/ECS/Systems/Idle/IdleSliceActionSystems.cs` — D16 claim mutual exclusion (file also holds concurrent cozy/gacha hunks in working tree)
- `Assets/Scripts/Editor/Tests/IdleKernelCorrectnessTests.cs` — D14/D15/D16 cases
- `.agents/idle_swarm/round_02/STAMP_POLICY.md` — shared contract
- `.agents/idle_swarm/round_02/impl_01_kernel.md` — this receipt

**Not staged from this agent:** `IdleSliceBootstrap.cs` concurrent EnergyPool / `SyncGeneratorOwnedCountFromState` hunks (other agents).

---

## Deviations

- Prefer PendingClaim-for-all was **not** forced: Egg/Miner keep direct PrimaryCurrency per STAMP_POLICY production contract.
- Pending-preferring claim leaves AfkChestSeconds for a later claim (does not wipe unpaid chest).

---

## STATUS

**ROUTE:** Executor (plan = review_01 P0 fix list + STAMP_POLICY)  
**VERIFIED:** IdleKernelCorrectnessTests 9/9 EditMode  
**COMMIT:** local only — never push
