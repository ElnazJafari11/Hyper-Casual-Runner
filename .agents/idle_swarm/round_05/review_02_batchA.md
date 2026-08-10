# Round 5 Review 02 — Batch A CPS / Cookie / AdvCap / Paperclips (post–impl_09 R4)

**Agent:** ROUND 5 EXAMINE 2/10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Scope:** Cookie Clicker, AdVenture Capitalist, Universal Paperclips (+ shared CPS / prestige / manager / load sync / offline catch-up / persist keys)  
**Prior:** `round_04/review_02_batchA.md` → `round_04/impl_09_batchA.md` (`e4acd01`); R3 OL-sync `0936ffa` / CatchUp order `bd06da8`; R2 harden `57d2f5d`; R1 P0 `57910f7`  
**Date:** 2026-08-10  
**Action:** Re-audit only — no implementation, no push.

---

## Verdict

Round-4 **Managers / Phase / MgrHired persist asserts landed**. EditMode Batch A is **23/23** green with A1–A6, AdvCap hire→sim, and the new R4-A3 round-trip fixture. Production bootstrap still runs CatchUp **after** `SyncGeneratorOwnedCountFromState` (D25). R1 P0 math / prestige / manager-gate and R2–R3 contracts remain intact on disk.

Batch A CPS titles remain **MVP-correct EditMode shells**, not Play-verified products.

| Lane | Grade |
|------|--------|
| EditMode P0 + R2 harden + R3 OL-sync / hire income + R4 persist asserts | **PASS** — `IdleBatchASmokeTests` `23/23` (`Logs/IdleBatchA-Summary.txt` / `IdleBatchA-impl09-r4.log`); AllSmoke tip commit `89/89` includes A6 + R4 persist (`Logs/IdleAllSmoke-impl03-r4b.log` / committed `Logs/IdleAllSmoke-Summary.txt`) |
| Play Mode (A7 / R2-A4 / R3-A4 / R4-A4) | **Still open** — checklist `[ ]` for 01/03/04; Play **0/19** |
| Archetype identity | **Unchanged** — Paperclips phase still prestige-reset clone (deferred P2) |
| New residual this round | **None at P0/P1** — only prior P2 proof/identity leftovers |

**Do not re-open M1–M5, R2-A2/A3, OL-sync, or R4-A3 persist as P0/P1** unless new evidence contradicts the paths below. Remaining work: Play-Smoke, optional live-bootstrap fixture, deferred identity depth.

---

## ASSUMPTIONS (this audit)

1. `57910f7`, `57d2f5d`, `bd06da8`, `0936ffa`, `e4acd01` are ancestors of current HEAD — **high** — `git merge-base --is-ancestor` all exit 0.  
2. Latest dedicated Batch A green is `pass=23` including R4 persist fixture — **high** — `Logs/IdleBatchA-impl09-r4.log` + `Logs/IdleBatchA-Summary.txt`.  
3. Committed AllSmoke tip `89/89` still includes Batch A A1/A5/A6/hire/persist — **high** — `IdleAllSmoke-impl03-r4b.log` + `git show HEAD:Logs/IdleAllSmoke-Summary.txt`. Working-tree Summary currently reads `91` (dirty concurrent growth) — cite committed tip or Batch A log for Batch A claims.  
4. Scope stays Cookie / AdvCap / Paperclips; CH/AD only where shared contracts matter — **high** — same as R1–R4.  
5. Play Mode still unverified on Hyper-Casual-Runner — **high** — `docs/idle-play-smoke-checklist.md` + progress Play **0/19**.  
6. A5/A6 still exercise shared sync/catch-up helpers, not a live `IdleSliceBootstrap` MonoBehaviour — **high** — prior receipts + test source.  
7. Progress doc AllSmoke line may lag WT Summary (`89` committed vs `91` dirty) — **med** — cite Summary-at-HEAD / Batch A dedicated logs for pass counts.

---

## Acceptance delta

### R1 A1–A7 (still)

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| A1 | Mult=2, raw CPS=1, 1s → +2 not ~4 | **MET (hardened)** | Buy→sim in `A1_…`; raw PassiveRate; sim `PassiveRate * GlobalMultiplier * dt` |
| A2 | Cookie prestige = single conversion | **MET** | `FirePrestige` prestige-only; `A2_…` |
| A3 | AdvCap post-prestige RequiresManager; buy alone PassiveRate=0 | **MET** | `ResetBuyableGenerator` + `A3_…` / AngelReset |
| A4 | Hire OwnedCount=0 → PassiveRate=0 | **MET** | Hire uses `ComputePassiveRate(…, OwnedCount)`; `A4_…` |
| A5 | Reload cost uses growth^OwnedCount | **MET (hardened)** | Save→load→sync→buy `15*1.15^3`; Passive rebuild 9→3 |
| A6 | Batch A smokes + A1–A4 | **MET** | Suite `23/23` (incl. R3 A6 + R4 persist) |
| A7 | Play Mode 01/03/04 | **UNMET** | Checklist unchecked |

### R2-A1…A5

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R2-A1 | Existing A1–A5 + zero-currency + AngelReset + phase exclusivity still PASS | **MET** | Batch A `pass=23 fail=0` |
| R2-A2 | Buy Mult=2 → 1s → Primary **2±0.05**, PassiveRate **1** | **MET** | Hardened `A1_…` |
| R2-A3 | Save Gens=3 → reload → next buy `15*1.15^3` | **MET** | Hardened `A5_…` |
| R2-A4 | Play Mode 01/03/04 checklist + evidence | **UNMET** | Still deferred |
| R2-A5 | `FirePrestige` has no `FirePhase` | **MET** | UI: creates `PrestigeEventComponent` only |

### R3-A1…A5

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R3-A1 | Existing Batch A suite + AngelReset + phase exclusivity still PASS | **MET** | `23/23` |
| R3-A2 | Stale Mult² Passive → Sync then catch-up 1s → gain **6±0.05**, PassiveRate **3** | **MET** | Prod D25; `A6_LoadCatchUp_UsesRawPassiveNotStaleMultSquared` |
| R3-A3 | Persist Managers/Phase/MgrHired round-trip asserts | **MET** (closed in R4) | `GameProgressData_IdleSlice_RoundTrip_ManagersPhaseMgrHired_AdvCapAndPaperclips` |
| R3-A4 | Play Mode 01/03/04 | **UNMET** | Checklist `[ ]` |
| R3-A5 | `FirePrestige` no `FirePhase`; Sync still used from bootstrap CPS attach | **MET** | UI + `AttachArchetypeExtras` Sync + post-Attach CatchUp |

### R4-A1…A5 (impl_09 targets)

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R4-A1 | Existing Batch A suite (A1–A6 + zero-currency + AngelReset + phase exclusivity + AdvCap hire→sim) still PASS | **MET** | `23/23` (`IdleBatchA-impl09-r4.log`) |
| R4-A2 | Bootstrap Attach Sync **before** `ApplyPersistedElapsed`; `A6_…` gain **6±0.05** / PassiveRate **3** | **MET** | `IdleSliceBootstrap` D25 comment+order; A6 Passed |
| R4-A3 | Round-trip Save/Load asserts ManagersHired, PhaseIndex, MgrHired for AdvCap + Paperclips | **MET** | New fixture Passed (AdvCap mgrs=1 hired=true; Paperclips phase=3 hired=false) |
| R4-A4 | Play Mode checklist 01/03/04 | **UNMET** | Deferred |
| R4-A5 | `FirePrestige` no `FirePhase`; hire keeps `RequiresManager` | **MET** | UI source; hire comment+flag keep |

---

## R4 issue close-out

### Closed by impl_09

| ID | R4 issue | Resolution |
|----|----------|------------|
| T4 / R3-A3 | Persist round-trip asserted Primary/Gens only (short overload) | Closed — long Save/Load asserts Managers/Phase/MgrHired for AdvCap + Paperclips |
| R4-A3 | Acceptance criterion for persist keys | Closed — fixture green in Batch A + AllSmoke |

### Still open

| ID | Issue | Priority | Notes |
|----|--------|----------|-------|
| A7 / R2-A4 / R3-A4 / R4-A4 | No Play Mode evidence for prefabs 01/03/04 | **P0 playability claim** | Blocks “playable”; EditMode ≠ Play |
| T3 | No UI-level assert Prestige skips Phase | P2 | Code-path review still sufficient |
| A5/A6-live | Fixtures do not spawn live `IdleSliceBootstrap` | P2 test | Same Sync→CatchUp contract as bootstrap; MonoBehaviour path still unproven in fixture |
| A6-stamp | A6 uses bare `Apply(…, 1.0)` not `ApplyPersistedElapsed` | P2 test | Wall-clock stamp path covered in Kernel / Melvor BC; Batch A OL math locked without stamp |
| ID-Cookie | Single generator tier | P2 deferred | Matrix ladder still one-row |
| ID-Paper | Phase ≠ loop replacement / no compute alloc | P2 deferred | `IdlePhaseShiftSystem` still reset + PhaseIndex + mult |
| E-stub | UI `TODO: [STUB]` PanelSettings / inline styles | P2 | Prefabs assign PanelSettings; fallback only |

### R1 P0 + R2 harden + R3 OL-sync (remain closed — spot-checked)

| ID | Status |
|----|--------|
| M1 Mult² CPS | Closed — raw PassiveRate + sim Mult |
| M2 Dual Phase+Prestige | Closed — `FirePrestige` prestige-only |
| M3 Forced +1 at 0 currency | Closed — `ConvertRunCurrency` / Prestige gate |
| M4 Hire Max(1, OwnedCount) | Closed — `ComputePassiveRate` |
| M5 Angel drops RequiresManager | Closed — hire keeps flag; reset restores |
| T1 A1 buy→sim | Closed (R2) |
| T2 A5 Save/load sync | Closed (R2) |
| OL-sync CatchUp-before-Sync | Closed (R3) |
| T4 persist Managers/Phase/MgrHired | Closed (R4) |

---

## New finding (Round 5)

**None at P0 or P1.** Spot-check of bootstrap D25 order, Sync helper, offline Apply, buy/hire/prestige/phase, UI FirePrestige, prefab Archetype wiring, R4 persist fixture, and EditMode logs found no new Batch A correctness regressions.

Optional notes (not reopens):

- Concurrent peer work after CatchUp (`PersistNow` when grant>0; Melvor D33 skill-node rewrite) does not weaken Batch A CPS contracts; PersistNow is AFK-friendly for Cookie/Paperclips grants.
- Historical saves that already baked Mult³ Primary into disk remain inflated once; Sync only corrects PassiveRate going forward. Acceptable for MVP.
- Working-tree `IdleAllSmoke-Summary.txt` may show `91` while HEAD commit tip is `89` — do not treat dirty Summary alone as Batch A regression evidence.

---

## Per-title re-audit

### 1) Cookie Clicker (`IdleArchetype.CookieClicker` / `01_…`)

**Loop (MVP):** Click → Buy Generator (raw CPS) → Prestige (single conversion) → Mult on sim / offline.

**Since R4:** Persist fixture is AdvCap/Paperclips-scoped; Cookie CPS path unchanged and still A1/A5/A6 green. Prefab Archetype `0`, PanelSettings assigned.

**Remaining:** Play Mode; single-tier gen.

### 2) AdVenture Capitalist (`…AdventureCapitalist` / `03_…`)

**Loop (MVP):** Collect → Buy (PassiveRate 0 while gated) → Hire (keep RequiresManager) → income tick → Angel Reset restores gate.

**Since R4:** ManagersHired + MgrHired round-trip asserted; hire→PumpSim still green; Sync with `automated = MgrHired` forces PassiveRate=0 when not hired before CatchUp. Prefab Archetype `2`, `StartingCurrency: 25`, `GeneratorRequiresManager: 1`. Collect still `Max(1, OwnedGenerators)` manual labor (intentional).

**Remaining:** Play Mode; optional AdvCap Sync→CatchUp gain=0 when MgrHired=false + stale Passive>0 (P2).

### 3) Universal Paperclips (`…UniversalPaperclips` / `04_…`)

**Loop (MVP):** Manufacture → Buy Autoclipper → Phase Shift (Paperclips/AD-only conversion + PhaseIndex).

**Since R4:** PhaseIndex round-trip asserted (phase=3); phase exclusivity still green; identity depth still deferred.

**Remaining:** Phase ≠ loop replacement; soft floor `currency >= 25 → at least 1` still in `ConvertRunCurrency` (documented); Play Mode.

---

## Shared contract (pinned — do not regress)

```
PassiveRate = BaseCps * OwnedCount   // raw; never * GlobalMultiplier
sim / offline: Primary += PassiveRate * GlobalMultiplier * dt
Prestige button → PrestigeEvent only (no FirePhase)
Phase Shift → IdlePhaseShiftEvent; Paperclips + Antimatter only
Manager archetypes: hire sets IsAutomated, keeps RequiresManager;
  prestige/phase → ResetBuyableGenerator restores RequiresManager + clears automation
Load: Attach Sync rebuilds OwnedCount + raw Passive (or 0 if !automated)
     THEN ApplyPersistedElapsed (D25 CatchUp-after-Sync)
Persist: ManagersHired / PhaseIndex / MgrHired round-trip (R4-A3)
ConvertRunCurrency: floor(sqrt(C/50)); soft +1 if C>=25 and floor<1; else 0
```

Primary owners: `IdlePrestigeMath.cs`, `IdleBuyGeneratorSystem.cs` (incl. hire), `PrestigeSystem.cs`, `IdlePhaseShiftSystem`, `IdleSliceSimulationSystem.cs`, `IdleOfflineCatchUp` / bootstrap load order, `IdleSliceUIController.FirePrestige/FirePhase`, `IdleSliceBootstrap` + `GameProgressData`.

---

## Files re-examined

| Area | Paths |
|------|--------|
| Math / sync | `Assets/Scripts/ECS/Systems/Idle/IdlePrestigeMath.cs` |
| Buy / hire | `IdleBuyGeneratorSystem.cs` (incl. hire) |
| Prestige / phase / sim | `PrestigeSystem.cs`, `IdleSliceActionSystems.cs` (`IdlePhaseShiftSystem`), `IdleSliceSimulationSystem.cs` |
| Click | `IdleClickProduceSystem.cs` (AdvCap Collect) |
| Offline | `IdleOfflineCatchUp.cs`, bootstrap CatchUp-after-Attach order |
| Bootstrap / persist | `IdleSliceBootstrap.cs`, `GameProgressData.cs` |
| UI | `IdleSliceUIController.cs` (`FirePrestige` / `FirePhase`) |
| Prefabs | `01_CookieClicker_Generators_Slice.prefab`, `03_AdventureCapitalist_Managers_Slice.prefab`, `04_UniversalPaperclips_Phase_Slice.prefab` |
| Tests / evidence | `IdleBatchASmokeTests.cs`, `Logs/IdleBatchA-Summary.txt`, `Logs/IdleBatchA-impl09-r4.log`, `Logs/IdleAllSmoke-impl03-r4b.log`, committed `Logs/IdleAllSmoke-Summary.txt`, `docs/idle-toolkit-progress.md`, `docs/idle-play-smoke-checklist.md` |
| Receipts | `round_04/impl_09_batchA.md`, `round_04/review_02_batchA.md`, `round_03/impl_06_batchA.md`, `round_02/impl_06_batchA.md`, `round_01/impl_01_batchA.md` |

---

## Implementer fix list (Round 5 — priority)

### P0 — only if claiming Play-ready

1. **Play-Smoke prefabs 01, 03, 04** on Hyper-Casual-Runner (human or MCP). Check boxes in `docs/idle-play-smoke-checklist.md`; update Play column only with evidence.

### P1 — none for Batch A correctness

No open P1 numerics/contracts after R3 OL-sync + R4 persist close-out.

### P2 — deferred proof / identity / polish (skip unless product asks)

2. Optional live `IdleSliceBootstrap` spawn/load fixture (A5/A6-live) or AdvCap Sync→CatchUp gain=0 when MgrHired=false + stale Passive>0.  
3. Cookie: 2–3 named generator rows.  
4. Paperclips: PhaseIndex≥1 swaps verb set and/or production formula (compute stub OK).  
5. Optional UI-level assert that Prestige button path never creates `IdlePhaseShiftEvent` (T3).

---

## Acceptance criteria for next implementer (machine-checkable)

| # | Criterion |
|---|-----------|
| R5-A1 | Existing Batch A suite (A1–A6 + zero-currency + AngelReset + Paperclips phase exclusivity + AdvCap hire→sim + Managers/Phase/MgrHired round-trip) still **PASS** (`IdleBatchA` / AllSmoke). |
| R5-A2 | Bootstrap source still: Attach Sync **before** `ApplyPersistedElapsed`; `A6_…` still expects gain **6±0.05** / PassiveRate **3**. |
| R5-A3 | Persist fixture still asserts AdvCap ManagersHired/MgrHired and Paperclips PhaseIndex round-trip. |
| R5-A4 | Play Mode: checklist rows for 01, 03, 04 marked done with log/capture path cited in progress doc. |
| R5-A5 | No regression: `FirePrestige` source still has no `FirePhase` call; hire still keeps `RequiresManager`. |

---

## Evidence notes

- Commits: `57910f7` (R1 P0), `57d2f5d` (R2 A1/A5), `bd06da8` (D25 CatchUp-after-Sync), `0936ffa` (R3 Batch A), `e4acd01` (R4 persist asserts).  
- Batch A: `result=Passed pass=23 fail=0` (`Logs/IdleBatchA-Summary.txt`); A1/A5/A6/AdvCap hire/R4 persist/FirePrestige **Passed** in `Logs/IdleBatchA-impl09-r4.log`.  
- AllSmoke (committed tip): `result=Passed pass=89 fail=0` (`git show HEAD:Logs/IdleAllSmoke-Summary.txt`); same fixtures Passed in `Logs/IdleAllSmoke-impl03-r4b.log`.  
- Prefabs: Archetype 0 / 2 / 3; AdvCap start 25 + RequiresManager; PanelSettings assigned.  
- This review did **not** run Unity Play Mode or modify gameplay code.

---

## Out of scope

- Clicker Heroes / Antimatter depth (except shared phase/prestige / Mult contracts already green).  
- Full Cookie / Paperclips commercial parity.  
- Kernel stamp-policy ownership beyond confirming Batch A uses CatchUp-after-Sync.  
- Pushing remotes.
