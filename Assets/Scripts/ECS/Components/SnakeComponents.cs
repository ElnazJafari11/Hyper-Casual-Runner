using Unity.Entities;
using Unity.Mathematics;

namespace HyperCasualRunner.ECS.Components
{
    /// <summary>
    /// Component attached to Snake Head / Leader entity managing chain length, spacing, and segment prefabs.
    /// </summary>
    public struct SnakeChainComponent : IComponentData
    {
        public int TargetLength;        // Target number of active follower segments
        public int CurrentLength;       // Current instantiated count of follower segments
        public float SegmentSpacing;    // Distance along path between consecutive segments
        public float FollowSpeed;       // Interpolation movement speed for followers
        public float RotationSpeed;     // Interpolation rotation speed for followers
        public Entity FollowerPrefab;   // Entity prefab used to instantiate follower body segments
        public float HeadRadius;        // Collision/pickup radius of the snake head
    }

    /// <summary>
    /// Dynamic buffer element attached to Snake Head recording historical position and rotation sample points along the trail.
    /// </summary>
    [InternalBufferCapacity(64)]
    public struct SnakeSegmentBuffer : IBufferElementData
    {
        public float3 Position;             // World space position of head at sample point
        public quaternion Rotation;         // World space rotation of head at sample point
        public float AccumulatedDistance;   // Total path distance accumulated up to this point
    }

    /// <summary>
    /// Dynamic buffer element attached to Snake Head maintaining ordered references to instantiated follower entities.
    /// </summary>
    public struct SnakeFollowerLinkBuffer : IBufferElementData
    {
        public Entity FollowerEntity;
    }

    /// <summary>
    /// Component attached to each individual follower body segment entity in the snake chain.
    /// </summary>
    public struct SnakeFollowerComponent : IComponentData
    {
        public Entity LeaderEntity;   // Reference to Snake Head entity
        public int SegmentIndex;      // 1-based index in chain
        public float DistanceOffset;  // Target distance offset behind head
        public float FollowerRadius;  // Collision radius of segment
        public bool IsRecruited;      // Flag indicating whether unit has joined chain
        public bool IsDying;          // Flag indicating segment removal/death transition
    }

    /// <summary>
    /// Component attached to standalone recruitment collectibles waiting on the track to join the snake chain.
    /// </summary>
    public struct SnakeJoinCollectibleComponent : IComponentData
    {
        public int JoinCount;           // Number of follower segments added to chain upon collection
        public float CollisionRadius;   // Pickup radius
        public bool IsCollected;        // Prevents multi-trigger in single frame
    }

    /// <summary>
    /// Component attached to track obstacles interacting with snake head or follower segments.
    /// </summary>
    public struct SnakeObstacleComponent : IComponentData
    {
        public int SeverCount;          // Number of segments lost/severed on impact
        public float CollisionRadius;   // Impact detection radius
        public bool DestroyOnImpact;    // Whether obstacle entity is destroyed after impact
    }

    /// <summary>
    /// Hybrid ECS presentation event tag for follower join audio/VFX.
    /// </summary>
    public struct SnakeJoinEventComponent : IComponentData
    {
        public float3 Position;
        public int JoinedAmount;
    }

    /// <summary>
    /// Hybrid ECS presentation event tag for follower sever/damage audio/VFX.
    /// </summary>
    public struct SnakeSeverEventComponent : IComponentData
    {
        public float3 Position;
        public int SeveredAmount;
    }
}
