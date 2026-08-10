# Round 05 Review 07 — Cozy / Assign (re-audit after R4)

**Scope:** A Dark Room, Cats & Soup, Neko Atsume, Fallout Shelter  
**Agent:** examine/analyze/review 7/10  
**Date:** 2026-08-10  
**Prior:** `round_01/review_07_cozy.md` → `impl_06` → `round_02/review_07_cozy.md` → `impl_07` → `round_03/review_07_cozy.md` → `impl_07` → `round_04/review_07_cozy.md` → `round_04/impl_07_cozy.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Mode:** Review only — no implementation, no push  
**Verdict:** Round-04 required Persistence-copy honesty and optional station AFK are **closed on disk and EditMode-backed**. Core cozy verbs + D18 SurvivePersist still hold. Residual work is deferred content/Play Mode, soft prestige→extras wipe, and Neko wall-clock cats (kernel D26 face) — not verb resurrection, persist gaps, or stale “two archetypes” copy.

---

## Round-04 required checklist vs current code

| Required item (R04) | Status | Evidence |
|---------------------|--------|----------|
| Refresh `docs/idle-toolkit-progress.md` Persistence (not “two archetypes”) | **MET** | Persistence section names D18 extras + SurvivePersist fixtures; scorecard 05/17/18/19 mention SurvivePersist / station AFK |
| Document Cats/Fallout station bank under stamp policy | **MET** | `STAMP_POLICY.md` Kernel B rows: Cats → Primary; Fallout → Pending; 0 workers grant 0 |
| Station AFK Cats → Primary | **MET** | `IdleOfflineCatchUp.ApplyStationCatchUp`; `CatsAndSoup_OfflineCatchUp_StationWorkers_AddsPrimary` (2×1.5×10=30) |
| Station AFK Fallout → PendingClaim | **MET** | Same path; `FalloutShelter_OfflineCatchUp_StationWorkers_BanksPendingClaim` (1×1.5×2×8=24) |
| Zero workers preserves D23 | **MET** | `ApplyStationCatchUp` early-return 0; Cats fixture asserts zero-worker grant 0 |
| EditMode fixtures green | **MET** | R4 cozy log + current Batch BC / AllSmoke summaries |

Deferred R04 items (multi-station / multi-room / ADR atmosphere / cat variety / raids / Play Mode / Neko food-gate / narrative harden / prestige→cozy wipe) correctly remain deferred.

---

## Summary table

| # | Slice | Core verbs | Representable after R4? | Empty stub? | EditMode evidence |
|---|-------|------------|-------------------------|-------------|-------------------|
| 05 | A Dark Room | Stoke → Explore → Craft | Yes + SurvivePersist narrative | No | StokeThenExplore + Craft + SurvivePersist |
| 17 | Cats & Soup | Assign → station cook → Unassign | Yes + SurvivePersist + station AFK | No | AssignCatThenSim + Workers SurvivePersist + OfflineCatchUp Primary |
| 18 | Neko Atsume | Food/Toys → wait → Check In (clears) | Yes + SurvivePersist cats | No | PlaceFood / PlaceToys / CheckIn_Clears + Cats SurvivePersist |
| 19 | Fallout Shelter | Assign → Pending → Claim | Yes + SurvivePersist + station AFK | No | AssignDwellerThenSim + Pending/Workers SurvivePersist + OfflineCatchUp Pending |

**Suite evidence:**
- `Logs/IdleBatchBC-Summary.txt` → `result=Passed pass=58 fail=0` (2026-08-10 ~04:05)
- `Logs/IdleBatchBC-impl07-cozy-r4.log` → all cozy verb + SurvivePersist + OfflineCatchUp fixtures `Passed`
- `Logs/IdleAllSmoke-Summary.txt` → `result=Passed pass=91 fail=0` (2026-08-10 ~04:12)
- Play Mode: **still UNVERIFIED** (`docs/idle-toolkit-progress.md`)

---

## What R4 closed (confirmed on disk)

### Persistence copy honesty
- Progress Persistence no longer claims round-trip covers only “two” archetypes.
- Scorecard rows 05/17/18/19 mention SurvivePersist; 17/19 also note station AFK destinations.
- Scoop commit noted in receipt: `6da58d0` (progress sync); cozy AFK commit `ad4e712`.

### Station AFK (Kernel B)
| Archetype | Condition | Bank | Cap / stamp |
|-----------|-----------|------|-------------|
| Cats & Soup | `AssignedWorkers > 0` | `PrimaryCurrency` | 8h; grant 0 → no stamp wipe (D23) |
| Fallout Shelter | `AssignedWorkers > 0` | `PendingClaim` + `HasOfflineClaim` | same |
| Rate | `workers × 1.5 × GlobalMultiplier / 1s` | Matches bootstrap station defaults (pure-state path; no live `IdleAssignmentStation` read) |

### Prior polish still holding
- R01: station-only cook; Fallout Pending buffer; Neko toys + clear; ADR craft — no regression.
- R02: narrative ECB `Playback`/`Dispose`; prefab HowTo + compose toys/Pending wording — still aligned.
- R03: D18 Workers/Cats/Narr*/Pending SurvivePersist + AttachArchetypeExtras restore — still green.
- D16 claim XOR (Pending vs chest/cats) still in `IdleClaimOfflineSystem`.

---

## Per-slice status (post–impl_07 R4)

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

### 18 — Neko Atsume — **MVP playable + session-honest (online cats)**
- Food (5/− +2) + Toys (8/− +3) + passive +1 cat / 5s + check-in clear/pay.
- `CheckInCats` survives reload; claim flag restored when cats > 0.
- **Absence gap (still open):** `IdleOfflineCatchUp` does not grow cats for Neko (`PassiveRate` stays 0 → grant 0; D23 preserves stamp). Zen check-in buffer only accrues while sim runs — kernel D26 cozy face.
- Soft risks (unchanged): verbs in `ApplySliceOnlyNarrative` only (bootstrap omits `IdleNarrativeState` — OK); free timer cats without food spend (zen MVP).

### 19 — Fallout Shelter — **MVP playable + session-honest + station AFK**
- Assign → Pending buffer → Claim drain; workers + Pending survive reload.
- Wall-clock AFK with workers banks Pending (+ claim flag).
- Gaps (deferred): multi-room, SPECIAL, raids / neglect.

---

## Residual issues (Round 05 — for next implement if prioritized)

### P2 — Neko absence fantasy (cozy face of kernel D26)
1. **Neko wall-clock does not accrue `CheckInCats`.** Persist preserves existing cats; closed-app time does not add cats. Optional: cheap CatchUp arm (`elapsed / 5s` capped) into CheckInCats + HasOfflineClaim, or document “online-only cat timer” next to station AFK in progress/STAMP_POLICY.

### P3 — Soft code / design (non-blocking for EditMode verbs)
2. Neko narrative routing depends on *absence* of `IdleNarrativeState`.
3. Neko free timer cats without food spend.
4. Cats vs Fallout share one station component; differentiation is economic + UI copy.
5. Prestige clears `AssignedWorkers` / `PendingClaim` but not station `AssignedCount` / `CheckInCats` / narrative — **cozy UI has no Prestige button**, so not demo-blocking; mid-session prestige could leave station cooking with workers=0 until reload.
6. Station AFK rate is hard-coded to bootstrap defaults (not live station fields) — correct for pure-state path; drift risk if editor overrides OutputPerWorker/Interval.
7. Generator play-smoke table rows for 17/19 slightly terser than seeded checklist (verbs still present); Cats checklist omits “station cook / AFK” wording.

### Still deferred (quality bar)
8. Multi-station / multi-room layouts.
9. ADR narrative content / atmosphere.
10. Cat collection variety, decoration, survival incidents / neglect.
11. Play Mode smoke for all four.

### Not regressions / not stubs
- No `TODO: [STUB]` on cozy verbs (UI PanelSettings / inline styles / Antimatter only).
- R01 double-pay and soft Fallout claim remain **gone**.
- R02 ECB leak and HowTo drift remain **gone**.
- R03 persist omit extras remain **gone**.
- R04 “two archetypes” Persistence copy and Cats/Fallout online-only AFK remain **gone**.

---

## Evidence pointers

| Artifact | Path |
|----------|------|
| R04 review | `.agents/idle_swarm/round_04/review_07_cozy.md` |
| R04 impl receipt | `.agents/idle_swarm/round_04/impl_07_cozy.md` (`ad4e712`; progress scoop `6da58d0`) |
| Station AFK | `Assets/Scripts/ECS/Systems/Idle/IdleOfflineCatchUp.cs` `Apply` / `ApplyStationCatchUp` |
| Stamp policy | `.agents/idle_swarm/round_02/STAMP_POLICY.md` Kernel B Cats/Fallout rows |
| Progress Persistence | `docs/idle-toolkit-progress.md` Persistence + scorecard 05/17/18/19 |
| Cozy persist API | `Assets/Scripts/GameProgressData.cs` `IdleSliceCozyPersist` / `LoadIdleCozyPersist` |
| Bootstrap save/load | `Assets/Scripts/ECS/Authoring/IdleSliceBootstrap.cs` PersistNow + BuildInitialState + AttachArchetypeExtras |
| Sim dedupe + buffer | `Assets/Scripts/ECS/Systems/Idle/IdleSliceSimulationSystem.cs` |
| Actions / claim XOR | `Assets/Scripts/ECS/Systems/Idle/IdleSliceActionSystems.cs` |
| Prestige wipe (partial) | `Assets/Scripts/ECS/Systems/Idle/PrestigeSystem.cs` |
| UI verbs + Pending HUD | `Assets/Scripts/UI/IdleSliceUIController.cs` |
| Tests | `Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs` |
| Batch BC / AllSmoke | `Logs/IdleBatchBC-Summary.txt`, `Logs/IdleBatchBC-impl07-cozy-r4.log`, `Logs/IdleAllSmoke-Summary.txt` |
| Prefabs | `Assets/ToolkitExamples/Idle/05_*`, `17_*`, `18_*`, `19_*` |
| Cross-ref D26 | `.agents/idle_swarm/round_04/review_01_kernel.md` |

---

## Implement-round recommendations (do not implement in this examine)

**Required (only if Round 05 polish targets cozy absence honesty):**
- [ ] Optional Neko CatchUp cats arm **or** document online-only cat timer beside station AFK in progress / STAMP_POLICY.
- [ ] Optionally wipe station `AssignedCount` / `CheckInCats` / narrative on prestige if prestige is ever wired to cozy UI.

**Deferred (MVP verb + session + Cats/Fallout AFK bar already met):**
- [ ] Multi-station / multi-room / ADR atmosphere / cat variety / raids.
- [ ] Play Mode verification.
- [ ] Optional Neko food-gate; harden Neko vs `IdleNarrativeState`.

---

## Final call

**Round-04 cozy polish landed.** Persistence copy honesty and Cats/Fallout station AFK are verified by code + EditMode fixtures; prior verb hardening, HowTo sync, and D18 SurvivePersist hold; Batch BC 58/0 and AllSmoke 91/0 stay green for cozy fixtures.

**Round 05 examine verdict:** cozy/assign **pass for required verb + session + station-AFK bar**; next meaningful gaps are **Neko wall-clock cats (D26)**, deferred **content fantasy**, and **Play Mode** — not persist resurrection, empty stubs, or stale Persistence copy.
