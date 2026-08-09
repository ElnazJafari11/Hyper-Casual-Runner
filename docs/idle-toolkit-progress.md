# Idle Toolkit Progress

Branch: `antigravity/toolkit-persist`  
Matrix source: `docs/idle_mechanics_matrix.md` (19 titles)

## Kernel (shared)

| Piece | Status | Path |
|-------|--------|------|
| Idle slice components / archetypes | Done | `Assets/Scripts/ECS/Components/IdleSliceComponents.cs` |
| Click / buy / manager / sim / actions | Done | `Assets/Scripts/ECS/Systems/Idle/*` |
| Prestige hooks IdleSliceState | Done | `Assets/Scripts/ECS/Systems/Idle/PrestigeSystem.cs` |
| Runtime bootstrap + persistence load/save | Done | `Assets/Scripts/ECS/Authoring/IdleSliceBootstrap.cs` |
| `GameProgressData` idle keys | Done | `Assets/Scripts/GameProgressData.cs` |
| Adaptive UI Toolkit controller | Done | `Assets/Scripts/UI/IdleSliceUIController.cs` |
| Prefab generator menu | Done | `Assets/Scripts/Editor/IdleToolkitSliceGenerator.cs` |
| Batch A EditMode runner | Done | `Assets/Scripts/Editor/IdleBatchATestRunner.cs` |

## Verification evidence (this loop)

| Check | Result |
|-------|--------|
| `compile_check` Assembly-CSharp + Editor | **PASS** (0 errors) |
| Prefab regenerate `IdleToolkit/Generate All Idle MVP Slices` | **PASS** (19 slices) |
| EditMode `IdleBatchASmokeTests` via `IdleBatchATestRunner.RunAndExit` | **PASS** `6/6` (`result=Passed pass=6 fail=0`) |
| Play Mode / MCP Hyper-Casual-Runner editor | **UNVERIFIED** (MCP still on `thepcgtoolkit` only) |

Batch A EditMode covers: Cookie Clicker, AdVenture Capitalist, Clicker Heroes, Universal Paperclips, Antimatter Dimensions, + GameProgressData round-trip.

## Per-game MVP checklist

| # | Game | Prefab | Status |
|---|------|--------|--------|
| 1 | Cookie Clicker | `Idle/01_CookieClicker_Generators_Slice.prefab` | **EditMode-verified** (Batch A) |
| 2 | Clicker Heroes | `Idle/02_ClickerHeroes_TapKill_Slice.prefab` | **EditMode-verified** (Batch A) |
| 3 | AdVenture Capitalist | `Idle/03_AdventureCapitalist_Managers_Slice.prefab` | **EditMode-verified** (Batch A) |
| 4 | Universal Paperclips | `Idle/04_UniversalPaperclips_Phase_Slice.prefab` | **EditMode-verified** (Batch A) |
| 5 | A Dark Room | `Idle/05_ADarkRoom_Narrative_Slice.prefab` | Code+prefab (play UNVERIFIED) |
| 6 | Antimatter Dimensions | `Idle/06_AntimatterDimensions_Layers_Slice.prefab` | **EditMode-verified** (Batch A) |
| 7 | Realm Grinder | `Idle/07_RealmGrinder_Factions_Slice.prefab` | Code+prefab (play UNVERIFIED) |
| 8 | NGU Idle | `Idle/08_NGUIdle_Energy_Slice.prefab` | Code+prefab (play UNVERIFIED) |
| 9 | Melvor Idle | `Idle/09_MelvorIdle_Skills_Slice.prefab` | Code+prefab (play UNVERIFIED) |
| 10 | Egg, Inc. | `Idle/10_EggInc_Hatch_Slice.prefab` | Code+prefab (play UNVERIFIED) |
| 11 | Idle Miner Tycoon | `Idle/11_IdleMinerTycoon_Shafts_Slice.prefab` | Code+prefab (play UNVERIFIED) |
| 12 | Tap Titans 2 | `Idle/12_TapTitans2_TapDps_Slice.prefab` | Code+prefab (play UNVERIFIED) |
| 13 | Idle Heroes | `Idle/13_IdleHeroes_GachaCombat_Slice.prefab` | Code+prefab (play UNVERIFIED) |
| 14 | AFK Arena | `Idle/14_AFKArena_Chest_Slice.prefab` | Code+prefab (play UNVERIFIED) |
| 15 | Legend of Mushroom | `Idle/15_LegendOfMushroom_Lamp_Slice.prefab` | Code+prefab (play UNVERIFIED) |
| 16 | Capybara Go! | `Idle/16_CapybaraGo_Steps_Slice.prefab` | Code+prefab (play UNVERIFIED) |
| 17 | Cats & Soup | `Idle/17_CatsAndSoup_Assign_Slice.prefab` | Code+prefab (play UNVERIFIED) |
| 18 | Neko Atsume | `Idle/18_NekoAtsume_CheckIn_Slice.prefab` | Code+prefab (play UNVERIFIED) |
| 19 | Fallout Shelter | `Idle/19_FalloutShelter_Dwellers_Slice.prefab` | Code+prefab (play UNVERIFIED) |

**Counts:** prefabs 19/19 · EditMode-verified core beats **5/19** (Batch A) · Play Mode **0/19**

## How to verify

```text
# Compile pre-gate (MCP or local):
compile_check project_path=D:/Git/Hyper-Casual-Runner

# EditMode Batch A (no -quit; runner exits itself):
Unity.exe -batchmode -nographics -projectPath D:\Git\Hyper-Casual-Runner ^
  -executeMethod HyperCasualRunner.Editor.IdleBatchATestRunner.RunAndExit ^
  -logFile Logs/IdleBatchA-api.log
# Expect Logs/IdleBatchA-Summary.txt → pass=6 fail=0

# Play smoke (needs Hyper-Casual-Runner editor):
# Drop Assets/ToolkitExamples/Idle/0X_*_Slice.prefab → Play → HUD buttons
```

## Next batch (loop)

1. Add `IdleBatchBSmokeTests` (Dark Room, Realm, NGU, Melvor, Egg, Miner).
2. Add `IdleBatchCSmokeTests` (Tap Titans, Idle Heroes, AFK, Mushroom, Capybara, Cats, Neko, Shelter).
3. When Hyper-Casual-Runner is on MCP: play-smoke Batch A HUD once; mark Playable.
4. Optional: persist OwnedGenerators onto BuyableGenerator on load.

## Blockers

- MCP Unity instance remains `thepcgtoolkit` — cannot play-mode smoke this repo via MCP.
- Plain `-runTests -quit` exits before Test Runner starts on this project; use `IdleBatchATestRunner.RunAndExit` instead.
