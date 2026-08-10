# Round 07 Review 06 — Gacha / Narrative / Auto (re-audit post identity DNA)

**Agent:** examine/analyze/review 6/10  
**Scope:** Re-audit Idle Heroes, Legend of Mushroom, Capybara Go! against `docs/idle_mechanics_matrix.md` after Round 06 `impl_04_gacha.md` (minimal IH/LoM identity DNA split)  
**Prior reviews:** `round_01/review_06_gacha_narrative.md` → … → `round_06/review_06_gacha.md`  
**Prior impls:** `round_01/impl_04_gacha.md` … `round_06/impl_04_gacha.md`  
**Date:** 2026-08-10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Mode:** Examine only — no implementation, no push

---

## Verdict

**R6 P1 #1 (minimal identity split) is closed on disk and EditMode-backed** (`commit 8319c9b`). IH now stamps `HeroId` / `DupeCount`; LoM stamps `GearSlot` (1..3); both persist via `IdleGachaIdentityPersist` and surface on HUD. Shared `TryApplyPull` remains; fields diverge in `ApplyIdentityDna` post-roll — matches the review acceptance wording. Automation, Farm hide, checklist honesty, HeroDps attach, and prior persist remain intact.

Remaining matrix debt for this lane is almost entirely **Capybara second-axis** (roguelite + pet/gear gacha), dead fallbacks, optional PersistNow→entity identity fixture, and **Play Mode**.

| Title | R01 | R02 | R03 | R04 | R05 | R06 | R07 fit (MVP bar) | Delta since R06 review |
|-------|-----|-----|-----|-----|-----|-----|-------------------|------------------------|
| Idle Heroes | ~40% | ~72% | ~75% | ~80% | ~88% | ~88% | **~93%** | HeroId + DupeCount DNA + HUD + prefs persist |
| Legend of Mushroom | ~45% | ~65% | ~78% | ~83% | ~86% | ~91% | **~95%** | GearSlot DNA + HUD + prefs persist |
| Capybara Go! | ~30% | ~48% | ~68% | ~76% | ~82% | ~82% | **~82%** | No lane delta (P2 deferred in R6) |

Identity EditMode: **3/3 Passed** (`Logs/IdleGacha-impl04-r6b-TestResults.xml`). Checked-in AllSmoke Summary still **103/103** (likely pre-identity reclaim — BC fixture count is now 64; do not treat 103 as proof identity is in AllSmoke). Play Mode: still **0/19 UNVERIFIED**.

---

## R6 impl_04 acceptance re-check

| Criterion (from `round_06/review_06_gacha.md` P1 #1 / `impl_04_gacha.md`) | On-disk evidence | Status |
|---------------------------------------------------------------------------|------------------|--------|
| Archetype-specific fields asserted by tests | `IdleHeroes_GachaPull_SetsHeroIdAndDupes` (5th pull → HeroId=1, DupeCount=1, GearSlot=0); `LegendOfMushroom_GachaPull_SetsGearSlot` (GearSlot∈[1,3], HeroId/DupeCount=0); both Passed in `IdleGacha-impl04-r6b-TestResults.xml` | **MET** |
| Shared `TryApplyPull` may stay if fields diverge post-pull | `TryApplyPull` → `ApplyIdentityDna(ref gacha, slice.Archetype, rarity)` after shared rarity/cost/stage math | **MET** |
| Persist identity | `SaveGachaIdentity` / `LoadGachaIdentity` / `ClearIdleSlice` deletes `GachaHeroId`/`GachaDupes`/`GachaGearSlot`; bootstrap PersistNow + attach load fields; `GachaIdentity_SurvivePersistPrefsRoundTrip` Passed | **MET** (prefs path) |
| HUD second-axis identity lines | `RefreshStats`: IH `Hero: {HeroId} \| Dupes: {DupeCount}`; LoM `GearSlot: {GearSlot}` | **MET** (no dedicated HUD EditMode assert) |

Deferred from R6 (not claimed): Capybara roguelite/pet (P2), dead-fallback delete (P3), Play-smoke MCP (P3), full AllSmoke reclaim under contention.

---

## Matrix contract (unchanged source of truth)

| Game | Core Verb | Prestige | Automation | Monetization / Fun DNA |
|------|-----------|----------|------------|------------------------|
| **Idle Heroes** | Auto-Combat | None (gacha progression) | 100% Auto (**AFK Chest**) | Hero Dupe Gacha; collection chase |
| **Legend of Mushroom** | Rub Lamp (Gacha Gear) | None (linear stage) | **Auto-Lamp rubbing** | Idle + gacha collapsed into **one verb** |
| **Capybara Go!** | Step-based Narrative | None (run-based roguelite) | **Auto-advancing tiles** | Pet/Gear Gacha; story-you-watch |

---

## Per-game fidelity (post R6 identity DNA)

### 1. Idle Heroes — Auto-Combat + Gacha Power + AFK Chest

**Closed (stable / newly closed)**
- Prior: AFK chest, pull→PassiveRate/HeroDps, combat SoT, stage Max, gacha/AFK/HeroDps persist, HUD Pulls/Rarity/Stage/HeroDps, HowTo honesty.
- **NEW (R6):** `IdleGachaState.HeroId` (1..4 roster cycle) + `DupeCount` (`PullCount - 4` after wrap); GearSlot forced 0; HUD `Hero` / `Dupes`; prefs identity persist + attach restore.

**Still open / residual thinness**
1. **Collection depth is scalar, not a bag** — last `HeroId` + wrap `DupeCount`; no owned-heroes set / named roster UX. MVP DNA beat **PASS**; full collection chase still out of scope.
2. **No literal gacha-pull → PersistNow → entity reload identity fixture** — prefs round-trip + PersistNow save/load wiring exist; entity attach path not EditMode-asserted for identity (same proxy class as earlier HeroDps gap).
3. **Dead `else if` IH/LoM branch without `IdleGachaState`** still in `IdleGachaPullSystem`.
4. **Play Mode UNVERIFIED.**

**MVP acceptance vs matrix**

| Criterion | R01 | R02 | R03 | R04 | R05 | R06 | R07 |
|-----------|-----|-----|-----|-----|-----|-----|-----|
| Auto-combat advances without input | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Gacha is primary progression (not prestige) | PARTIAL | PASS | PASS | PASS | PASS | PASS | PASS |
| AFK chest accumulates while idle | FAIL | PASS | PASS | PASS | PASS | PASS | PASS |
| Hero-dupe / collection second axis | FAIL | FAIL | FAIL | FAIL | FAIL | FAIL | **PASS** (MVP: HeroId+DupeCount) |
| Reload preserves AFK / gacha / HeroDps | — | FAIL | PARTIAL | PARTIAL | PASS | PASS | PASS |
| HUD shows gacha / HeroDps / identity | — | FAIL | FAIL | FAIL | PASS | PASS | **PASS** (+ Hero/Dupes) |

---

### 2. Legend of Mushroom — Rub Lamp + Auto-Lamp + Stage

**Closed (stable / newly closed)**
- Prior: auto-lamp, Stage≥1 loot refund, Farm hide at Stage≥1, checklist/HowTo single-verb, Stage/Pulls/Rarity persist + HUD.
- **NEW (R6):** `GearSlot` = `1 + ((rarity-1) % 3)` (weapon/armor/acc); clears IH fields; HUD `GearSlot`; prefs identity persist + attach.

**Still open / residual thinness**
1. **Gear inventory is last-slot only** — each pull overwrites `GearSlot`; no multi-piece gear bag. MVP “gear-shaped” beat **PASS**.
2. Shared pull formula with IH unchanged (identity post-hook only).
3. Dead fallback branch still present.
4. **Play Mode UNVERIFIED.**

**MVP acceptance vs matrix**

| Criterion | R01 | R02 | R03 | R04 | R05 | R06 | R07 |
|-----------|-----|-----|-----|-----|-----|-----|-----|
| Rub lamp = primary gacha verb | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Linear stage push from pulls | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Auto-lamp automation | FAIL | PASS | PASS | PASS | PASS | PASS | PASS |
| Single-verb collapse (idle⊂lamp) | FAIL | FAIL | PARTIAL | PARTIAL | PARTIAL | PASS | PASS |
| Gear-shaped reward identity | FAIL | FAIL | FAIL | FAIL | FAIL | FAIL | **PASS** (MVP: GearSlot 1..3) |
| Reload keeps Stage / auto-lamp / identity | — | — | FAIL | PASS | PASS | PASS | PASS (prefs identity) |
| HUD shows Pulls / Stage / Rarity / Gear | — | FAIL | FAIL | FAIL | PASS | PASS | **PASS** (+ GearSlot) |

---

### 3. Capybara Go! — Step Narrative + Auto-Tiles + Roguelite

**Closed (stable — unchanged this round)**
- Auto-tiles after Take Step unlock; ColdStart EditMode; ExploreUnlocked (+ RoomOrStep / Soft) persist; HUD RoomOrStep/Soft/Explore; checklist synced.

**Still open**
1. **No run-based roguelite** — no run start/end, branch choice, death/reset.
2. **No pet/gear gacha** — Lucky Find remains `FireClick(2f)`.
3. **Narrative schema still A Dark Room’s** (`Wood` / `StokeCount` / explore gate) — Capybara identity is steps + timer only.
4. Slice-only `%5` Capybara path remains dead alternate (bootstrap always adds `IdleNarrativeState`).
5. **Play Mode UNVERIFIED.**

**MVP acceptance vs matrix**

| Criterion | R01 | R02 | R03 | R04 | R05 | R06 | R07 |
|-----------|-----|-----|-----|-----|-----|-----|-----|
| Step advances progression | PASS | PASS | PASS (gated) | PASS | PASS | PASS | PASS |
| Milestone / narrative beat feel | FAIL | PASS | PASS | PASS | PASS | PASS | PASS |
| Auto-advancing tiles | FAIL | FAIL | PASS | PASS | PASS | PASS | PASS |
| Run-based roguelite structure | FAIL | FAIL | FAIL | FAIL | FAIL | FAIL | **FAIL** |
| Pet/gear gacha | FAIL | FAIL | FAIL | FAIL | FAIL | FAIL | **FAIL** |
| Story-you-watch from cold start | — | — | PARTIAL | PASS | PASS | PASS | PASS |
| Reload keeps Explore / steps | — | — | FAIL | PASS | PASS | PASS | PASS |
| HUD shows RoomOrStep / Explore | — | FAIL | FAIL | FAIL | PASS | PASS | PASS |

---

## Cross-cutting (delta from Round 06)

| Finding | R06 | R07 |
|---------|-----|-----|
| A. Over-shared gacha/narrative components | Open | **Partially mitigated** — shared math OK; IH/LoM fields diverge post-pull; Capybara still shares ADR narrative |
| B. Automation pillar for this lane | Closed | **Still closed** |
| C. UI stats HUD second-axis | Closed | **Still closed** (+ identity lines) |
| D. Dual combat / Auto Fight gold click | Closed | **Still closed** |
| E. Prefab ↔ generator HowTo | Closed | **Still closed** |
| F. Capybara HowTo ↔ bootstrap unlock | Closed | **Still closed** |
| G. Persist gacha/narrative/AFK extras | Closed | **Still closed** (+ identity prefs) |
| H. IH HeroDps reload fidelity | Closed | **Still closed** |
| I. Verification ceiling | AllSmoke 103; Play 0/19; identity unclaimed | Identity **3/3 Passed**; AllSmoke Summary still **103** (stale vs BC+3); Play **0/19** |
| J. Play-smoke checklist vs live verbs | Closed | **Still closed** |
| K. LoM Farm button vs single-verb | Closed | **Still closed** |
| L. IH/LoM identity DNA | Open (P1) | **Closed** — HeroId/DupeCount vs GearSlot |

---

## Priority fix list (for Round 07 implementers — do not implement here)

Ordered by remaining matrix DNA impact vs cost:

1. **[P2] Capybara roguelite stub** — Run counter + one binary choice every N steps (or run end/reset). Acceptance: EditMode asserts run field / choice mutates state; HowTo mentions run beat.
2. **[P2] Pet/gear gacha on Capybara** — Lucky Find → `IdleGachaPullEvent` or tiny pet flag (not flat `FireClick(2f)`). Acceptance: test asserts non-click identity field or gacha pull side-effect.
3. **[P2] Optional:** gacha identity → PersistNow → clear world → attach → HeroId/GearSlot match (closes prefs-only proxy gap on L).
4. **[P3] Delete dead fallbacks** — IH/LoM branch without `IdleGachaState`; slice-only Capybara narrative path.
5. **[P3] Play-smoke** — MCP on Hyper-Casual-Runner; mark slices 13/15/16.
6. **[P3] Reclaim AllSmoke** after identity so Summary count includes the 3 new BC fixtures (evidence hygiene).

No remaining **P0/P1** for IH/LoM matrix automation, cold-start honesty, Stage·Explore·AFK·HeroDps persist, HUD second-axis, LoM single-verb UI, checklist sync, or **minimal identity DNA**. Next work is **Capybara DNA** (and optional persist/fixture hygiene).

---

## Out of scope (noted, not scored)

- AFK Arena chest (working reference).
- A Dark Room narrative (shared system; primary cozy lane).
- Full commercial hero roster / lamp gear inventory UX / story content packs.
- CH/TT2 combat HUD breadth beyond this lane’s IH HeroDps line (combat review owns).
- Deepening DupeCount into per-hero bags or GearSlot into multi-slot inventory past MVP bar.

---

## ASSUMPTIONS

1. Quality bar = matrix MVP DNA (core verb + automation + one second-axis beat), not full clones — **high** — `docs/project-context.md` + prior reviews.
2. R6 P1 #1 acceptance (“archetype-specific fields” + post-pull divergence) is the re-audit baseline; thin scalar DNA still scores PASS at MVP — **high** — review wording + EditMode XML.
3. AllSmoke Summary `pass=103` is stale relative to +3 identity tests and is not used as identity evidence — **high** — BC `[Test]` count 64; dedicated `IdleGacha-impl04-r6b` XML is SoT for identity.
4. Capybara P2 deferred in R6 remains FAIL — **high** — code re-read (`Lucky Find` → `FireClick(2f)`; no run fields).
5. Play Mode matches EditMode systems — **med** — UNVERIFIED (MCP on other project per progress doc).

---

## Sources consulted

- `.agents/idle_swarm/round_06/review_06_gacha.md`
- `.agents/idle_swarm/round_06/impl_04_gacha.md` (identity DNA; commit `8319c9b`)
- `docs/idle_mechanics_matrix.md`
- `docs/idle-toolkit-progress.md`
- `docs/idle-play-smoke-checklist.md`
- `docs/project-context.md`
- `Assets/Scripts/ECS/Components/IdleSliceComponents.cs` (`HeroId` / `DupeCount` / `GearSlot`)
- `Assets/Scripts/ECS/Systems/Idle/IdleSliceActionSystems.cs` (`TryApplyPull` + `ApplyIdentityDna`)
- `Assets/Scripts/ECS/Authoring/IdleSliceBootstrap.cs` (load/save/attach identity)
- `Assets/Scripts/UI/IdleSliceUIController.cs` (HUD + Farm hide)
- `Assets/Scripts/GameProgressData.cs` (`IdleGachaIdentityPersist`)
- `Assets/Scripts/Editor/Tests/IdleBatchBCSmokeTests.cs` (3 identity fixtures)
- `Logs/IdleGacha-impl04-r6b-TestResults.xml` → 3/3 Passed
- `Logs/IdleAllSmoke-Summary.txt` → `pass=103 fail=0` (stale reclaim note)
- Prefabs / checklist: `13_*`, `15_*`, `16_*`; rows 50–53

---

## STATUS

```
TASK: Round 07 re-audit — gacha/narrative/auto after R6 identity DNA
ROUTE: Examine/Review only
ASSUMPTIONS: 4 high verified, 1 med (Play Mode)
CHANGES: wrote .agents/idle_swarm/round_07/review_06_gacha.md only
VERIFICATION:
 - Build: N/A (no code changes)
 - Behavior: code+test+matrix re-read; R6 P1 #1 MET on disk
 - Identity: IdleGacha-impl04-r6b 3/3 Passed; AllSmoke Summary 103 (not identity SoT); Play Mode: UNVERIFIED
STATUS: VERIFIED (review artifact authored)
  Fidelity: IH ~93% / LoM ~95% / Capybara ~82% vs matrix MVP
RISKS: Capybara DNA still FAIL; identity is scalar MVP not collection bags; AllSmoke not reclaimed; Play Mode never exercised
```
