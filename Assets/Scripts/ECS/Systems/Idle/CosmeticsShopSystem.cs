using Unity.Entities;
using Unity.Collections;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    /// <summary>
    /// Cosmetics spend IdleSliceState.PrestigeCurrency when present (source of truth),
    /// keeping PersistentPlayerStats.PrestigeCurrency synced. Runner-only entities still
    /// spend PersistentPlayerStats directly.
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
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (purchaseEvent, entity) in SystemAPI.Query<RefRO<CosmeticPurchaseEventComponent>>().WithEntityAccess())
            {
                int skinIndex = purchaseEvent.ValueRO.TargetSkinIndex;
                double cost = purchaseEvent.ValueRO.PrestigeCost;

                bool handled = false;

                // Prefer idle slice prestige ledger when co-located
                foreach (var (slice, stats) in SystemAPI.Query<RefRW<IdleSliceState>, RefRW<PersistentPlayerStats>>())
                {
                    handled = true;
                    // Keep ledgers aligned before spend
                    stats.ValueRW.PrestigeCurrency = slice.ValueRO.PrestigeCurrency;

                    if (GameProgressData.IsSkinUnlocked(skinIndex))
                    {
                        GameProgressData.CurrentSkinIndex = skinIndex;
                    }
                    else if (slice.ValueRO.PrestigeCurrency >= cost)
                    {
                        slice.ValueRW.PrestigeCurrency -= cost;
                        stats.ValueRW.PrestigeCurrency = slice.ValueRO.PrestigeCurrency;
                        GameProgressData.UnlockSkin(skinIndex);
                        GameProgressData.CurrentSkinIndex = skinIndex;

                        var soundEntity = ecb.CreateEntity();
                        ecb.AddComponent(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                    }
                    break; // first idle slice with stats
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

                if (state.EntityManager.HasComponent<CosmeticPurchaseEventComponent>(entity))
                    SystemAPI.SetComponentEnabled<CosmeticPurchaseEventComponent>(entity, false);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
