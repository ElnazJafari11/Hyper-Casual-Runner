using Unity.Entities;
using Unity.Collections;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    /// <summary>
    /// Cosmetics spend IdleSliceState.PrestigeCurrency when a TargetSlice (or sole slice) resolves,
    /// keeping PersistentPlayerStats.PrestigeCurrency synced. Runner-only entities still
    /// spend PersistentPlayerStats directly. Multi-slice worlds without TargetSlice refuse idle spend.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct CosmeticsShopSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CosmeticPurchaseEventComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            Entity sole = IdleEventTarget.FindSoleSlice(em);

            foreach (var (purchaseEvent, entity) in SystemAPI.Query<RefRO<CosmeticPurchaseEventComponent>>().WithEntityAccess())
            {
                int skinIndex = purchaseEvent.ValueRO.TargetSkinIndex;
                double cost = purchaseEvent.ValueRO.PrestigeCost;
                Entity sliceEntity = IdleEventTarget.Resolve(em, entity, purchaseEvent.ValueRO.TargetSlice, sole);

                bool handled = false;

                if (sliceEntity != Entity.Null && em.HasComponent<IdleSliceState>(sliceEntity))
                {
                    handled = true;
                    var slice = em.GetComponentData<IdleSliceState>(sliceEntity);

                    if (em.HasComponent<PersistentPlayerStats>(sliceEntity))
                    {
                        var stats = em.GetComponentData<PersistentPlayerStats>(sliceEntity);
                        stats.PrestigeCurrency = slice.PrestigeCurrency;
                        em.SetComponentData(sliceEntity, stats);
                    }

                    if (GameProgressData.IsSkinUnlocked(skinIndex))
                    {
                        GameProgressData.CurrentSkinIndex = skinIndex;
                    }
                    else if (slice.PrestigeCurrency >= cost)
                    {
                        slice.PrestigeCurrency -= cost;
                        em.SetComponentData(sliceEntity, slice);

                        if (em.HasComponent<PersistentPlayerStats>(sliceEntity))
                        {
                            var stats = em.GetComponentData<PersistentPlayerStats>(sliceEntity);
                            stats.PrestigeCurrency = slice.PrestigeCurrency;
                            em.SetComponentData(sliceEntity, stats);
                        }

                        GameProgressData.UnlockSkin(skinIndex);
                        GameProgressData.CurrentSkinIndex = skinIndex;

                        var soundEntity = ecb.CreateEntity();
                        ecb.AddComponent(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                    }
                }

                if (!handled)
                {
                    foreach (var stats in SystemAPI.Query<RefRW<PersistentPlayerStats>>().WithNone<IdleSliceState>())
                    {
                        handled = true;
                        if (GameProgressData.IsSkinUnlocked(skinIndex))
                        {
                            GameProgressData.CurrentSkinIndex = skinIndex;
                        }
                        else if (stats.ValueRO.PrestigeCurrency >= cost)
                        {
                            stats.ValueRW.PrestigeCurrency -= cost;
                            GameProgressData.UnlockSkin(skinIndex);
                            GameProgressData.CurrentSkinIndex = skinIndex;

                            var soundEntity = ecb.CreateEntity();
                            ecb.AddComponent(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                        }
                    }
                }

                if (!handled)
                {
                    if (GameProgressData.IsSkinUnlocked(skinIndex))
                        GameProgressData.CurrentSkinIndex = skinIndex;
                }

                if (em.HasComponent<CosmeticPurchaseEventComponent>(entity))
                    SystemAPI.SetComponentEnabled<CosmeticPurchaseEventComponent>(entity, false);
            }

            ecb.Playback(em);
            ecb.Dispose();
        }
    }
}
