# Round 02 IMPLEMENT 02/10 — Production / Offline / Managers

**Agent:** 2/10 (IMPLEMENT)  
**Source review:** `review_05_production.md`  
**Coordination:** `STAMP_POLICY.md` + Kernel impl `8d2407c` (`impl_01_kernel.md`)  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Date:** 2026-08-10  
**Commit:** `157acd5ecd4a54fa8bb5c05da917e958d76c40a3` (local only — never push)  
**Push:** never
**Peer Kernel offline landing:** `8d2407c` (`impl_01_kernel.md`)

---

## ASSUMPTIONS

1. Round 02 production required bar = Play Mode stamp wipe + unify Kernel-B + Melvor AfkChest honesty; Egg/Miner Claim UI only if banking PendingClaim — **high** — `review_05_production.md` backlog.
2. Shared stamp policy with Kernel is binding — **high** — `STAMP_POLICY.md` owned jointly; Kernel landed OS/claim, Production owns bank-style + Melvor/Egg/Miner acceptance tests.
3. Quality bar = toolkit MVP EditMode — **high** — `docs/project-context.md`.

---

## Stamp policy (agreed with Kernel)

| Rule | Owner landing |
|------|----------------|
| Kernel A `OfflineSimulationSystem` = Producer wallets only; stamp `LastIdleUpdateTime` **only if** ≥1 producer catch-up applied | Kernel `8d2407c` |
| Kernel B sole slice writer = `IdleOfflineCatchUp` via bootstrap | Kernel + Production comment/docs |
| Melvor → `PendingClaim` (+ XP); **no** AfkChest bump | Kernel CatchUp + Production tests |
| Egg/Miner → **direct** `PrimaryCurrency`; **no Claim UI** (not required under this bank) | Production decision (ranked) |
| Claim XOR Pending vs chest/cats | Kernel D16 |

---

## Items completed

| Item | Status | Criteria |
|------|--------|----------|
| **[required] Unify Kernel B + fix Init race** | **met** (coord) | OS no longer mutates slices / wipes T0; sole Kernel B = `IdleOfflineCatchUp`. Machine: PlayOrder + empty-OS stamp tests. |
| **[required] Align Egg/Miner offline UX** | **met** | Keep PrimaryCurrency bank → **no Claim UI added**. Grep: Egg/Miner CatchUp asserts `PendingClaim == 0`. |
| **[required] Melvor catch-up must not inflate via AfkChest** | **met** | CatchUp leaves `AfkChestSeconds == 0`; claim XOR (Kernel) if both flags ever set. |
| Deferred Egg research / Miner elevators / SkillXp persist / soft-cap | **skipped** | deferred in review |

---

## Changes (this agent)

- `.agents/idle_swarm/round_02/STAMP_POLICY.md` — shared contract (committed with Kernel `8d2407c`).
- `Assets/Scripts/ECS/Systems/Idle/IdleOfflineCatchUp.cs` — docs: sole Kernel B + Egg/Miner Primary bank (Kernel commit).
- `Assets/Scripts/ECS/Authoring/IdleSliceBootstrap.cs` — comment points at STAMP_POLICY (working tree / this commit if staged).
- `Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs` — production acceptance tests:
  - `Melvor_OfflineCatchUp_DoesNotBumpAfkChestSeconds`
  - `PlayOrder_OfflineSimulationThenMelvorCatchUp_PreservesStamp`
  - `IdleMiner_OfflineCatchUp_AddsPrimaryCurrency_NoPendingClaim`
  - EggInc asserts no PendingClaim / no HasOfflineClaim
- Peer Kernel also landed `OfflineSimulationSystem` + claim XOR + kernel fixture tests (`impl_01_kernel.md`).

**Egg/Miner Claim UI:** ranked decision = **not added** (PrimaryCurrency path satisfies review “either/or”).

---

## VERIFICATION

```
ASSUMPTIONS: 3 verified

IdleKernelCorrectnessTests (Logs/IdleKernel-impl01.log / IdleKernel-TestResults.xml):
  total=9 passed=9 failed=0
  OfflineSimulation_EmptyWorld_DoesNotStampLastIdleUpdateTime => Passed
  OfflineSimulation_ThenBootstrapCatchUp_StillAccruesPendingClaim => Passed
  OfflineCatchup_IdleSlice_WithoutProducer_UsesKernelBCatchUp => Passed
  Claim_PendingClaim_DoesNotAlsoPayAfkChest => Passed

IdleBatchBC / AllSmoke (Logs/IdleBatchBC-impl07-cozy.log, IdleAllSmoke-impl04-r2.log):
  Melvor_OfflineCatchUp_DoesNotBumpAfkChestSeconds => Passed
  PlayOrder_OfflineSimulationThenMelvorCatchUp_PreservesStamp => Passed
  IdleMiner_OfflineCatchUp_AddsPrimaryCurrency_NoPendingClaim => Passed
  EggInc_OfflineCatchUp_AddsPrimaryCurrency => Passed
  Melvor_OfflineCatchUp_BanksPendingClaimCapped => Passed
  Melvor_ClaimOffline_NoDemoWithoutPending_PaysCatchUp => Passed
  IdleBatchBC-Summary: result=Passed pass=36 fail=0
  IdleAllSmoke-Summary: result=Passed pass=56 fail=0
```

**STATUS:** VERIFIED (EditMode)  
**Play Mode:** UNVERIFIED — MCP not registered to Hyper-Casual-Runner this session; EditMode Play-order mirror covers stamp race.

---

## Deviations

- Core OS stamp + claim XOR landed in Kernel commit `8d2407c` under shared `STAMP_POLICY.md` (intentional coord, not duplicate rewrite).
- Production BC acceptance tests were present in working tree before combat peer commit `b83e90e` also touched `IdleBatchBCSmokeTests.cs`; tests remain in HEAD and pass above.
- Did not add Egg/Miner “Claim Offline” buttons — bank style is PrimaryCurrency.

## Flash Base

None.

## Escalations

None.
