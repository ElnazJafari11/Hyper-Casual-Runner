# Round 07 IMPLEMENT 02 — Idle Kernel D26 IH/AFK chest CatchUp + D24 blocker

**Agent:** implement 2/10 (kernel)  
**Review:** `.agents/idle_swarm/round_07/review_01_kernel.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Date:** 2026-08-10  
**Mode:** D24 stays BLOCKED (one `doctor` max, no Play thrash); land cheap P1 D26 IH/AFK closed-app chest CatchUp  
**Push:** never (local commit only)

---

## ASSUMPTIONS

1. Quality bar = MVP toolkit slice correctness — high — `docs/project-context.md`.
2. D24 remains evidence-blocked (no HCR interactive on MCP) — high — this pass single `doctor`.
3. D26 fix = chest-seconds arm (option 1), not PassiveRate=0 — high — matches Neko D26 cozy pattern + online sim `AfkChestSeconds += dt`.
4. D27/D29/D31 must not be re-opened — high — review scorecard.

---

## Checklist (review_01 → this pass)

| ID | Item | Status | Evidence |
|----|------|--------|----------|
| **D24** | Play Mode relaunch AFK | **BLOCKED** | One `doctor` — no HCR interactive; pin = `thepcgtoolkit@96e3a310`; no Play probe |
| **D26 IH/AFK** | Closed-app chest CatchUp | **MET** | `ApplyChestCatchUp`; STAMP_POLICY row; `IhAfk_OfflineCatchUp_FillsChestNotPrimary` Passed |
| D30 / D32 | Below P1 | **deferred** | Per review |

---

## Acceptance criteria → evidence

| Criterion | Result | Evidence |
|-----------|--------|----------|
| D26: chest-seconds arm **or** PassiveRate=0 + doc; keep D23; EditMode fixture | **MET** | Chest arm: IH/AFK CatchUp fills `AfkChestSeconds`, leaves Primary alone, `HasOfflineClaim` at ≥10s; fixture Passed |
| D24: Play AFK probe without harness `Apply*` | **BLOCKED** | Doctor below — not run |

---

## Doctor (single attempt — D24)

`doctor` with `project_path=D:\Git\Hyper-Casual-Runner`, `probe_editor_state=true` (2026-08-10 ~02:37Z):

| Field | Value |
|-------|-------|
| Target path | `D:\Git\Hyper-Casual-Runner` — exists, Unity project |
| Editors running | `thepcgtoolkit` interactive (pid 27580); `TheCheckout` batch (pid 23860) |
| HCR interactive editor | **Absent** |
| Discovered MCP instances | `thepcgtoolkit@96e3a310` only |
| Registered sessions | **[]** |
| Pin | `thepcgtoolkit@96e3a310` (explicit) |
| Machine | CPU 100%; editor_count 2 |

**Conclusion:** D24 Play AFK remains **UNVERIFIED / BLOCKED**. No Play thrash. No second doctor.

---

## Changes

- `Assets/Scripts/ECS/Systems/Idle/IdleOfflineCatchUp.cs` — route IH/AFK to `ApplyChestCatchUp` (elapsed → `AfkChestSeconds` + claim flag ≥10s; no Primary bank)
- `Assets/Scripts/Editor/Tests/IdleKernelCorrectnessTests.cs` — `IhAfk_OfflineCatchUp_FillsChestNotPrimary`
- `.agents/idle_swarm/round_02/STAMP_POLICY.md` — IH/AFK Kernel B bank row + machine check
- `.agents/idle_swarm/round_07/impl_02_kernel.md` — this receipt

---

## VERIFICATION

```
compile_check (rsp_replay): Assembly-CSharp + Assembly-CSharp-Editor errorCount=0 status=pass
EditMode: Logs/IdleKernel-R7F1-TestResults.xml
  IdleKernelCorrectnessTests total=16 passed=16 failed=0
  IhAfk_OfflineCatchUp_FillsChestNotPrimary => Passed
Log: Logs/IdleKernel-impl02-r7.log
  Test run completed. Exiting with code 0 (Ok).
```

---

## Deviations / deferred

- D24 not executed — MCP/HCR interactive prerequisite unmet after single `doctor`.
- AfkArena online sim still drips Primary while open; closed-app CatchUp is chest-only (review option 1 — no Primary double-bank). Online Primary drip unchanged.
- D30 / D32 left deferred.
- Prefab / unrelated log noise left unstaged.

---

## Unblock prerequisite (D24 next)

1. Open Unity interactively on `D:\Git\Hyper-Casual-Runner` and register MCP (or human play-smoke).
2. Re-run `doctor` until pin/target is HCR interactive.
3. PersistNow → relaunch Melvor/Egg/Miner; assert CatchUp without harness `Apply*`; stop-Play proof.

---

## STATUS

**ROUTE:** Executor (review_01 P1 D26 + D24 blocker doc)  
**D26 IH/AFK:** **VERIFIED** (EditMode 16/16 + compile_check)  
**D24:** **UNVERIFIED / BLOCKED**  
**COMMIT:** `9dc0997` local only — never push
