using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class ArcadeIdleAuthoring : MonoBehaviour
    {
        public float HarvestingTime = 1f;
        public int Yield = 5;
        public float Radius = 2f;

        class Baker : Baker<ArcadeIdleAuthoring>
        {
            public override void Bake(ArcadeIdleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new ResourceNodeComponent
                {
                    HarvestingTimeRequired = authoring.HarvestingTime,
                    CurrentProgress = 0f,
                    YieldAmount = authoring.Yield,
                    CollisionRadius = authoring.Radius,
                    IsDepleted = false
                });
            }
        }
    }
}
