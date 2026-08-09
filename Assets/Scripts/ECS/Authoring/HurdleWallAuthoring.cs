using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class HurdleWallAuthoring : MonoBehaviour
    {
        public float RequiredHeight = 2.0f;
        public float CollisionRadius = 1.5f;

        class Baker : Baker<HurdleWallAuthoring>
        {
            public override void Bake(HurdleWallAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new HurdleWallComponent
                {
                    RequiredHeight = authoring.RequiredHeight,
                    CollisionRadius = authoring.CollisionRadius,
                    Cleared = false
                });
            }
        }
    }
}
