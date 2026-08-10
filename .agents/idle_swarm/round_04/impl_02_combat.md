# Round 04 Implement 02 — Combat persist (R4-F1)

**Agent:** implement 2/10  
**Source review:** `.agents/idle_swarm/round_04/review_03_combat.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Commit:** _(filled after local commit)_  
**Local only — never push.**

---

## ASSUMPTIONS

1. Required this round = R4-F1 only; R4-F2..F5 deferred — confidence: **high** — verified by: review ranked list  
2. Prefab path still attaches `IdleCombatState` for CH/TT2/IH — confidence: **high** — verified by: bootstrap `AttachArchetypeExtras`  
3. EditMode smokes sufficient (Play Mode out of scope) — confidence: **high** — verified by: review out-of-scope  

---

## Items completed

| ID | Status | Criteria |
|----|--------|----------|
| R4-F1 Persist + restore `IdleCombatState` | **met** | `SaveIdleCombatPersist` / `TryLoadIdleCombatPersist` for Zone, EnemyHp, EnemyMaxHp, HeroDps, TapDamage, GoldPerKill. Bootstrap `BuildCombatState` restores prefs Zone (not Stage-inflated Level); MaxHp≤0 rebases via `20+Zone*25`. Test: Level=8 Zone=5 → reload Zone=5, EnemyHp=40, HeroDps=12. Kill-Max + Stage-bump smokes green. |
| R4-F2..F5 | **deferred** | HUD / dead gacha branch / GlobalMultiplier DPS / Max(1) floors — skipped per review |

---

## Files touched

- `Assets/Scripts/GameProgressData.cs` — `IdleCombatPersist` + save/load/clear keys  
- `Assets/Scripts/ECS/Authoring/IdleSliceBootstrap.cs` — `PersistNow` writes combat SoT; `BuildCombatState` restore; IH cold-start HeroDps from PassiveRate  
- `Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs` — Zone/HP/DPS round-trip smoke (+ HeroDps attach/persist companions)

---

## Deviations

- Concurrent swarm agents raced the project lock and briefly overwrote bootstrap; R4-F1 wiring re-applied and kept Melvor D33 catch-up sync already on tip.  
- Full `RunAllIdleSmokeAndExit` intermittently compiled a stale Editor DLL under lock contention; acceptance verified via filtered EditMode bundle (see evidence).  

---

## VERIFICATION

```
ROUTE: implementer (review_03_combat R4-F1)
ASSUMPTIONS: 3 verified, 0 unverified
CHANGES: IdleCombatPersist prefs; bootstrap save/restore; Zone≠Level round-trip smoke
VERIFICATION:
 - Build: Unity 6000.5.5f1 batchmode EditMode (filtered)
 - Run: -testFilter combat R4-F1 bundle
 - Behavior check: Logs/IdleCombat-R4F1-bundle-TestResults.xml
   result=Passed passed=6 failed=0 total=6
   Passed IdleHeroes_CombatState_PersistsRoundTrip_ZoneNotRebuiltFromStageLevel
   Passed IdleHeroes_HeroDps_SurvivePersistNowReload
   Passed IdleHeroes_HeroDps_RestoresFromPassiveRateOnAttach
   Passed IdleHeroes_Kill_DoesNotDropStageInflatedProgressionLevel
   Passed ClickerHeroes_TapKill_DoesNotDropStageInflatedProgressionLevel
   Passed IdleHeroes_GachaStageBump_DoesNotDropProgressionBelowZone
   Single-test: Logs/IdleCombat-R4F1-TestResults.xml (CombatState Passed)
   Log: Logs/IdleCombat-impl02-r4e.log
STATUS: VERIFIED
RISKS: R4-F2–F5 deferred; Play Mode MCP not run; AllSmoke fixture-wide runs race-prone under swarm lock contention.
```
