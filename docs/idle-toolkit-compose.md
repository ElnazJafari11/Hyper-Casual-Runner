# Idle Toolkit — Piece Catalog & Compose Contract (M6)

LLM / agent entrypoint for **matrix idle MVPs** under `Assets/ToolkitExamples/Idle/`.

## Non-negotiables

1. **M6 path = slice kernel**, not the arcade/producer stack.
   - Use: `IdleSliceBootstrap` + `IdleSliceState` + events in `IdleSliceComponents.cs` + `IdleSliceUIController`.
   - Do **not** use for M6 MVPs: `ProducerAuthoring`, `ArcadeIdleAuthoring`, `ResourceWallet` / `ProducerComponent` / `IdleProductionSystem` (legacy runner-arcade idle). Bootstrap may add an empty wallet buffer; slice UI ignores it.
2. **New prototype without a new `IdleArchetype` = unsupported.**
   - There is no piece-flag / `GenerateCustomIdleSlice` API yet.
   - Fastest safe path: clone nearest of the 19 prefabs and retune inspector numbers / `HowToPlay`.
   - Novel loop → enum fan-out (see Authoring loop below). Mixing generators + assignment + gacha on one slice without a new enum is unsupported.
3. **Menus**
   - Idle MVPs: `IdleToolkit/MVP/…`
   - 21 runner hyper-casual slices: `IdleToolkit/RunnerSlices/…`
4. **Quality bar**: one core verb + one progression beat. Matrix “Why Loved” pillars are design intent; MVPs often collapse to renamed click/buy/claim. Do not overbuild past smoke.

## Kernel pieces

| Piece | Components | Events (enableable / spawn) | Systems | Typical UI verbs |
|-------|------------|-----------------------------|---------|------------------|
| Core slice | `IdleSliceState`, `IdleSliceTag` | — | `IdleSliceSimulationSystem` | HUD stats |
| Click / tap | (state fields) | `IdleClickEvent` | `IdleClickProduceSystem` | Click / Tap / Hatch / Collect |
| Generators | `BuyableGenerator` | `IdleBuyGeneratorEvent` | `IdleBuyGeneratorSystem` | Buy Generator / Business / Dimension |
| Managers | `IdleManager` | `IdleHireManagerEvent` | (hire path in action systems) | Hire Manager |
| Prestige | `PrestigeEventComponent` | same | `PrestigeSystem` | Prestige / Angel Reset / Rebirth |
| Phase shift | — | `IdlePhaseShiftEvent` | `IdlePhaseShiftSystem` | Phase Shift / Prestige Layer |
| Combat | `IdleCombatState` | click + buy | sim + click | Tap / Attack, Buy Hero DPS |
| Assignment | `IdleAssignmentStation` | `IdleAssignWorkerEvent` | `IdleAssignWorkerSystem` | Assign / Unassign |
| Skills | `IdleSkillNode` | click / claim | sim + click | Train Skill, Claim Offline |
| Narrative | `IdleNarrativeState` | `IdleNarrativeActionEvent` | narrative action system | Stoke / Explore / Craft / Next Step |
| Gacha | `IdleGachaState` | `IdleGachaPullEvent` | `IdleGachaPullSystem` | Gacha Pull / Rub Lamp |
| Energy | (state `EnergyPool`) | `IdleAllocateEnergyEvent` | allocate system | Allocate Energy |
| Offline / AFK claim | (state flags / `PendingClaim`) | `IdleClaimOfflineEvent` | claim system | Claim Offline / Open Chest / Check In |

Authoring / UI / persist: `IdleSliceBootstrap`, `IdleSliceUIController`, `IdleSaveManager` + `GameProgressData` idle keys.

## Matrix → code map (19)

| # | Game | Archetype | Extra components | UI verbs (HUD) | Smoke test |
|---|------|-----------|------------------|----------------|------------|
| 01 | Cookie Clicker | `CookieClicker` | `BuyableGenerator` | Click Cookie, Buy Generator, Prestige | `IdleBatchASmokeTests.CookieClicker_ClickThenBuy_RaisesCurrencyAndOwnedGens` |
| 02 | Clicker Heroes | `ClickerHeroes` | `IdleCombatState`, `BuyableGenerator` | Tap / Attack, Buy Hero DPS, Prestige | `IdleBatchASmokeTests.ClickerHeroes_TapKillThenBuyHero_AdvancesZoneAndDps` |
| 03 | AdVenture Capitalist | `AdventureCapitalist` | `BuyableGenerator`, `IdleManager` | Collect, Buy Business, Hire Manager, Angel Reset | `IdleBatchASmokeTests.AdventureCapitalist_BuyThenHireManager_AutomatesPassiveRate` |
| 04 | Universal Paperclips | `UniversalPaperclips` | `BuyableGenerator` | Make Paperclip, Buy Autoclipper, Phase Shift | `IdleBatchASmokeTests.Paperclips_ManufactureThenPhaseShift_ConvertsToPrestige` |
| 05 | A Dark Room | `ADarkRoom` | `IdleNarrativeState` | Stoke Fire, Explore, Craft | `IdleBatchBCSmokeTests.ADarkRoom_StokeThenExplore_AdvancesProgression` |
| 06 | Antimatter Dimensions | `AntimatterDimensions` | `BuyableGenerator` | Buy Dimension, Click Antimatter, Prestige Layer | `IdleBatchASmokeTests.Antimatter_BuyDimension_RaisesOwnedAndCps` |
| 07 | Realm Grinder | `RealmGrinder` | `BuyableGenerator` | Build, Align Good, Align Evil, Rebirth | `IdleBatchBCSmokeTests.RealmGrinder_AlignFaction_SetsFactionWithoutFreeMult` (+ `RealmGrinder_Rebirth_ResetsRun_KeepsFaction`) |
| 08 | NGU Idle | `NguIdle` | (energy on state) | Allocate Energy, Idle Tick Boost, Rebirth | `IdleBatchBCSmokeTests.NguIdle_AllocateEnergyThenTick_ProducesFromSpend` |
| 09 | Melvor Idle | `MelvorIdle` | `IdleSkillNode` | Train Skill, Claim Offline | `IdleBatchBCSmokeTests.MelvorIdle_GrindSkillNode_LevelsViaSimTick` |
| 10 | Egg, Inc. | `EggInc` | `BuyableGenerator` | Hatch Burst, Upgrade Habitat, Soul Prestige | `IdleBatchBCSmokeTests.EggInc_HatchBurst_RaisesCurrencyAndPassive` |
| 11 | Idle Miner Tycoon | `IdleMinerTycoon` | `BuyableGenerator`, `IdleManager` | Collect Shaft, Upgrade Shaft, Hire Super-Manager, New Mine | `IdleBatchBCSmokeTests.IdleMiner_BuyShaftThenHireManager_AutomatesShaft` |
| 12 | Tap Titans 2 | `TapTitans2` | `IdleCombatState`, `BuyableGenerator` | Tap / Attack, Buy Hero DPS, Prestige | `IdleBatchBCSmokeTests.TapTitans2_TapKill_AdvancesZoneAndGrantsGold` |
| 13 | Idle Heroes | `IdleHeroes` | `IdleCombatState`, `IdleGachaState` | Gacha Pull, Claim AFK | `IdleBatchBCSmokeTests.IdleHeroes_GachaPull_RaisesHeroDps` |
| 14 | AFK Arena | `AfkArena` | (AFK chest on state) | Campaign Progress, Open AFK Chest | `IdleBatchBCSmokeTests.AfkArena_SimFillsChestThenClaim_GrantsCurrency` |
| 15 | Legend of Mushroom | `LegendOfMushroom` | `IdleGachaState` | Rub Lamp, Farm Stage Gold | `IdleBatchBCSmokeTests.LegendOfMushroom_RubLampThreeTimes_SpendsAndAdvancesStage` |
| 16 | Capybara Go! | `CapybaraGo` | `IdleNarrativeState` | Take Step, Next Step, Lucky Find | `IdleBatchBCSmokeTests.CapybaraGo_StepsThenAdvance_RaisesLevel` |
| 17 | Cats & Soup | `CatsAndSoup` | `IdleAssignmentStation` | Assign Cat, Unassign (station cook → Primary) | `IdleBatchBCSmokeTests.CatsAndSoup_AssignCatThenSim_ProducesOutput` |
| 18 | Neko Atsume | `NekoAtsume` | (check-in on state) | Place Food, Place Toys, Check In (clears cats) | `IdleBatchBCSmokeTests.NekoAtsume_PlaceFood_SpendsAndAttractsCats` (+ PlaceToys / CheckIn_Clears*) |
| 19 | Fallout Shelter | `FalloutShelter` | `IdleAssignmentStation` | Assign Dweller, Unassign, Claim Production (PendingClaim drain) | `IdleBatchBCSmokeTests.FalloutShelter_AssignDwellerThenSim_FillsStationAndAccrues` |

Batch A EditMode = 01–04 + 06 (+ persist round-trip). Batch B/C = remaining 14. Persist smoke: `IdleBatchASmokeTests.GameProgressData_IdleSlice_RoundTripTwoArchetypes_NoKeyCollision`.
Note: Idle Heroes auto-combat is passive (`IdleCombatState` DPS) — no Auto Fight HUD button.

## Authoring loop (add a 20th matrix MVP)

1. Add `IdleArchetype` enum value.
2. Extend `IdleToolkitSliceGenerator.Slices[]`.
3. Extend `IdleSliceBootstrap.BuildInitialState` + `AttachArchetypeExtras`.
4. Extend `IdleSliceUIController.AddButtonsForArchetype` (+ stats if needed).
5. Extend sim / click / action systems switches as required.
6. Add EditMode smoke (Batch A or BC).
7. Update matrix, `docs/idle-toolkit-progress.md`, README archetype map, this catalog.
8. Menu: `IdleToolkit/MVP/Generate All Idle MVP Slices` → Play-Smoke checklist via `IdleToolkit/MVP/Human Play-Smoke Instructions`.

## Prefabs & smoke menus

| Action | Menu |
|--------|------|
| Regen 19 idle MVPs | `IdleToolkit/MVP/Generate All Idle MVP Slices` |
| Open idle prefab folder | `IdleToolkit/MVP/Open Idle Prefab Folder` |
| EditMode Batch A (exit) | `IdleToolkit/MVP/Run Batch A Smoke And Exit` |
| Human Play-Smoke (writes checklist) | `IdleToolkit/MVP/Human Play-Smoke Instructions` |
| 21 runner slices (not idle MVPs) | `IdleToolkit/RunnerSlices/Generate 21 Playable Slices` |

Status board: `docs/idle-toolkit-progress.md` (EditMode vs Play — keep Play honest until this project is play-smoked).  
Play checklist artifact: `docs/idle-play-smoke-checklist.md` (regenerated by the Human Play-Smoke menu).

## Matrix gap (candidate #20 — deferred)

All 19 matrix titles have MVP prefabs. Next expansion (do **not** implement until Play Mode Batch A is green on Hyper-Casual-Runner):

| Candidate | Core verb | Gap filled |
|-----------|-----------|------------|
| **Synergism** | Buy upgrades → nested prestige; achievements as buyables | Achievement-gated generators (completionism → power), distinct from Antimatter layers / Realm factions |

Details: `docs/idle_mechanics_matrix.md` § Matrix coverage gap.
