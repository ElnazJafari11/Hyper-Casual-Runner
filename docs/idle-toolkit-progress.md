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

| # | Game | EditMode | Prefab |
|---|------|----------|--------|
| 1 | Cookie Clicker | Batch A PASS | `01_CookieClicker_Generators_Slice` |
| 2 | Clicker Heroes | Batch A PASS | `02_ClickerHeroes_TapKill_Slice` |
| 3 | AdVenture Capitalist | Batch A PASS | `03_AdventureCapitalist_Managers_Slice` |
| 4 | Universal Paperclips | Batch A PASS | `04_UniversalPaperclips_Phase_Slice` |
| 5 | A Dark Room | Batch BC PASS | `05_ADarkRoom_Narrative_Slice` |
| 6 | Antimatter Dimensions | Batch A PASS | `06_AntimatterDimensions_Layers_Slice` |
| 7 | Realm Grinder | Batch BC PASS | `07_RealmGrinder_Factions_Slice` |
| 8 | NGU Idle | Batch BC PASS | `08_NGUIdle_Energy_Slice` |
| 9 | Melvor Idle | Batch BC PASS | `09_MelvorIdle_Skills_Slice` |
| 10 | Egg, Inc. | Batch BC PASS | `10_EggInc_Hatch_Slice` |
| 11 | Idle Miner Tycoon | Batch BC PASS | `11_IdleMinerTycoon_Shafts_Slice` |
| 12 | Tap Titans 2 | Batch BC PASS | `12_TapTitans2_TapDps_Slice` |
| 13 | Idle Heroes | Batch BC PASS | `13_IdleHeroes_GachaCombat_Slice` |
| 14 | AFK Arena | Batch BC PASS | `14_AFKArena_Chest_Slice` |
| 15 | Legend of Mushroom | Batch BC PASS | `15_LegendOfMushroom_Lamp_Slice` |
| 16 | Capybara Go! | Batch BC PASS | `16_CapybaraGo_Steps_Slice` |
| 17 | Cats & Soup | Batch BC PASS | `17_CatsAndSoup_Assign_Slice` |
| 18 | Neko Atsume | Batch BC PASS | `18_NekoAtsume_CheckIn_Slice` |
| 19 | Fallout Shelter | Batch BC PASS | `19_FalloutShelter_Dwellers_Slice` |

## Run tests (batchmode; do not pass `-quit`)

```text
Unity.exe -batchmode -nographics -projectPath D:\Git\Hyper-Casual-Runner ^
  -executeMethod HyperCasualRunner.Editor.IdleBatchATestRunner.RunAllIdleSmokeAndExit ^
  -logFile Logs/IdleAllSmoke-api.log
```

## Next

1. Open Hyper-Casual-Runner in Unity + MCP → play-smoke one Batch A prefab → mark Playable.
2. Optional: re-run `RunAllIdleSmokeAndExit` after further mechanic changes.

## Blockers

- MCP editor pinned to `thepcgtoolkit` — no Play Mode smoke for this repo via MCP.
