using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct IdleProductionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ProducerComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
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

            foreach (var (producer, entity) in SystemAPI.Query<RefRW<ProducerComponent>>().WithEntityAccess())
            {
                if (!producer.ValueRO.IsAutomated)
                {
                    continue;
                }

                float interval = producer.ValueRO.ProductionInterval <= 0f ? 1.0f : producer.ValueRO.ProductionInterval;
                producer.ValueRW.Timer += deltaTime;

                if (producer.ValueRO.Timer >= interval)
                {
                    double multiplier = producer.ValueRO.Multiplier <= 0 ? 1.0 : producer.ValueRO.Multiplier;
                    double amountProduced = producer.ValueRO.BaseProductionRate * multiplier;
                    
                    producer.ValueRW.Timer -= interval;

                    Entity targetEntity = producer.ValueRO.TargetWalletEntity;
                    if (targetEntity == Entity.Null || !walletLookup.HasBuffer(targetEntity))
                    {
                        if (walletLookup.HasBuffer(entity))
                        {
                            targetEntity = entity;
                        }
                        else
                        {
                            targetEntity = playerWalletEntity;
                        }
                    }

                    if (targetEntity != Entity.Null && walletLookup.HasBuffer(targetEntity))
                    {
                        DynamicBuffer<ResourceWallet> wallet = walletLookup[targetEntity];
                        AddOrUpdateResource(wallet, producer.ValueRO.ResourceId, amountProduced);
                    }
                }
            }
        }

        private static void AddOrUpdateResource(DynamicBuffer<ResourceWallet> wallet, int resourceId, double amount)
        {
            for (int i = 0; i < wallet.Length; i++)
            {
                if (wallet[i].ResourceId == resourceId)
                {
                    var item = wallet[i];
                    item.Amount += amount;
                    wallet[i] = item;
                    return;
                }
            }

            wallet.Add(new ResourceWallet
            {
                ResourceId = resourceId,
                Amount = amount
            });
        }
    }
}
