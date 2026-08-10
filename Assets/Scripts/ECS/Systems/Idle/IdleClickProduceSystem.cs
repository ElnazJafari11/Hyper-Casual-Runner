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
                    IdleEventTarget.DestroyIfEphemeral(em, ecb, entity, sliceEntity);
                    continue;
                }

                float mult = click.ValueRO.Multiplier <= 0f ? 1f : click.ValueRO.Multiplier;
                var slice = em.GetComponentData<IdleSliceState>(sliceEntity);
                double gain = slice.ClickPower * mult * slice.GlobalMultiplier;

                switch (slice.Archetype)
                {
                    case IdleArchetype.EggInc:
                        if (gain <= 0) gain = 1.0 * mult;
                        slice.PrimaryCurrency += gain;
                        slice.OwnedGenerators = System.Math.Max(slice.OwnedGenerators, 1);
                        slice.PassiveRate = System.Math.Max(slice.PassiveRate, 0.5);
                        break;
                    case IdleArchetype.ClickerHeroes:
                    case IdleArchetype.TapTitans2:
                        // Fractional taps allowed — TapDamage/ClickPower drive HP loss (no Max(1) floor).
                        ApplyTapDamage(ref slice, gain, mult, em, sliceEntity);
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
                        if (gain <= 0) gain = 1.0 * mult;
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
                        if (gain <= 0) gain = 1.0 * mult;
                        slice.PrimaryCurrency += gain;
                        break;
                }

                em.SetComponentData(sliceEntity, slice);
                IdleEventTarget.SyncPairedRunGold(em, sliceEntity, slice.PrimaryCurrency);

                var sfx = ecb.CreateEntity();
                ecb.AddComponent(sfx, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                IdleEventTarget.DestroyIfEphemeral(em, ecb, entity, sliceEntity);
            }

            ecb.Playback(em);
            ecb.Dispose();
        }

        private static void ApplyTapDamage(ref IdleSliceState slice, double clickPowerDamage, float mult,
            EntityManager em, Entity sliceEntity)
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

                // TapDamage drives combat hits; fall back to ClickPower-derived damage if unset.
                double baseTap = combat.TapDamage > 0.0 ? combat.TapDamage : slice.ClickPower;
                double applied = baseTap * mult * slice.GlobalMultiplier;
                combat.EnemyHp -= (float)applied;
                if (combat.EnemyHp <= 0f)
                {
                    slice.PrimaryCurrency += combat.GoldPerKill * slice.GlobalMultiplier;
                    combat.Zone += 1;
                    // Max with Stage-inflated Level — never clobber ProgressionLevel back down to Zone.
                    slice.ProgressionLevel = System.Math.Max(slice.ProgressionLevel, combat.Zone);
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

            // Non-combat fallback: still no Max(1) floor; int HP truncates sub-1 to 0.
            int hit = (int)clickPowerDamage;
            slice.EnemyHp -= hit;
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
