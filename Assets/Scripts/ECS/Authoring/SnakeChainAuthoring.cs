using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    /// <summary>
    /// Authoring component for the Snake Head entity (PlayerEntity).
    /// Bakes SnakeChainComponent and attaches dynamic history and follower link buffers.
    /// </summary>
    public class SnakeChainAuthoring : MonoBehaviour
    {
        [Header("Prefab References")]
        public GameObject FollowerPrefab;

        [Header("Chain Settings")]
        public int StartingFollowerCount = 3;
        public float SegmentSpacing = 0.8f;
        public float FollowSpeed = 15.0f;
        public float RotationSpeed = 12.0f;
        public float HeadRadius = 0.8f;

        class Baker : Baker<SnakeChainAuthoring>
        {
            public override void Bake(SnakeChainAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                Entity followerPrefabEntity = Entity.Null;
                if (authoring.FollowerPrefab != null)
                {
                    followerPrefabEntity = GetEntity(authoring.FollowerPrefab, TransformUsageFlags.Dynamic);
                }

                // Bake main chain component onto head entity
                AddComponent(entity, new SnakeChainComponent
                {
                    TargetLength = authoring.StartingFollowerCount,
                    CurrentLength = 0,
                    SegmentSpacing = authoring.SegmentSpacing,
                    FollowSpeed = authoring.FollowSpeed,
                    RotationSpeed = authoring.RotationSpeed,
                    FollowerPrefab = followerPrefabEntity,
                    HeadRadius = authoring.HeadRadius
                });

                // Attach dynamic buffer for position/rotation history
                AddBuffer<SnakeSegmentBuffer>(entity);

                // Attach dynamic buffer for active follower entity handles
                AddBuffer<SnakeFollowerLinkBuffer>(entity);
            }
        }
    }
}
