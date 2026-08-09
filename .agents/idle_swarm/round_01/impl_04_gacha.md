# Round 01 Implement 04 — Gacha / Narrative / Auto

**Agent:** implement 4/10  
**Source review:** `review_06_gacha_narrative.md`  
**Date:** 2026-08-10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Commit:** local only (never push) — `acc3f74` (receipt + LoM HowTo); core code already in tree (e.g. `1e2d9df`)

---

## Scope executed

From review priority list:

| # | Item | Status |
|---|------|--------|
| P0 | Idle Heroes AFK chest fill | **DONE** |
| P0 | Gacha → combat power (HeroDps / PassiveRate) | **DONE** |
| P0 | Capybara milestone on live `IdleNarrativeState` path | **DONE** |
| P1 | LoM auto-lamp (cheap unlock after Stage≥1) | **DONE** |

Deferred (review P1+/P2): auto-tiles, dual-combat rename, gear identity, roguelite stub.

---

## ASSUMPTIONS

1. Quality bar = matrix MVP DNA (core verb + automation + one second-axis beat) — **high** — `docs/project-context.md` + review.
2. Claim AFK already consumes `AfkChestSeconds` — only fill path was missing for Idle Heroes — **high** — verified `IdleClaimOfflineSystem`.
3. LoM auto-lamp unlock at `Stage >= 1` (after 3 manual rubs) matches automation-as-graduation — **med** — cheap MVP choice.
4. Concurrent swarm edits may touch shared idle files — **high** — observed; re-applied patches until markers stable.

---

## Changes

### 1. Idle Heroes AFK chest
`IdleSliceSimulationSystem`: split `IdleHeroes` from CH/TT2; accrue `AfkChestSeconds += dt`, set `HasOfflineClaim` at ≥10s (combat authority unchanged).

### 2. Gacha → combat power
`IdleGachaPullSystem.TryApplyPull` shared pull math. On Idle Heroes success: `PassiveRate += rarity*0.5` and `IdleCombatState.HeroDps += rarity*0.5` (plus TapDamage sync).

### 3. Capybara milestone (live path)
`ApplyNarrative` action 1: when `CapybaraGo` and `RoomOrStep % 5 == 0`, `GlobalMultiplier += 0.1f` (bootstrap always has `IdleNarrativeState`).

### 4. LoM auto-lamp (cheap)
`IdleGachaState.AutoTimer` field. Simulation: if LoM and `Stage >= 1`, every 2s call `TryApplyPull` when affordable. Manual rub remains the unlock (3 pulls → stage 1).

### Tests added (`IdleBatchBCSmokeTests`)
- `IdleHeroes_AfkChest_FillsOverSimTime`
- `IdleHeroes_GachaPull_RaisesHeroDps`
- `LegendOfMushroom_AutoLamp_PullsAfterStageUnlock`
- `CapybaraGo_FiveSteps_RaisesGlobalMultiplier`

### Misc
- Generator HowTo for LoM mentions auto-lamp unlock.

---

## Acceptance criteria

| Criterion | Result |
|-----------|--------|
| Idle Heroes sim dt → `AfkChestSeconds >= N` → claim raises currency + clears chest | **MET** — test Passed |
| Idle Heroes pull raises `HeroDps` (not only ClickPower) | **MET** — test Passed |
| Capybara 5 steps with `IdleNarrativeState` raises `GlobalMultiplier` | **MET** — test Passed |
| LoM Stage≥1 + sim time → PullCount increases | **MET** — test Passed |

---

## VERIFICATION

```
TASK: Round 01 implement 4/10 — gacha/narrative/auto fixes
ROUTE: Executor (parity) against review_06 plan
ASSUMPTIONS: 3 high verified, 1 med (LoM unlock beat)
CHANGES:
 - IdleSliceComponents.cs (AutoTimer)
 - IdleSliceSimulationSystem.cs (IH AFK + LoM auto-lamp)
 - IdleSliceActionSystems.cs (TryApplyPull + HeroDps + Capybara milestone)
 - IdleBatchBCSmokeTests.cs (4 tests)
 - IdleToolkitSliceGenerator.cs (LoM HowTo)
VERIFICATION:
 - Build: EditMode Batch BC compiled and ran (swarm concurrent runs)
 - Run: Logs/IdleBatchBC-Summary.txt → result=Passed pass=29 fail=0
 - Behavior: per-test lines in Logs/IdleCombat-impl05f.log / IdleKernel-impl07.log:
     IdleHeroes_AfkChest_FillsOverSimTime => Passed
     IdleHeroes_GachaPull_RaisesHeroDps => Passed
     LegendOfMushroom_AutoLamp_PullsAfterStageUnlock => Passed
     CapybaraGo_FiveSteps_RaisesGlobalMultiplier => Passed
STATUS: VERIFIED
RISKS:
 - Play Mode still UNVERIFIED (MCP on other project)
 - Concurrent implementers share idle systems; dual-combat / UI polish still open per review
```

---

## Deviations

- LoM auto-lamp inlines via `TryApplyPull` in simulation rather than spawning `IdleGachaPullEvent` (avoids system-order ambiguity).
- Did not regenerate LoM prefab HowTo string (generator source updated only).

## Flash Base

None.
