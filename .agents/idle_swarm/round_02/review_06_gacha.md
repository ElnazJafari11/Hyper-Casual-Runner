# Round 02 Review 06 — Gacha / Narrative / Auto (re-audit post impl_04)

**Agent:** examine/analyze/review 6/10  
**Scope:** Re-audit Idle Heroes, Legend of Mushroom, Capybara Go! against `docs/idle_mechanics_matrix.md` after Round 01 `impl_04_gacha.md`  
**Prior review:** `.agents/idle_swarm/round_01/review_06_gacha_narrative.md`  
**Prior impl:** `.agents/idle_swarm/round_01/impl_04_gacha.md`  
**Date:** 2026-08-10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Mode:** Examine only — no implementation, no push

---

## Verdict

**impl_04’s four required items landed and are EditMode-backed.** Idle Heroes now has AFK chest fill + gacha→HeroDps; Capybara milestone mult fires on the live `IdleNarrativeState` path; LoM has Stage≥1 auto-lamp. Dual-combat / mislabeled Auto Fight were also cleaned (combat/UI harden since Round 01). Matrix DNA for this lane is **no longer under-delivered on the P0 automation/coupling beats**, but second-axis identity (hero dupes, gear, auto-tiles/roguelite) and HUD/prefab polish remain open.

| Title | Round 01 fit | Round 02 fit (MVP bar) | Delta drivers |
|-------|---------------|------------------------|---------------|
| Idle Heroes | ~40% | **~72%** | AFK fill, gacha→DPS, single combat authority, UI verb fix |
| Legend of Mushroom | ~45% | **~65%** | Auto-lamp unlock; still dual-verb + no gear identity |
| Capybara Go! | ~30% | **~48%** | Live-path milestone mult; still no auto-tiles / run / pet gacha |

EditMode AllSmoke: **46/46** (`Logs/IdleAllSmoke-Summary.txt`). Play Mode: still **0/19 UNVERIFIED**.

---

## impl_04 acceptance re-check

| Criterion (from impl_04) | On-disk evidence | Status |
|--------------------------|------------------|--------|
| IH sim dt → `AfkChestSeconds >= N` → claim raises currency + clears chest | `IdleSliceSimulationSystem` case `IdleHeroes` accrues chest; `IdleHeroes_AfkChest_FillsOverSimTime` | **MET** |
| IH pull raises `HeroDps` (not only ClickPower) | `IdleGachaPullSystem` bumps `PassiveRate` + `IdleCombatState.HeroDps`; `IdleHeroes_GachaPull_RaisesHeroDps` | **MET** |
| Capybara 5 steps with `IdleNarrativeState` raises `GlobalMultiplier` | `ApplyNarrative` action 1 `% 5`; `CapybaraGo_FiveSteps_RaisesGlobalMultiplier` | **MET** |
| LoM Stage≥1 + sim time → PullCount increases | `AutoTimer` + sim `TryApplyPull`; `LegendOfMushroom_AutoLamp_PullsAfterStageUnlock` | **MET** |

Deferred by impl_04 (still open): auto-tiles, gear identity, roguelite stub. Dual-combat rename was deferred then but **appears fixed elsewhere** (see below).

---

## Matrix contract (unchanged source of truth)

| Game | Core Verb | Prestige | Automation | Monetization / Fun DNA |
|------|-----------|----------|------------|------------------------|
| **Idle Heroes** | Auto-Combat | None (gacha progression) | 100% Auto (**AFK Chest**) | Hero Dupe Gacha; collection chase |
| **Legend of Mushroom** | Rub Lamp (Gacha Gear) | None (linear stage) | **Auto-Lamp rubbing** | Idle + gacha collapsed into **one verb** |
| **Capybara Go!** | Step-based Narrative | None (run-based roguelite) | **Auto-advancing tiles** | Pet/Gear Gacha; story-you-watch |

---

## Per-game fidelity (post impl_04)

### 1. Idle Heroes — Auto-Combat + Gacha Power + AFK Chest

**Fixed since Round 01**
- AFK chest accrues in simulation for `IdleHeroes` (`AfkChestSeconds += dt`, claim flag ≥10s).
- Gacha success raises `PassiveRate` and `IdleCombatState.HeroDps` by `rarity * 0.5`.
- Single HP authority: when `IdleCombatState` present, archetype switch skips `TickHeroDps`; shared combat foreach owns DPS.
- UI: `Gacha Pull` | `Claim AFK` only — no mislabeled Auto Fight gold click.
- Click path: `IdleClickProduceSystem` Idle Heroes case is no-op (no flat gold).
- Tests: AFK fill, gacha→HeroDps, auto-combat kill, click-no-gold/claim-gated.

**Still open**
1. **No hero collection / dupe meta.** `BestRarity` remains a scalar high-water mark — monetization DNA (“Hero Dupe Gacha”) still FAIL.
2. **HUD omits PullCount / BestRarity / Stage** — Chest/Mult visible; gacha second-axis still opaque.
3. **Shared pull math with LoM** — same cost/rarity/stage%3; IH-specific delta is only the post-pull DPS bump.
4. **Play Mode UNVERIFIED.**

**MVP acceptance vs matrix**

| Criterion | R01 | R02 |
|-----------|-----|-----|
| Auto-combat advances without input | PASS | PASS |
| Gacha is primary progression (not prestige) | PARTIAL | **PASS** (pulls raise combat DPS) |
| AFK chest accumulates while idle | **FAIL** | **PASS** |
| Hero-dupe / collection second axis | **FAIL** | **FAIL** |

---

### 2. Legend of Mushroom — Rub Lamp + Auto-Lamp + Stage

**Fixed since Round 01**
- `IdleGachaState.AutoTimer`; after `Stage >= 1`, simulation auto-calls `TryApplyPull` every 2s when affordable.
- Manual rub remains graduation gate (3 pulls → stage 1) — matches automation-as-earned pillar.
- Generator HowTo updated to mention auto-lamp unlock.
- Test: `LegendOfMushroom_AutoLamp_PullsAfterStageUnlock`.

**Still open**
1. **Prefab HowTo stale.** `15_LegendOfMushroom_Lamp_Slice.prefab` still says “Rub lamp → farm gold → stage progression” — omits auto-lamp (impl_04 deviation: generator only).
2. **Not a single compulsive verb.** UI still splits `Rub Lamp` vs `Farm Stage Gold`. Auto-lamp spends currency, so farm click remains required fuel — closer to “gacha + income click” than lamp-as-everything.
3. **No gear inventory / lamp loot identity** — abstract rarity only.
4. **Shared gacha formula with Idle Heroes** unchanged.
5. **Dead `WithNone<IdleGachaState>` fallback** still present (unreachable under bootstrap).

**MVP acceptance vs matrix**

| Criterion | R01 | R02 |
|-----------|-----|-----|
| Rub lamp = primary gacha verb | PASS | PASS |
| Linear stage push from pulls | PASS | PASS |
| Auto-lamp automation | **FAIL** | **PASS** (Stage≥1, affordable) |
| Single-verb collapse (idle⊂lamp) | **FAIL** | **FAIL** |
| Gear-shaped reward identity | **FAIL** | **FAIL** |

---

### 3. Capybara Go! — Step Narrative + Auto-Tiles + Roguelite

**Fixed since Round 01**
- Milestone mult on **live** path: `ApplyNarrative` action 1, `CapybaraGo && RoomOrStep % 5 == 0` → `GlobalMultiplier += 0.1f`.
- HowTo claim (“milestone mult beats”) now matches bootstrap behavior.
- Test: `CapybaraGo_FiveSteps_RaisesGlobalMultiplier`.

**Still open (unchanged from R01 P1+/P2)**
1. **No auto-advancing tiles.** Simulation only PassiveRate drip (bootstrap 0) — steps remain fully manual.
2. **No run-based roguelite** — no run start/end, branch choice, death/reset.
3. **No pet/gear gacha** — Lucky Find is generic click income.
4. **Narrative schema still A Dark Room’s** (`Wood` / `StokeCount` / explore gate).
5. **Stats HUD** still omits `RoomOrStep` / SoftCurrency.
6. Slice-only `%5` path remains as dead alternate (bootstrap always adds `IdleNarrativeState`).

**MVP acceptance vs matrix**

| Criterion | R01 | R02 |
|-----------|-----|-----|
| Step advances progression | PASS | PASS |
| Milestone / narrative beat feel | **FAIL** | **PASS** (mult on live path) |
| Auto-advancing tiles | **FAIL** | **FAIL** |
| Run-based roguelite structure | **FAIL** | **FAIL** |
| Pet/gear gacha | **FAIL** | **FAIL** |

---

## Cross-cutting (delta from Round 01)

| Finding | R01 | R02 |
|---------|-----|-----|
| A. Over-shared gacha/narrative components | Open | **Still open** — one pull system; IH DPS branch only differentiator |
| B. Automation pillar for this lane | Fail (only IH combat) | **Partial** — IH AFK + LoM auto-lamp done; Capybara auto-tiles missing |
| C. UI stats HUD generator-centric | Open | **Still open** — Mult/Chest shown; Pull/Rarity/Step not |
| D. Dual combat / Auto Fight gold click | Open | **Closed** (sim skip + UI + click no-op) |
| E. Verification ceiling | EditMode thin; Play 0/19 | EditMode stronger (4 impl_04 tests + combat harden); Play still 0/19 |
| F. Prefab ↔ generator HowTo drift | N/A | **Open** — LoM prefab stale vs generator |

---

## Priority fix list (for Round 02 implementers — do not implement here)

Ordered by remaining matrix DNA impact vs cost:

1. **[P0] Capybara auto-tiles** — Sim tick advances `RoomOrStep` on an interval when unlocked; fire milestone mult on auto steps too. Acceptance: EditMode sim dt → RoomOrStep increases without narrative event; after 5 auto steps mult rises.
2. **[P1] LoM prefab HowTo regen** — Regenerate or patch `15_*` HowTo to match generator (“unlock auto-lamp at stage 1”). Acceptance: prefab string contains “auto-lamp”.
3. **[P1] LoM single-verb / income-in-lamp** — Auto-lamp or rub grants enough gold that Farm Stage Gold is optional (or remove/rename secondary). Acceptance: Stage≥1 run with no click events still sustains ≥1 auto-pull over window, or UI drops Farm button.
4. **[P1] Minimal identity split** — LoM: one gear-slot / stage-loot field; IH: one hero-id or dupe counter on pull. Acceptance: distinct component fields asserted by archetype-specific tests.
5. **[P2] Capybara roguelite stub** — Run counter + one binary choice every N steps.
6. **[P2] HUD second-axis** — Show PullCount/BestRarity/Stage for gacha archetypes; RoomOrStep for Capybara.
7. **[P2] Pet/gear gacha surface on Capybara** — Lucky Find → `IdleGachaPullEvent` or tiny pet flag (not flat click gold).
8. **[P3] Delete dead gacha/narrative fallbacks** — `WithNone<IdleGachaState>` branch; slice-only Capybara path if permanently unused.
9. **[P3] Play-smoke** — MCP on Hyper-Casual-Runner; mark slices 13/15/16.

---

## Out of scope (noted, not scored)
- AFK Arena chest (working reference; already green).
- A Dark Room narrative (shared system; not primary titles here).
- Full commercial hero roster / lamp gear UX.

---

## ASSUMPTIONS

1. Quality bar = matrix MVP DNA (core verb + automation + one second-axis beat), not full clones — **high** — `docs/project-context.md` + R01 review.
2. impl_04 four criteria are the re-audit baseline; deferred items stay scored FAIL until implemented — **high** — verified by reading impl receipt + current sources.
3. Dual-combat / Auto Fight fixes may be from combat/UI swarm agents, not only impl_04 — **high** — present on disk; credited as lane closed regardless of author.
4. AllSmoke 46/46 includes the four impl_04 tests — **high** — test names present in `IdleBatchBCSmokeTests.cs`; summary `Logs/IdleAllSmoke-Summary.txt`.
5. Play Mode matches EditMode — **med** — UNVERIFIED (MCP on other project per progress doc).

---

## Sources consulted
- `.agents/idle_swarm/round_01/review_06_gacha_narrative.md`
- `.agents/idle_swarm/round_01/impl_04_gacha.md`
- `docs/idle_mechanics_matrix.md`
- `docs/idle-toolkit-progress.md`
- `docs/project-context.md`
- `Assets/Scripts/ECS/Components/IdleSliceComponents.cs`
- `Assets/Scripts/ECS/Systems/Idle/IdleSliceSimulationSystem.cs`
- `Assets/Scripts/ECS/Systems/Idle/IdleSliceActionSystems.cs`
- `Assets/Scripts/ECS/Systems/Idle/IdleClickProduceSystem.cs`
- `Assets/Scripts/UI/IdleSliceUIController.cs`
- `Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs`
- `Assets/Scripts/Editor/IdleToolkitSliceGenerator.cs`
- Prefabs: `13_*`, `15_*`, `16_*`
- `Logs/IdleAllSmoke-Summary.txt` → `pass=46 fail=0`
- `Logs/IdleBatchBC-Summary.txt` → `pass=29 fail=0`

---

## STATUS

```
TASK: Round 02 re-audit — gacha/narrative/auto after impl_04
ROUTE: Examine/Review only
ASSUMPTIONS: 4 high verified, 1 med (Play Mode)
CHANGES: wrote .agents/idle_swarm/round_02/review_06_gacha.md only
VERIFICATION:
 - Build: N/A (no code changes)
 - Behavior: code+test+matrix re-read; impl_04 P0/P1 MET on disk
 - AllSmoke: pass=46 fail=0; Play Mode: UNVERIFIED
STATUS: VERIFIED (review artifact authored)
  Fidelity: IH ~72% / LoM ~65% / Capybara ~48% vs matrix MVP
RISKS: LoM prefab HowTo drift; Capybara still missing matrix automation pillar
```
