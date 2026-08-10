# Round 06 Review 05 — Production / Offline / Managers (re-audit post R5)

**Agent:** 5/10 (EXAMINE / ANALYZE / REVIEW)  
**Scope:** Melvor Idle, Egg, Inc., Idle Miner Tycoon, offline rewards, automation/managers; adjacent Neko D26 CatchUp (cozy face on shared Kernel B)  
**Prior:** Round 05 `review_05_production.md` → Round 05 `impl_03_production.md` (`388eb02`) + Round 05 `impl_02_cozy.md` (`eef133f`, Neko D26)  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Mode:** Read-only analysis — no implementation, no push  
**Quality bar:** Prototype / toolkit MVP (`docs/project-context.md`); matrix DNA in `docs/idle_mechanics_matrix.md`

---

## ASSUMPTIONS

1. In-scope surface remains Kernel B toolkit slices (`IdleArchetype` + `IdleSliceState`) for Melvor / Egg / Miner, plus Neko CatchUp as shared `IdleOfflineCatchUp` contract — **high** — `STAMP_POLICY.md` + bootstrap / UI verbs.
2. Round 05 required bar = Melvor live `SpawnBootstrap` / `TrySpawn` load-path fixture; SkillXp persist was optional (“if cheap”) and landed — **high** — `review_05` backlog + `impl_03` receipt.
3. Neko D26 wall-clock cats owned by cozy `impl_02` (`eef133f`); fixture scooped into production commit `388eb02` — **high** — receipts + `IdleOfflineCatchUp.ApplyNekoCatchUp`.
4. EditMode mirrors prove stamp / skill / persist / Neko contracts; live Play Mode MonoBehaviour `Start` vs `InitializationSystemGroup` remains UNVERIFIED — **high** — no Play probe in logs.
5. This examine did not re-run Unity — **high** — static re-read of HEAD + existing logs (`IdleAllSmoke-Summary.txt` 103/103, `IdleBatchBC-Summary.txt` 60/60, `IdleBatchBC-impl03-r5.log`, `IdleBatchBC-impl02-cozy-r5.log`, `IdleAllSmoke-impl04-r5b.log`).

---

## Verdict (one line)

**R5 closed Melvor’s live TrySpawn load-path and SkillXp durability, and Neko D26 wall-clock cats now bank via CatchUp; Melvor/Egg/Miner offline + manager re-lock stay EditMode-green, with residuals only in Play probes, Neko live-bootstrap fixture gap, CatchUp ClickPower parity, IH/AFK chest CatchUp, and deferred matrix depth.**

---

## Round 05 required backlog — status

| Round 05 item | Impl claim | HEAD status | Evidence |
|---------------|------------|-------------|----------|
| Melvor `SpawnBootstrap` / live `TrySpawn` load-path fixture | met (`388eb02`) | **PASS (code + EditMode)** | `Melvor_SpawnBootstrap_TrySpawn_SyncsSkillNodeAndPersistsPending`: prefs + T−300s → reflected `TrySpawn`; asserts D33 Level/Xp match, `PendingClaim > 0`, cold `TryLoad` Pending + `LoadSkillXp` without 2s wait |
| SkillXp persist (optional / “if cheap”) | met (deviation from deferred) | **PASS** | `SaveIdleSlice` SkillXp key; `LoadSkillXp`; bootstrap restore; `PersistNow` reads `IdleSkillNode.Xp`; sim writes `slice.SkillXp` every skill tick; ClearIdleSlice deletes key; fixture asserts cold SkillXp |
| Must not re-break D25 / D33 / R4 PersistNow-on-grant | met | **PASS** | Live TrySpawn still: Attach Sync → CatchUp → SyncSkillNode → PersistNow when `catchUpGained > 0`; AttachThenCatchUp + SpawnBootstrap fixtures Passed |
| Must not re-break Egg/Miner Primary, OS empty-Init stamp, AfkChest honesty, Pending durability, manager re-lock | met | **PASS** | Egg/Miner → Primary; OS Producer-only stamp; D16 XOR; Pending prefs; `IdleMiner_Prestige_ReLocksManager_*` Passed in BC/AllSmoke logs |
| Neko D26 CatchUp cats (cozy peer) | met (`eef133f` + fixture scoop) | **PASS (helper + EditMode)** | `ApplyNekoCatchUp`: floor(elapsed/5) → CheckInCats cap 20; no Primary; D23 zero at cap/sub-interval; STAMP_POLICY rows; `NekoAtsume_OfflineCatchUp_AccruesCheckInCats` Passed |

Deferred (still correctly skipped): Play Mode wall-clock probe; CatchUp ClickPower parity; per-archetype stamp; Miner elevators / Egg research; claim soft-cap / multi-slice hubs; IH/AFK chest CatchUp (kernel D26 remainder).

---

## Architecture map (post R5)

| Path | When | Melvor | Egg / Miner (`PassiveRate>0`) | Neko | Cap | Stamps `LastIdleUpdateTime`? |
|------|------|--------|-------------------------------|------|-----|------------------------------|
| **B — `IdleOfflineCatchUp`** via bootstrap load | `TrySpawn` after Attach Sync when `_loadedFromPrefs` | Bank `PendingClaim` + skill XP ticks on **slice**; **D33** rewrite `IdleSkillNode`; **no** AfkChest | Add **directly** to `PrimaryCurrency` | `CheckInCats` += floor(t/5s) cap 20; `HasOfflineClaim`; **no** Primary | 8h | Only if grant **> 0** (D23); then **PersistNow** if grant > 0 (R4) |
| **A — `OfflineSimulationSystem`** | First Init tick, then disables | N/A | N/A | N/A | 8h | **Only if** ≥1 automated `ProducerComponent` catch-up applied |

Claim (`IdleClaimOfflineSystem`): D16 XOR — `PendingClaim` **or** chest/cats, never both. Honest no-op if empty. UI: Melvor “Claim Offline”; Egg/Miner none; Neko “Check In”.

Persist: `PendingClaim` + `HasOfflineClaim` + `ProgressionLevel` + **`SkillXp`** (R5) via `SaveIdleSlice` / `PersistNow`. Cozy `CheckInCats` / workers via same SaveIdleSlice params.

```
R5+ intended Play Mode:
  Quit → PersistNow writes Pending/SkillXp/Cats + stamps T0
  Init → OfflineSimulationSystem (empty / no producers) → leaves T0
  Start → Attach (Sync PassiveRate; restore SkillXp into IdleSkillNode) → IdleOfflineCatchUp(elapsed)
       → Melvor PendingClaim + skill ticks → SyncSkillNodeFromSlice
       → Egg/Miner Primary | Neko CheckInCats
       → stamp now if grant>0
       → PersistNow immediately (R4) flushes bank to prefs
```

Shared contract: `.agents/idle_swarm/round_02/STAMP_POLICY.md` (D33 Melvor sync + Neko Kernel B rows).

---

## Matrix vs implementation (scoped titles)

| Title | Matrix promise | Toolkit realization (HEAD) | Gap severity |
|-------|----------------|----------------------------|--------------|
| **Melvor Idle** | Grind skills; **total offline progression** | Online skill node OK; Pending durable; D33 node sync; live TrySpawn proven; SkillXp durable; claim honest; Play UNVERIFIED | **Low** (Play) — was Low–Med on SkillXp |
| **Egg, Inc.** | Hatch → habitats → soul eggs | Hatch + habitat CPS + prestige; catch-up → Primary + immediate Persist | **Low** vs MVP; Play still unverified |
| **Idle Miner Tycoon** | Shafts; super-managers; prestige mines | Shaft + hire + prestige re-lock; catch-up → Primary when automated | **Low** vs MVP bar |
| **Neko Atsume** (adjacent D26) | Absence / check-in fantasy | Online cats + CatchUp cats + SurvivePersist; no live TrySpawn AFK fixture; Play UNVERIFIED | **Low–Med** (fixture / Play) |

---

## Findings (ordered by severity)

### P1 — Play Mode wall-clock AFK still UNVERIFIED (unchanged, still deferred)

No PersistNow → stop Play → enter Play probe for Melvor/Egg/Miner/Neko. EditMode stamp/OS/CatchUp/TrySpawn fixtures remain the only evidence. Acceptable to keep deferred until MCP/editor bridge owns a Play probe; do not claim matrix “Absence Made Valuable” fully closed for players.

---

### P1 — Global `LastIdleUpdateTime` shared across all idle archetypes (unchanged)

`GameProgressData.LastIdleUpdateTime` is a single PlayerPrefs key (`HCR_LastIdleTime`), not per-archetype. Acceptable for single-slice prefab Play Mode; breaks multi-title hubs. Neko D26 positive grants now also consume this stamp (ZeroGrant fixture correctly moved off Neko to CookieClicker). Deferred unless stamp policy is touched again.

---

### P2 — Neko D26 lacks live `SpawnBootstrap` / `TrySpawn` CatchUp fixture

**Where:** `NekoAtsume_OfflineCatchUp_AccruesCheckInCats` exercises `IdleOfflineCatchUp.Apply` only. `NekoAtsume_CheckInCats_SurvivePersistNowReload` proves PersistNow cats durability after **manual** set, not wall-clock CatchUp inside real `TrySpawn`.

**Effect:** Melvor’s former false-green risk is closed; Neko’s CatchUp→stamp→PersistNow→cold `CheckInCats` pipe is implied by code (`ApplyNekoCatchUp` return > 0 → PersistNow writes `CheckInCats`) but not asserted end-to-end.

**Acceptance:** EditMode `SpawnBootstrap(NekoAtsume)` with T−N stamp + prefs → after `TrySpawn`, assert `CheckInCats` accrued (cap rules), Primary unchanged, and cold reload restores cats without a 2s wait.

---

### P2 — CatchUp does not bump `ClickPower` on offline Melvor level-ups (unchanged)

Online skill level-up: `ClickPower += 1`. `ApplyMelvorSkillTicks` / `SyncSkillNodeFromSlice` do not. Melvor UI `Train Skill` fires `FireClick(1f)` (fixed gain), so impact is mostly `BaseDamage` / vestigial ClickPower — MVP-acceptable. Close if Train Skill ever keys off ClickPower.

---

### P2 — Melvor click currency side-path (unchanged)

`IdleClickProduceSystem` Melvor arm still grants click currency; XP owned by skill node. Acceptable MVP.

---

### P2 — Hybrid Kernel A stamp can starve Kernel B (latent, unchanged)

If an automated `ProducerComponent` applies in the same Init pass, OS stamps before bootstrap CatchUp. Toolkit prefabs do not add producers today.

---

### P2 — IH / AfkArena closed-app chest CatchUp still absent (kernel D26 remainder)

Neko face closed; IH/AFK still accrue `AfkChestSeconds` online only. `IdleOfflineCatchUp` does not bump chest on PassiveRate=0 combat AFK titles. Out of Melvor/Egg/Miner required bar; note for combat/kernel — do not expand Production scope unless stamp policy adds IH/AFK rows.

---

### P2 — Idle Miner / Egg matrix depth (unchanged, still deferred)

Miner: no elevator/warehouse. Egg: no research/vehicles/contracts. Do not expand unless bar rises.

---

### P3 — Claim formula uncapped beyond chest fill / soft-cap (unchanged)

D16 XOR closed double-pay. Soft-cap deferred until multi-slice hubs.

---

### P3 — Neko CatchUp discards fractional timer remainder (latent)

Online Neko uses `IdleSliceState.Timer` (global += dt, fire at 5s). CatchUp uses `floor(elapsed/5)` and does not seed Timer with leftover seconds; Timer is not persisted. MVP-acceptable; at most one cat of pacing drift per cold load.

---

## Round 01–05 items that stay closed (do not re-open)

1. **Manager prestige re-lock** — hire keeps `RequiresManager`; prestige clears hire + `ResetBuyableGenerator`; BC tests Passed.
2. **Melvor demo +10** — removed; empty claim is no-op.
3. **Melvor dual continuous grind** — switch arm empty; skill node is online authority; `PassiveRate` aligned to `1/interval`.
4. **Init-group timestamp wipe (R2 P0)** — OS no longer stamps on empty Init; Producer-only.
5. **Dual Kernel-B OS slice loop (R2 P1)** — removed; sole slice writer = `IdleOfflineCatchUp`.
6. **Melvor AfkChest inflation from CPS catch-up (R2)** — CatchUp does not bump chest; claim XOR.
7. **Egg/Miner Claim UI** — correctly **not** required under PrimaryCurrency bank.
8. **Melvor PendingClaim non-persistence (R3 P0)** — **closed** via Pending/HasClaim PlayerPrefs + bootstrap restore.
9. **Zero-grant AFK wipe (D23)** — **closed**; `ApplyPersistedElapsed` stamps only if grant > 0 (ZeroGrant fixture now CookieClicker — Neko correctly grants/stamps).
10. **CatchUp Mult² before Sync (D25)** — **closed** for generator PassiveRate order.
11. **Melvor IdleSkillNode orphan after CatchUp (R4 P0 / D33)** — **closed** via bootstrap Sync + kernel/BC fixtures.
12. **CatchUp stamp before Pending flush (R4 P1)** — **closed** via immediate `PersistNow` when `catchUpGained > 0`.
13. **Melvor live TrySpawn load-path gap (R5 P1)** — **closed** via `Melvor_SpawnBootstrap_TrySpawn_SyncsSkillNodeAndPersistsPending` (`388eb02`).
14. **Melvor SkillXp session-only (R5 P2 / optional)** — **closed** via prefs key + PersistNow node read + every-tick slice sync + fixture cold assert.
15. **Neko wall-clock cats absent (D26 cozy face)** — **closed** via `ApplyNekoCatchUp` + STAMP_POLICY + EditMode helper fixture (`eef133f` / scoop).

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
| Prior review / impl | `.agents/idle_swarm/round_05/review_05_production.md`, `impl_03_production.md`, `impl_02_cozy.md` |

---

## Evidence snapshot

- `Logs/IdleAllSmoke-Summary.txt`: `result=Passed pass=103 fail=0`.
- `Logs/IdleBatchBC-Summary.txt`: `result=Passed pass=60 fail=0`.
- `Logs/IdleBatchBC-impl03-r5.log`: `Melvor_SpawnBootstrap_TrySpawn_SyncsSkillNodeAndPersistsPending`, `NekoAtsume_OfflineCatchUp_AccruesCheckInCats`, manager prestige / Melvor Pending / Egg/Miner CatchUp suite → Passed (`pass=60`).
- `Logs/IdleBatchBC-impl02-cozy-r5.log` / `IdleAllSmoke-impl04-r5b.log`: same Melvor TrySpawn + Neko CatchUp → Passed; AllSmoke `pass=103`.
- Commits: `388eb02` (Melvor TrySpawn + SkillXp + BC fixtures incl. Neko scoop), `eef133f` (Neko CatchUp + STAMP_POLICY); prior `195aa9f` (D33), `2b21929` (PersistNow flush), `578a474` (Pending persist), `bd06da8` (D23/D25).
- This Round 06 examine did **not** re-run Unity; status above is log + static re-read of HEAD.
- Play Mode offline: **UNVERIFIED**.

---

## Recommended implementer backlog (Round 06 IMPLEMENT — not executed)

**[required] none** — R5 production required bar is met on EditMode evidence.

**[deferred] Play Mode wall-clock probe** — when MCP/editor bridge is on this repo (Melvor Pending / Egg Primary / Neko cats after relaunch).

**[deferred] Neko `SpawnBootstrap` CatchUp fixture** — mirror Melvor live TrySpawn: T−N → `TrySpawn` → assert CheckInCats + cold persist (closes P2 false-green gap).

**[deferred] CatchUp ClickPower parity** — only if Train Skill / BaseDamage starts using ClickPower meaningfully.

**[deferred] Per-archetype `LastIdleUpdateTime`** — multi-slice / multi-prefab hub.

**[deferred] IH/AFK chest CatchUp (D26 remainder)** — combat/kernel ownership; STAMP_POLICY row required in same change.

**[deferred] Miner elevators / Egg research** — beyond MVP bar.

**[deferred] Soft-cap claim formula for multi-slice hubs.**

---

## Confidence ranking (lowest first — for evaluator spot-check)

1. **Play Mode AFK (P1 deferred)** — med confidence it still works given EditMode; zero live evidence.
2. **Neko live-bootstrap CatchUp gap (P2)** — high that helper path is correct; medium that PersistNow flush on TrySpawn is covered only by implication.
3. **R5 Melvor TrySpawn + SkillXp closed** — highest; code path + BC/AllSmoke logs Passed.
4. **Neko D26 helper + STAMP_POLICY** — highest; Apply arm + fixture Passed; ZeroGrant retargeted off Neko intentionally.
5. **R2–R4 stamp / AfkChest / Pending / manager re-lock** — highest; unchanged closed items + logs.

---

## Receipt meta

- **Implemented:** nothing  
- **Committed:** no  
- **Pushed:** no  
- **Play Mode:** UNVERIFIED (static analysis + prior EditMode logs only)
