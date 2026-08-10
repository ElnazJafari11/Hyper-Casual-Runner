# Round 05 Implement 01 — Combat HUD honesty (R5-F1)

**Agent:** implement 1/10  
**Source review:** `.agents/idle_swarm/round_05/review_03_combat.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Code commit:** `e016b13ff703daab14d1e062af573bb3be4cc5cc`  
**Receipt note commit:** (filled after local commit — never push)

---

## ASSUMPTIONS

1. Required this round = R5-F1 only; R5-F2..F5 deferred — confidence: **high** — verified by: review ranked list  
2. CH/TT2/IH prefab path still attaches `IdleCombatState` — confidence: **high** — verified by: bootstrap `AttachArchetypeExtras`  
3. EditMode string assert sufficient (Play Mode out of scope) — confidence: **high** — verified by: review acceptance + out-of-scope  

---

## Items completed

| ID | Status | Criteria |
|----|--------|----------|
| R5-F1 HUD shows combat SoT | **met** | For CH/TT2/IH with `IdleCombatState`: stats include `HeroDps`, `TapDamage`, `Zone`; `Lv/Zone` prefers `combat.Zone` over Stage-inflated `ProgressionLevel`; HP label reads combat floats. EditMode TestCase×3 PASS. |
| R5-F2..F5 | **deferred** | Dead gacha branch / GlobalMultiplier DPS / Max(1) floors / CH-TT2 persist twin — skipped per review |

---

## Files touched

- `Assets/Scripts/UI/IdleSliceUIController.cs` — `RefreshStats` combat SoT for CH/TT2/IH (not IH-only)  
- `Assets/Scripts/Editor/Tests/IdleSliceHudSmokeTests.cs` — `IdleSliceUIController_CombatHud_ShowsHeroDpsTapDamageTrueZone` (CH/TT2/IH)  
- `.agents/idle_swarm/round_05/impl_01_combat.md` — this receipt  

---

## Deviations

- Concurrent swarm agents raced the same R5-F1 working tree; code + initial receipt landed as `e016b13`. This receipt documents the EditMode run that produced `Logs/IdleCombat-R5F1-TestResults.xml`.  
- Reused `ResolveTargetSlice` + query fallback so EditMode entities without bootstrap spawn still refresh. Preserved LoM farm-button Stage gate already in `RefreshStats`.

---

## VERIFICATION

```
ROUTE: implementer (review_03_combat R5-F1)
ASSUMPTIONS: 3 verified, 0 unverified
CHANGES: RefreshStats combat HUD; EditMode string assert ×3 archetypes
VERIFICATION:
 - Build: Unity 6000.5.5f1 batchmode EditMode
 - Run: -testFilter IdleSliceUIController_CombatHud_ShowsHeroDpsTapDamageTrueZone
 - Behavior check: Logs/IdleCombat-R5F1-TestResults.xml
   result=Passed passed=3 failed=0 total=3
   Passed IdleSliceUIController_CombatHud_ShowsHeroDpsTapDamageTrueZone(ClickerHeroes)
   Passed IdleSliceUIController_CombatHud_ShowsHeroDpsTapDamageTrueZone(TapTitans2)
   Passed IdleSliceUIController_CombatHud_ShowsHeroDpsTapDamageTrueZone(IdleHeroes)
   Log: Logs/IdleCombat-impl01-r5.log
STATUS: VERIFIED
RISKS: R5-F2–F5 deferred; Play Mode MCP not run; AfkArena unchanged (no IdleCombatState).
```
