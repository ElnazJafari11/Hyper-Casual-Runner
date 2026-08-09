using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class IdleStateAuthoring : MonoBehaviour
    {
        public double PrestigeCurrency = 0.0;
        public double CurrentGold = 0.0;

        class Baker : Baker<IdleStateAuthoring>
        {
            public override void Bake(IdleStateAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new PersistentPlayerStats
                {
                    PrestigeCurrency = authoring.PrestigeCurrency,
                    PermanentDamageMultiplier = 1f,
                    PermanentGoldMultiplier = 1f
                });

                AddComponent(entity, new CurrentRunStats
                {
                    CurrentDistance = 0,
                    CurrentGold = authoring.CurrentGold,
                    BaseDamage = 10f
                });

                AddBuffer<ResourceWallet>(entity);
            }
        }
    }
}
