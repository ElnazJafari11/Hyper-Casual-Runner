# Round 08 Review 10/10 — LLM Compose / Docs (post–R7)

**Agent:** examine/analyze/review  
**From:** `round_07/review_10_llm.md` + `round_07/impl_03_llm.md`  
**Scope:** Re-audit progress/compose/checklist honesty + Play Mode 0/19 + Synergism #20 deferral after Round 07 AllSmoke tip rewrite to **114/114**  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Date:** 2026-08-10  
**Implements:** none (review only). Never push.

---

## Verdict

**Round 07 LLM P0 (R7-C1) landed and holds at HEAD.** `impl_03_llm` synced progress Evidence/claim to tip **114/114** with tip-writing log `Logs/IdleAllSmoke-impl03-r7.log` (`duration=7.2796769`). Live tip Summary + XML + AllSmoke leaf inventory now agree: **114/114** (A23 + BC68 + K16 + Cos6 + Prest1). PrestigeFaction naming from R6 still holds. Compose smoke map remains **20/20**. Seed checklist B/C HUD rows still match `WritePlaySmokeChecklist`. Menus stay `MVP/` vs `RunnerSlices/`. Honesty chain still says **Play Mode 0/19**. Synergism **#20** remains documented-deferred with enum capped at `FalloutShelter = 18`; R5-C6 Synergy≠Synergism name-trap still present.

**Post–R7 sibling note (not tip-integer stale):** gacha commit `09b9b72` added four Capybara DNA EditMode leaves *and* Fight Path / Safe Path HUD buttons. Tip/progress/HEAD leaf counts remain lockstep at **114** (those leaves were already in the tip inventory). Soft doc drift is Ideal-HUD naming only (compose still lists Take Step / Next Step / Lucky Find without Fight/Safe Path).

**Standing gaps (unchanged quality blockers):**
1. **Play Mode 0/19** — EditMode tip/progress **114/114** do not close quality; MCP still on `thepcgtoolkit`; Batch A Play 01/03/04 BLOCKED (R5-A4 / R6-A4 / R7-A4).
2. **Synergism #20** — matrix/compose/progress keep the deferred candidate; gate remains “Play Mode Batch A green on Hyper-Casual-Runner” before any 20th archetype. Do not confuse with legacy `SynergySystem` (compose name-trap).

**Bottom line:** R7 tip/progress honesty is green at **114/114**. Remaining LLM work is keep Play **0/19**, keep Synergism deferred, optionally name Capybara Fight/Safe Path in compose Ideal HUD, and re-run AllSmoke if sibling churn adds leaves again. Do **not** treat EditMode 114 as Play.

---

## Assumptions (ledger)

| # | Assumption | Confidence | Verified by |
|---|------------|------------|-------------|
| 1 | R7 impl_03 claimed R7-C1 MET at tip **114/114** via `IdleAllSmoke-impl03-r7.log` | high | `round_07/impl_03_llm.md`; tip Summary + log duration match |
| 2 | Authoritative EditMode count = tip `Logs/IdleAllSmoke-Summary.txt` (not progress prose alone) | high | progress itself says tip is authoritative; tip `pass=114` |
| 3 | HEAD AllSmoke leaf sum now matches tip (R7 soft-stale 103→109 closed by tip rewrite + gacha leaf commit) | high | `[Test]` sum 114; BC Summary `pass=68`; tip XML `passed="114"` |
| 4 | Piece-flag / GenerateCustomIdleSlice still deferred | high | compose Non-negotiable / R7-C7 |
| 5 | Play Mode still 0/19; Synergism still deferred | high | progress + checklist + matrix + compose + `IdleArchetype` enum + no Idle `*Synerg*` prefab |
| 6 | Legacy `SynergySystem.cs` ≠ matrix Synergism #20; R5-C6 compose note remains | high | compose § Matrix gap name-trap line present |

---

## Round 07 acceptance re-score (post–impl_03 + sibling gacha)

| ID | Criterion | R7 claim | Round 08 finding |
|----|-----------|----------|------------------|
| R7-C1 | Re-run AllSmoke; progress AllSmoke **integer** matches fresh tip Summary (EditMode-only; Play stays 0/19) | MET (114/114) | **PASS** — progress **114/114** = tip Summary **114/114** = HEAD leaves **114**; Play still **0/19** |
| R7-C2 | Progress (+ README) still name live AllSmoke fixtures incl. **IdlePrestigeFactionBonusTests** | hold | **PASS** — Evidence / claim / Run-tests / README all name PrestigeFaction |
| R7-C3 | All compose smoke cells still resolve (full 20-ref re-grep) | hold | **PASS** — `refs=20 ok=20 miss=0` |
| R7-C4 | Seed checklist B/C HUD rows still match `WritePlaySmokeChecklist` | hold | **PASS** — LoM / Capybara / Cats / Neko / Fallout identical |
| R7-C5 | Play column / Play Mode claim remains **0/19** | hold | **PASS** — progress + checklist + README + PROJECT M6 + writer + R5–R7-A4 BLOCKED notes |
| R7-C6 | Synergism #20 deferred; no enum/prefab; Synergy≠Synergism note kept | hold | **PASS** — matrix/compose/progress deferred; enum ends at 18; 0 Synerg prefabs; name-trap present |
| R7-C7 | Piece-flag / GenerateCustomIdleSlice | DEFERRED | **Still deferred** |

### R7-C1 evidence (tip vs progress vs HEAD leaves)

| Source | Count / note |
|--------|----------------|
| `Logs/IdleAllSmoke-Summary.txt` (2026-08-10 05:48) | `result=Passed pass=114 fail=0` duration=7.2796769 |
| `Logs/IdleAllSmoke-TestResults.xml` | `total="114" passed="114" failed="0"` |
| `docs/idle-toolkit-progress.md` Evidence + claim tables | **PASS 114/114** / **114/114**; cites tip + `IdleAllSmoke-impl03-r7.log` |
| `Logs/IdleAllSmoke-impl03-r7.log` | duration-matched tip writer (`duration=7.2796769`) |
| `Logs/IdleBatchA-Summary.txt` | `pass=23` |
| `Logs/IdleBatchBC-Summary.txt` (05:43) | `pass=68` |
| Live `[Test]` attr sum in AllSmoke leafs | A23 + BC68 + K16 + Cos6 + Prestige1 = **114** |
| Tip-sync commit `95d79d1` committed-tree leaf sum | was **110** (BC64) while tip already **114** — dirty gacha leaves later committed in `09b9b72` (+4 Capybara) → HEAD **114** matches tip |

Scope note: AllSmoke runner filter is still **A + BC + Kernel + CosmeticsShop + IdlePrestigeFactionBonusTests**. Integer lockstep tip↔progress↔HEAD is clean at **114**.

### R7-C2 evidence (PrestigeFaction named)

| Surface | State |
|---------|-------|
| Progress Evidence row | lists `IdlePrestigeFactionBonusTests` |
| Progress claim parenthetical | `A+BC+Kernel+Cosmetics+PrestigeFaction` |
| Progress Run-tests paragraph | names Prestige |
| README fixture line | `… + PrestigeFaction in AllSmoke` |
| `IdleBatchATestRunner.RunAllIdleSmokeAndExit` | includes `IdlePrestigeFactionBonusTests` |

### R7-C3 evidence (smoke grep)

| Check | Result |
|-------|--------|
| Unique `IdleBatch*SmokeTests.<Method>` refs in compose | **20** |
| Resolve to `public void <Method>(` in Batch A or BC | **20/20** |
| Stale `RealmGrinder_BuildThenAlignFaction_RaisesMultAndLevel` | **Absent** from compose |
| Compose cites `IdlePrestigeFactionBonusTests.RealmGrinder_AlignEvil_…` | Present — correct |

### R7-C4 evidence (seed ↔ writer)

| Row | Seed checklist | `WritePlaySmokeChecklist` |
|-----|----------------|---------------------------|
| LoM | `Rub Lamp → auto-lamp at Stage≥1` | same |
| Capybara | `Take Step unlocks auto-tiles → run forks → Lucky Find pets` | same |
| Cats | `Assign Cat → Unassign` | same |
| Neko | `Place Food → Place Toys → Check In` | same |
| Fallout | `Assign Dweller → Unassign → Claim Production` | same |
| Status | Play **0/19**; ~17/19 verb honesty | same |
| Anti-regression | `Farm Stage Gold` absent from seed Expected column | **Absent** |

### Soft sibling drift (Capybara Ideal HUD — not R7-C4 fail)

| Surface | Capybara verbs |
|---------|----------------|
| `IdleSliceUIController` (post-`09b9b72`) | Take Step, Next Step, **Fight Path**, **Safe Path**, Lucky Find |
| Compose Ideal HUD cell | Take Step, Next Step, Lucky Find (omits Fight/Safe Path) |
| Seed / writer Expected | “run forks” prose — does not name Fight/Safe Path buttons |

Seed↔writer still match each other (R7-C4). Optional R8 polish: name Fight Path / Safe Path in compose Ideal HUD (and optionally seed Expected) so LLM agents discover path-choice DNA without reading UI code.

---

## What still works (do not regress)

1. **Compose entrypoint** — `docs/idle-toolkit-compose.md`: stack warning, piece table, 19-row map (live smoke names), authoring fan-out, Synergism #20 section + SynergySystem name-trap, menu table.
2. **Menu namespace** — Editor `[MenuItem]` only under `IdleToolkit/MVP/…` and `IdleToolkit/RunnerSlices/…`.
3. **HUD fidelity (core)** — compose Ideal HUD named verbs still ⊆ `IdleSliceUIController` `Btn` labels for prior map; Capybara Fight/Safe Path are additive UI not yet in compose Ideal cell.
4. **Honesty chain** — Play Mode **0/19** in progress, checklist, README, PROJECT M6, regenerator; EditMode ≠ Play called out; Batch A Play 01/03/04 BLOCKED annotated (R5-A4 + R6-A4 + R7-A4).
5. **Synergism #20 documented as gap** — matrix §coverage gap + compose §Matrix gap + progress header; implement gate explicit; R5-C6 name-trap holds.
6. **Seed ↔ regenerator** — R5-C3 / R6-C4 / R7-C4 alignment still holds (do not re-diverge LoM/Capybara/Cats/Neko/Fallout prose).
7. **R6-C2 PrestigeFaction composition naming** — progress/README match runner filter.
8. **R7-C1 tip integer** — progress **114/114** matches tip Summary **114/114** and HEAD leaf sum **114**.

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

| Doc | Round 08 note |
|-----|---------------|
| `docs/idle-toolkit-compose.md` | Keep; smoke 20/20; #20 deferred; Synergy≠Synergism present; Capybara Ideal HUD soft-stale vs Fight/Safe Path |
| `docs/idle-play-smoke-checklist.md` | Seed usable; B/C rows match writer; Play 0/19 honest; R5–R7-A4 BLOCKED notes present |
| `docs/idle-toolkit-progress.md` | Play 0/19 + Synergism deferred + PrestigeFaction named + AllSmoke **114/114** = tip = HEAD leaves |
| `docs/idle_mechanics_matrix.md` | Pointer + #20 gap OK; gate = Play Mode Batch A |
| `Assets/ToolkitExamples/Idle/README_IDLE_SLICES.txt` | Menus + contract + Play 0/19 + PrestigeFaction in AllSmoke; EditMode defers to tip Summary |
| `PROJECT.md` M6 | Accurate enum-gated / Play 0/19 wording |

### C. Architecture (unchanged contract)

- Behavior = `switch (IdleArchetype)` fan-out — not piece flags.
- `IdleArchetype` = 0..18 only (no Synergism).
- Dual legacy stack still in tree (`SynergySystem` / Producer path); compose warns off Producer/ArcadeIdle **and** names Synergy≠Synergism.
- Generator = fixed 19 `Slices[]`; no Generate One / custom-piece API.
- AllSmoke = A + BC + Kernel + Cosmetics + **PrestigeFactionBonus**.

---

## Fidelity debt (Round 08)

### P0 — Truth for LLM agents

*(none on AllSmoke integer — R7-C1 closed.)*

### P1 — Soft honesty / quality loop

1. **Play Mode 0/19 + MCP wrong project** — checklist exists; quality loop cannot close. Progress blockers still list `thepcgtoolkit`; R5-A4 / R6-A4 / R7-A4 keep 01/03/04 BLOCKED.
2. **Synergism #20 gap (deferred by design)** — documented; must stay unimplemented until Play Mode Batch A is green. Distinct DNA: achievement-gated generators / completionism→power (not Antimatter layers, not Realm factions). **Not** the legacy `SynergySystem` on `ProducerComponent` (compose name-trap already present).
3. **Archetype monolith** — novel idle = enum fan-out; hybrids without enum unsupported (honestly stated).
4. **Capybara Ideal HUD soft-stale (optional)** — UI has Fight Path / Safe Path after gacha DNA; compose Ideal cell omits them; seed/writer still say “run forks” without button names.

### P2 — Polish

5. Cosmetics / Actions tabs exist in UI but are absent from compose “ideal journey” (`Cosmetics`, `Primary Action` Btn labels not in compose map — expected).
6. No per-slice LLM brief beyond HowTo (start currency, prestige gate).
7. Old `.agents/explorer_m*/handoff.md` may still cite pre-`MVP/` / pre-`RunnerSlices/` paths.
8. Hardcoded generator tunables still undocumented as intentional MVP constants.
9. LoM `Farm Stage Gold` button remains UI identity (sibling/gacha lane); seed Expected column correctly omits it per R5 sync.
10. If sibling churn adds AllSmoke leaves again, re-run tip before changing progress integer (prefer tip Summary over inventing a leaf sum).

---

## Play Mode 0/19 (standing)

| Source | Claim |
|--------|-------|
| `docs/idle-toolkit-progress.md` | Play-mode verified **0/19**; MCP UNVERIFIED on wrong project; R5–R7-A4 Batch A Play BLOCKED |
| `docs/idle-play-smoke-checklist.md` | Play Mode **0/19**; 01/03/04 **BLOCKED** (R5-A4 + R6-A4 + R7-A4 notes) |
| `README_IDLE_SLICES.txt` | Play Mode **0/19** |
| `PROJECT.md` M6 | Play 0/19 |
| `WritePlaySmokeChecklist` | Play Mode 0/19 |
| Latest AllSmoke summary | EditMode **114/114** only — not Play |

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
**Not now:** do not implement in Round 08; Play Mode 0/19 is the hard prerequisite called out in matrix/compose.  
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
9. Current tip/progress/HEAD lockstep is 114/114 — re-run AllSmoke if sibling test commits land after tip timestamp
```

Novel / hybrid composition still requires compose §Authoring loop enum fan-out.

---

## Acceptance criteria for a future implementer (Round 08)

| ID | Criterion | Check |
|----|-----------|-------|
| R8-C1 | Progress AllSmoke **integer** still matches tip `Logs/IdleAllSmoke-Summary.txt` (EditMode-only; Play stays 0/19). If sibling churn grows leaves past tip, re-run AllSmoke then sync — do not invent a leaf sum | diff numbers |
| R8-C2 | Progress (+ README) still name live AllSmoke fixtures: A + BC + Kernel + CosmeticsShop + **IdlePrestigeFactionBonusTests** | grep progress/README vs runner |
| R8-C3 | All compose smoke cells still resolve (full 20-ref re-grep) | script like this audit |
| R8-C4 | Seed checklist B/C HUD rows still match `WritePlaySmokeChecklist` (or seed documents “run menu to refresh” as sole canonical) | diff LoM/Capybara/Cats/Neko/Fallout |
| R8-C5 | Play column / Play Mode claim remains **0/19** until Play evidence | progress + checklist + README |
| R8-C6 | Synergism #20 remains deferred in matrix + compose + progress; no enum/prefab; keep Synergy≠Synergism note | grep + enum read |
| R8-C7 (optional) | Compose Ideal HUD for Capybara names Fight Path / Safe Path (and optionally seed Expected) to match `IdleSliceUIController` | diff compose vs UI |
| R8-C8 (deferred) | Piece-flag / GenerateCustomIdleSlice — out of scope unless product asks | — |

---

## Evidence citations (read paths)

- `docs/idle-toolkit-compose.md` (19-row map; Synergism § + SynergySystem name-trap; smoke 20 refs)
- `docs/idle-play-smoke-checklist.md` (Play 0/19; seed B/C rows; R5–R7-A4 BLOCKED)
- `docs/idle-toolkit-progress.md` (Play 0/19; Synergism deferred; AllSmoke **114/114**; PrestigeFaction named)
- `docs/idle_mechanics_matrix.md` § Matrix coverage gap
- `Assets/ToolkitExamples/Idle/README_IDLE_SLICES.txt`
- `Assets/Scripts/Editor/IdleToolkitSliceGenerator.cs` (`WritePlaySmokeChecklist`)
- `Assets/Scripts/Editor/IdleBatchATestRunner.cs` (`RunAllIdleSmokeAndExit` → A+BC+Kernel+Cosmetics+Prestige)
- `Assets/Scripts/Editor/ToolkitExampleGenerator.cs` (`RunnerSlices/` MenuItems)
- `Assets/Scripts/UI/IdleSliceUIController.cs` (`AddButtonsForArchetype` — Capybara Fight/Safe Path)
- `Assets/Scripts/Editor/Tests/IdleBatchASmokeTests.cs`, `IdleBatchBCSmokeTests.cs`, `IdleKernelCorrectnessTests.cs`, `CosmeticsShopTests.cs`, `IdlePrestigeFactionBonusTests.cs`
- `Assets/Scripts/ECS/Components/IdleSliceComponents.cs` (`IdleArchetype` 0..18)
- `Assets/Scripts/ECS/Systems/Idle/SynergySystem.cs` (legacy — not #20)
- `PROJECT.md` M6
- `Logs/IdleAllSmoke-Summary.txt` (114/114 EditMode — tip at 05:48)
- `Logs/IdleAllSmoke-TestResults.xml` (`passed="114"`)
- `Logs/IdleAllSmoke-impl03-r7.log` (tip-writing log duration-matched to 114)
- `Logs/IdleBatchA-Summary.txt` (`pass=23`)
- `Logs/IdleBatchBC-Summary.txt` (`pass=68`)
- `.agents/idle_swarm/round_07/impl_03_llm.md`, `review_10_llm.md`
- commits: `95d79d1` (tip/progress 114 sync), `09b9b72` (Capybara DNA +4 leaves + Fight/Safe Path UI)

---

## Status

```
TASK: Round 08 examine 10/10 — re-audit LLM compose/docs honesty (Play 0/19 + Synergism #20) post–R7 tip 114/114
ROUTE: Examine/Analyze/Review only
ASSUMPTIONS: 6 listed; tip 114 = progress 114 = HEAD leaves 114 verified; PrestigeFaction naming holds; Synergy≠Synergism verified present
CHANGES: wrote .agents/idle_swarm/round_08/review_10_llm.md only
VERIFICATION: tip Summary+XML 114/114; progress 114/114 MATCH tip; live leaf sum 114; smoke grep 20/20 OK; seed↔writer B/C OK; menus MVP/RunnerSlices OK; Play 0/19 + Synergism #20 deferred confirmed; R7-C1 closed; optional Capybara Fight/Safe Path Ideal-HUD soft drift noted
STATUS: VERIFIED as review artifact (no implementation; never push)
RISKS: sibling churn can re-stale tip integer; regenerator may overwrite seed when menu is run; compose Ideal HUD lag behind gacha DNA until R8-C7 optional sync
```

**Do not implement from this review in the examine phase.** Hand optional R8-C7 (Capybara Ideal HUD) to implement swarm if desired; keep R8-C1/C5/C6 honest; keep Synergism #20 and piece-flag DSL deferred; keep Play Mode 0/19 until Hyper-Casual-Runner Play evidence exists.
