using Unity.Entities;
using Unity.Collections;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
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
                foreach (var stats in SystemAPI.Query<RefRW<PersistentPlayerStats>>())
                {
                    handled = true;
                    if (GameProgressData.IsSkinUnlocked(skinIndex))
                    {
                        // Already unlocked: equip skin without deducting currency
                        GameProgressData.CurrentSkinIndex = skinIndex;
                    }
                    else if (stats.ValueRO.PrestigeCurrency >= cost)
                    {
                        // Deduct PrestigeCurrency, unlock skin, and equip skin
                        stats.ValueRW.PrestigeCurrency -= cost;
                        GameProgressData.UnlockSkin(skinIndex);
                        GameProgressData.CurrentSkinIndex = skinIndex;

                        var soundEntity = ecb.CreateEntity();
                        ecb.AddComponent(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                    }
                }

                // If no PersistentPlayerStats entity exists, fall back to checking/unlocking via GameProgressData directly if free/unlocked
                if (!handled)
                {
                    if (GameProgressData.IsSkinUnlocked(skinIndex))
                    {
                        GameProgressData.CurrentSkinIndex = skinIndex;
                    }
                }

                if (state.EntityManager.HasComponent<CosmeticPurchaseEventComponent>(entity))
                {
                    SystemAPI.SetComponentEnabled<CosmeticPurchaseEventComponent>(entity, false);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
