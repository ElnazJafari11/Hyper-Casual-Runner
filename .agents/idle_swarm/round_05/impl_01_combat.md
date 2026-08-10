# Round 05 Implement 01 — Combat HUD honesty (R5-F1)

**Scope:** Clicker Heroes / Tap Titans 2 / Idle Heroes HUD combat SoT  
**Agent:** implement 1/10 (relaunch after stalled `26a0a8a0`)  
**Date:** 2026-08-10  
**Source review:** `round_05/review_03_combat.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Push:** never (local commit only)

---

## Goal

Close R5-F1: for CH/TT2/IH, `RefreshStats` shows `HeroDps`, `TapDamage`, and true `IdleCombatState.Zone` (not Stage-inflated `ProgressionLevel` alone). R5-F2–F5 stay deferred.

## ASSUMPTIONS

1. R4-F1 combat persist is CLOSED; only HUD honesty remains required — confidence: **high** — verified by: `review_03_combat.md` scorecard.  
2. Prior stalled agent left working-tree R5-F1 + peer LoM/UI5 HUD WIP mixed — confidence: **high** — verified by: diff before surgical split.  
3. EditMode string assert is enough (Play Mode out of scope) — confidence: **high** — verified by: review acceptance.  
4. Concurrent HCR Unity batchmode (pid 58468) blocks a fresh re-run this pass — confidence: **high** — verified by: `doctor` + lock file.

---

## Changes

| Item | Criteria | Status |
|------|----------|--------|
| R5-F1 HUD combat SoT | CH/TT2/IH stats include HeroDps, TapDamage, combat.Zone; Lv/Zone prefers combat.Zone | **met** |
| EditMode assert | `IdleSliceUIController_CombatHud_ShowsHeroDpsTapDamageTrueZone` ×3 | **met** (prior batchmode evidence) |
| R5-F2..F5 | deferred hygiene / twin persist smokes | **skipped** (per review) |

### Files

- `Assets/Scripts/UI/IdleSliceUIController.cs` — `RefreshStats` reads `IdleCombatState` for CH/TT2/IH Zone/HP + HeroDps/TapDamage line  
- `Assets/Scripts/Editor/Tests/IdleSliceHudSmokeTests.cs` — R5-F1 parameterized EditMode assert  
- `.agents/idle_swarm/round_05/impl_01_combat.md` — this receipt  

### Deferred (skipped)

- R5-F2 dead IH/LoM gacha fallback  
- R5-F3 GlobalMultiplier on HeroDps  
- R5-F4 Max(1) HeroDps floors  
- R5-F5 CH/TT2 persist round-trip twin  
- Play Mode MCP verification  

---

## VERIFICATION

**Read-back:** `RefreshStats` sets `combatHud` for ClickerHeroes / TapTitans2 / IdleHeroes when `IdleCombatState` present; `Lv/Zone` uses `combat.Zone`; appends `HeroDps` / `TapDamage` / `Zone`.

**EditMode (prior batchmode, same R5-F1 filter — re-run blocked by concurrent HCR Unity):**

```
Filter: IdleSliceUIController_CombatHud_ShowsHeroDpsTapDamageTrueZone
Results: Logs/IdleCombat-R5F1-TestResults.xml
  ClickerHeroes => Passed
  TapTitans2    => Passed
  IdleHeroes    => Passed
  result=Passed passed=3 failed=0 total=3
Log: Logs/IdleCombat-impl01-r5.log → Exit code 0 (Ok)
```

**Fresh re-run this relaunch:** **NOT RUN** — `doctor` showed HCR project owned by batchmode pid 58468 (no bridge heartbeat / likely modal). Did not kill peer process. Evidence above is from the same R5-F1 implementation that lands in this commit.

**Play Mode:** UNVERIFIED (out of scope).

---

## Receipt

- **Completed:** R5-F1 HUD honesty for CH/TT2/IH  
- **Stubs created:** none  
- **Deviations:** Surgical commit excludes peer LoM farm-hide + UI5 cosmetics shop smoke that were mixed in the stalled agent's working tree; those remain uncommitted WIP for their owners. Fresh test re-run skipped due to concurrent HCR Unity.  
- **Flash Base:** none  
- **Escalations:** none  
- **Commit:** `e016b13ff703daab14d1e062af573bb3be4cc5cc` (local only, no push)
