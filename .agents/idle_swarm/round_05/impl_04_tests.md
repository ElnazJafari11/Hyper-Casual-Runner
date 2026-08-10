# Round 05 Implement 04 — AllSmoke Tip Sync (~HEAD) + Receipt Hygiene

**Agent:** implement 4/10  
**Review:** `.agents/idle_swarm/round_05/review_09_tests.md`  
**Repo:** `D:\Git\Hyper-Casual-Runner`  
**Commit:** local only (no push) — `2be3171`

---

## ASSUMPTIONS

1. Review ACs #1–3 are in scope: fresh AllSmoke tip rewrite, progress sync to tip, annotate false gacha tip claim — confidence: **high** — verified by: user query + review_09 recommended next #1–3.  
2. Review-era “expect 94” (A23+BC58+K13) is a snapshot; live HEAD may have grown fixtures / folded Cosmetics — confidence: **high** — verified by: on-disk `[Test]` counts + `RunAllIdleSmokeAndExit` filter listing Cosmetics.  
3. `ApplyPersistedElapsed_ZeroGrant` using `NekoAtsume` is stale after D26 Neko cat accrual — confidence: **high** — verified by: fail `Expected: 0 But was: 12` (= 60s/5s cats) on first r5 run.  
4. HUD stays outside AllSmoke — confidence: **high** — runner filter has no `IdleSliceHudSmokeTests`.

---

## Changes

| Area | What |
|------|------|
| `IdleBatchATestRunner.cs` | Include `CosmeticsShopTests` in `RunAllIdleSmokeAndExit` (was already on working tree; committed so tip 103 is reproducible) |
| `IdleKernelCorrectnessTests.cs` | ZeroGrant fixture archetype `NekoAtsume` → `CookieClicker` (PassiveRate-gated path; D26-safe) |
| `docs/idle-toolkit-progress.md` | AllSmoke cite **→ 103/103**; fixtures list Cosmetics; cites `IdleAllSmoke-impl04-r5b.log` |
| `round_04/impl_06_gacha.md` | Receipt hygiene: remove false tip-91 / r4d corroboration for IH HeroDps lines; point to Batch BC log |
| `Logs/IdleAllSmoke-Summary.txt` / `TestResults.xml` | Rewritten by successful AllSmoke run |

Deferred (explicit in review / out of scope): README soften; AfkArena mid-gate twin; PrefsUsed Egg/Miner/TT2; multi-slice TargetSlice; HUD under AllSmoke; Antimatter/AFK PARTIAL product picks.

---

## Acceptance criteria → status

| # | Criterion | Status |
|---|-----------|--------|
| 1 | Re-run `RunAllIdleSmokeAndExit`; tip pass equals on-disk AllSmoke leafs; includes three IH combat-persist names | **MET** — tip `pass=103`; leaf Passed lines=103; IH HeroDps attach + PersistNow + CombatState round-trip all `=> Passed` in `IdleAllSmoke-impl04-r5b.log` |
| 2 | Sync `docs/idle-toolkit-progress.md` AllSmoke cite to tip (not 89) | **MET** — progress **103/103** cites tip Summary + r5b log |
| 3 | Annotate / reject `impl_06_gacha` tip corroboration for tests absent from tip-writing log | **MET** — R5 correction block on receipt; Batch BC log named as evidence |

---

## HEAD fixture math (at tip write)

| Fixture | `[Test]` | In AllSmoke? |
|---------|----------|--------------|
| Batch A | 23 | Yes |
| Batch B+C | 61 | Yes |
| Kernel | 13 | Yes |
| Cosmetics | 6 | Yes (folded on HEAD before this lane) |
| HUD | 4 | No |
| **AllSmoke total** | **103** | A+BC+Kernel+Cosmetics |

Review’s “~94” = prior A23+BC58+K13 without Cosmetics / without later BC growth.

---

## VERIFICATION

```
ROUTE: Executor (round_05 review_09 ACs 1–3)
ASSUMPTIONS: 4 verified, 0 unverified

Logs/IdleAllSmoke-impl04-r5.log (FAILED — ZeroGrant + Cosmetics in filter):
  Starting EditMode fixtures: A, BC, Kernel, CosmeticsShopTests
  FAIL ApplyPersistedElapsed_ZeroGrant_DoesNotStampLastIdleUpdateTime
    Expected: 0.0d +/- 0.001d  But was: 12.0d
  result=Failed(Child) pass=102 fail=1  (summary rewritten; XML race left stale 91)

Fix: ZeroGrant archetype → CookieClicker (PassiveRate=0 path)

Logs/IdleAllSmoke-impl04-r5b.log (SUCCESS):
  [IdleToolkit] Starting EditMode fixtures: IdleBatchASmokeTests, IdleBatchBCSmokeTests, IdleKernelCorrectnessTests, CosmeticsShopTests
  IdleHeroes_CombatState_PersistsRoundTrip_ZoneNotRebuiltFromStageLevel => Passed
  IdleHeroes_HeroDps_RestoresFromPassiveRateOnAttach => Passed
  IdleHeroes_HeroDps_SurvivePersistNowReload => Passed
  ApplyPersistedElapsed_ZeroGrant_DoesNotStampLastIdleUpdateTime => Passed
  [IdleToolkit] result=Passed pass=103 fail=0 skip=0 inconclusive=0 duration=6.0454057
  Leaf Passed lines: 103

Logs/IdleAllSmoke-Summary.txt:
result=Passed pass=103 fail=0 skip=0 inconclusive=0 duration=6.0454057

Logs/IdleAllSmoke-TestResults.xml:
total="103" passed="103" failed="0" result="Passed"

STATUS: VERIFIED
```

**RISKS:** Concurrent swarm agents may still rewrite fixtures/Summary; Play Mode still 0/19; Antimatter + AFK remain PARTIAL; HUD (4) still outside AllSmoke.

---

## Flash Base

None.
