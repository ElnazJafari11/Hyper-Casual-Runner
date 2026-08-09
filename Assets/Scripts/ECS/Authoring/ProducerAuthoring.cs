using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class ProducerAuthoring : MonoBehaviour
    {
        public int ResourceId;
        public double BaseProductionRate = 1.0;
        public float ProductionInterval = 1.0f;
        public bool IsAutomated = true;

        class Baker : Baker<ProducerAuthoring>
        {
            public override void Bake(ProducerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new ProducerComponent
                {
                    ResourceId = authoring.ResourceId,
                    BaseProductionRate = authoring.BaseProductionRate,
                    ProductionInterval = authoring.ProductionInterval,
                    Timer = 0f,
                    Multiplier = 1.0,
                    IsAutomated = authoring.IsAutomated,
                    TargetWalletEntity = Entity.Null
                });

                AddBuffer<ResourceWallet>(entity);
            }
        }
    }
}
