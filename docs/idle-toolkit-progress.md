# Idle Toolkit Progress

Branch: `antigravity/toolkit-persist`  
Matrix: `docs/idle_mechanics_matrix.md` (19 titles + candidate #20 Synergism deferred)  
Compose: `docs/idle-toolkit-compose.md`

## Evidence (latest — round_02 impl_09)

| Check | Result |
|-------|--------|
| Prefabs `Assets/ToolkitExamples/Idle/*` | 19/19 present |
| EditMode AllSmoke (`IdleBatchA` + `IdleBatchBC`) | **PASS 56/56** — `Logs/IdleAllSmoke-Summary.txt` (`result=Passed pass=56 fail=0`); target fixtures also in `Logs/IdleCombat-impl03-r2.log` |
| Fixture presence (1 named smoke per matrix title) | **19/19** |
| Play Mode (MCP Hyper-Casual-Runner) | UNVERIFIED — MCP still on `thepcgtoolkit` |

**Do not equate fixture / NUnit green with full matrix verb closure.** AllSmoke proves EditMode asserts; the scorecard below is the verb bar (review_09 / round_02 review_09).

| Claim | Value |
|-------|--------|
| Fixture presence | **19/19** |
| EditMode NUnit green (AllSmoke) | **56/56** (includes extras beyond matrix 19) |
| Matrix core-verb + causal beat | **~17/19 OK** — Antimatter + AFK Arena remain PARTIAL (see scorecard) |
| Play-mode verified | **0/19** |

## Per-game EditMode verb status (round_02 impl_09)

| # | Game | Matrix core verb | Verb score |
|---|------|------------------|------------|
| 1 | Cookie Clicker | Click → Buy Generators | OK — click funds buy; spend + OwnedCount + CPS sim |
| 2 | Clicker Heroes | Tap to Kill → Buy Heroes | OK — kill then buy; gold + combat sync |
| 3 | AdVenture Capitalist | Buy Businesses (+ managers) | OK — buy then hire; no pre-owned; hire spend |
| 4 | Universal Paperclips | Manufacture → Phase-Shift | OK — manufacture then phase |
| 5 | A Dark Room | Stoke → Explore | OK — wood/stoke + explore |
| 6 | Antimatter Dimensions | Buy Dimensions | PARTIAL — buy + spend only; no nested layer/eternity; Cookie-like |
| 7 | Realm Grinder | Build & Align | OK — buy then faction |
| 8 | NGU Idle | Allocate Energy | OK — allocate then TickEnergy; SkillXp/level beyond free alloc=1 |
| 9 | Melvor Idle | Grind Skills | OK — IdleSkillNode + sim (not click) |
| 10 | Egg, Inc. | Hatch (Tap Burst) | OK — hatch + CPS tick |
| 11 | Idle Miner Tycoon | Upgrade Shafts (+ managers) | OK — buy then hire |
| 12 | Tap Titans 2 | Tap / Hero DPS | OK — tap-kill + gold + respawn + Hero DPS fractional sim |
| 13 | Idle Heroes | Auto-Combat | OK — IdleCombatState DPS (not gacha stand-in) |
| 14 | AFK Arena | Auto-Combat (+ AFK chest) | PARTIAL — Campaign + AFK Chest (compose); not IdleCombatState auto-combat |
| 15 | Legend of Mushroom | Rub Lamp | OK — 3 pulls from 0; spend; stage |
| 16 | Capybara Go! | Step-based Narrative | OK — advance gated on earned steps; fail-closed + success paths |
| 17 | Cats & Soup | Assign Cats | OK — assign + station + sim output |
| 18 | Neko Atsume | Place Food/Toys | OK — spend + attract |
| 19 | Fallout Shelter | Assign Dwellers | OK — station + PendingClaim sim |

Still **out of scope** for “verb verified” marketing: Play Mode, prefab↔bootstrap wiring, multi-slice crosstalk stress, full Soul Eggs / nested eternity layers, AFK combat path.

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

Unity menu: **IdleToolkit → MVP → Human Play-Smoke Instructions**  
Checklist artifact: `docs/idle-play-smoke-checklist.md` (also written by that menu).

## Next

1. Bridge MCP to Hyper-Casual-Runner → play-smoke → Play column.
2. Optional: multi-slice isolation + prefab bootstrap EditMode; AFK combat or rename matrix marketing; Antimatter nested layer beat.

## Blockers

- MCP instance is only `thepcgtoolkit` — Hyper-Casual-Runner editor not connected → Play Mode 0/19.
