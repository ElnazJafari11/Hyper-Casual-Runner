# Round 3 Review 02 — Batch A CPS / Cookie / AdvCap / Paperclips (post–impl_06)

**Agent:** ROUND 3 EXAMINE 2/10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Scope:** Cookie Clicker, AdVenture Capitalist, Universal Paperclips (+ shared CPS / prestige / manager / load sync)  
**Prior:** `round_02/review_02_batchA.md` → `round_02/impl_06_batchA.md` (`57d2f5d`); R1 P0 via `57910f7`  
**Date:** 2026-08-10  
**Action:** Re-audit only — no implementation, no push.

---

## Verdict

Round-2 **fixture harden landed**. A1 is now a real buy→sim path; A5 is Save→TryLoad→`SyncGeneratorOwnedCountFromState` with Mult² Passive rebuild. R1 P0 math / prestige / manager-gate contracts remain intact in code and EditMode.

Batch A CPS titles are still **MVP-correct EditMode shells**, not Play-verified products.

| Lane | Grade |
|------|--------|
| EditMode P0 + R2 harden (A1–A5, R2-A1…A3, R2-A5) | **PASS** — `IdleBatchASmokeTests` `20/20` (`Logs/IdleBatchA-Summary.txt` / `IdleBatchA-impl06.log`); AllSmoke `56/56` (`Logs/IdleAllSmoke-Summary.txt`) |
| Play Mode (A7 / R2-A4) | **Still open** — checklist `[ ]` for 01/03/04; progress Play **0/19** |
| Archetype identity | **Unchanged** — Paperclips phase still prestige-reset clone (deferred P2) |
| New residual | **P1** — offline catch-up runs on disk `PassiveRate` **before** load sync rebuilds raw CPS |

**Do not re-open M1–M5 or R2-A2/A3 as P0** unless new evidence contradicts the paths below. Remaining work: Play-Smoke, catch-up-before-sync ordering, optional persist asserts / identity depth.

---

## ASSUMPTIONS (this audit)

1. `impl_06` commit `57d2f5d` + R1 `57910f7` are ancestors of current HEAD — **high** — `git log` shows both.  
2. Latest Batch A green is `pass=20` including hardened A1/A5 — **high** — `Logs/IdleBatchA-impl06.log` + `Logs/IdleBatchA-Summary.txt`.  
3. AllSmoke `56/56` still includes Batch A fixtures — **high** — `docs/idle-toolkit-progress.md` + `Logs/IdleAllSmoke-Summary.txt`.  
4. Scope stays Cookie / AdvCap / Paperclips; CH/AD only where shared contracts matter — **high** — same as R1/R2.  
5. Play Mode still unverified on Hyper-Casual-Runner — **high** — `docs/idle-play-smoke-checklist.md` + progress doc.  
6. A5 still exercises the shared sync helper (bootstrap contract), not a live `IdleSliceBootstrap` MonoBehaviour — **high** — `impl_06` receipt risk note + test source.

---

## Acceptance delta

### R1 A1–A7 (still)

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| A1 | Mult=2, raw CPS=1, 1s → +2 not ~4 | **MET (hardened)** | Buy→sim in `A1_…`; `ComputePassiveRate` raw; sim `PassiveRate * GlobalMultiplier * dt` |
| A2 | Cookie prestige = single conversion | **MET** | `FirePrestige` prestige-only; `A2_…` |
| A3 | AdvCap post-prestige RequiresManager; buy alone PassiveRate=0 | **MET** | `ResetBuyableGenerator` + `A3_…` |
| A4 | Hire OwnedCount=0 → PassiveRate=0 | **MET** | Hire uses `ComputePassiveRate(…, OwnedCount)`; `A4_…` |
| A5 | Reload cost uses growth^OwnedCount | **MET (hardened)** | Save→load→sync→buy `15*1.15^3`; also asserts Passive rebuild 9→3 |
| A6 | Batch A smokes + A1–A4 | **MET** | Suite 20/20 |
| A7 | Play Mode 01/03/04 | **UNMET** | Checklist unchecked |

### R2-A1…A5 (impl_06 targets)

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R2-A1 | Existing A1–A5 + zero-currency + AngelReset + phase exclusivity still PASS | **MET** | Batch A `pass=20 fail=0` |
| R2-A2 | Buy Mult=2 → 1s → Primary **2±0.05**, PassiveRate **1** | **MET** | Hardened `A1_Cookie_SimCps_AppliesGlobalMultiplierOnce` |
| R2-A3 | Save Gens=3 → reload → next buy `15*1.15^3` | **MET** | Hardened `A5_Reload_OwnedCountSynced_NextBuyUsesGrowthCost` |
| R2-A4 | Play Mode 01/03/04 checklist + evidence | **UNMET** | Deferred by design in `impl_06` |
| R2-A5 | `FirePrestige` has no `FirePhase` | **MET** | Source: creates `PrestigeEventComponent` only |

---

## R2 issue close-out

### Closed by impl_06

| ID | R2 issue | Resolution |
|----|----------|------------|
| T1 | A1 injected PassiveRate — not buy→sim | Closed — A1 buys then pumps `IdleSliceSimulationSystem` |
| T2 | A5 did not drive Save/load sync | Closed — `SaveIdleSlice` → clear → `TryLoad` → `SyncGeneratorOwnedCountFromState` |
| P1#4 (optional) | Rebuild PassiveRate on load | Closed in sync helper + bootstrap attach path |

### Still open

| ID | Issue | Priority | Notes |
|----|--------|----------|-------|
| A7 / R2-A4 | No Play Mode evidence for prefabs 01/03/04 | **P0 playability claim** | Blocks “playable”; EditMode ≠ Play |
| T4 | Persist round-trip asserts Primary/Gens only (short overload) | P2 | Managers/Phase/MgrHired keys exist; under-asserted |
| T3 | No UI-level assert Prestige skips Phase | P2 | Code-path review still sufficient |
| A5-live | A5 does not spawn live `IdleSliceBootstrap` | P2 test | Same contract as bootstrap; MonoBehaviour path still unproven in fixture |
| **OL-sync** | **Catch-up uses disk PassiveRate before sync rebuild** | **P1** | New finding this round — see below |
| ID-Cookie | Single generator tier | P2 deferred | Matrix ladder still one-row |
| ID-Paper | Phase ≠ loop replacement / no compute alloc | P2 deferred | `IdlePhaseShiftSystem` still reset + PhaseIndex + mult |
| E-stub | UI `TODO: [STUB]` PanelSettings / inline styles | P2 | Prefabs assign PanelSettings; fallback only |

### R1 P0 (remain closed — spot-checked)

| ID | Status |
|----|--------|
| M1 Mult² CPS | Closed — raw PassiveRate + sim Mult |
| M2 Dual Phase+Prestige | Closed — `FirePrestige` prestige-only |
| M3 Forced +1 at 0 currency | Closed — `ConvertRunCurrency` / Prestige gate |
| M4 Hire Max(1, OwnedCount) | Closed — `ComputePassiveRate` |
| M5 Angel drops RequiresManager | Closed — hire keeps flag; reset restores |

---

## New finding (Round 3)

### OL-sync — Offline catch-up before PassiveRate rebuild (P1)

**Where:** `IdleSliceBootstrap.BuildInitialState` loads `state.PassiveRate = passive` from PlayerPrefs, then calls `ApplyPersistedOfflineCatchUp` **before** `AttachArchetypeExtras` runs `IdlePrestigeMath.SyncGeneratorOwnedCountFromState` (which rebuilds raw `Owned×BaseCps`).

**Impact:** For Cookie / Paperclips (and any automated CPS archetype):

- Post-fix saves with raw PassiveRate → catch-up is correct (`raw * Mult * dt`).
- Stale Mult² Passive on disk → catch-up grants **Mult³-effective** currency once, then sync fixes PassiveRate going forward. Primary already inflated.
- AdvCap edge: disk PassiveRate > 0 while `MgrHired=false` → catch-up can pay before sync forces PassiveRate=0.

**Not a re-open of M1** (live buy/sim still single-apply). It is an incomplete landing of R2 “rebuild on load” relative to Kernel-B offline.

**Cheap fix (for next implementer):** recompute/sync raw PassiveRate (and automation gate) **before** `ApplyPersistedOfflineCatchUp`, or run catch-up after `AttachArchetypeExtras` using the synced state.

---

## Per-title re-audit

### 1) Cookie Clicker (`IdleArchetype.CookieClicker` / `01_…`)

**Loop (MVP):** Click → Buy Generator (raw CPS) → Prestige (single conversion) → Mult on sim.

**Since R2:** A1 buy→sim green; load sync rebuilds PassiveRate; prefab Archetype `0`, PanelSettings assigned.

**Remaining:** Play Mode; single-tier gen; OL-sync on AFK reload.

### 2) AdVenture Capitalist (`…AdventureCapitalist` / `03_…`)

**Loop (MVP):** Collect → Buy (PassiveRate 0 while gated) → Hire (keep RequiresManager) → Angel Reset restores gate.

**Since R2:** No regression in A3/A4/AngelReset; bootstrap restores `IsHired` from MgrHired + sync; prefab Archetype `2`, `StartingCurrency: 25`, `GeneratorRequiresManager: 1`.

**Remaining:** Play Mode; Collect still `Max(1, OwnedGenerators)` manual labor (intentional); OL-sync edge if disk Passive wrong.

### 3) Universal Paperclips (`…UniversalPaperclips` / `04_…`)

**Loop (MVP):** Manufacture → Buy Autoclipper → Phase Shift (Paperclips/AD-only conversion + PhaseIndex).

**Since R2:** Phase exclusivity still green; identity depth still deferred (not cheap).

**Remaining:** Phase ≠ loop replacement; soft floor `currency >= 25 → at least 1` still in `ConvertRunCurrency` (documented); Play Mode; OL-sync.

---

## Shared contract (pinned — do not regress)

```
PassiveRate = BaseCps * OwnedCount   // raw; never * GlobalMultiplier
sim / offline: Primary += PassiveRate * GlobalMultiplier * dt
Prestige button → PrestigeEvent only (no FirePhase)
Phase Shift → IdlePhaseShiftEvent; Paperclips + Antimatter only
Manager archetypes: hire sets IsAutomated, keeps RequiresManager;
  prestige/phase → ResetBuyableGenerator restores RequiresManager + clears automation
Load: SyncGeneratorOwnedCountFromState rebuilds OwnedCount + raw Passive when automated
ConvertRunCurrency: floor(sqrt(C/50)); soft +1 if C>=25 and floor<1; else 0
```

Primary owners: `IdlePrestigeMath.cs`, `IdleBuyGeneratorSystem.cs`, `IdleManagerHireSystem`, `PrestigeSystem.cs`, `IdlePhaseShiftSystem`, `IdleSliceSimulationSystem.cs`, `IdleOfflineCatchUp` / bootstrap load order, `IdleSliceUIController.FirePrestige/FirePhase`, `IdleSliceBootstrap` + `GameProgressData`.

---

## Files re-examined

| Area | Paths |
|------|--------|
| Math / sync | `Assets/Scripts/ECS/Systems/Idle/IdlePrestigeMath.cs` |
| Buy / hire | `IdleBuyGeneratorSystem.cs` (incl. hire) |
| Prestige / phase / sim | `PrestigeSystem.cs`, `IdleSliceActionSystems.cs` (`IdlePhaseShiftSystem`), `IdleSliceSimulationSystem.cs` |
| Offline | `IdleOfflineCatchUp.cs`, bootstrap `BuildInitialState` order |
| Bootstrap / persist | `IdleSliceBootstrap.cs`, `GameProgressData.cs` |
| UI | `IdleSliceUIController.cs` (`FirePrestige` / `FirePhase`) |
| Prefabs | `01_CookieClicker_Generators_Slice.prefab`, `03_AdventureCapitalist_Managers_Slice.prefab`, `04_UniversalPaperclips_Phase_Slice.prefab` |
| Tests / evidence | `IdleBatchASmokeTests.cs`, `Logs/IdleBatchA-Summary.txt`, `Logs/IdleBatchA-impl06.log`, `Logs/IdleAllSmoke-Summary.txt`, `docs/idle-toolkit-progress.md`, `docs/idle-play-smoke-checklist.md` |
| Receipts | `round_02/impl_06_batchA.md`, `round_02/review_02_batchA.md`, `round_01/impl_01_batchA.md` |

---

## Implementer fix list (Round 3 — priority)

### P0 — only if claiming Play-ready

1. **Play-Smoke prefabs 01, 03, 04** on Hyper-Casual-Runner (human or MCP). Check boxes in `docs/idle-play-smoke-checklist.md`; update Play column only with evidence.

### P1 — correctness / proof harden (no redesign)

2. **OL-sync:** Rebuild raw PassiveRate (and manager automation gate) **before** `ApplyPersistedOfflineCatchUp`, or move catch-up after attach sync. Add EditMode: save Mult² Passive + Gens + Mult → bootstrap-style load with elapsed → currency uses **raw×Mult×dt**, not Mult²×Mult.  
3. Optionally extend persist round-trip to assert Managers / Phase / MgrHired keys (T4).

### P2 — deferred identity / polish (skip unless product asks)

4. Cookie: 2–3 named generator rows.  
5. Paperclips: PhaseIndex≥1 swaps verb set and/or production formula (compute stub OK).  
6. A5-live: optional fixture that drives real `IdleSliceBootstrap` spawn/load (only if OL-sync fix needs MonoBehaviour proof).

---

## Acceptance criteria for next implementer (machine-checkable)

| # | Criterion |
|---|-----------|
| R3-A1 | Existing Batch A A1–A5 + zero-currency + AngelReset + Paperclips phase exclusivity still **PASS** (`IdleBatchA` / AllSmoke). |
| R3-A2 | (If P1#2 done) Save PassiveRate=`Owned*BaseCps*Mult` (stale), Mult=2, Owned=3, BaseCps=1 → load catch-up 1.0s → Primary gain **6 ± 0.05** (raw 3 × Mult 2), not **12**. Post-load PassiveRate **3**. |
| R3-A3 | (If P1#3 done) Round-trip Save/Load asserts ManagersHired, PhaseIndex, MgrHired for at least one AdvCap + one Paperclips write. |
| R3-A4 | Play Mode: checklist rows for 01, 03, 04 marked done with log/capture path cited in progress doc. |
| R3-A5 | No regression: `FirePrestige` source still has no `FirePhase` call; `SyncGeneratorOwnedCountFromState` still used from bootstrap CPS attach. |

---

## Evidence notes

- Commits: `57910f7` (R1 P0), `57d2f5d` (R2 A1/A5 harden).  
- Batch A: `result=Passed pass=20 fail=0` (`Logs/IdleBatchA-Summary.txt`); A1/A5 **Passed** in `Logs/IdleBatchA-impl06.log`.  
- AllSmoke: `result=Passed pass=56 fail=0` (`Logs/IdleAllSmoke-Summary.txt`).  
- Prefabs: Archetype 0 / 2 / 3; AdvCap start 25 + RequiresManager; PanelSettings assigned.  
- This review did **not** run Unity Play Mode or modify gameplay code.

---

## Out of scope

- Clicker Heroes / Antimatter depth (except shared phase/prestige / Mult contracts already green).  
- Full Cookie / Paperclips commercial parity.  
- Kernel stamp-policy ownership beyond Batch A catch-up ordering.  
- Pushing remotes.
