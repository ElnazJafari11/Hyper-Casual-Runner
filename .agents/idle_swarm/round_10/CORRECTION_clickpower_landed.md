# Late correction — Melvor CatchUp ClickPower (post R9/R10)

**Date:** 2026-08-10  
**Source:** late R7 seat `162da1d7` / commit `9bd9e99` (+ receipt note `fa8f124`)  
**Push:** never

## What changed
R9/R10 production receipts said ClickPower CatchUp was **deferred** (stash `r8-prod-clickpower-wip` / uncommitted WT). After those receipts, `9bd9e99` landed on HEAD:

- `IdleOfflineCatchUp.ApplyMelvorSkillTicks`: `ClickPower += levelsGained`
- BC + Kernel fixtures assert the delta
- Verified: Batch BC `pass=69` (`Logs/IdleProduction-impl08-r7b.log` / Summary `duration=9.481185`); Kernel 16/16

## Tip honesty
AllSmoke tip remains **115/115** (`duration=10.9292972` / `IdleAllSmoke-impl01-r7b.log`). Leaf math still `23+69+16+6+1` — **no new [Test] names**. ClickPower is assertion growth inside existing leaves, proven by BC re-run above (not by re-tipping AllSmoke).

## Supersedes
- `round_09/impl_05_production.md` claim that HEAD has no ClickPower bump
- `round_09/review_05_production.md` / `round_10/*_production.md` "defer ClickPower" as open work

Play Mode still **UNVERIFIED**. Stash `r8-prod-clickpower-wip` may be obsolete vs HEAD — leave stash; do not re-apply.
