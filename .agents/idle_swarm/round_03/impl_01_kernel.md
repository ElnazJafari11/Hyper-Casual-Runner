# Round 03 Implement 01 — Idle Kernel Stamp + CatchUp Order (D23/D25)

**Agent:** implement 1/10  
**Sources:** `.agents/idle_swarm/round_03/review_01_kernel.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Date:** 2026-08-10  
**Push:** never

---

## ASSUMPTIONS

1. Quality bar = MVP toolkit slice correctness — high — `docs/project-context.md`.
2. Scope = P0 D23 + D25 only; D24 Play probe deferred — high — review suggested order + user prompt.
3. Binding stamp contract = `round_02/STAMP_POLICY.md` (amended this pass) — high — file updated.
4. Kernel B sole writer remains `IdleOfflineCatchUp` via bootstrap — high — review + prior receipts.

---

## Items completed

| ID | Status | Notes |
|----|--------|-------|
| **D23** zero-grant stamp | **met** | `IdleOfflineCatchUp.ApplyPersistedElapsed` stamps `LastIdleUpdateTime` **only if** `Apply` returns grant `> 0` |
| **D25** CatchUp before Sync | **met** | Bootstrap moves CatchUp to **after** `AttachArchetypeExtras` / `SyncGeneratorOwnedCountFromState`; Melvor default via `EnsureMelvorPassiveDefault` |
| Tests | **met** | Three EditMode cases added to `IdleKernelCorrectnessTests` |
| D24 Play AFK probe | **deferred** | Out of this implement slot (next suggested order item) |
| D26–D32 | **deferred** | P1/P2 leftovers |

---

## Acceptance criteria

| Criterion | Result | Evidence |
|-----------|--------|----------|
| Stamp T−60 + CatchUp PassiveRate=0 non-Melvor → timestamp unchanged | **PASS** | `ApplyPersistedElapsed_ZeroGrant_DoesNotStampLastIdleUpdateTime` |
| Positive grant still stamps | **PASS** | `ApplyPersistedElapsed_PositiveGrant_StampsLastIdleUpdateTime` |
| Sync Mult² stale prefs then CatchUp → grant == raw×Mult×t (not Mult²) | **PASS** | `BootstrapLoadPath_SyncBeforeCatchUp_UsesRawPassiveNotMultSquared` |
| Prior kernel gates still green | **PASS** | Prestige/mult/click/save/claim/D14–D16 fixtures |

---

## Verification

```
IdleKernelCorrectnessTests: total=12 passed=12 failed=0 result=Passed
  ApplyPersistedElapsed_PositiveGrant_StampsLastIdleUpdateTime
  ApplyPersistedElapsed_ZeroGrant_DoesNotStampLastIdleUpdateTime
  BootstrapLoadPath_SyncBeforeCatchUp_UsesRawPassiveNotMultSquared
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

Log: `Logs/IdleKernel-impl01-r3b.log`  
Results: `Logs/IdleKernel-TestResults.xml` (2026-08-10 ~00:34Z)

Play Mode relaunch AFK (D24): **UNVERIFIED** / deferred.

---

## Files touched (this agent)

- `Assets/Scripts/ECS/Systems/Idle/IdleOfflineCatchUp.cs` — `EnsureMelvorPassiveDefault` + `ApplyPersistedElapsed` (stamp-if-grant)
- `Assets/Scripts/ECS/Authoring/IdleSliceBootstrap.cs` — CatchUp after attach sync; remove unconditional stamp helper
- `Assets/Scripts/Editor/Tests/IdleKernelCorrectnessTests.cs` — D23/D25 fixtures
- `.agents/idle_swarm/round_02/STAMP_POLICY.md` — rule #3 + machine checks amended
- `.agents/idle_swarm/round_03/impl_01_kernel.md` — this receipt

---

## Deviations

- D24 Play/bootstrap integration probe not landed (explicit defer per ranked fix list item 2).
- D25 EditMode mirrors Sync→Apply order used by bootstrap rather than spawning a MonoBehaviour (matches Round 02 mirror style).

---

## STATUS

**ROUTE:** Executor (plan = review_01 P0 items 1–2)  
**VERIFIED:** IdleKernelCorrectnessTests 12/12 EditMode  
**COMMIT:** local only — never push
