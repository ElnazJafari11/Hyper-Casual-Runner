# Idle Toolkit Progress

Branch: `antigravity/toolkit-persist`  
Matrix: `docs/idle_mechanics_matrix.md` (19 titles + candidate #20 Synergism deferred)  
Compose: `docs/idle-toolkit-compose.md`

## Evidence (latest — impl_09)

| Check | Result |
|-------|--------|
| Prefabs `Assets/ToolkitExamples/Idle/*` | 19/19 present |
| EditMode AllSmoke (`IdleBatchA` + `IdleBatchBC`) | **PASS 46/46** — `Logs/IdleAllSmoke-Summary.txt` (`result=Passed pass=46 fail=0`, 2026-08-10 02:44) |
| Fixture presence (1 named smoke per matrix title) | **19/19** |
| Play Mode (MCP Hyper-Casual-Runner) | UNVERIFIED — MCP still on `thepcgtoolkit` |

**Do not equate fixture green with verb proof.** After review_09 / impl_09 the 19 matrix titles have strengthened EditMode paths (spend asserts, causal chains, `IdleSliceSimulationSystem` beats where fantasy is passive/AFK/skill/DPS). Extra tests beyond the 19 (kernel/prestige/combat harden) are included in the 46 count.

| Claim | Value |
|-------|--------|
| Fixture presence | **19/19** |
| EditMode NUnit green (AllSmoke) | **46/46** (includes extras beyond matrix 19) |
| Matrix core-verb + causal beat (review_09 bar) | **19/19 targeted in impl_09** — see scorecard below |
| Play-mode verified | **0/19** |

## Per-game EditMode verb status (impl_09)

| # | Game | Matrix core verb | Verb score |
|---|------|------------------|------------|
| 1 | Cookie Clicker | Click → Buy Generators | OK — click funds buy; spend + OwnedCount + CPS sim |
| 2 | Clicker Heroes | Tap to Kill → Buy Heroes | OK — kill then buy; gold + combat sync |
| 3 | AdVenture Capitalist | Buy Businesses (+ managers) | OK — buy then hire; no pre-owned; hire spend |
| 4 | Universal Paperclips | Manufacture → Phase-Shift | OK — manufacture then phase |
| 5 | A Dark Room | Stoke → Explore | OK — wood/stoke + explore |
| 6 | Antimatter Dimensions | Buy Dimensions | OK — buy + spend |
| 7 | Realm Grinder | Build & Align | OK — buy then faction |
| 8 | NGU Idle | Allocate Energy | OK — allocate then TickEnergy sim |
| 9 | Melvor Idle | Grind Skills | OK — IdleSkillNode + sim (not click) |
| 10 | Egg, Inc. | Hatch (Tap Burst) | OK — hatch + CPS tick |
| 11 | Idle Miner Tycoon | Upgrade Shafts (+ managers) | OK — buy then hire |
| 12 | Tap Titans 2 | Tap / Hero DPS | OK — kill + gold + respawn |
| 13 | Idle Heroes | Auto-Combat | OK — IdleCombatState DPS (not gacha stand-in) |
| 14 | AFK Arena | Auto-Combat (+ AFK chest) | OK — sim fills chest then claim |
| 15 | Legend of Mushroom | Rub Lamp | OK — 3 pulls from 0; spend; stage |
| 16 | Capybara Go! | Step-based Narrative | OK — steps then advance; no ExploreUnlocked seed |
| 17 | Cats & Soup | Assign Cats | OK — assign + station + sim output |
| 18 | Neko Atsume | Place Food/Toys | OK — spend + attract |
| 19 | Fallout Shelter | Assign Dwellers | OK — station + PendingClaim sim |

Still **out of scope** for “19/19 verb verified” marketing: Play Mode, prefab↔bootstrap wiring, multi-slice crosstalk stress, full Soul Eggs / nested eternity layers.

## Persistence

`GameProgressData` round-trip covers **two** archetypes with clear isolation.

## Run tests (batchmode; do not pass `-quit`)

```text
Unity.exe -batchmode -nographics -projectPath D:\Git\Hyper-Casual-Runner ^
  -executeMethod HyperCasualRunner.Editor.IdleBatchATestRunner.RunAllIdleSmokeAndExit ^
  -logFile Logs/IdleAllSmoke-impl09.log
```

Separate: `RunAndExit` → `Logs/IdleBatchA-Summary.txt`; `RunBatchBCAndExit` → `Logs/IdleBatchBC-Summary.txt` (runner no longer overwrites Batch A paths when running BC/All).

## Human Play-Smoke

Unity menu: **IdleToolkit → Human Play-Smoke Instructions**

## Next

1. Bridge MCP to Hyper-Casual-Runner → play-smoke → Play column.
2. Optional: multi-slice isolation + prefab bootstrap EditMode.

## Blockers

- MCP instance is only `thepcgtoolkit` — Hyper-Casual-Runner editor not connected → Play Mode 0/19.
