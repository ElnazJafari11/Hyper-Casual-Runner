using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class CollectibleAuthoring : MonoBehaviour
    {
        public float CollisionRadius = 0.8f;
        public double GoldValue = 1.0;

        class Baker : Baker<CollectibleAuthoring>
        {
            public override void Bake(CollectibleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CollectibleComponent
                {
                    CollisionRadius = authoring.CollisionRadius,
                    GoldValue = authoring.GoldValue
                });
            }
        }
    }
}
