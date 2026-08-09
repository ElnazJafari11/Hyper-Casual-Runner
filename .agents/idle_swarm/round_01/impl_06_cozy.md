# Round 01 Implement 06 — Cozy / Assign

**Scope:** A Dark Room, Cats & Soup, Neko Atsume, Fallout Shelter  
**Agent:** implement 6/10  
**Date:** 2026-08-10  
**Source review:** `review_07_cozy.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`

---

## Goal

Make Dark Room / Cats&Soup / Neko / Fallout Shelter core verbs **representable** (review required polish), not empty stubs.

## ASSUMPTIONS

1. Review required polish is the implement bar (dedupe assign pay, Fallout buffer-claim, Neko clear cats + toys) — confidence: high — verified by: `review_07_cozy.md` Required list.
2. Deferred multi-room / atmosphere / cat variety stay deferred — confidence: high — verified by: review Deferred list + project quality bar.
3. Concurrent agents may share Idle ECS files; cozy semantics must remain after their TargetSlice/offline work — confidence: med — verified by: re-read of sim/claim/narrative after contention.

---

## Changes ([required])

| Item | Criteria | Status |
|------|----------|--------|
| Deduplicate assign production | Cats & Fallout no longer pay both continuous CPS **and** station interval | **met** — archetype switch leaves continuous empty; sole pay path is `IdleAssignmentStation` |
| Fallout buffer-then-claim | Station ticks → `PendingClaim`; Claim drains buffer to `PrimaryCurrency` | **met** — sim Fallout branch + claim system + HUD `Pending` |
| Neko clear cats on check-in | Claim sets `CheckInCats = 0` | **met** — `IdleClaimOfflineSystem` |
| Neko toys verb (optional required) | Place Toys narrative action 1 attracts cats / spends currency | **met** — UI button + `ApplySliceOnlyNarrative` action 1 |
| A Dark Room craft beat | Craft spends wood → `PassiveRate` | **met** (already wired) + EditMode test `ADarkRoom_Craft_SpendsWoodForPassive` |

### Differentiation (17 vs 19)

- **Cats & Soup:** station interval → live `PrimaryCurrency` (cook drip).
- **Fallout Shelter:** station interval → `PendingClaim` (Claim Production drains).

### Files touched (cozy-owned)

- `Assets/Scripts/ECS/Systems/Idle/IdleSliceSimulationSystem.cs` — assign pay dedupe + Fallout buffer
- `Assets/Scripts/ECS/Systems/Idle/IdleSliceActionSystems.cs` — Neko food/toys; claim clears cats / drains pending
- `Assets/Scripts/ECS/Components/IdleSliceComponents.cs` — `PendingClaim` field (shared)
- `Assets/Scripts/UI/IdleSliceUIController.cs` — Place Toys; Pending HUD
- `Assets/Scripts/Editor/IdleToolkitSliceGenerator.cs` — HowTo / play-smoke copy
- `Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs` — cozy criteria tests
- `Assets/Scripts/ECS/Systems/Idle/OfflineSimulationSystem.cs` — **compile unblock**: moved `SystemAPI` out of helpers (CS0120) so Batch BC can compile

### Deferred (skipped)

- Multi-station / multi-room layouts
- Dark Room narrative text / atmosphere
- Cat collection variety, decoration, survival incidents

---

## Tests added / strengthened

- `CatsAndSoup_AssignCatThenSim_ProducesOutput` — station-only live pay (no PendingClaim)
- `NekoAtsume_PlaceToys_SpendsAndAttractsCats`
- `NekoAtsume_CheckIn_ClearsCatsAndPays`
- `FalloutShelter_AssignDwellerThenSim_FillsStationAndAccrues` — buffer + claim drain
- `ADarkRoom_Craft_SpendsWoodForPassive`

---

## VERIFICATION

**Build / Batch BC:** After fixing OfflineSimulationSystem CS0120 and waiting out lock contention:

```text
Logs/IdleBatchBC-Summary.txt → result=Passed pass=29 fail=0 skip=0
Logs/IdleBatchBC-TestResults.xml → total="29" passed="29" failed="0"
```

(Earlier suite was 14; expanded suite includes cozy PlaceToys / CheckIn / Fallout claim / ADR craft fixtures.)

**STATUS:** **VERIFIED** (EditMode Batch BC 29/29).  
**Play Mode:** still Pending (unchanged).

---

## Receipt

- **Completed:** required cozy polish (dedupe, Fallout claim buffer, Neko toys + clear cats, ADR craft test)
- **Stubs created:** none new on cozy verbs
- **Deviations:** Used station-only pay for both assign archetypes (not continuous-for-Cats); still meets “pick one path.” Fixed unrelated OfflineSimulationSystem CS0120 to unblock compile.
- **Flash Base:** none
- **Escalations:** none
- **Commit:** local only (never push)
