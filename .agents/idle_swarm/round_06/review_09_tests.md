# Round 06 Review 09 — AllSmoke Tip (103/103) Honesty Re-Audit

**Agent:** examine 9/10  
**Scope:** Tip `Logs/IdleAllSmoke-Summary.txt` (claimed **103/103**) vs on-disk AllSmoke leafs + R5 `review_09_tests` / `impl_04_tests` disposition  
**Mode:** review only — no implementation, no push  
**Prior:** R5 review_09 (tip 91 vs HEAD 94; progress 89; IH persist absent from tip) → R5 `impl_04_tests.md` (fresh tip **103**, Cosmetics folded, progress synced, gacha receipt hygiene)

---

## Verdict

**Tip `pass=103` is honest against current HEAD AllSmoke leafs.** Machine check: tip-writing log leaf `=> Passed` names = on-disk `[Test]` names across the five runner fixtures (**103 ↔ 103**, empty set-diff). This closes R5’s HIGH false-green class (tip subset of HEAD).

**Residual is cite hygiene, not count fraud.** Progress still attributes tip green to `IdleAllSmoke-impl04-r5b.log` (`duration=6.0454057`), but live tip Summary duration `3.4890533` was written by the later `Logs/IdlePrestige-impl09-r5b.log` after `IdlePrestigeFactionBonusTests` was folded into `RunAllIdleSmokeAndExit`. Both runs printed **103/103** with the **same** Passed name set; the prestige rewrite did not invent coverage — it re-locked the same 103 names under the expanded filter.

Primary matrix verbs remain **~17/19 OK** (Antimatter + AFK Arena PARTIAL). Do **not** equate tip green with Play Mode or full verb closure. HUD (4) remains outside AllSmoke.

---

## Tip reconciliation (machine-checkable)

| Artifact | Pass count | Notes |
|----------|------------|-------|
| `Logs/IdleAllSmoke-Summary.txt` | **103** | `result=Passed pass=103 fail=0 … duration=3.4890533` |
| `Logs/IdleAllSmoke-TestResults.xml` | **103** | `total="103" passed="103"` |
| **Tip-writing log (authoritative)** | **103** | `Logs/IdlePrestige-impl09-r5b.log` — same duration; filter A+BC+Kernel+Cosmetics+**PrestigeFaction**; 103 leaf Passed lines |
| R5 impl_04 success log (superseded tip writer) | **103** | `Logs/IdleAllSmoke-impl04-r5b.log` — `duration=6.0454057`; filter **without** PrestigeFaction at that moment; same 103 Passed **names** |
| Progress doc | **103** | Count OK; **wrong corroborating log** + fixture list omits PrestigeFaction |
| HEAD source AllSmoke `[Test]` | **103** | Exact name match to tip log |

### HEAD fixture math (at review)

| Fixture | Path | `[Test]` | In AllSmoke? | In tip log? |
|---------|------|----------|--------------|-------------|
| Batch A | `IdleBatchASmokeTests.cs` | **23** | Yes | Yes (all) |
| Batch B+C | `IdleBatchBCSmokeTests.cs` | **60** | Yes | Yes (all) |
| Kernel | `IdleKernelCorrectnessTests.cs` | **13** | Yes | Yes (all) |
| Cosmetics | `CosmeticsShopTests.cs` | **6** | Yes | Yes (all) |
| Prestige faction | `IdlePrestigeFactionBonusTests.cs` | **1** | Yes | Yes (`RealmGrinder_AlignEvil_…`) |
| HUD | `IdleSliceHudSmokeTests.cs` | **4** | **No** | No |
| **AllSmoke total** | | **103** | A+BC+Kernel+Cosmetics+PrestigeFaction | **103/103** |

Runner (`IdleBatchATestRunner.RunAllIdleSmokeAndExit`) lists all five fixtures above. Expected leaf total **103**. Tip **103** is a **full HEAD lock** for that filter (unlike R5’s tip-91 subset).

### R5 “expect ~94 / BC61” → how 103 stayed stable

| Era | Filter | Approx leafs |
|-----|--------|--------------|
| R5 review | A+BC+Kernel (no Cosmetics) | **94** (BC 58 then; tip 91 missing 3 IH) |
| R5 `impl_04` tip write | A+BC+Kernel+Cosmetics | **103** (impl receipt: A23+BC61+K13+C6) |
| Post–prestige fold (live tip) | + `IdlePrestigeFactionBonusTests` | **103** (A23+**BC60**+K13+C6+**P1**) |

`RealmGrinder_AlignEvil_RaisesPassiveIncomeViaFactionBonus` moved from BC race-surface into the dedicated prestige fixture; BC count dropped 61→60 while runner gained +1 — net tip stayed **103**. Name-level tip↔disk diff is empty.

### Named lines R5 required in tip (still present)

| Name | In tip-writing prestige log |
|------|-----------------------------|
| `IdleHeroes_HeroDps_RestoresFromPassiveRateOnAttach` | Passed |
| `IdleHeroes_HeroDps_SurvivePersistNowReload` | Passed |
| `IdleHeroes_CombatState_PersistsRoundTrip_ZoneNotRebuiltFromStageLevel` | Passed |
| `ApplyPersistedElapsed_ZeroGrant_DoesNotStampLastIdleUpdateTime` | Passed (CookieClicker ZeroGrant path from R5 fix) |
| `RealmGrinder_AlignEvil_RaisesPassiveIncomeViaFactionBonus` | Passed |

---

## Round_05 → disposition

| Round_05 gap / AC | Post–impl_04 + prestige fold status |
|-------------------|-------------------------------------|
| HIGH — tip 91 vs HEAD 94 (IH persist missing) | **FIXED** — tip **103** includes IH trio; name-locked to HEAD |
| HIGH — progress 89 vs tip 91 | **FIXED on count** — progress **103/103**; **LOW cite drift** remains (points at r5b, tip writer is prestige) |
| HIGH — gacha receipt overclaimed tip-91 membership | **FIXED** — R5 correction block on `round_04/impl_06_gacha.md` |
| AC1 fresh AllSmoke = leaf count | **MET** (still met after prestige tip rewrite) |
| AC2 progress sync to tip | **MET count / PARTIAL cite** — omits PrestigeFaction in fixture prose; wrong log path |
| AC3 gacha hygiene | **MET** |
| LOW — README `EditMode-verified: 19/19` without nuance | **FIXED** — README points at tip Summary + ~17/19 verbs |
| LOW — multi-slice TargetSlice buy/narrative | **STILL OPEN** — BC `FireNarrative` still emits `IdleNarrativeActionEvent` **without** `TargetSlice` |
| LOW — AfkArena mid-gate claim twin | **STILL OPEN** — IH `ClaimBelowTenSeconds_NoPayout` only |
| LOW — PrefsUsed Egg/Miner/TT2 | **STILL OPEN** — BC array still 11 archetypes; EggInc / IdleMinerTycoon / TapTitans2 absent |
| MED — AFK Auto-Combat vs chest MVP | **ACKNOWLEDGED PARTIAL** — unchanged (correct) |
| MED — HUD outside AllSmoke | **STILL OPEN** — HUD **4** leafs still excluded |
| — | **NEW (LOW)** — progress fixture list / evidence log lag prestige fold |

---

## Matrix core-verb scorecard (R6 re-audit)

Legend: **OK** · **PARTIAL** · **MISS** · **SEED**

| # | Game | Matrix core verb | Primary test | Score | Remaining issue |
|---|------|------------------|--------------|-------|-----------------|
| 1 | Cookie Clicker | Click → Buy Generators | `CookieClicker_ClickThenBuy_…` | **OK** | — |
| 2 | Clicker Heroes | Tap to Kill → Buy Heroes | `ClickerHeroes_TapKillThenBuyHero_…` | **OK** | HeroDps `>=` PassiveRate floor |
| 3 | AdVenture Cap. | Buy Businesses (+ managers) | `AdventureCapitalist_BuyThenHire_…` | **OK** | — |
| 4 | Univ. Paperclips | Manufacture → Phase-Shift | `Paperclips_ManufactureThenPhaseShift_…` | **OK** | Nested compute untested |
| 5 | A Dark Room | Stoke → Explore | `ADarkRoom_StokeThenExplore_…` | **OK** | — |
| 6 | Antimatter Dim. | Buy Dimensions (+ nested) | `Antimatter_BuyDimension_…` (+ PhaseBand/PhaseShift) | **PARTIAL** | No nested dim / eternity |
| 7 | Realm Grinder | Build & Align | `RealmGrinder_AlignFaction_…` + Evil fixture | **OK** | Good in BC; Evil in dedicated AllSmoke fixture |
| 8 | NGU Idle | Allocate Energy | `NguIdle_AllocateEnergyThenTick_…` | **OK** | — |
| 9 | Melvor Idle | Grind Skills | `MelvorIdle_GrindSkillNode_…` | **OK** | Attach→CatchUp secondary on disk |
| 10 | Egg, Inc. | Hatch (Tap Burst) | `EggInc_HatchBurst_…` | **OK** | Soul Eggs out of scope |
| 11 | Idle Miner | Upgrade Shafts (+ managers) | `IdleMiner_BuyShaftThenHire_…` | **OK** | — |
| 12 | Tap Titans 2 | Tap / Hero DPS | `TapTitans2_TapKill_…` + CombatDps | **OK** | — |
| 13 | Idle Heroes | Auto-Combat | `IdleHeroes_AutoCombatDps_…` | **OK** | Persist harden **in tip** |
| 14 | AFK Arena | Auto-Combat (+ AFK chest) | `AfkArena_SimFillsChestThenClaim_…` | **PARTIAL** | Chest+campaign; not IdleCombatState; no mid-gate twin |
| 15 | Legend of Mushroom | Rub Lamp | `LegendOfMushroom_RubLampThreeTimes_…` | **OK** | — |
| 16 | Capybara Go! | Step-based Narrative | `CapybaraGo_StepsThenAdvance_…` | **OK** | — |
| 17 | Cats & Soup | Assign Cats | `CatsAndSoup_AssignCatThenSim_…` | **OK** | — |
| 18 | Neko Atsume | Place Food/Toys | `NekoAtsume_PlaceFood/Toys_…` | **OK** | — |
| 19 | Fallout Shelter | Assign Dwellers | `FalloutShelter_AssignDwellerThenSim_…` | **OK** | — |

**Rough tally:** OK ≈ **17** · PARTIAL ≈ **2** · MISS/SEED (primary) ≈ **0**.  
Aligned with progress **~17/19**. Verb bar is **not** the tip-count problem (tip count is honest).

---

## Remaining soft greens / holes (prioritized)

### 1. LOW (cite hygiene) — Progress attributes tip to wrong log + incomplete fixture list

| Layer | Claim | Reality |
|-------|-------|---------|
| Tip Summary | **103** / `duration=3.4890533` | Honest; writer = `IdlePrestige-impl09-r5b.log` |
| Progress evidence row | tip + **`impl04-r5b.log`** | r5b wrote an earlier 103 tip (`duration=6.045…`); **overwritten** |
| Progress “AllSmoke = A+BC+Kernel+Cosmetics” | omits PrestigeFaction | Runner + tip include it |
| README fixture blurb | Kernel + Cosmetics | Same omission (count still deferred to tip) |

**Not a false green on NUnit count** — agents quoting **103/103** are correct. Agents quoting **only** r5b as the live tip writer are stale.

### 2. LOW — Multi-slice / TargetSlice under-tested for action verbs

Unchanged: Kernel two-slice **click** + Cosmetics TargetSlice spend remain the only crosstalk proofs. BC `FireNarrative` still omits `TargetSlice` (`IdleNarrativeActionEvent { ActionId = … }` only).

### 3. LOW — AFK Arena claim mid-gate not mirrored

IH: `IdleHeroes_ClaimBelowTenSeconds_NoPayout` covers ∈[1,10). AFK primary still only ≥10s fill→claim.

### 4. LOW — PrefsUsed hygiene lag

BC `PrefsUsed` still omits **EggInc / IdleMinerTycoon / TapTitans2**. Local clears inside tests mitigate; SetUp/TearDown blanket incomplete.

### 5. LOW — Named tests that still overclaim bootstrap/reload (thin, not tip false greens)

| Test | Note |
|------|------|
| `A5_Reload_OwnedCountSynced_…` | Manual sync helper — sync contract, not live MonoBehaviour-only |
| `Melvor_BootstrapLoadPath_CatchUpBanksPendingAndSkillLevel` | Kernel API path; mitigated by Attach→CatchUp |
| `FirePrestige_DoesNotAlsoCreatePhaseEvent_…` | Empty prestige after phase; does not prove prestige cannot emit phase |
| `IdleHeroes_HeroDps_SurvivePersistNowReload` | Soft floor `>= preSave − 0.01` — intentional; **now in tip** |

### 6. MED (coverage scope) — HUD still outside AllSmoke

HUD **4** leafs (`EnsureHudBuilt`, CosmeticsBuy emit, CosmeticsBuy shop outcome, LoM FarmButton hide) remain outside `RunAllIdleSmokeAndExit`. AllSmoke can stay green while HUD regresses. Acceptable if CI/docs name them separately.

### 7. MED (product) — Antimatter nested + AFK combat remain PARTIAL

Do not inflate scorecard from tip greens.

---

## Coverage holes still outside “false green”

| Hole | Notes |
|------|-------|
| Prefab ↔ UI event → system | HUD smoke ≠ click-through EditMode |
| Antimatter nested meta-layers | Explicit PARTIAL |
| Egg Soul Eggs / TT2 relics / RG depth | Prestige second-axis thin (Evil 1.25 now locked) |
| Play Mode | Still **0/19** — tip EditMode green ≠ playable |
| Swarm tip races | Concurrent agents still rewrite Summary; prefer duration-matched log over progress cite |

---

## R5 recommended ACs — re-score

| # | R5 recommended criterion | Re-score |
|---|--------------------------|----------|
| 1 | Re-run AllSmoke; tip pass = on-disk leafs; include IH persist trio | **MET** — tip **103** = disk **103**; IH trio Present |
| 2 | Sync progress AllSmoke cite to tip (not 89) | **MET count / PARTIAL cite** — 103 OK; log path + PrestigeFaction list stale |
| 3 | Receipt hygiene on gacha tip overclaim | **MET** |
| 4 | Soften README EditMode line | **MET** |
| 5 | Deferred mid-gate / PrefsUsed / TargetSlice / HUD | **UNMET** (still deferred) |
| 6 | Leave Antimatter + AFK PARTIAL | **HELD** (correct) |

---

## Recommended next acceptance criteria (implement later — do not do now)

1. **Cite sync only:** Update `docs/idle-toolkit-progress.md` evidence row to the tip-writing log (`IdlePrestige-impl09-r5b.log` **or** a fresh re-run), and list `IdlePrestigeFactionBonusTests` beside Cosmetics/Kernel. Machine-check: progress corroborating log `duration=` equals tip Summary `duration=`.  
2. Optional README fixture blurb: mention PrestigeFaction (or keep “see tip Summary” only).  
3. Deferred (still valuable, not tip-blocking): AfkArena mid-gate twin; PrefsUsed Egg/Miner/TT2; ≥1 multi-slice narrative/buy TargetSlice; optional HUD under AllSmoke or sibling CI runner.  
4. Leave Antimatter nested + AFK combat as **PARTIAL** until product picks.  
5. **Do not** treat another tip rewrite as required for honesty — live tip already locks HEAD 103/103.

---

## Bottom line

R5’s tip/HEAD false-green is **closed**: live AllSmoke tip **103/103** matches on-disk leaf names exactly (including the three IH combat-persist tests and Evil factionBonus). Matrix verbs stay **~17/19**. Highest-value leftover in the tests lane is **progress cite hygiene** (wrong corroborating log + missing PrestigeFaction in the fixture list) plus the same deferred depth holes — not another primary-verb or tip-count rewrite.
