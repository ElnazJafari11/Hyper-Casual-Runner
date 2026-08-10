using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    /// <summary>
    /// Passive CPS / AFK chest / skill ticks / assignment output for all idle archetypes.
    /// GlobalMultiplier is applied here once — PassiveRate must not already bake it in.
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

            foreach (var (slice, entity) in SystemAPI.Query<RefRW<IdleSliceState>>().WithEntityAccess())
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
                        if (s.PassiveRate > 0)
                            s.PrimaryCurrency += s.PassiveRate * s.GlobalMultiplier * dt;
                        break;

                    case IdleArchetype.ClickerHeroes:
                    case IdleArchetype.TapTitans2:
                        // Single HP authority: IdleCombatState loop owns DPS when present
                        if (!SystemAPI.HasComponent<IdleCombatState>(entity))
                        {
                            TickHeroDps(ref s, dt);
                        }
                        else
                        {
                            var combat = SystemAPI.GetComponentRW<IdleCombatState>(entity);
                            if (s.PassiveRate > combat.ValueRO.HeroDps)
                                combat.ValueRW.HeroDps = s.PassiveRate;
                        }
                        break;

                    case IdleArchetype.IdleHeroes:
                        // Matrix: 100% Auto (AFK Chest) — accrue while auto-combat runs
                        s.AfkChestSeconds += dt;
                        if (s.AfkChestSeconds >= 10f) s.HasOfflineClaim = true;
                        if (!SystemAPI.HasComponent<IdleCombatState>(entity))
                        {
                            TickHeroDps(ref s, dt);
                        }
                        else
                        {
                            var combat = SystemAPI.GetComponentRW<IdleCombatState>(entity);
                            if (s.PassiveRate > combat.ValueRO.HeroDps)
                                combat.ValueRW.HeroDps = s.PassiveRate;
                        }
                        break;

                    case IdleArchetype.AfkArena:
                        s.AfkChestSeconds += dt;
                        s.PrimaryCurrency += (1.0 + s.ProgressionLevel * 0.5) * s.GlobalMultiplier * dt;
                        // Campaign level rises from AFK time milestones (chest-only MVP).
                        int campaignFromChest = (int)(s.AfkChestSeconds / 10f);
                        if (campaignFromChest > s.ProgressionLevel)
                            s.ProgressionLevel = campaignFromChest;
                        if (s.AfkChestSeconds >= 10f) s.HasOfflineClaim = true;
                        break;

                    case IdleArchetype.NguIdle:
                        TickEnergy(ref s, dt);
                        break;

                    case IdleArchetype.MelvorIdle:
                        // Single grind authority: IdleSkillNode loop (currency + XP/level).
                        // No continuous drip here — that double-counted online vs skill ticks.
                        break;

                    case IdleArchetype.CatsAndSoup:
                        // Pay exclusively via IdleAssignmentStation (no continuous AssignedWorkers path)
                        break;

                    case IdleArchetype.FalloutShelter:
                        // Pay exclusively via IdleAssignmentStation → PendingClaim
                        break;

                    case IdleArchetype.NekoAtsume:
                        if (s.Timer >= 5f)
                        {
                            s.Timer = 0f;
                            s.CheckInCats = System.Math.Min(20, s.CheckInCats + 1);
                            s.HasOfflineClaim = true;
                        }
                        break;

                    case IdleArchetype.RealmGrinder:
                    {
                        double factionBonus = s.FactionId == 1 ? 1.5 : s.FactionId == 2 ? 1.25 : 1.0;
                        s.PrimaryCurrency += (s.PassiveRate > 0 ? s.PassiveRate : 1.0) * factionBonus *
                                             s.GlobalMultiplier * dt;
                        break;
                    }

                    case IdleArchetype.ADarkRoom:
                    case IdleArchetype.CapybaraGo:
                    case IdleArchetype.LegendOfMushroom:
                        if (s.PassiveRate > 0)
                            s.PrimaryCurrency += s.PassiveRate * s.GlobalMultiplier * dt;
                        break;
                }

                if (SystemAPI.HasComponent<CurrentRunStats>(entity))
                {
                    var run = SystemAPI.GetComponentRW<CurrentRunStats>(entity);
                    run.ValueRW.CurrentGold = s.PrimaryCurrency;
                }
            }

            // LoM auto-lamp: unlock after Stage >= 1 (3 manual rubs), then pull every 2s if affordable.
            foreach (var (gacha, slice, entity) in SystemAPI
                         .Query<RefRW<IdleGachaState>, RefRW<IdleSliceState>>()
                         .WithEntityAccess())
            {
                if (slice.ValueRO.Archetype != IdleArchetype.LegendOfMushroom) continue;
                if (gacha.ValueRO.Stage < 1) continue;

                gacha.ValueRW.AutoTimer += dt;
                if (gacha.ValueRO.AutoTimer < 2f) continue;
                gacha.ValueRW.AutoTimer -= 2f;

                if (!IdleGachaPullSystem.TryApplyPull(ref gacha.ValueRW, ref slice.ValueRW, out _))
                    continue;

                if (SystemAPI.HasComponent<CurrentRunStats>(entity))
                {
                    var run = SystemAPI.GetComponentRW<CurrentRunStats>(entity);
                    run.ValueRW.CurrentGold = slice.ValueRO.PrimaryCurrency;
                }
            }

            // Capybara auto-tiles: when ExploreUnlocked, advance RoomOrStep every 1s (milestone mult included).
            foreach (var (narr, slice, entity) in SystemAPI
                         .Query<RefRW<IdleNarrativeState>, RefRW<IdleSliceState>>()
                         .WithEntityAccess())
            {
                if (slice.ValueRO.Archetype != IdleArchetype.CapybaraGo) continue;
                if (narr.ValueRO.ExploreUnlocked == 0) continue;

                narr.ValueRW.AutoTimer += dt;
                if (narr.ValueRO.AutoTimer < 1f) continue;
                narr.ValueRW.AutoTimer -= 1f;

                if (!IdleNarrativeActionSystem.TryAdvanceStep(ref narr.ValueRW, ref slice.ValueRW))
                    continue;

                if (SystemAPI.HasComponent<CurrentRunStats>(entity))
                {
                    var run = SystemAPI.GetComponentRW<CurrentRunStats>(entity);
                    run.ValueRW.CurrentGold = slice.ValueRO.PrimaryCurrency;
                }
            }

            foreach (var (station, slice, entity) in SystemAPI
                         .Query<RefRW<IdleAssignmentStation>, RefRW<IdleSliceState>>()
                         .WithEntityAccess())
            {
                if (station.ValueRO.AssignedCount <= 0) continue;
                float interval = station.ValueRO.Interval <= 0f ? 1f : station.ValueRO.Interval;
                station.ValueRW.Timer += dt;
                if (station.ValueRO.Timer < interval) continue;
                station.ValueRW.Timer -= interval;

                double payout = station.ValueRO.AssignedCount *
                                station.ValueRO.OutputPerWorker *
                                slice.ValueRO.GlobalMultiplier;

                if (slice.ValueRO.Archetype == IdleArchetype.FalloutShelter)
                {
                    slice.ValueRW.PendingClaim += payout;
                    slice.ValueRW.HasOfflineClaim = slice.ValueRO.PendingClaim > 0.5;
                }
                else
                {
                    slice.ValueRW.PrimaryCurrency += payout;
                }

                if (SystemAPI.HasComponent<CurrentRunStats>(entity))
                {
                    var run = SystemAPI.GetComponentRW<CurrentRunStats>(entity);
                    run.ValueRW.CurrentGold = slice.ValueRO.PrimaryCurrency;
                }
            }

            foreach (var (skill, slice, entity) in SystemAPI
                         .Query<RefRW<IdleSkillNode>, RefRW<IdleSliceState>>()
                         .WithEntityAccess())
            {
                if (!skill.ValueRO.IsActive) continue;
                float interval = skill.ValueRO.TickInterval <= 0f ? 1f : skill.ValueRO.TickInterval;
                skill.ValueRW.Timer += dt;
                if (skill.ValueRO.Timer < interval) continue;
                skill.ValueRW.Timer -= interval;
                skill.ValueRW.Xp += 5;
                slice.ValueRW.PrimaryCurrency += 1.0 * slice.ValueRO.GlobalMultiplier;
                // Align PassiveRate with skill tick for save / IdleOfflineCatchUp (1 currency / interval).
                if (interval > 0f)
                    slice.ValueRW.PassiveRate = 1.0 / interval;
                if (skill.ValueRO.Xp >= skill.ValueRO.XpToLevel)
                {
                    skill.ValueRW.Xp = 0;
                    skill.ValueRW.Level += 1;
                    skill.ValueRW.XpToLevel = 20 + skill.ValueRO.Level * 10;
                    slice.ValueRW.ProgressionLevel = skill.ValueRO.Level;
                    slice.ValueRW.ClickPower += 1;
                }
                // R5: keep slice SkillXp in lockstep every tick (PersistNow reads node, but prefs
                // callers / CatchUp still key off slice.SkillXp).
                slice.ValueRW.SkillXp = skill.ValueRO.Xp;

                if (SystemAPI.HasComponent<CurrentRunStats>(entity))
                {
                    var run = SystemAPI.GetComponentRW<CurrentRunStats>(entity);
                    run.ValueRW.CurrentGold = slice.ValueRO.PrimaryCurrency;
                }
            }

            foreach (var (combat, slice, entity) in SystemAPI
                         .Query<RefRW<IdleCombatState>, RefRW<IdleSliceState>>()
                         .WithEntityAccess())
            {
                if (combat.ValueRO.EnemyMaxHp <= 0f)
                {
                    combat.ValueRW.EnemyMaxHp = 20f + combat.ValueRO.Zone * 25f;
                    combat.ValueRW.EnemyHp = combat.ValueRO.EnemyMaxHp;
                }

                float dps = (float)combat.ValueRO.HeroDps;
                if (dps > 0f) combat.ValueRW.EnemyHp -= dps * dt;

                if (combat.ValueRO.EnemyHp <= 0f)
                {
                    slice.ValueRW.PrimaryCurrency += combat.ValueRO.GoldPerKill * slice.ValueRO.GlobalMultiplier;
                    combat.ValueRW.Zone += 1;
                    // Max with Stage-inflated Level — never clobber ProgressionLevel back down to Zone.
                    slice.ValueRW.ProgressionLevel = System.Math.Max(slice.ValueRO.ProgressionLevel, combat.ValueRO.Zone);
                    combat.ValueRW.EnemyMaxHp = 20f + combat.ValueRO.Zone * 25f;
                    combat.ValueRW.EnemyHp = combat.ValueRO.EnemyMaxHp;
                    combat.ValueRW.GoldPerKill = 5 + combat.ValueRO.Zone * 2;
                }

                slice.ValueRW.EnemyHp = (int)combat.ValueRO.EnemyHp;
                slice.ValueRW.EnemyMaxHp = (int)combat.ValueRO.EnemyMaxHp;

                if (SystemAPI.HasComponent<CurrentRunStats>(entity))
                {
                    var run = SystemAPI.GetComponentRW<CurrentRunStats>(entity);
                    run.ValueRW.CurrentGold = slice.ValueRO.PrimaryCurrency;
                }
            }
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

            // No Max(1,…) per-frame floor — that turned dps=0.5 into ≥60 HP/s at 60 FPS.
            // Accumulate milli-HP in Timer's unused fractional channel via AfkChestSeconds sign bit? Keep simple:
            // floor(dps*dt); sub-1 frames deal 0 until Instantiated IdleCombatState (float HP) is present.
            int dmg = (int)(dps * dt);
            if (dmg > 0) s.EnemyHp -= dmg;

            if (s.EnemyHp <= 0)
            {
                s.PrimaryCurrency += (5 + s.ProgressionLevel * 3) * s.GlobalMultiplier;
                s.ProgressionLevel += 1;
                s.EnemyMaxHp = 10 + s.ProgressionLevel * 15;
                s.EnemyHp = s.EnemyMaxHp;
            }
        }

        private static void TickEnergy(ref IdleSliceState s, float dt)
        {
            s.EnergyPool = System.Math.Min(100f, s.EnergyPool + dt * 2f);
            if (s.EnergyAllocated <= 0) return;

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
}
