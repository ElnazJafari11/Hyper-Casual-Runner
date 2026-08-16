# Idle Toolkit Progress

Branch: `antigravity/toolkit-persist`  
Matrix: `docs/idle_mechanics_matrix.md` (19 titles + candidate #20 Synergism deferred)  
Compose: `docs/idle-toolkit-compose.md`

## Evidence (latest — 2026-08-16 Play Batch A)

| Check | Result |
|-------|--------|
| Prefabs `Assets/ToolkitExamples/Idle/*` | 19/19 present |
| EditMode AllSmoke (`IdleBatchA` + `IdleBatchBC` + `IdleKernelCorrectness` + `CosmeticsShop` + `IdlePrestigeFactionBonusTests`) | **PASS 115/115** — tip `Logs/IdleAllSmoke-Summary.txt` + tip-writing log `Logs/IdleAllSmoke-impl01-r7b.log` (`pass=115 fail=0` `duration=10.9292972`). Historical: `IdleAllSmoke-impl03-r7.log` wrote prior 114 tip (`duration=7.2796769`) and is superseded; prestige `IdlePrestige-impl09-r5b.log` prior 103 tip (`duration=3.4890533`) also obsolete; tip 91 / progress 89 cites remain obsolete. |
| Fixture presence (1 named smoke per matrix title) | **19/19** |
| Play Mode (MCP `Hyper-Casual-Runner@7e41a209`) | **5/19 Playable** — Batch A HUD verbs on this project (01, 02, 03, 04, 06). Batch B/C still unchecked. |

**Do not equate fixture / NUnit green with full matrix verb closure.** AllSmoke proves EditMode asserts; the scorecard below is the verb bar (round_05/06/07 review_09). AllSmoke green ≠ Play Mode. HUD smoke remains outside AllSmoke.

| Claim | Value |
|-------|--------|
| Fixture presence | **19/19** |
| EditMode NUnit green (AllSmoke) | **115/115** (A+BC+Kernel+Cosmetics+PrestigeFaction; tip Summary is authoritative) |
| Matrix core-verb + causal beat | **~17/19 OK** — Antimatter + AFK Arena remain PARTIAL (see scorecard) |
| Play-mode verified | **5/19** (Batch A only) |

### Play Batch A receipts (2026-08-16, instance `Hyper-Casual-Runner@7e41a209`)

HUD buttons invoked via UI Toolkit `clicked` on the live `IdleSliceUIController`, then `sim_pump` so ECS systems ran. Prefabs dropped into an untitled scene; persist keys cleared first.

| # | Prefab | HUD verbs | End state |
|---|--------|-----------|-----------|
| 01 | Cookie Clicker | Click Cookie (20) → Buy Generator → Prestige | after buy: `cur=5.08 gens=1 cps=1`; after prestige: `cur=0 gens=0 prestige=1 mult=1.1` |
| 02 | Clicker Heroes | Tap / Attack → Buy Hero DPS → Prestige | after buy: `gens=1 cps=1 HeroDps=2 zone=4`; after prestige: `cur=0 prestige=1 zone=1 level=1 mult=1.1` |
| 03 | AdVenture Capitalist | Collect → Buy Business → Hire Manager → Angel Reset | after hire: `hired=1 IsHired cps=1`; after angel: `cur=0 gens=0 hired=0 prestige=1 mult=1.1` |
| 04 | Universal Paperclips | Make Paperclip → Buy Autoclipper → Phase Shift | after buy: `gens=1 cps=1 cur≈30`; after phase: `cur=0 prestige=1 phase=1 mult=1.1` |
| 06 | Antimatter Dimensions | Click Antimatter → Buy Dimension → Prestige Layer | after buy: `gens=1 cps=1 cur≈30`; after layer: `prestige=1 phase=1 band=Infinity`. Matrix nested eternity still PARTIAL. |

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
| 18 | Neko Atsume | Place Food/Toys | OK — spend + attract; SurvivePersist CheckInCats; wall-clock AFK → CheckInCats |
| 19 | Fallout Shelter | Assign Dwellers | OK — station + PendingClaim; SurvivePersist; station AFK → Pending |

Still **out of scope** for “verb verified” marketing: Batch B/C Play, prefab↔bootstrap wiring, multi-slice crosstalk stress, full Soul Eggs / nested eternity layers, AFK combat path.

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

**Neko AFK (Kernel B / D26 cozy):** wall-clock accrues `CheckInCats` at 1 / 5s (cap 20) + `HasOfflineClaim`; does not bank Primary (claim pays cats). Sub-interval or full buffer grants 0 and preserves the D23 stamp.

## Run tests (batchmode; do not pass `-quit`)

```text
Unity.exe -batchmode -nographics -projectPath D:\Git\Hyper-Casual-Runner ^
  -executeMethod HyperCasualRunner.Editor.IdleBatchATestRunner.RunAllIdleSmokeAndExit ^
  -logFile Logs/IdleAllSmoke-impl01-r7b.log
```

Separate: `RunAndExit` → `Logs/IdleBatchA-Summary.txt`; `RunBatchBCAndExit` → `Logs/IdleBatchBC-Summary.txt` (runner no longer overwrites Batch A paths when running BC/All). AllSmoke includes `IdleKernelCorrectnessTests` + `CosmeticsShopTests` + `IdlePrestigeFactionBonusTests` (HUD smoke still separate). Tip `Logs/IdleAllSmoke-Summary.txt` is authoritative for the live EditMode count (prefer duration-matched tip-writing log over progress if they disagree).

## Human Play-Smoke

Unity menu: **IdleToolkit → MVP → Human Play-Smoke Instructions**  
Checklist artifact: `docs/idle-play-smoke-checklist.md` (also written by that menu).

## Next

1. Play-smoke Batch B/C (14 remaining) on this same HCR editor.
2. Optional: multi-slice TargetSlice for buy/narrative; AFK mid-gate claim twin; Antimatter nested layer / AFK combat product pick.

## Blockers

- Batch A Play **unblocked** (2026-08-16) — `Hyper-Casual-Runner@7e41a209` on port 6400. Prior R5–R9 `thepcgtoolkit`-only cites are historical.
- Batch B/C Play still **0/14**.
