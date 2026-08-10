# Round 04 Review 05 — Production / Offline / Managers (re-audit post R3)

**Agent:** 5/10 (EXAMINE / ANALYZE / REVIEW)  
**Scope:** Melvor Idle, Egg, Inc., Idle Miner Tycoon, offline rewards, automation/managers  
**Prior:** Round 03 `review_05_production.md` → Round 03 `impl_02_production.md` (`578a474`) + Kernel `impl_01_kernel.md` (`bd06da8`, D23/D25) + `STAMP_POLICY.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Mode:** Read-only analysis — no implementation, no push  
**Quality bar:** Prototype / toolkit MVP (`docs/project-context.md`); matrix DNA in `docs/idle_mechanics_matrix.md`

---

## ASSUMPTIONS

1. In-scope surface remains Kernel B toolkit slices (`IdleArchetype` + `IdleSliceState`) for Melvor / Egg / Miner — **high** — verified via `IdleSliceBootstrap` / generator 09–11.
2. Round 03 required bar = Melvor `PendingClaim` durable across PersistNow/quit + bootstrap load-path EditMode test — **high** — `review_05` backlog + `impl_02` receipt (`578a474`).
3. D23/D25 landed via Kernel R3 (`bd06da8`): stamp-only-if-grant + CatchUp **after** Attach Sync — **high** — code + `IdleKernelCorrectnessTests` / Batch A Sync fixtures.
4. EditMode mirrors prove stamp/persist contracts; live Play Mode MonoBehaviour `Start` vs `InitializationSystemGroup` remains UNVERIFIED — **high** — fixtures still call helpers / OS Update directly; no live `IdleSliceBootstrap` spawn test.
5. This examine did not re-run Unity — **high** — static re-read of HEAD + existing logs (`IdleAllSmoke-Summary.txt` 72/72, `IdleBatchBC-Summary.txt` 51/51, `IdleBatchBC-impl02-r3b.log`).

---

## Verdict (one line)

**R3 closed Melvor PendingClaim save/load durability and R2 stamp races stay closed, but D25 CatchUp-after-Attach orphaned Melvor `IdleSkillNode` from offline skill ticks, so AFK levels are written to slice then clobbered online.**

---

## Round 03 required backlog — status

| Round 03 item | Impl_02 claim | HEAD status | Evidence |
|---------------|---------------|-------------|----------|
| Melvor PendingClaim durable under claim-bank | met | **PASS (persist API + EditMode)** | `SaveIdleSlice`/`TryLoadIdleSlice` Pending+HasClaim; bootstrap restore; `Melvor_PendingClaim_SurvivesPersistAndColdReload` Passed in `IdleBatchBC-impl02-r3b.log` / AllSmoke logs |
| Bootstrap load-path catch-up test | met | **PASS (helper mirror)** / **PARTIAL vs live bootstrap** | `Melvor_BootstrapLoadPath_CatchUpBanksPendingAndSkillLevel` uses TryLoad + `ApplyPersistedElapsed`; does **not** spawn `IdleSliceBootstrap` or assert `IdleSkillNode` sync |
| Must not re-break Egg/Miner Primary or OS empty-Init stamp | met | **PASS** | Egg/Miner → Primary; OS Producer-only + stamp-if-applied; PlayOrder / EmptyWorld fixtures Passed |

Deferred (still correctly skipped): per-archetype stamp; Melvor SkillXp persist; Miner elevators; Egg research; claim soft-cap / multi-slice hubs.

---

## Architecture map (post R3)

| Path | When | Melvor | Egg / Miner (`PassiveRate>0`) | Cap | Stamps `LastIdleUpdateTime`? |
|------|------|--------|-------------------------------|-----|------------------------------|
| **B — `IdleOfflineCatchUp`** via bootstrap load | `TrySpawn` after Attach Sync when `_loadedFromPrefs` | Bank `PendingClaim` + skill XP ticks on **slice**; **no** AfkChest | Add **directly** to `PrimaryCurrency` | 8h | Only if grant **> 0** (D23) |
| **A — `OfflineSimulationSystem`** | First Init tick, then disables | N/A (no slice mutation) | N/A | 8h | **Only if** ≥1 automated `ProducerComponent` catch-up applied |

Claim (`IdleClaimOfflineSystem`): D16 XOR — `PendingClaim` **or** chest/cats, never both. Honest no-op if empty. UI: Melvor “Claim Offline”; Egg/Miner none (by design).

Persist: `PendingClaim` + `HasOfflineClaim` written by `SaveIdleSlice` / `PersistNow` (R3 path chosen — keep claim-bank, not auto-pay Primary).

```
R3+ intended Play Mode:
  Quit → PersistNow writes Pending + stamps T0
  Init → OfflineSimulationSystem (empty / no producers) → leaves T0
  Start → Attach (Sync PassiveRate) → IdleOfflineCatchUp(elapsed)
       → Melvor PendingClaim / Egg Primary → stamp now if grant>0
  ≤2s autosave / quit → PersistNow flushes Pending to prefs
```

Shared contract: `.agents/idle_swarm/round_02/STAMP_POLICY.md` (+ D23/D25 amendments landed in Kernel R3).

---

## Matrix vs implementation (scoped titles)

| Title | Matrix promise | Toolkit realization (HEAD) | Gap severity |
|-------|----------------|----------------------------|--------------|
| **Melvor Idle** | Grind skills; **total offline progression** | PendingClaim durable; claim honest; **offline skill ticks not applied to IdleSkillNode** after D25 | **High** (skill sync) / Low (claim bank) |
| **Egg, Inc.** | Hatch → habitats → soul eggs | Hatch + habitat CPS + prestige; catch-up → Primary (persisted); D25 Sync-before-CatchUp | **Low** vs MVP; Play still unverified |
| **Idle Miner Tycoon** | Shafts; super-managers; prestige mines | Shaft + hire + prestige re-lock; catch-up → Primary when automated | **Low** vs MVP bar |

---

## Findings (ordered by severity)

### P0 — Melvor offline skill ticks orphaned from `IdleSkillNode` (D25 regression)

**Where:**

- `IdleSliceBootstrap.TrySpawn`: `AttachArchetypeExtras` creates `IdleSkillNode` from **pre-catch-up** `initial` (`Xp = initial.SkillXp` usually 0; `Level` from prefs only).
- Then `IdleOfflineCatchUp.ApplyPersistedElapsed` updates **`IdleSliceState`.SkillXp / ProgressionLevel** only — does not touch `IdleSkillNode`.
- `IdleSliceSimulationSystem` skill loop is the online authority: on skill level-up it writes `slice.ProgressionLevel = skill.Level`, which **clobbers** catch-up levels when the node is still at the old level.

**Effect:**

1. AFK currency bank (`PendingClaim`) still works and now survives Persist (R3 closed).
2. AFK skill progression is applied to slice state, then ignored / overwritten by the skill node → matrix “total offline progression” for Melvor skills is still lossy online.
3. R3 bootstrap test asserts `ProgressionLevel` on a bare `IdleSliceState` after `ApplyPersistedElapsed` — **false green** for the real Attach→CatchUp→sim path.

**Acceptance for a future fix:**

After Melvor `ApplyPersistedElapsed` in bootstrap (or inside CatchUp when a skill node exists), sync `IdleSkillNode.Level` / `Xp` / `XpToLevel` from the post-catch-up slice (and keep `ClickPower` policy explicit).  
Machine check: EditMode path that Attach Melvor skill node → ApplyPersistedElapsed with T−N → assert `IdleSkillNode.Level == IdleSliceState.ProgressionLevel` (and optionally one sim tick does not drop level).

Must not re-break D25 Sync-before-CatchUp for generator PassiveRate.

---

### P1 — CatchUp stamps wall-clock before PendingClaim is flushed to prefs

**Where:** `IdleOfflineCatchUp.ApplyPersistedElapsed` stamps `LastIdleUpdateTime` immediately on grant > 0. Bootstrap does **not** call `PersistNow` right after CatchUp; first autosave is ≤2s (`Update` timer) or quit/pause/destroy.

**Effect:** Force-kill / crash / domain loss in the window between CatchUp and first Persist re-opens a narrow form of the R3 P0: stamp advanced, Pending only in ECS memory → reload restores stale Pending (often 0) and will not re-accrue the AFK window.

Normal mobile pause/quit + Editor stop (OnDestroy Persist) cover the happy path; durability tests always `SaveIdleSlice` explicitly after CatchUp.

**Acceptance:** `PersistNow` (or equivalent SaveIdleSlice with Pending) immediately after successful Melvor CatchUp in bootstrap, **or** defer stamp until Pending is on disk when `PendingClaim` increased.

---

### P1 — No live `IdleSliceBootstrap` / `IdleSkillNode` integration test

Existing coverage still mirrors pieces:

- `Melvor_PendingClaim_SurvivesPersistAndColdReload` — TryLoad + ApplyPersistedElapsed + SaveIdleSlice round-trip
- `Melvor_BootstrapLoadPath_CatchUpBanksPendingAndSkillLevel` — same helpers; skill level on bare state
- `PlayOrder_*` / Kernel EmptyWorld — OS stamp policy
- D25 Kernel/Batch A — Sync→CatchUp Mult² magnitude

Missing: spawn-order fixture that builds Melvor extras + CatchUp + asserts skill node + Pending together. Stamp race remains EditMode-mirrored; full MonoBehaviour Start pipe still UNVERIFIED.

---

### P1 — Global `LastIdleUpdateTime` shared across all idle archetypes (unchanged)

`GameProgressData.LastIdleUpdateTime` is a single PlayerPrefs key (`HCR_LastIdleTime`), not per-archetype. Acceptable for single-slice prefab Play Mode; breaks multi-title hubs. Deferred unless stamp policy is touched again.

---

### P2 — Melvor `SkillXp` not persisted (unchanged)

`SaveIdleSlice` stores `ProgressionLevel` only (plus Pending). Intra-level XP still session-only. Compounded by P0: even in-session CatchUp XP never reaches the skill node.

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

## Round 01–03 items that stay closed (do not re-open)

1. **Manager prestige re-lock** — hire keeps `RequiresManager`; `IdlePrestigeMath.ResetBuyableGenerator` + prestige clear hire; BC tests Passed.
2. **Melvor demo +10** — removed; empty claim is no-op.
3. **Melvor dual continuous grind** — switch arm empty; skill node is online authority; `PassiveRate` aligned to `1/interval`.
4. **Init-group timestamp wipe (R2 P0)** — OS no longer stamps on empty Init; Producer-only.
5. **Dual Kernel-B OS slice loop (R2 P1)** — removed; sole slice writer = `IdleOfflineCatchUp`.
6. **Melvor AfkChest inflation from CPS catch-up (R2)** — CatchUp does not bump chest; claim XOR.
7. **Egg/Miner Claim UI** — correctly **not** required under PrimaryCurrency bank.
8. **Melvor PendingClaim non-persistence (R3 P0)** — **closed** via Pending/HasClaim PlayerPrefs + bootstrap restore (`578a474`).
9. **Zero-grant AFK wipe (R3 kernel D23 / prior R3 review P2)** — **closed**; `ApplyPersistedElapsed` stamps only if grant > 0.
10. **CatchUp Mult² before Sync (R3 kernel D25)** — **closed** for generator PassiveRate order; Melvor skill-node sync is the new residual (P0 above), not a re-open of Mult².

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
| Prior review / impl | `.agents/idle_swarm/round_03/review_05_production.md`, `impl_02_production.md`; Kernel `impl_01_kernel.md` |

---

## Evidence snapshot

- `Logs/IdleAllSmoke-Summary.txt`: `result=Passed pass=72 fail=0`.
- `Logs/IdleBatchBC-Summary.txt`: `result=Passed pass=51 fail=0`.
- `Logs/IdleBatchBC-impl02-r3b.log` / AllSmoke R3 logs: `Melvor_PendingClaim_SurvivesPersistAndColdReload`, `Melvor_BootstrapLoadPath_CatchUpBanksPendingAndSkillLevel`, `Melvor_OfflineCatchUp_DoesNotBumpAfkChestSeconds`, `IdleMiner_OfflineCatchUp_AddsPrimaryCurrency_NoPendingClaim`, `EggInc_OfflineCatchUp_AddsPrimaryCurrency`, `PlayOrder_OfflineSimulationThenMelvorCatchUp_PreservesStamp`, `IdleMiner_Prestige_ReLocksManager_*` → Passed.
- `Logs/IdleKernel-TestResults.xml`: `OfflineSimulation_EmptyWorld_DoesNotStampLastIdleUpdateTime`, `ApplyPersistedElapsed_ZeroGrant_DoesNotStampLastIdleUpdateTime`, `Claim_PendingClaim_DoesNotAlsoPayAfkChest` → Passed.
- Commits on production path: `578a474` (Pending persist), `bd06da8` (D23/D25 stamp + CatchUp order).
- This Round 04 examine did **not** re-run Unity; status above is log + static re-read of HEAD.
- Play Mode offline: **UNVERIFIED**.

---

## Recommended implementer backlog (Round 04 IMPLEMENT — not executed)

**[required] Sync Melvor `IdleSkillNode` after CatchUp (close D25 skill orphan)**

- Criteria: After bootstrap Melvor CatchUp (Attach then ApplyPersistedElapsed), `IdleSkillNode.Level` / `Xp` / `XpToLevel` match post-catch-up slice progression. One subsequent sim tick must not drop ProgressionLevel below catch-up level.
- Must preserve D25 generator Sync-before-CatchUp and Melvor PendingClaim bank style.

**[required] Flush PendingClaim to prefs when CatchUp stamps (or stamp only after Persist)**

- Criteria: After Melvor CatchUp banks Pending and advances `LastIdleUpdateTime`, a cold `TryLoadIdleSlice` **without** waiting for the 2s autosave still restores that Pending (bootstrap PersistNow immediately post-CatchUp is enough). Machine: CatchUp → (optional crash mirror: no extra wait) → TryLoad asserts Pending.

**[required] Extend Melvor load-path test past bare state**

- Criteria: EditMode fixture attaches Melvor `IdleSkillNode` (or mirrors full Attach→CatchUp order) and asserts skill node + Pending together — not only `IdleSliceState` fields after helper Apply.

**[deferred] Per-archetype `LastIdleUpdateTime`** — multi-slice / multi-prefab hub.  
**[deferred] Persist Melvor SkillXp** — nice-to-have once skill node sync lands.  
**[deferred] Miner elevators / Egg research** — beyond MVP bar.  
**[deferred] Soft-cap claim formula for multi-slice hubs.**  
**[deferred] Play Mode wall-clock probe** — when MCP/editor bridge is on this repo.

---

## Confidence ranking (lowest first — for evaluator spot-check)

1. **Melvor IdleSkillNode orphan after D25 (P0)** — highest code confidence; TrySpawn order + Attach skill fields + sim level-up writeback read end-to-end; tests do not cover node sync.
2. **Stamp-before-Persist window (P1)** — high; ApplyPersistedElapsed stamps; no immediate PersistNow after CatchUp.
3. **R3 PendingClaim durability closed** — high; Save/Load keys + SurvivePersist fixture Passed in logs.
4. **R2 stamp race / Egg-Miner Primary / manager re-lock** — highest; unchanged closed items + logs.
5. **Global stamp key (P1 deferred)** — med; only bites multi-archetype sessions.

---

## Receipt meta

- **Implemented:** nothing  
- **Committed:** no  
- **Pushed:** no  
- **Play Mode:** UNVERIFIED (static analysis + prior EditMode logs only)
