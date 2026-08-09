# Idle Toolkit Progress

Branch: `antigravity/toolkit-persist`  
Matrix: `docs/idle_mechanics_matrix.md` (19 titles + candidate #20 Synergism deferred)  
Compose: `docs/idle-toolkit-compose.md`

## Evidence (latest)

| Check | Result |
|-------|--------|
| Prefabs `Assets/ToolkitExamples/Idle/*` | 19/19 present |
| EditMode AllSmoke (`RunAllIdleSmokeAndExit`) | **PASS 46/46** — `Logs/IdleAllSmoke-Summary.txt` / `Logs/IdleKernel-impl07.log` |
| EditMode Batch BC | **PASS 29/29** — `Logs/IdleBatchBC-Summary.txt` |
| Play Mode (MCP Hyper-Casual-Runner) | UNVERIFIED — MCP instance is `thepcgtoolkit` only |

**EditMode-verified matrix coverage: 19/19** (fixtures + green AllSmoke)  
**Play-mode verified: 0/19**

## Per-game status

| # | Game | EditMode | Play Mode | Prefab |
|---|------|----------|-----------|--------|
| 1 | Cookie Clicker | PASS | Pending | `01_CookieClicker_Generators_Slice` |
| 2 | Clicker Heroes | PASS | Pending | `02_ClickerHeroes_TapKill_Slice` |
| 3 | AdVenture Capitalist | PASS | Pending | `03_AdventureCapitalist_Managers_Slice` |
| 4 | Universal Paperclips | PASS | Pending | `04_UniversalPaperclips_Phase_Slice` |
| 5 | A Dark Room | PASS | Pending | `05_ADarkRoom_Narrative_Slice` |
| 6 | Antimatter Dimensions | PASS | Pending | `06_AntimatterDimensions_Layers_Slice` |
| 7 | Realm Grinder | PASS | Pending | `07_RealmGrinder_Factions_Slice` |
| 8 | NGU Idle | PASS | Pending | `08_NGUIdle_Energy_Slice` |
| 9 | Melvor Idle | PASS | Pending | `09_MelvorIdle_Skills_Slice` |
| 10 | Egg, Inc. | PASS | Pending | `10_EggInc_Hatch_Slice` |
| 11 | Idle Miner Tycoon | PASS | Pending | `11_IdleMinerTycoon_Shafts_Slice` |
| 12 | Tap Titans 2 | PASS | Pending | `12_TapTitans2_TapDps_Slice` |
| 13 | Idle Heroes | PASS | Pending | `13_IdleHeroes_GachaCombat_Slice` |
| 14 | AFK Arena | PASS | Pending | `14_AFKArena_Chest_Slice` |
| 15 | Legend of Mushroom | PASS | Pending | `15_LegendOfMushroom_Lamp_Slice` |
| 16 | Capybara Go! | PASS | Pending | `16_CapybaraGo_Steps_Slice` |
| 17 | Cats & Soup | PASS | Pending | `17_CatsAndSoup_Assign_Slice` |
| 18 | Neko Atsume | PASS | Pending | `18_NekoAtsume_CheckIn_Slice` |
| 19 | Fallout Shelter | PASS | Pending | `19_FalloutShelter_Dwellers_Slice` |
| 20 | Synergism (gap) | Deferred | — | see matrix § coverage gap |

## Run tests (batchmode; do not pass `-quit`)

```text
Unity.exe -batchmode -nographics -projectPath D:\Git\Hyper-Casual-Runner ^
  -executeMethod HyperCasualRunner.Editor.IdleBatchATestRunner.RunAllIdleSmokeAndExit ^
  -logFile Logs/IdleAllSmoke-api.log
```

## Human Play-Smoke

Unity menu: **IdleToolkit → MVP → Human Play-Smoke Instructions**  
Also: `Assets/ToolkitExamples/Idle/README_IDLE_SLICES.txt` · `docs/idle-toolkit-compose.md`

## Next

1. Open **Hyper-Casual-Runner** in Unity 6000.5.5f1 and connect MCP (not `thepcgtoolkit`) → play-smoke Batch A → mark Playable.
2. Do not implement candidate #20 Synergism until Play Mode evidence exists.

## Blockers

- MCP bridge only registers `thepcgtoolkit` — Hyper-Casual-Runner editor not connected → Play Mode stuck at 0/19.
