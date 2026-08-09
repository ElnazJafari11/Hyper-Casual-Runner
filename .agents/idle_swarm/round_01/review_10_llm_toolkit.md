# Round 01 Review 10/10 — LLM-Facing Idle Toolkit Usability

**Agent:** examine/analyze/review  
**Scope:** How an LLM would discover, compose, smoke, and extend idle prototypes from toolkit pieces  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Date:** 2026-08-10  
**Implements:** none (review only)

---

## Verdict

**Usable as a clone-and-tweak MVP catalog; not yet a composable “piece kit” for fast high-quality new prototypes.**

An LLM can regenerate the 19 matrix prefabs, map titles → archetypes, and run EditMode smoke. It cannot cleanly compose a *new* idle from orthogonal pieces (generators + managers + gacha + assignment + narrative) without editing multiple hard-coded `switch (IdleArchetype)` sites. Docs describe *what games exist* and *verification status*, not *how to assemble*. Human Play-Smoke is a log dump, not an LLM-operable checklist. Dual menus under `IdleToolkit/` (21 runner slices vs 19 idle MVPs) create routing ambiguity.

**Bottom line for M6 “fast high-quality prototypes”:** EditMode coverage is strong (19/19); LLM composition ergonomics and Play Mode evidence are the primary gaps.

---

## Assumptions (ledger)

| # | Assumption | Confidence | Verified by |
|---|------------|------------|-------------|
| 1 | Primary LLM entrypoints are generator + progress/matrix/PROJECT + Play-Smoke menu | high | read those files |
| 2 | “Compose from pieces” means mix kernel components/events without new enum cases | med | inferred from M6 wording + component surface |
| 3 | Older `ProducerAuthoring` / `ResourceWallet` stack is parallel legacy, not the MVP path | high | slice path uses `IdleSliceState.PrimaryCurrency`; wallet buffer added but barely driven by slice UI |
| 4 | Quality bar = playable matrix MVP, not full game fidelity | high | `docs/project-context.md` + M6 scope |

---

## Surface inventory (what an LLM actually finds)

### A. Generator — `Assets/Scripts/Editor/IdleToolkitSliceGenerator.cs`

| Menu | Behavior | LLM fitness |
|------|----------|-------------|
| `IdleToolkit/Generate All Idle MVP Slices` | Writes 19 prefabs under `Assets/ToolkitExamples/Idle/` | Good for bulk regen; **no single-slice / custom-archetype / piece-flag API** |
| `IdleToolkit/Open Idle Prefab Folder` | Pings folder | Good |
| `IdleToolkit/Smoke Batch A (instructions)` | `Debug.Log` + opens Test Runner | Weak: instructions only; Batch A comment list is slightly messy vs README |
| `IdleToolkit/Run Batch A Smoke And Exit` | Batchmode runner | Good for CI/agent |
| `IdleToolkit/Human Play-Smoke Instructions` | `Debug.Log` + open folder | Weak for LLMs (see §Gaps) |

Prefab recipe is fixed: Plane + `IdleHub` with `IdleSliceBootstrap` + `UIDocument` + `IdleSliceUIController` + `IdleSaveManager`. Tunables mostly hard-coded (`GeneratorBaseCost=15`, `MaxWorkers=5`, `PullCost=10`). Archetype differentiation is almost entirely the enum + string `HowTo`, not data-driven piece sets.

### B. Docs

| Doc | Role today | Gap for LLM compose |
|-----|------------|---------------------|
| `docs/idle_mechanics_matrix.md` | Design taxonomy: Core Verb / Prestige / Automation / Fun Factor + 5 pillars | **No code map** (components, events, systems, UI verbs, smoke tests) |
| `docs/idle-toolkit-progress.md` | Evidence table + Play Pending + batchmode command | Status board, not authoring guide; Play 0/19 blocks “quality” claims |
| `PROJECT.md` M6 | One-line milestone: shared kernel + 19 archetypes | No acceptance recipe for “new prototype from pieces” |
| `Assets/ToolkitExamples/Idle/README_IDLE_SLICES.txt` | Best operational cheat-sheet (status split, smoke steps, archetype map) | Still clone-list, not composition grammar |

### C. Runtime kernel (actual “pieces”)

Present and useful, but **wired by archetype switches**, not by composition:

- **State / tags:** `IdleSliceState`, `IdleSliceTag`, `BuyableGenerator`, `IdleManager`, `IdleCombatState`, `IdleAssignmentStation`, `IdleSkillNode`, `IdleNarrativeState`, `IdleGachaState`
- **Events:** click / buy / hire / assign / gacha / narrative / energy / claim offline / phase
- **Systems:** `IdleSliceSimulationSystem`, `IdleClickProduceSystem`, `IdleBuyGeneratorSystem`, `IdleSliceActionSystems`, prestige/phase paths
- **Authoring:** `IdleSliceBootstrap` (spawn + extras + persist)
- **UI:** `IdleSliceUIController` builds buttons from `switch (arch)`
- **Persist:** `GameProgressData` idle keys via bootstrap / `IdleSaveManager`

Parallel older stack still in tree: `IdleComponents` (`ResourceWallet`, `ProducerComponent`), `IdleProductionSystem`, `ProducerAuthoring`, `ArcadeIdleAuthoring`, plus runner generator menus under the same `IdleToolkit/` prefix. An LLM can easily “compose” the wrong stack.

### D. PROJECT.md M6

```
M6 | Idle Mechanics Toolkit (matrix MVPs) | Shared idle kernel + 19 matrix-game archetypes ... | IN PROGRESS
```

Implies a **shared kernel** usable beyond the 19. Code delivers 19 enum-gated MVPs that *share* systems, not a documented piece DSL. Milestone text oversells composability relative to implementation shape.

### E. Human Play-Smoke

- Menu only logs to Console (no EditorUtility dialog, no markdown checklist write-back, no MCP-friendly assert).
- Hard-codes `D:\Git\Hyper-Casual-Runner` and Unity `6000.5.5f1`.
- Progress doc correctly states Play Mode **0/19** and MCP on wrong project (`thepcgtoolkit`).
- README correctly warns: EditMode ≠ Play Mode.

For an LLM agent: Play-Smoke is a **human handoff**, not an automated loop. That is fine if documented as such; it is a blocker for “high-quality” claims without a connected editor on this project.

---

## How an LLM would compose a *new* idle today (as-is path)

Intended happy path an agent might invent:

1. Read matrix → pick fun pillars / verbs.
2. Copy nearest prefab or re-run Generate All.
3. Change `IdleSliceBootstrap.Archetype` / DisplayName / HowTo in Inspector.
4. Play and smoke.

**What actually happens for a *novel* prototype (not one of the 19):**

1. Add `IdleArchetype` enum value.
2. Extend `IdleToolkitSliceGenerator.Slices[]`.
3. Extend `IdleSliceBootstrap.BuildInitialState` + `AttachArchetypeExtras`.
4. Extend `IdleSliceUIController.AddButtonsForArchetype` (+ maybe stats string).
5. Extend `IdleSliceSimulationSystem` and/or `IdleSliceActionSystems` / click / buy switches.
6. Add EditMode smoke case (Batch A or BC).
7. Update matrix, progress, README archetype map.
8. Regenerate prefab; hope PanelSettings resolves; Play-Smoke manually.

That is an **8-touch enum fan-out**, not “compose pieces.” Mixing e.g. generators + assignment + gacha on one slice without a new enum is unsupported by UI and poorly specified by bootstrap (extras are exclusive cases, not additive flags).

**Fastest LLM path that works today:** clone an existing archetype prefab and only retune inspector numbers / HowTo text → low novelty, high success rate.

---

## Gaps ranked for “fast high-quality prototypes”

### P0 — Blocks LLM composition / truthful quality

1. **No piece catalog or composition contract**  
   Matrix columns ≠ code modules. Missing machine-readable map:  
   `Piece → Components → Events → Systems → UI verbs → Smoke assert`.  
   Without it, LLMs guess from switches and invent wrong APIs.

2. **Archetype-monolith wiring**  
   Behavior lives in multi-file `switch (IdleArchetype)`. New games require enum + N switches. Quality bar “modular toolkit” is not reflected in authoring UX.

3. **Play Mode 0/19 + bridge mismatch**  
   Progress/README honest; still means “high-quality playable” is EditMode-proxied only. Human Play-Smoke cannot close the loop while MCP is on another project.

4. **Menu namespace collision**  
   Same `IdleToolkit/` hosts:
   - `Generate 21 Playable Slices` / verification (runner arcade slices)
   - `Generate All Idle MVP Slices` (matrix idle MVPs)  
   LLM agents routinely pick the wrong generator.

### P1 — Slows / confuses high-quality output

5. **Docs do not teach the authoring loop**  
   Need a short `docs/idle-toolkit-compose.md` (or M6 section): minimal entity recipe, event firing pattern, how UI discovers verbs, how to add smoke, when *not* to touch legacy Producer stack.

6. **Human Play-Smoke is not agent-shaped**  
   `Debug.Log` only; no structured steps file, no per-prefab expected HUD labels, no “mark Playable” automation hint beyond prose. Batch A smoke menu text ≠ precise Batch A set in places (README is clearer).

7. **Generator is all-or-nothing**  
   No `Generate One`, no override path, no “custom slice from piece flags” (e.g. `HasGenerators | HasManagers | HasGacha`). Regenerating all risks stomping local prefab edits.

8. **Dual idle stacks**  
   Slice `PrimaryCurrency` path vs `ResourceWallet`/`ProducerComponent` path. Bootstrap adds empty wallet buffer; slice UI/events ignore it. LLMs may “upgrade” prototypes into the dead path.

9. **Matrix ↔ code fidelity not documented**  
   Pillars (second axis, absence, earned automation) are design intent; MVPs often collapse to click/buy/claim with renamed buttons. An LLM optimizing for matrix “Why Loved” will overbuild relative to smoke bar unless the bar is explicit (“MVP = one core verb + one beat”).

### P2 — Polish / scalability

10. **UI is code-built only** (no UXML/USS per archetype) — fine for MVP; undocumented as intentional stub (`PanelSettings` runtime stub exists).
11. **Stats HUD is one mega-string** for all archetypes — noisy for play-smoke judgment.
12. **Hardcoded machine path / Unity version** in Play-Smoke log — brittle for other agents/machines.
13. **Save key = archetype int** — custom clones sharing an enum collide in PlayerPrefs.
14. **No “LLM brief” in prefab/README** per slice (expected buttons, currency start, prestige condition) beyond HowTo one-liner.

---

## What already works well (do not break)

- Clear folder of 19 named prefabs + archetype enum aligned 0..18 with matrix order.
- README status split (EditMode 19/19 vs Play 0/19) — rare honesty; keep.
- EditMode Batch A + B/C smoke as the real verification spine.
- Event-driven UI → systems pattern is the right kernel shape for future composition.
- Progress doc batchmode command (notes not to pass `-quit`) is agent-usable.

---

## Ideal LLM compose journey (target, not implemented)

```text
1. Read piece catalog (matrix row → required pieces)
2. Call GenerateCustomIdleSlice(name, pieces[], tunables)
   → prefab with bootstrap flags / additive components (no new enum required for experiments)
3. HUD auto-builds from present components (not archetype switch)
4. EditMode smoke template asserts “core verb delta” + “progression beat delta”
5. Optional Human/MCP Play-Smoke checklist updates progress Play column
```

Minimum viable step toward that (for Round 01 implementers — recommendation only):

1. Doc: piece ↔ code map + “do not use ArcadeIdle/Producer for M6 MVPs”.
2. Split menus: `IdleToolkit/MVP/…` vs `IdleToolkit/RunnerSlices/…` (or rename).
3. Data-driven button set OR component-presence UI builder for 1–2 hybrid experiments.
4. Play-Smoke: write/update a checklist file under `docs/` or `.agents/` instead of Console-only.
5. Keep enum MVPs for the 19; treat composition as additive flags for *new* prototypes.

---

## Acceptance criteria an implementer could use later (review-derived)

Machine-checkable suggestions (not executed this round):

| ID | Criterion |
|----|-----------|
| C1 | Doc exists mapping each matrix game → components/events/UI verbs/smoke test name |
| C2 | `IdleToolkit` menu labels distinguish MVP idle vs 21 runner slices (grep MenuItem) |
| C3 | README or compose doc states: new prototype without enum = unsupported OR documents flag API |
| C4 | Human Play-Smoke produces durable artifact (file or dialog content) listing Batch A steps + expected HUD verbs |
| C5 | Progress doc Play column remains honest until Hyper-Casual-Runner MCP/Play evidence exists |

---

## Evidence citations (read paths)

- `Assets/Scripts/Editor/IdleToolkitSliceGenerator.cs` — GenerateAll, HumanPlaySmokeInstructions
- `Assets/Scripts/Editor/ToolkitExampleGenerator.cs` — competing `IdleToolkit/Generate 21 Playable Slices`
- `Assets/Scripts/ECS/Authoring/IdleSliceBootstrap.cs` — archetype extras switch
- `Assets/Scripts/UI/IdleSliceUIController.cs` — UI switch + event fire helpers
- `Assets/Scripts/ECS/Components/IdleSliceComponents.cs` — enum + piece components/events
- `Assets/Scripts/ECS/Systems/Idle/IdleSliceSimulationSystem.cs` — archetype sim switch
- `docs/idle-toolkit-progress.md`, `docs/idle_mechanics_matrix.md`, `PROJECT.md` M6
- `Assets/ToolkitExamples/Idle/README_IDLE_SLICES.txt`

---

## Status

```
TASK: Round 01 review — LLM idle toolkit usability
ROUTE: Examine/Analyze/Review only
ASSUMPTIONS: 4 listed (1–4); composition-intent assumption marked med
CHANGES: wrote .agents/idle_swarm/round_01/review_10_llm_toolkit.md only
VERIFICATION: N/A (no code changes); findings grounded in file reads above
STATUS: VERIFIED as review artifact (analysis complete; no implementation)
RISKS: sibling Round 01 reviews may overlap; Play Mode / MCP state may change after this snapshot
```

**Do not implement from this review in Round 01 examine phase.** Hand to implement swarm with P0 items first.
