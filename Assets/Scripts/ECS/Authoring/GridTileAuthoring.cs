using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class GridTileAuthoring : MonoBehaviour
    {
        public Vector2Int GridPosition = Vector2Int.zero;
        public float TileSize = 1.0f;
        public float PickupRadius = 0.8f;
        public bool IsWalkable = true;
        public bool HasTileItem = true;
        public int TileValue = 1;

        class Baker : Baker<GridTileAuthoring>
        {
            public override void Bake(GridTileAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new GridTileComponent
                {
                    GridPosition = new int2(authoring.GridPosition.x, authoring.GridPosition.y),
                    TileSize = authoring.TileSize,
                    PickupRadius = authoring.PickupRadius,
                    IsCollected = false,
                    IsWalkable = authoring.IsWalkable,
                    HasTileItem = authoring.HasTileItem,
                    TileValue = authoring.TileValue
                });
            }
        }
    }
}
