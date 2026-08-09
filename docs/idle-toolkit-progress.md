# Idle Toolkit Progress

Branch: `antigravity/toolkit-persist`  
Matrix: `docs/idle_mechanics_matrix.md` (19 titles)

## Evidence (latest)

| Check | Result |
|-------|--------|
| `compile_check` (runtime + editor) | PASS (0 errors) |
| Prefabs `Assets/ToolkitExamples/Idle/*` | 19/19 present (regenerated via GenerateAll) |
| EditMode Batch A (`IdleBatchASmokeTests`) | **PASS 6/6** — `Logs/IdleBatchA-api.log` / prior summary |
| EditMode Batch B+C (`IdleBatchBCSmokeTests`) | **PASS 14/14** — `Logs/IdleBatchBC-Summary.txt` (`pass=14 fail=0`) |
| Play Mode (MCP Hyper-Casual-Runner) | UNVERIFIED — MCP still on `thepcgtoolkit` |

**EditMode-verified core verb + beat: 19/19**  
**Play-mode verified: 0/19**

## Per-game status

| # | Game | EditMode | Play Mode | Prefab |
|---|------|----------|-----------|--------|
| 1 | Cookie Clicker | Batch A PASS | Pending | `01_CookieClicker_Generators_Slice` |
| 2 | Clicker Heroes | Batch A PASS | Pending | `02_ClickerHeroes_TapKill_Slice` |
| 3 | AdVenture Capitalist | Batch A PASS | Pending | `03_AdventureCapitalist_Managers_Slice` |
| 4 | Universal Paperclips | Batch A PASS | Pending | `04_UniversalPaperclips_Phase_Slice` |
| 5 | A Dark Room | Batch BC PASS | Pending | `05_ADarkRoom_Narrative_Slice` |
| 6 | Antimatter Dimensions | Batch A PASS | Pending | `06_AntimatterDimensions_Layers_Slice` |
| 7 | Realm Grinder | Batch BC PASS | Pending | `07_RealmGrinder_Factions_Slice` |
| 8 | NGU Idle | Batch BC PASS | Pending | `08_NGUIdle_Energy_Slice` |
| 9 | Melvor Idle | Batch BC PASS | Pending | `09_MelvorIdle_Skills_Slice` |
| 10 | Egg, Inc. | Batch BC PASS | Pending | `10_EggInc_Hatch_Slice` |
| 11 | Idle Miner Tycoon | Batch BC PASS | Pending | `11_IdleMinerTycoon_Shafts_Slice` |
| 12 | Tap Titans 2 | Batch BC PASS | Pending | `12_TapTitans2_TapDps_Slice` |
| 13 | Idle Heroes | Batch BC PASS | Pending | `13_IdleHeroes_GachaCombat_Slice` |
| 14 | AFK Arena | Batch BC PASS | Pending | `14_AFKArena_Chest_Slice` |
| 15 | Legend of Mushroom | Batch BC PASS | Pending | `15_LegendOfMushroom_Lamp_Slice` |
| 16 | Capybara Go! | Batch BC PASS | Pending | `16_CapybaraGo_Steps_Slice` |
| 17 | Cats & Soup | Batch BC PASS | Pending | `17_CatsAndSoup_Assign_Slice` |
| 18 | Neko Atsume | Batch BC PASS | Pending | `18_NekoAtsume_CheckIn_Slice` |
| 19 | Fallout Shelter | Batch BC PASS | Pending | `19_FalloutShelter_Dwellers_Slice` |

## Run tests (batchmode; do not pass `-quit`)

```text
Unity.exe -batchmode -nographics -projectPath D:\Git\Hyper-Casual-Runner ^
  -executeMethod HyperCasualRunner.Editor.IdleBatchATestRunner.RunAllIdleSmokeAndExit ^
  -logFile Logs/IdleAllSmoke-api.log
```

## Human Play-Smoke

Unity menu: **IdleToolkit → Human Play-Smoke Instructions**  
Also: `Assets/ToolkitExamples/Idle/README_IDLE_SLICES.txt`

## Next

1. Open **this** project in Unity (bridge MCP to Hyper-Casual-Runner) → play-smoke Batch A → set Play column to Playable.
2. Optional: `RunAllIdleSmokeAndExit` after mechanic changes.

## Blockers

- Confirmed: MCP instance is only `thepcgtoolkit` — Hyper-Casual-Runner editor not connected → Play Mode stuck at 0/19.
