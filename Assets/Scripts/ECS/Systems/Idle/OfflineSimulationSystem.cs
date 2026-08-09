using System;
using System.Globalization;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    /// <summary>
    /// One-shot offline catchup on enter: ProducerComponent wallets and/or IdleSliceState CPS.
    /// Caps at 8h. Slice path credits PendingClaim + HasOfflineClaim (honest claim button).
    /// SystemAPI calls stay in OnUpdate (Entities source-gen forbids them in helpers).
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct OfflineSimulationSystem : ISystem
    {
        private const double MaxOfflineSeconds = 8.0 * 3600.0;

        public void OnCreate(ref SystemState state)
        {
            // Run once even when only IdleSliceState exists (no ProducerComponent required)
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;

            string lastTimeStr = GameProgressData.LastIdleUpdateTime;
            DateTime now = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(lastTimeStr) &&
                DateTime.TryParse(lastTimeStr, null, DateTimeStyles.RoundtripKind, out DateTime lastTime))
            {
                double totalSeconds = Math.Min(MaxOfflineSeconds, (now - lastTime).TotalSeconds);

                if (totalSeconds > 0)
                {
                    foreach (var slice in SystemAPI.Query<RefRW<IdleSliceState>>())
                    {
                        double rate = slice.ValueRO.PassiveRate;
                        if (rate <= 0) continue;

                        double earned = rate * slice.ValueRO.GlobalMultiplier * totalSeconds;
                        if (earned <= 0) continue;

                        slice.ValueRW.PendingClaim += earned;
                        slice.ValueRW.HasOfflineClaim = true;
                        slice.ValueRW.AfkChestSeconds += (float)Math.Min(totalSeconds, 3600.0);
                    }

                    var walletLookup = SystemAPI.GetBufferLookup<ResourceWallet>(false);

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

                    foreach (var (producer, entity) in SystemAPI.Query<RefRO<ProducerComponent>>().WithEntityAccess())
                    {
                        if (!producer.ValueRO.IsAutomated)
                            continue;

                        float interval = producer.ValueRO.ProductionInterval <= 0f ? 1.0f : producer.ValueRO.ProductionInterval;
                        double multiplier = producer.ValueRO.Multiplier <= 0 ? 1.0 : producer.ValueRO.Multiplier;
                        double yield = producer.ValueRO.BaseProductionRate;
                        double amountProduced = (totalSeconds / interval) * yield * multiplier;
                        if (amountProduced <= 0)
                            continue;

                        Entity targetEntity = producer.ValueRO.TargetWalletEntity;
                        if (targetEntity == Entity.Null || !walletLookup.HasBuffer(targetEntity))
                        {
                            if (walletLookup.HasBuffer(entity))
                                targetEntity = entity;
                            else
                                targetEntity = playerWalletEntity;
                        }

                        if (targetEntity != Entity.Null && walletLookup.HasBuffer(targetEntity))
                        {
                            DynamicBuffer<ResourceWallet> wallet = walletLookup[targetEntity];
                            AddOrUpdateResource(wallet, producer.ValueRO.ResourceId, amountProduced);
                        }
                    }
                }
            }

            GameProgressData.LastIdleUpdateTime = now.ToString("O");
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
