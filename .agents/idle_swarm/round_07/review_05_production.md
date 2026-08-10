# Round 07 Review 05 — Production / Offline / Managers (re-audit post R6)

**Agent:** 5/10 (EXAMINE / ANALYZE / REVIEW)  
**Scope:** Melvor Idle, Egg, Inc., Idle Miner Tycoon, offline rewards, automation/managers; adjacent Neko D26 CatchUp (cozy face on shared Kernel B)  
**Prior:** Round 06 `review_05_production.md` → Round 06 `impl_05_production.md` (`ed123af`)  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Mode:** Read-only analysis — no implementation, no push  
**Quality bar:** Prototype / toolkit MVP (`docs/project-context.md`); matrix DNA in `docs/idle_mechanics_matrix.md`

---

## ASSUMPTIONS

1. In-scope surface remains Kernel B toolkit slices (`IdleArchetype` + `IdleSliceState`) for Melvor / Egg / Miner, plus Neko CatchUp as shared `IdleOfflineCatchUp` contract — **high** — `STAMP_POLICY.md` + bootstrap / UI verbs.
2. Round 06 **[required] = none**; optional “if cheap” = Neko live `SpawnBootstrap` CatchUp fixture (landed) and/or CatchUp ClickPower parity (skipped) — **high** — `review_05` + `impl_05` receipt.
3. Post-`ed123af` gacha-identity DNA writes in `PersistNow` / attach do **not** alter CatchUp stamp, Pending, SkillXp, or Neko cats paths — **high** — `git diff ed123af..HEAD` on bootstrap / `GameProgressData` is identity-key only.
4. EditMode mirrors prove stamp / skill / persist / Neko live load-path; live Play Mode MonoBehaviour `Start` vs `InitializationSystemGroup` remains UNVERIFIED — **high** — no Play probe in logs.
5. This examine did not re-run Unity — **high** — static re-read of HEAD + existing logs (`IdleBatchBC-Summary.txt` 64/64, `IdleBatchBC-impl09-cozy-r6c.log`, `IdleGacha-impl04-r6.log`; tip `IdleAllSmoke-Summary.txt` still 103/103 and may predate the Neko fixture count).

---

## Verdict (one line)

**R6 closed the last production EditMode false-green gap (Neko live TrySpawn CatchUp→PersistNow); Melvor/Egg/Miner/Neko offline + manager re-lock stay EditMode-green, with residuals only in Play probes, shared stamp, CatchUp ClickPower parity, Egg/Miner live-bootstrap fixture parity, IH/AFK chest CatchUp, and deferred matrix depth.**

---

## Round 06 required / optional backlog — status

| Round 06 item | Impl claim | HEAD status | Evidence |
|---------------|------------|-------------|----------|
| **[required] none** | n/a | **n/a** | R5 Melvor TrySpawn + SkillXp bar still closed; no required Production item |
| **[optional] Neko `SpawnBootstrap` CatchUp fixture** | met (`ed123af`) | **PASS (code + EditMode)** | `NekoAtsume_SpawnBootstrap_TrySpawn_AccruesCheckInCatsAndPersists`: prefs + T−50s → live `TrySpawn`; `CheckInCats == 10`, Primary unchanged, `HasOfflineClaim`, cold `LoadIdleCozyPersist` cats without 2s wait — Passed in `IdleBatchBC-impl09-cozy-r6c.log` / `IdleGacha-impl04-r6.log` |
| **[optional] CatchUp ClickPower parity** | skipped | **still deferred** | Train Skill still `FireClick(1f)`; online level-up still bumps `ClickPower`; `ApplyMelvorSkillTicks` / `SyncSkillNodeFromSlice` do not |
| Must not re-break Melvor D25/D33/R4 PersistNow, Egg/Miner Primary, OS empty-Init, Pending, manager re-lock, Neko helper CatchUp | met | **PASS** | Melvor TrySpawn + Egg/Miner CatchUp + `IdleMiner_Prestige_ReLocksManager_*` + Neko helper + PlayOrder stamp fixtures Passed in BC/Gacha logs |
| Deferred Play / per-archetype stamp / IH-AFK chest / elevators / soft-cap | skipped | **still deferred** | correctly skipped |

---

## Architecture map (post R6)

| Path | When | Melvor | Egg / Miner (`PassiveRate>0`) | Neko | Cap | Stamps `LastIdleUpdateTime`? |
|------|------|--------|-------------------------------|------|-----|------------------------------|
| **B — `IdleOfflineCatchUp`** via bootstrap load | `TrySpawn` after Attach Sync when `_loadedFromPrefs` | Bank `PendingClaim` + skill XP ticks on **slice**; **D33** rewrite `IdleSkillNode`; **no** AfkChest | Add **directly** to `PrimaryCurrency` | `CheckInCats` += floor(t/5s) cap 20; `HasOfflineClaim`; **no** Primary | 8h | Only if grant **> 0** (D23); then **PersistNow** if grant > 0 (R4) |
| **A — `OfflineSimulationSystem`** | First Init tick, then disables | N/A | N/A | N/A | 8h | **Only if** ≥1 automated `ProducerComponent` catch-up applied |

Claim (`IdleClaimOfflineSystem`): D16 XOR — `PendingClaim` **or** chest/cats, never both. Honest no-op if empty. UI: Melvor “Claim Offline”; Egg/Miner none; Neko “Check In”.

Persist: `PendingClaim` + `HasOfflineClaim` + `ProgressionLevel` + **`SkillXp`** (R5) via `SaveIdleSlice` / `PersistNow`. Cozy `CheckInCats` / workers via same SaveIdleSlice params. Post-R6 PersistNow also writes gacha identity keys (IH/LoM) — orthogonal to production offline bank.

```
R6+ intended Play Mode:
  Quit → PersistNow writes Pending/SkillXp/Cats + stamps T0
  Init → OfflineSimulationSystem (empty / no producers) → leaves T0
  Start → Attach (Sync PassiveRate; restore SkillXp into IdleSkillNode) → IdleOfflineCatchUp(elapsed)
       → Melvor PendingClaim + skill ticks → SyncSkillNodeFromSlice
       → Egg/Miner Primary | Neko CheckInCats
       → stamp now if grant>0
       → PersistNow immediately (R4) flushes bank to prefs
```

Live EditMode coverage now includes Melvor **and** Neko `SpawnBootstrap`/`TrySpawn` pipes (R5 + R6). Egg/Miner still helper-`Apply` only for CatchUp (Primary bank path is simpler; PersistNow-on-grant still shared).

Shared contract: `.agents/idle_swarm/round_02/STAMP_POLICY.md` (D33 Melvor sync + Neko Kernel B rows).

---

## Matrix vs implementation (scoped titles)

| Title | Matrix promise | Toolkit realization (HEAD) | Gap severity |
|-------|----------------|----------------------------|--------------|
| **Melvor Idle** | Grind skills; **total offline progression** | Online skill node OK; Pending durable; D33 node sync; live TrySpawn proven; SkillXp durable; claim honest; Play UNVERIFIED | **Low** (Play) |
| **Egg, Inc.** | Hatch → habitats → soul eggs | Hatch + habitat CPS + prestige; catch-up → Primary + immediate Persist; no live TrySpawn fixture | **Low** vs MVP; Play still unverified |
| **Idle Miner Tycoon** | Shafts; super-managers; prestige mines | Shaft + hire + prestige re-lock; catch-up → Primary when automated; no live TrySpawn fixture | **Low** vs MVP bar |
| **Neko Atsume** (adjacent D26) | Absence / check-in fantasy | Online cats + CatchUp cats + SurvivePersist + **live TrySpawn** fixture; Play UNVERIFIED | **Low** (Play only) — was Low–Med |

---

## Findings (ordered by severity)

### P1 — Play Mode wall-clock AFK still UNVERIFIED (unchanged, still deferred)

No PersistNow → stop Play → enter Play probe for Melvor/Egg/Miner/Neko. EditMode stamp/OS/CatchUp/TrySpawn fixtures remain the only evidence. Acceptable to keep deferred until MCP/editor bridge owns a Play probe; do not claim matrix “Absence Made Valuable” fully closed for players.

---

### P1 — Global `LastIdleUpdateTime` shared across all idle archetypes (unchanged)

`GameProgressData.LastIdleUpdateTime` is a single PlayerPrefs key (`HCR_LastIdleTime`), not per-archetype. Acceptable for single-slice prefab Play Mode; breaks multi-title hubs. Neko D26 positive grants consume this stamp (ZeroGrant fixture correctly on CookieClicker). Deferred unless stamp policy is touched again.

---

### P2 — CatchUp does not bump `ClickPower` on offline Melvor level-ups (unchanged)

Online skill level-up: `ClickPower += 1`. `ApplyMelvorSkillTicks` / `SyncSkillNodeFromSlice` do not. Melvor UI `Train Skill` fires `FireClick(1f)` (fixed gain), so impact is mostly `BaseDamage` / vestigial ClickPower — MVP-acceptable. Close if Train Skill ever keys off ClickPower. R6 intentionally skipped.

---

### P2 — Melvor click currency side-path (unchanged)

`IdleClickProduceSystem` Melvor arm still grants click currency; XP owned by skill node. Acceptable MVP.

---

### P2 — Hybrid Kernel A stamp can starve Kernel B (latent, unchanged)

If an automated `ProducerComponent` applies in the same Init pass, OS stamps before bootstrap CatchUp. Toolkit prefabs do not add producers today. `PlayOrder_OfflineSimulationThenMelvorCatchUp_PreservesStamp` still Passed.

---

### P2 — IH / AfkArena closed-app chest CatchUp still absent (kernel D26 remainder)

Neko face closed (helper + live TrySpawn). IH/AFK still accrue `AfkChestSeconds` online only. `IdleOfflineCatchUp` does not bump chest on PassiveRate=0 combat AFK titles. Out of Melvor/Egg/Miner required bar; note for combat/kernel — do not expand Production scope unless stamp policy adds IH/AFK rows.

---

### P2 — Idle Miner / Egg matrix depth (unchanged, still deferred)

Miner: no elevator/warehouse. Egg: no research/vehicles/contracts. Do not expand unless bar rises.

---

### P3 — Egg / Miner lack live `SpawnBootstrap` CatchUp fixtures (new residual after Melvor/Neko closed)

**Where:** Egg/Miner CatchUp covered by `EggInc_OfflineCatchUp_AddsPrimaryCurrency` / `IdleMiner_OfflineCatchUp_*` helper `Apply` only. Melvor + Neko now exercise real `TrySpawn`.

**Effect:** Lower false-green risk than Melvor/Neko were (Primary is always in core `SaveIdleSlice`; PersistNow-on-grant is shared). Still no assertion that Egg/Miner live load-path stamps + flushes Primary without a 2s wait.

**Acceptance (optional):** EditMode `SpawnBootstrap(EggInc|IdleMinerTycoon)` with T−N + prefs → after `TrySpawn`, assert Primary delta and cold `TryLoad` without 2s wait — only if implementer wants fixture parity; not required for MVP bar.

---

### P3 — Claim formula uncapped beyond chest fill / soft-cap (unchanged)

D16 XOR closed double-pay. Soft-cap deferred until multi-slice hubs.

---

### P3 — Neko CatchUp discards fractional timer remainder (latent, unchanged)

Online Neko uses `IdleSliceState.Timer` (global += dt, fire at 5s). CatchUp uses `floor(elapsed/5)` and does not seed Timer with leftover seconds; Timer is not persisted. MVP-acceptable; at most one cat of pacing drift per cold load.

---

## Round 01–06 items that stay closed (do not re-open)

1. **Manager prestige re-lock** — hire keeps `RequiresManager`; prestige clears hire + `ResetBuyableGenerator`; BC tests Passed.
2. **Melvor demo +10** — removed; empty claim is no-op.
3. **Melvor dual continuous grind** — switch arm empty; skill node is online authority; `PassiveRate` aligned to `1/interval`.
4. **Init-group timestamp wipe (R2 P0)** — OS no longer stamps on empty Init; Producer-only.
5. **Dual Kernel-B OS slice loop (R2 P1)** — removed; sole slice writer = `IdleOfflineCatchUp`.
6. **Melvor AfkChest inflation from CPS catch-up (R2)** — CatchUp does not bump chest; claim XOR.
7. **Egg/Miner Claim UI** — correctly **not** required under PrimaryCurrency bank.
8. **Melvor PendingClaim non-persistence (R3 P0)** — **closed** via Pending/HasClaim PlayerPrefs + bootstrap restore.
9. **Zero-grant AFK wipe (D23)** — **closed**; `ApplyPersistedElapsed` stamps only if grant > 0 (ZeroGrant fixture CookieClicker — Neko correctly grants/stamps).
10. **CatchUp Mult² before Sync (D25)** — **closed** for generator PassiveRate order.
11. **Melvor IdleSkillNode orphan after CatchUp (R4 P0 / D33)** — **closed** via bootstrap Sync + kernel/BC fixtures.
12. **CatchUp stamp before Pending flush (R4 P1)** — **closed** via immediate `PersistNow` when `catchUpGained > 0`.
13. **Melvor live TrySpawn load-path gap (R5 P1)** — **closed** via `Melvor_SpawnBootstrap_TrySpawn_SyncsSkillNodeAndPersistsPending` (`388eb02`).
14. **Melvor SkillXp session-only (R5 P2 / optional)** — **closed** via prefs key + PersistNow node read + every-tick slice sync + fixture cold assert.
15. **Neko wall-clock cats absent (D26 cozy face)** — **closed** via `ApplyNekoCatchUp` + STAMP_POLICY + EditMode helper fixture (`eef133f` / scoop).
16. **Neko live TrySpawn CatchUp false-green gap (R6 P2)** — **closed** via `NekoAtsume_SpawnBootstrap_TrySpawn_AccruesCheckInCatsAndPersists` (`ed123af`).

---

## Adjacent non-regression note (post R6)

Commit(s) after `ed123af` added IH/LoM gacha identity DNA (`SaveGachaIdentity` / load into attach). Diff touches `IdleSliceBootstrap.PersistNow` / load fields and `GameProgressData` keys only — **no** change to `IdleOfflineCatchUp`, OS stamp policy, Melvor SkillXp, or Neko cats math. Production offline contracts remain intact on static read; latest BC/Gacha logs that include Melvor + Neko TrySpawn fixtures still Passed.

---

## File index (absolute paths)

| Role | Path |
|------|------|
| Kernel B catch-up helper + D33 sync + Neko D26 | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleOfflineCatchUp.cs` |
| Bootstrap load + CatchUp + Sync + PersistNow + SkillXp | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Authoring\IdleSliceBootstrap.cs` |
| Init-group offline (A only) | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\OfflineSimulationSystem.cs` |
| Claim / D16 XOR | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleSliceActionSystems.cs` |
| Melvor online tick + SkillXp lockstep | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleSliceSimulationSystem.cs` |
| Hire / buy | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdleBuyGeneratorSystem.cs` |
| Prestige reset | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\PrestigeSystem.cs` |
| Shared gate math | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\ECS\Systems\Idle\IdlePrestigeMath.cs` |
| Quit/pause stamp | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\UI\IdleSaveManager.cs` |
| UI verbs | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\UI\IdleSliceUIController.cs` |
| Persist + timestamp + SkillXp | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\GameProgressData.cs` |
| BC smoke + production / Neko fixtures | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\Editor\Tests\IdleBatchBCSmokeTests.cs` |
| Kernel offline + D33 / D23 fixtures | `D:\Git\Hyper-Casual-Runner\Assets\Scripts\Editor\Tests\IdleKernelCorrectnessTests.cs` |
| Stamp contract | `D:\Git\Hyper-Casual-Runner\.agents\idle_swarm\round_02\STAMP_POLICY.md` |
| Prior review / impl | `.agents/idle_swarm/round_06/review_05_production.md`, `impl_05_production.md` |

---

## Evidence snapshot

- `Logs/IdleBatchBC-Summary.txt`: `result=Passed pass=64 fail=0`.
- `Logs/IdleAllSmoke-Summary.txt`: `result=Passed pass=103 fail=0` (tip may predate Neko live-bootstrap fixture; prefer BC/Gacha logs below for R6 fixture).
- `Logs/IdleBatchBC-impl09-cozy-r6c.log`: `NekoAtsume_SpawnBootstrap_TrySpawn_AccruesCheckInCatsAndPersists`, `Melvor_SpawnBootstrap_TrySpawn_SyncsSkillNodeAndPersistsPending`, `EggInc_OfflineCatchUp_*`, `IdleMiner_OfflineCatchUp_*`, `IdleMiner_Prestige_ReLocksManager_*`, `PlayOrder_OfflineSimulationThenMelvorCatchUp_PreservesStamp` → Passed.
- `Logs/IdleGacha-impl04-r6.log`: same Melvor + Neko TrySpawn fixtures → Passed.
- Commits: `ed123af` (Neko live TrySpawn fixture); prior `388eb02` (Melvor TrySpawn + SkillXp), `eef133f` (Neko CatchUp + STAMP_POLICY), `195aa9f` (D33), `2b21929` (PersistNow flush), `578a474` (Pending persist), `bd06da8` (D23/D25).
- This Round 07 examine did **not** re-run Unity; status above is log + static re-read of HEAD.
- Play Mode offline: **UNVERIFIED**.
- **Implemented / committed / pushed this examine:** nothing / no / no.

---

## Recommended implementer backlog (Round 07 IMPLEMENT — not executed)

**[required] none** — R6 production optional bar met on EditMode evidence; R5 required bar still closed.

**[deferred] Play Mode wall-clock probe** — when MCP/editor bridge is on this repo (Melvor Pending / Egg Primary / Neko cats after relaunch).

**[deferred] CatchUp ClickPower parity** — only if Train Skill / BaseDamage starts using ClickPower meaningfully.

**[deferred] Per-archetype `LastIdleUpdateTime`** — multi-slice / multi-prefab hub.

**[deferred] IH/AFK chest CatchUp (D26 remainder)** — combat/kernel ownership; STAMP_POLICY row required in same change.

**[deferred] Egg/Miner live `SpawnBootstrap` CatchUp fixtures** — optional parity with Melvor/Neko; not MVP-blocking.

**[deferred] Miner elevators / Egg research** — beyond MVP bar.

**[deferred] Soft-cap claim formula for multi-slice hubs.**

---

## Confidence ranking (lowest first — for evaluator spot-check)

1. **Play Mode AFK (P1 deferred)** — med confidence it still works given EditMode; zero live evidence.
2. **Egg/Miner live-bootstrap fixture gap (P3)** — high that helper + shared PersistNow is correct; medium that TrySpawn Primary flush is covered only by implication.
3. **R6 Neko live TrySpawn closed** — highest; code path + BC/Gacha logs Passed.
4. **R5 Melvor TrySpawn + SkillXp closed** — highest; unchanged + logs Passed.
5. **R2–R4 stamp / AfkChest / Pending / manager re-lock** — highest; unchanged closed items + logs.

---

## Receipt meta

- **Implemented:** nothing  
- **Committed:** no  
- **Pushed:** no  
- **Play Mode:** UNVERIFIED (static analysis + prior EditMode logs only)
