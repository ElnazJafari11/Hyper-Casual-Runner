# Round 6 Review 02 — Batch A CPS / Cookie / AdvCap / Paperclips (post–impl_08 R5)

**Agent:** ROUND 6 EXAMINE 2/10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Scope:** Cookie Clicker, AdVenture Capitalist, Universal Paperclips (+ shared CPS / prestige / manager / load sync / offline catch-up / persist keys)  
**Prior:** `round_05/review_02_batchA.md` → `round_05/impl_08_batchA.md` (`6461b37`); R4 persist `e4acd01`; R3 OL-sync `0936ffa` / CatchUp order `bd06da8`; R2 harden `57d2f5d`; R1 P0 `57910f7`  
**Date:** 2026-08-10  
**Action:** Re-audit only — no implementation, no push.

---

## Verdict

Round-5 **verified EditMode green and documented Play BLOCKED** (no gameplay code). R1–R4 contracts remain intact on disk: Mult once, prestige-only, hire keeps RequiresManager, CatchUp-after-Sync (D25), Managers/Phase/MgrHired round-trip. Dedicated Batch A is still **23/23**; tip AllSmoke **103/103** still includes A1/A6/hire/persist fixtures Passed.

Batch A CPS titles remain **MVP-correct EditMode shells**, not Play-verified products. Play 01/03/04 stays **BLOCKED** — MCP instance is still `thepcgtoolkit@96e3a310` only (HCR editor absent).

| Lane | Grade |
|------|--------|
| EditMode P0 + R2–R5 contracts | **PASS** — `IdleBatchASmokeTests` `23/23` (`Logs/IdleBatchA-Summary.txt` / `IdleBatchA-impl08-r5.log`); AllSmoke tip `103/103` includes Batch A (`Logs/IdleAllSmoke-impl04-r5b.log` / `Logs/IdleAllSmoke-Summary.txt`) |
| Play Mode (A7 / R2-A4 / R3-A4 / R4-A4 / R5-A4) | **Still BLOCKED** — checklist `[ ] **BLOCKED**` for 01/03/04; Play **0/19** |
| Archetype identity | **Unchanged** — Paperclips phase still prestige-reset clone (deferred P2) |
| New residual this round | **None at P0/P1** — only prior P2 proof/identity leftovers |

**Do not re-open M1–M5, R2-A2/A3, OL-sync, R4-A3 persist, or R5-A1/A2/A3/A5 as P0/P1** unless new evidence contradicts the paths below. Remaining work: Play-Smoke (when HCR Unity is on the bridge), optional live-bootstrap fixture, deferred identity depth.

---

## ASSUMPTIONS (this audit)

1. `57910f7`, `57d2f5d`, `bd06da8`, `0936ffa`, `e4acd01`, `6461b37` are ancestors of current HEAD — **high** — `git merge-base --is-ancestor` all exit 0; HEAD `b175218`.  
2. Latest dedicated Batch A green remains `pass=23` from R5 verify — **high** — `Logs/IdleBatchA-impl08-r5.log` + `Logs/IdleBatchA-Summary.txt` (`result=Passed pass=23 fail=0`).  
3. Tip AllSmoke `103/103` still includes Batch A A1/A6/hire/persist — **high** — `IdleAllSmoke-impl04-r5b.log` + `Logs/IdleAllSmoke-Summary.txt` (committed + WT both `103`).  
4. Scope stays Cookie / AdvCap / Paperclips; CH/AD only where shared contracts matter — **high** — same as R1–R5.  
5. Play Mode still unverified / blocked on Hyper-Casual-Runner — **high** — checklist BLOCKED note + MCP instances = `thepcgtoolkit` only.  
6. A5/A6 still exercise shared sync/catch-up helpers, not a live `IdleSliceBootstrap` MonoBehaviour — **high** — test source unchanged.  
7. Post-`6461b37` peer edits to bootstrap / sim / UI are Melvor SkillXp, combat HUD honesty, LoM Farm hide — **high** — `git diff 6461b37..HEAD` on those paths; Batch A CPS buy/hire/prestige/CatchUp order untouched. Cite Batch A dedicated log for Batch A claims, not AllSmoke count alone.

---

## Acceptance delta

### R1 A1–A7 (still)

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| A1 | Mult=2, raw CPS=1, 1s → +2 not ~4 | **MET (hardened)** | Buy→sim in `A1_…`; raw PassiveRate; sim `PassiveRate * GlobalMultiplier * dt` |
| A2 | Cookie prestige = single conversion | **MET** | `FirePrestige` prestige-only; `A2_…` |
| A3 | AdvCap post-prestige RequiresManager; buy alone PassiveRate=0 | **MET** | `ResetBuyableGenerator` + `A3_…` / AngelReset |
| A4 | Hire OwnedCount=0 → PassiveRate=0 | **MET** | Hire uses `ComputePassiveRate(…, OwnedCount)`; `A4_…` |
| A5 | Reload cost uses growth^OwnedCount | **MET (hardened)** | Save→load→sync→buy `15*1.15^3`; Passive rebuild |
| A6 | Batch A smokes + A1–A4 | **MET** | Suite `23/23` (incl. R3 A6 + R4 persist) |
| A7 | Play Mode 01/03/04 | **UNMET / BLOCKED** | Checklist `[ ] **BLOCKED**` |

### R2-A1…A5

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R2-A1 | Existing A1–A5 + zero-currency + AngelReset + phase exclusivity still PASS | **MET** | Batch A `pass=23 fail=0` |
| R2-A2 | Buy Mult=2 → 1s → Primary **2±0.05**, PassiveRate **1** | **MET** | Hardened `A1_…` |
| R2-A3 | Save Gens=3 → reload → next buy `15*1.15^3` | **MET** | Hardened `A5_…` |
| R2-A4 | Play Mode 01/03/04 checklist + evidence | **UNMET / BLOCKED** | R5 documented BLOCKED |
| R2-A5 | `FirePrestige` has no `FirePhase` | **MET** | UI: creates `PrestigeEventComponent` only |

### R3-A1…A5

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R3-A1 | Existing Batch A suite + AngelReset + phase exclusivity still PASS | **MET** | `23/23` |
| R3-A2 | Stale Mult² Passive → Sync then catch-up 1s → gain **6±0.05**, PassiveRate **3** | **MET** | Prod D25; `A6_…` |
| R3-A3 | Persist Managers/Phase/MgrHired round-trip asserts | **MET** | R4 fixture still present + green |
| R3-A4 | Play Mode 01/03/04 | **UNMET / BLOCKED** | Checklist |
| R3-A5 | `FirePrestige` no `FirePhase`; Sync still used from bootstrap CPS attach | **MET** | UI + `AttachArchetypeExtras` Sync + post-Attach CatchUp |

### R4-A1…A5

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R4-A1 | Suite incl. hire→sim + phase exclusivity still PASS | **MET** | `23/23` |
| R4-A2 | Bootstrap Sync **before** `ApplyPersistedElapsed`; A6 gain **6±0.05** / Passive **3** | **MET** | D25 comment+order intact; A6 Passed |
| R4-A3 | Round-trip ManagersHired / PhaseIndex / MgrHired | **MET** | Fixture Passed in Batch A + AllSmoke |
| R4-A4 | Play Mode 01/03/04 | **UNMET / BLOCKED** | |
| R4-A5 | `FirePrestige` no `FirePhase`; hire keeps `RequiresManager` | **MET** | UI + hire comment+flag |

### R5-A1…A5 (impl_08 targets)

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R5-A1 | Existing Batch A suite still PASS | **MET** | `pass=23 fail=0` (`IdleBatchA-impl08-r5.log`) |
| R5-A2 | Bootstrap Attach Sync before CatchUp; A6 gain **6±0.05** / Passive **3** | **MET** | Source D25; `A6_… => Passed` |
| R5-A3 | Persist fixture AdvCap mgrs/hired + Paperclips phase | **MET** | Round-trip fixture Passed |
| R5-A4 | Play Mode checklist 01/03/04 done with evidence | **BLOCKED** | Documented in checklist + progress; MCP ≠ HCR |
| R5-A5 | `FirePrestige` no `FirePhase`; hire keeps `RequiresManager` | **MET** | Spot-check this audit |

---

## R5 issue close-out

### Closed / verified by impl_08

| ID | R5 issue | Resolution |
|----|----------|------------|
| R5-A1 / A2 / A3 / A5 | Reconfirm EditMode contracts after R4 | Closed — batchmode `23/23`; no gameplay edits |
| R5-A4 documentation | Play claim honesty | Closed as **BLOCKED** (not falsely checked) — checklist + progress cite MCP mismatch |

### Still open

| ID | Issue | Priority | Notes |
|----|--------|----------|-------|
| A7 / R2-A4 / R3-A4 / R4-A4 / R5-A4 | No Play Mode evidence for prefabs 01/03/04 | **P0 playability claim** | Still BLOCKED: bridge = `thepcgtoolkit` only; HCR editor absent |
| T3 | No UI-level assert Prestige skips Phase | P2 | Code-path review + `FirePrestige_…` fixture still sufficient |
| A5/A6-live | Fixtures do not spawn live `IdleSliceBootstrap` | P2 test | Same Sync→CatchUp contract; MonoBehaviour path unproven in fixture |
| A6-stamp | A6 uses bare `Apply(…, 1.0)` not `ApplyPersistedElapsed` | P2 test | Wall-clock stamp covered elsewhere; Batch A OL math locked without stamp |
| ID-Cookie | Single generator tier | P2 deferred | Matrix ladder still one-row |
| ID-Paper | Phase ≠ loop replacement / no compute alloc | P2 deferred | `IdlePhaseShiftSystem` still reset + PhaseIndex + mult |
| E-stub | UI `TODO: [STUB]` PanelSettings / inline styles | P2 | Prefabs assign PanelSettings; fallback only |

### R1 P0 + R2 harden + R3 OL-sync + R4 persist (remain closed — spot-checked)

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

## New finding (Round 6)

**None at P0 or P1.** Spot-check of bootstrap D25 order, Sync helper, offline Apply, buy/hire/prestige/phase, UI FirePrestige, prefab Archetype wiring, R4 persist fixture, R5 EditMode log, and post-R5 peer diffs found no new Batch A correctness regressions.

Optional notes (not reopens):

- Concurrent peer work after `6461b37` (Melvor `SkillXp` load/persist + sim lockstep; combat HUD Zone/HP/Tap; LoM Farm button hide) touches shared bootstrap/sim/UI files but does **not** alter Cookie/AdvCap/Paperclips CPS math, CatchUp-after-Sync order, hire `RequiresManager`, or `FirePrestige` exclusivity.
- Tip AllSmoke grew to **103/103** (R5 review cited committed tip 89 / dirty 91). Growth is concurrent suite expansion — Batch A still 23 tests; cite `IdleBatchA-impl08-r5.log` for Batch A claims.
- Historical Mult³ Primary-on-disk still accepted MVP (Sync corrects PassiveRate going forward).
- MCP re-probed this audit: instance_count=1 → `thepcgtoolkit@96e3a310` only; Play 01/03/04 remains **BLOCKED**.

---

## Per-title re-audit

### 1) Cookie Clicker (`IdleArchetype.CookieClicker` / `01_…`)

**Loop (MVP):** Click → Buy Generator (raw CPS) → Prestige (single conversion) → Mult on sim / offline.

**Since R5:** No Cookie gameplay change; A1/A5/A6 still green in dedicated + AllSmoke logs. Prefab Archetype `0`, PanelSettings assigned.

**Remaining:** Play Mode (BLOCKED until HCR Unity on bridge); single-tier gen.

### 2) AdVenture Capitalist (`…AdventureCapitalist` / `03_…`)

**Loop (MVP):** Collect → Buy (PassiveRate 0 while gated) → Hire (keep RequiresManager) → income tick → Angel Reset restores gate.

**Since R5:** ManagersHired + MgrHired round-trip still asserted; hire→PumpSim still green; Sync with `automated = MgrHired` forces PassiveRate=0 when not hired before CatchUp. Prefab Archetype `2`, `StartingCurrency: 25`, `GeneratorRequiresManager: 1`. Collect still `Max(1, OwnedGenerators)` manual labor (intentional).

**Remaining:** Play Mode (BLOCKED); optional AdvCap Sync→CatchUp gain=0 when MgrHired=false + stale Passive>0 (P2).

### 3) Universal Paperclips (`…UniversalPaperclips` / `04_…`)

**Loop (MVP):** Manufacture → Buy Autoclipper → Phase Shift (Paperclips/AD-only conversion + PhaseIndex).

**Since R5:** PhaseIndex round-trip still asserted; phase exclusivity still green; identity depth still deferred (`IdlePhaseShiftSystem` = convert + PhaseIndex++ + reset gens).

**Remaining:** Phase ≠ loop replacement; soft floor `currency >= 25 → at least 1` still in `ConvertRunCurrency` (documented); Play Mode (BLOCKED).

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
| Offline | `IdleOfflineCatchUp.cs`, bootstrap CatchUp-after-Attach order |
| Bootstrap / persist | `IdleSliceBootstrap.cs`, `GameProgressData.cs` |
| UI | `IdleSliceUIController.cs` (`FirePrestige` / `FirePhase`) |
| Prefabs | `01_CookieClicker_Generators_Slice.prefab`, `03_AdventureCapitalist_Managers_Slice.prefab`, `04_UniversalPaperclips_Phase_Slice.prefab` |
| Tests / evidence | `IdleBatchASmokeTests.cs` (23 `[Test]`), `Logs/IdleBatchA-Summary.txt`, `Logs/IdleBatchA-impl08-r5.log`, `Logs/IdleAllSmoke-impl04-r5b.log`, `Logs/IdleAllSmoke-Summary.txt`, `docs/idle-toolkit-progress.md`, `docs/idle-play-smoke-checklist.md` |
| Receipts | `round_05/impl_08_batchA.md`, `round_05/review_02_batchA.md`, `round_04/impl_09_batchA.md`, `round_04/review_02_batchA.md` |

---

## Implementer fix list (Round 6 — priority)

### P0 — only if claiming Play-ready

1. **Play-Smoke prefabs 01, 03, 04** on Hyper-Casual-Runner (human or MCP). Unblock requires Unity opened on this repo and registered on the MCP bridge. Check boxes in `docs/idle-play-smoke-checklist.md`; update Play column only with evidence. Until then keep `[ ] **BLOCKED**`.

### P1 — none for Batch A correctness

No open P1 numerics/contracts after R3 OL-sync + R4 persist + R5 verify-green.

### P2 — deferred proof / identity / polish (skip unless product asks)

2. Optional live `IdleSliceBootstrap` spawn/load fixture (A5/A6-live) or AdvCap Sync→CatchUp gain=0 when MgrHired=false + stale Passive>0.  
3. Cookie: 2–3 named generator rows.  
4. Paperclips: PhaseIndex≥1 swaps verb set and/or production formula (compute stub OK).  
5. Optional UI-level assert that Prestige button path never creates `IdlePhaseShiftEvent` (T3) — already covered by EditMode `FirePrestige_…` for Paperclips exclusivity.

---

## Acceptance criteria for next implementer (machine-checkable)

| # | Criterion |
|---|-----------|
| R6-A1 | Existing Batch A suite (A1–A6 + zero-currency + AngelReset + Paperclips phase exclusivity + AdvCap hire→sim + Managers/Phase/MgrHired round-trip) still **PASS** (`IdleBatchA` / AllSmoke). |
| R6-A2 | Bootstrap source still: Attach Sync **before** `ApplyPersistedElapsed`; `A6_…` still expects gain **6±0.05** / PassiveRate **3**. |
| R6-A3 | Persist fixture still asserts AdvCap ManagersHired/MgrHired and Paperclips PhaseIndex round-trip. |
| R6-A4 | Play Mode: checklist rows for 01, 03, 04 marked done with log/capture path cited in progress doc — **or** remain explicitly **BLOCKED** with MCP/HCR evidence (do not silently check). |
| R6-A5 | No regression: `FirePrestige` source still has no `FirePhase` call; hire still keeps `RequiresManager`. |

---

## Evidence notes

- Commits: `57910f7` (R1 P0), `57d2f5d` (R2 A1/A5), `bd06da8` (D25 CatchUp-after-Sync), `0936ffa` (R3 Batch A), `e4acd01` (R4 persist), `6461b37` (R5 verify + Play BLOCKED docs).  
- Batch A: `result=Passed pass=23 fail=0` (`Logs/IdleBatchA-Summary.txt`); A1/A5/A6/AdvCap hire/R4 persist/FirePrestige **Passed** in `Logs/IdleBatchA-impl08-r5.log`.  
- AllSmoke tip: `result=Passed pass=103 fail=0` (`Logs/IdleAllSmoke-Summary.txt`); same Batch A fixtures Passed in `Logs/IdleAllSmoke-impl04-r5b.log`.  
- Prefabs: Archetype 0 / 2 / 3; AdvCap start 25 + RequiresManager; PanelSettings assigned.  
- Play probe: MCP `instance_count=1` → `thepcgtoolkit@96e3a310` (`D:/Git/pcg-toolkit/thepcgtoolkit`); Hyper-Casual-Runner editor **absent**.  
- This review did **not** run Unity Play Mode, did **not** modify gameplay code, and did **not** push.

---

## Out of scope

- Clicker Heroes / Antimatter depth (except shared phase/prestige / Mult contracts already green).  
- Full Cookie / Paperclips commercial parity.  
- Kernel stamp-policy ownership beyond confirming Batch A uses CatchUp-after-Sync.  
- Pushing remotes.
