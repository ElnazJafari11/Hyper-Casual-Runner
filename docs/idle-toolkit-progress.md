# Idle Toolkit Progress

Branch: `antigravity/toolkit-persist`  
Matrix: `docs/idle_mechanics_matrix.md` (19 titles + candidate #20 Synergism deferred)  
Compose: `docs/idle-toolkit-compose.md`

## Evidence (latest — round_04 tip Summary)

| Check | Result |
|-------|--------|
| Prefabs `Assets/ToolkitExamples/Idle/*` | 19/19 present |
| EditMode AllSmoke (`IdleBatchA` + `IdleBatchBC`) | **PASS 72/72** — tip `Logs/IdleAllSmoke-Summary.txt` (`pass=72 fail=0`) |
| Fixture presence (1 named smoke per matrix title) | **19/19** |
| Play Mode (MCP Hyper-Casual-Runner) | UNVERIFIED — MCP still on `thepcgtoolkit` |

**Do not equate fixture / NUnit green with full matrix verb closure.** AllSmoke proves EditMode asserts; the scorecard below is the verb bar (round_04 review_09). AllSmoke green ≠ Play Mode.

| Claim | Value |
|-------|--------|
| Fixture presence | **19/19** |
| EditMode NUnit green (AllSmoke) | **72/72** (includes extras beyond matrix 19; tip Summary is authoritative) |
| Matrix core-verb + causal beat | **~17/19 OK** — Antimatter + AFK Arena remain PARTIAL (see scorecard) |
| Play-mode verified | **0/19** |

Note: `RunAllIdleSmokeAndExit` may also list `IdleKernelCorrectnessTests`, but do **not** raise the AllSmoke claim above tip Summary until a successful run rewrites `Logs/IdleAllSmoke-Summary.txt`.

## Per-game EditMode verb status (round_04 review_09)

| # | Game | Matrix core verb | Verb score |
|---|------|------------------|------------|
| 1 | Cookie Clicker | Click → Buy Generators | OK — click funds buy; spend + OwnedCount + CPS sim |
| 2 | Clicker Heroes | Tap to Kill → Buy Heroes | OK — kill then buy; gold + combat sync |
| 3 | AdVenture Capitalist | Buy Businesses (+ managers) | OK — buy then hire; post-hire PumpSim income tick |
| 4 | Universal Paperclips | Manufacture → Phase-Shift | OK — manufacture then phase |
| 5 | A Dark Room | Stoke → Explore | OK — wood/stoke + explore; SurvivePersist narrative |
| 6 | Antimatter Dimensions | Buy Dimensions | PARTIAL — buy + spend only; no nested layer/eternity; Cookie-like |
| 7 | Realm Grinder | Build & Align | OK — buy then faction |
| 8 | NGU Idle | Allocate Energy | OK — allocate then TickEnergy; SkillXp/level beyond free alloc=1 |
| 9 | Melvor Idle | Grind Skills | OK — IdleSkillNode + sim (not click) |
| 10 | Egg, Inc. | Hatch (Tap Burst) | OK — hatch + CPS tick |
| 11 | Idle Miner Tycoon | Upgrade Shafts (+ managers) | OK — buy then hire; post-hire PumpSim income tick |
| 12 | Tap Titans 2 | Tap / Hero DPS | OK — tap-kill + gold + respawn + Hero DPS fractional sim |
| 13 | Idle Heroes | Auto-Combat | OK — IdleCombatState DPS (not gacha stand-in) |
| 14 | AFK Arena | Auto-Combat (+ AFK chest) | PARTIAL — Campaign + AFK Chest (compose); not IdleCombatState auto-combat |
| 15 | Legend of Mushroom | Rub Lamp | OK — 3 pulls from 0; spend; stage |
| 16 | Capybara Go! | Step-based Narrative | OK — advance gated on earned steps; fail-closed + success paths |
| 17 | Cats & Soup | Assign Cats | OK — assign + station + sim; SurvivePersist workers; station AFK → Primary |
| 18 | Neko Atsume | Place Food/Toys | OK — spend + attract; SurvivePersist CheckInCats |
| 19 | Fallout Shelter | Assign Dwellers | OK — station + PendingClaim; SurvivePersist; station AFK → Pending |

Still **out of scope** for “verb verified” marketing: Play Mode, prefab↔bootstrap wiring, multi-slice crosstalk stress, full Soul Eggs / nested eternity layers, AFK combat path.

## Persistence

`GameProgressData` PersistNow→reload is **not** limited to two archetypes. Core slice keys plus D18 cozy/session extras round-trip under EditMode SurvivePersist fixtures (Batch BC):

| Extra | Keys / restore | Archetypes |
|-------|----------------|------------|
| AssignedWorkers + station `AssignedCount` | `Workers`; clamp on attach | Cats & Soup, Fallout Shelter |
| PendingClaim / HasOfflineClaim | `Pending` / `HasClaim` | Melvor, Fallout (+ shared path) |
| CheckInCats | `Cats` (+ claim flag if cats &gt; 0) | Neko Atsume |
| ADR narrative wood/stoke/room/explore/soft | `NarrStoke`/`NarrWood`/`NarrStep`/`NarrExplore`/`NarrSoft` | A Dark Room |

Fixtures: `ADarkRoom_NarrativeWoodStoke_SurvivePersistNowReload`, `CatsAndSoup_AssignedWorkers_SurvivePersistNowReload`, `FalloutShelter_PendingAndWorkers_SurvivePersistNowReload`, `NekoAtsume_CheckInCats_SurvivePersistNowReload`.

**Station AFK (Kernel B):** Cats/Fallout with `AssignedWorkers &gt; 0` earn wall-clock station output via `IdleOfflineCatchUp` (bootstrap defaults OutputPerWorker=1.5, Interval=1s, 8h cap) — Cats → PrimaryCurrency; Fallout → PendingClaim. Zero workers still grants 0 and preserves the D23 stamp.

## Run tests (batchmode; do not pass `-quit`)

```text
Unity.exe -batchmode -nographics -projectPath D:\Git\Hyper-Casual-Runner ^
  -executeMethod HyperCasualRunner.Editor.IdleBatchATestRunner.RunAllIdleSmokeAndExit ^
  -logFile Logs/IdleAllSmoke-impl04-llm.log
```

Separate: `RunAndExit` → `Logs/IdleBatchA-Summary.txt`; `RunBatchBCAndExit` → `Logs/IdleBatchBC-Summary.txt` (runner no longer overwrites Batch A paths when running BC/All). Prefer tip `Logs/IdleAllSmoke-Summary.txt` over progress for live EditMode count if they disagree.

## Human Play-Smoke

Unity menu: **IdleToolkit → MVP → Human Play-Smoke Instructions**  
Checklist artifact: `docs/idle-play-smoke-checklist.md` (also written by that menu).

## Next

1. Bridge MCP to Hyper-Casual-Runner → play-smoke → Play column.
2. Optional: multi-slice TargetSlice for buy/narrative; AFK mid-gate claim twin; Antimatter nested layer / AFK combat product pick.

## Blockers

- MCP instance is only `thepcgtoolkit` — Hyper-Casual-Runner editor not connected → Play Mode 0/19.
