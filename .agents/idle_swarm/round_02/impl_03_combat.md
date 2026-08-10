# Round 02 Implement 03 — Combat / Tap-Kill residuals (R2-D1..D3)

**Agent:** implement 3/10  
**Source review:** `.agents/idle_swarm/round_02/review_03_combat.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Commit:** `b83e90efe3b8292e915cbd292d08ba8060886b3a` (local only — never push)

---

## ASSUMPTIONS

1. Required this round = R2-F1..F3 (D1 Stage/Zone, D2 prestige sync, D3 tap Max(1)/TapDamage); R2-F4..F8 deferred — confidence: **high** — verified by: review ranked list  
2. Prefab path still attaches `IdleCombatState` for CH/TT2/IH — confidence: **high** — verified by: prior impl_05 + live tests  
3. EditMode AllIdleSmoke is sufficient verification (Play Mode out of scope) — confidence: **high** — verified by: review out-of-scope  

---

## Items completed

| ID | Status | Criteria |
|----|--------|----------|
| R2-F1 Unify IH Stage vs Zone | **met** | `TryApplyPull` uses `Max(ProgressionLevel, Stage)`; after pull with combat, `ProgressionLevel = Max(..., Zone)`. Test: Zone=5 + stage-bump → ProgressionLevel ≥ Zone |
| R2-F2 Prestige syncs Level↔Zone | **met** | After combat reset, `ProgressionLevel = combat.Zone` (1). Prestige test asserts equality; 0-gold still no-op |
| R2-F3 Remove tap Max(1); use TapDamage | **met** | Combat taps subtract `TapDamage * mult * GlobalMultiplier` (no Max(1)). Test: TapDamage=0.5, EnemyHp=10 → Hp≈9.5 |
| R2-F4..F8 | **deferred** | Claim gate / persist / HUD / dead gacha branch / GlobalMultiplier on HeroDps — skipped per review |

---

## Files touched

- `Assets/Scripts/ECS/Systems/Idle/IdleSliceActionSystems.cs` — Stage/Zone Max policy  
- `Assets/Scripts/ECS/Systems/Idle/PrestigeSystem.cs` — ProgressionLevel = Zone after combat reset  
- `Assets/Scripts/ECS/Systems/Idle/IdleClickProduceSystem.cs` — TapDamage-driven fractional taps  
- `Assets/Scripts/Editor/Tests/IdleBatchASmokeTests.cs` — prestige assert + fractional tap test  
- `Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs` — Stage vs Zone gacha test  

---

## Deviations

- None vs R2-F1..F3 acceptance. Concurrent swarm agents held the project lock briefly; verification waited for a free lock window.  
- `IdleSliceActionSystems` Stage/Zone Max edits were scooped into concurrent kernel commit `8d2407c` before this commit; tip still contains R2-F1 and is covered by the new BC smoke.  
- PrestigeSystem runner-only gate rename (`runnerConverted` / `sliceConverted`) co-landed with R2-F2 Level sync in this commit (needed for green AllSmoke tip).

---

## VERIFICATION

```
ROUTE: implementer (review_03_combat R2-F1..F3)
ASSUMPTIONS: 3 verified, 0 unverified
CHANGES: gacha Stage/Zone Max, prestige Level sync, tap TapDamage path + 2 new smokes
VERIFICATION:
 - Build: Unity 6000.5.5f1 batchmode compiled and ran EditMode fixtures
 - Run: IdleBatchATestRunner.RunAllIdleSmokeAndExit
 - Behavior check: Logs/IdleAllSmoke-Summary.txt
   result=Passed pass=56 fail=0 skip=0 inconclusive=0 duration=2.3682412
   Log: Logs/IdleCombat-impl03-r2.log
   New: ClickerHeroes_FractionalTapDamage_RemovesHalfHp => Passed
        IdleHeroes_GachaStageBump_DoesNotDropProgressionBelowZone => Passed
        ClickerHeroes_PrestigeAtZeroGold_IsNoOp_AfterGoldResetsCombat => Passed
STATUS: VERIFIED
RISKS: R2-F4 claim gate + F5–F8 still deferred; Play Mode MCP not run (editor bridge on other project).
```
