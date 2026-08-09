using Unity.Entities;
using Unity.Mathematics;

namespace HyperCasualRunner.ECS.Components
{
    /// <summary>
    /// Component representing a grid floor tile collectible or ground cell in StackyDash / Grid mazes.
    /// </summary>
    public struct GridTileComponent : IComponentData
    {
        public int2 GridPosition;
        public float TileSize;
        public float PickupRadius;
        public bool IsCollected;
        public bool IsWalkable;
        public bool HasTileItem;
        public int TileValue;
    }

    /// <summary>
    /// Component attached to player tracking collected stack tiles and visual offsets.
    /// </summary>
    public struct MazeCollectorComponent : IComponentData
    {
        public int StackedTiles;
        public float TileHeightOffset;
        public Entity StackVisualPrefab;
        public float CollectionRadius;
        public int TotalCollected;
    }

    /// <summary>
    /// Component attached to player handling automatic path paving over gaps.
    /// </summary>
    public struct GridPathfinderComponent : IComponentData
    {
        public Entity PathTilePrefab;
        public float StepDistance;
        public float3 LastTilePosition;
        public bool IsCrossingGap;
        public float PathYHeight;
        public int2 CurrentCell;
        public float3 GridOrigin;
    }

    /// <summary>
    /// Dynamic buffer element holding references to stacked visual entities attached to the collector entity.
    /// </summary>
    public struct StackedTileElement : IBufferElementData
    {
        public Entity VisualEntity;
    }
}
