using Unity.Entities;
using Unity.Mathematics;

namespace HyperCasualRunner.ECS.Components
{
    public struct BridgeBuilderComponent : IComponentData
    {
        public Entity BridgeTilePrefab;
        public float DistancePerPlank;
        public float LastPlankZ;
        public bool IsBuilding;
    }

    public struct GapZoneComponent : IComponentData
    {
        public float3 MinBounds;
        public float3 MaxBounds;
    }
}
