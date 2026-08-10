# Round 2 Review 02 — Batch A CPS / Cookie / AdvCap / Paperclips (post–impl_01)

**Agent:** ROUND 2 EXAMINE 2/10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Scope:** Cookie Clicker, AdVenture Capitalist, Universal Paperclips (+ shared CPS/prestige/manager systems)  
**Prior:** `round_01/review_02_batchA_cps.md` → `round_01/impl_01_batchA.md` (`57910f7`)  
**Date:** 2026-08-10  
**Action:** Re-audit only — no implementation, no push.

---

## Verdict

Round-1 **P0 numerics / prestige / manager-gate bugs are fixed in code** and covered by EditMode fixtures. Treat Batch A CPS titles as **MVP-correct shells**, not play-verified products.

| Lane | Grade |
|------|--------|
| EditMode P0 (A1–A6) | **PASS** — systems + `IdleBatchASmokeTests` A1–A5 + related; AllSmoke `46/46` (`Logs/IdleAllSmoke-Summary.txt`) |
| Play Mode (A7) | **Still open** — `docs/idle-toolkit-progress.md` Play Mode **0/19**; checklist unchecked for 01/03/04 |
| Archetype identity | **Partial** — Cookie/AdvCap loops work at MVP depth; Paperclips phase remains a prestige-reset clone (deferred R1 P2) |

**Do not re-open M1–M5 as P0** unless new evidence contradicts the paths below. Remaining work is Play-Smoke, test hardening, and optional identity depth.

---

## ASSUMPTIONS (this audit)

1. `impl_01` commit `57910f7` + peer prestige/production fixes are on the working tree — **high** — `git log` + file read-back.  
2. Latest green AllSmoke (`pass=46 fail=0`) includes Batch A A1–A5 — **high** — `docs/idle-toolkit-progress.md` + `Logs/IdleAllSmoke-Summary.txt` (impl_09 era); earlier `IdleBatchA-impl01b.log` failures are stale.  
3. Scope stays Cookie / AdvCap / Paperclips; CH/Antimatter only where shared systems matter — **high** — same as R1.  
4. Play Mode still unverified on this project — **high** — progress doc + play-smoke checklist.

---

## Acceptance delta (R1 A1–A7)

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| A1 | Mult=2, raw CPS=1, 1s → +2 not ~4 | **MET** | `IdlePrestigeMath.ComputePassiveRate` (no mult bake); sim `PassiveRate * GlobalMultiplier * dt`; test `A1_Cookie_SimCps_AppliesGlobalMultiplierOnce` |
| A2 | Cookie prestige = single conversion | **MET** | `FirePrestige` no longer calls `FirePhase`; `PrestigeSystem` uses `ConvertRunCurrency` only; `A2_…` |
| A3 | AdvCap post-prestige RequiresManager; buy alone PassiveRate=0 | **MET** | Hire keeps `RequiresManager`; `ResetBuyableGenerator` restores gate; `A3_…` |
| A4 | Hire OwnedCount=0 → PassiveRate=0 | **MET** | Hire uses `ComputePassiveRate(…, OwnedCount)` (no `Max(1,…)`); `A4_…` |
| A5 | Reload cost uses growth^OwnedCount | **MET in bootstrap path**; **weak in test** | Bootstrap sets `OwnedCount = _loadedGens`; test manually syncs (does not drive `IdleSliceBootstrap` / PlayerPrefs load) |
| A6 | Batch A smokes + A1–A4 | **MET** | Suite expanded; AllSmoke 46/46 |
| A7 | Play Mode 01/03/04 | **UNMET** | Checklist all `[ ]`; MCP not on HCR |

---

## R1 issue close-out

### Critical / P0 (closed)

| ID | R1 issue | Resolution |
|----|----------|------------|
| M1 | `GlobalMultiplier` double-applied to CPS | Closed — raw `PassiveRate`; sim applies mult once |
| M2 | Dual Phase+Prestige on Cookie/AdvCap Prestige | Closed — `IdleSliceUIController.FirePrestige` prestige-only |
| M3 | Prestige forced `converted = 1` at 0 currency | Closed — `ConvertRunCurrency` returns 0 below threshold; `PrestigeSystem_ZeroCurrency_DoesNotAward` |
| M4 | Hire `Max(1, OwnedCount)` phantom CPS | Closed |
| M5 | Angel reset dropped `RequiresManager` | Closed — hire keeps flag; reset restores via `IdlePrestigeMath.ResetBuyableGenerator` |
| P1 (partial) | OwnedCount / manager / phase not persisted | Closed for MVP — `GameProgressData` saves Gens/Managers/Phase/MgrHired; bootstrap restores `OwnedCount`, `IsHired`, `IsAutomated`, `PhaseIndex` |

### Still open (not P0 for CPS correctness)

| ID | Issue | Priority | Notes |
|----|--------|----------|-------|
| A7 | No Play Mode evidence for prefabs 01/03/04 | **P0 playability claim** | Blocks “playable” marketing; EditMode ≠ Play |
| T1 | A1 injects `PassiveRate` — does not buy→sim | P1 test | Purchase path reviewed correct; fixture does not prove end-to-end |
| T2 | A5 does not load via bootstrap/PlayerPrefs | P1 test | Comment cites sync contract; integration gap |
| T3 | No UI-level assert that Prestige button skips Phase | P2 test | Code-path review sufficient for now |
| T4 | Persist round-trip asserts Primary/Gens only (short overload) | P2 | Managers/Phase/MgrHired keys exist but under-asserted |
| P-load | Loaded `PassiveRate` trusted as-is (not rebuilt from Owned×BaseCps) | P2 | Fine if saves written post-fix; old Mult² saves could still be stale |
| ID-Cookie | Single generator tier | P2 deferred | Matrix Cursor/Grandma ladder still one-row shell |
| ID-Paper | Phase does not replace loop / no compute alloc | P2 deferred | `IdlePhaseShiftSystem` still reset + PhaseIndex + mult; verbs unchanged |
| E-stub | UI `TODO: [STUB]` PanelSettings / inline styles | P2 | Prefabs assign PanelSettings; fallback only |
| E-cleanup | Bootstrap `OnDestroy` now destroys `_sliceEntity` | Closed vs R1 | R1 stub cleanup addressed |

---

## Per-title re-audit

### 1) Cookie Clicker (`IdleArchetype.CookieClicker` / `01_…`)

**Loop now (MVP):** Click → Buy Generator (raw CPS) → Prestige (single conversion) → mult on sim.

**Fixed since R1:** M1, M2, M3; OwnedCount load sync.

**Remaining:** Single-tier gen; Play Mode; A1 buy→sim hardening.

### 2) AdVenture Capitalist (`…AdventureCapitalist` / `03_…`)

**Loop now (MVP):** Collect → Buy (PassiveRate 0 while gated) → Hire (automate, keep RequiresManager) → Angel Reset restores gate.

**Fixed since R1:** M4, M5, dual prestige; ManagerHired persist.

**Remaining:** Play Mode; economy still click-grind heavy (acceptable MVP); AdvCap Collect still uses `Max(1, OwnedGenerators)` for manual labor (intentional, not a CPS bake bug).

### 3) Universal Paperclips (`…UniversalPaperclips` / `04_…`)

**Loop now (MVP):** Manufacture → Buy Autoclipper → Phase Shift (Paperclips/AD-only conversion + PhaseIndex).

**Fixed since R1:** Shared mult² CPS bug; phase gated to Paperclips/AD; ECB destroy on phase events.

**Remaining:** Phase ≠ loop replacement (matrix fidelity gap); no compute allocation; soft floor `currency >= 25 → at least 1` still in `ConvertRunCurrency` (documented, smoke-friendly).

---

## Shared contract (pinned — do not regress)

```
PassiveRate = BaseCps * OwnedCount   // raw; never * GlobalMultiplier
sim: Primary += PassiveRate * GlobalMultiplier * dt
Prestige button → PrestigeEvent only
Phase Shift → IdlePhaseShiftEvent; Paperclips + Antimatter only
Manager archetypes: hire sets IsAutomated, keeps RequiresManager;
  prestige/phase → ResetBuyableGenerator restores RequiresManager + clears automation
ConvertRunCurrency: floor(sqrt(C/50)); soft +1 if C>=25 and floor<1; else 0
```

Primary owners: `IdlePrestigeMath.cs`, `IdleBuyGeneratorSystem.cs`, `IdleManagerHireSystem`, `PrestigeSystem.cs`, `IdlePhaseShiftSystem`, `IdleSliceSimulationSystem.cs`, `IdleSliceUIController.FirePrestige/FirePhase`, `IdleSliceBootstrap.AttachArchetypeExtras` + `PersistNow` / `GameProgressData`.

---

## Files re-examined

| Area | Paths |
|------|--------|
| Math / buy / hire | `Assets/Scripts/ECS/Systems/Idle/IdlePrestigeMath.cs`, `IdleBuyGeneratorSystem.cs` |
| Prestige / phase / sim | `PrestigeSystem.cs`, `IdleSliceActionSystems.cs` (`IdlePhaseShiftSystem`), `IdleSliceSimulationSystem.cs` |
| Click | `IdleClickProduceSystem.cs` |
| Bootstrap / persist | `IdleSliceBootstrap.cs`, `GameProgressData.cs` |
| UI | `IdleSliceUIController.cs` |
| Prefabs | `01_CookieClicker_Generators_Slice.prefab`, `03_AdventureCapitalist_Managers_Slice.prefab`, `04_UniversalPaperclips_Phase_Slice.prefab` |
| Tests / evidence | `IdleBatchASmokeTests.cs`, `Logs/IdleAllSmoke-Summary.txt`, `docs/idle-toolkit-progress.md`, `docs/idle-play-smoke-checklist.md` |
| Receipts | `round_01/impl_01_batchA.md`, `round_01/review_02_batchA_cps.md` |

---

## Implementer fix list (Round 2 — priority)

### P0 — only if claiming Play-ready

1. **Play-Smoke prefabs 01, 03, 04** on Hyper-Casual-Runner (human or MCP). Check boxes in `docs/idle-play-smoke-checklist.md`; update Play column only with evidence.

### P1 — harden proofs (no gameplay redesign)

2. Extend **A1** to buy one gen with Mult=2 then pump sim (assert PassiveRate raw + currency ≈ 2).  
3. Extend **A5** to `SaveIdleSlice` → clear world → bootstrap-style attach with loaded gens (or call real load helper) → assert next cost.  
4. Optionally rebuild `PassiveRate` on load for CPS archetypes from `OwnedCount * BaseCps` when automated (guards stale Mult² saves).

### P2 — deferred identity (skip unless product asks)

5. Cookie: 2–3 named generator rows.  
6. Paperclips: PhaseIndex≥1 swaps verb set and/or production formula (compute stub OK).  
7. Assert Managers/Phase/MgrHired in persist round-trip test.

---

## Acceptance criteria for next implementer (machine-checkable)

| # | Criterion |
|---|-----------|
| R2-A1 | Existing A1–A5 + zero-currency + AngelReset + Paperclips phase exclusivity still **PASS** in Batch A / AllSmoke. |
| R2-A2 | (If P1#2 done) Buy path: Mult=2, BaseCps=1, Owned=1 after buy → 1.0s sim → Primary **2 ± 0.05**, PassiveRate **1**. |
| R2-A3 | (If P1#3 done) PlayerPrefs save with Gens=3 → reload path → next buy costs `15 * 1.15^3`. |
| R2-A4 | Play Mode: checklist rows for 01, 03, 04 marked done with log/capture path cited in progress doc. |
| R2-A5 | No regression: `FirePrestige` source still has no `FirePhase` call (grep / review). |

---

## Evidence notes

- Commit: `57910f7 Fix Batch A idle P0 CPS double-mult, prestige double-fire, and manager gate.`  
- AllSmoke: `result=Passed pass=46 fail=0` (`Logs/IdleAllSmoke-Summary.txt`).  
- Prefabs: Archetype 0 / 2 / 3; AdvCap `StartingCurrency: 25`, `GeneratorRequiresManager: 1`.  
- This review did **not** run Unity Play Mode or modify gameplay code.

---

## Out of scope

- Clicker Heroes / Antimatter depth (except shared phase/prestige contracts already green).  
- Full Cookie / Paperclips commercial parity.  
- Pushing remotes.
