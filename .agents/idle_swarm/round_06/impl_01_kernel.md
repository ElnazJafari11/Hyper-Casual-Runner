# Round 06 IMPLEMENT 01 — Idle Kernel P1s (D27 / D31 / D29) + D24 blocker

**Agent:** implement 1/10 (kernel seat finish)  
**Review:** `.agents/idle_swarm/round_06/review_01_kernel.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Date:** 2026-08-10  
**Commit:** `dc1a472fd9ee92d01f6e12c953d93bf1ea69cea8` (local only, no push)  
**Mode:** Finish cheapest P1s present in WT; D24 document BLOCKED only (one `doctor`, no Play thrash)  
**Push:** never (local commit only)  
**Gacha:** out of scope — peer `impl_04_gacha` already landed (`8319c9b` / `ec0cd12`)

---

## ASSUMPTIONS

1. Quality bar = MVP toolkit slice correctness — high — `docs/project-context.md`.
2. D24 remains evidence-blocked (no HCR interactive on MCP) — high — this pass `doctor` once.
3. Gacha seat must not be touched — high — user UPDATE.
4. ActionSystems already call `DestroyIfEphemeral` (landed with gacha `8319c9b`); this seat must land the helper + Buy/Click call sites + fixtures or HEAD stays compile-broken for those call sites — high — `git show HEAD:IdleEventTarget` lacked the method until this WT.

---

## Checklist (review_01 → this pass)

| ID | Item | Status | Evidence |
|----|------|--------|----------|
| **D24** | Play Mode relaunch AFK | **BLOCKED** | One `doctor` — no HCR interactive; pin = `thepcgtoolkit@96e3a310`; no Play probe |
| **D27** | Two-step Claim Offline policy | **MET** | STAMP_POLICY + `Claim_SecondClick_AfterPending_PaysRetainedChest` Passed |
| **D31** | Event DestroyEntity guard | **MET** | `IdleEventTarget.DestroyIfEphemeral`; Buy/Click/ActionSystems call sites; `Claim_EventOnSlice_DoesNotDestroySlice` Passed |
| **D29** | Hybrid A+B stamp doc | **MET** | STAMP_POLICY hybrid note (doc only) |
| D26 IH/AFK chest CatchUp | P1 remainder | **deferred** | Not in WT; not cheap this seat |
| D30 / D32 | Below P1 | **deferred** | Per review |

---

## Acceptance criteria → evidence

| Criterion | Result | Evidence |
|-----------|--------|----------|
| D27: document two-step **or** clear chest / pay both; EditMode for chosen policy | **MET** | Policy = retain chest on pending pay; fixtures `Claim_PendingClaim_DoesNotAlsoPayAfkChest` + `Claim_SecondClick_AfterPending_PaysRetainedChest` |
| D31: destroy guard when event == slice (or IdleSliceState) | **MET** | Helper + call sites; `Claim_EventOnSlice_DoesNotDestroySlice` |
| D29: document hybrid Kernel A+B stamp coexistence | **MET** | `STAMP_POLICY.md` D29 section |
| D24: Play AFK probe without harness `Apply*` | **BLOCKED** | Doctor below — not run |

---

## Doctor (single attempt — D24)

`doctor` with `project_path=D:\Git\Hyper-Casual-Runner`, `probe_editor_state=true` (2026-08-10 ~02:27Z):

| Field | Value |
|-------|-------|
| Target path | `D:\Git\Hyper-Casual-Runner` — exists, Unity project |
| Editors running | `thepcgtoolkit` interactive (pid 27580); `TheCheckout` batch (pid 23860); HCR **batchmode** only (pid 61568 EditMode tests) |
| HCR interactive editor | **Absent** |
| Discovered MCP instances | `thepcgtoolkit@96e3a310` only |
| Registered sessions | **[]** |
| Pin | `thepcgtoolkit@96e3a310` (explicit) |

**Conclusion:** D24 Play AFK remains **UNVERIFIED / BLOCKED**. No Play thrash.

---

## Changes

- `Assets/Scripts/ECS/Systems/Idle/IdleEventTarget.cs` — `DestroyIfEphemeral` (D31)
- `Assets/Scripts/ECS/Systems/Idle/IdleBuyGeneratorSystem.cs` — use destroy guard
- `Assets/Scripts/ECS/Systems/Idle/IdleClickProduceSystem.cs` — use destroy guard
- `Assets/Scripts/Editor/Tests/IdleKernelCorrectnessTests.cs` — D27 second-click + D31 event-on-slice fixtures
- `.agents/idle_swarm/round_02/STAMP_POLICY.md` — D27 two-step + D29 hybrid stamp note
- `.agents/idle_swarm/round_06/impl_01_kernel.md` — this receipt

**Note:** `IdleSliceActionSystems` DestroyIfEphemeral call sites already on HEAD via gacha `8319c9b` (file coupling). Prestige keeps its own `!HasComponent<IdleSliceState>` skip.

---

## VERIFICATION

```
compile_check (rsp_replay): Assembly-CSharp + Assembly-CSharp-Editor errorCount=0 status=pass
EditMode: Logs/IdleKernel-R6F1-TestResults.xml
  IdleKernelCorrectnessTests total=15 passed=15 failed=0
  Claim_SecondClick_AfterPending_PaysRetainedChest => Passed
  Claim_EventOnSlice_DoesNotDestroySlice => Passed
  Claim_PendingClaim_DoesNotAlsoPayAfkChest => Passed
Log: Logs/IdleKernel-impl01-r6e.log
```

---

## Deviations / deferred

- D24 not executed — MCP/HCR interactive prerequisite unmet after single `doctor`.
- D26 IH/AFK closed-app chest CatchUp not touched.
- Prefab / unrelated log noise left unstaged.

---

## Unblock prerequisite (D24 next)

1. Open Unity interactively on `D:\Git\Hyper-Casual-Runner` and register MCP (or human play-smoke).
2. Re-run `doctor` until pin/target is HCR interactive.
3. PersistNow → relaunch Melvor/Egg/Miner; assert CatchUp without harness `Apply*`; stop-Play proof.

---

## STATUS

**ROUTE:** Executor (review_01 P1s + D24 blocker doc)  
**D27 / D29 / D31:** **VERIFIED** (EditMode 15/15 + compile_check)  
**D24:** **UNVERIFIED / BLOCKED**  
**COMMIT:** `dc1a472fd9ee92d01f6e12c953d93bf1ea69cea8` local only — never push
