using Unity.Entities;
using Unity.Collections;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    /// <summary>
    /// Prestige is gated by IdlePrestigeMath.ConvertRunCurrency (0 → no-op).
    /// Mutations are scoped to TargetSlice (or sole-slice fallback) — never world-wide.
    /// PersistentPlayerStats on the slice entity syncs to IdleSliceState.PrestigeCurrency.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct PrestigeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PrestigeEventComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var em = state.EntityManager;
            Entity sole = IdleEventTarget.FindSoleSlice(em);

            foreach (var (evt, entity) in SystemAPI.Query<RefRO<PrestigeEventComponent>>().WithEntityAccess())
            {
                Entity sliceEntity = IdleEventTarget.Resolve(em, entity, evt.ValueRO.TargetSlice, sole);

                // Runner-only prestige (no IdleSliceState in world): gated ConvertRunCurrency, no flat +1
                if (sliceEntity == Entity.Null)
                {
                    bool anyIdle = false;
                    foreach (var _ in SystemAPI.Query<RefRO<IdleSliceState>>())
                    {
                        anyIdle = true;
                        break;
                    }

                    if (!anyIdle)
                    {
                        double runGold = 0;
                        foreach (var currentRunStats in SystemAPI.Query<RefRO<CurrentRunStats>>())
                        {
                            runGold = currentRunStats.ValueRO.CurrentGold;
                            break;
                        }

                        double runnerConverted = IdlePrestigeMath.ConvertRunCurrency(runGold);
                        if (runnerConverted >= 1)
                        {
                            foreach (var persistentStats in SystemAPI.Query<RefRW<PersistentPlayerStats>>())
                                persistentStats.ValueRW.PrestigeCurrency += runnerConverted;

                            foreach (var currentRunStats in SystemAPI.Query<RefRW<CurrentRunStats>>())
                            {
                                currentRunStats.ValueRW.CurrentGold = 0.0;
                                currentRunStats.ValueRW.CurrentDistance = 0;
                            }

                            foreach (var walletBuffer in SystemAPI.Query<DynamicBuffer<ResourceWallet>>())
                                walletBuffer.Clear();

                            foreach (var producer in SystemAPI.Query<RefRW<ProducerComponent>>())
                            {
                                producer.ValueRW.Timer = 0f;
                                producer.ValueRW.Multiplier = 1.0;
                            }

                            var sfx = ecb.CreateEntity();
                            ecb.AddComponent(sfx, new PlaySoundEventComponent { SoundToPlay = SoundType.Victory });
                        }
                    }

                    SystemAPI.SetComponentEnabled<PrestigeEventComponent>(entity, false);
                    if (!em.HasComponent<IdleSliceState>(entity))
                        ecb.DestroyEntity(entity);
                    continue;
                }

                var slice = em.GetComponentData<IdleSliceState>(sliceEntity);
                double sliceConverted = IdlePrestigeMath.ConvertRunCurrency(slice.PrimaryCurrency);
                if (sliceConverted < 1)
                {
                    // Honest no-op below threshold — leave combat/gens untouched
                    SystemAPI.SetComponentEnabled<PrestigeEventComponent>(entity, false);
                    if (!em.HasComponent<IdleSliceState>(entity))
                        ecb.DestroyEntity(entity);
                    continue;
                }

                slice.PrestigeCurrency += sliceConverted;
                slice.PrimaryCurrency = 0;
                slice.OwnedGenerators = 0;
                slice.PassiveRate = 0;
                slice.PendingClaim = 0;
                slice.ManagersHired = 0;
                slice.AssignedWorkers = 0;
                slice.ProgressionLevel = 0;
                slice.EnemyHp = 0;
                slice.EnemyMaxHp = 0;
                // Intentionally retain FactionId / EnergyPool / EnergyAllocated / SkillXp / PhaseIndex.
                slice.ClickPower = 1 + slice.PrestigeCurrency * 0.5;
                slice.GlobalMultiplier = 1f + (float)slice.PrestigeCurrency * 0.1f;
                em.SetComponentData(sliceEntity, slice);

                // Single ledger: sync PersistentPlayerStats on this entity to slice prestige
                if (em.HasComponent<PersistentPlayerStats>(sliceEntity))
                {
                    var stats = em.GetComponentData<PersistentPlayerStats>(sliceEntity);
                    stats.PrestigeCurrency = slice.PrestigeCurrency;
                    em.SetComponentData(sliceEntity, stats);
                }

                if (em.HasComponent<CurrentRunStats>(sliceEntity))
                {
                    var run = em.GetComponentData<CurrentRunStats>(sliceEntity);
                    run.CurrentGold = 0;
                    run.CurrentDistance = 0;
                    em.SetComponentData(sliceEntity, run);
                }

                if (em.HasBuffer<ResourceWallet>(sliceEntity))
                    em.GetBuffer<ResourceWallet>(sliceEntity).Clear();

                if (em.HasComponent<BuyableGenerator>(sliceEntity))
                {
                    var gen = em.GetComponentData<BuyableGenerator>(sliceEntity);
                    IdlePrestigeMath.ResetBuyableGenerator(ref gen, slice.Archetype);
                    em.SetComponentData(sliceEntity, gen);
                }

                if (em.HasComponent<IdleManager>(sliceEntity))
                {
                    var manager = em.GetComponentData<IdleManager>(sliceEntity);
                    manager.IsHired = false;
                    em.SetComponentData(sliceEntity, manager);
                }

                if (em.HasComponent<ProducerComponent>(sliceEntity))
                {
                    var producer = em.GetComponentData<ProducerComponent>(sliceEntity);
                    producer.Timer = 0f;
                    producer.Multiplier = 1.0;
                    em.SetComponentData(sliceEntity, producer);
                }

                if (em.HasComponent<IdleCombatState>(sliceEntity))
                {
                    var combat = em.GetComponentData<IdleCombatState>(sliceEntity);
                    combat.Zone = 1;
                    combat.EnemyHp = 20f;
                    combat.EnemyMaxHp = 20f;
                    combat.GoldPerKill = 5;
                    combat.TapDamage = slice.ClickPower;
                    combat.HeroDps = System.Math.Max(1.0, slice.ClickPower * 0.25);
                    em.SetComponentData(sliceEntity, combat);
                    // Keep HUD Lv/Zone in sync with combat SoT after reset (was left at 0).
                    slice.ProgressionLevel = combat.Zone;
                    em.SetComponentData(sliceEntity, slice);
                }

                var soundEntity = ecb.CreateEntity();
                ecb.AddComponent(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Victory });

                SystemAPI.SetComponentEnabled<PrestigeEventComponent>(entity, false);
                if (!em.HasComponent<IdleSliceState>(entity))
                    ecb.DestroyEntity(entity);
            }

            ecb.Playback(em);
            ecb.Dispose();
        }
    }
}
