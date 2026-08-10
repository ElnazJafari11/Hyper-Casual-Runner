# Round 7 Review 02 — Batch A CPS / Cookie / AdvCap / Paperclips (post–impl_07 R6)

**Agent:** ROUND 7 EXAMINE 2/10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Scope:** Cookie Clicker, AdVenture Capitalist, Universal Paperclips (+ shared CPS / prestige / manager / load sync / offline catch-up / persist keys)  
**Prior:** `round_06/review_02_batchA.md` → `round_06/impl_07_batchA.md` (`851a0de`); R5 verify `6461b37`; R4 persist `e4acd01`; R3 OL-sync `0936ffa` / CatchUp order `bd06da8`; R2 harden `57d2f5d`; R1 P0 `57910f7`  
**Date:** 2026-08-10  
**Action:** Re-audit only — no implementation, no push.

---

## Verdict

Round-6 **documented Play BLOCKED** and claimed EditMode `23/23` with **no gameplay code**. On-disk Batch A CPS contracts (Mult once, prestige-only, hire keeps RequiresManager, CatchUp-after-Sync D25, Managers/Phase/MgrHired round-trip) remain intact under spot-check. Prefabs 01/03/04 wiring unchanged. Play 01/03/04 is still **BLOCKED** — MCP pin / doctor target is `thepcgtoolkit@96e3a310` only; Hyper-Casual-Runner interactive editor absent.

**Evidence caveat (new this round):** the receipt-cited `Logs/IdleBatchA-impl07-r6.log` on disk ends in **compile failure** (`CS0579` Duplicate `Test` in `IdleBatchBCSmokeTests.cs`), not the claimed `pass=23` lines. Sibling artifacts `IdleBatchA-Summary.txt` / `IdleBatchA-TestResults.xml` (mtime ~1 min earlier) do show `23/23 Passed`, and tip AllSmoke `103/103` still lists A1/A6/hire/persist/FirePrestige Passed — but AllSmoke tip predates post-R6 peer D31/gacha edits. Treat R6-A1 as **source-MET + Summary/XML green, dedicated log artifact invalid**; next implementer must re-run dedicated Batch A and cite a log that actually contains the pass lines.

| Lane | Grade |
|------|--------|
| EditMode P0 + R2–R6 contracts (source) | **PASS** — Mult/raw CPS, prestige-only, hire gate, D25 order, persist keys, hire→sim / A6 math fixtures still present |
| Dedicated Batch A log integrity (R6 cite) | **FAIL / STALE ARTIFACT** — `IdleBatchA-impl07-r6.log` = compile exit; Summary/XML `23/23` untracked sibling |
| Tip AllSmoke Batch A fixtures | **PASS (older tip)** — `Logs/IdleAllSmoke-impl04-r5b.log` A1/A6/hire/persist/FirePrestige Passed; Summary `103/103` — pre-D31/gacha peers |
| Play Mode (A7 / R2–R6-A4) | **Still BLOCKED** — checklist `[ ] **BLOCKED**`; Play **0/19**; doctor = HCR absent |
| Archetype identity | **Unchanged** — Paperclips phase still prestige-reset clone (deferred P2) |
| New residual this round | **Evidence hygiene** (P2↑ for verify claims) — no new P0/P1 Batch A numerics |

**Do not re-open M1–M5, R2-A2/A3, OL-sync, R4-A3 persist, or R5/R6 source contracts as P0/P1** unless a fresh dedicated suite fails. Remaining work: fresh Batch A EditMode log (hygiene), Play-Smoke when HCR Unity is on the bridge, optional live-bootstrap fixture, deferred identity depth.

---

## ASSUMPTIONS (this audit)

1. `57910f7`, `57d2f5d`, `bd06da8`, `0936ffa`, `e4acd01`, `6461b37`, `851a0de` are ancestors of current HEAD — **high** — `git merge-base --is-ancestor` all exit 0; HEAD `6964855`.  
2. R6 receipt claimed dedicated Batch A `pass=23` from `IdleBatchA-impl07-r6.log` — **high that receipt claims it; high that on-disk log does not contain those lines** — log ends `Scripts have compiler errors` / exit 1 (`CS0579` at BatchBC:1063).  
3. Sibling Summary/TestResults at 05:18:23 show `23/23` / `passed="23"` — **high** — file read; mtime earlier than failed log rewrite (05:19:26); both untracked (`??`), not in `851a0de`.  
4. Tip AllSmoke `103/103` still includes Batch A fixtures — **high** — `IdleAllSmoke-impl04-r5b.log` + committed `Logs/IdleAllSmoke-Summary.txt`; **med** as post-R6 Batch A regression proof because peer D31/gacha landed after that tip.  
5. Scope stays Cookie / AdvCap / Paperclips; CH/AD only where shared contracts matter — **high**.  
6. Play Mode still unverified / blocked on Hyper-Casual-Runner — **high** — checklist R6-A4 note + MCP instances + `doctor` (pin `thepcgtoolkit`; HCR process absent; TheCheckout batchmode only).  
7. Post-`851a0de` peer edits (gacha identity DNA `8319c9b`; kernel D27/D31/D29 `dc1a472`) touch shared buy/hire/phase destroy + bootstrap gacha fields but **do not** alter Cookie/AdvCap/Paperclips CPS math, CatchUp-after-Sync order, hire `RequiresManager`, or `FirePrestige` exclusivity — **high** — `git diff 851a0de..HEAD` on those paths.  
8. A5/A6 still exercise shared sync/catch-up helpers, not a live `IdleSliceBootstrap` MonoBehaviour — **high** — test source unchanged.

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
| A6 | Batch A smokes + A1–A4 | **MET (source + Summary)** | Suite still 23 `[Test]`; Summary/XML `23/23`; dedicated R6 log **invalid** |
| A7 | Play Mode 01/03/04 | **UNMET / BLOCKED** | Checklist `[ ] **BLOCKED**` |

### R2-A1…A5

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R2-A1 | Existing A1–A5 + zero-currency + AngelReset + phase exclusivity still PASS | **MET (source)** | Fixtures present; Summary `23/23` |
| R2-A2 | Buy Mult=2 → 1s → Primary **2±0.05**, PassiveRate **1** | **MET** | Hardened `A1_…` |
| R2-A3 | Save Gens=3 → reload → next buy `15*1.15^3` | **MET** | Hardened `A5_…` |
| R2-A4 | Play Mode 01/03/04 checklist + evidence | **UNMET / BLOCKED** | |
| R2-A5 | `FirePrestige` has no `FirePhase` | **MET** | UI: creates `PrestigeEventComponent` only |

### R3-A1…A5

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R3-A1 | Existing Batch A suite + AngelReset + phase exclusivity still PASS | **MET (source)** | |
| R3-A2 | Stale Mult² Passive → Sync then catch-up 1s → gain **6±0.05**, PassiveRate **3** | **MET** | Prod D25; `A6_…` asserts unchanged |
| R3-A3 | Persist Managers/Phase/MgrHired round-trip asserts | **MET** | Fixture + `GameProgressData` keys intact |
| R3-A4 | Play Mode 01/03/04 | **UNMET / BLOCKED** | |
| R3-A5 | `FirePrestige` no `FirePhase`; Sync still used from bootstrap CPS attach | **MET** | UI + `AttachArchetypeExtras` Sync + post-Attach CatchUp |

### R4-A1…A5

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R4-A1 | Suite incl. hire→sim + phase exclusivity still PASS | **MET (source)** | |
| R4-A2 | Bootstrap Sync **before** `ApplyPersistedElapsed`; A6 gain **6±0.05** / Passive **3** | **MET** | D25 comment+order intact |
| R4-A3 | Round-trip ManagersHired / PhaseIndex / MgrHired | **MET** | Fixture + prefs keys |
| R4-A4 | Play Mode 01/03/04 | **UNMET / BLOCKED** | |
| R4-A5 | `FirePrestige` no `FirePhase`; hire keeps `RequiresManager` | **MET** | UI + hire comment+flag |

### R5-A1…A5

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R5-A1…A3 / A5 | EditMode contracts | **MET (source)** | Unchanged since R5 verify |
| R5-A4 | Play Mode 01/03/04 | **BLOCKED** | |

### R6-A1…A5 (impl_07 targets)

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| R6-A1 | Existing Batch A suite still PASS | **MET on source + Summary/XML; cited log INVALID** | Summary `pass=23`; TestResults `passed="23"`; `IdleBatchA-impl07-r6.log` = CS0579 compile fail (no pass lines) |
| R6-A2 | Bootstrap Attach Sync before CatchUp; A6 gain **6±0.05** / Passive **3** | **MET** | Source D25; A6 fixture asserts unchanged |
| R6-A3 | Persist fixture AdvCap mgrs/hired + Paperclips phase | **MET** | Fixture + keys; no persist-key regression in peer gacha diff |
| R6-A4 | Play Mode checklist 01/03/04 done **or** remain BLOCKED | **BLOCKED (honest)** | Checklist + progress cite MCP mismatch; doctor this audit |
| R6-A5 | `FirePrestige` no `FirePhase`; hire keeps `RequiresManager` | **MET** | Spot-check this audit |

---

## R6 issue close-out

### Closed / verified by impl_07 (with caveat)

| ID | R6 issue | Resolution |
|----|----------|------------|
| R6-A2 / A3 / A5 | Reconfirm contracts after R5 | Closed on **source** — no gameplay edits; spot-check OK |
| R6-A4 documentation | Play claim honesty | Closed as **BLOCKED** (not falsely checked) — checklist + progress |
| R6-A1 EditMode green | Claimed `23/23` | **Partially closed** — Summary/XML green; **dedicated log cite does not match** (overwritten by compile-fail run) |

### Still open

| ID | Issue | Priority | Notes |
|----|--------|----------|-------|
| A7 / R2–R6-A4 | No Play Mode evidence for prefabs 01/03/04 | **P0 playability claim** | Still BLOCKED: bridge = `thepcgtoolkit` only; HCR editor absent (`doctor` this pass) |
| EVID-R6 | R6 dedicated Batch A log does not contain claimed pass lines | **P2↑ hygiene** (blocks “verified via that log”) | Re-run `IdleBatchATestRunner.RunAndExit`; cite log body + Summary together |
| T3 | No UI-level assert Prestige skips Phase | P2 | Code-path review + `FirePrestige_…` fixture still sufficient |
| A5/A6-live | Fixtures do not spawn live `IdleSliceBootstrap` | P2 test | Same Sync→CatchUp contract; MonoBehaviour path unproven in fixture |
| A6-stamp | A6 uses bare `Apply(…, 1.0)` not `ApplyPersistedElapsed` | P2 test | Wall-clock stamp covered elsewhere |
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

## New finding (Round 7)

### EVID-R6 — dedicated log / receipt mismatch (hygiene)

`round_06/impl_07_batchA.md` pastes:

```
A1_Cookie_… => Passed
…
result=Passed pass=23 fail=0 … duration=1.6259191
```

as coming from `Logs/IdleBatchA-impl07-r6.log`. On disk that log’s tail is:

```
Assets\Scripts\Editor\Tests\IdleBatchBCSmokeTests.cs(1063,10): error CS0579: Duplicate 'Test' attribute
Scripts have compiler errors.
… return code 1
```

Timeline: Summary/TestResults mtime **05:18:23** (`23/23`) → log rewrite **05:19:26** (compile fail) → commit `851a0de` **05:20:51**. Most likely a concurrent BatchBC/editor compile polluted the same `-logFile` path after a green Summary write. **Do not treat that log path as Batch A green evidence.** Current tree no longer shows duplicate `[Test]` on that method (peer fix), but Batch A still needs a **fresh** dedicated run whose log body contains the pass lines.

No new P0/P1 Batch A **correctness** regressions found in source.

Optional notes (not reopens):

- Post-`851a0de` peers: D31 `DestroyIfEphemeral` on buy/hire/click/phase (still destroys UI ephemeral CreateEntity events; skips slice-as-event); gacha HeroId/DupeCount/GearSlot bootstrap/persist/HUD — **out of Batch A CPS scope**.  
- Tip AllSmoke remains **103/103** (committed). Concurrent suite growth may diverge WT; cite dedicated Batch A log+Summary for Batch A claims after re-run.  
- Historical Mult³ Primary-on-disk still accepted MVP (Sync corrects PassiveRate going forward).  
- MCP/`doctor` this audit: discovered instance `thepcgtoolkit@96e3a310` only; processes = thepcgtoolkit + TheCheckout batchmode; Hyper-Casual-Runner editor **absent**. Play 01/03/04 remains **BLOCKED**.

---

## Per-title re-audit

### 1) Cookie Clicker (`IdleArchetype.CookieClicker` / `01_…`)

**Loop (MVP):** Click → Buy Generator (raw CPS) → Prestige (single conversion) → Mult on sim / offline.

**Since R6:** No Cookie gameplay change; A1/A5/A6 fixtures + sim Mult path unchanged. Prefab Archetype `0`, PanelSettings assigned. Peer D31 only changes event destroy hygiene.

**Remaining:** Play Mode (BLOCKED until HCR Unity on bridge); single-tier gen; fresh dedicated EditMode log.

### 2) AdVenture Capitalist (`…AdventureCapitalist` / `03_…`)

**Loop (MVP):** Collect → Buy (PassiveRate 0 while gated) → Hire (keep RequiresManager) → income tick → Angel Reset restores gate.

**Since R6:** ManagersHired + MgrHired keys + hire `IsAutomated` keep-flag still intact; Sync with `automated = MgrHired` still forces PassiveRate=0 when not hired before CatchUp. Prefab Archetype `2`, `StartingCurrency: 25`, `GeneratorRequiresManager: 1`. Collect still `Max(1, OwnedGenerators)` manual labor (intentional).

**Remaining:** Play Mode (BLOCKED); optional AdvCap Sync→CatchUp gain=0 when MgrHired=false + stale Passive>0 (P2).

### 3) Universal Paperclips (`…UniversalPaperclips` / `04_…`)

**Loop (MVP):** Manufacture → Buy Autoclipper → Phase Shift (Paperclips/AD-only conversion + PhaseIndex).

**Since R6:** PhaseIndex persist + phase exclusivity fixtures still present; `IdlePhaseShiftSystem` still convert + PhaseIndex++ + reset gens (D31 DestroyIfEphemeral only). Identity depth still deferred.

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
| Prestige / phase / sim | `PrestigeSystem.cs`, `IdleSliceActionSystems.cs` (`IdlePhaseShiftSystem`), `IdleSliceSimulationSystem.cs` |
| Offline | `IdleOfflineCatchUp.cs`, bootstrap CatchUp-after-Attach order |
| Bootstrap / persist | `IdleSliceBootstrap.cs`, `GameProgressData.cs` |
| UI | `IdleSliceUIController.cs` (`FirePrestige` / `FirePhase`) |
| Prefabs | `01_CookieClicker_Generators_Slice.prefab`, `03_AdventureCapitalist_Managers_Slice.prefab`, `04_UniversalPaperclips_Phase_Slice.prefab` |
| Tests / evidence | `IdleBatchASmokeTests.cs` (23 `[Test]`), `Logs/IdleBatchA-Summary.txt`, `Logs/IdleBatchA-TestResults.xml`, `Logs/IdleBatchA-impl07-r6.log` (**invalid cite**), `Logs/IdleAllSmoke-impl04-r5b.log`, `Logs/IdleAllSmoke-Summary.txt`, `docs/idle-toolkit-progress.md`, `docs/idle-play-smoke-checklist.md` |
| Receipts | `round_06/impl_07_batchA.md`, `round_06/review_02_batchA.md`, `round_05/impl_08_batchA.md`, `round_05/review_02_batchA.md` |

---

## Implementer fix list (Round 7 — priority)

### P0 — only if claiming Play-ready

1. **Play-Smoke prefabs 01, 03, 04** on Hyper-Casual-Runner (human or MCP). Unblock requires Unity opened on this repo and registered on the MCP bridge. Check boxes in `docs/idle-play-smoke-checklist.md`; update Play column only with evidence. Until then keep `[ ] **BLOCKED**`.

### P1 — none for Batch A numerics/contracts

No open P1 Batch A correctness after R3 OL-sync + R4 persist + R5/R6 source verify. **Do** re-establish a clean dedicated EditMode artifact (below) before claiming a new verify-green receipt.

### P2 — evidence hygiene + deferred proof / identity / polish

2. **Fresh dedicated Batch A EditMode** (`IdleBatchATestRunner.RunAndExit`) after peer compile is clean; paste pass lines from the **same** log file that wrote Summary; do not cite a log that only shows Bee/csc errors.  
3. Optional live `IdleSliceBootstrap` spawn/load fixture (A5/A6-live) or AdvCap Sync→CatchUp gain=0 when MgrHired=false + stale Passive>0.  
4. Cookie: 2–3 named generator rows.  
5. Paperclips: PhaseIndex≥1 swaps verb set and/or production formula (compute stub OK).  
6. Optional UI-level assert that Prestige button path never creates `IdlePhaseShiftEvent` (T3).

---

## Acceptance criteria for next implementer (machine-checkable)

| # | Criterion |
|---|-----------|
| R7-A1 | Dedicated Batch A suite still **PASS** (`pass=23 fail=0`); **cited log body** must contain `A1_…`/`A6_…`/`ManagersPhaseMgrHired`/`FirePrestige_…` => Passed **and** matching Summary line (do not cite a compile-fail log). |
| R7-A2 | Bootstrap source still: Attach Sync **before** `ApplyPersistedElapsed`; `A6_…` still expects gain **6±0.05** / PassiveRate **3**. |
| R7-A3 | Persist fixture still asserts AdvCap ManagersHired/MgrHired and Paperclips PhaseIndex round-trip. |
| R7-A4 | Play Mode: checklist rows for 01, 03, 04 marked done with log/capture path cited in progress doc — **or** remain explicitly **BLOCKED** with MCP/HCR evidence (do not silently check). |
| R7-A5 | No regression: `FirePrestige` source still has no `FirePhase` call; hire still keeps `RequiresManager`; D31 DestroyIfEphemeral must not break buy/hire/phase ephemeral event cleanup. |

---

## Evidence notes

- Commits: `57910f7` (R1 P0), `57d2f5d` (R2 A1/A5), `bd06da8` (D25 CatchUp-after-Sync), `0936ffa` (R3 Batch A), `e4acd01` (R4 persist), `6461b37` (R5 verify), `851a0de` (R6 verify + Play BLOCKED docs).  
- Batch A Summary/XML (untracked, 05:18:23): `result=Passed pass=23 fail=0` / `passed="23"`.  
- Batch A cited log (05:19:26): **compile fail** — not usable as green evidence.  
- AllSmoke tip: `result=Passed pass=103 fail=0` (`Logs/IdleAllSmoke-Summary.txt`); Batch A fixtures Passed in `Logs/IdleAllSmoke-impl04-r5b.log` (pre-D31/gacha peers).  
- Prefabs: Archetype 0 / 2 / 3; AdvCap start 25 + RequiresManager; PanelSettings assigned.  
- Play probe: MCP `instance_count=1` → `thepcgtoolkit@96e3a310`; `doctor` → HCR absent, pin thepcgtoolkit, registered_sessions empty.  
- This review did **not** run Unity Play Mode, did **not** modify gameplay code, and did **not** push.

---

## Out of scope

- Clicker Heroes / Antimatter depth (except shared phase/prestige / Mult contracts already green).  
- Full Cookie / Paperclips commercial parity.  
- Kernel stamp-policy ownership beyond confirming Batch A uses CatchUp-after-Sync + D31 destroy hygiene.  
- Fixing BatchBC duplicate-Test history (already absent on current disk; not Batch A ownership).  
- Pushing remotes.
