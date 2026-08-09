# Idle Toolkit Progress

Branch: `antigravity/toolkit-persist`  
Matrix source: `docs/idle_mechanics_matrix.md` (19 titles)

## Kernel (shared)

| Piece | Status | Path |
|-------|--------|------|
| Idle slice components / archetypes | Done | `Assets/Scripts/ECS/Components/IdleSliceComponents.cs` |
| Click / buy / manager / sim / actions | Done | `Assets/Scripts/ECS/Systems/Idle/*` |
| Prestige hooks IdleSliceState | Done | `Assets/Scripts/ECS/Systems/Idle/PrestigeSystem.cs` |
| Runtime bootstrap (no SubScene) | Done | `Assets/Scripts/ECS/Authoring/IdleSliceBootstrap.cs` |
| Adaptive UI Toolkit controller | Done | `Assets/Scripts/UI/IdleSliceUIController.cs` |
| Prefab generator menu | Done | `Assets/Scripts/Editor/IdleToolkitSliceGenerator.cs` |
| Legacy sandbox (pre-matrix) | Exists | `Assets/Prefabs/IdleSandboxToolkit.prefab` |

## Per-game MVP checklist

| # | Game | Core verb + progression beat | Prefab | Playable verify |
|---|------|------------------------------|--------|-----------------|
| 1 | Cookie Clicker | Click → buy gens → prestige | `Idle/01_CookieClicker_Generators_Slice.prefab` | UNVERIFIED |
| 2 | Clicker Heroes | Tap kill → zones → prestige | `Idle/02_ClickerHeroes_TapKill_Slice.prefab` | UNVERIFIED |
| 3 | AdVenture Capitalist | Collect → buy → manager → angels | `Idle/03_AdventureCapitalist_Managers_Slice.prefab` | UNVERIFIED |
| 4 | Universal Paperclips | Make → autoclip → phase shift | `Idle/04_UniversalPaperclips_Phase_Slice.prefab` | UNVERIFIED |
| 5 | A Dark Room | Stoke → explore → craft | `Idle/05_ADarkRoom_Narrative_Slice.prefab` | UNVERIFIED |
| 6 | Antimatter Dimensions | Buy dims → nested prestige | `Idle/06_AntimatterDimensions_Layers_Slice.prefab` | UNVERIFIED |
| 7 | Realm Grinder | Build → faction align | `Idle/07_RealmGrinder_Factions_Slice.prefab` | UNVERIFIED |
| 8 | NGU Idle | Allocate energy → rebirth | `Idle/08_NGUIdle_Energy_Slice.prefab` | UNVERIFIED |
| 9 | Melvor Idle | Skill grind → offline claim | `Idle/09_MelvorIdle_Skills_Slice.prefab` | UNVERIFIED |
| 10 | Egg, Inc. | Hatch burst → habitats → souls | `Idle/10_EggInc_Hatch_Slice.prefab` | UNVERIFIED |
| 11 | Idle Miner Tycoon | Shaft → manager → new mine | `Idle/11_IdleMinerTycoon_Shafts_Slice.prefab` | UNVERIFIED |
| 12 | Tap Titans 2 | Tap + DPS → relics | `Idle/12_TapTitans2_TapDps_Slice.prefab` | UNVERIFIED |
| 13 | Idle Heroes | Auto combat → gacha → AFK | `Idle/13_IdleHeroes_GachaCombat_Slice.prefab` | UNVERIFIED |
| 14 | AFK Arena | Campaign drip → AFK chest | `Idle/14_AFKArena_Chest_Slice.prefab` | UNVERIFIED |
| 15 | Legend of Mushroom | Rub lamp → stage push | `Idle/15_LegendOfMushroom_Lamp_Slice.prefab` | UNVERIFIED |
| 16 | Capybara Go! | Step narrative → milestones | `Idle/16_CapybaraGo_Steps_Slice.prefab` | UNVERIFIED |
| 17 | Cats & Soup | Assign cats → passive cook | `Idle/17_CatsAndSoup_Assign_Slice.prefab` | UNVERIFIED |
| 18 | Neko Atsume | Place food → check-in cats | `Idle/18_NekoAtsume_CheckIn_Slice.prefab` | UNVERIFIED |
| 19 | Fallout Shelter | Assign dwellers → claim | `Idle/19_FalloutShelter_Dwellers_Slice.prefab` | UNVERIFIED |

**MVP code + prefab assets:** 19/19  
**Play-mode verified:** 0/19 (MCP editor is `thepcgtoolkit`, not Hyper-Casual-Runner)

## How to play

1. Open this repo in Unity 6000.5.5f1.
2. Drop any `Assets/ToolkitExamples/Idle/*_Slice.prefab` into a scene.
3. Play; use HUD buttons (PanelSettings auto-created at runtime if missing).
4. Optional: re-bake via `IdleToolkit/Generate All Idle MVP Slices`.

## Next batch (loop)

1. Connect Hyper-Casual-Runner Unity to MCP → `compile_check` + fix errors.
2. Play-smoke Batch A: Cookie, AdvCap, Clicker Heroes, Paperclips, Antimatter.
3. Persist `IdleSliceState` via `GameProgressData` keys per archetype.
4. Mark Playable when smoke passes; deepen combat/gacha numbers only if needed.

## Blockers

- No Hyper-Casual-Runner Unity instance on MCP this tick → compile/play UNVERIFIED.
- Hand-authored prefab YAML may need one Unity import refresh / menu regenerate.
