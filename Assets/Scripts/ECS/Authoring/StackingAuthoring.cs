using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class StackingAuthoring : MonoBehaviour
    {
        public GameObject StackItemPrefab;
        public float ItemHeightOffset = 0.5f;

        class Baker : Baker<StackingAuthoring>
        {
            public override void Bake(StackingAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                Entity prefabEntity = Entity.Null;
                if (authoring.StackItemPrefab != null)
                {
                    prefabEntity = GetEntity(authoring.StackItemPrefab, TransformUsageFlags.Dynamic);
                }

                AddComponent(entity, new StackComponent
                {
                    CurrentCount = 0,
                    ItemHeightOffset = authoring.ItemHeightOffset,
                    StackItemPrefab = prefabEntity
                });
            }
        }
    }
}
