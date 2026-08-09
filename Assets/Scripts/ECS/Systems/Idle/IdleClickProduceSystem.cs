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
            var em = state.EntityManager;
            Entity sole = IdleEventTarget.FindSoleSlice(em);

            foreach (var (click, entity) in SystemAPI.Query<RefRO<IdleClickEvent>>().WithEntityAccess())
            {
                Entity sliceEntity = IdleEventTarget.Resolve(em, entity, click.ValueRO.TargetSlice, sole);
                if (sliceEntity == Entity.Null || !em.HasComponent<IdleSliceState>(sliceEntity))
                {
                    ecb.DestroyEntity(entity);
                    continue;
                }

                float mult = click.ValueRO.Multiplier <= 0f ? 1f : click.ValueRO.Multiplier;
                var slice = em.GetComponentData<IdleSliceState>(sliceEntity);
                double gain = slice.ClickPower * mult * slice.GlobalMultiplier;
                if (gain <= 0) gain = 1.0 * mult;

                switch (slice.Archetype)
                {
                    case IdleArchetype.EggInc:
                        slice.PrimaryCurrency += gain;
                        slice.OwnedGenerators = System.Math.Max(slice.OwnedGenerators, 1);
                        slice.PassiveRate = System.Math.Max(slice.PassiveRate, 0.5);
                        break;
                    case IdleArchetype.ClickerHeroes:
                    case IdleArchetype.TapTitans2:
                        ApplyTapDamage(ref slice, gain, em, sliceEntity);
                        break;
                    case IdleArchetype.IdleHeroes:
                        // Auto-combat owns progress — click must not mint flat gold
                        break;
                    case IdleArchetype.AdventureCapitalist:
                    case IdleArchetype.IdleMinerTycoon:
                        double owned = System.Math.Max(1, slice.OwnedGenerators);
                        slice.PrimaryCurrency += owned * System.Math.Max(1, slice.ClickPower) *
                                                 mult * slice.GlobalMultiplier;
                        break;
                    case IdleArchetype.MelvorIdle:
                        slice.PrimaryCurrency += gain;
                        // Skill XP owned by IdleSkillNode — click only grants currency
                        break;
                    case IdleArchetype.AfkArena:
                        // Chest-only MVP: campaign advances stage + chest fill; claim flag at ≥10s.
                        slice.ProgressionLevel += 1;
                        slice.AfkChestSeconds += 2f;
                        if (slice.AfkChestSeconds >= 10f)
                            slice.HasOfflineClaim = true;
                        break;
                    default:
                        slice.PrimaryCurrency += gain;
                        break;
                }

                em.SetComponentData(sliceEntity, slice);
                IdleEventTarget.SyncPairedRunGold(em, sliceEntity, slice.PrimaryCurrency);

                var sfx = ecb.CreateEntity();
                ecb.AddComponent(sfx, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(em);
            ecb.Dispose();
        }

        private static void ApplyTapDamage(ref IdleSliceState slice, double damage, EntityManager em, Entity sliceEntity)
        {
            // Prefer IdleCombatState as single HP authority when present
            if (em.HasComponent<IdleCombatState>(sliceEntity))
            {
                var combat = em.GetComponentData<IdleCombatState>(sliceEntity);
                if (combat.EnemyMaxHp <= 0f)
                {
                    combat.EnemyMaxHp = 20f + combat.Zone * 25f;
                    combat.EnemyHp = combat.EnemyMaxHp;
                }

                combat.EnemyHp -= (float)System.Math.Max(1.0, damage);
                if (combat.EnemyHp <= 0f)
                {
                    slice.PrimaryCurrency += combat.GoldPerKill * slice.GlobalMultiplier;
                    combat.Zone += 1;
                    slice.ProgressionLevel = combat.Zone;
                    combat.EnemyMaxHp = 20f + combat.Zone * 25f;
                    combat.EnemyHp = combat.EnemyMaxHp;
                    combat.GoldPerKill = 5 + combat.Zone * 2;
                    combat.TapDamage += 0.25;
                    slice.ClickPower = combat.TapDamage;
                }

                slice.EnemyHp = (int)combat.EnemyHp;
                slice.EnemyMaxHp = (int)combat.EnemyMaxHp;
                em.SetComponentData(sliceEntity, combat);
                return;
            }

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
                slice.ClickPower += 0.25;
            }
        }
    }
}
