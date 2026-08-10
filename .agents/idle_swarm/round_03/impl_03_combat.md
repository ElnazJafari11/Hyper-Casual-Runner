# Round 03 Implement 03 — Combat residuals (R3-D1 / R3-D2)

**Agent:** implement 3/10  
**Source review:** `.agents/idle_swarm/round_03/review_03_combat.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Commit:** `5ac81232f1b22b290e56ddbe1582fdd28e808f26` (local only — never push)  

---

## ASSUMPTIONS

1. Required this round = R3-F1 + R3-F2 (kill Max + IH/AfkArena claim gate); R3-F3..F7 deferred — confidence: **high** — verified by: review ranked list  
2. Prefab path still attaches `IdleCombatState` for CH/TT2/IH — confidence: **high** — verified by: prior impl receipts + live tests  
3. EditMode AllIdleSmoke is sufficient verification (Play Mode out of scope) — confidence: **high** — verified by: review out-of-scope  

---

## Items completed

| ID | Status | Criteria |
|----|--------|----------|
| R3-F1 Kill path Max with Stage/Level | **met** | Sim + tap kill: `ProgressionLevel = Max(ProgressionLevel, Zone)` after Zone++. Tests: sim Level=8→≥8 Zone=6; CH tap Level=6→≥6; Stage-bump still green |
| R3-F2 Tighten AFK claim gate for IH/AfkArena | **met** | IH/AfkArena chest requires `HasOfflineClaim` or `AfkChestSeconds >= 10f`; Neko/pending unchanged. Test: seconds=5 + flag false → no payout |
| R3-F3..F7 | **deferred** | Persist / HUD / dead gacha branch / GlobalMultiplier DPS / HeroDps Max(1) floors — skipped per review |

---

## Files touched

- `Assets/Scripts/ECS/Systems/Idle/IdleSliceSimulationSystem.cs` — combat kill Max (this commit)  
- `Assets/Scripts/ECS/Systems/Idle/IdleClickProduceSystem.cs` — tap-kill Max (this commit)  
- `Assets/Scripts/ECS/Systems/Idle/IdleSliceActionSystems.cs` — IH/AfkArena ≥10s claim gate (**already in** `763a323` prestige scoop; verified on tip)  
- `Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs` — 3 smokes (**already in** `7897db9` gacha scoop after rewrite; verified on tip)  

---

## Deviations

- Concurrent swarm agents scooped claim-gate + BC smokes into other commits before this receipt commit; this commit lands the kill Max writers that those smokes assert.  
- Tap-kill smoke uses `ClickerHeroes` (not IH): IH click path is intentionally a no-op (auto-combat only). Sim kill smoke remains on IdleHeroes.  
- Sim kill test uses Level=8 (not acceptance’s Level=6) so unconditional `Level=Zone` would fail (Level→6).

---

## VERIFICATION

```
ROUTE: implementer (review_03_combat R3-F1..F2)
ASSUMPTIONS: 3 verified, 0 unverified
CHANGES: kill Max both writers; claim gate + 3 BC smokes already on tip via concurrent commits
VERIFICATION:
 - Build: Unity 6000.5.5f1 batchmode compiled and ran EditMode fixtures
 - Run: IdleBatchATestRunner.RunAllIdleSmokeAndExit
 - Behavior check: Logs/IdleAllSmoke-Summary.txt
   result=Passed pass=72 fail=0 skip=0 inconclusive=0 duration=2.320163
   Log: Logs/IdleCombat-impl03-r3c.log
   New/kept green:
     IdleHeroes_Kill_DoesNotDropStageInflatedProgressionLevel => Passed
     ClickerHeroes_TapKill_DoesNotDropStageInflatedProgressionLevel => Passed
     IdleHeroes_ClaimBelowTenSeconds_NoPayout => Passed
     IdleHeroes_GachaStageBump_DoesNotDropProgressionBelowZone => Passed
STATUS: VERIFIED
RISKS: R3-F3–F7 deferred; Play Mode MCP not run (bridge on other project).
```
