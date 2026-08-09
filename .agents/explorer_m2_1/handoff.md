# Explorer Handoff Report — Milestone 2: Snake Follower Chain & Collision (5_JoinClash_Snake)

**Agent**: Explorer 1  
**Milestone**: M2 (Snake Follower Chain & Collision)  
**Working Directory**: `d:\Git\Hyper-Casual-Runner\.agents\explorer_m2_1`  
**Date**: 2026-07-22  

---

## 1. Observation

### Codebase & Asset Inspection Findings

#### A. Prefab Analysis (`Assets/ToolkitExamples/5_JoinClash_Snake_Slice.prefab`)
- **File Location**: `Assets/ToolkitExamples/5_JoinClash_Snake_Slice.prefab`
- **Generator Source**: Built via `ToolkitExampleGenerator.cs` (lines 221–245).
- **Current Component Setup**:
  - `PlayerEntity` currently has `PlayerAuthoring` and `StackingAuthoring` attached.
  - `StackingAuthoring` was assigned during initial slice generation as a placeholder for vertical stacking. For Milestone 2, this must be replaced with `SnakeAuthoring` / `SnakeChainComponent`.
  - Slice layout includes collectible cube grids along the track (`z = -20` to `20`), `LevelManagerAuthoring`, `UIDocument`, `UIManagerSystem`, `AudioManagerAuthoring`, `VFXManagerAuthoring`, and `EndZoneAuthoring`.
- **Milestone 2 Requirement**: Upgrade `5_JoinClash_Snake_Slice.prefab` to use pure DOTS Snake Follower chain mechanics, including path-history body smoothing, math gate integration, join recruitment, and obstacle collision/severing.

#### B. Existing ECS Components Analysis (`Assets/Scripts/ECS/Components/`)
1. **`SwarmComponent.cs`**:
   - Contains `SwarmComponent : IComponentData` (`TargetCount`, `CurrentCount`, `SwarmMemberPrefab`) and `MathGateComponent : IComponentData` (`Operation`, `Value`, `Radius`).
   - *Observation*: Swarm places units in a 2D cluster around the player root without path history or sequential follow order.
2. **`StackComponent.cs`**:
   - Contains `StackComponent : IComponentData` (`CurrentCount`, `ItemHeightOffset`, `StackItemPrefab`).
   - *Observation*: Stack places items vertically along the Y-axis above the player, which is unsuited for horizontal snake chain movement.
3. **`CollisionComponents.cs`**:
   - Contains `ObstacleComponent : IComponentData` (`CollisionRadius`, `DamageAmount`) and `CollectibleComponent : IComponentData` (`CollisionRadius`, `GoldValue`).
   - *Observation*: Standard sphere-distance collision structs. Needs dedicated snake collision interaction components for follower recruitment, math gate chain scaling, and follower segment severing.
4. **`GridPathfinderComponents.cs` (Reference Pattern from M1)**:
   - Utilizes `IComponentData` structs and `IBufferElementData` (`StackedTileElement`) for dynamic element collections.

#### C. Existing ECS Systems Analysis (`Assets/Scripts/ECS/Systems/`)
1. **`PlayerMovementSystem.cs`**:
   - Runs in `SimulationSystemGroup` when `LevelStateComponent.CurrentState == GameState.Playing`.
   - Moves player forward along Z-axis (`ForwardSpeed`) and swerves horizontally along X-axis (`SwerveDelta * 0.05f`), clamped between `-4f` and `4f`.
2. **`SwarmSystem.cs`**:
   - Performs math gate distance checks (`distSq < radiusSq`), updates `TargetCount`, destroys gate entities, and instantiates swarm entities.
3. **`CollisionSystem.cs`**:
   - Performs distance queries between `PlayerComponent` and `CollectibleComponent` / `ObstacleComponent`.

---

## 2. Logic Chain

From these observations, we derive the structural requirements for the Snake Follower Chain system:

1. **Path-History Trail Smoothing**:
   - Snake follower movement in hyper-casual runners requires trailing body segments to perfectly replicate the path taken by the head entity.
   - Dynamic buffer `SnakeSegmentBuffer` (`IBufferElementData`) attached to the Snake Head entity records sample points `(float3 Position, quaternion Rotation, float AccumulatedDistance)`.
   - Each follower segment samples target poses from this path buffer based on its individual `DistanceOffset = SegmentIndex * SegmentSpacing`.

2. **Entity Chain Management**:
   - `SnakeChainComponent` on the Snake Head tracks chain parameters (`TargetLength`, `CurrentLength`, `SegmentSpacing`, `FollowSpeed`, `RotationSpeed`, `FollowerPrefab`).
   - `SnakeFollowerLinkBuffer` (`IBufferElementData`) on the Head entity maintains an ordered array of `Entity` references to instantiated followers, enabling $O(1)$ segment severing or destruction upon obstacle hit or gate reduction.

3. **Follower Body Segment Data**:
   - `SnakeFollowerComponent` attached to individual follower entities stores `LeaderEntity`, `SegmentIndex`, `DistanceOffset`, `FollowerRadius`, and `IsDying`.

4. **Collision Interactions**:
   - **`SnakeJoinCollectibleComponent`**: Placed on recruitment entities along the track. When touched by the snake head/chain, increases `TargetLength += JoinCount` and triggers presentation event `SnakeJoinEventComponent`.
   - **`SnakeMathGateInteractionComponent`**: Interacts with `MathGateComponent`. Passing through recalculates `TargetLength` (Add, Subtract, Multiply, Divide).
   - **`SnakeObstacleComponent`**: Interacts with Snake Head or Follower segments. Collides with head -> severs `SeverCount` segments or ends game if 0 remain. Collides with follower segment $K$ -> severs segment $K$ and trailing segments, updating `TargetLength` and triggering `SnakeSeverEventComponent`.

---

## 3. Recommended Component Data Structures

Below are the complete, ready-to-implement C# DOTS component definitions for `Assets/Scripts/ECS/Components/SnakeComponents.cs`:

```csharp
using Unity.Entities;
using Unity.Mathematics;

namespace HyperCasualRunner.ECS.Components
{
    /// <summary>
    /// Component attached to the Snake Head / Leader entity managing chain length, spacing, and segment prefabs.
    /// </summary>
    public struct SnakeChainComponent : IComponentData
    {
        public int TargetLength;        // Target number of active follower segments
        public int CurrentLength;       // Current instantiated count of follower segments
        public float SegmentSpacing;    // Distance along path between consecutive segments (e.g., 0.8f)
        public float FollowSpeed;       // Interpolation movement speed for followers (e.g., 15f)
        public float RotationSpeed;     // Interpolation rotation speed for followers (e.g., 12f)
        public Entity FollowerPrefab;   // Entity prefab used to instantiate follower body segments
        public float HeadRadius;        // Collision/pickup radius of the snake head (e.g., 0.8f)
    }

    /// <summary>
    /// Dynamic buffer element attached to Snake Head recording historical position and rotation sample points along the trail.
    /// </summary>
    [InternalBufferCapacity(32)]
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
        public int SegmentIndex;      // 1-based index in chain (1 = 1st segment behind head)
        public float DistanceOffset;  // Target distance offset behind head (SegmentIndex * SegmentSpacing)
        public float FollowerRadius;  // Collision radius of segment (e.g., 0.6f)
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
        public int SeverCount;          // Number of segments lost/severed on impact (e.g., 1, 3, or -1 for full wipe)
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
```

---

## 4. Caveats

1. **Read-Only Scope**: This report is produced under read-only investigation rules. No source code in `Assets/Scripts/` has been created or modified by Explorer 1.
2. **Buffer Capacity Sizing**: `InternalBufferCapacity(32)` is optimal for typical hyper-casual snake lengths (1–30 segments). If snake length exceeds 32 segments, Unity DOTS automatically allocates dynamic heap memory for the buffer without breaking functionality.
3. **Slice Prefab Replacement**: `5_JoinClash_Snake_Slice.prefab` currently retains `StackingAuthoring` from placeholder generation; the Implementer agent must swap `StackingAuthoring` for `SnakeAuthoring` (authoring script for `SnakeChainComponent` and `SnakeSegmentBuffer`).

---

## 5. Conclusion

- The proposed DOTS component architecture (`SnakeChainComponent`, `SnakeSegmentBuffer`, `SnakeFollowerLinkBuffer`, `SnakeFollowerComponent`, `SnakeJoinCollectibleComponent`, `SnakeObstacleComponent`, and event tags) provides a high-performance, Burst-compatible foundation for Milestone 2.
- The path history buffer approach guarantees smooth 60 FPS body movement without physics jitter.
- The dynamic link buffer allows efficient $O(1)$ follower management during math gate operations and obstacle collisions.

---

## 6. Verification Method

To verify these designs independently:
1. **Component Verification**: Check that `SnakeComponents.cs` aligns with `IComponentData` and `IBufferElementData` standards in `Assets/Scripts/ECS/Components/`.
2. **Layout Compliance**: Confirm all source code targets `Assets/Scripts/ECS/Components/` and authorings target `Assets/Scripts/ECS/Authoring/`.
3. **Compiler / Burst Safety**: Ensure all struct fields use Burst-compatible primitive types (`int`, `float`, `bool`, `float3`, `quaternion`, `Entity`).
