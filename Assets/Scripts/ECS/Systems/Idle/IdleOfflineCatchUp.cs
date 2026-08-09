using System;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    /// <summary>
    /// Kernel B wall-clock catch-up for toolkit <see cref="IdleSliceState"/> slices.
    /// Cap mirrors Melvor-style soft offline (8h). Kernel A (<see cref="OfflineSimulationSystem"/>)
    /// remains ProducerComponent-only and is not used by Melvor/Egg/Miner prefabs.
    /// </summary>
    public static class IdleOfflineCatchUp
    {
        public const double CapSeconds = 8.0 * 60.0 * 60.0;

        /// <summary>
        /// Apply elapsed wall-clock time into slice state.
        /// Melvor banks currency into <see cref="IdleSliceState.PendingClaim"/> (claim UI).
        /// Passive CPS archetypes add directly to <see cref="IdleSliceState.PrimaryCurrency"/>.
        /// Returns currency granted or banked (0 if nothing applied).
        /// </summary>
        public static double Apply(ref IdleSliceState state, double elapsedSeconds)
        {
            double capped = Math.Min(Math.Max(0.0, elapsedSeconds), CapSeconds);
            if (capped <= 0) return 0;

            if (state.Archetype == IdleArchetype.MelvorIdle)
            {
                // IdleSkillNode is the single online authority: ~1 currency / second * mult.
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
