# Round 06 Implement 04 — Gacha / Narrative / Auto

**Agent:** implement 4/10  
**Source review:** `round_06/review_06_gacha.md`  
**Date:** 2026-08-10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Commit:** local only (never push)

---

## Scope executed

From review priority list:

| # | Item | Status |
|---|------|--------|
| P1 #1 | Minimal identity split — IH hero-id/dupe + LoM gear-slot | **DONE** |
| P2 | Capybara roguelite / pet-gacha | **DEFERRED** (not cheap this turn) |
| P3 | Dead-fallback delete / Play-smoke MCP | **DEFERRED** |

---

## ASSUMPTIONS

1. Quality bar = matrix MVP second-axis DNA beat, not full roster/gear UX — **high** — `docs/project-context.md` + review P1 #1.
2. Shared `TryApplyPull` may stay if archetype fields diverge post-pull — **high** — review acceptance wording.
3. Persist via separate prefs keys (SkillXp/cozy pattern) avoids expanding `TryLoadIdleSlice` signature — **high** — prior R5 persist style.
4. Unity MCP pinned to other project; verify via batchmode EditMode — **high** — doctor/instances.
5. Capybara pet/roguelite is review P2, not P1 — **high** — review priority list; deferred.

---

## Changes

### 1. Identity fields on `IdleGachaState`
- `HeroId`, `DupeCount` (IH DNA; LoM leaves 0)
- `GearSlot` (LoM DNA 1=weapon/2=armor/3=acc; IH leaves 0)

### 2. Post-pull divergence (`IdleGachaPullSystem.ApplyIdentityDna`)
- IH: 4-hero roster cycle; `DupeCount = PullCount - 4` after wrap
- LoM: `GearSlot` from rarity; clears IH fields
- Shared rarity / cost / stage math unchanged

### 3. Persist + bootstrap + HUD
- `IdleGachaIdentityPersist` + `SaveGachaIdentity` / `LoadGachaIdentity` / Clear keys
- Bootstrap load/save on PersistNow + attach
- HUD: IH `Hero | Dupes`; LoM `GearSlot`

### 4. EditMode fixtures
- `IdleHeroes_GachaPull_SetsHeroIdAndDupes`
- `LegendOfMushroom_GachaPull_SetsGearSlot`
- `GachaIdentity_SurvivePersistPrefsRoundTrip`

---

## Acceptance criteria

| Criterion (review_06_gacha P1 #1) | Result |
|----------------------------------|--------|
| Archetype-specific fields asserted by tests | **MET** — 3/3 EditMode Passed |
| Shared `TryApplyPull` may stay if fields diverge post-pull | **MET** — `ApplyIdentityDna` after shared math |

---

## VERIFICATION

```
TASK: Round 06 implement 4/10 — minimal gacha identity DNA split
ROUTE: Executor (parity) against review_06_gacha P1 #1
ASSUMPTIONS: 5 high verified
CHANGES:
 - IdleSliceComponents.cs (HeroId/DupeCount/GearSlot)
 - IdleSliceActionSystems.cs (ApplyIdentityDna)
 - GameProgressData.cs (IdleGachaIdentityPersist + save/load/clear)
 - IdleSliceBootstrap.cs (load/save/attach)
 - IdleSliceUIController.cs (HUD second-axis lines)
 - IdleBatchBCSmokeTests.cs (3 identity fixtures)
 - this receipt
VERIFICATION:
 - Build: Unity 6000.5.5f1 batchmode compiled
 - Run: Logs/IdleGacha-impl04-r6b-TestResults.xml
     testcasecount=3 result=Passed passed=3 failed=0
     IdleHeroes_GachaPull_SetsHeroIdAndDupes => Passed
     LegendOfMushroom_GachaPull_SetsGearSlot => Passed
     GachaIdentity_SurvivePersistPrefsRoundTrip => Passed
 - Broader BC (Logs/IdleGacha-impl04-r6.log): identity 3 Passed;
     unrelated FalloutShelter_PendingAndWorkers flake (12.513 vs 12.5±0.01) noted
STATUS: VERIFIED (P1 identity)
RISKS:
 - Play Mode still UNVERIFIED
 - Capybara pet/roguelite DNA still FAIL (P2 deferred)
 - Dead WithNone-style fallback still present (P3 deferred)
 - Full BC suite may flake on FalloutShelter catch-up tolerance under swarm contention
```

---

## Deviations

- Did not implement Capybara pet/roguelite (P2) — out of cheap P1 scope.
- Did not delete dead fallbacks / Play-smoke (P3).
- Full BC AllSmoke not reclaimed clean (project lock contention + unrelated FalloutShelter assert); targeted identity filter is the acceptance evidence.

## Flash Base

None.
