using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class MazeCollectorAuthoring : MonoBehaviour
    {
        public GameObject StackVisualPrefab;
        public float TileHeightOffset = 0.2f;
        public float CollectionRadius = 0.8f;
        public int StartingTiles = 0;

        class Baker : Baker<MazeCollectorAuthoring>
        {
            public override void Bake(MazeCollectorAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                Entity prefabEntity = Entity.Null;
                if (authoring.StackVisualPrefab != null)
                {
                    prefabEntity = GetEntity(authoring.StackVisualPrefab, TransformUsageFlags.Dynamic);
                }

                AddComponent(entity, new MazeCollectorComponent
                {
                    StackedTiles = authoring.StartingTiles,
                    TileHeightOffset = authoring.TileHeightOffset,
                    StackVisualPrefab = prefabEntity,
                    CollectionRadius = authoring.CollectionRadius,
                    TotalCollected = 0
                });

                AddBuffer<StackedTileElement>(entity);
            }
        }
    }
}
