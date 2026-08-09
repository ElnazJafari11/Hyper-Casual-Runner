using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class ShooterAuthoring : MonoBehaviour
    {
        public GameObject ProjectilePrefab;
        public float FireRate = 0.2f;
        public int SpreadCount = 1;
        public float SpreadAngle = 15f;
        public int Pierce = 0;

        class Baker : Baker<ShooterAuthoring>
        {
            public override void Bake(ShooterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                Entity prefabEntity = Entity.Null;
                if (authoring.ProjectilePrefab != null)
                {
                    prefabEntity = GetEntity(authoring.ProjectilePrefab, TransformUsageFlags.Dynamic);
                }

                AddComponent(entity, new ShooterComponent
                {
                    ProjectilePrefab = prefabEntity,
                    FireRate = authoring.FireRate,
                    Timer = 0f,
                    SpreadCount = authoring.SpreadCount,
                    SpreadAngle = authoring.SpreadAngle,
                    Pierce = authoring.Pierce
                });
            }
        }
    }
}
