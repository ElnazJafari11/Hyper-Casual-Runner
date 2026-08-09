using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class ProjectileAuthoring : MonoBehaviour
    {
        public float Speed = 30f;
        public float Damage = 1f;
        public Vector3 Direction = Vector3.forward;
        public float Lifetime = 3f;
        public float CollisionRadius = 0.5f;
        public int PierceCount = 0;

        class Baker : Baker<ProjectileAuthoring>
        {
            public override void Bake(ProjectileAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new ProjectileComponent
                {
                    Speed = authoring.Speed,
                    Damage = authoring.Damage,
                    Direction = authoring.Direction,
                    Lifetime = authoring.Lifetime,
                    CollisionRadius = authoring.CollisionRadius,
                    PierceCount = authoring.PierceCount
                });
            }
        }
    }
}
