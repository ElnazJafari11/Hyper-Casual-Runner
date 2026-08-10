# Round 02 — Shared Offline Stamp Policy

**Owners:** Kernel impl (1/10) + Production impl (2/10)  
**Source:** `review_01_kernel.md` D14/D15 + `review_05_production.md` P0/P1  
**Status:** AGREED for Round 02 landings

---

## Authority split

| Lane | Applies to | Writer | Consumes `LastIdleUpdateTime` |
|------|------------|--------|-------------------------------|
| **Kernel B** | Toolkit `IdleSliceState` (Melvor / Egg / Miner / …) | `IdleOfflineCatchUp.ApplyPersistedElapsed` via bootstrap **after** PassiveRate sync | Yes — only if grant &gt; 0 |
| **Kernel A** | `ProducerComponent` wallets only | `OfflineSimulationSystem` | Yes — only if ≥1 producer catch-up applied |

**Forbidden:** `OfflineSimulationSystem` must **not** mutate `IdleSliceState` (no `PendingClaim` / `HasOfflineClaim` / `AfkChestSeconds`).

---

## Stamp rules for `GameProgressData.LastIdleUpdateTime`

1. **Quit / Persist** (`SaveIdleSlice` / `IdleSliceBootstrap.PersistNow`) MAY stamp — that is session T0.
2. **`OfflineSimulationSystem`** stamps **only if** catch-up was applied to ≥1 automated `ProducerComponent`. Empty world / no producers / pending window unused → **leave timestamp unchanged** (so bootstrap CatchUp can still see T0).
3. **`IdleOfflineCatchUp.ApplyPersistedElapsed`** (bootstrap after PassiveRate sync) stamps **only if** `Apply` returns grant **&gt; 0**. Zero-grant (PassiveRate=0 non-Melvor, etc.) must **leave** `LastIdleUpdateTime` unchanged so a later writer can still consume T0 (D23).
4. Never wipe a real AFK window on a no-op Init tick (Play Mode wipe / D14).

**Landed by:** Kernel owns `OfflineSimulationSystem` + claim XOR (D16). Production owns Kernel B bank-style docs/tests + Melvor/Egg/Miner acceptance (no Egg/Miner Claim UI under PrimaryCurrency bank).

---

## Kernel B bank style (production contract)

| Archetype | Catch-up destination | Claim UI |
|-----------|----------------------|----------|
| Melvor | `PendingClaim` + skill XP ticks; **no** `AfkChestSeconds` bump | Already has Claim Offline |
| Egg / Miner (`PassiveRate > 0`) | **Direct** `PrimaryCurrency` | **None** — not required under this bank style |
| Other PassiveRate slices | Same as Egg/Miner (direct Primary) | None unless a later round switches to PendingClaim |

If a future change banks Egg/Miner into `PendingClaim`, Claim UI (or auto-drain) becomes required in the same change.

---

## Claim conservation (kernel D16 — kernel owns `IdleClaimOfflineSystem`)

- If `PendingClaim > 0`: pay pending only; **do not** also pay `AfkChestSeconds` on the same click.
- Else: chest / cats formula only.
- CPS offline catch-up must not fill both PendingClaim and AfkChestSeconds for the same elapsed window.

---

## Machine checks

- EditMode: OS update with **no** slices/producers + past stamp → timestamp **unchanged**.
- EditMode: then apply bootstrap/`IdleOfflineCatchUp` path with Melvor PassiveRate → `PendingClaim > 0`.
- Egg/Miner helper asserts PrimaryCurrency direct grant (no PendingClaim).
- Melvor catch-up leaves `AfkChestSeconds` at 0.
- EditMode: `ApplyPersistedElapsed` with PassiveRate=0 non-Melvor + past stamp → timestamp **unchanged** (D23).
- EditMode: Sync raw PassiveRate then CatchUp with Mult² stale prefs → grant == `ComputePassiveRate * Mult * t` (D25).
