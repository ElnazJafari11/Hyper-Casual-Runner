using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct IdleShopSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ShopPurchaseEventComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var walletLookup = SystemAPI.GetBufferLookup<ResourceWallet>(false);

            // Locate default target wallet entity (e.g., entity with PlayerComponent or CurrentRunStats)
            Entity playerWalletEntity = Entity.Null;
            foreach (var (_, entity) in SystemAPI.Query<RefRO<PlayerComponent>>().WithEntityAccess())
            {
                if (walletLookup.HasBuffer(entity))
                {
                    playerWalletEntity = entity;
                    break;
                }
            }

            if (playerWalletEntity == Entity.Null)
            {
                foreach (var (_, entity) in SystemAPI.Query<RefRO<CurrentRunStats>>().WithEntityAccess())
                {
                    if (walletLookup.HasBuffer(entity))
                    {
                        playerWalletEntity = entity;
                        break;
                    }
                }
            }

            foreach (var (shopEvent, entity) in SystemAPI.Query<RefRO<ShopPurchaseEventComponent>>().WithEntityAccess())
            {
                if (playerWalletEntity != Entity.Null && walletLookup.HasBuffer(playerWalletEntity))
                {
                    DynamicBuffer<ResourceWallet> wallet = walletLookup[playerWalletEntity];
                    for (int i = 0; i < wallet.Length; i++)
                    {
                        if (wallet[i].ResourceId == 1) // Gold
                        {
                            if (wallet[i].Amount >= shopEvent.ValueRO.Cost)
                            {
                                var item = wallet[i];
                                item.Amount -= shopEvent.ValueRO.Cost;
                                wallet[i] = item;

                                foreach (var producer in SystemAPI.Query<RefRW<ProducerComponent>>())
                                {
                                    if (producer.ValueRO.ResourceId == shopEvent.ValueRO.TargetProducerId)
                                    {
                                        producer.ValueRW.Multiplier *= 2.0;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }

                SystemAPI.SetComponentEnabled<ShopPurchaseEventComponent>(entity, false);
            }
        }
    }
}
