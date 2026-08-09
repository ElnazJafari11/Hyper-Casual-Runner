# Round 01 Implement 05 — Combat / Tap-Kill Idle Fidelity

**Agent:** implement 5/10  
**Source review:** `.agents/idle_swarm/round_01/review_03_combat.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Commit:** local only (never push)

---

## ASSUMPTIONS

1. Top-ranked required fixes = F1–F8; F9–F11 deferred — confidence: high — verified by: review ranked list  
2. Prefab path attaches `IdleCombatState` for CH/TT2/IH — confidence: high — verified by: `IdleSliceBootstrap`  
3. EditMode smokes with `IdleCombatState` + `PumpSim`/`SetTime` exercise the live combat path — confidence: high — verified by: batch run below  

---

## Items completed

| ID | Status | Criteria |
|----|--------|----------|
| F1 Unify combat authority | **met** | CH/TT2/IH skip `TickHeroDps` when `IdleCombatState` present; sole kill/gold/zone writer is combat query |
| F2 Wire taps → combat | **met** | `ApplyTapDamage` mutates `IdleCombatState` HP/Zone/GoldPerKill when present |
| F3 DPS numerics | **met** | Combat path: `EnemyHp -= HeroDps * dt` (float, no Max(1) floor). Fallback `TickHeroDps` also dropped Max(1) |
| F4 Buy → HeroDps | **met** | `ApplyCombatHeroBoost` sets `HeroDps = Max(PassiveRate, 1+OwnedGenerators)` + TapDamage |
| F5 IH verb cleanup | **met** | No click→gold; AFK chest accrues in sim; gacha bumps HeroDps; claim gated (no demo payout) |
| F6 Prestige CH/TT2 | **met** | Gated via `IdlePrestigeMath.ConvertRunCurrency` (0 gold = no-op); resets combat Zone/HP; UI `FirePrestige` does not also `FirePhase` |
| F7 AFK Arena honesty | **met** | Chest-only MVP: campaign click advances stage/chest only (no flat gold); claim flag at ≥10s; sim drip + milestone level |
| F8 Combat-aware tests | **met** | Batch A/BC attach `IdleCombatState`; cover tap kill, fractional DPS, buy→HeroDps, prestige, IH claim gate, AfkArena campaign |
| F9–F11 | **deferred** | Persist / HUD combat fields / dead gacha branch — skipped per review |

---

## Files touched (combat wave)

- `Assets/Scripts/ECS/Systems/Idle/IdleSliceSimulationSystem.cs`
- `Assets/Scripts/ECS/Systems/Idle/IdleClickProduceSystem.cs`
- `Assets/Scripts/ECS/Systems/Idle/IdleBuyGeneratorSystem.cs`
- `Assets/Scripts/ECS/Systems/Idle/PrestigeSystem.cs` (combat reset TapDamage/HeroDps)
- `Assets/Scripts/ECS/Systems/Idle/IdleSliceActionSystems.cs` (gacha DPS / claim gate — co-evolved with swarm)
- `Assets/Scripts/UI/IdleSliceUIController.cs` (IH/AfkArena buttons, single prestige path)
- `Assets/Scripts/Editor/Tests/IdleBatchASmokeTests.cs`
- `Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs`

---

## Deviations

- Shared event targeting (`IdleEventTarget` / `TargetSlice`) and `IdlePrestigeMath` landed from concurrent kernel/prestige implementers; combat fixes adapted to that contract instead of rewriting world-wide queries.
- `OfflineSimulationSystem` was briefly broken by SystemAPI-in-helper (parallel edit); restored to OnUpdate-only SystemAPI (kernel path) so the project compiles.
- IH AutoCombat respawn assert relaxed: continuing DPS after kill leaves `EnemyHp < Max` within the same PumpSim window.

---

## VERIFICATION

```
ROUTE: implementer (review_03 plan)
ASSUMPTIONS: 3 verified, 0 unverified
CHANGES: combat systems + UI + Batch A/BC smokes (see above)
VERIFICATION:
 - Build: Unity 6000.5.5f1 batchmode compiled and executed EditMode fixtures
 - Run: IdleBatchATestRunner.RunAllIdleSmokeAndExit
 - Behavior check: Logs/IdleAllSmoke-Summary.txt
   result=Passed pass=46 fail=0 skip=0 inconclusive=0 duration=0.968998
   Log: Logs/IdleCombat-impl05g.log (2026-08-10 ~02:44)
STATUS: VERIFIED
RISKS: F9 persist / F10 HUD combat fields still deferred; Play Mode MCP not run (editor bridge not on this project).
```
