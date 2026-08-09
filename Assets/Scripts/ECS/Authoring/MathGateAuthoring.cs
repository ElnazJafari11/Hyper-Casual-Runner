using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class MathGateAuthoring : MonoBehaviour
    {
        public GateOperation Operation;
        public int Value = 10;
        public float CollisionRadius = 1.5f;

        class Baker : Baker<MathGateAuthoring>
        {
            public override void Bake(MathGateAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MathGateComponent
                {
                    Operation = authoring.Operation,
                    Value = authoring.Value,
                    Radius = authoring.CollisionRadius
                });
            }
        }
    }
}
