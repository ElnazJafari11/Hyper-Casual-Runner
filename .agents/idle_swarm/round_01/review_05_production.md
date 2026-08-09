# Round 01 Review 05 — Production / Offline / Managers

**Agent:** 5/10 (EXAMINE / ANALYZE / REVIEW)  
**Scope:** Melvor Idle, Egg, Inc., Idle Miner Tycoon, offline rewards, automation/managers  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Mode:** Read-only analysis — no implementation, no push  
**Quality bar:** Prototype / toolkit MVP (`docs/project-context.md`); matrix DNA in `docs/idle_mechanics_matrix.md`

---

## ASSUMPTIONS

1. Toolkit idle MVPs under `IdleArchetype` + `IdleSliceState` are the in-scope production surface (not runner slices) — **high** — verified via `IdleSliceBootstrap`, `IdleToolkitSliceGenerator`, progress doc.
2. Generic `ProducerComponent` + `OfflineSimulationSystem` is a parallel legacy/kernel path, not wired into Melvor/EggInc/IdleMiner prefabs — **high** — bootstrap never adds `ProducerComponent`; systems `RequireForUpdate<ProducerComponent>()`.
3. Quality bar allows shallow MVP verbs if EditMode smoke passes; “total offline progression” remains a matrix pillar and is in-scope for gap calling — **med** — progress marks Batch BC PASS / Play Mode Pending.
4. `IdleManagerHireSystem` is intentionally shared between AdVenture Capitalist and Idle Miner — **high** — bootstrap + comment + tests.

---

## Verdict (one line)

**Ship-as-MVP verbs exist for all three titles, but offline is theater for Melvor and wall-clock catch-up never reaches `IdleSliceState`; manager prestige leaves automation permanently unlocked.**

---

## Architecture map (production surface)

Two production kernels coexist:

| Kernel | Components | Online tick | Offline / catch-up | Used by Melvor / Egg / Miner? |
|--------|------------|-------------|--------------------|-------------------------------|
| **A — Generic producers** | `ProducerComponent`, `ResourceWallet` | `IdleProductionSystem` | `OfflineSimulationSystem` (UTC delta × rate, once, then disables) | **No** — not spawned by `IdleSliceBootstrap` |
| **B — Toolkit slices** | `IdleSliceState`, `BuyableGenerator`, `IdleManager`, `IdleSkillNode` | `IdleSliceSimulationSystem` (+ click/buy/hire events) | `IdleClaimOfflineSystem` (claim event / AFK chest / demo grant) | **Yes** |

Persistence (`GameProgressData.SaveIdleSlice` / `LastIdleUpdateTime`) writes a timestamp and rates but **does not apply elapsed-time accrual on load** into `PrimaryCurrency` / skill XP. Timestamp is only consumed by Kernel A.

```
Online (Kernel B):
  IdleClickProduceSystem / IdleBuyGeneratorSystem / IdleManagerHireSystem
       → IdleSliceState.PassiveRate / OwnedGenerators / ManagersHired
  IdleSliceSimulationSystem
       → PrimaryCurrency += f(archetype, PassiveRate|skills|workers) * dt

Offline claim (Kernel B):
  UI FireClaim → IdleClaimOfflineEvent → IdleClaimOfflineSystem
       → reward from AfkChestSeconds / CheckInCats / else demo +10

Wall-clock (Kernel A only):
  LastIdleUpdateTime → OfflineSimulationSystem → ResourceWallet on ProducerComponent
```

---

## Matrix vs implementation (scoped titles)

| Title | Matrix promise | Toolkit realization | Gap severity |
|-------|----------------|---------------------|--------------|
| **Melvor Idle** | Grind skills; **total offline progression**; linear sandbox | Online skill tick + Train Skill click; Claim Offline button | **High** — claim does not simulate absence |
| **Egg, Inc.** | Hatch → habitats/vehicles → soul eggs | Hatch burst, habitat buy (shared CPS gen), generic prestige | **Low–Med** — no research/vehicles/contracts (acceptable MVP) |
| **Idle Miner Tycoon** | Shafts/elevators; **super-managers**; prestige mines | Single shaft + shared manager hire + prestige reset | **Med–High** — manager/prestige flag bug; no bottleneck chain |

---

## Findings (ordered by severity)

### P0 — Manager hire permanently disables the manager gate (Idle Miner + AdvCap)

**Where:** `IdleManagerHireSystem` sets `RequiresManager = false` and `IsAutomated = true`.  
**Prestige:** `PrestigeSystem` resets `OwnedCount`, `IsHired`, `ManagersHired`, `PassiveRate` — **does not** restore `RequiresManager` / clear `IsAutomated`.

**Effect:** After one hire + one “New Mine” prestige, the next shaft purchase auto-grants `PassiveRate` without re-hiring. Breaks pillar 2 (“Automation as an Earned Graduation”) for Idle Miner Tycoon and AdVenture Capitalist.

**Acceptance check for a future fix:** After prestige, `BuyableGenerator.RequiresManager == true`, `IsAutomated == false`, `IdleManager.IsHired == false`; buying shaft alone must leave `PassiveRate == 0` until hire.

---

### P0 — Melvor “Claim Offline” is a demo grant, not offline simulation

**Where:** `IdleClaimOfflineSystem` — if `!HasOfflineClaim && AfkChestSeconds < 1 && CheckInCats <= 0`, grant `10 * GlobalMultiplier`.

**Melvor sim:** `IdleSliceSimulationSystem` Melvor branch never sets `HasOfflineClaim` or `AfkChestSeconds`. UI still exposes “Claim Offline” (`IdleSliceUIController`).

**Effect:** HowTo (“Skills tick offline/online → claim offline”) and matrix “Total Offline Progression” are unmet. Claim is always the demo branch for Melvor.

**Also:** Kernel A (`OfflineSimulationSystem`) cannot backfill Melvor because no `ProducerComponent` exists on the slice.

---

### P1 — Wall-clock offline accrual disconnected from toolkit slices

**Evidence:**

- `SaveIdleSlice` / `IdleSaveManager` / bootstrap persist `LastIdleUpdateTime` + `PassiveRate`.
- On load, `TryLoadIdleSlice` restores rates/currency **as last saved**, with **zero** `deltaSeconds * PassiveRate` (or skill ticks).
- `OfflineSimulationSystem` only iterates automated `ProducerComponent` entities, then `state.Enabled = false`.

**Effect:** Egg Inc. / Idle Miner passive CPS and Melvor skill grind do not progress while the app is closed. Offline fantasy is limited to AFK-chest-style claim (AfkArena/Neko) or the Melvor demo grant.

**Suggested contract (for implementers, not done here):** On bootstrap load, if `LastIdleUpdateTime` parses and `PassiveRate > 0` (or Melvor skill active), apply capped catch-up to `PrimaryCurrency` / skill XP, then set `HasOfflineClaim` or show a claim modal. Cap policy must be pinned (e.g. 8h Melvor-style).

---

### P1 — Melvor double production while online

**Where:** `IdleSliceSimulationSystem`:

1. Melvor `switch` arm: continuous currency `(0.5 + ProgressionLevel * 0.1) * mult * dt` **and** Timer-based `SkillXp` / level on the slice.
2. Shared `IdleSkillNode` loop (bootstrap attaches active skill): per tick `Xp += 5`, `PrimaryCurrency += 1 * mult`, level-up writes `ProgressionLevel`.

**Effect:** Two independent grind clocks; UI “Train Skill” click is a third income path. MVP may tolerate this, but rates are not a single skill model and offline catch-up would need to pick one authority.

**Note:** Melvor `PassiveRate` is set to `0.5` at bootstrap / load but the Melvor switch arm **ignores** `PassiveRate` for currency (hardcoded formula). Persisted PassiveRate is misleading for this archetype.

---

### P2 — Idle Miner is AdvCap with different labels (acceptable MVP, weak matrix fidelity)

**Present:** Upgrade shaft (`IdleBuyGenerator`), Hire Super-Manager (`IdleHireManager`), Collect Shaft (manual click using `OwnedGenerators`), New Mine (`PrestigeSystem`).

**Absent vs matrix:** Elevator / warehouse / mainland spatial bottlenecks; manager rarity/gacha; multi-shaft graph. Quality bar does not require full IMT — flag only so implementers do not pretend spatial optimization exists.

**Manual collect:** `IdleClickProduceSystem` IdleMiner branch pays even when not automated — good “earned graduation” precursor **if** P0 flag reset is fixed.

---

### P2 — Egg Inc. habitat automation is immediate CPS (no manager graduation)

Bootstrap: Egg Inc. gets `BuyableGenerator` with `RequiresManager = false`, `IsAutomated = true`. Hatch floors `PassiveRate` at `0.5`. Habitat buy overwrites PassiveRate via shared purchase path.

Soul prestige maps to generic `PrestigeSystem` (sqrt convert → `PrestigeCurrency` / `GlobalMultiplier`). Fine for MVP; not Egg Inc. research permanence, but measurable acceleration exists.

---

### P3 — Claim system is global-broadcast and uncapped

`IdleClaimOfflineSystem` applies to **every** `IdleSliceState` when any claim event fires. Multi-slice worlds would cross-contaminate. Reward has no max-offline-hours clamp (`AfkChestSeconds * (1 + level) * mult`). Prototype-ok; dangerous if reused beyond single-slice prefabs.

---

### P3 — Test coverage gaps for this scope

| Test | Covers | Missing |
|------|--------|---------|
| `MelvorIdle_TrainSkillClick_GainsCurrency` | Click → currency | Skill node tick, claim offline, wall-clock catch-up |
| `EggInc_HatchBurst_RaisesCurrencyAndPassive` | Hatch → PassiveRate | Habitat buy, prestige soul eggs |
| `IdleMiner_HireManager_AutomatesShaft` | Hire → PassiveRate | Prestige re-locks manager; buy-without-hire PassiveRate==0 |
| `AfkArena_ClaimChest_GrantsCurrency` | Claim with flags set | Melvor claim path; empty-claim demo branch |

No EditMode test exercises `OfflineSimulationSystem` against toolkit slices (would fail RequireForUpdate without producers).

---

## File index (absolute paths)

| Role | Path |
|------|------|
| Archetypes / managers / claim events | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Components\IdleSliceComponents.cs` |
| Generic producer / wallet | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Components\IdleComponents.cs` |
| Online CPS / Melvor / Egg / Miner ticks | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleSliceSimulationSystem.cs` |
| Hatch / Collect Shaft | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleClickProduceSystem.cs` |
| Buy habitat/shaft + hire manager | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleBuyGeneratorSystem.cs` |
| Claim offline | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleSliceActionSystems.cs` (`IdleClaimOfflineSystem`) |
| Prestige / mine reset | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\PrestigeSystem.cs` |
| Wall-clock producers | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\OfflineSimulationSystem.cs` |
| Interval producers | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleProductionSystem.cs` |
| Bootstrap extras | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Authoring\IdleSliceBootstrap.cs` |
| UI verbs | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\UI\IdleSliceUIController.cs` |
| Persist + timestamp | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\GameProgressData.cs` |
| Prefab defs 09–11 | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\Editor\IdleToolkitSliceGenerator.cs` |
| Batch BC smoke | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\Editor\Tests\IdleBatchBCSmokeTests.cs` |
| Matrix | `D:\Git\Hyper-Casual-Runner\docs\idle_mechanics_matrix.md` |
| Progress | `D:\Git\Hyper-Casual-Runner\docs\idle-toolkit-progress.md` |

---

## Evidence snapshot (docs / tests)

- Progress: Melvor / Egg Inc. / Idle Miner = **Batch BC PASS**, Play Mode **Pending** (`docs/idle-toolkit-progress.md`).
- Prefabs intended: `09_MelvorIdle_Skills_Slice`, `10_EggInc_Hatch_Slice`, `11_IdleMinerTycoon_Shafts_Slice` under `Assets/ToolkitExamples/Idle/`.
- This review did **not** re-run Unity tests; status above is from project docs + static code read.

---

## Recommended implementer backlog (for Round 01 IMPLEMENT — not executed)

**[required] Fix manager prestige re-lock**

- Criteria: After hire→prestige, generator requires manager again; buy alone does not raise PassiveRate; second hire does.
- Prefer: hire sets `IsAutomated = true` only; leave `RequiresManager` true as historical flag **or** prestige restores both flags from archetype bootstrap defaults.

**[required] Melvor offline = catch-up or honest stub**

- Criteria: Either (a) load/claim applies `min(elapsed, CapSeconds) * skillRate` and levels XP accordingly, setting/clearing `HasOfflineClaim`, **or** (b) remove/rename Claim Offline and mark `// TODO: [STUB]` with greppable note. Do not leave demo +10 labeled as Melvor offline.

**[required] Bridge or document Kernel A vs B**

- Criteria: Toolkit slices either gain wall-clock apply on `IdleSliceBootstrap.BuildInitialState` using `LastIdleUpdateTime`, or docs/progress explicitly state “offline catch-up deferred; claim UI Melvor-only stub.” Machine check: greppable stub or a unit test that advances fake elapsed time and asserts currency delta.

**[deferred] Egg Inc. research / vehicles / contracts** — beyond MVP bar.  
**[deferred] Idle Miner elevator/warehouse graph** — beyond MVP bar.  
**[deferred] Offline reward soft-cap + per-slice claim targeting** — needed before multi-slice hubs.

---

## Confidence ranking (lowest first — for evaluator spot-check)

1. **P0 manager flag leak** — highest confidence; direct code path, no runtime needed.
2. **Melvor claim demo branch** — high; Melvor never sets claim flags.
3. **No slice wall-clock catch-up** — high; load path read end-to-end.
4. **Melvor dual tick** — med-high; both loops fire when skill node active.
5. **Egg Inc. matrix depth** — low severity / intentional MVP; not a defect relative to bar.

---

## Receipt meta

- **Implemented:** nothing  
- **Committed:** no  
- **Pushed:** no  
- **Play Mode:** UNVERIFIED (MCP / editor not required for this examine pass)
