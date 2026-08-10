# Round 05 IMPLEMENT 10/10 — Idle Kernel D24 Play AFK

**Agent:** implement 10/10  
**Review:** `.agents/idle_swarm/round_05/review_01_kernel.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Date:** 2026-08-10  
**Mode:** P0 D24 only — try `doctor` once; if wrong Unity, document blocker (no thrash)  
**Push:** never (local commit only)

---

## ASSUMPTIONS

1. Quality bar = MVP toolkit slice correctness — high — `docs/project-context.md`.
2. Sole remaining kernel P0 is D24 Play Mode relaunch AFK — high — `review_01_kernel.md` scorecard.
3. Prior code P0s (D14/D15/D16/D23/D25/D33 + Persist flush) stay MET; this slot does not re-open them — high — review + R4 receipts.
4. User instruction: one `doctor` attempt; if still not Hyper-Casual-Runner on the bridge, stop and receipt the blocker — high — implement prompt.

---

## P0 checklist (review_01 → this pass)

| ID | Item | Status | Evidence |
|----|------|--------|----------|
| **D24** | Play Mode relaunch AFK probe | **BLOCKED** | Unity MCP `doctor` once — no HCR editor; no Play probe run |
| D14–D16, D23, D25, D33 | Prior stamp/authority/sync P0s | **unchanged (MET on HEAD)** | Not re-audited this pass; review already closed them |
| D26–D32 / SkillXp persist | Non-P0 polish | **deferred** | Out of scope per review |

---

## Acceptance criteria → evidence

| Criterion (from review) | Result | Evidence |
|-------------------------|--------|----------|
| Stamp `LastIdleUpdateTime` = UtcNow − 3600 (or PersistNow backdate) | **NOT RUN** | No HCR Play editor |
| Enter Play with Melvor (+ optional Egg/Miner) `LoadPersistedProgress=true` | **NOT RUN** | Blocker below |
| Do **not** call harness `IdleOfflineCatchUp.Apply*` | **NOT RUN** | — |
| Assert Melvor `PendingClaim > 0`; Egg/Miner Primary delta > 0; Melvor `IdleSkillNode` matches slice post-Start | **NOT RUN** | — |
| Stop Play; prove stopped | **NOT RUN** | — |

---

## Doctor (single attempt — raw)

`doctor` with `project_path=D:\Git\Hyper-Casual-Runner`, `probe_editor_state=true` (2026-08-10 ~01:19Z):

| Field | Value |
|-------|-------|
| Target path | `D:\Git\Hyper-Casual-Runner` — exists, is Unity project |
| Editors running | `D:\Git\pcg-toolkit\thepcgtoolkit` (pid 27580); `D:\Git\checkout\TheCheckout` batch (pid 23860) |
| HCR editor process | **Absent** |
| Discovered MCP instances | `thepcgtoolkit@96e3a310` only |
| Registered sessions | **[]** (none) |
| Pin | `thepcgtoolkit@96e3a310` (explicit) |
| Diagnosis | Editors discovered via status files but **none REGISTERED**; bridge not connected to HCR |
| Remediation suggested | Restart MCP bridge / reopen editor, then `ready` — **not pursued** (one-shot policy) |

**Conclusion:** MCP is still on **thepcgtoolkit**, not Hyper-Casual-Runner. D24 Play AFK remains **UNVERIFIED / BLOCKED**. No further thrash (no Play, no pin flip to wrong project, no EditMode substitute claimed as D24).

---

## Changes

- `.agents/idle_swarm/round_05/impl_10_kernel.md` — this receipt only  
- **No code changes** (probe blocked; no workaround)

---

## Deviations

- Did not land D24 Play / full-`Start` evidence — editor/MCP prerequisite unmet after single `doctor`.
- Did not expand into D26–D32 or other non-P0 kernel polish (review forbid until D24 has evidence or explicit scope change).

---

## Unblock prerequisite (next implementer)

1. Open Unity on `D:\Git\Hyper-Casual-Runner` and register it with MCP (or human Editor play-smoke per `docs/idle-play-smoke-checklist.md`).
2. Re-run `doctor` until target/pin is HCR.
3. Execute D24 acceptance steps; paste raw assert + stop-Play proof into a follow-up receipt.

---

## STATUS

**ROUTE:** Executor (plan = review_01 remaining P0 D24)  
**VERIFICATION:** `doctor` once — wrong Unity / no HCR instance  
**D24:** **UNVERIFIED / BLOCKED** (evidence gap, not a new code wipe)  
**COMMIT:** `6a36c7e1457c92e03d93d54887b0431878bcf71d` local only — never push
