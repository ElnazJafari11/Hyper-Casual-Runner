# Round 02 Review 01 — Idle Kernel Re-Audit

**Agent:** 1/10 examine/analyze/review  
**Scope:** Idle kernel after Round 1 (`review_01` + `impl_07` + peer landings): components, systems, bootstrap, persistence, prestige math  
**Sources:** `.agents/idle_swarm/round_01/review_01_kernel.md`, `.agents/idle_swarm/round_01/impl_07_kernel.md`, current HEAD under `Assets/Scripts/ECS/**/Idle*`, `GameProgressData`, `IdleKernelCorrectnessTests`  
**Quality bar:** Prototype toolkit MVP — Authoring → Components → Systems; no gold-plating  
**Mode:** Review only — no implementation, no push  
**Date:** 2026-08-10  

---

## Verdict

Round 1 closed the **code-path** P0s that EditMode fixtures can see (prestige gate, event `TargetSlice`, Mult-once, single combat HP, OwnedCount/manager restore fields, claim no-op without evidence, cosmetics idle ledger). **Play-path offline catchup is still broken**: `OfflineSimulationSystem` one-shots and stamps `LastIdleUpdateTime` before `IdleSliceBootstrap` spawns, wiping the AFK window. Claim still **double-pays** when PendingClaim and AfkChestSeconds are both filled. Persistence still omits assignment/gacha/narrative extras. Smoke + kernel fixture remain **false confidence** for live AFK.

**Pillar 4 (absence valuable): still FAIL in Play Mode.** Prestige acceleration / Mult / scoping: largely PASS in EditMode.

---

## ASSUMPTIONS (reviewer)

1. Quality bar = MVP slice correctness, not full clones — high — `docs/project-context.md`.
2. Live economy path = `IdleSliceBootstrap` + `IdleSliceState` (not `ProducerComponent`) — high — bootstrap never adds producers.
3. ECS `InitializationSystemGroup` runs before MonoBehaviour `Start` on first frame — high — standard Entities player-loop ordering; defect D14 follows if true.
4. impl_07 claims P0 1–8 “met” based on EditMode AllSmoke / kernel fixture — high — `Logs/IdleKernel-TestResults.xml` 11/11; does **not** prove Play Mode offline.
5. P2 items (NGU rebirth DNA, Fallout neglect, AFK wall-clock chest-only) remain deferred unless they collide with P0 honesty — high — impl_07 explicit defer.

---

## Round 1 P0/P1 scorecard

| ID | Item | Status | Evidence |
|----|------|--------|----------|
| P0#1 | Prestige gate + scope | **MET** | `IdlePrestigeMath.ConvertRunCurrency` returns 0 below threshold; `PrestigeSystem` scopes via `IdleEventTarget`; soft floor only at ≥25 |
| P0#2 | Phase gate + Paperclips/AD only | **MET** | `IdlePhaseShiftSystem` archetype gate + ConvertRunCurrency |
| P0#3 | Event→entity scoping | **MET** (cosmetics excepted) | `TargetSlice` on idle events; UI `ResolveTargetSlice`; sole-slice fallback refuses multi-slice blast |
| P0#4 | Mult single-site | **MET** | `ComputePassiveRate` raw; sim applies Mult once; kernel + Batch A asserts |
| P0#5 | Single combat HP | **MET** | CH/TT2/IH skip `TickHeroDps` when `IdleCombatState` present |
| P0#6 | Persist OwnedCount / managers / extras | **PARTIAL** | Gens/Managers/Phase/Faction/Energy/MgrHired saved+restored; **AssignedWorkers / station / gacha Stage / narrative / SkillXp / PendingClaim omitted** |
| P0#7 | Offline catchup on IdleSliceState | **REGRESSED / INCOMPLETE** | EditMode path works if slice exists first; Play Mode race wipes timestamp (D14); dual CatchUp contracts disagree (D15) |
| P0#8 | Honest claim (no demo grant) | **PARTIAL** | No-op without evidence — MET; **double-pay when both PendingClaim and AfkChestSeconds set** — FAIL (D16) |
| P1#9 | Melvor single XP clock | **MET** | Slice switch empty; `IdleSkillNode` owns grind |
| P1#10 | Cats/Fallout single pay | **MET** | Station-only (no continuous AssignedWorkers pay) |
| P1#11 | InvariantCulture parse | **MET** | `TryParseInvariantDouble` |
| P1#12 | Bootstrap entity destroy | **MET** | `_destroyed` + `DestroyEntity` |
| P1#13 | Cosmetics single prestige ledger | **PARTIAL** | Idle+stats co-located spend slice prestige — MET; **no TargetSlice / first-slice-wins** (D17) |
| P1#14 | Paired CurrentRunStats | **MET** | Per-entity sync in sim/actions |
| P2#15–17 | NGU / AFK closed-app chest / neglect | **DEFERRED** | Still out of bar per impl_07 |
| P2#18 | Broader correctness tests | **PARTIAL** | Kernel fixture 6 cases; **no Play-order offline test, no claim double-pay test, no bootstrap load PassiveRate recompute** |

---

## Remaining defects (ranked)

### D14 — CRITICAL (P0#7 regression): OfflineSimulation stamps time before slices exist

**Files:** `OfflineSimulationSystem.cs`, `IdleSliceBootstrap.cs` (`ApplyPersistedOfflineCatchUp`)

```csharp
// OfflineSimulationSystem.OnUpdate — first Init tick
state.Enabled = false;
// ... foreach IdleSliceState — often empty (bootstrap Start not yet run) ...
GameProgressData.LastIdleUpdateTime = now.ToString("O"); // ALWAYS stamps
```

Play order:

1. Quit stamps `LastIdleUpdateTime = T0` via `PersistNow` / `IdleSaveManager`.
2. Relaunch: `OfflineSimulationSystem` runs once in `InitializationSystemGroup`, finds **zero** slices, still stamps `LastIdleUpdateTime = now`.
3. `IdleSliceBootstrap.Start` → `ApplyPersistedOfflineCatchUp` sees ~0 elapsed → **no PendingClaim / no Primary grant**.

EditMode `OfflineCatchup_IdleSlice_WithoutProducer_SetsPendingClaim` creates the slice **before** updating the system → false PASS.

**Acceptance for fix:** Play-or EditMode test that (a) stamps T−60s, (b) runs OfflineSimulation with **no** slices, (c) then spawns/loads bootstrap with PassiveRate>0, (d) asserts catchup still applied (or OfflineSimulation does not stamp until ≥1 slice processed / RequireForUpdate&lt;IdleSliceState&gt; and stay enabled until then).

---

### D15 — HIGH: Dual offline contracts disagree (CatchUp vs OfflineSimulation)

**Files:** `IdleOfflineCatchUp.cs`, `OfflineSimulationSystem.cs`, bootstrap comment lies

| Path | Non-Melvor | Melvor | AfkChestSeconds |
|------|------------|--------|-----------------|
| `IdleOfflineCatchUp.Apply` | **direct** `PrimaryCurrency +=` | `PendingClaim +=` | untouched |
| `OfflineSimulationSystem` | `PendingClaim +=` for any PassiveRate | same | `+= min(elapsed, 3600)` |

`IdleOfflineCatchUp` XML doc still claims OfflineSimulation is Producer-only — **stale / wrong**.

If both ever run on the same delta (or claim stacks Pending + chest seconds), economy desyncs by archetype. Unify on one authority + one bank style (prefer PendingClaim + claim button for AFK fantasy).

---

### D16 — HIGH (P0#8 incomplete): Claim double-pays PendingClaim + AfkChestSeconds

**File:** `IdleClaimOfflineSystem.cs`

```csharp
if (pending > 0) { Primary += pending; PendingClaim = 0; }
reward = AfkChestSeconds * (1.0 + ProgressionLevel) * GlobalMultiplier;
reward += CheckInCats * 5.0;
Primary += reward;
```

`OfflineSimulationSystem` sets **both** `PendingClaim = rate*mult*t` **and** `AfkChestSeconds += t`. One claim then pays CPS earnings **and** a second chest formula on the same wall-clock — not “honest claim,” inflation.

Also: `hasClaim` is true when `AfkChestSeconds >= 1f` alone, so any offline bump ≥1s unlocks claim even if PendingClaim were cleared.

**Fix direction:** For CPS offline, credit **only** PendingClaim (do not bump AfkChestSeconds), **or** chest-only archetypes (AFK Arena / Idle Heroes / Neko) use chest/cats and leave PendingClaim unused — never both for the same elapsed seconds.

---

### D17 — HIGH: Cosmetics still world-first-slice (P1#13 / D2 leftover)

**Files:** `CosmeticsShopSystem.cs`, `CosmeticPurchaseEventComponent.cs`

No `TargetSlice`. Query takes first `IdleSliceState`+`PersistentPlayerStats` pair and `break`s. Multi-slice / hub scenes spend the wrong prestige ledger. Runner-only path OK.

---

### D18 — HIGH (P0#6 incomplete): Load restores gens but not live extras that drive MVP verbs

**Files:** `GameProgressData.SaveIdleSlice` / `TryLoadIdleSlice`, `IdleSliceBootstrap.AttachArchetypeExtras`

Still **not** persisted / restored:

| Extra | Archetypes hurt |
|-------|-----------------|
| `IdleAssignmentStation.AssignedCount` + `AssignedWorkers` | Cats & Soup, Fallout Shelter — reload = empty stations |
| `IdleGachaState.Stage` / `PullCount` / `BestRarity` | LoM auto-lamp gated on `Stage >= 1` but Stage always respawns **0** even if `ProgressionLevel` restored → unlock lost |
| `IdleNarrativeState` (wood/soft/room/stoke) | A Dark Room / Capybara |
| `SkillXp` (intra-level) | Melvor — level from ProgressionLevel only |
| Combat `EnemyHp`/`GoldPerKill` | CH/TT2/IH — Zone≈ProgressionLevel OK; HP always 20 (acceptable MVP) |

Also: loaded `PassiveRate` is trusted as-is. No recompute via `IdlePrestigeMath.ComputePassiveRate(BaseCps, Owned)` gated by `IsAutomated`. Stale Mult² saves or mgrHired=false + PassiveRate&gt;0 can reintroduce wrong CPS. Batch A comments cite `SyncGeneratorOwnedCountFromState` — **method does not exist**; bootstrap only sets `OwnedCount = _loadedGens` at attach.

---

### D19 — MEDIUM: Prestige soft floor at 25 still grants 1 with tiny runs

**File:** `IdlePrestigeMath.ConvertRunCurrency`

`floor(sqrt(c/50))` plus `if (converted < 1 && primary >= 25) converted = 1`. Meets “no free prestige at 0” but Angel/Soul prestige at 25 gold is still a soft farm button. Matrix pacing demos may want a higher threshold or no soft floor (pure sqrt only). Not a Round-1 unconditional bug; tune under prestige honesty if demos look broken.

---

### D20 — MEDIUM: Ephemeral systems always `DestroyEntity` on events

**Files:** buy / click / hire / assign / gacha / claim / phase systems

Safe while UI creates **new** event entities. Bootstrap keeps enableable `PrestigeEventComponent` **on** the slice and correctly skips destroy when `IdleSliceState` present. Any future “enable event on slice entity” for buy/claim will destroy the slice. Prefer disable-only when event entity == slice, or never attach action events to the slice.

---

### D21 — LOW: Parallel Producer* economy still dead for slices

**Files:** `IdleProductionSystem`, `IdleShopSystem`, `SynergySystem`, `IdleWalletUtil`

Unchanged from Round 1 D12. Wallet buffer on slice stays empty. Prestige clears wallet on slice only (scoped) — blast radius reduced. Confusion remains; defer cleanup unless it blocks toolkit demos.

---

### D22 — LOW: Tests do not cover Play-order offline or claim conservation

**File:** `IdleKernelCorrectnessTests.cs`

Missing gates that would have caught D14/D16:

- OfflineSimulation with empty world then bootstrap load still accrues.
- Claim after offline: `ΔPrimary ≈ PendingClaim` only (no AfkChestSeconds stack).
- Bootstrap load: `BuyableGenerator.OwnedCount == Gens` **and** `PassiveRate == ComputePassiveRate` when automated.
- Cosmetics with two idle slices + TargetSlice (once field exists).

---

## Risks

| Risk | Why |
|------|-----|
| EditMode green, Play AFK empty | D14 — players/demo “offline claim” never fills after relaunch |
| Claim button feels broken OP | D16 — Pending + chest on same seconds |
| Cats/Fallout / LoM demo betrayal on reload | D18 — workers/auto-lamp forgotten |
| Cosmetics in multi-slice hub | D17 — wrong ledger |
| Stale PassiveRate prefs | D18 — Mult² or manager desync from old sessions |

---

## Pillar score (kernel, post–Round 1)

1. Prestige acceleration — **PASS** (gated; scoped; soft floor at 25 remains soft)  
2. Automation as graduation — **PARTIAL** (hire path OK; AdvCap still click≠discrete business cycle — Round 1 D11, still true)  
3. Second axis — **PARTIAL** (thin stubs; NGU rebirth DNA still P2)  
4. Absence valuable — **FAIL** (D14 play race; D15/D16 claim math)  
5. Monetization emotion — N/A kernel  

---

## Ranked fix list (implementers)

Do **not** expand into full clones. Fix play-path offline + claim conservation first.

### P0 — must fix before trusting AFK / persistence demos

1. **`OfflineSimulationSystem.cs`** — Do **not** stamp `LastIdleUpdateTime` unless catchup applied to ≥1 `IdleSliceState`, **or** `RequireForUpdate<IdleSliceState>()` and keep system enabled until first successful slice pass. Prefer: leave timestamp alone when query empty so bootstrap CatchUp can consume T0.
2. **Unify offline authority** — Pick one: bootstrap `IdleOfflineCatchUp` **or** `OfflineSimulationSystem` for slices; delete/disable the other for IdleSlice path. Single bank style: `PendingClaim` + `HasOfflineClaim` (not direct Primary for Cookie while Pending for Melvor).
3. **`IdleClaimOfflineSystem.cs`** — Mutually exclusive rewards: if `PendingClaim > 0`, pay pending only and clear claim flags; else pay chest/cats formula. Stop bumping `AfkChestSeconds` in CPS offline catchup.
4. **`IdleSliceBootstrap` load** — After attach: recompute `PassiveRate = IsAutomated ? ComputePassiveRate(BaseCps, OwnedCount) : 0`; restore `IdleGachaState.Stage` from ProgressionLevel (or persist Stage); restore `AssignedWorkers` / station `AssignedCount` (extend PlayerPrefs lean fields).
5. **Tests** — Add EditMode cases for D14 ordering, D16 conservation, bootstrap PassiveRate recompute; keep existing kernel 6 green.

### P1 — correctness leftovers

6. **`CosmeticPurchaseEventComponent` + `CosmeticsShopSystem`** — Add `TargetSlice`; resolve via `IdleEventTarget`; no first-slice-wins.
7. **Persist SkillXp / narrative soft+wood+room** (optional lean keys) if those slices are in smoke demos; else document as deferred with greppable stub.
8. **Event destroy safety** — Shared helper: destroy ephemeral event entities only when `!HasComponent<IdleSliceState>(entity)`.
9. **Docs/comments** — Fix `IdleOfflineCatchUp` Producer-only claim; remove phantom `SyncGeneratorOwnedCountFromState` comment in Batch A or implement the helper and call it from bootstrap.

### P2 — still deferred (bar-honest MVP later)

10. NGU dedicated rebirth retention (energy reset + permanent stat).  
11. Fallout neglect timer (`TODO: [STUB]` OK if behavior exists).  
12. Pure sqrt prestige (drop ≥25 soft floor) if demo pacing demands it.  
13. Dead Producer* system cleanup / wallet sync.

---

## Stub inventory (current)

| Location | Nature |
|----------|--------|
| `IdleSliceUIController` button styles | `TODO: [STUB]` |
| `IdleOfflineCatchUp` header | Stale “Producer-only OfflineSimulation” comment |
| Batch A A5 comment | References non-existent `SyncGeneratorOwnedCountFromState` |
| P2 neglect / NGU rebirth | Still absent (intentional defer) |

---

## What Round 1 got right (do not re-break)

- Prestige/phase threshold via `IdlePrestigeMath` (0 currency = no-op).  
- `IdleEventTarget` + UI `TargetSlice` (FindSoleSlice before foreach).  
- PassiveRate raw + Mult in sim once.  
- Combat single HP when `IdleCombatState` present.  
- Melvor skill-node-only; Cats/Fallout station-only pay.  
- Invariant double parse; bootstrap destroy; FirePrestige no longer double-fires phase.  
- Cosmetics prefer `IdleSliceState.PrestigeCurrency` when co-located.

---

## Suggested implementer order

1. Fix OfflineSimulation timestamp race (D14) + unify CatchUp (D15)  
2. Claim conservation (D16) + tests  
3. Load recompute PassiveRate + gacha Stage + workers (D18)  
4. Cosmetics TargetSlice (D17)  
5. Event destroy guard + comment cleanup  

**Commit expectation:** implementers land with EditMode evidence including a **slice-after-offline-system** ordering test; this file is analysis-only. Never push from review agents.

---

## Reviewer confidence

- D14–D16: **high** (read from source; ordering + claim arithmetic unconditional).  
- D17–D18: **high** (missing fields / first-slice loop visible).  
- Play Mode AFK empty: **high confidence defect, UNVERIFIED in-editor this pass** — confirm with one Play relaunch after PersistNow if disputed.  
- impl_07 “P0 1–8 met”: **EditMode met / Play P0#7–8 not met**.
