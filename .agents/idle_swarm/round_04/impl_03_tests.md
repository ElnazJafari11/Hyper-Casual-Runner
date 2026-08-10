# Round 04 Implement 03 — Fold Kernel into AllSmoke + tip count sync

**Agent:** implement 3/10  
**Review:** `.agents/idle_swarm/round_04/review_09_tests.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Commit:** `2c5dc4f` local only (no push)

---

## ASSUMPTIONS

1. Review ACs #1–2 are in scope: fold Kernel into `RunAllIdleSmokeAndExit` + sync progress AllSmoke cite to tip Summary — confidence: **high** — verified by: user query + review_09 recommended next #1–2.  
2. First AllSmoke attempt failed compile on stale `MelvorXpToLevel` (peer race); HEAD now uses `XpToLevelFor` — confidence: **high** — verified by: `IdleAllSmoke-impl03-r4.log` CS0117 + current BC line 1762 / `IdleOfflineCatchUp.XpToLevelFor`.  
3. Tip pass count may exceed review-era 72+12 because peers grew A/BC/Kernel fixtures — confidence: **high** — verified by: tip Summary `pass=89` after Kernel fold.  
4. HUD/Cosmetics stay outside AllSmoke (optional in review) — confidence: **high** — verified by: review “(+ optionally HUD)”; user asked Kernel fold only.

---

## Changes

| Area | What |
|------|------|
| `IdleBatchATestRunner.cs` | `RunAllIdleSmokeAndExit` filter adds `HyperCasualRunner.Tests.IdleKernelCorrectnessTests` |
| `docs/idle-toolkit-progress.md` | AllSmoke evidence **72/72 → 89/89**; lists Kernel in fixture set; cites `IdleAllSmoke-impl03-r4b.log` |
| `Logs/IdleAllSmoke-Summary.txt` / `TestResults.xml` | Rewritten by successful AllSmoke run |

Deferred (explicit in review / out of scope): HUD fold; multi-slice TargetSlice; AfkArena mid-gate twin; PrefsUsed hygiene; README soften (progress was the required doc sync).

---

## Acceptance criteria → status

| # | Criterion | Status |
|---|-----------|--------|
| 1 | Fold Kernel into `RunAllIdleSmokeAndExit`; summary pass count includes Kernel | **MET** — start log lists A+BC+Kernel; 13 Kernel leaf names Passed in r4b log |
| 2 | Progress AllSmoke cite matches tip Summary (not premature 84/84) | **MET** — tip `pass=89 fail=0` → progress **89/89** |

---

## VERIFICATION

```
ROUTE: Executor (round_04 review_09 ACs 1–2; user finish-after-premature-claim)
ASSUMPTIONS: 4 verified, 0 unverified

Logs/IdleAllSmoke-impl03-r4.log (FAILED — compile race):
  IdleBatchBCSmokeTests.cs(1762): CS0117 MelvorXpToLevel (stale; fixed on HEAD as XpToLevelFor)
  Scripts have compiler errors. — summary NOT rewritten

Logs/IdleAllSmoke-impl03-r4b.log (SUCCESS):
  [IdleToolkit] Starting EditMode fixtures: IdleBatchASmokeTests, IdleBatchBCSmokeTests, IdleKernelCorrectnessTests
  … Kernel fixtures Passed (Prestige_ZeroCurrency… Melvor_CatchUp_SyncsIdleSkillNode_FromSlice)
  [IdleToolkit] result=Passed pass=89 fail=0 skip=0 inconclusive=0 duration=5.1789819

Logs/IdleAllSmoke-Summary.txt:
result=Passed pass=89 fail=0 skip=0 inconclusive=0 duration=5.1789819

Logs/IdleAllSmoke-TestResults.xml:
total="89" passed="89" failed="0" result="Passed"

STATUS: VERIFIED
```

**RISKS:** Concurrent swarm agents may still rewrite BC/Kernel fixtures or Summary; Play Mode still 0/19; Antimatter + AFK remain PARTIAL.

---

## Flash Base

None.
