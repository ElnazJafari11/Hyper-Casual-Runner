# Round 06 Implement 08 — Combat deferred hygiene (no required P1)

**Agent:** implement 8/10  
**Source review:** `.agents/idle_swarm/round_06/review_03_combat.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Prior close:** R5-F1 `e016b13` + receipt note `a5d9966`  
**Receipt commit:** `e554ebc80289b345e1b94e92bf1b402a07a49353` (local — never push)

---

## ASSUMPTIONS

1. Round-06 has **no [required]** combat item; R6-F1..F5 stay [deferred] unless human/orchestrator promotes — confidence: **high** — verified by: `review_03_combat.md` ranked fix list  
2. R5-F1 HUD combat SoT remains closed in working tree — confidence: **high** — verified by: `IdleSliceUIController.RefreshStats` read-back (`combatHud` CH/TT2/IH; `zoneLabel = combat.Zone`; append HeroDps/TapDamage/Zone)  
3. Prior EditMode artifacts still on disk and usable for reconfirm — confidence: **high** — verified by: `Logs/IdleCombat-R5F1-TestResults.xml` (`3/0`), tip `Logs/IdleAllSmoke-Summary.txt` (`103/0`)  
4. Fresh batchmode re-run may be blocked by concurrent peer lanes — confidence: **high** — verified by: this session’s compile attempt  

---

## Items completed

| ID | Status | Criteria |
|----|--------|----------|
| R6 required combat P1 | **n/a — none** | Review: “No [required] combat item this round.” |
| R5-F1 still closed | **met (read-back + prior logs)** | Code pins present; prior R5-F1 XML `passed=3 failed=0`; tip AllSmoke `pass=103 fail=0`. |
| R6-F1 CH/TT2 persist twin | **deferred** | Skipped — IH-only persist smoke remains. |
| R6-F2 Dead IH/LoM gacha fallback | **deferred** | Skipped — `else if` without `IdleGachaState` still present (`IdleSliceActionSystems` ~111–125). |
| R6-F3 GlobalMultiplier × HeroDps | **deferred** | Skipped — combat still `EnemyHp -= dps * dt` (no mult). |
| R6-F4 Soften Max(1) HeroDps floors | **deferred** | Skipped — prestige `Max(1.0, ClickPower*0.25)`; buy `Max(PassiveRate, 1.0+OwnedGenerators)`. |
| R6-F5 HUD HP assert / Lv|Zone split | **deferred** | Skipped — soft polish only. |

---

## Files touched

- `.agents/idle_swarm/round_06/impl_08_combat.md` — this receipt only  
- **No combat runtime / test code changes** (deferred hygiene; no required item)

---

## Deferred hygiene ledger (still open — intentional)

| Review ID | Pin (still true) |
|-----------|------------------|
| R6-D1 / R6-F2 | Dead gacha `else if` IH/LoM path without `IdleGachaState` |
| R6-D2 / R6-F3 | Passive combat DPS ignores `GlobalMultiplier`; taps/gold bake it |
| R6-D3 / R6-F4 | Prestige/buy/cold-start `Max(1)` HeroDps floors |
| R6-D4 | Fallback `TickHeroDps` int-truncates `(int)(dps*dt)` |
| R6-D5 | Cold-start `BuildCombatState` invents Zone from Level with baby MaxHp=20 |
| R6-D6 / R6-F1 | Persist Zone≠Level round-trip smoke is IH-only |
| R6-D7..D10 | Soft HUD label / HP assert / query-fallback polish |

---

## Deviations

- No deferred item promoted or implemented (per review + user: verify + deferred hygiene receipt only).  
- Attempted fresh EditMode reconfirm of `IdleSliceUIController_CombatHud_ShowsHeroDpsTapDamageTrueZone` → **RunError / compile** from peer-owned mid-flight gacha identity: `IdleGachaIdentityPersist` referenced from `IdleSliceBootstrap.PersistNow` / BC tests while nested incorrectly inside `GameProgressData` (sibling persist structs are namespace-level). **Out of combat lane — not fixed here.** Log: `Logs/IdleCombat-impl08-r6.log` (`Exiting with code 3`, CS0246).  
- Relied on prior R5-F1 XML + tip AllSmoke + code read-back for closed-P1 reconfirm.

---

## VERIFICATION

```
ROUTE: implementer (review_03_combat — deferred hygiene / no required)
ASSUMPTIONS: 4 verified, 0 unverified
CHANGES: receipt only
VERIFICATION:
 - Build (fresh): UNVERIFIED — batchmode RunError CS0246 IdleGachaIdentityPersist
   (peer gacha lane; see Logs/IdleCombat-impl08-r6.log)
 - Code read-back: RefreshStats combatHud CH/TT2/IH + HeroDps/TapDamage/Zone append PRESENT
 - Prior Run: Logs/IdleCombat-R5F1-TestResults.xml
   result=Passed passed=3 failed=0 total=3
   Passed …(ClickerHeroes) …(TapTitans2) …(IdleHeroes)
 - Prior AllSmoke tip: Logs/IdleAllSmoke-Summary.txt
   result=Passed pass=103 fail=0 skip=0 inconclusive=0
 - Prior Batch A/BC tips: A pass=23; BC pass=60
 - Behavior check: no required criteria this round; deferred items intentionally open
STATUS: VERIFIED (closed P1 via prior EditMode + read-back);
         UNVERIFIED (fresh re-run — peer compile break)
RISKS: Play Mode still UNVERIFIED; R6-F1..F5 remain open; swarm may need gacha identity
       struct hoist before any fresh AllSmoke is trustworthy.
```
