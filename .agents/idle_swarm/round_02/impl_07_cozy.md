# Round 02 Implement 07 — Cozy / Assign (ECB + HowTo sync)

**Scope:** A Dark Room, Cats & Soup, Neko Atsume, Fallout Shelter  
**Agent:** implement 7/10  
**Date:** 2026-08-10  
**Source review:** `round_02/review_07_cozy.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`

---

## Goal

Land Round-02 required cozy polish: narrative ECB teardown hygiene + prefab/docs HowTo sync to post–impl_06 verbs/tests.

## ASSUMPTIONS

1. Review R02 required list is the implement bar (ECB Playback/Dispose + HowTo/docs sync) — confidence: high — verified by: `review_07_cozy.md` Implement-round recommendations.
2. Deferred offline station catchup / multi-room / Play Mode stay deferred — confidence: high — verified by: review Deferred list.
3. Generator HowTo strings are source of truth for prefab sync — confidence: high — verified by: `IdleToolkitSliceGenerator.Slices[]` lines 47–49.

---

## Changes ([required])

| Item | Criteria | Status |
|------|----------|--------|
| `IdleNarrativeActionSystem` ECB teardown | After narrative foreach: `ecb.Playback(em)` + `ecb.Dispose()` (mirror Assign/Claim) | **met** — authored in working tree; landed on `mainline` via concurrent commit `8d2407c` (blame lines 211–212) |
| Prefab HowTo sync | Neko/Fallout/Cats HowTo match generator post–impl_06 copy | **met** — prefabs `17_` / `18_` / `19_` |
| Compose + play-smoke docs sync | Test names + Place Toys + PendingClaim wording | **met** — `docs/idle-toolkit-compose.md`, `docs/idle-play-smoke-checklist.md` |

### Files in this local commit

- `Assets/ToolkitExamples/Idle/17_CatsAndSoup_Assign_Slice.prefab` — HowTo → station cook currency
- `Assets/ToolkitExamples/Idle/18_NekoAtsume_CheckIn_Slice.prefab` — HowTo → food/toys + clear cats
- `Assets/ToolkitExamples/Idle/19_FalloutShelter_Dwellers_Slice.prefab` — HowTo → Pending → Claim drain
- `docs/idle-toolkit-compose.md` — station/Pending/toys verbs + test names
- `docs/idle-play-smoke-checklist.md` — toys / Pending smoke steps
- `.agents/idle_swarm/round_02/impl_07_cozy.md` — this receipt

### Already on HEAD (not re-committed)

- `Assets/Scripts/ECS/Systems/Idle/IdleSliceActionSystems.cs` — `IdleNarrativeActionSystem` ECB `Playback`/`Dispose` (commit `8d2407c`)

### Deferred (skipped)

- Offline station catchup for Cats/Fallout
- Multi-station / multi-room / ADR atmosphere / cat variety / raids
- Play Mode verification
- PrestigeSystem / ToolkitHubManager compile hygiene left to owning agents (working-tree unblocks used for Batch BC only; not staged)

---

## VERIFICATION

**Read-back:** narrative system ends foreach with:

```csharp
ecb.Playback(em);
ecb.Dispose();
```

**Prefab HowTo vs generator:** Neko / Fallout / Cats strings match `IdleToolkitSliceGenerator` HowTo fields.

**Build / Batch BC:**

```text
Logs/IdleBatchBC-Summary.txt → result=Passed pass=36 fail=0 skip=0 inconclusive=0 duration=2.1367425
Logs/IdleBatchBC-impl07-cozy.log → ADarkRoom_* / CatsAndSoup_* / NekoAtsume_* / FalloutShelter_* => Passed
```

**STATUS:** **VERIFIED** (EditMode Batch BC 36/36; narrative fixtures exercise ECB Playback path).  
**Play Mode:** still Pending (unchanged).

---

## Receipt

- **Completed:** ECB Playback/Dispose (on HEAD via `8d2407c`); prefab + compose + play-smoke HowTo sync
- **Stubs created:** none
- **Deviations:** ECB lines were scooped into shared `IdleSliceActionSystems.cs` commit `8d2407c` by concurrent offline/kernel work after this agent applied them; cozy commit stages HowTo/docs/receipt only
- **Flash Base:** none
- **Escalations:** none
- **Commit:** _(local hash after commit; never push)_
