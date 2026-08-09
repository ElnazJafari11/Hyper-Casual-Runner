using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class EnemyAuthoring : MonoBehaviour
    {
        public float CollisionRadius = 0.5f;
        public float Health = 1f;

        class Baker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EnemyComponent { 
                    CollisionRadius = authoring.CollisionRadius,
                    Health = authoring.Health
                });
            }
        }
    }
}
