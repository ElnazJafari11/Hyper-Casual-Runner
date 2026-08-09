# Round 02 Review 10/10 — LLM Compose / Docs / Menus (post–impl_10)

**Agent:** examine/analyze/review  
**From:** `round_01/impl_10_llm.md` + re-audit of live surfaces  
**Scope:** Re-audit LLM discovery, compose contract, menus, play-smoke artifact, doc honesty  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Date:** 2026-08-10  
**Implements:** none (review only). Never push.

---

## Verdict

**Round 01 P0 acceptance (C1–C5) largely landed.** An LLM now has a real compose entrypoint (`docs/idle-toolkit-compose.md`), menu routing that separates idle MVPs from runner slices, an honest “no enum → unsupported” contract, and a durable Play-Smoke checklist file. M6 wording no longer oversells a piece DSL.

**Still not a composable piece kit.** Novel prototypes remain an 8-touch `IdleArchetype` fan-out. Residual **doc drift** (stale menu path in progress, compose smoke-method names ≠ current NUnit names, a few HUD verb mismatches) will make agents fail “open the right menu / run the named test / press the labeled button” loops. Play Mode remains **0/19**.

**Bottom line:** LLM *orientation* is fixed; LLM *composition* and *doc↔code fidelity* are the Round 02 gaps.

---

## Assumptions (ledger)

| # | Assumption | Confidence | Verified by |
|---|------------|------------|-------------|
| 1 | impl_10 intended only review C1–C5 (catalog, menu split, play-smoke artifact, honesty) | high | `impl_10_llm.md` scope table |
| 2 | Piece-flag generator / component-presence UI remain deferred | high | compose Non-negotiable #2; impl_10 deferred list |
| 3 | Seed checklist on disk may differ from menu regenerator until menu is run | med | seed vs `WritePlaySmokeChecklist` body |
| 4 | Quality bar still = one core verb + one beat; Play column must stay Pending | high | compose + progress + PROJECT M6 |

---

## Round 01 C1–C5 re-score (post–impl_10)

| ID | Criterion | Round 01 claim | Round 02 finding |
|----|-----------|----------------|------------------|
| C1 | Doc: matrix game → components/events/UI verbs/smoke | MET | **MET with fidelity debt** — catalog exists; smoke column has stale method names (see §Fidelity) |
| C2 | Menus distinguish MVP idle vs 21 runner slices | MET | **MET** — Editor `[MenuItem]` only under `IdleToolkit/MVP/…` and `IdleToolkit/RunnerSlices/…` |
| C3 | Compose/README: new prototype without enum = unsupported | MET | **MET** — compose §Non-negotiables + README COMPOSITION CONTRACT |
| C4 | Human Play-Smoke → durable artifact | MET | **MET** — writes `docs/idle-play-smoke-checklist.md` + dialog + ping |
| C5 | Progress Play column stays honest | MET | **MET** — Play still 0/19 / UNVERIFIED; wording reinforced |

---

## What improved (do not regress)

1. **Compose catalog** — `docs/idle-toolkit-compose.md`: slice-vs-legacy stack warning, piece table, 19-row map, authoring fan-out steps, menu table.
2. **Menu namespace split** — idle generator under `MVP/`; runner generator under `RunnerSlices/` (no bare `IdleToolkit/<verb>` left in Editor scripts).
3. **Play-Smoke agent surface** — checklist markdown + `EditorUtility.DisplayDialog` (no longer Console-only).
4. **Honesty chain** — README, compose, PROJECT M6 (“enum-gated… not a piece-flag DSL”), checklist status all say Play 0/19.
5. **Matrix pointer** — `docs/idle_mechanics_matrix.md` links compose as code map; MVP bar called out.
6. **Batch A list fixed** in smoke instructions (01–04, 06; 05 called out as B/C).

---

## Surface inventory (current)

### A. Menus (Editor)

| MenuItem | File | LLM fitness |
|----------|------|-------------|
| `IdleToolkit/MVP/Generate All Idle MVP Slices` | `IdleToolkitSliceGenerator.cs` | Good bulk regen; still all-or-nothing |
| `IdleToolkit/MVP/Open Idle Prefab Folder` | same | Good |
| `IdleToolkit/MVP/Smoke Batch A (instructions)` | same | OK; points at compose |
| `IdleToolkit/MVP/Run Batch A Smoke And Exit` | same | Good agent/CI |
| `IdleToolkit/MVP/Human Play-Smoke Instructions` | same | Good — writes checklist + dialog |
| `IdleToolkit/RunnerSlices/Generate 21 Playable Slices` | `ToolkitExampleGenerator.cs` | Correctly namespaced |
| `IdleToolkit/RunnerSlices/Generate and Verify All` | same | OK |
| `IdleToolkit/RunnerSlices/Run Verification Suite` | same | OK |

### B. Docs

| Doc | Role | Round 02 note |
|-----|------|---------------|
| `docs/idle-toolkit-compose.md` | Primary LLM compose entry | Keep; fix fidelity (below) |
| `docs/idle-play-smoke-checklist.md` | Durable Play checklist (seed) | Usable offline; regenerator strings slightly ahead of seed |
| `Assets/ToolkitExamples/Idle/README_IDLE_SLICES.txt` | Operational cheat-sheet | Strong; menus + contract aligned |
| `docs/idle-toolkit-progress.md` | Evidence board | **Stale menu path** (below); Play honesty OK |
| `docs/idle_mechanics_matrix.md` | Design taxonomy | Pointer to compose OK |
| `PROJECT.md` M6 | Milestone | Accurate enum-gated wording |

### C. Still true (unchanged architecture)

- Behavior wired by `switch (IdleArchetype)` in bootstrap / UI / sim / actions — not piece flags.
- Dual stack still in tree (`ProducerAuthoring` / `ResourceWallet`); compose correctly warns off it.
- Generator = 19 fixed `Slices[]`; no single-slice / custom-piece API.
- MCP Play bridge still wrong project → Play 0/19.

---

## Fidelity debt (compose / checklist vs code)

These are Round 02 P0/P1 for *docs*, not runtime bugs. Agents that copy names from the compose map will miss tests or press wrong HUD labels.

### Smoke method names in compose (stale vs NUnit)

| Game | Compose cites | Actual primary smoke (approx.) |
|------|---------------|--------------------------------|
| Clicker Heroes | `ClickerHeroes_TapKill_AdvancesZone` | `ClickerHeroes_TapKillThenBuyHero_AdvancesZoneAndDps` |
| AdVenture Capitalist | `AdventureCapitalist_HireManager_AutomatesPassiveRate` | `AdventureCapitalist_BuyThenHireManager_AutomatesPassiveRate` |
| Paperclips | `Paperclips_PhaseShift_ConvertsToPrestige` | `Paperclips_ManufactureThenPhaseShift_ConvertsToPrestige` |
| Realm Grinder | `RealmGrinder_AlignFaction_RaisesMultAndLevel` | `RealmGrinder_BuildThenAlignFaction_RaisesMultAndLevel` |
| NGU Idle | `NguIdle_AllocateEnergy_SetsAllocation` | `NguIdle_AllocateEnergyThenTick_ProducesFromSpend` |
| Melvor Idle | `MelvorIdle_TrainSkillClick_GainsCurrency` | `MelvorIdle_GrindSkillNode_LevelsViaSimTick` (+ claim tests) |
| Idle Miner | `IdleMiner_HireManager_AutomatesShaft` | `IdleMiner_BuyShaftThenHireManager_AutomatesShaft` |
| Tap Titans 2 | `TapTitans2_TapKill_AdvancesZone` | `TapTitans2_TapKill_AdvancesZoneAndGrantsGold` |
| AFK Arena | `AfkArena_ClaimChest_GrantsCurrency` | `AfkArena_SimFillsChestThenClaim_GrantsCurrency` |
| Legend of Mushroom | `LegendOfMushroom_RubLamp_AdvancesStage` | `LegendOfMushroom_RubLampThreeTimes_SpendsAndAdvancesStage` |
| Capybara Go! | `CapybaraGo_NextStep_AdvancesLevel` | `CapybaraGo_StepsThenAdvance_RaisesLevel` |
| Cats & Soup | `CatsAndSoup_AssignCat_IncreasesWorkers` | `CatsAndSoup_AssignCatThenSim_ProducesOutput` |
| Fallout Shelter | `FalloutShelter_AssignDweller_FillsStation` | `FalloutShelter_AssignDwellerThenSim_FillsStationAndAccrues` |
| Persist | `GameProgressData_IdleSlice_RoundTrip` | `GameProgressData_IdleSlice_RoundTripTwoArchetypes_NoKeyCollision` |

Cookie / Antimatter / A Dark Room / Egg Inc / Idle Heroes gacha / Neko place-food names are closer or exact.

### HUD verb mismatches

| Archetype | Compose / checklist | Live `IdleSliceUIController` |
|-----------|---------------------|------------------------------|
| Idle Heroes | Auto Fight → Gacha Pull → Claim AFK | **No Auto Fight button** — only `Gacha Pull`, `Claim AFK` (auto-combat is passive) |
| AFK Arena | Push Campaign → Open AFK Chest | Button is **`Campaign Progress`**, not “Push Campaign” |
| Realm Grinder | Build → Align Good / Align Evil | Also has **`Rebirth`** (omitted from compose UI verbs) |
| Neko Atsume (seed checklist) | Place Food → Check In | UI also has **`Place Toys`**; menu regenerator already includes toys |

### Stale agent routing in progress doc

`docs/idle-toolkit-progress.md` Human Play-Smoke section still says:

> Unity menu: **IdleToolkit → Human Play-Smoke Instructions**

Post–impl_10 path is **`IdleToolkit → MVP → Human Play-Smoke Instructions`**. README/compose/checklist are correct; progress is the outlier that will send agents to a missing menu item.

### Seed checklist vs menu writer

Checked-in `docs/idle-play-smoke-checklist.md` lacks regenerator header stamps (`Generated` / Unity / Project) and differs on Neko / Fallout rows from `WritePlaySmokeChecklist`. Harmless if agents run the menu; confusing if they treat the seed as canonical forever.

### Historical handoffs (low priority)

`.agents/explorer_m*/handoff.md` still cite pre-split `IdleToolkit/Generate 21 Playable Slices`. Outside M6 LLM entrypath, but pollutes global search.

---

## Gaps ranked (Round 02)

### P0 — Truth / routing for LLM agents

1. **Fix compose smoke column** to exact current NUnit method names (or “class + grep prefix” rule) so agents can `-runTests` / open Test Runner by name.
2. **Align HUD verbs** in compose + checklist with `IdleSliceUIController` (Idle Heroes, AFK Arena, Realm Rebirth, Neko toys).
3. **Update `docs/idle-toolkit-progress.md` Play-Smoke menu** to `IdleToolkit/MVP/…`.

### P1 — Still blocks “compose from pieces”

4. **Archetype monolith unchanged** — novel idle = enum + generator + bootstrap + UI + systems + smoke + docs. Documented honestly; still the M6 composition ceiling.
5. **No Generate One / piece-flag API** — regenerating all can stomp prefab edits; hybrids without enum unsupported.
6. **Play Mode 0/19 + MCP wrong project** — checklist exists but cannot close the quality loop.
7. **Dual idle stacks remain** — compose warns; temptation path still in tree.

### P2 — Polish

8. Seed checklist sync / stamp when menu not yet run.
9. Per-slice LLM brief beyond HowTo (start currency, prestige gate) — still missing.
10. Hardcoded tunables in generator (`GeneratorBaseCost=15`, etc.) — undocumented as intentional MVP constants.
11. Old explorer handoffs with pre-`MVP/` / pre-`RunnerSlices/` paths.

---

## Ideal LLM journey (updated — what works *now*)

```text
1. Read docs/idle-toolkit-compose.md (contract + 19 map)
2. Use IdleToolkit/MVP/* only (never RunnerSlices for idle MVPs)
3. Clone nearest prefab OR Generate All → retune inspector / HowTo
4. EditMode: IdleToolkit/MVP/Run Batch A Smoke And Exit (or AllSmoke batchmode)
5. Play: run Human Play-Smoke Instructions → follow docs/idle-play-smoke-checklist.md
6. Mark Playable in progress ONLY after Hyper-Casual-Runner Play evidence
```

Novel / hybrid composition still requires the enum fan-out in compose §Authoring loop — correctly labeled unsupported without enum.

---

## Acceptance criteria for a future implementer (Round 02)

| ID | Criterion | Check |
|----|-----------|-------|
| R2-C1 | Every compose “Smoke test” cell matches an existing `public void` in Batch A/BC (exact string) | grep compose names → test files |
| R2-C2 | Every compose “UI verbs” cell ⊆ labels in `IdleSliceUIController.AddButtonsForArchetype` for that archetype | side-by-side read |
| R2-C3 | `docs/idle-toolkit-progress.md` Play-Smoke menu path contains `MVP` | grep |
| R2-C4 | Seed checklist Neko/Fallout/IdleHeroes/AFK rows match UI + regenerator | diff |
| R2-C5 | Play column / Play Mode claim remains 0/19 or Pending until Play evidence | progress + checklist |
| R2-C6 (deferred) | Piece-flag / GenerateCustomIdleSlice — still out of scope unless product asks | — |

---

## Evidence citations (read paths)

- `docs/idle-toolkit-compose.md`
- `docs/idle-play-smoke-checklist.md`
- `docs/idle-toolkit-progress.md` (stale menu line ~67)
- `Assets/ToolkitExamples/Idle/README_IDLE_SLICES.txt`
- `Assets/Scripts/Editor/IdleToolkitSliceGenerator.cs` (MenuItems + `WritePlaySmokeChecklist`)
- `Assets/Scripts/Editor/ToolkitExampleGenerator.cs` (`RunnerSlices/` MenuItems)
- `Assets/Scripts/UI/IdleSliceUIController.cs` (`AddButtonsForArchetype`)
- `Assets/Scripts/Editor/Tests/IdleBatchASmokeTests.cs`, `IdleBatchBCSmokeTests.cs`
- `PROJECT.md` M6
- `.agents/idle_swarm/round_01/impl_10_llm.md`, `review_10_llm_toolkit.md`

---

## Status

```
TASK: Round 02 examine 10/10 — re-audit LLM compose/docs/menus after impl_10
ROUTE: Examine/Analyze/Review only
ASSUMPTIONS: 4 listed; seed-vs-regenerator marked med
CHANGES: wrote .agents/idle_swarm/round_02/review_10_llm.md only
VERIFICATION: MenuItem grep PASS (MVP vs RunnerSlices); C1–C5 re-scored from disk; fidelity mismatches listed from UI + NUnit reads
STATUS: VERIFIED as review artifact (no implementation; never push)
RISKS: smoke/UI names may shift again after sibling Round 02 impls; Play/MCP state may change
```

**Do not implement from this review in the examine phase.** Hand P0 fidelity fixes (R2-C1–C4) to implement swarm; keep composition DSL deferred unless product expands scope.
