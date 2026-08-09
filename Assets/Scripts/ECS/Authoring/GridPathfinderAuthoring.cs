using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class GridPathfinderAuthoring : MonoBehaviour
    {
        public GameObject PathTilePrefab;
        public float StepDistance = 1.0f;
        public float PathYHeight = 0.0f;
        public Vector3 GridOrigin = Vector3.zero;

        class Baker : Baker<GridPathfinderAuthoring>
        {
            public override void Bake(GridPathfinderAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                Entity prefabEntity = Entity.Null;
                if (authoring.PathTilePrefab != null)
                {
                    prefabEntity = GetEntity(authoring.PathTilePrefab, TransformUsageFlags.Dynamic);
                }

                AddComponent(entity, new GridPathfinderComponent
                {
                    PathTilePrefab = prefabEntity,
                    StepDistance = authoring.StepDistance,
                    LastTilePosition = new float3(-9999f, -9999f, -9999f),
                    IsCrossingGap = false,
                    PathYHeight = authoring.PathYHeight,
                    CurrentCell = int2.zero,
                    GridOrigin = new float3(authoring.GridOrigin.x, authoring.GridOrigin.y, authoring.GridOrigin.z)
                });
            }
        }
    }
}
