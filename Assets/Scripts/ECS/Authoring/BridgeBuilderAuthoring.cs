using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class BridgeBuilderAuthoring : MonoBehaviour
    {
        public GameObject BridgeTilePrefab;
        public float DistancePerPlank = 1.0f;

        class Baker : Baker<BridgeBuilderAuthoring>
        {
            public override void Bake(BridgeBuilderAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                Entity prefabEntity = Entity.Null;
                if (authoring.BridgeTilePrefab != null)
                {
                    prefabEntity = GetEntity(authoring.BridgeTilePrefab, TransformUsageFlags.Dynamic);
                }

                AddComponent(entity, new BridgeBuilderComponent
                {
                    BridgeTilePrefab = prefabEntity,
                    DistancePerPlank = authoring.DistancePerPlank,
                    LastPlankZ = -9999f,
                    IsBuilding = false
                });
            }
        }
    }
}
