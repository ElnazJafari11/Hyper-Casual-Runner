# Round 06 Review 10/10 — LLM Compose / Docs (post–R5)

**Agent:** examine/analyze/review  
**From:** `round_05/review_10_llm.md` + `round_05/impl_05_llm.md` (+ tip now **103**)  
**Scope:** Re-audit progress/compose/checklist honesty + Play Mode 0/19 + Synergism #20 deferral after Round 05 LLM docs sync and sibling AllSmoke growth  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Date:** 2026-08-10  
**Implements:** none (review only). Never push.

---

## Verdict

**Round 05 LLM P0 landed and held through tip growth.** `impl_05_llm` (`f04f99a`) synced progress to tip **91/91**, aligned LoM/Capybara seed verbs to `WritePlaySmokeChecklist`, and added the Synergy≠Synergism compose name-trap. Sibling lanes then moved tip to **103/103** and refreshed progress (tests `2be3171`, prestige fold `8480305`). Live tip `Logs/IdleAllSmoke-Summary.txt` is `pass=103 fail=0`; progress Evidence + claim tables also say **103/103**.

Compose smoke map remains **20/20**. Seed checklist B/C HUD rows still match the regenerator (incl. R5 LoM/Capybara). Compose Ideal HUD named verbs **48/48** ⊆ `IdleSliceUIController` `Btn` labels. Menus stay split (`MVP/` vs `RunnerSlices/`). Honesty chain still says **Play Mode 0/19**. Synergism **#20** remains documented-deferred with enum capped at `FalloutShelter = 18`. R5-C6 name-trap line still present.

**Standing gaps (call out explicitly):**
1. **Play Mode 0/19** — tip AllSmoke EditMode **103/103** does not close quality; progress/checklist still cite MCP on `thepcgtoolkit`; Batch A Play 01/03/04 BLOCKED.
2. **Synergism #20** — matrix/compose/progress keep the deferred candidate; gate remains “Play Mode Batch A green on Hyper-Casual-Runner” before any 20th archetype. Do not confuse with legacy `SynergySystem` (compose now names the trap).
3. **AllSmoke fixture-list prose soft-stale** — count **103/103** matches tip, but progress/README still describe the bundle as A+BC+Kernel+Cosmetics and omit `IdlePrestigeFactionBonusTests` now folded into `RunAllIdleSmokeAndExit` (AlignEvil lives only there; BC no longer hosts it).

**Bottom line:** LLM orientation + R5 smoke/seed/Play/#20/C6 fidelity hold at tip **103**. Remaining work is fixture-list honesty (name Prestige in progress/README), Play Mode evidence on this project, and (deferred) #20 / piece-flag composition. Do **not** treat EditMode 103 as Play.

---

## Assumptions (ledger)

| # | Assumption | Confidence | Verified by |
|---|------------|------------|-------------|
| 1 | R5 impl_05 claimed R5-C1..C6 MET at commit `f04f99a` (91/91 era) | high | `round_05/impl_05_llm.md` |
| 2 | Sibling R5 churn grew tip 91→103 (Cosmetics fold + BC growth + Prestige fixture fold) and progress count followed | high | tip Summary `pass=103`; progress **103/103**; commits `2be3171`, `8480305`; runner lists Prestige |
| 3 | Authoritative EditMode count = tip `Logs/IdleAllSmoke-Summary.txt` (not progress prose, not fixture `[Test]` attr sum alone) | high | Summary `pass=103`; XML `passed="103"`; progress itself says tip is authoritative |
| 4 | Piece-flag / GenerateCustomIdleSlice still deferred | high | compose Non-negotiable / R5-C7 |
| 5 | Play Mode still 0/19; Synergism still deferred | high | progress + checklist + matrix + compose + `IdleArchetype` enum + no Idle `*Synerg*` prefab |
| 6 | Legacy `SynergySystem.cs` ≠ matrix Synergism #20; R5-C6 compose note landed and remains | high | compose § Matrix gap name-trap line present |

---

## Round 05 acceptance re-score (post–sibling churn)

| ID | Criterion | R5 claim | Round 06 finding |
|----|-----------|----------|------------------|
| R5-C1 | Progress AllSmoke claim matches tip Summary pass count (EditMode-only; Play stays 0/19) | MET (91/91 then) | **PASS** — progress **103/103**; tip Summary **103/103**; Play still **0/19** |
| R5-C2 | All compose smoke cells still resolve (full 20-ref re-grep) | MET | **PASS** — `refs=20 ok=20 miss=0` |
| R5-C3 | Seed checklist B/C HUD rows match `WritePlaySmokeChecklist` | MET | **PASS** — LoM / Capybara / Cats / Neko / Fallout identical |
| R5-C4 | Play column / Play Mode claim remains **0/19** | MET | **PASS** — progress + checklist + README + PROJECT M6 + writer |
| R5-C5 | Synergism #20 deferred; no enum/prefab | MET | **PASS** — matrix/compose/progress deferred; enum ends at 18; no Idle Synergism prefab |
| R5-C6 (optional) | One-line compose note: legacy `SynergySystem` ≠ Synergism #20 | MET | **PASS** — compose name-trap still present |
| R5-C7 | Piece-flag / GenerateCustomIdleSlice | DEFERRED | **Still deferred** |

### R5-C1 evidence (tip vs progress)

| Source | Count / note |
|--------|----------------|
| `Logs/IdleAllSmoke-Summary.txt` (2026-08-10 04:57) | `result=Passed pass=103 fail=0` duration=3.4890533 |
| `Logs/IdleAllSmoke-TestResults.xml` | `total="103" passed="103" failed="0"` |
| `docs/idle-toolkit-progress.md` Evidence + claim tables | **PASS 103/103** / **103/103**; cites tip + `IdleAllSmoke-impl04-r5b.log` |
| `Logs/IdleAllSmoke-impl04-r5b.log` | historical green at 103 (Cosmetics-era start line; duration 6.045) — count still matches tip |
| Live `[Test]` attr sum in AllSmoke leafs | A23 + BC60 + K13 + Cos6 + Prestige1 = **103** |

Scope note: AllSmoke runner filter is now **A + BC + Kernel + CosmeticsShop + IdlePrestigeFactionBonusTests**. Progress correctly keeps tip Summary authoritative and Play at 0/19; only the **named fixture list** omits Prestige (soft honesty debt below).

### R5-C2 evidence (smoke grep)

| Check | Result |
|-------|--------|
| Unique `IdleBatch*SmokeTests.<Method>` refs in compose | **20** |
| Resolve to `public void <Method>(` in Batch A or BC | **20/20** |
| Stale `RealmGrinder_BuildThenAlignFaction_RaisesMultAndLevel` | **Absent** from compose |
| Compose also cites `IdlePrestigeFactionBonusTests.RealmGrinder_AlignEvil_…` (extra; not in the 20 Batch* count) | Present — correct after Evil move |

### R5-C3 evidence (seed ↔ writer)

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
3. **HUD fidelity** — compose Ideal HUD named verbs **48/48** present as `Btn(row, "…")` labels (Idle Heroes still no Auto Fight; AFK = Campaign Progress; Realm + Rebirth; Neko + Place Toys; Miner Hire Super-Manager). `Farm Stage Gold` remains UI-only / LoM parenthetical — not a seed Expected primary verb.
4. **Honesty chain** — Play Mode **0/19** in progress, checklist, README, PROJECT M6, regenerator; EditMode ≠ Play called out; Batch A Play 01/03/04 BLOCKED annotated.
5. **Synergism #20 documented as gap** — matrix §coverage gap + compose §Matrix gap + progress header; implement gate explicit; R5-C6 name-trap holds.
6. **Seed ↔ regenerator** — R5-C3 alignment still holds (do not re-diverge LoM/Capybara/Cats/Neko/Fallout prose).
7. **AllSmoke integer honesty** — progress **103/103** matches tip (R4/R5 count-drift failure mode closed for now).

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

| Doc | Round 06 note |
|-----|---------------|
| `docs/idle-toolkit-compose.md` | Keep; smoke 20/20; #20 deferred; R5-C6 Synergy≠Synergism present |
| `docs/idle-play-smoke-checklist.md` | Seed usable; B/C rows match writer; Play 0/19 honest; R5-A4 BLOCKED note present |
| `docs/idle-toolkit-progress.md` | Play 0/19 + Synergism deferred OK; **AllSmoke 103/103 matches tip**; fixture list omits Prestige |
| `docs/idle_mechanics_matrix.md` | Pointer + #20 gap OK |
| `Assets/ToolkitExamples/Idle/README_IDLE_SLICES.txt` | Menus + contract + Play 0/19 aligned; AllSmoke composition line Cosmetics-era (no Prestige name) |
| `PROJECT.md` M6 | Accurate enum-gated / Play 0/19 wording |

### C. Architecture (unchanged contract)

- Behavior = `switch (IdleArchetype)` fan-out — not piece flags.
- `IdleArchetype` = 0..18 only (no Synergism).
- Dual legacy stack still in tree (`SynergySystem` / Producer path); compose warns off Producer/ArcadeIdle **and** names Synergy≠Synergism.
- Generator = fixed 19 `Slices[]`; no Generate One / custom-piece API.
- AllSmoke = A + BC + Kernel + Cosmetics + **PrestigeFactionBonus** (Prestige fold landed since R5 LLM review).

---

## Fidelity debt (Round 06)

### P0 — Truth for LLM agents

1. **None on AllSmoke integer.** Progress **103/103** matches tip Summary + XML. Do **not** invent a new scoreboard number. Prefer tip Summary if sibling churn re-drifts.

### P1 — Soft honesty / quality loop

2. **AllSmoke fixture-list prose incomplete** — `docs/idle-toolkit-progress.md` Evidence row, claim parenthetical, and Run-tests paragraph (plus README “Kernel + Cosmetics” line) still say A+BC+Kernel+Cosmetics only. Live `IdleBatchATestRunner.RunAllIdleSmokeAndExit` also includes `IdlePrestigeFactionBonusTests` (1 leaf; hosts `RealmGrinder_AlignEvil_…`). Count stays 103 either way after Evil left BC; agents quoting the **composition** will miss Prestige. Refresh names; keep Play at 0/19.
3. **Play Mode 0/19 + MCP wrong project** — checklist exists; quality loop cannot close. Progress blockers still list `thepcgtoolkit`; R5-A4 keeps 01/03/04 BLOCKED.
4. **Synergism #20 gap (deferred by design)** — documented; must stay unimplemented until Play Mode Batch A is green. Distinct DNA: achievement-gated generators / completionism→power (not Antimatter layers, not Realm factions). **Not** the legacy `SynergySystem` on `ProducerComponent` (compose name-trap already present).
5. **Archetype monolith** — novel idle = enum fan-out; hybrids without enum unsupported (honestly stated).

### P2 — Polish

6. Cosmetics / Actions tabs exist in UI but are absent from compose “ideal journey”.
7. No per-slice LLM brief beyond HowTo (start currency, prestige gate).
8. Old `.agents/explorer_m*/handoff.md` may still cite pre-`MVP/` / pre-`RunnerSlices/` paths.
9. Hardcoded generator tunables still undocumented as intentional MVP constants.
10. Curiosity (not doc-blocking): tip Summary duration **3.489** differs from cited `impl04-r5b` (**6.045**) — same pass=103; tip was rewritten after Prestige fold. Prefer tip Summary pointer; demote or annotate r5b as Cosmetics-era corroboration if refreshing progress prose.
11. LoM `Farm Stage Gold` button remains UI identity (sibling/gacha lane); seed Expected column correctly omits it per R5 sync.

---

## Play Mode 0/19 (standing)

| Source | Claim |
|--------|-------|
| `docs/idle-toolkit-progress.md` | Play-mode verified **0/19**; MCP UNVERIFIED on wrong project; R5-A4 Batch A Play BLOCKED |
| `docs/idle-play-smoke-checklist.md` | Play Mode **0/19**; 01/03/04 **BLOCKED** |
| `README_IDLE_SLICES.txt` | Play Mode **0/19** |
| `PROJECT.md` M6 | Play 0/19 |
| `WritePlaySmokeChecklist` | Play Mode 0/19 |
| Latest AllSmoke summary | EditMode **103/103** only — not Play |

**Do not** let AllSmoke green, fixture 19/19, Cosmetics/Prestige-in-AllSmoke, or progress scorecard ~17/19 inflate Play claims. Human/MCP Play-Smoke on **Hyper-Casual-Runner** is still required before any Playable marks.

---

## Synergism #20 gap (standing)

| Surface | State |
|---------|-------|
| Matrix | § Matrix coverage gap — Synergism deferred; gate = Play Mode Batch A |
| Compose | § Matrix gap (candidate #20 — deferred) — same gate + R5-C6 SynergySystem name-trap |
| Progress | Header: “19 titles + candidate #20 Synergism deferred” |
| Code | `IdleArchetype` ends at `FalloutShelter = 18` — no enum / prefab / smoke |
| Prefabs | No `*Synerg*` under `Assets/ToolkitExamples/Idle/` |

**Gap filled if/when built:** achievement-gated generators (completionism → power).  
**Not now:** do not implement in Round 06; Play Mode 0/19 is the hard prerequisite called out in matrix/compose.  
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
```

Novel / hybrid composition still requires compose §Authoring loop enum fan-out.

---

## Acceptance criteria for a future implementer (Round 06)

| ID | Criterion | Check |
|----|-----------|-------|
| R6-C1 | Progress AllSmoke **integer** still matches `Logs/IdleAllSmoke-Summary.txt` pass count (EditMode-only; Play stays 0/19) | diff numbers → expect **103/103** at HEAD tip unless tip moved |
| R6-C2 | Progress (+ README if it lists composition) name the live AllSmoke fixtures: A + BC + Kernel + CosmeticsShop + **IdlePrestigeFactionBonusTests** | grep progress/README vs `IdleBatchATestRunner.RunAllIdleSmokeAndExit` |
| R6-C3 | All compose smoke cells still resolve (full 20-ref re-grep) | script like this audit |
| R6-C4 | Seed checklist B/C HUD rows still match `WritePlaySmokeChecklist` (or seed documents “run menu to refresh” as sole canonical) | diff LoM/Capybara/Cats/Neko/Fallout |
| R6-C5 | Play column / Play Mode claim remains **0/19** until Play evidence | progress + checklist + README |
| R6-C6 | Synergism #20 remains deferred in matrix + compose + progress; no enum/prefab; keep Synergy≠Synergism note | grep + enum read |
| R6-C7 (deferred) | Piece-flag / GenerateCustomIdleSlice — out of scope unless product asks | — |

---

## Evidence citations (read paths)

- `docs/idle-toolkit-compose.md` (19-row map; Synergism § + SynergySystem name-trap; smoke 20 refs)
- `docs/idle-play-smoke-checklist.md` (Play 0/19; seed B/C rows; R5-A4 BLOCKED)
- `docs/idle-toolkit-progress.md` (Play 0/19; Synergism deferred; AllSmoke **103/103**; Cosmetics-era fixture list)
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
- `Logs/IdleAllSmoke-Summary.txt` (103/103 EditMode)
- `Logs/IdleAllSmoke-TestResults.xml` (`passed="103"`)
- `Logs/IdleAllSmoke-impl04-r5b.log` (historical 103 Cosmetics-era start line)
- `.agents/idle_swarm/round_05/impl_05_llm.md`, `review_10_llm.md`

---

## Status

```
TASK: Round 06 examine 10/10 — re-audit LLM compose/docs vs tip AllSmoke 103 (Play 0/19 + Synergism #20)
ROUTE: Examine/Analyze/Review only
ASSUMPTIONS: 6 listed; tip 103 vs progress 103 verified; Prestige omitted from progress composition prose; Synergy≠Synergism verified present
CHANGES: wrote .agents/idle_swarm/round_06/review_10_llm.md only
VERIFICATION: tip Summary+XML 103/103; progress 103/103 MATCH; smoke grep 20/20 OK; HUD verb check 48/48 OK; seed↔writer B/C OK; menus MVP/RunnerSlices OK; Play 0/19 + Synergism #20 deferred confirmed; R5-C6 name-trap still present; runner includes Prestige (progress list soft-stale)
STATUS: VERIFIED as review artifact (no implementation; never push)
RISKS: further sibling tests will re-stale the tip count until progress is refreshed on each green AllSmoke; regenerator may overwrite seed when menu is run; agents may under-list Prestige when quoting progress composition
```

**Do not implement from this review in the examine phase.** Hand R6-C2 (fixture-list name refresh) and keep R6-C1/C5/C6 honest to implement swarm; keep Synergism #20 and piece-flag DSL deferred; keep Play Mode 0/19 until Hyper-Casual-Runner Play evidence exists.
