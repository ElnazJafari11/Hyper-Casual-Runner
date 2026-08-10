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
        /// Matches <c>IdleSliceBootstrap.AttachArchetypeExtras</c> station defaults for Cats/Fallout.
        /// Catch-up uses <see cref="IdleSliceState.AssignedWorkers"/> (no station component on the pure-state path).
        /// </summary>
        public const double StationOutputPerWorker = 1.5;
        public const float StationIntervalSeconds = 1f;

        /// <summary>
        /// Apply elapsed wall-clock time into slice state.
        /// Melvor banks into <see cref="IdleSliceState.PendingClaim"/> (+ skill XP); never bumps AfkChestSeconds.
        /// Cats &amp; Soup / Fallout Shelter with AssignedWorkers&gt;0 simulate station ticks (Primary vs Pending).
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

            if (state.Archetype == IdleArchetype.CatsAndSoup ||
                state.Archetype == IdleArchetype.FalloutShelter)
            {
                return ApplyStationCatchUp(ref state, capped);
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
        /// Wall-clock station ticks: rate = AssignedWorkers × OutputPerWorker × GlobalMultiplier / Interval.
        /// Cats → PrimaryCurrency; Fallout → PendingClaim (+ HasOfflineClaim). Zero workers → 0 (D23 stamp preserved).
        /// </summary>
        public static double ApplyStationCatchUp(ref IdleSliceState state, double cappedSeconds)
        {
            int workers = state.AssignedWorkers;
            if (workers <= 0) return 0;

            float interval = StationIntervalSeconds <= 0f ? 1f : StationIntervalSeconds;
            double gained = workers * StationOutputPerWorker * state.GlobalMultiplier *
                            (cappedSeconds / interval);
            if (gained <= 0) return 0;

            if (state.Archetype == IdleArchetype.FalloutShelter)
            {
                state.PendingClaim += gained;
                state.HasOfflineClaim = state.PendingClaim > 0.5;
            }
            else
            {
                state.PrimaryCurrency += gained;
            }

            return gained;
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
            int xpToLevel = XpToLevelFor(level);

            for (int i = 0; i < ticks; i++)
            {
                xp += 5;
                if (xp < xpToLevel) continue;
                xp = 0;
                level += 1;
                xpToLevel = XpToLevelFor(level);
            }

            state.SkillXp = xp;
            state.ProgressionLevel = level;
        }

        /// <summary>
        /// D33: Rewrite <see cref="IdleSkillNode"/> from post-CatchUp slice Level/XP so online
        /// authority cannot clobber CatchUp gains on the next skill tick.
        /// </summary>
        public static void SyncSkillNodeFromSlice(ref IdleSkillNode skill, in IdleSliceState state)
        {
            int level = state.ProgressionLevel > 0 ? state.ProgressionLevel : 1;
            skill.Level = level;
            skill.Xp = state.SkillXp;
            skill.XpToLevel = XpToLevelFor(level);
        }

        /// <summary>Level 1 threshold is 25; after that 20+level*10 (matches IdleSkillNode online).</summary>
        public static int XpToLevelFor(int level)
        {
            int l = level > 0 ? level : 1;
            return l <= 1 ? 25 : (20 + l * 10);
        }
    }
}
