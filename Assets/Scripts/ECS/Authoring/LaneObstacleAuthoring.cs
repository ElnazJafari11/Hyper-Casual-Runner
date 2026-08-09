using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class LaneObstacleAuthoring : MonoBehaviour
    {
        public int Lane = 0; // -1, 0, 1
        public float CollisionRadius = 1.0f;

        class Baker : Baker<LaneObstacleAuthoring>
        {
            public override void Bake(LaneObstacleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new LaneObstacleComponent
                {
                    Lane = authoring.Lane,
                    CollisionRadius = authoring.CollisionRadius
                });
            }
        }
    }
}
