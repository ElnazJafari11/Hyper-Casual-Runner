# Round 03 Implement 04 — Gacha / Narrative / Auto

**Agent:** implement 4/10  
**Source review:** `round_03/review_06_gacha.md`  
**Date:** 2026-08-10  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Commit:** local only (never push)

---

## Scope executed

From review priority list (P0 only):

| # | Item | Status |
|---|------|--------|
| P0 | Capybara HowTo / cold-start honesty | **DONE** — HowTo says Take Step unlocks auto-tiles; bootstrap stays `ExploreUnlocked=0` |
| P0 | Persist gacha / narrative / AFK extras | **DONE** — Stage/PullCount/BestRarity, RoomOrStep/ExploreUnlocked/SoftCurrency, AfkChestSeconds |

Deferred (review P1+): hero-dupe identity, HUD second-axis, LoM Farm UI polish, roguelite stub, pet gacha, dead-fallback delete, Play-smoke.

---

## ASSUMPTIONS

1. Quality bar = matrix MVP DNA (core verb + automation + one second-axis beat) — **high** — `docs/project-context.md` + review.
2. Keep causal Take Step gate (`ExploreUnlocked=0` bootstrap); honesty via HowTo + EditMode cold-start test — **high** — review option A.
3. Extend existing `SaveIdleSlice` / `TryLoadIdleSlice` optional params (coexist with PendingClaim + cozy Workers/Cats) — **high** — verified on-disk API.
4. Unity MCP was on another project; verify via batchmode AllSmoke — **high** — instances check.

---

## Changes

### 1. Capybara HowTo honesty (P0)
- Generator HowTo: `Take Step unlocks auto-tiles → Next Step / auto-advance → …`
- Prefab `16_CapybaraGo_Steps_Slice.prefab` HowToPlay matched.
- EditMode `CapybaraGo_ColdStart_AutoTilesRequireTakeStep`: ExploreUnlocked=0 → sim alone no advance → action0 → sim advances.

### 2. Persist Stage / Explore / AFK (P0)
- `GameProgressData.SaveIdleSlice` / full `TryLoadIdleSlice` / `ClearIdleSlice`:
  - GachaStage, GachaPulls, GachaRarity
  - NarrStep, NarrExplore, NarrSoft
  - AfkChest
- `IdleSliceBootstrap`: load → restore AfkChestSeconds + gacha/narrative extras; `PersistNow` writes them.
- EditMode:
  - `LegendOfMushroom_Stage_PersistsRoundTrip_AutoLampStillFires`
  - `CapybaraGo_ExploreUnlocked_PersistsRoundTrip`
  - `IdleHeroes_AfkChestSeconds_PersistsRoundTrip`

---

## Acceptance criteria

| Criterion (from review_06_gacha P0) | Result |
|-------------------------------------|--------|
| HowTo mentions unlock/Take Step **or** bootstrap ExploreUnlocked==1 | **MET** — HowTo + prefab |
| EditMode: bootstrap-mirrored 0 → sim no advance → action0 → sim advances | **MET** — ColdStart test |
| Round-trip Stage≥1 survives + auto-lamp still fires | **MET** — LoM Stage persist test |
| Capybara ExploreUnlocked survives reload | **MET** — ExploreUnlocked persist test |
| AfkChestSeconds persists (IH) | **MET** — AfkChest persist test |

---

## VERIFICATION

```
TASK: Round 03 implement 4/10 — Capybara HowTo honesty + persist Stage/Explore/AFK
ROUTE: Executor (parity) against review_06_gacha P0
ASSUMPTIONS: 4 high verified
CHANGES:
 - GameProgressData.cs (gacha/narrative/AFK prefs)
 - IdleSliceBootstrap.cs (load/save extras)
 - IdleToolkitSliceGenerator.cs (Capybara HowTo)
 - 16_CapybaraGo_Steps_Slice.prefab (HowToPlay)
 - IdleBatchBCSmokeTests.cs (ColdStart + 3 persist tests)
VERIFICATION:
 - Build: Unity 6000.5.5f1 batchmode compiled + ran AllSmoke
 - Run: Logs/IdleAllSmoke-Summary.txt → result=Passed pass=72 fail=0
 - Behavior (Logs/IdleAllSmoke-impl04-r3c.log):
     CapybaraGo_ColdStart_AutoTilesRequireTakeStep => Passed
     LegendOfMushroom_Stage_PersistsRoundTrip_AutoLampStillFires => Passed
     CapybaraGo_ExploreUnlocked_PersistsRoundTrip => Passed
     IdleHeroes_AfkChestSeconds_PersistsRoundTrip => Passed
STATUS: VERIFIED
RISKS:
 - Play Mode still UNVERIFIED
 - Concurrent cozy/kernel PersistNow fields share SaveIdleSlice; PersistNow forwards Workers/Cats/Stoke/Wood to avoid wipe
```

---

## Deviations

- Product choice: honesty via HowTo (not bootstrap ExploreUnlocked=1), preserving R2 causal gate from impl_09.
- SoftCurrency persisted with Stage/Explore; Stoke/Wood forwarded via peer cozy keys when present.

## Flash Base

None.
