using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class CoinGateAuthoring : MonoBehaviour
    {
        public MultiplierType GateType = MultiplierType.Additive;
        public float Value = 2.0f;
        public float GateWidth = 3.5f;
        public float TriggerDepth = 0.5f;
        public float MinimumOutput = 1.0f;

        class Baker : Baker<CoinGateAuthoring>
        {
            public override void Bake(CoinGateAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CoinMultiplierGateComponent
                {
                    GateType = authoring.GateType,
                    IsTriggered = false,
                    ReservedPadding = 0,
                    Value = authoring.Value,
                    GateWidth = authoring.GateWidth,
                    TriggerDepth = authoring.TriggerDepth,
                    MinimumOutput = authoring.MinimumOutput
                });
                AddComponent<CoinMultiplierGateTag>(entity);
            }
        }
    }
}
