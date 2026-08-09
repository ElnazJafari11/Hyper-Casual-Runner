using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class SplittingHazardAuthoring : MonoBehaviour
    {
        public int SplitCount = 2;
        public GameObject ChildPrefab;
        public float ImpulseForce = 2.0f;
        public float TriggerDistance = 5.0f;
        public float CollisionRadius = 1.5f;
        public float DamageAmount = 20.0f;

        class Baker : Baker<SplittingHazardAuthoring>
        {
            public override void Bake(SplittingHazardAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                Entity childPrefabEntity = Entity.Null;
                if (authoring.ChildPrefab != null)
                {
                    childPrefabEntity = GetEntity(authoring.ChildPrefab, TransformUsageFlags.Dynamic);
                }

                AddComponent(entity, new SplittingHazardComponent
                {
                    SplitCount = authoring.SplitCount,
                    ChildPrefab = childPrefabEntity,
                    ImpulseForce = authoring.ImpulseForce,
                    TriggerDistance = authoring.TriggerDistance,
                    HasSplit = false,
                    CollisionRadius = authoring.CollisionRadius,
                    DamageAmount = authoring.DamageAmount
                });

                AddComponent(entity, new ObstacleComponent
                {
                    CollisionRadius = authoring.CollisionRadius,
                    DamageAmount = authoring.DamageAmount
                });
            }
        }
    }
}
