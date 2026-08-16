Idle MVP slices (matrix → archetypes)
See docs/idle-toolkit-progress.md for EditMode vs Play status.
Compose / piece catalog: docs/idle-toolkit-compose.md

STATUS (do not confuse these):
  Fixture presence:  19/19  (named smoke per matrix title; IdleBatchA + IdleBatchBC + Kernel + Cosmetics + PrestigeFaction in AllSmoke)
  EditMode NUnit:    see tip Logs/IdleAllSmoke-Summary.txt (authoritative count; not verb closure)
  Matrix core verbs: ~17/19 OK — Antimatter + AFK Arena PARTIAL (see docs/idle-toolkit-progress.md)
  Play Mode:         5/19   (Batch A 01/02/03/04/06 Playable 2026-08-16 on Hyper-Casual-Runner; B/C unchecked)
  Note: AllSmoke green ≠ full verb closure ≠ Play Mode.

MENUS (do not mix these up):
  IdleToolkit/MVP/…            → 19 matrix idle MVPs (this folder)
  IdleToolkit/RunnerSlices/…   → 21 hyper-casual runner slices (Assets/ToolkitExamples/*.prefab)

HUMAN PLAY-SMOKE (Unity menu: IdleToolkit/MVP/Human Play-Smoke Instructions)
  Writes durable checklist: docs/idle-play-smoke-checklist.md
1. Open THIS project in Unity (not another toolkit repo)
2. IdleToolkit/MVP/Open Idle Prefab Folder
3. Drop Assets/ToolkitExamples/Idle/01_CookieClicker_Generators_Slice.prefab into a scene
4. Play → use HUD: core verb + one progression beat (e.g. Click → Buy → Prestige)
5. Repeat Batch A: 02, 03, 04, 06 then other slices as needed
6. Mark Playable in docs/idle-toolkit-progress.md ONLY after Play evidence

EDITMODE (no Play Mode):
  Unity menu IdleToolkit/MVP/Run Batch A Smoke And Exit
  or batchmode -executeMethod HyperCasualRunner.Editor.IdleBatchATestRunner.RunAllIdleSmokeAndExit
  (do NOT pass -quit; the runner exits itself)

GENERATE PREFABS:
  IdleToolkit/MVP/Generate All Idle MVP Slices

COMPOSITION CONTRACT (honest):
  New prototype without a new IdleArchetype enum = unsupported.
  Do not use ProducerAuthoring / ArcadeIdleAuthoring / ResourceWallet for M6 MVPs.
  Clone + retune nearest prefab for low-novelty experiments.
  Full map: docs/idle-toolkit-compose.md

ARCHETYPE MAP
01 Cookie Clicker          CookieClicker
02 Clicker Heroes          ClickerHeroes
03 AdVenture Capitalist    AdventureCapitalist
04 Universal Paperclips    UniversalPaperclips
05 A Dark Room             ADarkRoom
06 Antimatter Dimensions   AntimatterDimensions
07 Realm Grinder           RealmGrinder
08 NGU Idle                NguIdle
09 Melvor Idle             MelvorIdle
10 Egg, Inc.               EggInc
11 Idle Miner Tycoon       IdleMinerTycoon
12 Tap Titans 2            TapTitans2
13 Idle Heroes             IdleHeroes
14 AFK Arena               AfkArena
15 Legend of Mushroom      LegendOfMushroom
16 Capybara Go!            CapybaraGo
17 Cats & Soup             CatsAndSoup
18 Neko Atsume             NekoAtsume
19 Fallout Shelter         FalloutShelter
