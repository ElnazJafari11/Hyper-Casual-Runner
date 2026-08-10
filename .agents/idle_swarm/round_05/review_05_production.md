# Round 05 Review 05 — Production / Offline / Managers (re-audit post R4)

**Agent:** 5/10 (EXAMINE / ANALYZE / REVIEW)  
**Scope:** Melvor Idle, Egg, Inc., Idle Miner Tycoon, offline rewards, automation/managers  
**Prior:** Round 04 `review_05_production.md` → Round 04 `impl_05_production.md` (`2b21929`) + Kernel `impl_01_kernel.md` (`195aa9f`, D33) + `STAMP_POLICY.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Mode:** Read-only analysis — no implementation, no push  
**Quality bar:** Prototype / toolkit MVP (`docs/project-context.md`); matrix DNA in `docs/idle_mechanics_matrix.md`

---

## ASSUMPTIONS

1. In-scope surface remains Kernel B toolkit slices (`IdleArchetype` + `IdleSliceState`) for Melvor / Egg / Miner — **high** — verified via `IdleSliceBootstrap` / generator 09–11 / UI verbs.
2. Round 04 required bar = Melvor `IdleSkillNode` sync after CatchUp (D33) + immediate Pending Persist after CatchUp stamp + load-path skill+Pending EditMode fixture — **high** — `review_05` backlog + `impl_05` / `impl_01` receipts.
3. D33 Sync API + bootstrap rewrite owned by Kernel (`195aa9f`); Production owns PersistNow flush + BC attach fixture (`2b21929`) — **high** — code + receipts.
4. EditMode mirrors prove stamp/skill/persist contracts; live Play Mode MonoBehaviour `Start` vs `InitializationSystemGroup` remains UNVERIFIED — **high** — Melvor still has no `SpawnBootstrap` fixture (unlike cozy/IH peers).
5. This examine did not re-run Unity — **high** — static re-read of HEAD + existing logs (`IdleAllSmoke-Summary.txt` 91/91, `IdleBatchBC-Summary.txt` 58/58, `IdleBatchBC-impl05-r4c.log`, `IdleKernel-TestResults.xml`).

---

## Verdict (one line)

**R4 closed Melvor D33 skill-node sync and the CatchUp→Persist stamp window; production offline for Melvor/Egg/Miner is EditMode-green, with residuals only in live bootstrap/Play probes, SkillXp durability, and deferred matrix depth.**

---

## Round 04 required backlog — status

| Round 04 item | Impl claim | HEAD status | Evidence |
|---------------|------------|-------------|----------|
| Sync Melvor `IdleSkillNode` after CatchUp (D33) | met (kernel) | **PASS (code + EditMode)** | Bootstrap `SyncSkillNodeFromSlice` after `ApplyPersistedElapsed`; `Melvor_CatchUp_SyncsIdleSkillNode_FromSlice` Passed; BC `Melvor_AttachThenCatchUp_*` Passed |
| Flush PendingClaim when CatchUp stamps | met | **PASS (code + EditMode mirror)** | `TrySpawn`: `_spawned=true` then `PersistNow()` when `catchUpGained > 0`; BC fixture SaveIdleSlice immediately post-CatchUp asserts Pending restore |
| Load-path test covering skill node + Pending | met | **PASS (helper mirror)** / **PARTIAL vs live bootstrap** | Attach→CatchUp→Sync→Save covers node+Pending; still does **not** call `IdleSliceBootstrap` / `SpawnBootstrap` |
| Must not re-break Egg/Miner Primary, OS empty-Init stamp, AfkChest honesty, Pending durability | met | **PASS** | Egg/Miner → Primary; OS Producer-only + stamp-if-applied; D16 XOR; Pending prefs; fixtures Passed in BC/kernel logs |

Deferred (still correctly skipped): per-archetype stamp; Melvor SkillXp persist; Miner elevators; Egg research; claim soft-cap / multi-slice hubs; Play Mode wall-clock probe.

---

## Architecture map (post R4)

| Path | When | Melvor | Egg / Miner (`PassiveRate>0`) | Cap | Stamps `LastIdleUpdateTime`? |
|------|------|--------|-------------------------------|-----|------------------------------|
| **B — `IdleOfflineCatchUp`** via bootstrap load | `TrySpawn` after Attach Sync when `_loadedFromPrefs` | Bank `PendingClaim` + skill XP ticks on **slice**; then **D33** rewrite `IdleSkillNode`; **no** AfkChest | Add **directly** to `PrimaryCurrency` | 8h | Only if grant **> 0** (D23); then **PersistNow** if grant > 0 (R4) |
| **A — `OfflineSimulationSystem`** | First Init tick, then disables | N/A (no slice mutation) | N/A | 8h | **Only if** ≥1 automated `ProducerComponent` catch-up applied |

Claim (`IdleClaimOfflineSystem`): D16 XOR — `PendingClaim` **or** chest/cats, never both. Honest no-op if empty. UI: Melvor “Claim Offline”; Egg/Miner none (by design).

Persist: `PendingClaim` + `HasOfflineClaim` + `ProgressionLevel` via `SaveIdleSlice` / `PersistNow`. **`SkillXp` still omitted.**

```
R4+ intended Play Mode:
  Quit → PersistNow writes Pending + stamps T0
  Init → OfflineSimulationSystem (empty / no producers) → leaves T0
  Start → Attach (Sync PassiveRate) → IdleOfflineCatchUp(elapsed)
       → Melvor PendingClaim + skill ticks → SyncSkillNodeFromSlice
       → Egg/Miner Primary → stamp now if grant>0
       → PersistNow immediately (R4) flushes Pending/Primary/Level to prefs
```

Shared contract: `.agents/idle_swarm/round_02/STAMP_POLICY.md` (includes D33 Melvor skill-node sync row).

---

## Matrix vs implementation (scoped titles)

| Title | Matrix promise | Toolkit realization (HEAD) | Gap severity |
|-------|----------------|----------------------------|--------------|
| **Melvor Idle** | Grind skills; **total offline progression** | Online skill node OK; Pending durable; D33 node sync; claim honest; SkillXp session-only; Play UNVERIFIED | **Low–Med** (SkillXp / Play) — was High on skill orphan |
| **Egg, Inc.** | Hatch → habitats → soul eggs | Hatch + habitat CPS + prestige; catch-up → Primary + immediate Persist | **Low** vs MVP; Play still unverified |
| **Idle Miner Tycoon** | Shafts; super-managers; prestige mines | Shaft + hire + prestige re-lock; catch-up → Primary when automated | **Low** vs MVP bar |

---

## Findings (ordered by severity)

### P1 — Melvor still lacks live `IdleSliceBootstrap` / `SpawnBootstrap` integration test

**Where:** BC `Melvor_AttachThenCatchUp_SyncsSkillNodeAndPersistsPending` and kernel `Melvor_CatchUp_SyncsIdleSkillNode_FromSlice` mirror Attach→CatchUp→Sync→Save by hand. Cozy/IH already use `SpawnBootstrap` + reflected `TrySpawn`.

**Effect:** D33 + R4 PersistNow landings are proven as helper sequences, not as the MonoBehaviour load pipe (`BuildInitialState` → `AttachArchetypeExtras` → CatchUp → Sync → PersistNow). Order regressions (e.g. Persist before `_spawned`, Sync skipped, CatchUp before Attach) would not fail Melvor-specific fixtures.

**Acceptance:** EditMode `SpawnBootstrap(MelvorIdle)` with T−N stamp + prefs → after `TrySpawn`, assert `IdleSkillNode.Level == IdleSliceState.ProgressionLevel`, `PendingClaim > 0`, and a cold `TryLoadIdleSlice` restores that Pending without waiting 2s.

---

### P1 — Play Mode wall-clock AFK still UNVERIFIED (unchanged, still deferred)

No PersistNow → stop Play → enter Play probe for Melvor/Egg/Miner. EditMode stamp/OS/CatchUp fixtures remain the only evidence. Acceptable to keep deferred until MCP/editor bridge owns a Play probe; do not claim matrix “Absence Made Valuable” fully closed for players.

---

### P1 — Global `LastIdleUpdateTime` shared across all idle archetypes (unchanged)

`GameProgressData.LastIdleUpdateTime` is a single PlayerPrefs key (`HCR_LastIdleTime`), not per-archetype. Acceptable for single-slice prefab Play Mode; breaks multi-title hubs. Deferred unless stamp policy is touched again.

---

### P2 — Melvor `SkillXp` not persisted (unchanged; sharper after R4 PersistNow)

`SaveIdleSlice` / `TryLoadIdleSlice` store `ProgressionLevel` only. Intra-level XP is session-only. R4 immediate PersistNow after CatchUp correctly saves Level + Pending but **drops** CatchUp `SkillXp` on the next cold load (attach `Xp = 0` at restored Level). Online skill loop also writes `slice.SkillXp` **only on level-up**, so mid-bar `IdleSkillNode.Xp` and `IdleSliceState.SkillXp` can diverge until the next level — any future SkillXp key must read the node (or sync every tick).

**Acceptance:** Persist `SkillXp` (and restore into attach + Sync), **or** greppable `TODO: [STUB]` + document defer.

---

### P2 — CatchUp does not bump `ClickPower` on offline level-ups (unchanged)

Online skill level-up: `ClickPower += 1`. `ApplyMelvorSkillTicks` / `SyncSkillNodeFromSlice` do not. Melvor UI `Train Skill` fires `FireClick(1f)` (fixed gain), so impact is mostly `BaseDamage` / vestigial ClickPower — MVP-acceptable. Close if Train Skill ever keys off ClickPower.

---

### P2 — Melvor click currency side-path (unchanged)

`IdleClickProduceSystem` Melvor arm still grants click currency; XP owned by skill node. Acceptable MVP.

---

### P2 — Hybrid Kernel A stamp can starve Kernel B (latent, unchanged)

If an automated `ProducerComponent` applies in the same Init pass, OS stamps before bootstrap CatchUp. Toolkit prefabs do not add producers today.

---

### P2 — Idle Miner / Egg matrix depth (unchanged, still deferred)

Miner: no elevator/warehouse. Egg: no research/vehicles/contracts. Do not expand unless bar rises.

---

### P3 — Claim formula uncapped beyond chest fill / soft-cap (unchanged)

D16 XOR closed double-pay. Soft-cap deferred until multi-slice hubs.

---

## Round 01–04 items that stay closed (do not re-open)

1. **Manager prestige re-lock** — hire keeps `RequiresManager`; prestige clears hire; BC tests Passed.
2. **Melvor demo +10** — removed; empty claim is no-op.
3. **Melvor dual continuous grind** — switch arm empty; skill node is online authority; `PassiveRate` aligned to `1/interval`.
4. **Init-group timestamp wipe (R2 P0)** — OS no longer stamps on empty Init; Producer-only.
5. **Dual Kernel-B OS slice loop (R2 P1)** — removed; sole slice writer = `IdleOfflineCatchUp`.
6. **Melvor AfkChest inflation from CPS catch-up (R2)** — CatchUp does not bump chest; claim XOR.
7. **Egg/Miner Claim UI** — correctly **not** required under PrimaryCurrency bank.
8. **Melvor PendingClaim non-persistence (R3 P0)** — **closed** via Pending/HasClaim PlayerPrefs + bootstrap restore (`578a474`).
9. **Zero-grant AFK wipe (D23)** — **closed**; `ApplyPersistedElapsed` stamps only if grant > 0.
10. **CatchUp Mult² before Sync (D25)** — **closed** for generator PassiveRate order.
11. **Melvor IdleSkillNode orphan after CatchUp (R4 P0 / D33)** — **closed** via bootstrap Sync + kernel/BC fixtures (`195aa9f`).
12. **CatchUp stamp before Pending flush (R4 P1)** — **closed** via immediate `PersistNow` when `catchUpGained > 0` (`2b21929`).

---

## File index (absolute paths)

| Role | Path |
|------|------|
| Kernel B catch-up helper + D33 sync | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleOfflineCatchUp.cs` |
| Bootstrap load + CatchUp + Sync + PersistNow | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Authoring\IdleSliceBootstrap.cs` |
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
| Kernel offline + D33 fixture | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\Editor\Tests\IdleKernelCorrectnessTests.cs` |
| Stamp contract | `D:\Git\Hyper-Casual-Runner\.agents\idle_swarm\round_02\STAMP_POLICY.md` |
| Prior review / impl | `.agents/idle_swarm/round_04/review_05_production.md`, `impl_05_production.md`; Kernel `impl_01_kernel.md` |

---

## Evidence snapshot

- `Logs/IdleAllSmoke-Summary.txt`: `result=Passed pass=91 fail=0`.
- `Logs/IdleBatchBC-Summary.txt`: `result=Passed pass=58 fail=0`.
- `Logs/IdleBatchBC-impl05-r4c.log`: `Melvor_AttachThenCatchUp_SyncsSkillNodeAndPersistsPending`, `Melvor_PendingClaim_SurvivesPersistAndColdReload`, `Melvor_OfflineCatchUp_DoesNotBumpAfkChestSeconds` (via suite), `IdleMiner_OfflineCatchUp_*`, `EggInc_OfflineCatchUp_*`, `PlayOrder_OfflineSimulationThenMelvorCatchUp_PreservesStamp`, `IdleMiner_Prestige_ReLocksManager_*` → Passed.
- `Logs/IdleKernel-TestResults.xml`: `Melvor_CatchUp_SyncsIdleSkillNode_FromSlice`, `OfflineSimulation_EmptyWorld_DoesNotStampLastIdleUpdateTime`, `ApplyPersistedElapsed_ZeroGrant_DoesNotStampLastIdleUpdateTime`, `Claim_PendingClaim_DoesNotAlsoPayAfkChest` → Passed.
- Commits on production path: `195aa9f` (D33 sync), `2b21929` (PersistNow flush + BC fixture); prior `578a474` (Pending persist), `bd06da8` (D23/D25).
- This Round 05 examine did **not** re-run Unity; status above is log + static re-read of HEAD.
- Play Mode offline: **UNVERIFIED**.

---

## Recommended implementer backlog (Round 05 IMPLEMENT — not executed)

**[required] Melvor `SpawnBootstrap` / live TrySpawn load-path fixture**

- Criteria: EditMode creates Melvor bootstrap with `LoadPersistedProgress`, stamps T−N, invokes real `TrySpawn` (same helper cozy/IH use). Assert post-spawn: `IdleSkillNode.Level == ProgressionLevel`, `PendingClaim > 0`, and `TryLoadIdleSlice` restores Pending without a 2s wait.
- Must not re-break D25 Sync-before-CatchUp, D33 Sync, or R4 PersistNow-on-grant.

**[deferred] Play Mode wall-clock probe** — when MCP/editor bridge is on this repo (Melvor Pending / Egg Primary after relaunch).

**[deferred] Persist Melvor SkillXp** — restore into attach; if persisting, sync `IdleSliceState.SkillXp` from `IdleSkillNode` every skill tick (or read node in PersistNow).

**[deferred] CatchUp ClickPower parity** — only if Train Skill / BaseDamage starts using ClickPower meaningfully.

**[deferred] Per-archetype `LastIdleUpdateTime`** — multi-slice / multi-prefab hub.

**[deferred] Miner elevators / Egg research** — beyond MVP bar.

**[deferred] Soft-cap claim formula for multi-slice hubs.**

---

## Confidence ranking (lowest first — for evaluator spot-check)

1. **Live bootstrap Melvor gap (P1)** — high; SpawnBootstrap exists for peers, Melvor fixtures still hand-mirror; highest residual false-green risk.
2. **SkillXp omit after R4 PersistNow (P2)** — high; SaveIdleSlice has no SkillXp key; CatchUp XP bar lost on cold load.
3. **R4 D33 + PersistNow closed** — highest; code path + kernel/BC logs Passed.
4. **R2/R3 stamp / AfkChest / Pending / manager re-lock** — highest; unchanged closed items + logs.
5. **Play Mode AFK (P1 deferred)** — med confidence it still works given EditMode; zero live evidence.

---

## Receipt meta

- **Implemented:** nothing  
- **Committed:** no  
- **Pushed:** no  
- **Play Mode:** UNVERIFIED (static analysis + prior EditMode logs only)
