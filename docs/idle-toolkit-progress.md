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

| # | Game | Core verb + progression beat | Code archetype | Prefab slice | Playable verify |
|---|------|------------------------------|----------------|--------------|-----------------|
| 1 | Cookie Clicker | Click → buy gens → prestige | `CookieClicker` | Pending menu gen | UNVERIFIED |
| 2 | Clicker Heroes | Tap kill → zones → prestige | `ClickerHeroes` | Pending menu gen | UNVERIFIED |
| 3 | AdVenture Capitalist | Buy biz → hire manager → angels | `AdventureCapitalist` | Pending menu gen | UNVERIFIED |
| 4 | Universal Paperclips | Make → autoclip → phase shift | `UniversalPaperclips` | Pending menu gen | UNVERIFIED |
| 5 | A Dark Room | Stoke → explore → craft | `ADarkRoom` | Pending menu gen | UNVERIFIED |
| 6 | Antimatter Dimensions | Buy dims → nested prestige | `AntimatterDimensions` | Pending menu gen | UNVERIFIED |
| 7 | Realm Grinder | Build → faction align | `RealmGrinder` | Pending menu gen | UNVERIFIED |
| 8 | NGU Idle | Allocate energy → rebirth | `NguIdle` | Pending menu gen | UNVERIFIED |
| 9 | Melvor Idle | Skill grind → offline claim | `MelvorIdle` | Pending menu gen | UNVERIFIED |
| 10 | Egg, Inc. | Hatch burst → habitats → souls | `EggInc` | Pending menu gen | UNVERIFIED |
| 11 | Idle Miner Tycoon | Shaft upgrade → manager → mine | `IdleMinerTycoon` | Pending menu gen | UNVERIFIED |
| 12 | Tap Titans 2 | Tap + DPS → relics | `TapTitans2` | Pending menu gen | UNVERIFIED |
| 13 | Idle Heroes | Auto combat → gacha → AFK | `IdleHeroes` | Pending menu gen | UNVERIFIED |
| 14 | AFK Arena | Campaign drip → AFK chest | `AfkArena` | Pending menu gen | UNVERIFIED |
| 15 | Legend of Mushroom | Rub lamp → stage push | `LegendOfMushroom` | Pending menu gen | UNVERIFIED |
| 16 | Capybara Go! | Step narrative → milestones | `CapybaraGo` | Pending menu gen | UNVERIFIED |
| 17 | Cats & Soup | Assign cats → passive cook | `CatsAndSoup` | Pending menu gen | UNVERIFIED |
| 18 | Neko Atsume | Place food → check-in cats | `NekoAtsume` | Pending menu gen | UNVERIFIED |
| 19 | Fallout Shelter | Assign dwellers → claim | `FalloutShelter` | Pending menu gen | UNVERIFIED |

**MVP code representable:** 19/19 archetypes wired in bootstrap + UI.  
**Prefab assets on disk:** 0/19 until `IdleToolkit/Generate All Idle MVP Slices` runs in Hyper-Casual-Runner editor.  
**Play-mode verified:** 0/19 (connected MCP instance is `thepcgtoolkit`, not this repo).

## How to play a slice

1. Open Hyper-Casual-Runner in Unity 6000.5.5f1.
2. Menu: `IdleToolkit/Generate All Idle MVP Slices`.
3. Drop a prefab from `Assets/ToolkitExamples/Idle/` into a scene with an active Entities world.
4. Enter Play Mode; use HUD buttons for the core verb + progression beat.

## Next batch (loop)

1. Run generator in Hyper-Casual-Runner Unity; commit generated prefabs.
2. Wire PanelSettings on UIDocuments if missing.
3. Play-smoke Batch A (Cookie, AdvCap, Clicker Heroes, Paperclips, Antimatter).
4. Persist IdleSliceState via `GameProgressData` keys per archetype.
5. Mark checklist Playable when smoke passes.

## Blockers

- Unity MCP connected to `D:/Git/pcg-toolkit/thepcgtoolkit` only — cannot compile_check / play this repo via MCP this tick.
- Prefabs not yet generated (editor menu required).
