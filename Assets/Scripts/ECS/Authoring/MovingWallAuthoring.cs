using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class MovingWallAuthoring : MonoBehaviour
    {
        public Vector3 MovementAxis = Vector3.right;
        public float MoveDistance = 3.0f;
        public float Speed = 2.0f;
        public float CollisionRadius = 1.0f;
        public float DamageAmount = 10.0f;

        class Baker : Baker<MovingWallAuthoring>
        {
            public override void Bake(MovingWallAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                Vector3 normAxis = authoring.MovementAxis.sqrMagnitude > 0.0001f 
                    ? authoring.MovementAxis.normalized 
                    : Vector3.right;

                AddComponent(entity, new MovingWallComponent
                {
                    MovementAxis = normAxis,
                    MoveDistance = authoring.MoveDistance,
                    Speed = authoring.Speed,
                    InitialPosition = authoring.transform.position,
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
