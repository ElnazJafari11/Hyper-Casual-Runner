# Round 07 Implement 06 — Combat deferred hygiene (no required P1)

**Agent:** implement 6/10  
**Source review:** `.agents/idle_swarm/round_07/review_03_combat.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Prior close:** R5-F1 `e016b13` / R4-F1 persist; R6 deferred receipt `e554ebc` (+ note `b448be2`)  
**Receipt commit:** *(filled after local commit)*  

---

## ASSUMPTIONS

1. Round-07 has **no [required]** combat item; R7-F1..F5 stay [deferred] unless human/orchestrator promotes — confidence: **high** — verified by: `review_03_combat.md` ranked fix list (“No [required] combat item this round”)  
2. R5-F1 HUD combat SoT + R4-F1 persist remain closed in working tree — confidence: **high** — verified by: `IdleSliceUIController.RefreshStats` read-back (`combatHud` CH/TT2/IH; `zoneLabel = combat.Zone`; HP from combat floats; append HeroDps/TapDamage/Zone) + `BuildCombatState` prefs restore refuse Level→Zone invent  
3. Peer CS0246 (`IdleGachaIdentityPersist`) that blocked R6 fresh HUD re-run is fixed — confidence: **high** — verified by: namespace-level `struct IdleGachaIdentityPersist` in `GameProgressData.cs` + commit `8319c9b`; tip BC `64/0`  
4. Tip AllSmoke `103/0` (04:57) is **stale** vs Batch BC tip `64/0` (05:13) and Batch A tip `23/0` (05:18) — confidence: **high** — verified by: file `LastWriteTime`  
5. Fresh MCP EditMode reconfirm not available this session — confidence: **high** — verified by: `mcpforunity://instances` shows only `thepcgtoolkit` (wrong project path)  

---

## Items completed

| ID | Status | Criteria |
|----|--------|----------|
| R7 required combat P1 | **n/a — none** | Review: “No [required] combat item this round.” |
| R5-F1 still closed | **met (read-back + prior logs)** | Code pins present; prior R5-F1 XML `passed=3 failed=0`. |
| R4-F1 persist still closed | **met (read-back)** | `SaveIdleCombatPersist` / `TryLoadIdleCombatPersist`; prefs branch restores Zone/HP; cold-start baby MaxHp path unchanged. |
| R7-F1 CH/TT2 persist twin | **deferred** | Skipped — IH-only persist smoke remains. |
| R7-F2 Dead IH/LoM gacha fallback | **deferred** | Skipped — `else if` without `IdleGachaState` still present (`IdleSliceActionSystems` ~111–125). |
| R7-F3 GlobalMultiplier × HeroDps | **deferred** | Skipped — combat still `EnemyHp -= dps * dt` (no mult). |
| R7-F4 Soften Max(1) HeroDps floors | **deferred** | Skipped — prestige `Max(1.0, ClickPower*0.25)`; buy `Max(PassiveRate, 1.0+OwnedGenerators)`. |
| R7-F5 HUD HP assert / Lv\|Zone split | **deferred** | Skipped — soft polish only. |

---

## Files touched

- `.agents/idle_swarm/round_07/impl_06_combat.md` — this receipt only  
- **No combat runtime / test code changes** (deferred hygiene; no required item)

---

## Deferred hygiene ledger (still open — intentional)

| Review ID | Pin (still true) |
|-----------|------------------|
| R7-D1 / R7-F2 | Dead gacha `else if` IH/LoM path without `IdleGachaState` (`IdleSliceActionSystems` ~111–125) |
| R7-D2 / R7-F3 | Passive combat DPS ignores `GlobalMultiplier`; taps/gold bake it (`EnemyHp -= dps * dt`) |
| R7-D3 / R7-F4 | Prestige/buy/cold-start `Max(1)` HeroDps floors |
| R7-D4 | Fallback `TickHeroDps` int-truncates `(int)(dps*dt)` |
| R7-D5 | Cold-start `BuildCombatState` invents Zone from Level with baby MaxHp=20 |
| R7-D6 / R7-F1 | Persist Zone≠Level round-trip smoke is IH-only |
| R7-D7..D10 | Soft HUD label / HP assert / query-fallback polish |
| R7-D11 | Tip AllSmoke aggregate stale vs post-identity Batch BC (evidence hygiene) |

---

## Deviations

- No deferred item promoted or implemented (per review + user: verify + deferred-only receipt).  
- Did **not** re-run combat HUD filter or AllSmoke this session: Unity MCP instance is `thepcgtoolkit`, not `Hyper-Casual-Runner`. Relied on tip artifacts + code read-back.  
- Peer identity fix (`8319c9b`) cleared R6’s CS0246 trap; tip BC is green at `64/0` — fresh combat reconfirm path is open for a future agent with the correct editor.

---

## VERIFICATION

```
ROUTE: implementer (review_03_combat — deferred hygiene / no required)
ASSUMPTIONS: 5 verified, 0 unverified
CHANGES: receipt only
VERIFICATION:
 - Build (fresh): UNVERIFIED this session — MCP editor is wrong project
 - Code read-back: RefreshStats combatHud CH/TT2/IH + HeroDps/TapDamage/Zone append PRESENT
 - Code read-back: BuildCombatState prefs restore + cold-start baby MaxHp PRESENT
 - Code read-back: combat loop EnemyHp -= dps * dt (no GlobalMultiplier) PRESENT
 - Code read-back: dead IH/LoM else if (~111–125) PRESENT (deferred)
 - Code read-back: prestige/buy Max(1) HeroDps floors PRESENT (deferred)
 - Code read-back: IdleGachaIdentityPersist namespace-level sibling PRESENT (peer 8319c9b)
 - Prior Run: Logs/IdleCombat-R5F1-TestResults.xml (LastWriteTime 04:42)
   result=Passed passed=3 failed=0 total=3
   Passed …(ClickerHeroes) …(TapTitans2) …(IdleHeroes)
 - Tip Batch BC: Logs/IdleBatchBC-Summary.txt (05:13) pass=64 fail=0
 - Tip Batch A: Logs/IdleBatchA-Summary.txt (05:18) pass=23 fail=0
 - Tip AllSmoke: Logs/IdleAllSmoke-Summary.txt (04:57) pass=103 fail=0 — STALE vs BC tip
 - Behavior check: no required criteria this round; deferred items intentionally open
STATUS: VERIFIED (closed P1 via prior EditMode + read-back);
         UNVERIFIED (fresh HUD/AllSmoke re-run — no Hyper-Casual-Runner editor)
RISKS: Play Mode still UNVERIFIED; R7-F1..F5 remain open; AllSmoke tip is stale
       aggregate — do not treat 103 as post-identity reconfirm.
```
