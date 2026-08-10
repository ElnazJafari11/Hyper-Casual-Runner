# Round 02 Implement 09 — Idle EditMode False-Green Fixes

**Agent:** implement 9/10  
**Review:** `.agents/idle_swarm/round_02/review_09_tests.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Commit:** `dee5793` (+ tip `e5fdce2` receipt hash note) local only (no push)

---

## ASSUMPTIONS

1. Capybara advance must gate on earned `ExploreUnlocked` (one step unlocks; ADR remains 5 stokes) — confidence: **high** — verified by: review_09 §False greens #1 + `TryAdvanceStep` shared with auto-tiles.
2. NGU free `ProgressionLevel=1` on alloc must not satisfy XP/level asserts — confidence: **high** — verified by: `IdleAllocateEnergySystem` + review_09 §2.
3. TT2 `IdleCombatState.HeroDps` shares CH combat DPS loop — confidence: **high** — verified by: `IdleSliceSimulationSystem` combat foreach + CH fractional fixture mirror.
4. Progress/generator “19/19 EditMode-verified” overclaims matrix verb closure — confidence: **high** — verified by: review_09 §7; demote Antimatter + AFK to PARTIAL.
5. Bootstrap `ExploreUnlocked=0` for Capybara requires a HUD “Take Step” (action 0) or play is stuck — confidence: **high** — verified by: prior UI only fired action 1.

---

## Changes

| Area | What |
|------|------|
| `IdleSliceActionSystems.cs` | Capybara unlocks `ExploreUnlocked` after 1 step; `TryAdvanceStep` gates ADR **and** Capybara when locked |
| `IdleSliceBootstrap.cs` | Capybara starts `ExploreUnlocked=0` (no free advance/auto-tiles) |
| `IdleSliceUIController.cs` | Capybara HUD: **Take Step** (action 0) + **Next Step** (action 1) |
| `IdleBatchBCSmokeTests.cs` | Capybara fail-closed + success; FiveSteps earns unlock (no seed); NGU XP/level after alloc snapshot; `TapTitans2_CombatDps_FractionalOverOneSecond` |
| `docs/idle-toolkit-progress.md` | Honest ~17/19 scorecard; Antimatter + AFK PARTIAL; Capybara/NGU/TT2 OK after fixes |
| `IdleToolkitSliceGenerator.cs` | Stop “EditMode-verified 19/19” as verb proof; checklist Take Step |

---

## Acceptance criteria (review_09 recommended) → status

1. **Capybara causal gate** — **MET** — advance no-op without steps; steps set `ExploreUnlocked`; then advance raises level. FiveSteps no longer seeds unlock.
2. **NGU assert hygiene** — **MET** — snapshot after alloc; require `SkillXp` **or** `ProgressionLevel` beyond free alloc=1.
3. **TT2 Hero DPS** — **MET** — fractional 1s DPS fixture mirrors CH.
4. **Honest progress labels** — **MET** — scorecard demotes Antimatter/AFK; generator no longer equates AllSmoke green to 19/19 verb closure.

Deferred (review listed, out of user scope): AdvCap/Miner post-hire `PumpSim`; AFK combat path; fold Kernel into AllSmoke.

---

## VERIFICATION

```
ROUTE: Executor (from round_02 review_09 ACs)
ASSUMPTIONS: 5 verified, 0 unverified

Logs/IdleAllSmoke-Summary.txt:
result=Passed pass=56 fail=0 skip=0 inconclusive=0 duration=2.6209638

Logs/IdleCombat-impl03-r2.log (same AllSmoke compile of these fixtures):
CapybaraGo_FiveSteps_RaisesGlobalMultiplier => Passed
CapybaraGo_StepsThenAdvance_RaisesLevel => Passed
NguIdle_AllocateEnergyThenTick_ProducesFromSpend => Passed
TapTitans2_CombatDps_FractionalOverOneSecond => Passed

STATUS: VERIFIED
```

**Note:** Dedicated `Logs/IdleAllSmoke-impl09-r2.log` could not be started while sibling swarm batchmodes held `Temp/UnityLockfile`; evidence above is from a concurrent AllSmoke that compiled the same on-disk fixtures.

**RISKS:** AutoTiles harden still seeds `ExploreUnlocked=1` (intentional isolation). AdvCap/Miner income-tick gap and AFK “Auto-Combat” naming remain PARTIAL. Play Mode still 0/19.

---

## Flash Base

None.
