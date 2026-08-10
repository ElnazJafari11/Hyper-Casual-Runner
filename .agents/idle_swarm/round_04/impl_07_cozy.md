# Round 04 Implement 07 — Cozy / Assign (Persistence copy + station AFK)

**Scope:** A Dark Room, Cats & Soup, Neko Atsume, Fallout Shelter  
**Agent:** implement 7/10  
**Date:** 2026-08-10  
**Source review:** `round_04/review_07_cozy.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`

---

## Goal

Close Round-04 cozy polish: refresh stale Persistence progress copy (D18 SurvivePersist), and land cheap station AFK for Cats/Fallout via Kernel B.

## ASSUMPTIONS

1. Required bar is Persistence doc honesty; station AFK is optional-if-cheap — confidence: high — verified by: `review_07_cozy.md` Implement-round recommendations + user ask.
2. Station catch-up can use `AssignedWorkers` + bootstrap defaults (1.5 / 1s) without reading `IdleAssignmentStation` on the pure-state path — confidence: high — verified by: `IdleSliceBootstrap.AttachArchetypeExtras` + online sim payout formula.
3. EditMode Batch BC is accepted proof (Play Mode still UNVERIFIED) — confidence: high — verified by: prior cozy receipts.

---

## Changes

| Item | Criteria | Status |
|------|----------|--------|
| Refresh Persistence section (not “two archetypes”) | Names SurvivePersist extras + fixtures / Batch BC | **met** |
| Optional scorecard session-honesty one-liners | 05/17/18/19 mention SurvivePersist / AFK | **met** |
| Station AFK Cats → Primary | `AssignedWorkers>0` wall-clock grant via `IdleOfflineCatchUp` | **met** |
| Station AFK Fallout → PendingClaim | Same rate; sets `HasOfflineClaim` | **met** |
| Zero workers preserves D23 | Grant 0 (no stamp wipe on ApplyPersistedElapsed) | **met** |
| EditMode fixtures | Cats + Fallout OfflineCatchUp tests green | **met** |
| STAMP_POLICY Kernel B table | Document Cats/Fallout station bank destinations | **met** |

### Files

- `Assets/Scripts/ECS/Systems/Idle/IdleOfflineCatchUp.cs` — `ApplyStationCatchUp` for Cats/Fallout (D33 `SyncSkillNodeFromSlice` / `XpToLevelFor` helpers present in same file for concurrent Melvor sync)
- `Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs` — two OfflineCatchUp fixtures
- `.agents/idle_swarm/round_02/STAMP_POLICY.md` — Kernel B Cats/Fallout bank rows
- `.agents/idle_swarm/round_04/impl_07_cozy.md` — this receipt

### Already on HEAD (not re-committed)

- `docs/idle-toolkit-progress.md` Persistence rewrite + scorecard SurvivePersist/AFK lines — scooped by `6da58d0` (impl_04/llm progress sync)

### Deferred (skipped)

- Multi-station / multi-room / ADR atmosphere / cat variety / raids
- Play Mode verification
- Optional Neko food-gate / narrative-routing harden / prestige→cozy wipe

---

## VERIFICATION

**Read-back:** Persistence section no longer claims “two” archetypes; `Apply` routes Cats/Fallout before PassiveRate; tests assert 2×1.5×10=30 Primary and 1×1.5×2×8=24 Pending.

**EditMode Batch BC** (`Logs/IdleBatchBC-impl07-cozy-r4.log` / `Logs/IdleBatchBC-Summary.txt`):

```text
result=Passed pass=57 fail=0 skip=0 inconclusive=0 duration=3.0140421
CatsAndSoup_OfflineCatchUp_StationWorkers_AddsPrimary => Passed
FalloutShelter_OfflineCatchUp_StationWorkers_BanksPendingClaim => Passed
ADarkRoom_NarrativeWoodStoke_SurvivePersistNowReload => Passed
CatsAndSoup_AssignedWorkers_SurvivePersistNowReload => Passed
FalloutShelter_PendingAndWorkers_SurvivePersistNowReload => Passed
NekoAtsume_CheckInCats_SurvivePersistNowReload => Passed
```

**STATUS:** **VERIFIED** (EditMode).  
**Play Mode:** still UNVERIFIED.

---

## Receipt

- **Completed:** Persistence doc honesty (D18 SurvivePersist); cheap station AFK for Cats/Fallout; STAMP_POLICY + EditMode fixtures
- **Stubs created:** none
- **Deviations:** none (optional AFK taken because cheap — AssignedWorkers × bootstrap station defaults)
- **Flash Base:** none
- **Escalations:** none
- **Commit:** (this local commit; never push)
