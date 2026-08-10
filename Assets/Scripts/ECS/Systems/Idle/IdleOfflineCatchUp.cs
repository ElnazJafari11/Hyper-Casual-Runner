using System;
using HyperCasualRunner;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    /// <summary>
    /// Sole Kernel B wall-clock catch-up for toolkit <see cref="IdleSliceState"/> slices
    /// (invoked from <see cref="IdleSliceBootstrap"/> on load). Cap mirrors Melvor soft offline (8h).
    /// Kernel A (<see cref="OfflineSimulationSystem"/>) is ProducerComponent-wallet only and must not
    /// mutate slices or wipe <c>LastIdleUpdateTime</c> on empty Init ticks — see round_02/STAMP_POLICY.md.
    /// </summary>
    public static class IdleOfflineCatchUp
    {
        public const double CapSeconds = 8.0 * 60.0 * 60.0;

        /// <summary>
        /// Apply elapsed wall-clock time into slice state.
        /// Melvor banks into <see cref="IdleSliceState.PendingClaim"/> (+ skill XP); never bumps AfkChestSeconds.
        /// Egg/Miner and other PassiveRate&gt;0 archetypes add directly to PrimaryCurrency (no Claim UI required).
        /// Returns currency granted or banked (0 if nothing applied).
        /// </summary>
        public static double Apply(ref IdleSliceState state, double elapsedSeconds)
        {
            double capped = Math.Min(Math.Max(0.0, elapsedSeconds), CapSeconds);
            if (capped <= 0) return 0;

            if (state.Archetype == IdleArchetype.MelvorIdle)
            {
                // IdleSkillNode is the single online authority: ~1 currency / second * mult.
                // Caller should EnsureMelvorPassiveDefault before Apply; keep fallback for direct tests.
                double rate = state.PassiveRate > 0 ? state.PassiveRate : 1.0;
                double gained = rate * state.GlobalMultiplier * capped;
                state.PendingClaim += gained;
                ApplyMelvorSkillTicks(ref state, (int)capped);
                if (gained > 0) state.HasOfflineClaim = true;
                return gained;
            }

            if (state.PassiveRate > 0)
            {
                double gained = state.PassiveRate * state.GlobalMultiplier * capped;
                state.PrimaryCurrency += gained;
                return gained;
            }

            return 0;
        }

        /// <summary>
        /// Melvor skill tick default rate when prefs saved PassiveRate=0 (D25).
        /// Call before <see cref="Apply"/> / <see cref="ApplyPersistedElapsed"/>.
        /// </summary>
        public static void EnsureMelvorPassiveDefault(ref IdleSliceState state)
        {
            if (state.Archetype == IdleArchetype.MelvorIdle && state.PassiveRate <= 0)
                state.PassiveRate = 1.0;
        }

        /// <summary>
        /// Bootstrap Kernel B load path: read <c>LastIdleUpdateTime</c>, apply catch-up,
        /// stamp only if grant &gt; 0 (D23 / STAMP_POLICY #3 — zero-grant must not wipe AFK window).
        /// Returns currency granted or banked.
        /// </summary>
        public static double ApplyPersistedElapsed(ref IdleSliceState state)
        {
            string lastTimeStr = GameProgressData.LastIdleUpdateTime;
            if (string.IsNullOrEmpty(lastTimeStr)) return 0;
            if (!DateTime.TryParse(
                    lastTimeStr,
                    null,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out DateTime lastTime))
                return 0;

            double elapsed = (DateTime.UtcNow - lastTime).TotalSeconds;
            if (elapsed <= 0) return 0;

            EnsureMelvorPassiveDefault(ref state);
            double gained = Apply(ref state, elapsed);
            if (gained > 0)
                GameProgressData.LastIdleUpdateTime = DateTime.UtcNow.ToString("O");
            return gained;
        }

        /// <summary>Matches IdleSkillNode tick: +5 XP / second, level curve 25 then 20+level*10.</summary>
        public static void ApplyMelvorSkillTicks(ref IdleSliceState state, int ticks)
        {
            if (ticks <= 0) return;

            int level = state.ProgressionLevel > 0 ? state.ProgressionLevel : 1;
            int xp = state.SkillXp;
            int xpToLevel = level <= 1 ? 25 : (20 + level * 10);

            for (int i = 0; i < ticks; i++)
            {
                xp += 5;
                if (xp < xpToLevel) continue;
                xp = 0;
                level += 1;
                xpToLevel = 20 + level * 10;
            }

            state.SkillXp = xp;
            state.ProgressionLevel = level;
        }
    }
}
