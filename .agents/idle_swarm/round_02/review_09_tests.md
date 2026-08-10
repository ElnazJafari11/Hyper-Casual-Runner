# Round 02 Review 09 — EditMode Idle Tests Re-Audit (post impl_09)

**Agent:** examine 9/10  
**Scope:** `Assets/Scripts/Editor/Tests/IdleBatchASmokeTests.cs`, `IdleBatchBCSmokeTests.cs`, `IdleKernelCorrectnessTests.cs` vs `docs/idle_mechanics_matrix.md` + review_09 / impl_09 acceptance bar  
**Mode:** review only — no implementation, no push  
**Prior:** round_01 `review_09_tests.md` (harsh false-green list) → `impl_09_tests.md` (claimed MET on all 7 ACs) → progress doc “19/19 targeted … OK”

---

## Verdict

**impl_09 cleared most high-severity false greens from round_01.** The 19 matrix titles now have real causal chains for the verbs that were previously MISS/SEED (Melvor skill node, IH auto-combat DPS, Cookie click-funded buy, AdvCap/Miner buy→hire, Paperclips manufacture→phase, AFK chest accrual, LoM 3 pulls from 0, Cats/Fallout station sim, NGU TickEnergy currency, TT2 gold/respawn).

**Remaining issue is not “fixtures are empty wiring checks.”** It is a smaller set of **half-verb / non-causal / overclaim** greens, plus progress/generator copy that still reads as full matrix proof. Treat progress “19/19 OK” as **overstated by ~3–5 titles**, not as round_01’s ~7 MISS/SEED collapse.

Evidence cited: fixture source on disk (2026-08-10), `Logs/IdleAllSmoke-Summary.txt` → `pass=46 fail=0` (17 A + 29 BC). Kernel (5) is **outside** AllSmoke.

---

## Round_01 → impl_09 disposition

| Round_01 false green | Post-impl_09 status |
|----------------------|---------------------|
| Melvor click substitute | **FIXED** — `MelvorIdle_GrindSkillNode_LevelsViaSimTick` + `IdleSkillNode` + `PumpSim` |
| Idle Heroes gacha-for-combat | **FIXED** — `IdleHeroes_AutoCombatDps_KillsEnemy` (gacha kept as secondary monetization test) |
| Cookie injected affordability | **FIXED** — 3 clicks fund BaseCost=15; spend + OwnedCount + CPS sim |
| AdvCap / Miner hire-only seed | **FIXED** — buy then hire; spend asserts; no pre-owned |
| Paperclips prestige-only | **FIXED** — click manufacture then phase |
| AFK chest pre-seeded | **FIXED** — sim fills ≥10s then claim |
| LoM `PullCount=2` seed | **FIXED** — 3 pulls from 0 with per-pull spend |
| Capybara `ExploreUnlocked=1` seed (primary) | **CLAIMED FIXED** — primary test no longer seeds; **still false green** (see below) |
| NGU free ProgressionLevel as beat | **PARTIALLY FIXED** — currency sim assert is real; XP assert still soft (see below) |
| Zero `IdleSliceSimulationSystem` coverage | **FIXED** for Cookie/Melvor/NGU/Cats/Fallout/AFK/Egg/IH DPS |
| Demo claim free gold | **FIXED** in claim system + IH/Melvor empty-claim asserts |
| Persistence single archetype | **FIXED** — Cookie + Antimatter isolation |
| Evidence Batch A/BC overwrite | **FIXED** per impl_09 / runner split (not re-audited this pass beyond summary path) |

---

## Matrix core-verb scorecard (re-audit)

Legend: **OK** · **PARTIAL** · **MISS** · **SEED** · **SOFT** (passes for wrong reason / overclaims name)

| # | Game | Matrix core verb | Primary test | Score | Remaining issue |
|---|------|------------------|--------------|-------|-----------------|
| 1 | Cookie Clicker | Click → Buy Generators | `CookieClicker_ClickThenBuy_…` | **OK** | — |
| 2 | Clicker Heroes | Tap to Kill → Buy Heroes | `ClickerHeroes_TapKillThenBuyHero_…` | **OK** | (+ separate DPS fractional harden) |
| 3 | AdVenture Cap. | Buy Businesses (+ managers) | `AdventureCapitalist_BuyThenHire_…` | **OK / SOFT** | Buy funded by **injected** 200 (acceptable for buy-first verb). **No post-hire CPS `PumpSim`** — `PassiveRate > 0` without income-over-time (review_09 AC#4 gap). |
| 4 | Univ. Paperclips | Manufacture → Phase-Shift | `Paperclips_ManufactureThenPhaseShift_…` | **OK** | Manufacture = click path (wired MVP). Nested compute allocation untested. |
| 5 | A Dark Room | Stoke → Explore | `ADarkRoom_StokeThenExplore_…` | **OK** | Craft covered in separate harden test. |
| 6 | Antimatter Dim. | Buy Dimensions (+ nested layers) | `Antimatter_BuyDimension_…` | **PARTIAL** | Buy+spend OK; **indistinguishable from Cookie buy**; no eternity/layer. Injected 50 funds buy. No CPS sim. |
| 7 | Realm Grinder | Build & Align | `RealmGrinder_BuildThenAlign_…` | **OK / SOFT** | Build+align present. Align is free `Amount→FactionId` (no energy cost). Faction 2 / re-pick untested. Injected build funds. |
| 8 | NGU Idle | Allocate Energy | `NguIdle_AllocateEnergyThenTick_…` | **OK / SOFT** | Currency growth from `TickEnergy` is real. **`SkillXp + ProgressionLevel > xpBefore` is a false-green half-assert** (see §False greens). |
| 9 | Melvor Idle | Grind Skills | `MelvorIdle_GrindSkillNode_…` | **OK** | — |
| 10 | Egg, Inc. | Hatch (Tap Burst) | `EggInc_HatchBurst_…` | **OK** | Soul Eggs prestige still out of scope. |
| 11 | Idle Miner | Upgrade Shafts (+ managers) | `IdleMiner_BuyShaftThenHire_…` | **OK / SOFT** | Same as AdvCap: no post-hire sim CPS beat. |
| 12 | Tap Titans 2 | Tap / **Hero DPS** | `TapTitans2_TapKill_…` | **PARTIAL** | Tap-kill + gold + respawn OK. **Hero DPS half of slash verb never exercised** (CH has `CombatDps_Fractional`; TT2 does not). |
| 13 | Idle Heroes | Auto-Combat | `IdleHeroes_AutoCombatDps_…` | **OK** | HeroDps seeded (legitimate arrange). AFK chest + gacha are secondary tests. |
| 14 | AFK Arena | **Auto-Combat** (+ AFK chest) | `AfkArena_SimFillsChestThenClaim_…` | **PARTIAL** | Chest accrual+claim is solid. Sim path is **chest drip / campaign**, **not** `IdleCombatState` auto-combat. Compose MVP (“Push Campaign, Open AFK Chest”) matches tests; matrix label “Auto-Combat” does not. Progress “OK — Auto-Combat” overclaims. |
| 15 | Legend of Mushroom | Rub Lamp | `LegendOfMushroom_RubLampThreeTimes_…` | **OK** | Auto-lamp harden seeds `Stage=1` (fine as follow-on). |
| 16 | Capybara Go! | Step-based Narrative | `CapybaraGo_StepsThenAdvance_…` | **SOFT / SEED-class** | **Causal chain not enforced by system** (see §False greens #1). Harden `FiveSteps` still seeds `ExploreUnlocked=1`. |
| 17 | Cats & Soup | Assign Cats | `CatsAndSoup_AssignCatThenSim_…` | **OK** | — |
| 18 | Neko Atsume | Place Food/Toys | `NekoAtsume_PlaceFood/Toys_…` | **OK** | Check-in harden seeds cats (acceptable isolation of claim path). |
| 19 | Fallout Shelter | Assign Dwellers | `FalloutShelter_AssignDwellerThenSim_…` | **OK** | — |

**Rough tally vs matrix bar:** OK ≈ 12 · OK/SOFT ≈ 4 · PARTIAL ≈ 2 · SOFT/causal miss ≈ 1.  
**Not** round_01’s ~2 OK. **Not** progress doc’s clean 19/19 OK.

---

## Remaining false greens (prioritized)

### 1. HIGH — Capybara “steps then advance” is non-causal

`CapybaraGo_StepsThenAdvance_RaisesLevel` fires action 0 twice (asserts `StokeCount==2`) then action 1 and asserts progression. Comment: “No pre-seed ExploreUnlocked — earn steps then advance tile.”

In `IdleNarrativeActionSystem.ApplyNarrative`, explore/advance gating is:

```csharp
if (narr.ExploreUnlocked == 0 && slice.Archetype == IdleArchetype.ADarkRoom) return;
```

**Only A Dark Room** is gated. Capybara action 1 advances with `ExploreUnlocked=0` and `StokeCount=0`. The step fires are ornamental — deleting them still greens the progression asserts.

**Why it still looks fixed:** impl_09 removed the `ExploreUnlocked=1` seed, which was the round_01 SEED pattern, but did not make steps a precondition of advance.

**Machine-checkable fix:** gate Capybara advance on earned steps / `ExploreUnlocked`, **and** assert advance-without-steps is a no-op in the same fixture.

### 2. MED — NGU XP/level assert rides free `ProgressionLevel=1`

After allocate with `Amount > 0`, system sets `ProgressionLevel = 1` before any tick. Test records `xpBefore = SkillXp` (0), then asserts:

`SkillXp + ProgressionLevel > xpBefore`

Even with **zero** sim ticks this is `0 + 1 > 0`. Currency assert (`PrimaryCurrency` grows) is the real TickEnergy proof; the XP line’s message overclaims.

**Fix:** snapshot `ProgressionLevel`/`SkillXp` **after** alloc (or require `SkillXp` alone to increase; or assert EnergyPool drain).

### 3. MED — Tap Titans 2 incomplete vs matrix “Tap / Hero DPS”

Fixture proves tap-kill parity with CH gold/respawn. It never:

- buys a hero / raises `HeroDps` / `PassiveRate`, or
- pumps `IdleSliceSimulationSystem` to damage via DPS (CH’s `ClickerHeroes_CombatDps_FractionalOverOneSecond` is archetype-CH only).

Progress row “OK — kill + gold + respawn” silently drops the DPS half.

### 4. MED — AFK Arena labeled Auto-Combat, tested as chest/campaign

`IdleSliceSimulationSystem` AfkArena branch: `AfkChestSeconds += dt`, passive currency drip, campaign level from chest time — **no combat component**. Tests match **compose** verbs; progress/matrix “Auto-Combat OK” is a naming false green.

### 5. LOW — AdvCap / Idle Miner: automation without proven income tick

Hire asserts `PassiveRate > 0` and component flags. review_09 AC#4 asked ≥1 sim beat for passive fantasies. Cookie/Egg do `PumpSim`; these two stop at rate field. Broken CPS application after manager unlock would still pass.

### 6. LOW — Named tests that overclaim setup

| Test | Issue |
|------|-------|
| `A5_Reload_OwnedCountSynced_NextBuyUsesGrowthCost` | Manually assigns `gen.OwnedCount = st.OwnedGenerators` in-body — proves cost math **if** sync happened, not that `IdleSliceBootstrap` syncs. |
| `FirePrestige_DoesNotAlsoCreatePhaseEvent_PaperclipsPhaseOnly` | Fires phase then empty prestige; does **not** prove prestige path fails to emit phase events. |
| `CapybaraGo_FiveSteps_RaisesGlobalMultiplier` | Still seeds `ExploreUnlocked=1` (unnecessary for current ungated advance; confuses future gate fixes). |
| `LegendOfMushroom_AutoLamp_PullsAfterStageUnlock` | Seeds `Stage=1` / `PullCount=3` — OK as post-unlock harden, but name implies unlock was earned in-test. |

### 7. LOW — Progress / generator marketing drift

- `docs/idle-toolkit-progress.md`: all 19 rows **OK**; claim “Matrix core-verb + causal beat … **19/19**”.
- `IdleToolkitSliceGenerator` HowTo still emits `EditMode-verified: **19/19**` — flattens fixture green into verb proof (round_01 hygiene issue, still present).

AllSmoke **46/46** NUnit green is real; equating that to matrix verb closure is the remaining confidence bug.

---

## Coverage holes still outside “false green” but load-bearing

| Hole | Notes |
|------|-------|
| Prefab ↔ bootstrap ↔ UI event | Still none in EditMode smoke |
| Multi-slice `TargetSlice` | Kernel covers click only. **Batch BC fires most events without `TargetSlice`** (sole-slice fallback). Buy/hire/narrative/gacha/assign/alloc crosstalk untested. |
| `IdleKernelCorrectnessTests` (5) | Not in `RunAllIdleSmokeAndExit` — offline catch-up / claim no-op / two-slice click can regress without AllSmoke noticing |
| Antimatter nested meta-layers | Explicitly deferred; buy path alone remains PARTIAL vs matrix DNA |
| Egg Soul Eggs / TT2 relics / RG faction 2 | Prestige/second-axis depth still thin |
| PlayerPrefs hygiene | A clears Cookie+Antimatter; BC clears a subset — CH/Paperclips/Egg/etc. prefs not cleared in SetUp/TearDown |

---

## Impl_09 acceptance criteria — re-score

| # | Criterion (from review_09) | Re-score |
|---|----------------------------|----------|
| 1 | Per-title verb / rename substitutes | **Mostly MET** — Melvor/IH/Paperclips/AdvCap/Miner/Realm renamed to real verbs. TT2 DPS half + AFK “Auto-Combat” label still soft. |
| 2 | No seeded skip of asserted outcome | **Mostly MET** — LoM/AFK/Capybara seed removals landed; **Capybara advance still skips causal need for steps** (system ungated). Harden tests still seed intentionally. |
| 3 | Spend asserts on buy/hire/pull/food | **MET** on primary paths exercised |
| 4 | ≥1 `IdleSliceSimulationSystem` beat for passive/AFK/skill/assign/DPS | **PARTIAL** — present for Cookie/Melvor/NGU/Cats/Fallout/AFK/Egg/IH; **missing after AdvCap/Miner hire**; TT2 DPS missing |
| 5 | TT2 parity with CH | **PARTIAL** — gold+respawn yes; buy hero + DPS tick no |
| 6 | Persistence two archetypes | **MET** |
| 7 | Evidence hygiene / honest progress claims | **PARTIAL** — runner split + “don’t equate fixture green” prose exist; **scorecard still paints 19/19 OK** and generator HowTo still says EditMode-verified 19/19 |

---

## Recommended next acceptance criteria (implement later — do not do now)

1. **Capybara causal gate:** advance no-ops unless steps/`ExploreUnlocked` earned; test both fail-closed and success paths.  
2. **NGU assert hygiene:** require `SkillXp` increase or EnergyPool drain independent of free `ProgressionLevel=1`.  
3. **TT2 Hero DPS:** one sim beat with `IdleCombatState.HeroDps` (or buy→DPS) mirroring CH fractional test.  
4. **AdvCap + Miner:** after hire, `PumpSim` and assert `PrimaryCurrency` rises.  
5. **AFK Arena naming:** either add combat path or downgrade progress/matrix marketing to “Campaign + AFK Chest” (compose-aligned).  
6. **Fold `IdleKernelCorrectnessTests` into AllSmoke** (or document exclusion); add ≥1 multi-slice test for buy or narrative.  
7. **Progress scorecard:** demote Capybara / TT2 / AFK / Antimatter from blanket OK until above land; stop generator “19/19 EditMode-verified” as verb proof.

---

## Bottom line

Round_01’s worst false greens (wrong verb, seeded outcomes, zero sim) are largely gone. **Remaining false greens are sharper:** Capybara step→advance is still non-causal under the live narrative gate; NGU’s XP assert is soft; TT2 and AFK Arena are half-matrix; AdvCap/Miner skip income ticks; progress/HowTo still over-advertise 19/19. AllSmoke **46/46** is trustworthy as NUnit green, **not** as full matrix verb closure.
