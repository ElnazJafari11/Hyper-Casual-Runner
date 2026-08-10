# Round 03 Review 05 — Production / Offline / Managers (re-audit post R2)

**Agent:** 5/10 (EXAMINE / ANALYZE / REVIEW)  
**Scope:** Melvor Idle, Egg, Inc., Idle Miner Tycoon, offline rewards, automation/managers  
**Prior:** Round 02 `review_05_production.md` → Round 02 `impl_02_production.md` (`157acd5`) + Kernel `impl_01_kernel.md` (`8d2407c`) + `STAMP_POLICY.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Mode:** Read-only analysis — no implementation, no push  
**Quality bar:** Prototype / toolkit MVP (`docs/project-context.md`); matrix DNA in `docs/idle_mechanics_matrix.md`

---

## ASSUMPTIONS

1. In-scope surface remains Kernel B toolkit slices (`IdleArchetype` + `IdleSliceState`) for Melvor / Egg / Miner — **high** — verified via `IdleSliceBootstrap` / generator 09–11.
2. Round 02 required bar = stamp race + unify Kernel B + Melvor AfkChest honesty + Egg/Miner PrimaryCurrency bank (no Claim UI) — **high** — `review_05` backlog + `STAMP_POLICY.md` + `impl_02` receipt.
3. EditMode Play-order mirrors prove stamp policy, not live Play Mode MonoBehaviour `Start` vs `InitializationSystemGroup` — **high** — fixtures invoke `OfflineSimulationSystem` then `IdleOfflineCatchUp.Apply` directly; no `IdleSliceBootstrap.BuildInitialState` load test.
4. HEAD includes R2 Kernel + Production landings; this examine did not re-run Unity — **high** — static re-read + existing logs (`IdleAllSmoke-Summary.txt` 56/56, `IdleBatchBC-impl07-cozy.log`, `IdleKernel-TestResults.xml`).

---

## Verdict (one line)

**R2 closed the Init stamp wipe and dual Kernel-B writers, but Melvor offline is still lossy: catch-up banks non-persisted `PendingClaim` while autosave stamps time, so unclaimed AFK earnings vanish across quit/reload.**

---

## Round 02 required backlog — status

| Round 02 item | Impl_02 claim | HEAD status | Evidence |
|---------------|---------------|-------------|----------|
| Unify Kernel B + fix Init race | met | **PASS (code + EditMode)** / **Play UNVERIFIED** | `OfflineSimulationSystem` Producer-only; stamps only if `appliedCatchUp`; `PlayOrder_*` + `EmptyWorld_DoesNotStamp*` Passed in logs |
| Align Egg/Miner offline UX | met | **PASS** | CatchUp → `PrimaryCurrency`; UI has no Claim for Egg/Miner; `IdleMiner_OfflineCatchUp_*` / Egg asserts `PendingClaim == 0` |
| Melvor catch-up must not inflate via AfkChest | met | **PASS** | CatchUp never bumps `AfkChestSeconds`; claim D16 XOR; `Melvor_OfflineCatchUp_DoesNotBumpAfkChestSeconds` + `Claim_PendingClaim_DoesNotAlsoPayAfkChest` Passed |

Deferred (still correctly skipped): Egg research/vehicles/contracts; Miner elevators; Melvor SkillXp persist; claim soft-cap / multi-slice hubs.

---

## Architecture map (post R2)

| Path | When | Melvor | Egg / Miner (`PassiveRate>0`) | Cap | Stamps `LastIdleUpdateTime`? |
|------|------|--------|-------------------------------|-----|------------------------------|
| **B — `IdleOfflineCatchUp`** via bootstrap load | `BuildInitialState` when `LoadPersistedProgress` | Bank `PendingClaim` + skill XP ticks; **no** AfkChest | Add **directly** to `PrimaryCurrency` | 8h | Yes — after Apply when `elapsed > 0` (even if gained==0) |
| **A — `OfflineSimulationSystem`** | First Init tick, then disables | N/A (no slice mutation) | N/A | 8h | **Only if** ≥1 automated `ProducerComponent` catch-up applied |

Claim (`IdleClaimOfflineSystem`): D16 XOR — `PendingClaim` **or** chest/cats, never both. Honest no-op if empty. UI: Melvor “Claim Offline”; Egg/Miner none (by design).

```
R2 Play Mode (intended):
  Quit → PersistNow stamps T0
  Init → OfflineSimulationSystem (empty / no producers) → leaves T0
  Start → Bootstrap load → IdleOfflineCatchUp(elapsed) → Melvor PendingClaim / Egg Primary → stamp now
```

R1 dual B1/B2 slice loop in OS: **removed**. Shared contract: `.agents/idle_swarm/round_02/STAMP_POLICY.md`.

---

## Matrix vs implementation (scoped titles)

| Title | Matrix promise | Toolkit realization (HEAD) | Gap severity |
|-------|----------------|----------------------------|--------------|
| **Melvor Idle** | Grind skills; **total offline progression** | Online skill node OK; catch-up banks PendingClaim; claim honest; **PendingClaim not saved + autosave stamps** | **High** (claim-bank durability) / Low (online) |
| **Egg, Inc.** | Hatch → habitats → soul eggs | Hatch + habitat CPS + prestige; catch-up → Primary (survives save) | **Low** vs MVP; Play still unverified |
| **Idle Miner Tycoon** | Shafts; super-managers; prestige mines | Shaft + hire + prestige re-lock; catch-up → Primary when automated | **Low** vs MVP bar |

---

## Findings (ordered by severity)

### P0 — Melvor `PendingClaim` is not durable across save/quit (new post-R2)

**Where:**

- `IdleOfflineCatchUp.Apply` Melvor arm → `PendingClaim` only (not `PrimaryCurrency`).
- `GameProgressData.SaveIdleSlice` / `IdleSliceBootstrap.PersistNow` — **no** `PendingClaim` / `HasOfflineClaim` fields.
- Autosave every 2s + `IdleSaveManager` quit/pause → `PersistNow` → stamps `LastIdleUpdateTime = now`.

**Effect:**

1. Load after AFK → catch-up correctly banks `PendingClaim` and consumes the wall-clock window (stamps now).
2. If player never clicks Claim Offline before quit / domain reload → PendingClaim discarded; timestamp already advanced → **no re-accrual**.
3. Egg/Miner avoid this because they credit `PrimaryCurrency` (persisted). Melvor’s claim-bank style is the only production title hit.

**Acceptance for a future fix (pick one):**

- Persist `PendingClaim` + `HasOfflineClaim` in `SaveIdleSlice` / load and restore before UI, **or**
- Melvor catch-up auto-pays into `PrimaryCurrency` (document claim as optional UX), **or**
- Do not stamp `LastIdleUpdateTime` until PendingClaim is drained (keep T0 until claim) — must not fight 2s autosave.

Machine check: load Melvor with T−1h → assert PendingClaim > 0 → `PersistNow` → simulate fresh `BuildInitialState` load → PendingClaim (or Primary) still reflects catch-up without requiring a new AFK window.

---

### P1 — No bootstrap `BuildInitialState` / `ApplyPersistedOfflineCatchUp` integration test

Existing coverage still calls `IdleOfflineCatchUp.Apply` and OS Update in isolation (`PlayOrder_*`, kernel fixtures). Missing: `LoadPersistedProgress` + `TryLoadIdleSlice` + catch-up + Melvor skill-node attach (`Xp = initial.SkillXp`) under one path. Stamp race is EditMode-mirrored; full load pipe is not.

---

### P1 — Global `LastIdleUpdateTime` shared across all idle archetypes

`GameProgressData.LastIdleUpdateTime` is a single PlayerPrefs key (`HCR_LastIdleTime`), not per-archetype. Saving Melvor stamps T0; loading Egg later sees Melvor’s clock (or vice versa). Acceptable for single-slice prefab Play Mode; breaks if a hub loads multiple titles or the player switches toolkit prefabs without quitting cleanly. Deferred for multi-slice hubs unless implementers touch stamp policy again — then pin per-archetype keys or per-slice stamps.

---

### P2 — `ApplyPersistedOfflineCatchUp` stamps even when `Apply` gains 0

If `elapsed > 0` but `PassiveRate == 0` (e.g. Miner pre-hire), Apply no-ops then stamp still burns the AFK window. Correct “nothing earned” for that session; wrong if a later same-process hire expects the old window (hire only matters after next Persist). Low severity for MVP.

---

### P2 — Melvor click currency side-path (unchanged)

`IdleClickProduceSystem` Melvor arm still grants click currency; XP owned by skill node. Online double-drip remains fixed. Acceptable MVP.

---

### P2 — Melvor `SkillXp` not persisted (unchanged)

`SaveIdleSlice` stores `ProgressionLevel` only. Catch-up updates in-memory XP and attaches to `IdleSkillNode`; quit mid-level loses XP fraction. Levels from offline ticks that crossed thresholds are persisted via `ProgressionLevel`.

---

### P2 — Idle Miner / Egg matrix depth (unchanged, still deferred)

Miner: no elevator/warehouse. Egg: no research/vehicles/contracts. Do not expand unless bar rises.

---

### P3 — Claim formula uncapped beyond chest fill / soft-cap (unchanged)

D16 XOR closed double-pay. `AfkChestSeconds * (1+level) * mult` still has no hours soft-cap beyond chest fill policy. Deferred until multi-slice hubs.

---

## Round 01 / Round 02 items that stay closed (do not re-open)

1. **Manager prestige re-lock** — hire keeps `RequiresManager`; `IdlePrestigeMath.ResetBuyableGenerator` + prestige clear hire; BC tests Passed.
2. **Melvor demo +10** — removed; empty claim is no-op.
3. **Melvor dual continuous grind** — switch arm empty; skill node is online authority; `PassiveRate` aligned to `1/interval`.
4. **Init-group timestamp wipe (R2 P0)** — OS no longer stamps on empty Init; Producer-only.
5. **Dual Kernel-B OS slice loop (R2 P1)** — removed; sole slice writer = `IdleOfflineCatchUp`.
6. **Melvor AfkChest inflation from CPS catch-up (R2)** — CatchUp does not bump chest; claim XOR.
7. **Egg/Miner Claim UI** — correctly **not** required under PrimaryCurrency bank.

---

## File index (absolute paths)

| Role | Path |
|------|------|
| Kernel B catch-up helper | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleOfflineCatchUp.cs` |
| Bootstrap load + catch-up | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Authoring\IdleSliceBootstrap.cs` |
| Init-group offline (A only) | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\OfflineSimulationSystem.cs` |
| Claim / D16 XOR | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleSliceActionSystems.cs` |
| Melvor online tick | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleSliceSimulationSystem.cs` |
| Hire / buy | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleBuyGeneratorSystem.cs` |
| Prestige reset | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\PrestigeSystem.cs` |
| Shared gate math | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdlePrestigeMath.cs` |
| Quit/pause stamp | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\UI\IdleSaveManager.cs` |
| UI verbs | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\UI\IdleSliceUIController.cs` |
| Persist + timestamp | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\GameProgressData.cs` |
| BC smoke + production tests | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\Editor\Tests\IdleBatchBCSmokeTests.cs` |
| Kernel offline fixture | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\Editor\Tests\IdleKernelCorrectnessTests.cs` |
| Stamp contract | `D:\Git\Hyper-Casual-Runner\.agents\idle_swarm\round_02\STAMP_POLICY.md` |
| Prior review / impl | `.agents/idle_swarm/round_02/review_05_production.md`, `impl_02_production.md` |

---

## Evidence snapshot

- `Logs/IdleAllSmoke-Summary.txt`: `result=Passed pass=56 fail=0`.
- `Logs/IdleBatchBC-impl07-cozy.log` / `IdleAllSmoke-impl04-r2.log`: `PlayOrder_OfflineSimulationThenMelvorCatchUp_PreservesStamp`, `Melvor_OfflineCatchUp_DoesNotBumpAfkChestSeconds`, `IdleMiner_OfflineCatchUp_AddsPrimaryCurrency_NoPendingClaim`, `EggInc_OfflineCatchUp_AddsPrimaryCurrency`, manager re-lock → Passed.
- `Logs/IdleKernel-TestResults.xml`: `OfflineSimulation_EmptyWorld_DoesNotStampLastIdleUpdateTime`, `Claim_PendingClaim_DoesNotAlsoPayAfkChest` → Passed.
- This Round 03 examine did **not** re-run Unity; status above is log + static re-read of HEAD.
- Play Mode offline: **UNVERIFIED** (MCP still not on Hyper-Casual-Runner per progress doc). Static proof of Melvor PendingClaim non-persistence does not need Play Mode.

---

## Recommended implementer backlog (Round 03 IMPLEMENT — not executed)

**[required] Make Melvor offline durable under claim-bank style**

- Criteria: After catch-up banks PendingClaim, a PersistNow + cold reload (or equivalent EditMode round-trip) must still expose the offline reward via PendingClaim restore **or** PrimaryCurrency, without requiring a second AFK window. Grep: either PendingClaim in Save/Load, or Melvor CatchUp credits Primary, or stamp deferred until claim — one path only.
- Must not re-break Egg/Miner Primary bank or OS empty-Init stamp policy.

**[required] Bootstrap load-path test for Melvor catch-up**

- Criteria: EditMode test exercises timestamp + load + catch-up (+ optional skill level tick), not only bare `IdleOfflineCatchUp.Apply`. Prefer mirroring `ApplyPersistedOfflineCatchUp` / `BuildInitialState` load branch.

**[deferred] Per-archetype `LastIdleUpdateTime`** — multi-slice / multi-prefab hub.  
**[deferred] Persist Melvor SkillXp** — nice-to-have.  
**[deferred] Miner elevators / Egg research** — beyond MVP bar.  
**[deferred] Soft-cap claim formula for multi-slice hubs.**

---

## Confidence ranking (lowest first — for evaluator spot-check)

1. **Melvor PendingClaim loss on quit (P0)** — highest code confidence; SaveIdleSlice field list + CatchUp bank destination read end-to-end; no runtime needed.
2. **R2 stamp race closed** — high for code + EditMode logs; Play Mode still unverified.
3. **Egg/Miner Primary path OK** — high; persists through SaveIdleSlice.
4. **Manager re-lock / demo claim / AfkChest honesty** — highest; unchanged closed items.
5. **Global stamp key (P1)** — med; only bites multi-archetype sessions.

---

## Receipt meta

- **Implemented:** nothing  
- **Committed:** no  
- **Pushed:** no  
- **Play Mode:** UNVERIFIED (static durability analysis + prior EditMode logs only)
