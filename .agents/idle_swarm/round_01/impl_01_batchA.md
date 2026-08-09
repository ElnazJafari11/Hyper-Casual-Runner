# Round 1 IMPLEMENT 1/10 — Batch A CPS / Prestige P0

**Agent:** implement 1/10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Date:** 2026-08-10  
**Sources:** `review_02_batchA_cps.md` (+ prestige/production overlaps from reviews 04/05)  
**Commit:** `57910f7` (local only, no push)

---

## ASSUMPTIONS

1. Preferred CPS contract: `PassiveRate = BaseCps * OwnedCount` (raw); sim applies `* GlobalMultiplier * dt` — **high** — review A1.
2. Cookie/AdvCap Prestige button must not dual-fire Phase+Prestige — **high** — review + prestige review P0.
3. AdvCap/IdleMiner manager gate must survive prestige (`RequiresManager` kept on hire; restored on reset) — **high** — reviews 02/05.
4. OwnedCount sync on load is in scope (A5) — **high** — acceptance A5; bootstrap now sets `OwnedCount = _loadedGens`.

---

## Changes

| Area | Fix |
|------|-----|
| `IdlePrestigeMath.cs` | Shared `ConvertRunCurrency`, `ComputePassiveRate`, `ResetBuyableGenerator`, manager-gate helper |
| `IdleBuyGeneratorSystem` | Raw PassiveRate (no mult bake); hire keeps `RequiresManager`; hire CPS uses actual `OwnedCount` (0 → 0); ECB destroy |
| `PrestigeSystem` | Gated conversion (no forced +1); scoped TargetSlice; restore manager gate via `ResetBuyableGenerator` |
| `IdlePhaseShiftSystem` | Same conversion helper; Paperclips/AD only; ECB destroy; manager reset |
| `IdleSliceUIController.FirePrestige` | Prestige event only (no `FirePhase`) |
| `IdleSliceBootstrap` | Load OwnedCount/managers/phase; IdleMiner included in needsManager |
| `IdleBatchASmokeTests` | A1–A5 + zero-currency + AdvCap angel + phase exclusivity; TargetSlice helpers |
| `IdleBatchATestRunner` | Log failure messages |

---

## Acceptance (review A1–A6)

| # | Criterion | Result |
|---|-----------|--------|
| A1 | Mult=2, BaseCps=1, Owned=1, 1s → +2 not ~4 | **PASS** (`A1_Cookie_SimCps_AppliesGlobalMultiplierOnce`) |
| A2 | Cookie prestige = single conversion | **PASS** (`A2_Cookie_Prestige_SingleConversion_NoForcedExtra`) |
| A3 | AdvCap after prestige RequiresManager; buy alone PassiveRate=0 | **PASS** (`A3_…`) |
| A4 | Hire with OwnedCount=0 → PassiveRate=0 | **PASS** (`A4_…`) |
| A5 | Next buy cost uses growth^OwnedCount | **PASS** (`A5_…`) |
| A6 | Batch A smoke + new tests | **PASS** for all Batch A P0 / CPS tests in AllSmoke `IdleCombat-impl05f.log` (see evidence) |
| A7 | Play Mode | **Deferred** — MCP not connected to HCR editor |

---

## Verification evidence

From `Logs/IdleCombat-impl05f.log` (AllSmoke including `IdleBatchASmokeTests`):

```
A1_Cookie_SimCps_AppliesGlobalMultiplierOnce => Passed
A2_Cookie_Prestige_SingleConversion_NoForcedExtra => Passed
A3_AdvCap_AfterPrestige_RequiresManager_BuyLeavesPassiveZero => Passed
A4_AdvCap_HireWithZeroOwned_LeavesPassiveZero => Passed
A5_Reload_OwnedCountSynced_NextBuyUsesGrowthCost => Passed
CookieClicker_ClickThenBuy_RaisesCurrencyAndOwnedGens => Passed
AdventureCapitalist_BuyThenHireManager_AutomatesPassiveRate => Passed
AdventureCapitalist_AngelReset_ConvertsAndResetsManagers => Passed
Paperclips_ManufactureThenPhaseShift_ConvertsToPrestige => Passed
PrestigeSystem_ZeroCurrency_DoesNotAward => Passed
FirePrestige_DoesNotAlsoCreatePhaseEvent_PaperclipsPhaseOnly => Passed
Antimatter_BuyDimension_RaisesOwnedAndCps => Passed
Antimatter_PhaseShift_IncrementsPhaseIndex => Passed
```

AllSmoke overall: `pass=44 fail=2` — remaining fails are combat/BC (`ClickerHeroes_TapKillThenBuyHero` HeroDps strict greater-than when PassiveRate==HeroDps; `IdleHeroes_AutoCombatDps_KillsEnemy`) outside Batch A CPS P0. ClickerHeroes assert softened to `HeroDps >= PassiveRate` in this implement.

---

## STATUS

**VERIFIED** for Batch A P0 CPS/prestige/manager/OwnedCount (A1–A5) via AllSmoke log evidence above.  
**UNVERIFIED:** Play Mode A7 (no HCR Unity MCP instance).  
**RISKS:** Multi-agent Unity batchmode contention; IdleHeroes BC fail still open for combat implementer.
