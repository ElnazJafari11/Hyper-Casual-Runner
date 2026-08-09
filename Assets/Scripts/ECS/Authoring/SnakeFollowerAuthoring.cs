using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    /// <summary>
    /// Authoring component for Snake Follower segment entities and recruit pickups.
    /// </summary>
    public class SnakeFollowerAuthoring : MonoBehaviour
    {
        public int SegmentIndex = 0;
        public float FollowerRadius = 0.5f;
        public bool IsRecruited = false;

        class Baker : Baker<SnakeFollowerAuthoring>
        {
            public override void Bake(SnakeFollowerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new SnakeFollowerComponent
                {
                    LeaderEntity = Entity.Null,
                    SegmentIndex = authoring.SegmentIndex,
                    DistanceOffset = authoring.SegmentIndex * 0.8f,
                    FollowerRadius = authoring.FollowerRadius,
                    IsRecruited = authoring.IsRecruited,
                    IsDying = false
                });
            }
        }
    }

    /// <summary>
    /// Authoring component for standalone recruitment collectibles on the track.
    /// </summary>
    public class SnakeJoinCollectibleAuthoring : MonoBehaviour
    {
        public int JoinCount = 1;
        public float CollisionRadius = 0.8f;

        class Baker : Baker<SnakeJoinCollectibleAuthoring>
        {
            public override void Bake(SnakeJoinCollectibleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new SnakeJoinCollectibleComponent
                {
                    JoinCount = authoring.JoinCount,
                    CollisionRadius = authoring.CollisionRadius,
                    IsCollected = false
                });
            }
        }
    }

    /// <summary>
    /// Authoring component for track obstacles interacting with snake chain segments.
    /// </summary>
    public class SnakeObstacleAuthoring : MonoBehaviour
    {
        public int SeverCount = 1;
        public float CollisionRadius = 1.2f;
        public bool DestroyOnImpact = true;

        class Baker : Baker<SnakeObstacleAuthoring>
        {
            public override void Bake(SnakeObstacleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new SnakeObstacleComponent
                {
                    SeverCount = authoring.SeverCount,
                    CollisionRadius = authoring.CollisionRadius,
                    DestroyOnImpact = authoring.DestroyOnImpact
                });
            }
        }
    }
}
