# Round 01 IMPLEMENT 03/10 — Production / Offline / Managers

**Agent:** 3/10 (IMPLEMENT)  
**Source review:** `review_05_production.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Commit:** `1e2d9df` local only (never push)

---

## ASSUMPTIONS

1. Required backlog from review_05: manager prestige re-lock, Melvor offline catch-up (not demo +10), Kernel B wall-clock bridge, Melvor double-grind fix — **high** — verified by review acceptance criteria.
2. Parallel Round-01 implementers also touched prestige/claim/bootstrap — reuse `IdlePrestigeMath.ResetBuyableGenerator` / honest claim no-op when already present — **high** — verified by re-read before final edits.
3. Quality bar = toolkit MVP EditMode smoke — **high** — `docs/project-context.md`.

---

## Items completed

| Item | Status | Criteria |
|------|--------|----------|
| **[required] Manager prestige re-lock** | **met** | Hire keeps `RequiresManager`; prestige restores gate + clears `IsAutomated`; buy alone leaves `PassiveRate == 0`; second hire raises CPS. |
| **[required] Melvor offline = catch-up** | **met** | `IdleOfflineCatchUp` banks `min(elapsed, 8h) * skillRate` into `PendingClaim` + XP ticks; claim pays pending; empty Melvor claim is no-op (no demo +10). |
| **[required] Kernel A vs B bridge** | **met** | Kernel B: bootstrap load calls `IdleOfflineCatchUp`; Kernel A `OfflineSimulationSystem` remains `ProducerComponent`-only. Greppable helper + EditMode tests. |
| **[ranked] Melvor double grind** | **met** | Melvor switch arm no longer drips currency; `IdleSkillNode` is sole online authority; `PassiveRate` aligned to 1/s. |
| Deferred Egg research / Miner elevators / soft-cap multi-slice | skipped | deferred in review |

---

## Changes

- `Assets/Scripts/ECS/Systems/Idle/IdleOfflineCatchUp.cs` (+ meta) — 8h-capped Melvor PendingClaim + passive CPS catch-up helper.
- `Assets/Scripts/ECS/Authoring/IdleSliceBootstrap.cs` — load-time catch-up; Melvor `PassiveRate=1`; keep `RequiresManager` on restore; Melvor skill sync from progression.
- `Assets/Scripts/ECS/Systems/Idle/IdleBuyGeneratorSystem.cs` — hire sets `IsAutomated` only (gate preserved).
- `Assets/Scripts/ECS/Systems/Idle/IdlePrestigeMath.cs` (+ meta) — shared prestige reset restores manager gate (also used by prestige/phase).
- `Assets/Scripts/ECS/Systems/Idle/IdleSliceSimulationSystem.cs` — Melvor single authority; skill tick updates `PassiveRate`.
- `Assets/Scripts/ECS/Systems/Idle/IdleSliceActionSystems.cs` — claim honest no-op without accrued flags (no Melvor demo +10).
- `Assets/Scripts/ECS/Systems/Idle/OfflineSimulationSystem.cs` — restored Kernel A producer-only (fixed CS0120 from helper-method SystemAPI leak).
- `Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs` — prestige re-lock + Melvor/Egg catch-up/claim tests.

---

## VERIFICATION

```
Logs/IdleBatchBC-Summary.txt
result=Passed pass=29 fail=0 skip=0 inconclusive=0 duration=0.8014532

Logs/IdleProduction-impl03.log (selected):
IdleMiner_Prestige_ReLocksManager_BuyAloneDoesNotAutomate => Passed
Melvor_ClaimOffline_NoDemoWithoutPending_PaysCatchUp => Passed
Melvor_OfflineCatchUp_BanksPendingClaimCapped => Passed
EggInc_OfflineCatchUp_AddsPrimaryCurrency => Passed
MelvorIdle_GrindSkillNode_LevelsViaSimTick => Passed
IdleMiner_BuyShaftThenHireManager_AutomatesShaft => Passed
```

**STATUS:** VERIFIED (EditMode Batch BC 29/29)  
**Play Mode:** UNVERIFIED — MCP not on Hyper-Casual-Runner.

---

## Deviations

- Reused parallel-agent `IdlePrestigeMath` + claim no-op rather than duplicating prestige flag logic.
- Reverted a parallel Kernel-B path inside `OfflineSimulationSystem` (SystemAPI-in-helper CS0120 + double-apply risk); bootstrap + `IdleOfflineCatchUp` remain the Kernel B contract.

## Flash Base

None.

## Escalations

None.
