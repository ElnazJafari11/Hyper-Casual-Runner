# Round 02 Review 07 — Cozy / Assign (re-audit after impl_06)

**Scope:** A Dark Room, Cats & Soup, Neko Atsume, Fallout Shelter  
**Agent:** examine/analyze/review 7/10  
**Date:** 2026-08-10  
**Prior:** `round_01/review_07_cozy.md` → `round_01/impl_06_cozy.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Verdict:** Round-01 **required polish is landed and EditMode-backed**. Cozy/assign are no longer soft-stub on claim / double-pay / missing toys. Residual gaps are doc/prefab sync, Play Mode, deferred depth, and one soft narrative ECB leak — not empty verbs.

---

## Round-01 required checklist vs current code

| Required item (R01) | Status | Evidence |
|---------------------|--------|----------|
| Deduplicate assign production (continuous vs station) | **MET** | `IdleSliceSimulationSystem` Cats/Fallout cases are empty `break`; sole pay is station query |
| Fallout buffer-then-claim | **MET** | Station tick → `PendingClaim`; `IdleClaimOfflineSystem` drains buffer; HUD `Pending:` |
| Neko clear cats on check-in | **MET** | Claim sets `CheckInCats = 0`; test `NekoAtsume_CheckIn_ClearsCatsAndPays` |
| Neko toys verb | **MET** | UI `Place Toys` → narrative action 1; spend 8 / +3 cats |
| ADR craft beat | **MET** (pre-existing + test) | Craft action 2; `ADarkRoom_Craft_SpendsWoodForPassive` |

Deferred R01 items (multi-room, ADR atmosphere, cat variety) correctly remain deferred.

---

## Summary table

| # | Slice | Core verbs | Representable after impl_06? | Empty stub? | EditMode evidence |
|---|-------|------------|------------------------------|-------------|-------------------|
| 05 | A Dark Room | Stoke → Explore → Craft | Yes | No | `ADarkRoom_StokeThenExplore_*` + `ADarkRoom_Craft_*` |
| 17 | Cats & Soup | Assign → station cook → Unassign | Yes (single pay path) | No | `CatsAndSoup_AssignCatThenSim_ProducesOutput` |
| 18 | Neko Atsume | Food/Toys → wait → Check In (clears) | Yes | No | PlaceFood / PlaceToys / CheckIn_Clears* |
| 19 | Fallout Shelter | Assign → Pending → Claim | Yes (claim is real buffer) | No | `FalloutShelter_AssignDwellerThenSim_FillsStationAndAccrues` |

Batch BC aggregate: `Logs/IdleBatchBC-Summary.txt` → `result=Passed pass=29 fail=0`.  
Impl_06 cozy fixtures also appear PASS in later suite logs (e.g. `Logs/IdleProduction-impl03.log`, `Logs/IdleKernel-impl07.log`).  
Play Mode: **still UNVERIFIED**.

---

## What impl_06 fixed (confirmed on disk)

### 17 / 19 — assign pay + Fallout claim semantics
- Continuous `AssignedWorkers` CPS for Cats/Fallout removed (`IdleSliceSimulationSystem` ~90–96).
- Shared station loop (~152–181):
  - **Cats:** `PrimaryCurrency += payout` (live cook).
  - **Fallout:** `PendingClaim += payout` + `HasOfflineClaim`; Primary stays 0 until Claim.
- Differentiation is now **economic**, not only UI labels — meets R01 “pick one path” + buffer-claim.

### 18 — Neko economy honesty
- `ApplySliceOnlyNarrative` action 1 = Place Toys (cost 8, +3 cats).
- Claim clears `CheckInCats` (no re-farm without re-attract / wait).
- UI exposes Place Food / Place Toys / Check In.

### 05 — ADR
- Unchanged core loop; craft covered by new EditMode assertion (wood spend → `PassiveRate`).

### Components / UI / generator copy
- `IdleSliceState.PendingClaim` documented for Fallout/AFK-style claim.
- HUD includes `Pending: {PendingClaim}`.
- `IdleToolkitSliceGenerator` HowTo strings updated for toys / Pending / clear-cats.

---

## Per-slice status (post–impl_06)

### 05 — A Dark Room — **MVP playable**
- Bootstrap attaches `IdleNarrativeState`; UI Stoke / Explore / Craft → actions 0/1/2.
- Craft → wood spend → `PassiveRate` → sim drip still valid.
- Gaps unchanged (deferred): mystery text, villager entities, atmosphere.

### 17 — Cats & Soup — **MVP playable (hardened)**
- Assign/Unassign + station-only cook verified at `1 worker × 1.5 × 2 ticks = 3` with `PendingClaim == 0`.
- Gaps (deferred): one station, no decoration/ASMR; still shares `IdleAssignmentStation` type with Fallout.

### 18 — Neko Atsume — **MVP playable (hardened)**
- Food + Toys + passive +1 cat / 5s + check-in clear/pay.
- Residual soft risk: Neko verbs live only in `ApplySliceOnlyNarrative`; attaching `IdleNarrativeState` would route to ADR-style stoke math (bootstrap still omits it — OK).
- Gaps (deferred): cat variety / surprise collection.

### 19 — Fallout Shelter — **MVP playable (claim no longer soft stub)**
- Assign + buffer + Claim drain asserted end-to-end.
- Gaps (deferred): multi-room, SPECIAL, raids / “risk losses.”
- Claim still shares generic `IdleClaimOfflineSystem` (also AFK/Neko); behavior correct for buffer drain, identity remains shared machinery.

---

## Residual issues (Round 02 — for next implement if prioritized)

### Docs / assets out of sync with code (mechanical)
1. **`docs/idle-toolkit-compose.md`** still cites renamed/missing tests:
   - `CatsAndSoup_AssignCat_IncreasesWorkers` → actual `CatsAndSoup_AssignCatThenSim_ProducesOutput`
   - `NekoAtsume_PlaceFood_AttractsCats` → actual `NekoAtsume_PlaceFood_SpendsAndAttractsCats` (+ toys / clear tests)
   - `FalloutShelter_AssignDweller_FillsStation` → actual `FalloutShelter_AssignDwellerThenSim_FillsStationAndAccrues`
   - Neko row omits Place Toys; Fallout row omits PendingClaim semantics.
2. **`docs/idle-play-smoke-checklist.md`** Neko row still `Place Food → Check In` (no Toys); Fallout wording predates Pending.
3. **Prefabs HowTo** not regenerated to match generator:
   - `18_NekoAtsume_CheckIn_Slice.prefab` still “Place food → wait → check in” (no toys / clear).
   - `19_FalloutShelter_Dwellers_Slice.prefab` still generic “production ticks → claim” (not Pending drain).
   - Cats/ADR HowTo close enough; generator is ahead of baked prefabs.

### Soft code defects (non-blocking for EditMode verbs)
4. **`IdleNarrativeActionSystem` never `Playback`/`Dispose`s its ECB** after `DestroyEntity` — event entities disabled but not destroyed (ADR + Neko + Capybara narrative path). Slice mutations still apply via `EntityManager` (tests pass). Fix: mirror Assign/Claim systems’ ECB teardown.
5. **Offline catchup does not simulate station ticks** (`OfflineSimulationSystem` only banks `PassiveRate > 0` into `PendingClaim`). Cats/Fallout room production is **online-sim only** — acceptable at MVP bar; note vs “absence” fantasy for Fallout.

### Still deferred (quality bar)
6. Multi-station / multi-room layouts.
7. ADR narrative content / atmosphere.
8. Cat collection variety, decoration, survival incidents.
9. Play Mode smoke for all four.

### Not regressions / not stubs
- No new `TODO: [STUB]` on cozy verbs; UI stubs remain PanelSettings / inline styles only.
- Double-pay and Fallout “claim = free offline bonus” issues from R01 are **gone**.

---

## Evidence pointers

| Artifact | Path |
|----------|------|
| R01 review | `.agents/idle_swarm/round_01/review_07_cozy.md` |
| R01 impl receipt | `.agents/idle_swarm/round_01/impl_06_cozy.md` |
| Sim (dedupe + buffer) | `Assets/Scripts/ECS/Systems/Idle/IdleSliceSimulationSystem.cs` ~90–96, 152–181 |
| Actions (toys / claim clear / pending drain) | `Assets/Scripts/ECS/Systems/Idle/IdleSliceActionSystems.cs` ~254–268, 343–369 |
| `PendingClaim` | `Assets/Scripts/ECS/Components/IdleSliceComponents.cs` |
| UI | `Assets/Scripts/UI/IdleSliceUIController.cs` ~279–282, 328–340, 393 |
| Generator HowTo | `Assets/Scripts/Editor/IdleToolkitSliceGenerator.cs` ~47–49 |
| Tests | `Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs` (cozy fixtures ~513–625, 721–740) |
| Batch BC summary | `Logs/IdleBatchBC-Summary.txt` (`pass=29 fail=0`) |
| Prefabs | `Assets/ToolkitExamples/Idle/05_*`, `17_*`, `18_*`, `19_*` |

---

## Implement-round recommendations (do not implement in this examine)

**Required (only if Round 02 polish targets cozy again):**
- [ ] Regenerate or hand-sync prefab HowTo + play-smoke / compose docs to post–impl_06 verbs and test names.
- [ ] Fix `IdleNarrativeActionSystem` ECB `Playback` + `Dispose` (entity leak on narrative fires).

**Deferred (MVP bar already met for required R01 polish):**
- [ ] Offline station catchup for Cats/Fallout (or document online-only).
- [ ] Multi-station / multi-room / ADR atmosphere / cat variety / raids.
- [ ] Play Mode verification.

---

## Final call

**impl_06 closed the Round-01 required cozy gaps.** Cats/Fallout no longer double-pay; Fallout Claim is a real `PendingClaim` drain; Neko has toys and honest clear-on-check-in; ADR craft is test-locked. All four remain thin shared-MVP slices — but they are **representable core-verb MVPs**, not empty stubs, and the prior soft-stub claim on Fallout no longer holds.

**Round 02 examine verdict:** cozy/assign **pass for required bar**; leftover work is sync/docs, soft ECB hygiene, and deferred depth / Play Mode — not verb resurrection.
