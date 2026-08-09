Idle MVP slices (matrix → archetypes)
See docs/idle-toolkit-progress.md for EditMode vs Play status.

STATUS (do not confuse these):
  EditMode-verified: 19/19  (IdleBatchASmokeTests + IdleBatchBCSmokeTests)
  Play Mode:         0/19   (needs Hyper-Casual-Runner open in Unity — MCP may be on another project)

HUMAN PLAY-SMOKE (Unity menu: IdleToolkit/Human Play-Smoke Instructions)
1. Open D:\Git\Hyper-Casual-Runner in Unity 6000.5.5f1
2. IdleToolkit/Open Idle Prefab Folder
3. Drop Assets/ToolkitExamples/Idle/01_CookieClicker_Generators_Slice.prefab into a scene
4. Play → use HUD: core verb + one progression beat (e.g. Click → Buy → Prestige)
5. Repeat Batch A: 02, 03, 04, 06 then other slices as needed
6. Mark Playable in docs/idle-toolkit-progress.md

EDITMODE (no Play Mode):
  Unity menu IdleToolkit/Run Batch A Smoke And Exit
  or batchmode -executeMethod HyperCasualRunner.Editor.IdleBatchATestRunner.RunAllIdleSmokeAndExit
  (do NOT pass -quit; the runner exits itself)

GENERATE PREFABS:
  IdleToolkit/Generate All Idle MVP Slices

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
