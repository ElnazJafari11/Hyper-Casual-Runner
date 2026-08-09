# Round 01 Review 07 — Cozy / Assign

**Scope:** A Dark Room, Cats & Soup, Neko Atsume, Fallout Shelter  
**Agent:** examine/analyze/review 7/10  
**Date:** 2026-08-10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Verdict:** Core verbs are **representable and EditMode-playable MVPs**, not empty stubs. Depth is shared-machinery shallow; Play Mode unverified.

---

## Summary

| # | Slice | Core verbs (matrix) | Representable? | Empty stub? | EditMode evidence |
|---|-------|---------------------|----------------|-------------|-------------------|
| 05 | A Dark Room | Stoke → Explore (+ Craft / passive) | Yes | No | `ADarkRoom_StokeThenExplore_AdvancesProgression` PASS |
| 17 | Cats & Soup | Assign cats → passive cook | Yes | No | `CatsAndSoup_AssignCat_IncreasesWorkers` PASS |
| 18 | Neko Atsume | Place food → wait → check-in | Yes | No | `NekoAtsume_PlaceFood_AttractsCats` PASS |
| 19 | Fallout Shelter | Assign dwellers → produce → claim | Yes (claim shallow) | No | `FalloutShelter_AssignDweller_FillsStation` PASS |

Batch BC aggregate: `Logs/IdleBatchBC-Summary.txt` → `pass=14 fail=0`.  
Play Mode: **UNVERIFIED** (`docs/idle-toolkit-progress.md` — MCP not on this project).

---

## Shared architecture (not stubbed)

All four route through the idle MVP spine:

| Layer | Files | Role |
|-------|-------|------|
| Prefabs | `Assets/ToolkitExamples/Idle/05_*`, `17_*`, `18_*`, `19_*` | Archetype + HowTo + UIDocument hub |
| Bootstrap | `IdleSliceBootstrap.AttachArchetypeExtras` | Spawns extras per archetype |
| Actions | `IdleSliceActionSystems.cs` | Assign / Narrative / Claim |
| Sim | `IdleSliceSimulationSystem.cs` | Passive / station ticks / cat timer |
| UI | `IdleSliceUIController.AddButtonsForArchetype` | Per-archetype buttons → fire events |

`// TODO: [STUB]` appears only once in this path: runtime `PanelSettings` fallback in UI — not the verbs themselves.

---

## 05 — A Dark Room (`IdleArchetype.ADarkRoom`)

### Intended loop
Stoke fire (×5) → Explore unlocks → Craft spends wood for passive (HowTo on prefab; matrix: Stoke → Explore, villagers as automation).

### Wiring
- **Bootstrap:** `IdleNarrativeState` (stoke/explore/wood).
- **UI:** Stoke Fire / Explore / Craft → `IdleNarrativeActionEvent` 0/1/2.
- **System:** `IdleNarrativeActionSystem.ApplyNarrative`
  - 0: +stoke, +wood, unlock explore at 5
  - 1: gated until unlock; advances `RoomOrStep` / `ProgressionLevel`
  - 2: spend 3 wood → `PassiveRate` + mult
- **Sim:** passive currency if `PassiveRate > 0` after craft.

### Playable?
**Yes.** Gated explore + craft→passive is a real mini-loop. Fallback `ApplySliceOnlyNarrative` exists if narrative component missing (bootstrap does attach it).

### Gaps (not empty — thin)
- No mystery text / rooms as content; progression is numeric `RoomOrStep`.
- “Villagers gather” = craft bumps `PassiveRate`, not worker entities.
- Atmosphere / narrative reward loop not represented.

### Status: **MVP playable** (EditMode); Play Mode pending.

---

## 17 — Cats & Soup (`IdleArchetype.CatsAndSoup`)

### Intended loop
Assign cats to stations → they cook passively → unassign freely.

### Wiring
- **Bootstrap:** `IdleAssignmentStation` (capacity = `MaxWorkers`, usually 5).
- **UI:** Assign Cat / Unassign → `IdleAssignWorkerEvent` ±1.
- **System:** `IdleAssignWorkerSystem` updates `AssignedWorkers` + station `AssignedCount`.
- **Sim:** continuous `AssignedWorkers * rate * dt` **and** discrete station interval payout (double path — see Issues).

### Playable?
**Yes.** Assign/unassign + passive income is the core verb and is implemented end-to-end.

### Gaps
- One station only; no facility unlock / decoration / ASMR (matrix flavor).
- Identical machinery to Fallout Shelter (label swap).

### Status: **MVP playable** (EditMode); Play Mode pending.

---

## 18 — Neko Atsume (`IdleArchetype.NekoAtsume`)

### Intended loop
Place food → wait for cats → check in to collect (zen idle). Matrix: Place Food/Toys; pure passive check-in.

### Wiring
- **Bootstrap:** no narrative/station extra; starts with ≥20 currency; uses `CheckInCats` / `HasOfflineClaim` on `IdleSliceState`.
- **UI:** Place Food → narrative action 0; Check In → `IdleClaimOfflineEvent`.
- **Place food:** `ApplySliceOnlyNarrative` — spend 5 currency, +2 cats, set claim flag (only when no `IdleNarrativeState`).
- **Wait:** sim every 5s → +1 cat (cap 20), set claim flag.
- **Check-in:** `IdleClaimOfflineSystem` pays `CheckInCats * 5` (+ chest term).

### Playable?
**Yes.** Food → accumulate cats → claim is wired. Passive timer supports absence-as-mechanic at MVP scale.

### Gaps / bugs (verbs work; economy soft)
- No toys verb (matrix lists Food/Toys).
- No cat variety / surprise — only a counter.
- Claim does **not** clear `CheckInCats` (“cats linger”), so check-in can be re-farmed without re-placing food once cats > 0.
- Place Food uses narrative action 0; would mis-fire if an `IdleNarrativeState` existed on the same world (bootstrap correctly omits it for Neko).

### Status: **MVP playable** (EditMode); Play Mode pending.

---

## 19 — Fallout Shelter (`IdleArchetype.FalloutShelter`)

### Intended loop
Assign dwellers to rooms → production ticks → claim output. Matrix: Assign; dwellers auto-produce; survival tension.

### Wiring
- **Bootstrap / assign / sim:** same as Cats & Soup (`IdleAssignmentStation` + shared assign/sim cases).
- **UI:** Assign Dweller / Unassign / **Claim Production**.
- **Claim:** generic `IdleClaimOfflineSystem` — not a room storage drain.

### Playable?
**Assign + passive production: yes.**  
**Claim: wired but semantically weak** — production already accrues into `PrimaryCurrency` continuously; Claim grants a bonus offline-style payout (or a free 10 if no claim flags), not “empty room storage.”

### Gaps
- No multi-room layout, SPECIAL stats, incidents/raids, or “check in or risk losses.”
- Nearly a clone of Cats & Soup + Claim button.

### Status: **MVP playable for assign; claim is a soft stub of room-claim fantasy** (still not an empty button — system runs).

---

## Cross-cutting issues (for implement round)

1. **Cats & Fallout double production** — `IdleSliceSimulationSystem` pays both continuous `AssignedWorkers` CPS and `IdleAssignmentStation` interval ticks. Functional but inflated; pick one path.
2. **Fallout Claim ≠ stored output** — either buffer station output for claim, or rename UI to match offline-bonus behavior.
3. **Neko check-in economy** — clear or decay `CheckInCats` on claim if zen pacing matters.
4. **Archetype differentiation** — 17 and 19 share one component + switch cases; cozy identity is UI copy only.
5. **Play Mode gap** — EditMode smoke ≠ in-editor play; progress doc marks Play column Pending for all four.
6. **No `TODO: [STUB]` on these verbs** — do not treat as unimplemented; treat as shallow MVP.

---

## Evidence pointers

| Artifact | Path |
|----------|------|
| Matrix | `docs/idle_mechanics_matrix.md` (rows ADR / Cats / Neko / Fallout) |
| Progress | `docs/idle-toolkit-progress.md` rows 5, 17–19 |
| Components | `Assets/Scripts/ECS/Components/IdleSliceComponents.cs` |
| Bootstrap extras | `IdleSliceBootstrap.cs` ~252–288 |
| Actions | `IdleSliceActionSystems.cs` (Assign, Narrative, Claim) |
| Sim | `IdleSliceSimulationSystem.cs` ~71–85, 106–117 |
| UI | `IdleSliceUIController.cs` ~131–135, 178–190 |
| Tests | `IdleBatchBCSmokeTests.cs` |
| Logs | `Logs/IdleBatchBC-api.log` (four PASS lines); `Logs/IdleBatchBC-Summary.txt` |

---

## Implement-round recommendations (do not implement in Round 01 examine)

**Required (if polish pass targets cozy):**
- [ ] Deduplicate assign production (continuous vs station tick).
- [ ] Fallout: buffer-then-claim OR document Claim as offline bonus.
- [ ] Neko: clear cats on check-in; optional toys as second narrative action.

**Deferred (quality bar = MVP slice):**
- [ ] Multi-station / multi-room.
- [ ] Narrative text / atmosphere for A Dark Room.
- [ ] Cat collection variety, decoration, survival incidents.

---

## Final call

**None of the four cozy/assign slices are empty stubs.** Prefabs, UI verbs, ECS events, systems, and Batch BC tests all exist and pass EditMode smoke for the primary beat of each title. They are **thin shared-MVP representations**: playable enough to demonstrate the verb, too shallow to feel like the named games — especially Fallout’s claim and Cats/Fallout sameness.
