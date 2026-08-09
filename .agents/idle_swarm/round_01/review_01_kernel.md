# Round 01 Review 01 — Idle Kernel

**Agent:** 1/10 examine/analyze/review  
**Scope:** `IdleSliceComponents`, `Assets/Scripts/ECS/Systems/Idle/*`, `IdleSliceBootstrap`, prestige hooks, `GameProgressData` idle persistence  
**Quality bar:** Prototype toolkit — playable 60FPS DOTS MVP slices; Authoring → Components → Systems; no gold-plating past bar. Matrix pillars from `docs/idle_mechanics_matrix.md`.  
**Verdict:** Kernel compiles and EditMode smokes pass, but the idle path has **load-bearing numeric and entity-scope defects**, a **broken offline loop for slice archetypes**, and **prestige/persistence dual-ledger desync**. Smoke tests exercise happy paths that mask these. Critical-lane: DOTS query safety + numerics.

---

## ASSUMPTIONS (reviewer)

1. Quality bar = MVP slice per matrix title (core verb + one progression beat), not full game clones — confidence: high — verified by: `docs/project-context.md`, `docs/idle-toolkit-progress.md`.
2. IdleSlice path (`IdleSliceState` + events) is the live MVP economy; `ProducerComponent`/`ResourceWallet` path is legacy/parallel — confidence: high — verified by: bootstrap never adds `ProducerComponent`; slice systems write `PrimaryCurrency` only.
3. Multiple slices in one world is intended toolkit usage — confidence: med — verified by: enum of 19 archetypes + shared queries with no entity filter.

---

## Defects (ordered by severity)

### D1 — CRITICAL: Prestige always grants ≥1 currency even with empty run
**File:** `Assets/Scripts/ECS/Systems/Idle/PrestigeSystem.cs`  
```csharp
if (converted < 1 && slice.ValueRO.PrimaryCurrency >= 25) converted = 1;
if (converted < 1) converted = 1; // unconditional floor
```
Any enabled `PrestigeEventComponent` awards prestige and resets the run with **zero** spend requirement. Pillar 1 (“prestige as replay acceleration”) becomes “free reset button.” Same soft floor pattern in `IdlePhaseShiftSystem` (≥25 primary forces conversion).

### D2 — CRITICAL: Prestige / phase / buy / click queries are world-global (cross-slice contamination)
Action and prestige systems iterate **all** `IdleSliceState` / `BuyableGenerator` / `IdleManager` / `CurrentRunStats` / wallets on every event, not the event’s owning entity or tagged slice.

Examples:
- `IdleClickProduceSystem`: one click event applies gain to **every** slice.
- `PrestigeSystem`: one prestige event resets **all** slices, clears **all** wallets, bumps **all** `PersistentPlayerStats`, resets **all** producers/gens/managers.
- `IdleBuyGeneratorSystem` fallback nests slice×gen Cartesian products.
- `IdleSliceSimulationSystem` syncs `latestGold` from the **last** slice into **all** `CurrentRunStats`.

Toolkit demos with one slice hide this; any multi-slice / shared-world scene silently corrupts state.

### D3 — CRITICAL: Dual combat DPS tick double-counts kills/gold
**Files:** `IdleSliceBootstrap.cs` (adds `IdleCombatState` for CH/TT2/IdleHeroes), `IdleSliceSimulationSystem.cs`

For combat archetypes the switch calls `TickHeroDps(ref s, dt)` **and** the later `IdleCombatState` loop also applies `HeroDps` and awards gold on kill. Two independent HP bars (`IdleSliceState.EnemyHp` int vs `IdleCombatState.EnemyHp` float) advance in parallel → ~2× kill rate / gold / zone progression vs intended MVP.

### D4 — CRITICAL: `GlobalMultiplier` squared into passive CPS
**Files:** `IdleBuyGeneratorSystem.Purchase`, `IdleSliceSimulationSystem`

Purchase writes:
`PassiveRate = BaseCps * OwnedCount * GlobalMultiplier`  
Simulation then adds:
`PrimaryCurrency += PassiveRate * GlobalMultiplier * dt`  

Effective CPS = `BaseCps * Owned * GlobalMultiplier²`. Prestige that raises `GlobalMultiplier` then compounds incorrectly on every buy (and again every frame). Numerics critical-lane bug; EditMode asserts only `PassiveRate > 0`, not rate identity.

### D5 — HIGH: Persistence desync — owned generators / managers / extras not restored
**Files:** `IdleSliceBootstrap.BuildInitialState` / `PersistNow`, `GameProgressData.SaveIdleSlice` / `TryLoadIdleSlice`

Persisted fields: Primary, Prestige, Mult, Level, Click, Passive, Gens.  
On load, `OwnedGenerators` is restored on `IdleSliceState`, but `AttachArchetypeExtras` always creates `BuyableGenerator` with `OwnedCount = 0`, managers `IsHired = false`, combat/gacha/skill/station/narrative at defaults.

After relaunch:
- UI/state says N generators owned with PassiveRate R
- Next buy prices as if OwnedCount=0 (cheap) while PassiveRate stays inflated
- Managers must be re-hired though progression “remembers” automation via PassiveRate only
- Combat zone / gacha stage / skill level / workers / faction / phase / energy **not saved at all**

Also: `PersistentPlayerStats.PrestigeCurrency` is seeded once at spawn and **never** written back from `IdleSliceState.PrestigeCurrency` on save or prestige — cosmetics path (`CosmeticsShopSystem`) spends a **different ledger**.

### D6 — HIGH: Offline simulation never runs for IdleSlice-only worlds
**File:** `OfflineSimulationSystem.cs`

`RequireForUpdate<ProducerComponent>()` + only credits `ResourceWallet` via producers. Bootstrap slices have wallets but **no** `ProducerComponent`. System disables itself after one init tick without touching `IdleSliceState.PrimaryCurrency` / `PassiveRate`.

`LastIdleUpdateTime` is still stamped by `SaveIdleSlice`, so timestamps look valid while **absence / AFK fantasy (pillar 4) is unimplemented** for the live path. `IdleClaimOfflineSystem` is a fake claim (grants ≥10 even when no claim flag / chest / cats — “demo sample”).

### D7 — HIGH: Prestige dual-ledger + cosmetics spend wrong currency
Prestige updates `IdleSliceState.PrestigeCurrency` but only does `PersistentPlayerStats.PrestigeCurrency += 1.0` (flat +1 per event, not converted amount). Cosmetics deduct from `PersistentPlayerStats`. Slice prestige and cosmetics prestige diverge immediately; runner meta and idle meta collide on the same components.

### D8 — HIGH: Melvor / Cats&Soup double income paths
**Melvor:** switch ticks `SkillXp` every 2s on slice **and** `IdleSkillNode` ticks every 1s with separate XP/level — two progression clocks.  
**Cats & Soup / Fallout Shelter:** switch pays continuous `AssignedWorkers * rate * dt` **and** `IdleAssignmentStation` pays discrete `AssignedCount * OutputPerWorker` — `IdleAssignWorkerSystem` updates both → double pay.

### D9 — MEDIUM: Culture-sensitive double parse on load
`GameProgressData.TryLoadIdleSlice` uses `double.TryParse(string)` without `CultureInfo.InvariantCulture` while save uses `"R"`. Non-en-US locales can fail parse → silent zero / wrong currency after load. Save of `LastIdleUpdateTime` uses invariant `"O"` (good); load of doubles does not match.

### D10 — MEDIUM: Entity lifecycle / domain reload stub
`IdleSliceBootstrap.OnDestroy` has `TODO: [STUB] entity cleanup`. Persist-on-destroy + no destroy entity → leaked entities across play-mode cycles when domain reload is off; possible double-spawn if bootstrap GO recreates while old entity remains.

### D11 — MEDIUM: AdvCap / Miner manager automation model incomplete vs matrix
Matrix: hire managers to **earn** automation after manual labor. Current: buys with `RequiresManager` set PassiveRate only when `auto`; hire flips gen automated. But click path for AdvCap/Miner still pays manual income forever after hire (fine) while there is **no** manual “run business once” producer tick when un-automated — only clicks. Pillar 2 partially missing: no discrete manual production action separate from click.

### D12 — LOW: Dead / parallel economy surface
`IdleProductionSystem`, `IdleShopSystem`, `SynergySystem`, `OfflineSimulationSystem`, `IdleWalletUtil` operate on `ProducerComponent`/`ResourceWallet` and are unused by `IdleSliceBootstrap`. Wallet buffer is added empty and never synced from `PrimaryCurrency`. Increases confusion and prestige blast radius (wallet clear).

### D13 — LOW: Enableable event entities without ownership component
Events are free-floating enableable components; systems `RequireForUpdate<T>` keep systems alive while any enabled event exists. Pattern works for smoke tests but has no slice Entity reference — compounds D2.

---

## Risks

| Risk | Why it matters |
|------|----------------|
| Smoke-test false confidence | Batch A/BC assert increments, not conservation, rates, or single-slice isolation. |
| Prestige farming / soft-lock avoidance | Unconditional prestige (D1) breaks pacing demos and any future balance. |
| Multiplier^2 runaway (D4) | After 1–2 prestiges numbers explode vs intended MVP curves; looks “broken” in play-smoke. |
| Save/load betrayal (D5) | Player buys gens → quit → reload → cheap rebuy + kept PassiveRate = exploit or empty generators with CPS. |
| Offline claim lying (D6) | `HasOfflineClaim` / chest UI will feel fake; Melvor pillar “respects player time” unmet. |
| Cosmetics vs prestige (D7) | Buying skins may no-op or free-unlock relative to displayed idle prestige. |
| Mixing runner + idle in one world | Prestige clears runner wallets/producers and idle slices together. |

---

## Missing mechanics vs quality bar / matrix pillars

Bar = MVP core verb + one progression beat per title (not full clone). Gaps below are **required for honest MVP**, not gold-plate.

| Archetype | Matrix core | Present | Missing / stub vs bar |
|-----------|-------------|---------|------------------------|
| Cookie Clicker | Click → buy gens → heavenly mult | Click, buy, CPS, prestige mult | Prestige should cost a meaningful run; heavenly chips not distinct from generic prestige |
| Clicker Heroes | Tap kill → heroes → zone prestige | Tap + DPS | Double combat (D3); no true zone-reset prestige distinct from generic; Hero Souls unused as spendable |
| AdVenture Cap. | Buy → hire manager → automate | Buy + hire | No manual business cycle; manager is binary flip only |
| Universal Paperclips | Phase-shift loop replace | `IdlePhaseShiftEvent` | Phase ≈ prestige clone; no compute/wire distinct resources |
| A Dark Room | Stoke → unlock explore | Narrative actions | Soft/wood only in `IdleNarrativeState`, not wallet; no fire decay / tension |
| Antimatter Dim. | Nested meta-layers | Phase shift + buy dim | Single layer rename only; no eternity/reality distinction |
| Realm Grinder | Faction alignment | FactionId via energy event | Spells / rebuild variation absent (OK deferred); faction is mult only |
| NGU Idle | Allocate energy → rebirth | Energy tick + allocate | **No rebirth/prestige retention path** dedicated; allocate ≠ rebirth |
| Melvor Idle | Skills + total offline | Skill node + fake claim | Real offline accrual missing (D6); dual XP clocks (D8) |
| Egg Inc. | Hatch burst → soul eggs | Click sets passive floor | No research / soul-egg prestige distinct |
| Idle Miner | Shafts + managers + mine prestige | Gen + manager | No elevator/bottleneck; single shaft |
| Tap Titans 2 | Tap + hero DPS + artifacts | Same as CH | Same double-DPS defect |
| Idle Heroes | Auto combat + gacha | Combat + gacha | AFK chest not wired to real offline; gacha deterministic ladder |
| AFK Arena | AFK chest | Chest timer + claim | Offline claim not time-based from `LastIdleUpdateTime` |
| Legend of Mushroom | Rub lamp gacha | Gacha pull | Auto-rub absent |
| Capybara Go | Step narrative | Narrative / slice-only | Roguelite run structure absent (OK deferred) |
| Cats & Soup | Assign stations | Assign + station | Double pay (D8); cosmetics pillar unused in kernel |
| Neko Atsume | Place food, check-in | Food spend + cat timer | Pure passive absence weak; claim always pays |
| Fallout Shelter | Assign + risk losses | Assign only | **No incident / loss-on-neglect** — matrix “check in or risk losses” absent |

**Pillar score (kernel):**
1. Prestige acceleration — **FAIL** (free prestige, formula weak, dual ledger)
2. Automation as graduation — **PARTIAL** (AdvCap hire exists; no earned manual→auto loop elsewhere)
3. Second axis — **PARTIAL** (faction/energy/narrative/gacha present as thin stubs)
4. Absence valuable — **FAIL** (offline system not on slice path; claim fabricates rewards)
5. Monetization emotion — N/A for kernel gameplay focus

---

## Concrete fix list (file + action) for implementers

Do **not** expand scope into full game clones. Fix kernel correctness first.

### P0 — must fix before trusting play-smoke / persistence

1. **`PrestigeSystem.cs`** — Remove unconditional `converted = 1`. Gate prestige on `converted >= 1` (or `PrimaryCurrency >= threshold`). Scope all mutations to the **event entity** (or `IdleSliceTag` match), not world-wide queries. Sync `PersistentPlayerStats.PrestigeCurrency` from slice converted amount (or stop attaching dual stats on idle entities).
2. **`IdlePhaseShiftSystem.cs`** — Same threshold gate; only affect Paperclips/Antimatter (or event owner); reset matching `BuyableGenerator` on same entity.
3. **`IdleClickProduceSystem.cs` / `IdleBuyGeneratorSystem.cs` / `IdleManagerHireSystem.cs` / all `IdleSliceActionSystems.cs`** — Bind events to a target `Entity` (add `Target` field or put enableable events **on the slice entity**). Query only that entity.
4. **`IdleBuyGeneratorSystem.cs` + `IdleSliceSimulationSystem.cs`** — Pick one multiplier site: either bake mult into `PassiveRate` **or** apply `GlobalMultiplier` in sim, not both. Add EditMode assert: after buy with Mult=2, CPS ≈ `BaseCps * Owned * 2` (±epsilon), not ×4.
5. **`IdleSliceSimulationSystem.cs`** — For archetypes with `IdleCombatState`, **delete** `TickHeroDps` switch path (or remove component and keep slice-only). Single HP/gold authority. Sync one way into UI fields.
6. **`IdleSliceBootstrap.cs` + `GameProgressData.cs`** — On load, set `BuyableGenerator.OwnedCount = gens`, restore `IsHired` / `IsAutomated` from saved flags (extend save schema: ManagersHired, PhaseIndex, FactionId, EnergyAllocated, Zone/Progression extras as needed for MVP). Keep schema lean but **consistent** with runtime components.
7. **`OfflineSimulationSystem.cs`** — Either `RequireForUpdate<IdleSliceState>` and apply `PassiveRate * GlobalMultiplier * cappedSeconds` into `PrimaryCurrency` (cap e.g. 8h), **or** new `IdleOfflineCatchupSystem`. Set `HasOfflineClaim` instead of silent apply if UX wants claim button. Stop depending solely on `ProducerComponent`.
8. **`IdleClaimOfflineSystem.cs`** — Remove free “demo sample” grant when no claim; no-op unless `HasOfflineClaim` or computed offline seconds > 0.

### P1 — correctness / double-pay / ledger

9. **`IdleSliceSimulationSystem.cs`** — Melvor: drive XP **only** from `IdleSkillNode` (remove slice `SkillXp` branch) or only from slice — not both.  
10. **`IdleSliceSimulationSystem.cs`** — Cats/Fallout: pay **only** via `IdleAssignmentStation` **or** only via `AssignedWorkers` continuous, not both.  
11. **`GameProgressData.cs`** — `double.TryParse(..., CultureInfo.InvariantCulture, ...)`. Consider `IdleKey` without redundant cast.  
12. **`IdleSliceBootstrap.cs`** — On destroy / disable: destroy `_sliceEntity` if world alive; guard against double Persist after destroy.  
13. **`CosmeticsShopSystem.cs` / prestige** — Spend from one prestige source (`IdleSliceState` **or** `PersistentPlayerStats`), document which; keep them synced if both must exist.  
14. **`IdleSliceSimulationSystem.cs`** — Stop writing last-slice gold into all `CurrentRunStats`; update only the paired entity.

### P2 — MVP mechanic honesty (thin, still bar-scoped)

15. **`IdleSliceActionSystems.cs` / components** — NGU: add rebirth event that resets energy allocation and retains a permanent stat (can reuse prestige fields with archetype gate).  
16. **`IdleSliceSimulationSystem.cs` / claim** — AFK Arena / Idle Heroes: chest reward from `LastIdleUpdateTime` delta, not only realtime `AfkChestSeconds` while app open.  
17. **`IdleSliceComponents` + sim** — Fallout Shelter: minimal neglect timer that reduces currency or workers if unclaimed beyond N minutes (stub OK if greppable `TODO: [STUB]` with behavior).  
18. **Tests** — Extend Batch A/B: (a) prestige with 0 currency does nothing; (b) Mult not squared; (c) two slices + one click only mutates target; (d) save/load restores `BuyableGenerator.OwnedCount`; (e) offline catchup increases primary with no `ProducerComponent`.

### Explicitly defer (out of bar)

- Full Antimatter nested layer currencies, Realm spells, Miner elevator graph, Capybara roguelite runs, real gacha RNG/pity, co-op Egg Inc contracts, UI Toolkit screens polish.

---

## Stub inventory (greppable)

| Location | Marker / nature |
|----------|-----------------|
| `IdleSliceBootstrap.OnDestroy` | `// TODO: [STUB] entity cleanup on domain reload / destroy` |
| `IdleClaimOfflineSystem` | Implicit stub: grants currency with no offline evidence |
| `OfflineSimulationSystem` | Dead for slice path (Producer-gated) |
| Dual `Producer*` systems in `Systems/Idle/` | Parallel unused economy |

---

## Reviewer confidence

- Defects D1–D8: **high** (read from source; logic is unconditional / double-apply).  
- Multi-slice production risk: **high** if toolkit hosts >1 bootstrap; **med** if scenes are always single-slice.  
- Play-mode impact: **UNVERIFIED** here (progress doc: Play Mode 0/19); EditMode smokes do not cover these failure modes.

---

## Suggested implementer order

1. Event→entity scoping + prestige threshold (D1/D2)  
2. Multiplier single-site + combat single-path (D3/D4)  
3. Persist restore OwnedCount/managers + invariant parse (D5/D9)  
4. Offline catchup on `IdleSliceState` + honest claim (D6/D8 claim)  
5. Remove double Melvor/assign pay (D8)  
6. Tests that fail on today’s kernel, then pass  

**Commit expectation:** implementers land fixes with matching EditMode evidence; this review file is analysis-only.
