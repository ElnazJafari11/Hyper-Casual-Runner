# Round 01 Review 03 — Combat / Tap-Kill Idles

**Agent:** examine/analyze/review 3/10  
**Scope:** Clicker Heroes, Tap Titans 2, Idle Heroes, AFK Arena  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Quality bar:** Prototype MVP slices (playable core verb + one progression beat). Critical-lane: DOTS systems, numerics, damage.  
**Action:** Review only — do not implement in this round’s examine pass.

---

## Assets in scope

| Slice | Prefab | Archetype | Core systems |
|-------|--------|-----------|--------------|
| Clicker Heroes | `Assets/ToolkitExamples/Idle/02_ClickerHeroes_TapKill_Slice.prefab` | `ClickerHeroes` | tap kill, hero DPS buy, prestige |
| Tap Titans 2 | `Assets/ToolkitExamples/Idle/12_TapTitans2_TapDps_Slice.prefab` | `TapTitans2` | tap + hero DPS, prestige |
| Idle Heroes | `Assets/ToolkitExamples/Idle/13_IdleHeroes_GachaCombat_Slice.prefab` | `IdleHeroes` | auto-combat, gacha, AFK claim |
| AFK Arena | `Assets/ToolkitExamples/Idle/14_AFKArena_Chest_Slice.prefab` | `AfkArena` | campaign drip, AFK chest |

**Primary code:**  
`IdleSliceComponents.cs`, `IdleSliceBootstrap.cs`, `IdleClickProduceSystem.cs`, `IdleSliceSimulationSystem.cs`, `IdleBuyGeneratorSystem.cs`, `IdleSliceActionSystems.cs` (gacha/claim/phase), `PrestigeSystem.cs`, `IdleSliceUIController.cs`  
**Tests:** `IdleBatchASmokeTests.ClickerHeroes_TapKill_AdvancesZone`, `IdleBatchBCSmokeTests.TapTitans2_* / IdleHeroes_* / AfkArena_*`  
**Evidence note:** EditMode smokes PASS in logs, but they never attach `IdleCombatState` — they only exercise the `IdleSliceState` HP path. Live prefabs attach `IdleCombatState` for CH/TT2/IH and therefore run a second combat loop that tests never see.

---

## Defects (severity-ranked)

### P0 — Critical lane / broken runtime contract

#### D1. Dual combat authorities fight each other (CH / TT2 / IH)
**Where:** `IdleSliceSimulationSystem.OnUpdate`  
- Switch arm calls `TickHeroDps(ref IdleSliceState)` for CH/TT2/IH.  
- Later query also ticks every entity with `IdleCombatState` (same entities on live prefabs).

**Effect:** Two independent HP pools + two kill/gold/zone writers per frame.  
- Slice path: HP scale `10 + level*15`, gold `5 + level*3`, uses `PassiveRate` / `ClickPower`.  
- Combat path: HP scale `20 + zone*25`, gold `GoldPerKill`, uses `HeroDps` only.  
Combat path then **overwrites** `slice.EnemyHp/MaxHp` from combat floats, so taps that mutate slice HP are visually/clobber-stomped next frame.

**Acceptance to close:** Exactly one combat authority for these archetypes; taps and passive DPS mutate the same HP; one kill grants gold/zone once.

#### D2. Tap damage never writes `IdleCombatState` (CH / TT2)
**Where:** `IdleClickProduceSystem.ApplyTapDamage` only mutates `IdleSliceState`.  
Bootstrap attaches `IdleCombatState` with its own `EnemyHp` / `TapDamage`.

**Effect:** On prefab play, taps damage the unused/clobbered slice ints while passive combat floats drive zone clears. “Tap / Attack” becomes a weak/noisy side channel instead of the core verb.

**Acceptance to close:** Tap applies `TapDamage * mult * GlobalMultiplier` to the same HP used by hero DPS; kill bookkeeping shared.

#### D3. Buy Hero DPS never updates `IdleCombatState.HeroDps`
**Where:** `IdleBuyGeneratorSystem.Purchase` / `ApplyCombatHeroBoost` only bump `IdleSliceState.PassiveRate` / `OwnedGenerators`.

**Effect:** Prefab combat query uses frozen bootstrap `HeroDps` (CH/TT2 ≈ max(1, PassiveRate), IH = 3). Buying heroes accelerates `TickHeroDps` but **not** the combat-state DPS that also runs — progression feel is incoherent; wall pacing is wrong.

**Acceptance to close:** Hero purchase updates the single DPS field that combat simulation reads (and UI shows).

#### D4. Integer floor turns tiny `dps*dt` into ≥1 HP/frame (`TickHeroDps`)
**Where:** `IdleSliceSimulationSystem.TickHeroDps`  
`s.EnemyHp -= (int)Math.Max(1, dps * dt);`

**Effect:** At 60 FPS, even `PassiveRate = 0.5` deals **≥60 HP/s**, not 0.5. Early zones evaporate; “the wall” (Clicker Heroes DNA) cannot exist. Critical-lane numerics bug.

**Acceptance to close:** Accumulate fractional damage (float/double remainder) or subtract `dps*dt` without `Max(1, …)` per frame; verified with a dt-sensitive unit test (e.g. 1s @ dps=2 clears ~2 HP, not ~60).

---

### P1 — Core verb / matrix mismatch (MVP broken relative to HowTo)

#### D5. Idle Heroes “Auto Fight” is a currency click, not combat
**Where:** `IdleSliceUIController` → `FireClick`; `IdleClickProduceSystem` switch includes only CH/TT2 for `ApplyTapDamage`. `IdleHeroes` falls into `default` → `PrimaryCurrency += gain`.

**Effect:** Button labeled “Auto Fight” prints free gold and bypasses the auto-combat fantasy. Passive `TickHeroDps` + `IdleCombatState` already run; the button invents a third economy.

**Acceptance to close:** Remove misleading click-for-gold **or** make the button a no-op / combat boost that does not grant flat currency; auto progress comes from hero DPS only.

#### D6. Idle Heroes AFK chest never fills
**Where:** `IdleSliceSimulationSystem` IH arm only `TickHeroDps` — no `AfkChestSeconds` / `HasOfflineClaim`. UI still offers “Claim AFK”; `IdleClaimOfflineSystem` grants a **demo 10 gold** when chest empty.

**Effect:** HowTo (“claim AFK chest”) is fake; claim is always free cheese.

**Acceptance to close:** IH accumulates chest time (or stage-based offline flag) like AfkArena; claim requires `HasOfflineClaim` / accrued seconds; no unconditional demo payout for IH.

#### D7. Gacha power does not drive combat DPS component
**Where:** `IdleGachaPullSystem` raises `ClickPower` / `GlobalMultiplier` / `Stage`, never `IdleCombatState.HeroDps` or `PassiveRate`.

**Effect:** Pulls advance stage counters and click power, but the combat-state loop (HeroDps=3) ignores them. Stage vs zone can diverge (`ProgressionLevel = gacha.Stage` vs combat `Zone`).

**Acceptance to close:** Pull updates the single combat DPS/power source; stage/zone remain one progression axis (or explicitly mapped 1:1).

#### D8. Prestige double-fires + ignores `IdleCombatState` (CH / TT2)
**Where:** `IdleSliceUIController.FirePrestige` calls `FirePhase()` **and** adds `PrestigeEventComponent`.  
`PrestigeSystem` always converts **at least 1** prestige even at 0 gold; neither path resets `IdleCombatState` (Zone/HeroDps/EnemyHp/GoldPerKill).

**Effect:** Double conversion risk same frame; prestige cheese at 0 gold; after prestige, combat zone/HP keep old wall while slice resets — broken prestige contract vs matrix (“Hero Souls & Ancients (Zone Resets)” / “Relics & Artifacts”).

**Acceptance to close:** One prestige path for CH/TT2; gate on meaningful currency/zone; reset both slice + combat combat state; souls/relics raise post-prestige tap/DPS mult.

#### D9. AFK Arena “Push Campaign” is flat click income, not campaign progress
**Where:** UI `FireClick` → default currency add. Simulation only drips currency + fills chest. No stage/combat entity.

**Effect:** Acceptable if MVP is **chest-only**, but button text implies campaign push. Matrix lists Auto-Combat; slice name is Chest. Player can farm click gold unrelated to offline fantasy.

**Acceptance to close (pick one, document):**  
(A) Chest-only MVP: rename button / remove click gold; campaign level rises from time or claim milestones.  
(B) Minimal auto-combat: attach combat state, passive kills advance campaign, chest fills from AFK time.

---

### P2 — Persistence, tests, hygiene

#### D10. Persist omits combat/chest fields
**Where:** `GameProgressData.SaveIdleSlice` / `IdleSliceBootstrap.PersistNow` — currency, prestige, mult, level, click, passive, gens only.  
No Zone, EnemyHp, HeroDps, AfkChestSeconds, gacha PullCount/Stage.

**Effect:** Reload loses wall position / chest progress; level alone is a partial ghost.

#### D11. Smoke tests give false confidence on combat
**Where:** Batch A/BC CreateSlice helpers never add `IdleCombatState`. No test for hero buy → DPS, dual-path kill, fractional DPS, IH claim gating, prestige combat reset.

**Effect:** PASS logs do not cover the prefab runtime path (D1–D4).

#### D12. HP type split (`int` on slice vs `float` on combat)
Casting `(int)combat.EnemyHp` truncates; remainder damage discarded when syncing. Reinforces dual-authority smell.

#### D13. Dead / misleading fallback gacha loop for Idle Heroes
`IdleGachaPullSystem` second loop (`WithNone<IdleGachaState>`) still special-cases `IdleHeroes`, but bootstrap always adds `IdleGachaState` — dead code that invites future double-spend if bootstrap regresses.

#### D14. UI stats show slice HP/CPS, not combat Zone/HeroDps/TapDamage
HUD can disagree with the loop that actually kills (when D1 present).

---

## What is OK / keep

- Prefabs exist, archetype enums + HowTo strings match generator defs; Batch A/BC EditMode smokes green for the **narrow** paths they hit.
- Enableable event pattern (click/buy/gacha/claim) is consistent with other idle slices.
- AfkArena chest timer → `HasOfflineClaim` at 10s is a coherent minimal offline fantasy **if** campaign click cheese (D9) is cleaned up.
- Hybrid SFX spawn on tap/gacha matches project hybrid ECS guidance.

---

## Ranked implementer fix list

Implement in order. Each item: [required] unless marked [deferred]. Skip work outside combat/tap-kill four slices unless a shared helper must change.

| Rank | ID | Fix | Pass/fail criteria | Files (likely) |
|------|----|-----|--------------------|----------------|
| 1 | F1 | **Unify combat authority.** Pick `IdleCombatState` as source of truth for CH/TT2/IH. Remove `TickHeroDps` for entities that have combat state (or delete slice HP combat entirely for these archetypes). Sync `ProgressionLevel` ↔ `Zone` once. | Prefab play: one HP bar; one gold grant per kill; tap + DPS same enemy. Grep: no dual kill writers. | `IdleSliceSimulationSystem.cs`, optionally `IdleSliceState` field docs |
| 2 | F2 | **Wire taps into combat state.** `ApplyTapDamage` reads/writes `IdleCombatState` (TapDamage, EnemyHp, Zone, GoldPerKill). | Unit test: CH/TT2 with `IdleCombatState` EnemyHp=5, tap ClickPower≥5 → Zone++ and gold>0 on **combat** component. | `IdleClickProduceSystem.cs` |
| 3 | F3 | **Fix DPS numerics.** Remove per-frame `Max(1, dps*dt)` int floor; use float HP or damage accumulator. | Test: Passive/HeroDps=2, simulate sum(dt)=1 → HP reduced by ≈2 (±0.1), not ≥60. | `IdleSliceSimulationSystem.cs` |
| 4 | F4 | **Sync hero buys → HeroDps.** On purchase for combat archetypes set `HeroDps` from owned gens / PassiveRate (single formula). | Test: buy hero with gold → `IdleCombatState.HeroDps` increases. | `IdleBuyGeneratorSystem.cs` |
| 5 | F5 | **Idle Heroes verb cleanup.** Stop flat-gold “Auto Fight”; ensure AFK chest accrues; gacha bumps HeroDps (or shared power); claim gated. | IH: no click→gold; after 10s sim claim pays chest formula and clears flag; third claim without refill fails or pays 0. | `IdleClickProduceSystem.cs`, `IdleSliceSimulationSystem.cs`, `IdleGachaPullSystem`, `IdleClaimOfflineSystem`, UI |
| 6 | F6 | **Prestige for CH/TT2.** Single event path; reset `IdleCombatState`; no free prestige at 0 gold; keep souls→mult. | Prestige at 0 currency: no-op. Prestige after kills: Zone/HP reset, PrestigeCurrency↑, mult↑ once. | `IdleSliceUIController.cs`, `PrestigeSystem.cs` |
| 7 | F7 | **AFK Arena campaign button honesty.** Chest-only **or** minimal auto-combat (see D9). | Button behavior matches HowTo; EditMode test updated. | UI + sim/bootstrap |
| 8 | F8 | **Tests that attach `IdleCombatState`.** Add CH/TT2/IH cases mirroring bootstrap extras; cover F1–F6. | New/adjusted EditMode tests PASS in batch A/BC runners. | `IdleBatchASmokeTests.cs`, `IdleBatchBCSmokeTests.cs` |
| 9 | F9 | [deferred] Persist Zone/chest/gacha fields. | Round-trip save/load keeps Zone & AfkChestSeconds. | `GameProgressData.cs`, bootstrap |
| 10 | F10 | [deferred] HUD shows Zone / HeroDps / TapDamage / chest ETA. | Stats string includes combat fields for combat archetypes. | `IdleSliceUIController.cs` |
| 11 | F11 | [deferred] Remove dead `WithNone<IdleGachaState>` IH branch. | Dead branch gone or assert-only. | `IdleSliceActionSystems.cs` |

---

## Suggested implementer sequence (minimal)

1. F1 → F2 → F3 → F4 (combat correctness; critical lane).  
2. F5 → F6 → F7 (archetype fantasy).  
3. F8 (lock with tests).  
4. F9–F11 only if time remains.

**Do not** expand into full gacha inventory, artifacts UI, or runner `CombatSystem.cs` (swarm collision) — that file is unrelated to idle tap-kill.

---

## Out of scope / non-goals for this fix wave

- Play Mode MCP verification (editor may not be on this project).  
- Cosmetics / monetization.  
- Refactoring all 19 idle archetypes’ event broadcasting (all-slice queries) — note only if touching shared systems; prefer archetype filters when editing combat paths.

---

## Trace signals for capability map

- Dual-path combat + int-floor DPS are **critical-lane** misses that EditMode green did not catch → smoke tests under-specified (map: “tests that omit live components”).  
- UI labels / HowTo ahead of simulation (IH AFK, Auto Fight) → fantasy drift under parity routing.
)
