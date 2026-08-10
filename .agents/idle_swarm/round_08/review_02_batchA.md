# Round 8 Review 02 — Batch A CPS / Cookie / AdvCap / Paperclips (post–impl_05 R7)

**Agent:** ROUND 8 EXAMINE 2/10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Scope:** Cookie Clicker, AdVenture Capitalist, Universal Paperclips (+ shared CPS / prestige / manager / load sync / offline catch-up / persist keys)  
**Prior:** `round_07/review_02_batchA.md` → `round_07/impl_05_batchA.md` (`e12176c` / docs `7eb485b`); R6 verify `851a0de` (EVID-R6 invalid log); R5 `6461b37`; R4 persist `e4acd01`; R3 OL-sync `0936ffa` / CatchUp order `bd06da8`; R2 harden `57d2f5d`; R1 P0 `57910f7`  
**Date:** 2026-08-10  
**Action:** Re-audit only — no implementation, no push.

---

## Verdict

Round-7 closed the **EVID-R6** hygiene hole with a **valid** dedicated Batch A artifact: `Logs/IdleBatchA-impl05-r7.log` body contains all critical `=> Passed` lines and the matching `pass=23` Summary line (sibling Summary/XML `23/23`). On-disk Batch A CPS contracts (Mult once, prestige-only, hire keeps RequiresManager, CatchUp-after-Sync D25, Managers/Phase/MgrHired round-trip) remain intact under spot-check. Prefabs 01/03/04 wiring unchanged. Post-`e12176c` peer Capybara DNA edits touch shared bootstrap/action/UI files but **do not** alter Cookie/AdvCap/Paperclips CPS, hire, prestige exclusivity, or CatchUp order.

Play 01/03/04 remains **BLOCKED**. MCP `doctor` this pass: discovered instance still `thepcgtoolkit@96e3a310` only; registered_sessions empty; pin = thepcgtoolkit. HCR appears only as a **batchmode** process (not an interactive bridge session) — still insufficient for Play-smoke.

| Lane | Grade |
|------|--------|
| EditMode P0 + R2–R7 contracts (source) | **PASS** — Mult/raw CPS, prestige-only, hire gate, D25 order, persist keys, hire→sim / A6 math fixtures still present |
| Dedicated Batch A log integrity (R7 cite) | **PASS** — `IdleBatchA-impl05-r7.log` contains A1/A5/A6/hire/ManagersPhaseMgrHired/FirePrestige + `pass=23`; Summary/XML match |
| Tip AllSmoke Batch A fixtures | **PASS** — tip `114/114` (`IdleAllSmoke-Summary.txt`); duration-matched `IdleAllSmoke-impl03-r7.log` shows A1/A6/persist/FirePrestige Passed |
| Play Mode (A7 / R2–R7-A4) | **Still BLOCKED** — checklist `[ ] **BLOCKED**`; Play **0/19**; doctor = no interactive HCR on bridge |
| Archetype identity | **Unchanged** — Paperclips phase still prestige-reset clone (deferred P2) |
| New residual this round | **None for Batch A numerics** — optional note: R7 Unity exit AV after green Summary (documented; not a test failure) |

**Do not re-open M1–M5, R2-A2/A3, OL-sync, R4-A3 persist, R5–R7 source contracts, or EVID-R6 as P0/P1** unless a fresh dedicated suite fails. Remaining work: Play-Smoke when interactive HCR Unity is on the bridge; optional live-bootstrap fixture; deferred identity depth. Re-run dedicated Batch A only if peers touch buy/hire/prestige/CatchUp/persist again.

---

## ASSUMPTIONS (this audit)

1. `57910f7`, `57d2f5d`, `bd06da8`, `0936ffa`, `e4acd01`, `6461b37`, `851a0de`, `e12176c`, `7eb485b` are ancestors of current HEAD — **high** — `git merge-base --is-ancestor` all exit 0; HEAD `b8527f5`.  
2. R7 receipt claimed dedicated Batch A `pass=23` from `IdleBatchA-impl05-r7.log` — **high** — log body contains 23 fixture `=> Passed` lines plus `result=Passed pass=23 … → Logs/IdleBatchA-Summary.txt`.  
3. Sibling Summary/TestResults at 05:44:59 show `23/23` / `passed="23"`; log mtime 05:45:19 continues with shutdown noise after Summary write — **high** — same run; Summary not rewritten by a later failed Batch A.  
4. Tip AllSmoke `114/114` still includes Batch A fixtures — **high** — `IdleAllSmoke-impl03-r7.log` A1/A6/ManagersPhaseMgrHired/FirePrestige Passed + Summary duration match.  
5. Scope stays Cookie / AdvCap / Paperclips; CH/AD only where shared contracts matter — **high**.  
6. Play Mode still unverified / blocked on interactive Hyper-Casual-Runner — **high** — checklist R7-A4 note + `doctor` this audit (HCR batchmode pid present ≠ bridge Play session).  
7. Post-`e12176c` peer Capybara DNA (`IdleSliceBootstrap` RunId cold-start; `IdleSliceActionSystems` choice/pet; UI Capybara buttons) does **not** alter Cookie/AdvCap/Paperclips CPS math, CatchUp-after-Sync order, hire `RequiresManager`, or `FirePrestige` exclusivity — **high** — `git diff e12176c..HEAD` on those paths; no commits to buy/prestige/math/offline/BatchA tests since `e12176c`.  
8. A5/A6 still exercise shared sync/catch-up helpers, not a live `IdleSliceBootstrap` MonoBehaviour — **high** — test source unchanged (23 `[Test]`).

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
| A6 | Batch A smokes + A1–A4 | **MET (source + valid log)** | Suite 23 `[Test]`; Summary/XML `23/23`; `impl05-r7.log` pass lines |
| A7 | Play Mode 01/03/04 | **UNMET / BLOCKED** | Checklist `[ ] **BLOCKED**` |

### R2-A1…A5

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R2-A1 | Existing A1–A5 + zero-currency + AngelReset + phase exclusivity still PASS | **MET** | Fixtures + dedicated log `23/23` |
| R2-A2 | Buy Mult=2 → 1s → Primary **2±0.05**, PassiveRate **1** | **MET** | Hardened `A1_…` |
| R2-A3 | Save Gens=3 → reload → next buy `15*1.15^3` | **MET** | Hardened `A5_…` |
| R2-A4 | Play Mode 01/03/04 checklist + evidence | **UNMET / BLOCKED** | |
| R2-A5 | `FirePrestige` has no `FirePhase` | **MET** | UI: creates `PrestigeEventComponent` only |

### R3-A1…A5

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R3-A1 | Existing Batch A suite + AngelReset + phase exclusivity still PASS | **MET** | |
| R3-A2 | Stale Mult² Passive → Sync then catch-up 1s → gain **6±0.05**, PassiveRate **3** | **MET** | Prod D25; `A6_…` asserts unchanged |
| R3-A3 | Persist Managers/Phase/MgrHired round-trip asserts | **MET** | Fixture + `GameProgressData` keys intact |
| R3-A4 | Play Mode 01/03/04 | **UNMET / BLOCKED** | |
| R3-A5 | `FirePrestige` no `FirePhase`; Sync still used from bootstrap CPS attach | **MET** | UI + `AttachArchetypeExtras` Sync + post-Attach CatchUp |

### R4-A1…A5

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R4-A1 | Suite incl. hire→sim + phase exclusivity still PASS | **MET** | |
| R4-A2 | Bootstrap Sync **before** `ApplyPersistedElapsed`; A6 gain **6±0.05** / Passive **3** | **MET** | D25 comment+order intact |
| R4-A3 | Round-trip ManagersHired / PhaseIndex / MgrHired | **MET** | Fixture + prefs keys `Managers` / `Phase` / `MgrHired` |
| R4-A4 | Play Mode 01/03/04 | **UNMET / BLOCKED** | |
| R4-A5 | `FirePrestige` no `FirePhase`; hire keeps `RequiresManager` | **MET** | UI + hire comment+flag |

### R5-A1…A5 / R6-A1…A5

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R5/R6 EditMode contracts | **MET (source)** | Unchanged; R6 log cite superseded by R7 |
| R5/R6-A4 Play | **BLOCKED** | |

### R7-A1…A5 (impl_05 targets)

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R7-A1 | Dedicated Batch A suite PASS; cited log body has pass lines + matching Summary | **MET** | `IdleBatchA-impl05-r7.log` + Summary/XML `23/23` (EVID-R6 closed) |
| R7-A2 | Bootstrap Attach Sync before CatchUp; A6 gain **6±0.05** / PassiveRate **3** | **MET** | Source D25; `A6_… => Passed` in log |
| R7-A3 | Persist fixture AdvCap mgrs/hired + Paperclips phase | **MET** | `ManagersPhaseMgrHired_… => Passed` |
| R7-A4 | Play Mode checklist 01/03/04 done **or** remain BLOCKED | **BLOCKED (honest)** | Checklist + progress + doctor this audit |
| R7-A5 | `FirePrestige` no `FirePhase`; hire keeps `RequiresManager`; D31 DestroyIfEphemeral | **MET** | Spot-check this audit; suite green |

---

## R7 issue close-out

### Closed / verified by impl_05

| ID | R7 issue | Resolution |
|----|----------|------------|
| R7-A1 / EVID-R6 | Fresh dedicated log with pass lines | **Closed** — `IdleBatchA-impl05-r7.log` valid; do not cite `impl07-r6.log` |
| R7-A2 / A3 / A5 | Reconfirm contracts after R6 | Closed on **source + green suite** — no gameplay edits |
| R7-A4 documentation | Play claim honesty | Closed as **BLOCKED** (not falsely checked) — checklist + progress |

### Still open

| ID | Issue | Priority | Notes |
|----|--------|----------|-------|
| A7 / R2–R7-A4 | No Play Mode evidence for prefabs 01/03/04 | **P0 playability claim** | Still BLOCKED: interactive HCR absent from MCP bridge (`doctor` this pass); HCR batchmode process ≠ Play-smoke |
| T3 | No UI-level assert Prestige skips Phase | P2 | Code-path review + `FirePrestige_…` fixture still sufficient |
| A5/A6-live | Fixtures do not spawn live `IdleSliceBootstrap` | P2 test | Same Sync→CatchUp contract; MonoBehaviour path unproven in fixture |
| A6-stamp | A6 uses bare `Apply(…, 1.0)` not `ApplyPersistedElapsed` | P2 test | Wall-clock stamp covered elsewhere |
| ID-Cookie | Single generator tier | P2 deferred | Matrix ladder still one-row |
| ID-Paper | Phase ≠ loop replacement / no compute alloc | P2 deferred | `IdlePhaseShiftSystem` still reset + PhaseIndex + mult |
| E-stub | UI `TODO: [STUB]` PanelSettings / inline styles | P2 | Prefabs assign PanelSettings; fallback only |
| EXIT-AV | R7 batchmode process exit AV after green Summary | P3 note | Receipt documents `0xC0000005` on exit; treat Summary+log pass lines as evidence, not process exit code |

### R1 P0 + R2 harden + R3 OL-sync + R4 persist + R7 evidence (remain closed — spot-checked)

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
| EVID-R6 invalid dedicated log | Closed (R7) — superseded by `impl05-r7.log` |

---

## New finding (Round 8)

### No new P0/P1 Batch A correctness regressions

Source + dedicated EditMode evidence remain green. Peer Capybara DNA post-`e12176c` is **out of Batch A CPS scope** (narrative RunId / PendingChoice / PetId only).

### Play bridge nuance (evidence update, not a reopen)

R7 doctor: HCR process **absent**. This audit: HCR **batchmode** process present (`pid` ~50380) alongside thepcgtoolkit interactive + TheCheckout batchmode, but **discovered_instances** still only `thepcgtoolkit@96e3a310`, **registered_sessions** empty, pin = thepcgtoolkit. Play-smoke of prefabs 01/03/04 still **cannot** be claimed.

Optional notes (not reopens):

- Tip AllSmoke advanced **103 → 114** since R6 tip; Batch A fixtures still Passed in duration-matched `IdleAllSmoke-impl03-r7.log`. Prefer dedicated `IdleBatchA-impl05-r7.log` (+ Summary) for Batch A claims.  
- Historical Mult³ Primary-on-disk still accepted MVP (Sync corrects PassiveRate going forward).  
- R7 exit AV after Summary write is shutdown noise; do not treat as suite failure.

---

## Per-title re-audit

### 1) Cookie Clicker (`IdleArchetype.CookieClicker` / `01_…`)

**Loop (MVP):** Click → Buy Generator (raw CPS) → Prestige (single conversion) → Mult on sim / offline.

**Since R7:** No Cookie gameplay change; A1/A5/A6 fixtures + sim Mult path unchanged. Prefab Archetype `0`, PanelSettings assigned. Peer Capybara edits do not touch Cookie buttons / FirePrestige.

**Remaining:** Play Mode (BLOCKED until interactive HCR Unity on bridge); single-tier gen.

### 2) AdVenture Capitalist (`…AdventureCapitalist` / `03_…`)

**Loop (MVP):** Collect → Buy (PassiveRate 0 while gated) → Hire (keep RequiresManager) → income tick → Angel Reset restores gate.

**Since R7:** ManagersHired + MgrHired keys + hire `IsAutomated` keep-flag still intact; Sync with `automated = MgrHired` still forces PassiveRate=0 when not hired before CatchUp. Prefab Archetype `2`, `StartingCurrency: 25`, `GeneratorRequiresManager: 1`. Collect still `Max(1, OwnedGenerators)` manual labor (intentional).

**Remaining:** Play Mode (BLOCKED); optional AdvCap Sync→CatchUp gain=0 when MgrHired=false + stale Passive>0 (P2).

### 3) Universal Paperclips (`…UniversalPaperclips` / `04_…`)

**Loop (MVP):** Manufacture → Buy Autoclipper → Phase Shift (Paperclips/AD-only conversion + PhaseIndex).

**Since R7:** PhaseIndex persist + phase exclusivity fixtures still present; `IdlePhaseShiftSystem` still convert + PhaseIndex++ + reset gens. Identity depth still deferred.

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
Events: DestroyIfEphemeral (D31) — ephemeral CreateEntity only; never destroy slice
```

Primary owners: `IdlePrestigeMath.cs`, `IdleBuyGeneratorSystem.cs` (incl. hire), `PrestigeSystem.cs`, `IdlePhaseShiftSystem`, `IdleSliceSimulationSystem.cs`, `IdleOfflineCatchUp` / bootstrap load order, `IdleSliceUIController.FirePrestige/FirePhase`, `IdleSliceBootstrap` + `GameProgressData`, `IdleEventTarget.DestroyIfEphemeral`.

---

## Files re-examined

| Area | Paths |
|------|--------|
| Math / sync | `Assets/Scripts/ECS/Systems/Idle/IdlePrestigeMath.cs` |
| Buy / hire | `IdleBuyGeneratorSystem.cs` (incl. hire + DestroyIfEphemeral) |
| Prestige / phase / sim | `PrestigeSystem.cs`, `IdleSliceActionSystems.cs` (`IdlePhaseShiftSystem` + peer Capybara DNA delta), `IdleSliceSimulationSystem.cs` |
| Offline | `IdleOfflineCatchUp.cs`, bootstrap CatchUp-after-Attach order |
| Bootstrap / persist | `IdleSliceBootstrap.cs`, `GameProgressData.cs` |
| UI | `IdleSliceUIController.cs` (`FirePrestige` / `FirePhase`) |
| Prefabs | `01_CookieClicker_Generators_Slice.prefab`, `03_AdventureCapitalist_Managers_Slice.prefab`, `04_UniversalPaperclips_Phase_Slice.prefab` |
| Tests / evidence | `IdleBatchASmokeTests.cs` (23 `[Test]`), `Logs/IdleBatchA-Summary.txt`, `Logs/IdleBatchA-TestResults.xml`, `Logs/IdleBatchA-impl05-r7.log` (**valid**), `Logs/IdleAllSmoke-impl03-r7.log`, `Logs/IdleAllSmoke-Summary.txt` (`114/114`), `docs/idle-toolkit-progress.md`, `docs/idle-play-smoke-checklist.md` |
| Receipts | `round_07/impl_05_batchA.md`, `round_07/review_02_batchA.md`, `round_06/impl_07_batchA.md`, `round_06/review_02_batchA.md` |

---

## Implementer fix list (Round 8 — priority)

### P0 — only if claiming Play-ready

1. **Play-Smoke prefabs 01, 03, 04** on Hyper-Casual-Runner (human or MCP). Unblock requires **interactive** Unity opened on this repo and **registered** on the MCP bridge (batchmode HCR alone is not enough). Check boxes in `docs/idle-play-smoke-checklist.md`; update Play column only with evidence. Until then keep `[ ] **BLOCKED**`.

### P1 — none for Batch A numerics/contracts

No open P1 Batch A correctness after R3 OL-sync + R4 persist + R5–R7 verify + valid dedicated log. Re-run dedicated Batch A only if a peer touches buy/hire/prestige/CatchUp/persist/D25 order.

### P2 — deferred proof / identity / polish

2. Optional live `IdleSliceBootstrap` spawn/load fixture (A5/A6-live) or AdvCap Sync→CatchUp gain=0 when MgrHired=false + stale Passive>0.  
3. Cookie: 2–3 named generator rows.  
4. Paperclips: PhaseIndex≥1 swaps verb set and/or production formula (compute stub OK).  
5. Optional UI-level assert that Prestige button path never creates `IdlePhaseShiftEvent` (T3).

---

## Acceptance criteria for next implementer (machine-checkable)

| # | Criterion |
|---|-----------|
| R8-A1 | Dedicated Batch A suite still **PASS** (`pass=23 fail=0`); cite `Logs/IdleBatchA-impl05-r7.log` (or a newer dedicated log whose **body** contains `A1_…`/`A6_…`/`ManagersPhaseMgrHired`/`FirePrestige_…` => Passed **and** matching Summary). Do **not** cite `IdleBatchA-impl07-r6.log`. |
| R8-A2 | Bootstrap source still: Attach Sync **before** `ApplyPersistedElapsed`; `A6_…` still expects gain **6±0.05** / PassiveRate **3**. |
| R8-A3 | Persist fixture still asserts AdvCap ManagersHired/MgrHired and Paperclips PhaseIndex round-trip. |
| R8-A4 | Play Mode: checklist rows for 01, 03, 04 marked done with log/capture path cited in progress doc — **or** remain explicitly **BLOCKED** with MCP/HCR evidence (interactive bridge required; do not silently check). |
| R8-A5 | No regression: `FirePrestige` source still has no `FirePhase` call; hire still keeps `RequiresManager`; D31 DestroyIfEphemeral must not break buy/hire/phase ephemeral event cleanup. |

---

## Evidence notes

- Commits: `57910f7` (R1 P0), `57d2f5d` (R2 A1/A5), `bd06da8` (D25 CatchUp-after-Sync), `0936ffa` (R3 Batch A), `e4acd01` (R4 persist), `6461b37` (R5 verify), `851a0de` (R6 verify + Play BLOCKED; invalid log cite), `e12176c` / `7eb485b` (R7 verify-green + Play BLOCKED docs).  
- Batch A Summary/XML (05:44:59): `result=Passed pass=23 fail=0` / `passed="23"`.  
- Batch A dedicated log (05:45:19): **valid** — contains A1…A6, hire, ManagersPhaseMgrHired, FirePrestige, and final `pass=23` line (R6 compile-fail cite superseded).  
- AllSmoke tip: `result=Passed pass=114 fail=0` (`Logs/IdleAllSmoke-Summary.txt`); Batch A fixtures Passed in `Logs/IdleAllSmoke-impl03-r7.log`.  
- Prefabs: Archetype 0 / 2 / 3; AdvCap start 25 + RequiresManager; PanelSettings assigned.  
- Play probe: MCP `doctor` → discovered `thepcgtoolkit@96e3a310` only; registered_sessions empty; pin thepcgtoolkit; HCR batchmode process present but **not** bridge-registered for Play.  
- This review did **not** run Unity Play Mode, did **not** modify gameplay code, and did **not** push.

---

## Out of scope

- Clicker Heroes / Antimatter depth (except shared phase/prestige / Mult contracts already green).  
- Full Cookie / Paperclips commercial parity.  
- Capybara / gacha DNA ownership (peer lane; confirmed non-regressive for Batch A CPS).  
- Kernel stamp-policy ownership beyond confirming Batch A uses CatchUp-after-Sync + D31 destroy hygiene.  
- Pushing remotes.
