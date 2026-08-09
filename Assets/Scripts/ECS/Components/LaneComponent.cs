using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public struct LaneComponent : IComponentData
    {
        public int CurrentLane;     // -1 (Left), 0 (Center), 1 (Right)
        public int TargetLane;      // -1, 0, 1
        public float LaneWidth;     // X offset between lanes (default 2.5m)
        public float LaneChangeSpeed; // Interpolation speed
    }

    public struct LaneObstacleComponent : IComponentData
    {
        public int Lane;            // -1, 0, 1
        public float CollisionRadius;
    }
}
