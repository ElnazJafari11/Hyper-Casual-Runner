using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class TransformationAuthoring : MonoBehaviour
    {
        public float StartingHeight = 1f;
        public float StartingWidth = 1f;

        class Baker : Baker<TransformationAuthoring>
        {
            public override void Bake(TransformationAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new TransformationComponent
                {
                    TargetHeight = authoring.StartingHeight,
                    TargetWidth = authoring.StartingWidth
                });
            }
        }
    }
}
