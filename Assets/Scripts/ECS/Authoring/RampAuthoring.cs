using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class RampAuthoring : MonoBehaviour
    {
        public float JumpForce = 15f;
        public float SpeedBoost = 1.5f;
        public float CollisionRadius = 2f;

        class Baker : Baker<RampAuthoring>
        {
            public override void Bake(RampAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new RampComponent
                {
                    JumpForce = authoring.JumpForce,
                    SpeedBoost = authoring.SpeedBoost,
                    CollisionRadius = authoring.CollisionRadius
                });
            }
        }
    }
}
