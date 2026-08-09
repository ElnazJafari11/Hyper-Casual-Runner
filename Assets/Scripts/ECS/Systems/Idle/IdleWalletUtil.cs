using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    internal static class IdleWalletUtil
    {
        public static void AddOrUpdate(DynamicBuffer<ResourceWallet> wallet, int resourceId, double amount)
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

            wallet.Add(new ResourceWallet { ResourceId = resourceId, Amount = amount });
        }

        public static bool TrySpend(DynamicBuffer<ResourceWallet> wallet, int resourceId, double cost)
        {
            for (int i = 0; i < wallet.Length; i++)
            {
                if (wallet[i].ResourceId != resourceId) continue;
                if (wallet[i].Amount < cost) return false;
                var item = wallet[i];
                item.Amount -= cost;
                wallet[i] = item;
                return true;
            }
            return false;
        }

        public static double Get(DynamicBuffer<ResourceWallet> wallet, int resourceId)
        {
            for (int i = 0; i < wallet.Length; i++)
            {
                if (wallet[i].ResourceId == resourceId) return wallet[i].Amount;
            }
            return 0.0;
        }

        public const int Gold = 1;
        public const int Prestige = 2;
        public const int Wood = 3;
        public const int Soft = 4;
    }
}
