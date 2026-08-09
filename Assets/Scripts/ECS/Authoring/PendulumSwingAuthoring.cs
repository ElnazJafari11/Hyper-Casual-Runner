using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class PendulumSwingAuthoring : MonoBehaviour
    {
        public float MaxAngleDegrees = 45.0f;
        public float Speed = 2.0f;
        public float PhaseOffset = 0.0f;
        public Vector3 SwingAxis = Vector3.forward;
        public float CollisionRadius = 1.0f;
        public float DamageAmount = 15.0f;

        class Baker : Baker<PendulumSwingAuthoring>
        {
            public override void Bake(PendulumSwingAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                Vector3 normAxis = authoring.SwingAxis.sqrMagnitude > 0.0001f 
                    ? authoring.SwingAxis.normalized 
                    : Vector3.forward;

                AddComponent(entity, new PendulumSwingComponent
                {
                    MaxAngleDegrees = authoring.MaxAngleDegrees,
                    Speed = authoring.Speed,
                    PhaseOffset = authoring.PhaseOffset,
                    SwingAxis = normAxis,
                    InitialRotation = authoring.transform.rotation,
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
