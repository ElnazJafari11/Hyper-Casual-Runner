using Unity.Entities;
using Unity.Collections;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    /// <summary>Manual click / tap burst: Cookie Clicker, Egg Inc hatch, Tap Titans taps.</summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct IdleClickProduceSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IdleClickEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (click, entity) in SystemAPI.Query<RefRO<IdleClickEvent>>().WithEntityAccess())
            {
                float mult = click.ValueRO.Multiplier <= 0f ? 1f : click.ValueRO.Multiplier;

                foreach (var slice in SystemAPI.Query<RefRW<IdleSliceState>>())
                {
                    double gain = slice.ValueRO.ClickPower * mult * slice.ValueRO.GlobalMultiplier;
                    if (gain <= 0) gain = 1.0 * mult;

                    switch (slice.ValueRO.Archetype)
                    {
                        case IdleArchetype.EggInc:
                            // Hatch burst: primary eggs + slight passive unlock
                            slice.ValueRW.PrimaryCurrency += gain;
                            slice.ValueRW.OwnedGenerators = System.Math.Max(slice.ValueRO.OwnedGenerators, 1);
                            slice.ValueRW.PassiveRate = System.Math.Max(slice.ValueRO.PassiveRate, 0.5);
                            break;
                        case IdleArchetype.ClickerHeroes:
                        case IdleArchetype.TapTitans2:
                            ApplyTapDamage(ref slice.ValueRW, gain);
                            break;
                        case IdleArchetype.AdventureCapitalist:
                        case IdleArchetype.IdleMinerTycoon:
                            // Manual collect before manager automation graduates the labor
                            double owned = System.Math.Max(1, slice.ValueRO.OwnedGenerators);
                            slice.ValueRW.PrimaryCurrency += owned * System.Math.Max(1, slice.ValueRO.ClickPower) * mult *
                                                             slice.ValueRO.GlobalMultiplier;
                            break;
                        default:
                            slice.ValueRW.PrimaryCurrency += gain;
                            break;
                    }

                    SyncRunStats(ref state, slice.ValueRO.PrimaryCurrency);
                }

                var sfx = ecb.CreateEntity();
                ecb.AddComponent(sfx, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                SystemAPI.SetComponentEnabled<IdleClickEvent>(entity, false);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private static void ApplyTapDamage(ref IdleSliceState slice, double damage)
        {
            // Combat slices store HP in EnemyHp ints when IdleCombatState absent on same entity.
            if (slice.EnemyMaxHp <= 0)
            {
                slice.EnemyMaxHp = 10 + slice.ProgressionLevel * 15;
                slice.EnemyHp = slice.EnemyMaxHp;
            }

            slice.EnemyHp -= (int)System.Math.Max(1, damage);
            if (slice.EnemyHp <= 0)
            {
                double gold = (5 + slice.ProgressionLevel * 3) * slice.GlobalMultiplier;
                slice.PrimaryCurrency += gold;
                slice.ProgressionLevel += 1;
                slice.EnemyMaxHp = 10 + slice.ProgressionLevel * 15;
                slice.EnemyHp = slice.EnemyMaxHp;
                slice.ClickPower += 0.25; // soft progression beat
            }
        }

        private static void SyncRunStats(ref SystemState state, double gold)
        {
            foreach (var run in SystemAPI.Query<RefRW<CurrentRunStats>>())
            {
                run.ValueRW.CurrentGold = gold;
            }
        }
    }
}
