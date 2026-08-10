# Round 04 Implement 06 — Gacha / Narrative / Auto

**Agent:** implement 6/10  
**Source review:** `round_04/review_06_gacha.md`  
**Date:** 2026-08-10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Commit:** local only (never push) — see git log tip after this receipt

---

## Scope executed

From review priority list (required + cheap P1):

| # | Item | Status |
|---|------|--------|
| P1 #4 | IH HeroDps reload / attach hardcode | **DONE** — attach `HeroDps = Max(3, PassiveRate)`; cold-start PassiveRate floor 3; EditMode PassiveRate-only + PersistNow paths |
| P1 #2 | HUD second-axis | **DONE** — Pulls/BestRarity/Stage; IH HeroDps; Capybara RoomOrStep/Soft/Explore |
| P1 #3 | LoM single-verb HowTo/checklist | **DONE** — drop “farm gold” as peer verb; checklist auto-lamp row |
| P2 #7 | Checklist honesty (cheap adjacent) | **DONE** — Capybara checklist mentions Take Step unlocks auto-tiles |

Deferred (not cheap / out of MVP budget this turn): hero-dupe identity (P1 #1), LoM Farm button hide at Stage≥1 (HowTo path met), roguelite/pet gacha (P2), dead-fallback delete (P3), Play-smoke (P3).

---

## ASSUMPTIONS

1. Quality bar = matrix MVP DNA (core verb + automation + one second-axis beat) — **high** — `docs/project-context.md` + review.
2. Review P1 #4 accepts PassiveRate restore **or** combat persist on attach — **high** — both present: `Max(3, PassiveRate)` default + combat SoT via `BuildCombatState` / `SaveIdleCombatPersist` (peer combat lane).
3. Aligning IH cold-start `PassiveRate` 2→3 keeps gacha bumps lockstep with HeroDps — **high** — attach floor was already 3.
4. Unity MCP pinned to another project; verify via batchmode EditMode — **high** — doctor/instances.

---

## Changes

### 1. IH HeroDps reload (P1 #4) — required
- `IdleSliceBootstrap` IdleHeroes: `BuildCombatState(..., Max(3.0, statePassiveFromEntity))` (no bare `HeroDps = 3`).
- Cold-start `PassiveRate = 3` for IH so gacha deltas stay aligned.
- EditMode:
  - `IdleHeroes_HeroDps_RestoresFromPassiveRateOnAttach` — PassiveRate prefs only (no combat keys) → attach HeroDps ≥ 7.5
  - `IdleHeroes_HeroDps_SurvivePersistNowReload` — PersistNow → reload → HeroDps ≥ pre-save

### 2. HUD second-axis (P1 #2)
- `IdleSliceUIController.RefreshStats`: gacha Pulls/BestRarity/Stage; IH `| HeroDps:`; Capybara RoomOrStep/Soft/Explore lock.

### 3. LoM / checklist polish (P1 #3 + cheap P2)
- Generator + prefab HowTo: `…unlock auto-lamp at stage 1 (auto-lamp sustains).` (no peer “farm gold”).
- Play-smoke checklist: LoM `Rub Lamp → auto-lamp at Stage≥1`; Capybara `Take Step unlocks auto-tiles → …`.

---

## Acceptance criteria

| Criterion (from review_06_gacha P1) | Result |
|-------------------------------------|--------|
| Round-trip DPS does not reset to bare 3 while PassiveRate reflects gacha | **MET** — PassiveRate-only attach test Passed |
| pull → save → clear → load attach → HeroDps ≥ pre-save | **MET** — PersistNow reload test Passed |
| RefreshStats contains Pull/Rarity/Stage (gacha) + RoomOrStep/Explore (Capybara); optional HeroDps (IH) | **MET** — labels in `RefreshStats` |
| Stage≥1 UI lacks Farm **or** HowTo+checklist no longer list Farm as peer | **MET** — HowTo+checklist path |

---

## VERIFICATION

```
TASK: Round 04 implement 6/10 — IH HeroDps reload + cheap P1 HUD/HowTo
ROUTE: Executor (parity) against review_06_gacha P1 #4 + cheap P1s
ASSUMPTIONS: 4 high verified
CHANGES:
 - IdleSliceBootstrap.cs (IH PassiveRate=3 + HeroDps Max(3,PassiveRate) attach)
 - IdleBatchBCSmokeTests.cs (HeroDps attach + PersistNow tests)
 - IdleSliceUIController.cs (HUD second-axis)
 - IdleToolkitSliceGenerator.cs (LoM HowTo + checklist)
 - 15_LegendOfMushroom_Lamp_Slice.prefab (HowToPlay)
VERIFICATION:
 - Build: Unity 6000.5.5f1 batchmode EditMode (compiled assemblies)
 - Run: Logs/IdleAllSmoke-Summary.txt → result=Passed pass=91 fail=0
 - Behavior (Logs/IdleBatchBC-impl05-r4c.log):
     IdleHeroes_HeroDps_RestoresFromPassiveRateOnAttach => Passed
     IdleHeroes_HeroDps_SurvivePersistNowReload => Passed
     Batch BC result=Passed pass=58 fail=0
 - AllSmoke corroboration: Logs/IdleCombat-impl02-r4d.log → pass=91 fail=0
STATUS: VERIFIED
RISKS:
 - Play Mode still UNVERIFIED (MCP on other project)
 - Hero-dupe / gear / roguelite DNA still FAIL (deferred)
 - Farm Stage Gold button still in LoM UI (HowTo/checklist no longer peer it)
```

---

## Deviations

- Combat-lane `BuildCombatState` / combat prefs (R4-F1) landed in peer commit `adcf6c5`; this lane’s acceptance still holds via PassiveRate default when combat keys are absent.
- Did not hide LoM Farm button at Stage≥1 — HowTo+checklist acceptance path chosen (cheaper, meets review OR).

## Flash Base

None.
