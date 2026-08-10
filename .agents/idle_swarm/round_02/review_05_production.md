# Round 02 Review 05 — Production / Offline / Managers (re-audit post impl_03)

**Agent:** 5/10 (EXAMINE / ANALYZE / REVIEW)  
**Scope:** Melvor Idle, Egg, Inc., Idle Miner Tycoon, offline rewards, automation/managers  
**Prior:** Round 01 `review_05_production.md` → Round 01 `impl_03_production.md` (`1e2d9df`) + peer kernel offline landings (`impl_07`)  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Mode:** Read-only analysis — no implementation, no push  
**Quality bar:** Prototype / toolkit MVP (`docs/project-context.md`); matrix DNA in `docs/idle_mechanics_matrix.md`

---

## ASSUMPTIONS

1. In-scope surface remains Kernel B toolkit slices (`IdleArchetype` + `IdleSliceState`), not runner slices — **high** — verified via `IdleSliceBootstrap` / prefab generator 09–11.
2. Round 01 required criteria from `review_05` + `impl_03` receipt are the pass bar for this re-audit; deferred Egg research / Miner elevators stay deferred — **high** — verified by both receipts.
3. EditMode Batch BC / helper-unit tests do **not** prove Play Mode load ordering — **high** — bootstrap is `MonoBehaviour.Start`; `OfflineSimulationSystem` is `InitializationSystemGroup`.
4. Current HEAD may include peer edits after `impl_03` (esp. `OfflineSimulationSystem` Kernel B path from kernel work) — **high** — verified by reading current `OfflineSimulationSystem.cs` vs impl_03 deviation note (“ProducerComponent-only”).

---

## Verdict (one line)

**Round-01 production P0s are fixed in code and EditMode, but Play Mode wall-clock offline for Melvor/Egg/Miner is still broken by an init-group timestamp race and dual Kernel-B catch-up contracts.**

---

## Round 01 required backlog — status

| Round 01 item | Impl_03 claim | HEAD status | Evidence |
|---------------|---------------|-------------|----------|
| Manager prestige re-lock | met | **PASS** | Hire keeps `RequiresManager`; `IdlePrestigeMath.ResetBuyableGenerator` + `PrestigeSystem` clear hire; `IdleMiner_Prestige_ReLocksManager_*` Passed in `Logs/IdleProduction-impl03.log` |
| Melvor offline ≠ demo +10 | met | **PASS (logic)** / **FAIL (Play load)** | Honest claim no-op present; helper banks `PendingClaim`; Play Mode catch-up likely wiped (see P0) |
| Kernel A vs B bridge | met | **PARTIAL / REGRESSED** | `IdleOfflineCatchUp` + bootstrap load exist, but Kernel B also re-entered `OfflineSimulationSystem` with divergent semantics + timestamp wipe |
| Melvor single grind clock | met | **PASS** | Melvor switch arm empty; `IdleSkillNode` sole online XP/currency tick; click is currency-only |

Deferred (still correctly skipped): Egg research/vehicles/contracts; Miner elevator/warehouse graph; multi-slice soft-cap.

---

## Architecture map (post impl_03)

Three catch-up / claim paths now coexist:

| Path | When | Melvor | Egg / Miner (PassiveRate>0) | Cap |
|------|------|--------|------------------------------|-----|
| **B1 — `IdleOfflineCatchUp`** via `IdleSliceBootstrap.ApplyPersistedOfflineCatchUp` | Load in `BuildInitialState` (MonoBehaviour `Start`) | Bank `PendingClaim` + skill XP ticks; set `HasOfflineClaim` | Add **directly** to `PrimaryCurrency` | 8h |
| **B2 — `OfflineSimulationSystem` slice loop** | First `InitializationSystemGroup` update, then disables | Same elapsed×rate into `PendingClaim` **and** bumps `AfkChestSeconds` | Same → `PendingClaim` (needs Claim UI — Egg/Miner have **none**) | 8h |
| **A — `OfflineSimulationSystem` producers** | Same one-shot | N/A (no `ProducerComponent` on toolkit prefabs) | N/A | 8h |

Claim (`IdleClaimOfflineSystem`): pays `PendingClaim`, then **also** `AfkChestSeconds * (1+level) * mult` + cats. No demo +10. UI Melvor still has “Claim Offline”; Egg/Miner do not.

```
Session quit → SaveIdleSlice stamps LastIdleUpdateTime
Next Play Mode:
  World Init → OfflineSimulationSystem (often zero IdleSliceState) → stamps LastIdleUpdateTime=now
  Later Start → Bootstrap load → IdleOfflineCatchUp(elapsed≈0) → no Melvor/Egg/Miner catch-up
```

EditMode masks this: tests call `IdleOfflineCatchUp.Apply` directly, or create `OfflineSimulationSystem` **after** a slice exists with a stamped past time.

---

## Matrix vs implementation (scoped titles)

| Title | Matrix promise | Toolkit realization (HEAD) | Gap severity |
|-------|----------------|----------------------------|--------------|
| **Melvor Idle** | Grind skills; **total offline progression** | Online skill node OK; claim honest; catch-up helper OK; **Play load race** | **High** (Play offline) / Low (online) |
| **Egg, Inc.** | Hatch → habitats → soul eggs | Hatch + habitat CPS + prestige; catch-up helper adds currency; no Claim UI | **Med** (Play offline + OS PendingClaim strand) |
| **Idle Miner Tycoon** | Shafts; super-managers; prestige mines | Shaft + hire + prestige re-lock **fixed**; no elevator graph (deferred) | **Low** vs MVP bar; offline same race |

---

## Findings (ordered by severity)

### P0 — Play Mode offline window wiped before bootstrap catch-up

**Where:** `OfflineSimulationSystem.OnUpdate` always writes `GameProgressData.LastIdleUpdateTime = now` at end, even when the `IdleSliceState` query is empty. Runs in `InitializationSystemGroup` once, then `state.Enabled = false`.

**Bootstrap:** `IdleSliceBootstrap.TrySpawn` / catch-up runs from `Start()` / `Update` — after world init.

**Effect:** Cross-session Melvor/Egg/Miner offline accrual claimed by impl_03 does not survive real Play Mode entry. Helper + EditMode tests still pass. Matrix pillar “Absence Made Valuable” remains unmet in the path players hit.

**Acceptance for a future fix:** Integration test or Play Mode probe: stamp `LastIdleUpdateTime` to T−3600, enter play with Melvor prefab `LoadPersistedProgress`, assert `PendingClaim` (or Melvor level) increases **without** manually invoking the helper; and `OfflineSimulationSystem` must not clear the timestamp until slice catch-up has consumed it (or must not own Kernel B at all).

---

### P1 — Dual Kernel-B contracts disagree (and Egg/Miner Claim UI missing)

**B1 (`IdleOfflineCatchUp`):** Egg/Miner → `PrimaryCurrency` (no claim). Melvor → `PendingClaim` only (no `AfkChestSeconds`).

**B2 (`OfflineSimulationSystem`):** Any `PassiveRate > 0` → `PendingClaim` + `HasOfflineClaim` + `AfkChestSeconds += min(elapsed, 3600)`.

**Effect:**

1. If B2 ever runs with a live Melvor slice and old timestamp, Claim pays **PendingClaim + AfkChest formula** → inflated Melvor offline.
2. If B2 runs for Egg/Miner, rewards sit in `PendingClaim` with **no Claim button** → stranded unless something else drains it.
3. impl_03 receipt said Kernel A stayed Producer-only; peer kernel work re-added B2. Comments on bootstrap still claim “ProducerComponent-only.”

**Acceptance:** Single authority for toolkit offline — either bootstrap/`IdleOfflineCatchUp` **or** `OfflineSimulationSystem` slice loop, not both; Egg/Miner either keep direct PrimaryCurrency **or** gain Claim UI; Melvor must not get AfkChestSeconds from CPS catch-up.

---

### P1 — No end-to-end load test for Melvor/Egg catch-up

Existing coverage:

- `Melvor_OfflineCatchUp_BanksPendingClaimCapped` — pure helper
- `EggInc_OfflineCatchUp_AddsPrimaryCurrency` — pure helper
- `Melvor_ClaimOffline_NoDemoWithoutPending_PaysCatchUp` — claim only
- `OfflineCatchup_IdleSlice_WithoutProducer_SetsPendingClaim` — manual OS update **after** slice spawn (Cookie archetype)

Missing: bootstrap `BuildInitialState` + `LastIdleUpdateTime` + Melvor skill node XP sync; OS-before-Start ordering.

---

### P2 — Melvor still has a click currency side-path

`IdleClickProduceSystem` Melvor arm still grants click currency (XP owned by skill node). Online double-drip is fixed; Train Skill remains a third income tap. Acceptable MVP if HowTo stays “skills tick”; do not treat click as offline substitute.

---

### P2 — Melvor `SkillXp` not persisted

`SaveIdleSlice` stores `ProgressionLevel` but not intra-level `SkillXp`. Offline ticks update state XP before skill node attach on a successful catch-up load; quit mid-level still loses XP fraction. Low severity for MVP.

---

### P2 — Idle Miner / Egg matrix depth (unchanged, still deferred)

Miner: no elevator/warehouse bottlenecks. Egg: no research tree / vehicles / contracts. Prestige + hire/habitat verbs meet toolkit bar. Do not expand in production round unless bar rises.

---

### P3 — Claim soft-cap / multi-slice (improved targeting, still uncapped reward math)

`IdleEventTarget` + UI `TargetSlice` closed Round 01 “global blast” for prefab single-slice. `AfkChestSeconds * (1+level) * mult` still has no hours soft-cap beyond chest fill policy. Deferred until multi-slice hubs.

---

## Round 01 P0s that stay closed (do not re-open)

1. **Manager gate leak** — hire preserves `RequiresManager`; prestige resets via `IdlePrestigeMath`; buy-alone PassiveRate==0 covered by Batch A + BC tests.
2. **Melvor demo +10** — removed; empty claim is no-op.
3. **Melvor dual continuous grind** — switch arm no longer drips; skill node is online authority; `PassiveRate` aligned to `1/interval` for save/catch-up.

---

## File index (absolute paths)

| Role | Path |
|------|------|
| Kernel B catch-up helper | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleOfflineCatchUp.cs` |
| Bootstrap load + catch-up | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Authoring\IdleSliceBootstrap.cs` |
| Init-group offline (A+B2) | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\OfflineSimulationSystem.cs` |
| Claim / honest no-op | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleSliceActionSystems.cs` |
| Melvor online tick | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleSliceSimulationSystem.cs` |
| Hire / buy | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleBuyGeneratorSystem.cs` |
| Prestige reset | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\PrestigeSystem.cs` |
| Shared gate math | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdlePrestigeMath.cs` |
| Event targeting | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleEventTarget.cs` |
| UI verbs | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\UI\IdleSliceUIController.cs` |
| Persist + timestamp | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\GameProgressData.cs` |
| BC smoke + production tests | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\Editor\Tests\IdleBatchBCSmokeTests.cs` |
| Kernel offline fixture | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\Editor\Tests\IdleKernelCorrectnessTests.cs` |
| Prefabs 09–11 | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\Editor\IdleToolkitSliceGenerator.cs` |
| Prior review / impl | `.agents/idle_swarm/round_01/review_05_production.md`, `impl_03_production.md` |

---

## Evidence snapshot

- `Logs/IdleProduction-impl03.log`: Batch BC **29/29 Passed**, including `IdleMiner_Prestige_ReLocksManager_*`, `Melvor_ClaimOffline_*`, `Melvor_OfflineCatchUp_*`, `EggInc_OfflineCatchUp_*`, `MelvorIdle_GrindSkillNode_*`.
- `Logs/IdleBatchBC-Summary.txt`: `result=Passed pass=29 fail=0`.
- This Round 02 examine did **not** re-run Unity; status above is log + static re-read of HEAD.
- Play Mode offline ordering: **UNVERIFIED** by design of this examine (static proof of race is sufficient to call P0).

---

## Recommended implementer backlog (Round 02 IMPLEMENT — not executed)

**[required] Unify Kernel B offline + fix init race**

- Criteria: Exactly one writer applies toolkit wall-clock catch-up. `OfflineSimulationSystem` either (a) stays ProducerComponent-only and does **not** stamp `LastIdleUpdateTime` when no producers/slices were processed for catch-up, or (b) owns slice catch-up but runs only after slices exist / does not clear timestamp before apply. Machine check: EditMode test that creates world → runs init systems → then spawns Melvor via bootstrap path (or mirrors Start order) with T−1h stamp and asserts `PendingClaim > 0` (or documented PrimaryCurrency for Egg).

**[required] Align Egg/Miner offline UX with payment path**

- Criteria: If catch-up credits `PrimaryCurrency`, document that and keep OS from banking `PendingClaim` for those archetypes. If banking `PendingClaim`, add Claim verb (or auto-drain on load). Grep: no Egg/Miner `PendingClaim` credit without a claim/drain path.

**[required] Melvor catch-up must not inflate via AfkChest**

- Criteria: Melvor offline path sets PendingClaim (± skill ticks) only; claim of Melvor catch-up does not also multiply `AfkChestSeconds` from the same elapsed window.

**[deferred] Miner elevators / Egg research** — still beyond MVP bar.  
**[deferred] Persist Melvor SkillXp** — nice-to-have.  
**[deferred] Soft-cap claim formula for multi-slice hubs.**

---

## Confidence ranking (lowest first — for evaluator spot-check)

1. **Play Mode timestamp wipe (P0)** — high confidence from system-group vs `Start` ordering; not Play-verified this pass.
2. **Dual B1/B2 semantics (P1)** — highest code confidence; both files read end-to-end.
3. **Manager re-lock still good** — highest; tests + prestige path.
4. **Melvor demo grant gone** — high; claim branch + tests.
5. **Egg/Miner matrix depth** — intentional deferred; not a defect vs bar.

---

## Receipt meta

- **Implemented:** nothing  
- **Committed:** no  
- **Pushed:** no  
- **Play Mode:** UNVERIFIED (static race analysis only)
