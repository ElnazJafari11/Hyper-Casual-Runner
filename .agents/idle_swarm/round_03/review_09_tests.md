# Round 03 Review 09 — EditMode Idle Tests Re-Audit (post R2)

**Agent:** examine 9/10  
**Scope:** `Assets/Scripts/Editor/Tests/IdleBatchASmokeTests.cs`, `IdleBatchBCSmokeTests.cs`, `IdleKernelCorrectnessTests.cs` (+ adjacent HUD/cosmetics) vs `docs/idle_mechanics_matrix.md` + round_02 `review_09` / `impl_09` acceptance bar  
**Mode:** review only — no implementation, no push  
**Prior:** R1 harsh false-green list → R1 impl_09 → R2 re-audit (Capybara non-causal, soft NGU XP, TT2 DPS half, AdvCap/Miner income, AFK naming, 19/19 marketing) → R2 `impl_09_tests.md` (claimed MET on 4 targeted ACs; deferred AdvCap/Miner PumpSim, AFK combat, Kernel-in-AllSmoke)

---

## Verdict

**R2 impl_09 closed the remaining high-severity false greens from round_02 review_09.** Capybara step→advance is now causally gated in system + fixture; NGU XP/level snapshot is after alloc; TT2 has a CH-parity Hero DPS fractional sim; progress/generator no longer sell AllSmoke green as full matrix verb closure.

**Remaining issue is not “fixtures are empty wiring checks.”** It is a **small SOFT/PARTIAL set** already mostly acknowledged in the scorecard, plus **AllSmoke scope gaps** (Kernel + HUD outside runner) and **stale marketing in non-progress docs**.

Evidence cited: fixture + system source on disk (2026-08-10), `Logs/IdleAllSmoke-Summary.txt` → `result=Passed pass=56 fail=0` (20 A + 36 BC). Kernel (9) and `IdleSliceHudSmokeTests` (1) are **outside** AllSmoke.

Treat progress **~17/19 OK** as **approximately honest**. Overstatement vs a strict AC#4 income-tick bar is **~2 titles** (AdvCap/Miner SOFT), not R1’s ~7 MISS/SEED collapse and not R2’s Capybara causal miss.

---

## Round_02 → impl_09 disposition

| Round_02 remaining false green / gap | Post–R2 impl_09 status |
|--------------------------------------|------------------------|
| HIGH — Capybara advance non-causal | **FIXED** — `TryAdvanceStep` gates ADR **and** Capybara; one step sets `ExploreUnlocked`; bootstrap starts locked; primary + FiveSteps assert fail-closed then success; AutoTiles still seeds unlock (intentional isolation) |
| MED — NGU XP rides free `ProgressionLevel=1` | **FIXED** — snapshot after alloc; require `SkillXp` **or** level **beyond** free alloc=1; currency growth still asserted |
| MED — TT2 Hero DPS half missing | **FIXED** — `TapTitans2_CombatDps_FractionalOverOneSecond` mirrors CH |
| MED — AFK labeled Auto-Combat | **ACKNOWLEDGED PARTIAL** — progress/scorecard demoted; compose-aligned chest/campaign fixtures unchanged (correct for MVP) |
| LOW — AdvCap / Miner no post-hire `PumpSim` | **STILL OPEN (deferred by impl_09)** — hire asserts `PassiveRate > 0` only |
| LOW — progress/HowTo 19/19 verb proof | **MOSTLY FIXED** — `docs/idle-toolkit-progress.md` ~17/19; generator STATUS honest. **Residue:** checked-in `docs/idle-play-smoke-checklist.md` still says `EditMode-verified: **19/19**`; matrix blurb still “EditMode-verified MVP slices” without PARTIAL nuance |
| Fold Kernel into AllSmoke | **STILL OPEN (deferred)** |
| Multi-slice buy/narrative/gacha | **STILL OPEN** — Kernel click-only; Cosmetics has TargetSlice isolation; BC events mostly sole-slice / null TargetSlice |

---

## Inventory (current)

| Fixture | Path | `[Test]` count | In AllSmoke? |
|---------|------|----------------|--------------|
| Batch A | `Assets/Scripts/Editor/Tests/IdleBatchASmokeTests.cs` | 20 | Yes |
| Batch B+C | `Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs` | 36 | Yes |
| Kernel | `Assets/Scripts/Editor/Tests/IdleKernelCorrectnessTests.cs` | 9 | **No** |
| HUD | `Assets/Scripts/Editor/Tests/IdleSliceHudSmokeTests.cs` | 1 | **No** |
| Cosmetics | `Assets/Scripts/Editor/Tests/CosmeticsShopTests.cs` | 6 | **No** (economy lane, not matrix verb) |

AllSmoke = **56** NUnit greens. That number is trustworthy as fixture green; matrix verb bar remains the scorecard below.

---

## Matrix core-verb scorecard (R3 re-audit)

Legend: **OK** · **OK/SOFT** · **PARTIAL** · **MISS** · **SEED**

| # | Game | Matrix core verb | Primary test | Score | Remaining issue |
|---|------|------------------|--------------|-------|-----------------|
| 1 | Cookie Clicker | Click → Buy Generators | `CookieClicker_ClickThenBuy_…` | **OK** | — |
| 2 | Clicker Heroes | Tap to Kill → Buy Heroes | `ClickerHeroes_TapKillThenBuyHero_…` | **OK** | (+ fractional DPS / prestige harden) |
| 3 | AdVenture Cap. | Buy Businesses (+ managers) | `AdventureCapitalist_BuyThenHire_…` | **OK / SOFT** | Buy+hire+spend solid. **No post-hire `PumpSim` income beat** (deferred). |
| 4 | Univ. Paperclips | Manufacture → Phase-Shift | `Paperclips_ManufactureThenPhaseShift_…` | **OK** | Nested compute untested |
| 5 | A Dark Room | Stoke → Explore | `ADarkRoom_StokeThenExplore_…` | **OK** | Craft in harden test |
| 6 | Antimatter Dim. | Buy Dimensions (+ nested) | `Antimatter_BuyDimension_…` | **PARTIAL** | Buy+spend only; Cookie-like; no eternity/layer. Injected funds. No CPS sim |
| 7 | Realm Grinder | Build & Align | `RealmGrinder_AlignFaction_…` | **OK** | Build then align; Align no longer free Mult/Level (R2 prestige polish). Faction 2 thin |
| 8 | NGU Idle | Allocate Energy | `NguIdle_AllocateEnergyThenTick_…` | **OK** | Post-alloc XP/level hygiene fixed |
| 9 | Melvor Idle | Grind Skills | `MelvorIdle_GrindSkillNode_…` | **OK** | — |
| 10 | Egg, Inc. | Hatch (Tap Burst) | `EggInc_HatchBurst_…` | **OK** | Soul Eggs out of scope |
| 11 | Idle Miner | Upgrade Shafts (+ managers) | `IdleMiner_BuyShaftThenHire_…` | **OK / SOFT** | Same as AdvCap: rate field, no income tick |
| 12 | Tap Titans 2 | Tap / Hero DPS | `TapTitans2_TapKill_…` + `…CombatDps_…` | **OK** | HeroDps seeded for DPS fixture (legitimate arrange). Buy-hero path still CH-only |
| 13 | Idle Heroes | Auto-Combat | `IdleHeroes_AutoCombatDps_…` | **OK** | Gacha/claim are secondary |
| 14 | AFK Arena | Auto-Combat (+ AFK chest) | `AfkArena_SimFillsChestThenClaim_…` | **PARTIAL** | Chest+campaign match compose; not `IdleCombatState` auto-combat |
| 15 | Legend of Mushroom | Rub Lamp | `LegendOfMushroom_RubLampThreeTimes_…` | **OK** | AutoLamp harden still seeds Stage (fine) |
| 16 | Capybara Go! | Step-based Narrative | `CapybaraGo_StepsThenAdvance_…` | **OK** | Fail-closed + earned unlock + advance |
| 17 | Cats & Soup | Assign Cats | `CatsAndSoup_AssignCatThenSim_…` | **OK** | — |
| 18 | Neko Atsume | Place Food/Toys | `NekoAtsume_PlaceFood/Toys_…` | **OK** | — |
| 19 | Fallout Shelter | Assign Dwellers | `FalloutShelter_AssignDwellerThenSim_…` | **OK** | Station + PendingClaim + claim drain |

**Rough tally:** OK ≈ 14 · OK/SOFT ≈ 2 · PARTIAL ≈ 2 · MISS/SEED (primary) ≈ 0.  
Aligned with progress **~17/19** if SOFT counts as OK for marketing; stricter bar → **~15/19** OK + 2 SOFT + 2 PARTIAL.

---

## Remaining soft greens / holes (prioritized)

### 1. LOW — AdvCap / Idle Miner: automation without proven income tick

Hire proves `PassiveRate > 0` and flags. Cookie/Egg/`PumpSim` paths would catch a broken CPS apply after manager unlock; these two would still green. **Same deferred item as R2 review_09 #5 / impl_09 deferred list.**

**Machine-checkable fix:** after hire, `PumpSim` ≥1s and assert `PrimaryCurrency` rises (mirror Cookie).

### 2. MED (naming) — AFK Arena matrix “Auto-Combat” vs compose chest MVP

Fixtures and progress PARTIAL label are honest. Matrix DNA column still says Auto-Combat. Do **not** treat chest accrual as combat proof. Optional: add combat path **or** leave PARTIAL until product picks a lane.

### 3. MED (scope) — `IdleKernelCorrectnessTests` outside AllSmoke

Kernel now holds load-bearing R2 gates: empty-world offline stamp race, bootstrap-after-offline catch-up, claim Pending vs AFK double-pay, claim-without-evidence no-op, mult-once, two-slice click. **AllSmoke 56/56 can stay green while Kernel regresses.**

Also outside: `IdleSliceHudSmokeTests` (UI Toolkit named tabs/skins — wiring only).

### 4. LOW — Multi-slice / TargetSlice under-tested for action verbs

Batch A helpers set `TargetSlice` on click/buy/hire/phase/prestige. Batch BC `FireNarrative` and most buy/hire/gacha/assign/claim events omit it (sole-slice fallback). Crosstalk for narrative/gacha/assign/alloc remains unproven beyond Kernel click + Cosmetics TargetSlice spend.

### 5. LOW — Claim mid-gate `[1,10)` unasserted in smoke

IH claim test: empty → 0; seeded `AfkChestSeconds=12` + flag → pays. Combat R2 noted soft gate drift for ∈[1,10) without flag. **No EditMode assert** that mid-range without `HasOfflineClaim` pays 0 (Kernel covers evidence-less no-op, not the 1–10s band).

### 6. LOW — Doc residue / marketing drift

| Doc | Issue |
|-----|-------|
| `docs/idle-toolkit-progress.md` | Honest ~17/19; Antimatter + AFK PARTIAL — **good** |
| `IdleToolkitSliceGenerator` HowTo STATUS | Honest ~17/19 — **good** |
| `docs/idle-play-smoke-checklist.md` | Still `EditMode-verified: **19/19**` (stale seed vs generator) |
| `docs/idle_mechanics_matrix.md` | “All 19 rows … EditMode-verified MVP slices” reads as verb closure |

### 7. LOW — Named-test overclaims (unchanged / improved)

| Test | Note |
|------|------|
| `A5_Reload_OwnedCountSynced_…` | Now calls `IdlePrestigeMath.SyncGeneratorOwnedCountFromState` (better than raw field assign). Still **not** full `IdleSliceBootstrap` reload path |
| `FirePrestige_DoesNotAlsoCreatePhaseEvent_…` | Phase then empty prestige; does not prove prestige path cannot emit phase events |
| `CapybaraGo_AutoTiles_…` | Seeds `ExploreUnlocked=1` — OK isolation |
| `LegendOfMushroom_AutoLamp_…` | Seeds Stage/PullCount — OK post-unlock harden |
| TT2/IH DPS fixtures | Seed `HeroDps` — legitimate arrange for combat DPS half |

---

## Coverage holes still outside “false green”

| Hole | Notes |
|------|-------|
| Prefab ↔ bootstrap ↔ UI event | HUD smoke checks named buttons only; no EditMode event→system via UI click |
| Antimatter nested meta-layers | Explicit PARTIAL / deferred |
| Egg Soul Eggs / TT2 relics / RG faction 2 | Prestige second-axis depth thin |
| PlayerPrefs hygiene | A clears Cookie+Antimatter; BC `PrefsUsed` is a subset (RG/NGU/Melvor/AFK/LoM/Cats/Fallout) — Egg/IH/TT2/etc. if they start persisting need list updates |
| Play Mode | Still 0/19 — EditMode green ≠ playable |

---

## Impl_09 (R2) acceptance criteria — re-score

| # | Criterion (R2 recommended) | Re-score |
|---|----------------------------|----------|
| 1 | Capybara causal gate + fail-closed/success | **MET** — system + primary fixture |
| 2 | NGU assert hygiene post-alloc | **MET** |
| 3 | TT2 Hero DPS fractional sim | **MET** |
| 4 | Honest progress / generator labels | **MOSTLY MET** — progress + generator fixed; play-smoke checklist + matrix blurb still soft |
| — | AdvCap/Miner post-hire PumpSim | **DEFERRED / UNMET** (explicit) |
| — | AFK combat or lasting PARTIAL | **PARTIAL retained** (correct) |
| — | Fold Kernel into AllSmoke | **DEFERRED / UNMET** |

---

## Recommended next acceptance criteria (implement later — do not do now)

1. **AdvCap + Miner income tick:** after hire, `PumpSim` and assert `PrimaryCurrency` rises; demote scorecard to OK only after both green.  
2. **Fold Kernel (+ optionally HUD) into `RunAllIdleSmokeAndExit`** (or a sibling `RunIdleCorrectnessAndExit` that CI always runs and progress cites).  
3. **Claim mid-gate:** AfkChestSeconds ∈[1,10), `HasOfflineClaim=false` → payout 0 for IH and/or AfkArena.  
4. **≥1 multi-slice TargetSlice test** for buy or narrative (not only click/cosmetics).  
5. **Doc sync:** regenerate or hand-fix `idle-play-smoke-checklist.md` EditMode line; soften matrix “EditMode-verified” wording to fixture/MVP presence + pointer to progress scorecard.  
6. **AFK Arena product pick:** combat path **or** keep PARTIAL and align matrix DNA wording to “Campaign + AFK Chest”.

---

## Bottom line

Round_02’s sharp remaining false greens (Capybara non-causal, soft NGU XP, missing TT2 DPS, 19/19 verb marketing) are **closed in code and fixtures**. AllSmoke **56/56** is trustworthy NUnit green. Matrix verb bar is **~17/19 OK** with Antimatter + AFK **PARTIAL**, and AdvCap/Miner still **SOFT** on post-hire income. Highest-value leftover work is **income-tick for manager titles**, **Kernel under AllSmoke**, and **checklist/matrix doc residue** — not another round_01-style verb rewrite.
