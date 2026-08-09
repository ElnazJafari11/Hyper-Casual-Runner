using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    /// <summary>
    /// Passive CPS / AFK chest / skill ticks / assignment output for all idle archetypes.
    /// Keeps one simulation tick so MVP slices stay coherent.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(IdleProductionSystem))]
    public partial struct IdleSliceSimulationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IdleSliceState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            foreach (var slice in SystemAPI.Query<RefRW<IdleSliceState>>())
            {
                ref var s = ref slice.ValueRW;
                s.Timer += dt;

                switch (s.Archetype)
                {
                    case IdleArchetype.CookieClicker:
                    case IdleArchetype.AdventureCapitalist:
                    case IdleArchetype.AntimatterDimensions:
                    case IdleArchetype.UniversalPaperclips:
                    case IdleArchetype.EggInc:
                    case IdleArchetype.IdleMinerTycoon:
                        TickPassive(ref s, dt);
                        break;

                    case IdleArchetype.ClickerHeroes:
                    case IdleArchetype.TapTitans2:
                    case IdleArchetype.IdleHeroes:
                        TickHeroDps(ref s, dt);
                        break;

                    case IdleArchetype.AfkArena:
                        TickAfkChest(ref s, dt);
                        break;

                    case IdleArchetype.NguIdle:
                        TickEnergy(ref s, dt);
                        break;

                    case IdleArchetype.MelvorIdle:
                        TickSkill(ref s, dt);
                        break;

                    case IdleArchetype.CatsAndSoup:
                    case IdleArchetype.FalloutShelter:
                        TickAssignments(ref s, dt);
                        break;

                    case IdleArchetype.NekoAtsume:
                        TickCheckIn(ref s, dt);
                        break;

                    case IdleArchetype.RealmGrinder:
                        TickFaction(ref s, dt);
                        break;

                    case IdleArchetype.ADarkRoom:
                    case IdleArchetype.CapybaraGo:
                    case IdleArchetype.LegendOfMushroom:
                        // driven by narrative/gacha action events; tiny ambient drip
                        if (s.PassiveRate > 0) TickPassive(ref s, dt);
                        break;
                }

                SyncGold(ref state, s.PrimaryCurrency);
            }

            TickStations(ref state, dt);
            TickSkills(ref state, dt);
            TickCombat(ref state, dt);
        }

        private static void TickPassive(ref IdleSliceState s, float dt)
        {
            if (s.PassiveRate <= 0) return;
            s.PrimaryCurrency += s.PassiveRate * s.GlobalMultiplier * dt;
        }

        private static void TickHeroDps(ref IdleSliceState s, float dt)
        {
            double dps = s.PassiveRate > 0 ? s.PassiveRate : s.ClickPower * 0.25;
            if (dps <= 0) return;

            if (s.EnemyMaxHp <= 0)
            {
                s.EnemyMaxHp = 10 + s.ProgressionLevel * 15;
                s.EnemyHp = s.EnemyMaxHp;
            }

            s.EnemyHp -= (int)System.Math.Max(1, dps * dt);
            if (s.EnemyHp <= 0)
            {
                s.PrimaryCurrency += (5 + s.ProgressionLevel * 3) * s.GlobalMultiplier;
                s.ProgressionLevel += 1;
                s.EnemyMaxHp = 10 + s.ProgressionLevel * 15;
                s.EnemyHp = s.EnemyMaxHp;
            }
        }

        private static void TickAfkChest(ref IdleSliceState s, float dt)
        {
            s.AfkChestSeconds += dt;
            // Auto-combat drip while chest fills
            s.PrimaryCurrency += (1.0 + s.ProgressionLevel * 0.5) * s.GlobalMultiplier * dt;
            if (s.AfkChestSeconds >= 10f)
            {
                s.HasOfflineClaim = true;
            }
        }

        private static void TickEnergy(ref IdleSliceState s, float dt)
        {
            s.EnergyPool = System.Math.Min(100f, s.EnergyPool + dt * 2f);
            if (s.EnergyAllocated > 0)
            {
                float spend = System.Math.Min(s.EnergyAllocated, s.EnergyPool);
                s.EnergyPool -= spend * dt * 0.1f;
                s.PrimaryCurrency += spend * 0.5 * s.GlobalMultiplier * dt;
                s.SkillXp += (int)(spend * dt);
                if (s.SkillXp >= 20 + s.ProgressionLevel * 10)
                {
                    s.SkillXp = 0;
                    s.ProgressionLevel += 1;
                    s.GlobalMultiplier += 0.05f;
                }
            }
        }

        private static void TickSkill(ref IdleSliceState s, float dt)
        {
            // Melvor-style always-on skill grind + periodic level beat
            s.PrimaryCurrency += (0.5 + s.ProgressionLevel * 0.1) * s.GlobalMultiplier * dt;
            if (s.Timer >= 2f)
            {
                s.Timer = 0f;
                s.SkillXp += 5;
                if (s.SkillXp >= 25)
                {
                    s.SkillXp = 0;
                    s.ProgressionLevel += 1;
                    s.ClickPower += 1;
                }
            }
        }

        private static void TickAssignments(ref IdleSliceState s, float dt)
        {
            if (s.AssignedWorkers <= 0) return;
            s.PrimaryCurrency += s.AssignedWorkers * (1.0 + s.ProgressionLevel * 0.2) * s.GlobalMultiplier * dt;
        }

        private static void TickCheckIn(ref IdleSliceState s, float dt)
        {
            // Pure passive: cats arrive slowly; claim via offline/check-in action
            s.Timer += 0; // already added
            if (s.Timer >= 5f)
            {
                s.Timer = 0f;
                s.CheckInCats = System.Math.Min(20, s.CheckInCats + 1);
                s.HasOfflineClaim = true;
            }
        }

        private static void TickFaction(ref IdleSliceState s, float dt)
        {
            double factionBonus = s.FactionId == 1 ? 1.5 : s.FactionId == 2 ? 1.25 : 1.0;
            s.PrimaryCurrency += (s.PassiveRate > 0 ? s.PassiveRate : 1.0) * factionBonus * s.GlobalMultiplier * dt;
        }

        private static void TickStations(ref SystemState state, float dt)
        {
            foreach (var (station, slice) in SystemAPI.Query<RefRW<IdleAssignmentStation>, RefRW<IdleSliceState>>())
            {
                if (station.ValueRO.AssignedCount <= 0) continue;
                float interval = station.ValueRO.Interval <= 0f ? 1f : station.ValueRO.Interval;
                station.ValueRW.Timer += dt;
                if (station.ValueRO.Timer < interval) continue;
                station.ValueRW.Timer -= interval;
                double yield = station.ValueRO.AssignedCount * station.ValueRO.OutputPerWorker *
                               slice.ValueRO.GlobalMultiplier;
                slice.ValueRW.PrimaryCurrency += yield;
            }
        }

        private static void TickSkills(ref SystemState state, float dt)
        {
            foreach (var (skill, slice) in SystemAPI.Query<RefRW<IdleSkillNode>, RefRW<IdleSliceState>>())
            {
                if (!skill.ValueRO.IsActive) continue;
                float interval = skill.ValueRO.TickInterval <= 0f ? 1f : skill.ValueRO.TickInterval;
                skill.ValueRW.Timer += dt;
                if (skill.ValueRO.Timer < interval) continue;
                skill.ValueRW.Timer -= interval;
                skill.ValueRW.Xp += 5;
                slice.ValueRW.PrimaryCurrency += 1.0 * slice.ValueRO.GlobalMultiplier;
                if (skill.ValueRO.Xp >= skill.ValueRO.XpToLevel)
                {
                    skill.ValueRW.Xp = 0;
                    skill.ValueRW.Level += 1;
                    skill.ValueRW.XpToLevel = 20 + skill.ValueRO.Level * 10;
                    slice.ValueRW.ProgressionLevel = skill.ValueRO.Level;
                }
            }
        }

        private static void TickCombat(ref SystemState state, float dt)
        {
            foreach (var (combat, slice) in SystemAPI.Query<RefRW<IdleCombatState>, RefRW<IdleSliceState>>())
            {
                if (combat.ValueRO.EnemyMaxHp <= 0f)
                {
                    combat.ValueRW.EnemyMaxHp = 20f + combat.ValueRO.Zone * 25f;
                    combat.ValueRW.EnemyHp = combat.ValueRO.EnemyMaxHp;
                }

                float dps = (float)combat.ValueRO.HeroDps;
                if (dps > 0f)
                {
                    combat.ValueRW.EnemyHp -= dps * dt;
                }

                if (combat.ValueRO.EnemyHp <= 0f)
                {
                    slice.ValueRW.PrimaryCurrency += combat.ValueRO.GoldPerKill * slice.ValueRO.GlobalMultiplier;
                    combat.ValueRW.Zone += 1;
                    slice.ValueRW.ProgressionLevel = combat.ValueRO.Zone;
                    combat.ValueRW.EnemyMaxHp = 20f + combat.ValueRO.Zone * 25f;
                    combat.ValueRW.EnemyHp = combat.ValueRO.EnemyMaxHp;
                    combat.ValueRW.GoldPerKill = 5 + combat.ValueRO.Zone * 2;
                }

                slice.ValueRW.EnemyHp = (int)combat.ValueRO.EnemyHp;
                slice.ValueRW.EnemyMaxHp = (int)combat.ValueRO.EnemyMaxHp;
            }
        }

        private static void SyncGold(ref SystemState state, double gold)
        {
            foreach (var run in SystemAPI.Query<RefRW<CurrentRunStats>>())
            {
                run.ValueRW.CurrentGold = gold;
            }
        }
    }

}
