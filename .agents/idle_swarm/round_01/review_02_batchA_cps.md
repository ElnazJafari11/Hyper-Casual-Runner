# Round 1 Review 02 — Batch A Generator / CPS Slices

**Agent:** 2/10 (examine / analyze / review)  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Scope:** Cookie Clicker, AdVenture Capitalist, Universal Paperclips (+ shared authoring / systems / UI verbs / Idle prefabs)  
**Date:** 2026-08-10  
**Action taken:** Review only — no gameplay/code implementation.

---

## Verdict

EditMode Batch A smoke is green (`IdleBatchASmokeTests` 6/6 per `docs/idle-toolkit-progress.md`), but the three CPS/generator titles are **MVP shells with load-bearing math and prestige bugs**. They are not Play-Mode verified (0/19). Treat “Batch A PASS” as verb wiring evidence, not playability or numerics correctness.

**Playability grade (toolkit quality bar):** partial — core HUD verbs exist; progression beats are incomplete or wrong for two of three titles.

---

## Files examined

| Area | Paths |
|------|--------|
| Prefabs | `Assets/ToolkitExamples/Idle/01_CookieClicker_Generators_Slice.prefab`, `03_AdventureCapitalist_Managers_Slice.prefab`, `04_UniversalPaperclips_Phase_Slice.prefab` |
| Authoring | `Assets/Scripts/ECS/Authoring/IdleSliceBootstrap.cs` |
| Components | `Assets/Scripts/ECS/Components/IdleSliceComponents.cs` |
| Systems | `IdleBuyGeneratorSystem.cs`, `IdleClickProduceSystem.cs`, `IdleSliceSimulationSystem.cs`, `IdleSliceActionSystems.cs` (`IdlePhaseShiftSystem`), `PrestigeSystem.cs` |
| UI | `Assets/Scripts/UI/IdleSliceUIController.cs` |
| Generator | `Assets/Scripts/Editor/IdleToolkitSliceGenerator.cs` |
| Persist | `Assets/Scripts/GameProgressData.cs`, `IdleSaveManager.cs` |
| Tests | `Assets/Scripts/Editor/Tests/IdleBatchASmokeTests.cs` |
| Design | `docs/idle_mechanics_matrix.md`, `docs/idle-toolkit-progress.md`, `README_IDLE_SLICES.txt` |

---

## Per-title findings

### 1) Cookie Clicker (`IdleArchetype.CookieClicker` / prefab 01)

**Intended loop (matrix):** Click → buy generators (CPS) → heavenly prestige mult.

**What works (code path):**
- Prefab wires bootstrap + UIDocument + `IdleSliceUIController` + `IdleSaveManager`.
- UI: Click Cookie / Buy Generator / Prestige.
- Click adds `ClickPower * Mult * GlobalMultiplier` (default click 1).
- Buy uses geometric cost `BaseCost * growth^OwnedCount` (15 × 1.15^n) and sets `PassiveRate` when automated.
- Sim tick: `PrimaryCurrency += PassiveRate * GlobalMultiplier * dt`.

**Gaps / bugs:**
1. **Double prestige on Prestige button (P0).** `FirePrestige()` fires `IdlePhaseShiftEvent` *and* a free-floating `PrestigeEventComponent`. Both `IdlePhaseShiftSystem` and `PrestigeSystem` convert run currency → prestige with nearly the same √ formula. Prestige then often runs on already-zeroed currency and still forces `converted = 1` (`PrestigeSystem` lines 50–52), so every prestige grants **at least +1 extra** prestige past the phase-shift conversion.
2. **GlobalMultiplier applied twice to CPS (P0 math).** Buy stores `PassiveRate = BaseCps * OwnedCount * GlobalMultiplier`; sim applies `PassiveRate * GlobalMultiplier * dt` again → effective CPS ∝ **mult²**.
3. **Single generator tier only.** Matrix “Cursor/Grandma” ladder is a one-row shell (`GeneratorId = 1`, one `BuyableGenerator`). Acceptable as deferred depth, not as “theorycrafting” fidelity.
4. **No Play Mode evidence.** EditMode never ticks `IdleSliceSimulationSystem` for Cookie CPS accrual.

### 2) AdVenture Capitalist (`IdleArchetype.AdventureCapitalist` / prefab 03)

**Intended loop:** Buy business → hire manager (automate) → angel reset.

**What works:**
- Prefab: `StartingCurrency: 25`, `GeneratorRequiresManager: 1`, `ManagerHireCost: 100`.
- Bootstrap attaches `BuyableGenerator` (RequiresManager) + `IdleManager`.
- Buy without manager correctly leaves `PassiveRate` at 0 (`auto = !RequiresManager || IsAutomated`).
- Hire spends cost, sets `IsAutomated`, raises `PassiveRate`.
- UI: Collect / Buy Business / Hire Manager / Angel Reset.
- Smoke test covers hire → PassiveRate > 0.

**Gaps / bugs:**
1. **Angel Reset breaks manager gate (P0).** Hire clears `RequiresManager = false` on the gen. `PrestigeSystem` resets `OwnedCount` / `IsHired` / `ManagersHired` but **does not restore `RequiresManager`**. After reset, the next Buy Business immediately automates CPS without re-hiring — kills the signature AdvCap beat.
2. **Hire with zero businesses grants phantom CPS (P1).** Hire uses `Max(1, OwnedCount)` when computing PassiveRate, so hiring with 0 owned businesses still yields `BaseCps * 1 * mult`.
3. **Economy friction is OK for MVP but brittle:** 25 start → one business (15) → Collect grind (~90 clicks at 1$/click) → hire at 100. Playable once; after broken reset (above), loop collapses.
4. **SliceDef `ClickPower = 0` is coerced to 1** in `IdleToolkitSliceGenerator` (`ClickPower > 0 ? … : 1`), matching prefab. Collect path also uses `Max(1, OwnedGenerators)` — intentional manual labor before manager, fine.
5. **Angel Reset uses Cookie-style double prestige path** (`FirePrestige` → phase + prestige), same double-count risk as Cookie.

### 3) Universal Paperclips (`IdleArchetype.UniversalPaperclips` / prefab 04)

**Intended loop (matrix):** Manufacture → autoclippers → **phase-shift replaces the loop**; compute allocation.

**What works:**
- Same CPS buy/click shell as Cookie (click power 1, gen cost 15, automated).
- UI: Make Paperclip / Buy Autoclipper / **Phase Shift only** (`FirePhase` — does *not* call `FirePrestige`). Better than Cookie’s dual fire.
- Smoke: phase with 100 currency → prestige > 0, primary 0, mult > 1.

**Gaps / bugs (stub shell vs matrix):**
1. **No loop replacement.** Phase shift only resets currency/gens/passive and bumps `PhaseIndex` / ClickPower / GlobalMultiplier. Post-phase verbs and systems are identical — not a new loop.
2. **No compute allocation** (matrix automation pattern). No energy/compute split, no wire/price market, no trust/drone phases.
3. **Same √ prestige formula as Antimatter/Cookie phase**, with soft floor `PrimaryCurrency >= 25` allowing prestige with converted=0 → forced +1. Fine for smoke; not “phase” design.
4. **Same double-`GlobalMultiplier` CPS bug** as Cookie.

---

## Shared / cross-cutting issues

### Math / numerics (critical-lane)

| ID | Issue | Where | Impact |
|----|--------|-------|--------|
| M1 | `GlobalMultiplier` baked into `PassiveRate` **and** reapplied in sim | `IdleBuyGeneratorSystem.Purchase`, `IdleManagerHireSystem`, `IdleSliceSimulationSystem` | CPS scales as mult²; prestige mult feels broken |
| M2 | Dual prestige converters on Cookie/AdvCap Prestige | `IdleSliceUIController.FirePrestige`, `IdlePhaseShiftSystem`, `PrestigeSystem` | Double (or +1 forced) prestige; order-dependent |
| M3 | `PrestigeSystem` always `converted = 1` minimum even at 0 currency | `PrestigeSystem` ~L50–52 | Prestige with empty run still pays |
| M4 | AdvCap hire `Max(1, OwnedCount)` | `IdleManagerHireSystem` | Free CPS without a business |
| M5 | Phase/prestige gen reset does not restore `RequiresManager` | Hire + Prestige/Phase | Automation survives angel reset |

### Persistence

| ID | Issue | Impact |
|----|--------|--------|
| P1 | Save stores `OwnedGenerators` / `PassiveRate` but **not** `BuyableGenerator.OwnedCount`, `IsAutomated`, `RequiresManager`, `IdleManager.IsHired`, `ManagersHired`, `PhaseIndex` | Reload: HUD gens vs buy cost desync (cost restarts at BaseCost while gens count high); AdvCap manager state lost; soft exploit |
| P2 | `IdleSaveManager` only mirrors time + `PersistNow`; bootstrap also saves every 2s | Redundant, OK; does not fix P1 |
| P3 | Bootstrap `OnDestroy` has `// TODO: [STUB] entity cleanup` | Domain reload / leave play leaks slice entities |

### Event / ECS hygiene

| ID | Issue | Impact |
|----|--------|--------|
| E1 | UI `Fire*` creates a **new entity per click** and only disables the enableable component | Event entity leak during play |
| E2 | Click/Buy/Hire/Phase queries are **world-global** (all `IdleSliceState` / all gens) | Multi-slice scenes cross-contaminate; phase zeros every generator in the world |
| E3 | Runtime `PanelSettings` creation marked `// TODO: [STUB]` | Prefabs already assign PanelSettings asset — stub is fallback only |

### Tests vs playability

- Covered: Cookie click+buy, AdvCap hire, Paperclips phase, Antimatter buy, PlayerPrefs round-trip.
- **Not covered:** sim CPS tick, double-prestige, AdvCap buy-without-manager PassiveRate==0, AdvCap post-prestige RequiresManager restore, persistence OwnedCount sync, Play Mode.
- Progress doc: **Play Mode Pending** for all three.

---

## Stub shells (greppable / structural)

| Marker / shell | Location | Note |
|----------------|----------|------|
| `// TODO: [STUB] runtime PanelSettings` | `IdleSliceUIController` | Fallback only |
| `// TODO: [STUB] entity cleanup` | `IdleSliceBootstrap.OnDestroy` | No destroy of `_sliceEntity` |
| Single-gen CPS shell shared by Cookie / Paperclips / Antimatter / Egg / Miner / Realm | `IdleSliceBootstrap.AttachArchetypeExtras` | Archetype enum differs; mechanics nearly identical |
| Paperclips “phase” = prestige reset clone | `IdlePhaseShiftSystem` | No loop replacement / compute |
| Generic HUD stats line | `RefreshStats` | Shows Workers/HP/Cats for CPS titles (noise, not blocking) |

---

## Implementer fix list (priority order)

Do **not** expand scope into full Cookie/Paperclips fidelity unless explicitly requested. Fix correctness of the existing MVP beats first.

### P0 — must fix before claiming playable

1. **Single-apply `GlobalMultiplier` for CPS.** Pick one contract:
   - Preferred: `PassiveRate = BaseCps * OwnedCount` (raw), sim does `+= PassiveRate * GlobalMultiplier * dt`; **or**
   - Bake mult into PassiveRate and sim uses `+= PassiveRate * dt` only.  
   Apply consistently in Purchase, Hire, and any reload path that rebuilds PassiveRate.
2. **Prestige button routing.** Cookie / AdvCap: either `FirePhase` **or** enable prestige on the slice entity — not both. Align with Paperclips (`FirePhase` only) **or** make Prestige the only path and stop creating `IdlePhaseShiftEvent` from `FirePrestige`.
3. **`PrestigeSystem` idle branch:** remove unconditional `converted = 1` when currency is below threshold; share one helper with `IdlePhaseShiftSystem` (same floor policy).
4. **AdvCap manager gate across reset:** on prestige/phase for manager archetypes, restore `RequiresManager = true`, `IsAutomated = false`, `IdleManager.IsHired = false`, `ManagersHired = 0`, `PassiveRate = 0`.
5. **Hire PassiveRate:** use actual `OwnedCount` (allow 0 → PassiveRate 0), never `Max(1, OwnedCount)`.

### P1 — playability / persistence

6. On load, sync `BuyableGenerator.OwnedCount` (and automation/manager flags) from saved state, or stop saving `OwnedGenerators` without the component fields.
7. Persist `ManagersHired` / `IsHired` / `RequiresManager` / `PhaseIndex` (or rebuild PassiveRate from components after load).
8. Destroy or recycle UI event entities after handling (ECB destroy), scoped to the bootstrap’s slice entity if possible.
9. Add EditMode tests:
   - Cookie: buy → advance time via sim system → currency increases by expected CPS (assert **not** mult²).
   - Cookie/AdvCap: one prestige → prestige delta equals single conversion formula.
   - AdvCap: buy without hire → PassiveRate == 0; hire → PassiveRate > 0; prestige → RequiresManager true again; buy without rehire → PassiveRate == 0.
   - Paperclips: phase does not invoke PrestigeSystem path.

### P2 — archetype identity (deferred unless product asks)

10. Cookie: 2–3 named generator rows (Cursor/Grandma-style) still using same buy math.
11. Paperclips: minimal “phase replaces loop” — e.g. PhaseIndex≥1 swaps UI verb set and/or production formula (compute alloc stub is enough for MVP identity).
12. Scope event systems to owning slice (tag event with archetype or Entity target) before multi-slice demos.
13. Human Play-Smoke in Unity on this project; update `docs/idle-toolkit-progress.md` Play column only after evidence.

---

## Acceptance criteria for implementers (machine-checkable)

| # | Criterion |
|---|-----------|
| A1 | With `GlobalMultiplier = 2`, one gen `BaseCps = 1`, OwnedCount = 1: over 1.0s sim dt, primary increases by **2 ± epsilon**, not ~4. |
| A2 | Cookie Prestige from currency C grants prestige equal to **one** conversion (`floor(sqrt(C/50))` with documented floor), not ≥ conversion+1. |
| A3 | AdvCap: after Angel Reset, `BuyableGenerator.RequiresManager == true` and buying without hire leaves `PassiveRate == 0`. |
| A4 | AdvCap hire with `OwnedCount == 0` leaves `PassiveRate == 0`. |
| A5 | Reload after owning N gens: next buy cost uses `BaseCost * growth^N` (OwnedCount synced). |
| A6 | Existing `IdleBatchASmokeTests` still pass; new tests for A1–A4 added. |
| A7 | Play Mode: drop prefabs 01, 03, 04 → core verb + one progression beat observed (human or MCP capture). |

---

## Evidence notes

- Prefabs 01/03/04 present with correct archetypes (0 / 2 / 3) and PanelSettings assigned.
- `Logs/IdleBatchA-Summary.txt` / progress doc claim EditMode pass; **no Play Mode verification** in-repo for these slices.
- This review did **not** run Unity or modify gameplay code.

---

## Out of scope (intentionally not expanded)

- Clicker Heroes / Antimatter (Batch A neighbors) except where shared systems cause CPS/prestige bugs above.
- Full Cookie Clicker / Paperclips feature parity.
- Offline earnings for these three titles.
