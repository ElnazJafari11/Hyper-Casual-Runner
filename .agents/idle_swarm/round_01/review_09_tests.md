# Round 01 Review 09 — IdleToolkit EditMode Tests

**Agent:** examine/analyze/review 9/10  
**Scope:** `Assets/Scripts/Editor/Tests/` IdleToolkit smoke fixtures vs `docs/idle_mechanics_matrix.md` core verbs  
**Mode:** review only — no implementation  
**Verdict:** Progress claim **“EditMode-verified core verb + beat: 19/19” is overstated.** Fixtures exist 1:1 per matrix title and mostly green, but many pass without exercising the matrix core verb, without a real progression beat, or with seeded/forced state that skips the causal chain.

---

## Inventory

| Fixture | Path | Count |
|---------|------|-------|
| Batch A | `Assets/Scripts/Editor/Tests/IdleBatchASmokeTests.cs` | 5 game tests + 1 `GameProgressData` round-trip = 6 |
| Batch B+C | `Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs` | 14 game tests |
| Related (not matrix smoke) | `CosmeticsShopTests.cs`, `ChallengerM1M2M3StressTests.cs` prestige cosmetics | runner-economy, not IdleArchetype matrix |
| Prefab presence | `ToolkitGeneratorTests.MyMiniMart_ContainsArcadeIdleNodes` | ArcadeIdle nodes, not IdleToolkit slices |

**Absent from EditMode tests entirely:**
- `IdleSliceSimulationSystem` (passive CPS, AFK tick, Melvor skill timer, NGU energy spend, assignment output, hero DPS tick)
- `IdleProductionSystem` / `ProducerComponent` wallet path
- `PrestigeSystem` (IdleSliceState branch)
- `OfflineSimulationSystem`
- `IdleShopSystem`, `SynergySystem`
- `IdleCombatState` / `IdleSkillNode` component paths
- Prefab/authoring/bootstrap wiring (`IdleSliceBootstrap`, UI event → system)
- Multi-slice isolation (most action systems query **all** `IdleSliceState`)

**Evidence hygiene:** `Logs/IdleBatchA-Summary.txt` / `IdleBatchA-TestResults.xml` currently report `pass=14` (Batch BC shape), so Batch A “6/6” in `docs/idle-toolkit-progress.md` is not independently confirmed by those log filenames.

---

## Matrix core-verb scorecard

Legend: **OK** = verb + causal beat roughly present · **PARTIAL** = related verb or half-chain · **MISS** = false green / wrong verb · **SEED** = outcome pre-forced

| # | Game | Matrix core verb | Test | Score | Primary issue |
|---|------|------------------|------|-------|---------------|
| 1 | Cookie Clicker | Click → Buy Generators | `CookieClicker_ClickThenBuy_…` | PARTIAL | Click proven; buy funded by **forced** `PrimaryCurrency = 100`, not click earnings. No spend/cost assert. No CPS tick. |
| 2 | Clicker Heroes | Tap to Kill → Buy Heroes | `ClickerHeroes_TapKill_…` | PARTIAL | Tap-kill OK. **Buy Heroes never tested.** |
| 3 | AdVenture Cap. | Buy Businesses (+ managers) | `AdventureCapitalist_HireManager_…` | MISS/SEED | Core buy skipped; gen pre-owned; only hire. No hire cost deduction assert. |
| 4 | Univ. Paperclips | Manufacture → Phase-Shift | `Paperclips_PhaseShift_…` | MISS | Manufacture never run; prestige-only. |
| 5 | A Dark Room | Stoke → Explore | `ADarkRoom_StokeThenExplore_…` | OK | Strongest narrative chain. Missing craft (action 2) / wood asserts. |
| 6 | Antimatter Dim. | Buy Dimensions (+ nested layers) | `Antimatter_BuyDimension_…` | PARTIAL | Buy identical to Cookie path. No layer/eternity prestige. No spend assert. |
| 7 | Realm Grinder | Build & Align | `RealmGrinder_AlignFaction_…` | PARTIAL | Align only via energy event. **Build never tested.** Faction 2 / re-pick untested. |
| 8 | NGU Idle | Allocate Energy | `NguIdle_AllocateEnergy_…` | PARTIAL | Sets `EnergyAllocated` only. No `TickEnergy` production/XP. `ProgressionLevel=1` is free on any alloc > 0. |
| 9 | Melvor Idle | Grind Skills | `MelvorIdle_TrainSkillClick_…` | **MISS** | Explicitly uses **click** because “DeltaTime can be 0”. Wrong verb; no `IdleSkillNode` / sim tick. |
| 10 | Egg, Inc. | Hatch (Tap Burst) | `EggInc_HatchBurst_…` | OK | Hatch path + PassiveRate. Prestige (Soul Eggs) untested. |
| 11 | Idle Miner | Upgrade Shafts (+ managers) | `IdleMiner_HireManager_…` | MISS/SEED | Same as AdvCap: hire only, shaft buy skipped. |
| 12 | Tap Titans 2 | Tap / Hero DPS | `TapTitans2_TapKill_…` | PARTIAL | Tap-kill level only; **no gold assert**; no hero DPS / buy. |
| 13 | Idle Heroes | Auto-Combat | `IdleHeroes_GachaPull_…` | **MISS** | Tests gacha (monetization), not auto-combat / `TickHeroDps`. |
| 14 | AFK Arena | Auto-Combat (+ AFK chest) | `AfkArena_ClaimChest_…` | SEED | Chest seconds + claim flag **pre-seeded**; sim never fills chest. Auto-combat untested. |
| 15 | Legend of Mushroom | Rub Lamp | `LegendOfMushroom_RubLamp_…` | SEED | `PullCount=2` pre-set so one pull hits `% 3 == 0`. No spend/rarity asserts. |
| 16 | Capybara Go! | Step-based Narrative | `CapybaraGo_NextStep_…` | SEED | `ExploreUnlocked=1` pre-set; unlock chain skipped. |
| 17 | Cats & Soup | Assign Cats | `CatsAndSoup_AssignCat_…` | PARTIAL | Assign OK; **no station/sim output**. |
| 18 | Neko Atsume | Place Food/Toys | `NekoAtsume_PlaceFood_…` | PARTIAL | Slice-only narrative path; no spend assert; no check-in/claim/timer. Prefab+`IdleNarrativeState` would take different branch. |
| 19 | Fallout Shelter | Assign Dwellers | `FalloutShelter_AssignDweller_…` | PARTIAL | Assign OK; no production; station `AssignedCount` not asserted. |

**Rough tally:** OK ≈ 2 · PARTIAL ≈ 10 · MISS/SEED ≈ 7. Not 19/19 core-verb verification.

---

## False greens (high severity)

### 1. MelvorIdle — wrong verb, still green
`MelvorIdle_TrainSkillClick_GainsCurrency` drives `IdleClickProduceSystem` (default cookie-like grant). Matrix verb is grind skills / offline. Comment admits EditMode `DeltaTime` limitation instead of injecting time / calling `IdleSliceSimulationSystem` with controlled `dt` / advancing `IdleSkillNode`.

### 2. IdleHeroes — monetization standing in for core verb
Matrix: Auto-Combat. Test: `IdleGachaPullSystem` raises `ClickPower`. Never damages an enemy or advances zone via DPS.

### 3. Cookie buy affordability is injected
After clicks, test sets `PrimaryCurrency = 100` then buys. Passes even if click income were broken below cost. No assert that currency decreased by generator cost; no `BuyableGenerator.OwnedCount` check (only slice `OwnedGenerators`).

### 4. AdvCap / IdleMiner — automation without earned buy
Both pre-set `OwnedCount = 1` and only hire. Matrix stories are buy-business / upgrade-shaft then graduate to managers. PassiveRate increase proves hire math, not the loop.

### 5. Paperclips — prestige without manufacture
`IdlePhaseShiftSystem` alone with seeded currency. Core manufacture verb uncovered. Also: phase system has **no archetype filter** — would mutate every slice in a multi-slice world (untested).

### 6. AFK Arena — claim without accrual
`AfkChestSeconds` / `HasOfflineClaim` planted. `IdleSliceSimulationSystem` AfkArena branch never run. Worse: `IdleClaimOfflineSystem` still grants ≥10 currency even when claim flags are empty (demo fallback) — a zero-setup claim can green.

### 7. NGU “progression beat” is free
Any `Amount > 0` sets `ProgressionLevel = 1` without energy spend/XP thresholds from `TickEnergy`.

### 8. Legend of Mushroom stage advance is pre-aligned
`PullCount = 2` guarantees next pull trips `PullCount % 3 == 0`. Does not prove first pulls or cost gate.

### 9. Progress doc false confidence
`docs/idle-toolkit-progress.md` equates “fixture named after game exists and NUnit passed” with “core verb + beat verified.” Several greens do not encode matrix verbs.

---

## Missing assertions (even where verb is right)

Cross-cutting gaps in otherwise useful tests:

| Missing check | Where it hurts |
|---------------|----------------|
| Currency **decreased** by cost | Cookie buy, Antimatter buy, gacha pulls, Neko food (spend 5), manager hire |
| Component mirrors (`BuyableGenerator.OwnedCount`, `IsAutomated`, station counts) | Cookie, Fallout (station not asserted), hire paths |
| Enemy respawn / HP reset after kill | Clicker Heroes, Tap Titans |
| Gold on kill | Tap Titans (CH asserts gold; TT2 does not) |
| Passive **tick** after CPS unlock | Cookie, Egg, managers — `PassiveRate > 0` without proving income over time |
| Archetype-specific branch | Antimatter indistinguishable from Cookie buy |
| Persistence isolation | `GameProgressData` only Cookie id; no cross-archetype overwrite / clear hygiene for other ids |
| Event disable / one-shot | Events disabled after update; never asserted |
| Multi-entity queries | Systems foreach all slices; single-slice worlds hide crosstalk |

---

## System coverage holes (IdleToolkit-critical)

| System / path | Covered by smoke? | Risk |
|---------------|-------------------|------|
| `IdleClickProduceSystem` | Yes (several) | Strongest covered surface |
| `IdleBuyGeneratorSystem` | Cookie + Antimatter only | Combat hero fallback / CH–TT2–IH boost untested |
| `IdleManagerHireSystem` | AdvCap + Miner | Cost fail path untested |
| `IdleNarrativeActionSystem` | ADR, Capybara, Neko | Craft action 2 unused; Neko dual code paths |
| `IdleAllocateEnergySystem` | Realm + NGU | Faction 2 / clamp edges thin |
| `IdleGachaPullSystem` | Idle Heroes + LoM | Fallback Without`IdleGachaState` untested |
| `IdleAssignWorkerSystem` | Cats + Fallout | Capacity clamp / negative delta untested |
| `IdleClaimOfflineSystem` | AFK only | Fallback grant masks empty state |
| `IdlePhaseShiftSystem` | Paperclips | Antimatter nested layers unused |
| `IdleSliceSimulationSystem` | **None** | Entire passive pillar unverified in EditMode |
| `PrestigeSystem` IdleSlice branch | **None** | Parallel prestige path to PhaseShift |
| Prefab ↔ ECS bootstrap | **None** | Generator/UI may diverge from smoke entity recipes |

---

## Fixture hygiene issues

1. **Batch A** clears only `IdleArchetype.CookieClicker` PlayerPrefs; Batch BC clears none.
2. **Batch BC** `CreateSlice` omits `CurrentRunStats` (Batch A adds it) — gold mirror path inconsistently exercised.
3. **World time:** No helper to set `World.Time` / pump multiple sim frames — drives Melvor/AFK/NGU/assign production avoidance.
4. **Naming vs behavior:** `…TrainSkillClick…`, `…GachaPull…` for auto-combat titles advertise different verbs than the matrix.
5. **Log filenames:** Batch A summary files currently look like Batch BC results — CI/human evidence for “6/6” is unreliable.

---

## Recommended acceptance criteria (for implement round — do not do now)

Machine-checkable upgrades, prioritized:

1. **Per-title verb table:** each of 19 tests must call the system(s) matching matrix Core Verb column; rename or split tests that currently substitute (Melvor, Idle Heroes, Paperclips manufacture, AdvCap/Miner buy).
2. **No seeded skip of causal chain:** forbid pre-setting the asserted outcome field except as documented arrange for a *prior* step that the same test also performs (e.g. stoke then explore is fine; pre-`ExploreUnlocked=1` alone is not).
3. **Spend asserts:** every buy/hire/pull/food action asserts `PrimaryCurrency` delta equals expected cost (±epsilon).
4. **At least one `IdleSliceSimulationSystem` beat** for titles whose fantasy is passive/AFK/skill/assign/DPS (Cookie CPS, Melvor skill, NGU energy, Cats/Fallout output, AFK chest fill, Idle Heroes DPS) — inject non-zero `dt` or equivalent.
5. **Tap Titans parity with Clicker Heroes:** assert gold + level after kill.
6. **Persistence:** round-trip two archetypes; prove no key collision; clear in TearDown for all used ids.
7. **Evidence:** separate Batch A vs BC summary artifacts; progress doc must not claim core-verb coverage for MISS/SEED rows until fixed.

---

## Bottom line

EditMode smoke is a useful **wiring check** for event → system → a few fields, and Batch A/B+C naming coverage is complete. It is **not** a faithful matrix verb suite. Highest-value holes: **Melvor click substitute**, **Idle Heroes gacha-for-combat**, **zero `IdleSliceSimulationSystem` coverage**, **injected affordability / pre-owned gens / pre-filled chests**, and **missing cost/spend assertions**. Treat “19/19 EditMode-verified” as **fixture presence + smoke green**, not core-verb proof, until the criteria above land.
