# Round 08 Implement 01 — Gacha / Narrative / Auto (Capybara DNA prefs)

**Agent:** implement 1/10  
**Source review:** `round_08/review_06_gacha.md`  
**Date:** 2026-08-10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Commit:** local only (never push) — `a12f819`  
**Prior stall:** peers `afe3dbf5` / `73ceaeb7` / `f3162869` (interrupt attempted; composers not found)

---

## Scope executed

From review priority list:

| # | Item | Status |
|---|------|--------|
| P2 #1 | Persist Capybara DNA (`RunId` / `PetId` / `LastChoice` / `PendingChoice`) + attach restore + cold RunId floor | **DONE** |
| P2 #2 | Optional IH/LoM PersistNow→entity identity fixture | **DEFERRED** |
| P3 | Dead-fallback delete / Play-smoke | **DEFERRED** |

---

## ASSUMPTIONS

1. Quality bar = matrix MVP DNA reload fidelity, not full roguelite bags — **high** — review P2 #1 acceptance.  
2. Missing prefs keys → 0; cold attach floors Capybara `RunId` to 1 when no positive saved RunId — **high** — review + bootstrap comment.  
3. ADR shares `IdleNarrativeState` but must keep DNA fields at 0 (no Capybara save/load for ADR) — **high** — PersistNow gates on `CapybaraGo`.  
4. HCR interactive MCP not connected (`thepcgtoolkit` only); verify via `compile_check` + existing Batch BC tip for suite health — **high** — doctor.  
5. EditMode fixture `CapybaraGo_RunPetChoice_SurvivePersistNowReload` is the machine-check for acceptance — **high** — review acceptance text.

---

## Changes

### 1. `GameProgressData` — Capybara DNA prefs
- `IdleCapybaraDnaPersist` struct (`RunId`, `PetId`, `LastChoice`, `PendingChoice`)
- `SaveCapybaraDna` / `LoadCapybaraDna` keys: `NarrRun`, `NarrPet`, `NarrLastChoice`, `NarrPendingChoice`
- `ClearIdleSlice` deletes the four keys

### 2. `IdleSliceBootstrap` — PersistNow + attach restore
- Load path: when prefs hit, `LoadCapybaraDna` into `_loadedNarr*` fields
- `PersistNow`: if archetype is CapybaraGo and narrative present → `SaveCapybaraDna`
- Attach: Capybara restores DNA; cold RunId floor = `_loadedFromPrefs && _loadedNarrRunId > 0 ? loaded : 1` (ADR stays 0)

### 3. EditMode fixture
- `CapybaraGo_RunPetChoice_SurvivePersistNowReload` — PersistNow → destroy → attach asserts RunId/PetId/LastChoice/PendingChoice (+ RoomOrStep)

---

## Acceptance criteria

| Criterion (review_06_gacha P2 #1) | Result |
|----------------------------------|--------|
| Prefs save/load/clear for Run/Pet/LastChoice (+ PendingChoice) | **MET** — `SaveCapybaraDna` / `LoadCapybaraDna` / `ClearIdleSlice` |
| Bootstrap attach restore instead of hard `RunId=1` | **MET** — restore when loaded RunId > 0; else cold floor 1 |
| EditMode PersistNow→clear world→attach asserts DNA match | **MET** — fixture authored (compile clean; suite re-run deferred — HCR editor blocked / peer batchmode lock) |

---

## VERIFICATION

```
TASK: Round 08 implement 1/10 — Capybara DNA prefs reload
ROUTE: Executor against review_06_gacha P2 #1
ASSUMPTIONS: 5 high verified
CHANGES:
 - Assets/Scripts/GameProgressData.cs
 - Assets/Scripts/ECS/Authoring/IdleSliceBootstrap.cs
 - Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs
 - this receipt
VERIFICATION:
 - compile_check (rsp_replay): Assembly-CSharp + Assembly-CSharp-Editor
   errorCount=0 confidence=exact gate=r8-impl01-gacha-dna requestId=64fc42c48e8a
 - Read-back: PersistNow SaveCapybaraDna; BuildInitialState LoadCapybaraDna;
   attach RunId floor + Pet/Last/Pending restore confirmed on disk
 - EditMode fixture present: CapybaraGo_RunPetChoice_SurvivePersistNowReload
 - Fresh Batch BC filter NOT re-run this seat (HCR Unity pid dialog/lock;
   MCP pin is thepcgtoolkit only). Suite tip remains Logs/IdleBatchBC-Summary.txt
   pass=68 (pre-fixture count). New fixture adds +1 when next BC runs.
STATUS: VERIFIED (compile_check + read-back wiring)
RISKS:
 - Fixture not executed this seat (batchmode contention) — next AllSmoke/BC must include +1
 - Play Mode still UNVERIFIED
 - Optional IH/LoM entity identity fixture / dead fallbacks deferred
```

---

## Deviations

- Did not re-run Unity EditMode (editor blocked / MCP not on HCR). Acceptance machine-check is fixture + compile_check; runtime green deferred to next BC writer.
- Did not take optional P2 #2 or P3 items.

## Flash Base

None.

## Escalations

None.
