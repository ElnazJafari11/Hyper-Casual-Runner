# Round 07 Review 07 — Cozy / Assign (re-audit after R6 verify-only)

**Scope:** A Dark Room, Cats & Soup, Neko Atsume, Fallout Shelter  
**Agent:** examine/analyze/review 7/10  
**Date:** 2026-08-10  
**Prior:** `round_01/review_07_cozy.md` → … → `round_06/review_07_cozy.md` → `round_06/impl_09_cozy.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Mode:** Review only — no implementation, no push  
**Verdict:** Round-06 required work was empty and stayed empty; D26 Neko wall-clock cats + station AFK + SurvivePersist still **green on disk and EditMode**. Soft optional live TrySpawn CatchUp fixture (R6 P2) was **closed by peer production** (`ed123af`), not by cozy `impl_09`. Residual work is deferred content fantasy, Play Mode, soft prestige→extras wipe, and soft Neko design coupling — not verb resurrection, CatchUp gaps, or missing live-pipe coverage.

---

## Round-06 required checklist vs current code

| Required item (R06) | Status | Evidence |
|---------------------|--------|----------|
| Required cozy code | **N/A — none** | `review_07_cozy.md` “Required: none”; `impl_09_cozy.md` verify-only |
| D26 EditMode reconfirm | **MET** | `NekoAtsume_OfflineCatchUp_AccruesCheckInCats` Passed in `IdleBatchBC-impl09-cozy-r6c.log` |
| Station AFK + SurvivePersist cozy fixtures | **MET** | Cats/Fallout OfflineCatchUp + all four SurvivePersist fixtures Passed (same log) |
| Soft prestige extras wipe | **still open** (skipped) | `PrestigeSystem` clears `AssignedWorkers`/`PendingClaim` only; no `AssignedCount` / `CheckInCats` / narrative wipe |
| Soft Neko food-gate / narrative harden | **still open** (skipped) | verbs still in `ApplySliceOnlyNarrative`; free timer cats unchanged |
| Soft bootstrap PersistElapsed Neko fixture | **MET (peer)** | `NekoAtsume_SpawnBootstrap_TrySpawn_AccruesCheckInCatsAndPersists` via `ed123af` (production); Passed in Batch BC r6c |

Deferred R06 items (multi-station / multi-room / ADR atmosphere / cat variety / raids / Play Mode / IH-AFK chest D26 remainder) correctly remain deferred.

---

## Summary table

| # | Slice | Core verbs | Representable after R6? | Empty stub? | EditMode evidence |
|---|-------|------------|-------------------------|-------------|-------------------|
| 05 | A Dark Room | Stoke → Explore → Craft | Yes + SurvivePersist narrative | No | StokeThenExplore + Craft + SurvivePersist |
| 17 | Cats & Soup | Assign → station cook → Unassign | Yes + SurvivePersist + station AFK | No | AssignCatThenSim + Workers SurvivePersist + OfflineCatchUp Primary |
| 18 | Neko Atsume | Food/Toys → wait → Check In (clears) | Yes + SurvivePersist + wall-clock cats + **live TrySpawn** | No | PlaceFood / PlaceToys / CheckIn_Clears + Cats SurvivePersist + OfflineCatchUp + **SpawnBootstrap TrySpawn** |
| 19 | Fallout Shelter | Assign → Pending → Claim | Yes + SurvivePersist + station AFK | No | AssignDwellerThenSim + Pending/Workers SurvivePersist + OfflineCatchUp Pending |

**Suite evidence:**
- `Logs/IdleBatchBC-impl09-cozy-r6c.log` / `Logs/IdleBatchBC-Summary.txt` → `result=Passed pass=64 fail=0` (2026-08-10 ~05:13); all cozy fixtures below Passed
- `Logs/IdleAllSmoke-Summary.txt` → `result=Passed pass=103 fail=0` (2026-08-10 ~04:57; tip predates Batch BC 64 count; AllSmoke ≠ Batch BC)
- Play Mode: **still UNVERIFIED** (`docs/idle-toolkit-progress.md` — MCP not on Hyper-Casual-Runner)

Cozy fixture lines confirmed Passed in Batch BC r6c:
- `ADarkRoom_StokeThenExplore` / `Craft` / `NarrativeWoodStoke_SurvivePersistNowReload`
- `CatsAndSoup_AssignCatThenSim` / `AssignedWorkers_SurvivePersist` / `OfflineCatchUp_StationWorkers_AddsPrimary`
- `NekoAtsume_PlaceFood` / `PlaceToys` / `CheckIn_Clears` / `CheckInCats_SurvivePersist` / `OfflineCatchUp_AccruesCheckInCats` / **`SpawnBootstrap_TrySpawn_AccruesCheckInCatsAndPersists`**
- `FalloutShelter_AssignDwellerThenSim` / `PendingAndWorkers_SurvivePersist` / `OfflineCatchUp_StationWorkers_BanksPendingClaim`

---

## What R6 closed / confirmed

### Cozy `impl_09` (verify-only, commit `cd92610`)
- No `Assets/` cozy edits — correct for empty required list.
- Reconfirmed `Apply` → `ApplyNekoCatchUp` (floor elapsed/5s → CheckInCats cap 20; no Primary; D23 zero paths).
- Soft polish intentionally skipped (prestige wipe / food-gate / narrative harden).

### Peer production close of soft P2 (`ed123af`)
| Contract | Behavior |
|----------|----------|
| Fixture | `NekoAtsume_SpawnBootstrap_TrySpawn_AccruesCheckInCatsAndPersists` |
| Path | prefs + T−50s → live `IdleSliceBootstrap.TrySpawn` CatchUp → PersistNow flush |
| Asserts | CheckInCats=10; Primary untouched (20); HasOfflineClaim; cold `LoadIdleCozyPersist` cats match |
| Note | Closes R6 soft “pure Apply only” gap; progress.md still does not name this fixture (soft doc lag) |

### Prior bars still holding (read-back)
- D26 CatchUp arm + STAMP_POLICY Neko rows + progress Neko AFK one-liner — unchanged.
- R01–R05 verb / ECB / HowTo / D18 SurvivePersist / station AFK / Persistence-copy honesty / D16 claim XOR — no regression on disk.
- Kernel ZeroGrant stays off Neko (CookieClicker) so D26 cats do not false-fail PassiveRate stamp tests.

---

## Per-slice status (post–impl_09 R6 + peer fixture)

### 05 — A Dark Room — **MVP playable + session-honest**
- Stoke / Explore / Craft via `IdleNarrativeState`; craft → wood → `PassiveRate` drip.
- Narrative extras survive PersistNow → reload (EditMode).
- Soft: after craft, Kernel B PassiveRate path banks Primary on relaunch — intentional drip fantasy.
- Gaps (deferred): mystery text, villagers, atmosphere.

### 17 — Cats & Soup — **MVP playable + session-honest + station AFK**
- Assign/Unassign + station-only cook (`PendingClaim == 0`); workers survive reload.
- Wall-clock AFK with workers banks Primary at bootstrap station rate.
- Gaps (deferred): one station; no decoration/ASMR; shared `IdleAssignmentStation` type with Fallout.
- Soft: station `Timer` resets on respawn (first online tick delayed) — non-blocking.

### 18 — Neko Atsume — **MVP playable + session-honest + wall-clock cats + live TrySpawn**
- Food (5/− +2) + Toys (8/− +3) + passive +1 cat / 5s + check-in clear/pay.
- `CheckInCats` survives reload; claim flag restored when cats > 0.
- Absence fantasy: closed-app time accrues cats at online rate (cap 20) without Primary double-bank.
- **Live pipe:** TrySpawn CatchUp→PersistNow now EditMode-backed (peer fixture) — stronger than R6 examine.
- Soft risks (unchanged): verbs in `ApplySliceOnlyNarrative` only (bootstrap omits `IdleNarrativeState` — OK); free timer cats without food spend (zen MVP).

### 19 — Fallout Shelter — **MVP playable + session-honest + station AFK**
- Assign → Pending buffer → Claim drain; workers + Pending survive reload.
- Wall-clock AFK with workers banks Pending (+ claim flag).
- Gaps (deferred): multi-room, SPECIAL, raids / neglect.

---

## Residual issues (Round 07 — for next implement if prioritized)

### P3 — Soft code / design (non-blocking for EditMode verbs + AFK bar)
1. Neko narrative routing depends on *absence* of `IdleNarrativeState`.
2. Neko free timer cats without food spend (online + offline both zen).
3. Cats vs Fallout share one station component; differentiation is economic + UI copy.
4. Prestige clears `AssignedWorkers` / `PendingClaim` but not station `AssignedCount` / `CheckInCats` / narrative — **cozy UI has no Prestige button**, so not demo-blocking; mid-session prestige could leave station cooking with workers=0 until reload, or leave lingering cats.
5. Station AFK rate is hard-coded to bootstrap defaults (not live station fields) — correct for pure-state path; drift risk if editor overrides OutputPerWorker/Interval.
6. Neko CatchUp ignores `GlobalMultiplier` (matches online timer; intentional consistency, not a bug).
7. Progress Persistence section does not yet name the live TrySpawn Neko fixture (soft doc honesty; fixture itself is green).

### Still deferred (quality bar)
8. Multi-station / multi-room layouts.
9. ADR narrative content / atmosphere.
10. Cat collection variety, decoration, survival incidents / neglect.
11. Play Mode smoke for all four.
12. **Kernel D26 remainder** (IH / AfkArena closed-app chest CatchUp) — out of cozy lane; cross-ref kernel reviews.

### Not regressions / not stubs
- No `TODO: [STUB]` on cozy verbs (UI PanelSettings / inline styles / Antimatter only).
- R01 double-pay and soft Fallout claim remain **gone**.
- R02 ECB leak and HowTo drift remain **gone**.
- R03 persist omit extras remain **gone**.
- R04 “two archetypes” Persistence copy and Cats/Fallout online-only AFK remain **gone**.
- R05 Neko online-only cat timer / missing CatchUp arm remain **gone**.
- R06 “pure Apply only” live-pipe hole for Neko CatchUp remain **gone** (peer fixture).

---

## Evidence pointers

| Artifact | Path |
|----------|------|
| R06 review | `.agents/idle_swarm/round_06/review_07_cozy.md` |
| R06 cozy impl | `.agents/idle_swarm/round_06/impl_09_cozy.md` (`cd92610`; verify-only) |
| Peer TrySpawn fixture | commit `ed123af` + `IdleBatchBCSmokeTests.NekoAtsume_SpawnBootstrap_TrySpawn_AccruesCheckInCatsAndPersists` |
| Neko CatchUp | `Assets/Scripts/ECS/Systems/Idle/IdleOfflineCatchUp.cs` `Apply` / `ApplyNekoCatchUp` |
| Online Neko timer | `Assets/Scripts/ECS/Systems/Idle/IdleSliceSimulationSystem.cs` `NekoAtsume` case |
| Stamp policy | `.agents/idle_swarm/round_02/STAMP_POLICY.md` Kernel B Neko rows |
| Progress Persistence | `docs/idle-toolkit-progress.md` Persistence + scorecard 05/17/18/19 + Neko AFK |
| Cozy persist API | `Assets/Scripts/GameProgressData.cs` `IdleSliceCozyPersist` / `LoadIdleCozyPersist` |
| Bootstrap save/load | `Assets/Scripts/ECS/Authoring/IdleSliceBootstrap.cs` PersistNow + BuildInitialState + AttachArchetypeExtras |
| Actions / claim XOR | `Assets/Scripts/ECS/Systems/Idle/IdleSliceActionSystems.cs` |
| Prestige wipe (partial) | `Assets/Scripts/ECS/Systems/Idle/PrestigeSystem.cs` |
| UI verbs + Pending HUD | `Assets/Scripts/UI/IdleSliceUIController.cs` |
| Tests | `Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs` |
| Batch BC / AllSmoke | `Logs/IdleBatchBC-impl09-cozy-r6c.log`, `Logs/IdleBatchBC-Summary.txt`, `Logs/IdleAllSmoke-Summary.txt` |
| Prefabs | `Assets/ToolkitExamples/Idle/05_*`, `17_*`, `18_*`, `19_*` |
| Cross-ref D26 remainder | kernel reviews (IH/AFK chest still non-cozy) |

---

## Implement-round recommendations (do not implement in this examine)

**Required:** none for cozy/assign MVP + session + station-AFK + Neko wall-clock + live TrySpawn bar — **bar met**.

**Optional polish (only if Round 07+ targets soft honesty):**
- [ ] Wipe station `AssignedCount` / `CheckInCats` / narrative on prestige if prestige is ever wired to cozy UI.
- [ ] Optional Neko food-gate; harden Neko vs `IdleNarrativeState`.
- [ ] Name live TrySpawn Neko fixture in `docs/idle-toolkit-progress.md` Persistence fixtures list (doc-only).

**Deferred (content / Play Mode / other lanes):**
- [ ] Multi-station / multi-room / ADR atmosphere / cat variety / raids.
- [ ] Play Mode verification.
- [ ] IH/AFK chest CatchUp (kernel D26 remainder — not cozy).

---

## Final call

**Round-06 cozy was correctly verify-only; D26 still green.** Peer production closed the soft live TrySpawn CatchUp fixture that cozy `impl_09` skipped; Batch BC 64/0 includes it. Prior verb hardening, SurvivePersist, and Cats/Fallout station AFK hold.

**Round 07 examine verdict:** cozy/assign **pass for required verb + session + station-AFK + Neko absence + live TrySpawn bar**; next meaningful gaps are deferred **content fantasy**, **Play Mode**, and soft **prestige→extras wipe** — not CatchUp resurrection, persist gaps, or missing bootstrap pipe coverage.
