# Round 07 Review 10/10 — LLM Compose / Docs (post–R6)

**Agent:** examine/analyze/review  
**From:** `round_06/review_10_llm.md` + `round_06/impl_06_llm.md` (`06f2617`)  
**Scope:** Re-audit progress/compose/checklist honesty + Play Mode 0/19 + Synergism #20 deferral after Round 06 PrestigeFaction fixture-list sync and sibling AllSmoke leaf growth  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Date:** 2026-08-10  
**Implements:** none (review only). Never push.

---

## Verdict

**Round 06 LLM P1 landed and holds.** `impl_06_llm` (`06f2617`, note commit `a17a07f`) named `IdlePrestigeFactionBonusTests` / PrestigeFaction in progress Evidence + claim + Run-tests prose (and README already had it via `bd597b0`). Compose smoke map remains **20/20**. Seed checklist B/C HUD rows still match `WritePlaySmokeChecklist`. Menus stay `MVP/` vs `RunnerSlices/`. Honesty chain still says **Play Mode 0/19**. Synergism **#20** remains documented-deferred with enum capped at `FalloutShelter = 18`; R5-C6 Synergy≠Synergism name-trap still present.

**New standing gap (call out explicitly):** tip AllSmoke integer re-drifted under sibling R6 churn. Progress and tip Summary still lockstep at **103/103**, but live AllSmoke leaf inventory is **109** (A23 + BC64 + K15 + Cos6 + Prest1). Tip Summary was written **04:57**; BC/Kernel fixtures grew after that (`IdleBatchBC-Summary.txt` already `pass=64`). Progress matching tip is no longer matching HEAD test surface.

**Standing gaps (unchanged quality blockers):**
1. **Play Mode 0/19** — EditMode tip/progress numbers do not close quality; MCP still on `thepcgtoolkit`; Batch A Play 01/03/04 BLOCKED (R5-A4 / R6-A4).
2. **Synergism #20** — matrix/compose/progress keep the deferred candidate; gate remains “Play Mode Batch A green on Hyper-Casual-Runner” before any 20th archetype. Do not confuse with legacy `SynergySystem` (compose name-trap).

**Bottom line:** R6 PrestigeFaction fixture-list honesty holds. Remaining LLM work is **re-run AllSmoke → refresh progress to the new tip** (do not invent **109** as the scoreboard until tip Summary says so), keep Play **0/19**, keep Synergism deferred. Do **not** treat EditMode 103 (or any future green tip) as Play.

---

## Assumptions (ledger)

| # | Assumption | Confidence | Verified by |
|---|------------|------------|-------------|
| 1 | R6 impl_06 claimed R6-C1..C6 MET at commit `06f2617` (103/103 + PrestigeFaction named) | high | `round_06/impl_06_llm.md`; `git log` `06f2617` |
| 2 | Sibling R6 churn added EditMode leaves after tip Summary (gacha identity + Neko TrySpawn + kernel D27/D31) without refreshing AllSmoke tip | high | tip Summary 04:57 `pass=103`; BC Summary `pass=64`; live `[Test]` sum 109; commits after `06f2617` touch BC/Kernel tests |
| 3 | Authoritative EditMode count = tip `Logs/IdleAllSmoke-Summary.txt` (not progress prose, not leaf-attr sum alone) | high | progress itself says tip is authoritative; tip still `pass=103` |
| 4 | Piece-flag / GenerateCustomIdleSlice still deferred | high | compose Non-negotiable / R6-C7 |
| 5 | Play Mode still 0/19; Synergism still deferred | high | progress + checklist + matrix + compose + `IdleArchetype` enum + no Idle `*Synerg*` prefab |
| 6 | Legacy `SynergySystem.cs` ≠ matrix Synergism #20; R5-C6 compose note remains | high | compose § Matrix gap name-trap line present |

---

## Round 06 acceptance re-score (post–sibling churn)

| ID | Criterion | R6 claim | Round 07 finding |
|----|-----------|----------|------------------|
| R6-C1 | Progress AllSmoke **integer** still matches tip Summary (EditMode-only; Play stays 0/19) | MET (103/103) | **PASS lockstep / tip STALE vs HEAD** — progress **103/103** = tip Summary **103/103**; live leaf sum **109**; Play still **0/19** |
| R6-C2 | Progress (+ README) name live AllSmoke fixtures incl. **IdlePrestigeFactionBonusTests** | MET | **PASS** — Evidence / claim / Run-tests / README all name PrestigeFaction |
| R6-C3 | All compose smoke cells still resolve (full 20-ref re-grep) | MET | **PASS** — `refs=20 ok=20 miss=0` |
| R6-C4 | Seed checklist B/C HUD rows still match `WritePlaySmokeChecklist` | MET | **PASS** — LoM / Capybara / Cats / Neko / Fallout identical |
| R6-C5 | Play column / Play Mode claim remains **0/19** | MET | **PASS** — progress + checklist + README + PROJECT M6 + writer + R6-A4 BLOCKED note |
| R6-C6 | Synergism #20 deferred; no enum/prefab; Synergy≠Synergism note kept | MET | **PASS** — matrix/compose/progress deferred; enum ends at 18; 0 Synerg prefabs; name-trap present |
| R6-C7 | Piece-flag / GenerateCustomIdleSlice | DEFERRED | **Still deferred** |

### R6-C1 evidence (tip vs progress vs HEAD leaves)

| Source | Count / note |
|--------|----------------|
| `Logs/IdleAllSmoke-Summary.txt` (2026-08-10 04:57) | `result=Passed pass=103 fail=0` duration=3.4890533 |
| `Logs/IdleAllSmoke-TestResults.xml` | `total="103" passed="103" failed="0"` |
| `docs/idle-toolkit-progress.md` Evidence + claim tables | **PASS 103/103** / **103/103**; cites tip + `IdlePrestige-impl09-r5b.log` (duration-matched tip writer) |
| `Logs/IdleBatchBC-Summary.txt` (05:13) | `pass=64` — newer than tip; shows BC growth already landed |
| `Logs/IdleBatchA-Summary.txt` | `pass=23` |
| Live `[Test]` attr sum in AllSmoke leafs | A23 + BC64 + K15 + Cos6 + Prestige1 = **109** |
| Delta since `06f2617` | BC +4 (gacha identity / Neko TrySpawn); Kernel +2 (D27 second-claim + D31 destroy-guard) |

Scope note: AllSmoke runner filter is still **A + BC + Kernel + CosmeticsShop + IdlePrestigeFactionBonusTests** (R6-C2 composition naming still accurate). Only the **integer** is soft-stale: tip+progress agree at 103 while HEAD leaves imply ~109 after a fresh AllSmoke run.

### R6-C2 evidence (PrestigeFaction named)

| Surface | State |
|---------|-------|
| Progress Evidence row | lists `IdlePrestigeFactionBonusTests` |
| Progress claim parenthetical | `A+BC+Kernel+Cosmetics+PrestigeFaction` |
| Progress Run-tests paragraph | names Prestige |
| README fixture line | `… + PrestigeFaction in AllSmoke` |
| `IdleBatchATestRunner.RunAllIdleSmokeAndExit` | includes `IdlePrestigeFactionBonusTests` |

### R6-C3 evidence (smoke grep)

| Check | Result |
|-------|--------|
| Unique `IdleBatch*SmokeTests.<Method>` refs in compose | **20** |
| Resolve to `public void <Method>(` in Batch A or BC | **20/20** |
| Stale `RealmGrinder_BuildThenAlignFaction_RaisesMultAndLevel` | **Absent** from compose |
| Compose cites `IdlePrestigeFactionBonusTests.RealmGrinder_AlignEvil_…` | Present — correct |

### R6-C4 evidence (seed ↔ writer)

| Row | Seed checklist | `WritePlaySmokeChecklist` |
|-----|----------------|---------------------------|
| LoM | `Rub Lamp → auto-lamp at Stage≥1` | same |
| Capybara | `Take Step unlocks auto-tiles → Next Step → Lucky Find` | same |
| Cats | `Assign Cat → Unassign` | same |
| Neko | `Place Food → Place Toys → Check In` | same |
| Fallout | `Assign Dweller → Unassign → Claim Production` | same |
| Status | Play **0/19**; ~17/19 verb honesty | same |
| Anti-regression | `Farm Stage Gold` absent from seed Expected column | **Absent** (button still exists in UI; compose LoM parenthetical only) |

---

## What still works (do not regress)

1. **Compose entrypoint** — `docs/idle-toolkit-compose.md`: stack warning, piece table, 19-row map (live smoke names), authoring fan-out, Synergism #20 section + SynergySystem name-trap, menu table.
2. **Menu namespace** — Editor `[MenuItem]` only under `IdleToolkit/MVP/…` and `IdleToolkit/RunnerSlices/…`.
3. **HUD fidelity** — compose Ideal HUD named verbs map to `IdleSliceUIController` `Btn` labels (`Tap / Attack` is one label; Idle Heroes still no Auto Fight; AFK = Campaign Progress; Realm + Rebirth; Neko + Place Toys; Miner Hire Super-Manager). `Farm Stage Gold` remains UI-only / LoM parenthetical — not a seed Expected primary verb.
4. **Honesty chain** — Play Mode **0/19** in progress, checklist, README, PROJECT M6, regenerator; EditMode ≠ Play called out; Batch A Play 01/03/04 BLOCKED annotated (R5-A4 + R6-A4).
5. **Synergism #20 documented as gap** — matrix §coverage gap + compose §Matrix gap + progress header; implement gate explicit; R5-C6 name-trap holds.
6. **Seed ↔ regenerator** — R5-C3 / R6-C4 alignment still holds (do not re-diverge LoM/Capybara/Cats/Neko/Fallout prose).
7. **R6-C2 PrestigeFaction composition naming** — progress/README match runner filter (closed R6 soft honesty debt).

---

## Surface inventory (current)

### A. Menus

| MenuItem | LLM fitness |
|----------|-------------|
| `IdleToolkit/MVP/Generate All Idle MVP Slices` | Bulk regen OK; still all-or-nothing |
| `IdleToolkit/MVP/Open Idle Prefab Folder` | Good |
| `IdleToolkit/MVP/Smoke Batch A (instructions)` | OK |
| `IdleToolkit/MVP/Run Batch A Smoke And Exit` | Good agent/CI |
| `IdleToolkit/MVP/Human Play-Smoke Instructions` | Good — writes checklist + dialog |
| `IdleToolkit/RunnerSlices/Generate 21 Playable Slices` (+ verify) | Correctly namespaced |

### B. Docs

| Doc | Round 07 note |
|-----|---------------|
| `docs/idle-toolkit-compose.md` | Keep; smoke 20/20; #20 deferred; Synergy≠Synergism present |
| `docs/idle-play-smoke-checklist.md` | Seed usable; B/C rows match writer; Play 0/19 honest; R5-A4 + R6-A4 BLOCKED notes present |
| `docs/idle-toolkit-progress.md` | Play 0/19 + Synergism deferred + PrestigeFaction named OK; **AllSmoke 103/103 matches tip but tip stale vs HEAD ~109 leaves** |
| `docs/idle_mechanics_matrix.md` | Pointer + #20 gap OK; gate = Play Mode Batch A |
| `Assets/ToolkitExamples/Idle/README_IDLE_SLICES.txt` | Menus + contract + Play 0/19 + PrestigeFaction in AllSmoke aligned |
| `PROJECT.md` M6 | Accurate enum-gated / Play 0/19 wording |

### C. Architecture (unchanged contract)

- Behavior = `switch (IdleArchetype)` fan-out — not piece flags.
- `IdleArchetype` = 0..18 only (no Synergism).
- Dual legacy stack still in tree (`SynergySystem` / Producer path); compose warns off Producer/ArcadeIdle **and** names Synergy≠Synergism.
- Generator = fixed 19 `Slices[]`; no Generate One / custom-piece API.
- AllSmoke = A + BC + Kernel + Cosmetics + **PrestigeFactionBonus**.

---

## Fidelity debt (Round 07)

### P0 — Truth for LLM agents

1. **AllSmoke tip integer soft-stale** — progress **103/103** matches tip Summary **103/103**, but HEAD leaf inventory is **109** and BC Summary already reports **64**. Re-run `RunAllIdleSmokeAndExit`, then sync progress Evidence/claim to the new tip. Do **not** invent **109** as the scoreboard until tip Summary says so. Prefer tip Summary if sibling churn re-drifts again. Keep Play at **0/19**.

### P1 — Soft honesty / quality loop

2. **Play Mode 0/19 + MCP wrong project** — checklist exists; quality loop cannot close. Progress blockers still list `thepcgtoolkit`; R5-A4 / R6-A4 keep 01/03/04 BLOCKED.
3. **Synergism #20 gap (deferred by design)** — documented; must stay unimplemented until Play Mode Batch A is green. Distinct DNA: achievement-gated generators / completionism→power (not Antimatter layers, not Realm factions). **Not** the legacy `SynergySystem` on `ProducerComponent` (compose name-trap already present).
4. **Archetype monolith** — novel idle = enum fan-out; hybrids without enum unsupported (honestly stated).

### P2 — Polish

5. Cosmetics / Actions tabs exist in UI but are absent from compose “ideal journey” (`Cosmetics`, `Primary Action` Btn labels not in compose map — expected).
6. No per-slice LLM brief beyond HowTo (start currency, prestige gate).
7. Old `.agents/explorer_m*/handoff.md` may still cite pre-`MVP/` / pre-`RunnerSlices/` paths.
8. Hardcoded generator tunables still undocumented as intentional MVP constants.
9. Progress Evidence still points at prestige-era tip-writing log (`IdlePrestige-impl09-r5b.log`) — fine while tip stays 103; after AllSmoke re-run, retarget cite to the new tip-writing log + duration.
10. LoM `Farm Stage Gold` button remains UI identity (sibling/gacha lane); seed Expected column correctly omits it per R5 sync.

---

## Play Mode 0/19 (standing)

| Source | Claim |
|--------|-------|
| `docs/idle-toolkit-progress.md` | Play-mode verified **0/19**; MCP UNVERIFIED on wrong project; R5-A4 / R6-A4 Batch A Play BLOCKED |
| `docs/idle-play-smoke-checklist.md` | Play Mode **0/19**; 01/03/04 **BLOCKED** (R5-A4 + R6-A4 notes) |
| `README_IDLE_SLICES.txt` | Play Mode **0/19** |
| `PROJECT.md` M6 | Play 0/19 |
| `WritePlaySmokeChecklist` | Play Mode 0/19 |
| Latest AllSmoke summary | EditMode **103/103** only (stale vs HEAD leaves) — not Play |

**Do not** let AllSmoke green, fixture 19/19, Cosmetics/Prestige-in-AllSmoke, gacha/kernel leaf growth, or progress scorecard ~17/19 inflate Play claims. Human/MCP Play-Smoke on **Hyper-Casual-Runner** is still required before any Playable marks.

---

## Synergism #20 gap (standing)

| Surface | State |
|---------|-------|
| Matrix | § Matrix coverage gap — Synergism deferred; gate = Play Mode Batch A on Hyper-Casual-Runner |
| Compose | § Matrix gap (candidate #20 — deferred) — same gate + R5-C6 SynergySystem name-trap |
| Progress | Header: “19 titles + candidate #20 Synergism deferred” |
| Code | `IdleArchetype` ends at `FalloutShelter = 18` — no enum / prefab / smoke |
| Prefabs | No `*Synerg*` under `Assets/ToolkitExamples/Idle/` (count **0**) |

**Gap filled if/when built:** achievement-gated generators (completionism → power).  
**Not now:** do not implement in Round 07; Play Mode 0/19 is the hard prerequisite called out in matrix/compose.  
**Name trap:** `Assets/Scripts/ECS/Systems/Idle/SynergySystem.cs` is legacy Producer synergy, not candidate #20 (documented in compose).

---

## Ideal LLM journey (still valid)

```text
1. Read docs/idle-toolkit-compose.md (contract + 19 map + #20 deferral + Synergy≠Synergism)
2. Use IdleToolkit/MVP/* only (never RunnerSlices for idle MVPs)
3. Clone nearest prefab OR Generate All → retune inspector / HowTo
4. EditMode: IdleToolkit/MVP/Run Batch A Smoke And Exit (or AllSmoke batchmode)
5. Play: Human Play-Smoke Instructions → docs/idle-play-smoke-checklist.md
6. Mark Playable ONLY after Hyper-Casual-Runner Play evidence
7. Do NOT add Synergism / #20 until Play Mode Batch A is green
8. Prefer Logs/IdleAllSmoke-Summary.txt over progress for live EditMode count if they disagree
9. If tip Summary timestamp predates recent test commits, re-run AllSmoke before trusting 103
```

Novel / hybrid composition still requires compose §Authoring loop enum fan-out.

---

## Acceptance criteria for a future implementer (Round 07)

| ID | Criterion | Check |
|----|-----------|-------|
| R7-C1 | Re-run AllSmoke; progress AllSmoke **integer** matches fresh `Logs/IdleAllSmoke-Summary.txt` pass count (EditMode-only; Play stays 0/19). Expect tip to move off 103 (live leaves currently **109**) — use tip number, not this review’s leaf sum | diff numbers after batchmode |
| R7-C2 | Progress (+ README) still name live AllSmoke fixtures: A + BC + Kernel + CosmeticsShop + **IdlePrestigeFactionBonusTests** | grep progress/README vs runner |
| R7-C3 | All compose smoke cells still resolve (full 20-ref re-grep) | script like this audit |
| R7-C4 | Seed checklist B/C HUD rows still match `WritePlaySmokeChecklist` (or seed documents “run menu to refresh” as sole canonical) | diff LoM/Capybara/Cats/Neko/Fallout |
| R7-C5 | Play column / Play Mode claim remains **0/19** until Play evidence | progress + checklist + README |
| R7-C6 | Synergism #20 remains deferred in matrix + compose + progress; no enum/prefab; keep Synergy≠Synergism note | grep + enum read |
| R7-C7 (deferred) | Piece-flag / GenerateCustomIdleSlice — out of scope unless product asks | — |

---

## Evidence citations (read paths)

- `docs/idle-toolkit-compose.md` (19-row map; Synergism § + SynergySystem name-trap; smoke 20 refs)
- `docs/idle-play-smoke-checklist.md` (Play 0/19; seed B/C rows; R5-A4 / R6-A4 BLOCKED)
- `docs/idle-toolkit-progress.md` (Play 0/19; Synergism deferred; AllSmoke **103/103**; PrestigeFaction named)
- `docs/idle_mechanics_matrix.md` § Matrix coverage gap
- `Assets/ToolkitExamples/Idle/README_IDLE_SLICES.txt`
- `Assets/Scripts/Editor/IdleToolkitSliceGenerator.cs` (`WritePlaySmokeChecklist`)
- `Assets/Scripts/Editor/IdleBatchATestRunner.cs` (`RunAllIdleSmokeAndExit` → A+BC+Kernel+Cosmetics+Prestige)
- `Assets/Scripts/Editor/ToolkitExampleGenerator.cs` (`RunnerSlices/` MenuItems)
- `Assets/Scripts/UI/IdleSliceUIController.cs` (`AddButtonsForArchetype`)
- `Assets/Scripts/Editor/Tests/IdleBatchASmokeTests.cs`, `IdleBatchBCSmokeTests.cs`, `IdleKernelCorrectnessTests.cs`, `CosmeticsShopTests.cs`, `IdlePrestigeFactionBonusTests.cs`
- `Assets/Scripts/ECS/Components/IdleSliceComponents.cs` (`IdleArchetype` 0..18)
- `Assets/Scripts/ECS/Systems/Idle/SynergySystem.cs` (legacy — not #20)
- `PROJECT.md` M6
- `Logs/IdleAllSmoke-Summary.txt` (103/103 EditMode — tip at 04:57)
- `Logs/IdleAllSmoke-TestResults.xml` (`passed="103"`)
- `Logs/IdleBatchBC-Summary.txt` (`pass=64` — newer than tip)
- `Logs/IdleBatchA-Summary.txt` (`pass=23`)
- `Logs/IdlePrestige-impl09-r5b.log` (tip-writing log duration-matched to 103)
- `.agents/idle_swarm/round_06/impl_06_llm.md`, `review_10_llm.md`

---

## Status

```
TASK: Round 07 examine 10/10 — re-audit LLM compose/docs honesty (Play 0/19 + Synergism #20) post–R6
ROUTE: Examine/Analyze/Review only
ASSUMPTIONS: 6 listed; tip 103 vs progress 103 lockstep verified; HEAD leaves 109 (tip soft-stale); PrestigeFaction naming holds; Synergy≠Synergism verified present
CHANGES: wrote .agents/idle_swarm/round_07/review_10_llm.md only
VERIFICATION: tip Summary+XML 103/103; progress 103/103 MATCH tip; live leaf sum 109 + BC Summary 64 prove tip stale; smoke grep 20/20 OK; seed↔writer B/C OK; menus MVP/RunnerSlices OK; Play 0/19 + Synergism #20 deferred confirmed; R6-C2 PrestigeFaction naming holds; R5-C6 name-trap still present
STATUS: VERIFIED as review artifact (no implementation; never push)
RISKS: agents may treat 103 as current EditMode truth until AllSmoke is re-run; inventing 109 without tip Summary would also be dishonest; regenerator may overwrite seed when menu is run
```

**Do not implement from this review in the examine phase.** Hand R7-C1 (AllSmoke re-run + progress tip sync) to implement swarm; keep R7-C2/C5/C6 honest; keep Synergism #20 and piece-flag DSL deferred; keep Play Mode 0/19 until Hyper-Casual-Runner Play evidence exists.
