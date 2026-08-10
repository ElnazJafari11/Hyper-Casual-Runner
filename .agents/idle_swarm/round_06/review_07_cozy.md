# Round 06 Review 07 — Cozy / Assign (re-audit after R5 Neko D26)

**Scope:** A Dark Room, Cats & Soup, Neko Atsume, Fallout Shelter  
**Agent:** examine/analyze/review 7/10  
**Date:** 2026-08-10  
**Prior:** `round_01/review_07_cozy.md` → … → `round_05/review_07_cozy.md` → `round_05/impl_02_cozy.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Mode:** Review only — no implementation, no push  
**Verdict:** Round-05 P2 Neko wall-clock cats (D26 cozy face) is **closed on disk and EditMode-backed**. Core cozy verbs, D18 SurvivePersist, Cats/Fallout station AFK, and Persistence-copy honesty still hold. Residual work is deferred content fantasy, Play Mode, soft prestige→extras wipe, and soft Neko design coupling — not verb resurrection, persist gaps, or absence of Neko CatchUp.

---

## Round-05 required checklist vs current code

| Required item (R05) | Status | Evidence |
|---------------------|--------|----------|
| Neko CatchUp cats arm (`elapsed/5s` → CheckInCats + HasOfflineClaim; cap 20; no Primary) | **MET** | `IdleOfflineCatchUp.Apply` routes `NekoAtsume` → `ApplyNekoCatchUp`; commit `eef133f` |
| D23 sub-interval / full buffer → grant 0 | **MET** | `ApplyNekoCatchUp` early-return 0; fixture asserts 4s and full-cap paths |
| STAMP_POLICY Kernel B Neko rows | **MET** | `.agents/idle_swarm/round_02/STAMP_POLICY.md` Neko bank + D23 rows |
| Progress scorecard / Neko AFK one-liner | **MET** | `docs/idle-toolkit-progress.md` scorecard 18 + Neko AFK note |
| EditMode fixture `NekoAtsume_OfflineCatchUp_AccruesCheckInCats` | **MET** | Batch BC cozy-r5 + AllSmoke logs Passed |

Deferred R05 items (multi-station / multi-room / ADR atmosphere / cat variety / raids / Play Mode / Neko food-gate / narrative harden / prestige→cozy wipe / IH-AFK chest D26 remainder) correctly remain deferred.

---

## Summary table

| # | Slice | Core verbs | Representable after R5? | Empty stub? | EditMode evidence |
|---|-------|------------|-------------------------|-------------|-------------------|
| 05 | A Dark Room | Stoke → Explore → Craft | Yes + SurvivePersist narrative | No | StokeThenExplore + Craft + SurvivePersist |
| 17 | Cats & Soup | Assign → station cook → Unassign | Yes + SurvivePersist + station AFK | No | AssignCatThenSim + Workers SurvivePersist + OfflineCatchUp Primary |
| 18 | Neko Atsume | Food/Toys → wait → Check In (clears) | Yes + SurvivePersist + **wall-clock cats** | No | PlaceFood / PlaceToys / CheckIn_Clears + Cats SurvivePersist + **OfflineCatchUp AccruesCheckInCats** |
| 19 | Fallout Shelter | Assign → Pending → Claim | Yes + SurvivePersist + station AFK | No | AssignDwellerThenSim + Pending/Workers SurvivePersist + OfflineCatchUp Pending |

**Suite evidence:**
- `Logs/IdleBatchBC-impl02-cozy-r5.log` → `result=Passed pass=61 fail=0`; Neko OfflineCatchUp + station AFK + SurvivePersist cozy fixtures Passed (~04:47)
- `Logs/IdleBatchBC-Summary.txt` → `result=Passed pass=60 fail=0` (2026-08-10 ~04:54; peer churn after cozy run)
- `Logs/IdleAllSmoke-Summary.txt` → `result=Passed pass=103 fail=0` (2026-08-10 ~04:57)
- `Logs/IdleAllSmoke-impl04-r5b.log` → `NekoAtsume_OfflineCatchUp_AccruesCheckInCats => Passed` in the 103/0 AllSmoke run
- Play Mode: **still UNVERIFIED** (`docs/idle-toolkit-progress.md`)

---

## What R5 closed (confirmed on disk)

### D26 Neko wall-clock cats (cozy face)
| Contract | Behavior |
|----------|----------|
| Route | `Apply` → `ApplyNekoCatchUp` before PassiveRate gate |
| Rate | `floor(elapsed / 5s)` cats (matches online `IdleSliceSimulationSystem` 1/5s) |
| Cap | soft 20 (`NekoMaxCheckInCats`) |
| Bank | `CheckInCats` += added; `HasOfflineClaim = true` |
| Primary | **untouched** (claim pays cats ×5) |
| Cap / stamp | 8h via shared `CapSeconds`; grant 0 → no stamp wipe (D23) |
| Return | cats added (not currency) so `ApplyPersistedElapsed` stamps only when cats accrue |

### Policy / progress honesty
- STAMP_POLICY Kernel B: Neko → CheckInCats; at-cap / &lt;5s → grant 0 leave stamp.
- Progress scorecard 18: “wall-clock AFK → CheckInCats”; Persistence section has Neko AFK one-liner beside station AFK.

### Prior polish still holding
- R01: station-only cook; Fallout Pending buffer; Neko toys + clear; ADR craft — no regression.
- R02: narrative ECB / HowTo / compose toys+Pending wording — still aligned.
- R03: D18 Workers/Cats/Narr*/Pending SurvivePersist — still green.
- R04: Persistence “not two archetypes” + Cats/Fallout station AFK — still green.
- D16 claim XOR (Pending vs chest/cats) still in `IdleClaimOfflineSystem`.
- Kernel ZeroGrant fixture retargeted off Neko → CookieClicker (`impl_04_tests`) so D26 cats do not false-fail PassiveRate-gated stamp tests.

---

## Per-slice status (post–impl_02 R5)

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

### 18 — Neko Atsume — **MVP playable + session-honest + wall-clock cats**
- Food (5/− +2) + Toys (8/− +3) + passive +1 cat / 5s + check-in clear/pay.
- `CheckInCats` survives reload; claim flag restored when cats > 0.
- **Absence fantasy closed:** closed-app time accrues cats at online rate (cap 20) without Primary double-bank; EditMode fixture covers 50s→+10, cap remainder, full-buffer 0, sub-interval 0.
- Soft risks (unchanged): verbs in `ApplySliceOnlyNarrative` only (bootstrap omits `IdleNarrativeState` — OK); free timer cats without food spend (zen MVP).

### 19 — Fallout Shelter — **MVP playable + session-honest + station AFK**
- Assign → Pending buffer → Claim drain; workers + Pending survive reload.
- Wall-clock AFK with workers banks Pending (+ claim flag).
- Gaps (deferred): multi-room, SPECIAL, raids / neglect.

---

## Residual issues (Round 06 — for next implement if prioritized)

### P3 — Soft code / design (non-blocking for EditMode verbs + AFK bar)
1. Neko narrative routing depends on *absence* of `IdleNarrativeState`.
2. Neko free timer cats without food spend (online + offline both zen).
3. Cats vs Fallout share one station component; differentiation is economic + UI copy.
4. Prestige clears `AssignedWorkers` / `PendingClaim` but not station `AssignedCount` / `CheckInCats` / narrative — **cozy UI has no Prestige button**, so not demo-blocking; mid-session prestige could leave station cooking with workers=0 until reload, or leave lingering cats.
5. Station AFK rate is hard-coded to bootstrap defaults (not live station fields) — correct for pure-state path; drift risk if editor overrides OutputPerWorker/Interval.
6. Neko CatchUp ignores `GlobalMultiplier` (matches online timer; intentional consistency, not a bug).
7. No bootstrap `ApplyPersistedElapsed` end-to-end fixture for Neko cats (pure `Apply` only) — same pattern as Cats/Fallout station AFK fixtures; non-blocking.

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

---

## Evidence pointers

| Artifact | Path |
|----------|------|
| R05 review | `.agents/idle_swarm/round_05/review_07_cozy.md` |
| R05 impl receipt | `.agents/idle_swarm/round_05/impl_02_cozy.md` (`eef133f`; fixture scoop `9999780` / peer `388eb02`; progress scoop `6461b37`) |
| Neko CatchUp | `Assets/Scripts/ECS/Systems/Idle/IdleOfflineCatchUp.cs` `Apply` / `ApplyNekoCatchUp` |
| Online Neko timer | `Assets/Scripts/ECS/Systems/Idle/IdleSliceSimulationSystem.cs` `NekoAtsume` case |
| Stamp policy | `.agents/idle_swarm/round_02/STAMP_POLICY.md` Kernel B Neko rows |
| Progress Persistence | `docs/idle-toolkit-progress.md` Persistence + scorecard 05/17/18/19 + Neko AFK |
| Cozy persist API | `Assets/Scripts/GameProgressData.cs` `IdleSliceCozyPersist` / `LoadIdleCozyPersist` |
| Bootstrap save/load | `Assets/Scripts/ECS/Authoring/IdleSliceBootstrap.cs` PersistNow + BuildInitialState + AttachArchetypeExtras |
| Actions / claim XOR | `Assets/Scripts/ECS/Systems/Idle/IdleSliceActionSystems.cs` |
| Prestige wipe (partial) | `Assets/Scripts/ECS/Systems/Idle/PrestigeSystem.cs` |
| UI verbs + Pending HUD | `Assets/Scripts/UI/IdleSliceUIController.cs` |
| Tests | `Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs` (`NekoAtsume_OfflineCatchUp_AccruesCheckInCats`) |
| Batch BC / AllSmoke | `Logs/IdleBatchBC-impl02-cozy-r5.log`, `Logs/IdleBatchBC-Summary.txt`, `Logs/IdleAllSmoke-Summary.txt`, `Logs/IdleAllSmoke-impl04-r5b.log` |
| Prefabs | `Assets/ToolkitExamples/Idle/05_*`, `17_*`, `18_*`, `19_*` |
| Cross-ref D26 remainder | `.agents/idle_swarm/round_05/review_01_kernel.md` (IH/AFK chest still non-cozy) |

---

## Implement-round recommendations (do not implement in this examine)

**Required:** none for cozy/assign MVP + session + station-AFK + Neko wall-clock bar — **bar met**.

**Optional polish (only if Round 06+ targets soft honesty):**
- [ ] Wipe station `AssignedCount` / `CheckInCats` / narrative on prestige if prestige is ever wired to cozy UI.
- [ ] Optional Neko food-gate; harden Neko vs `IdleNarrativeState`.
- [ ] Optional bootstrap PersistElapsed Neko fixture (parity with pure Apply coverage).

**Deferred (content / Play Mode / other lanes):**
- [ ] Multi-station / multi-room / ADR atmosphere / cat variety / raids.
- [ ] Play Mode verification.
- [ ] IH/AFK chest CatchUp (kernel D26 remainder — not cozy).

---

## Final call

**Round-05 cozy D26 Neko wall-clock cats landed.** CatchUp arm, STAMP_POLICY, progress copy, and EditMode fixture are verified by code + Batch BC cozy-r5 / AllSmoke green lines; prior verb hardening, SurvivePersist, and Cats/Fallout station AFK hold.

**Round 06 examine verdict:** cozy/assign **pass for required verb + session + station-AFK + Neko absence bar**; next meaningful gaps are deferred **content fantasy**, **Play Mode**, and soft **prestige→extras wipe** — not Neko CatchUp resurrection, persist gaps, or stale Persistence copy.
