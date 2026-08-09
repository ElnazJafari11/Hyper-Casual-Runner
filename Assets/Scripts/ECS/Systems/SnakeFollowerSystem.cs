using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PlayerMovementSystem))]
    [BurstCompile]
    public partial struct SnakeFollowerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SnakeChainComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Halt if level state is not Playing
            if (SystemAPI.HasSingleton<LevelStateComponent>())
            {
                if (SystemAPI.GetSingleton<LevelStateComponent>().CurrentState != GameState.Playing)
                {
                    return;
                }
            }

            float dt = SystemAPI.Time.DeltaTime;
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(false);
            var followerCompLookup = SystemAPI.GetComponentLookup<SnakeFollowerComponent>(false);

            foreach (var (chain, headTransform, historyBuffer, linkBuffer, headEntity) in 
                     SystemAPI.Query<RefRW<SnakeChainComponent>, RefRO<LocalTransform>, DynamicBuffer<SnakeSegmentBuffer>, DynamicBuffer<SnakeFollowerLinkBuffer>>()
                     .WithEntityAccess())
            {
                float3 headPos = headTransform.ValueRO.Position;
                quaternion headRot = headTransform.ValueRO.Rotation;

                // 1. Update position history trail
                if (historyBuffer.Length == 0)
                {
                    historyBuffer.Add(new SnakeSegmentBuffer
                    {
                        Position = headPos,
                        Rotation = headRot,
                        AccumulatedDistance = 0f
                    });
                }
                else
                {
                    float3 prevPos = historyBuffer[historyBuffer.Length - 1].Position;
                    float dist = math.distance(headPos, prevPos);
                    if (dist > 0.01f)
                    {
                        float newAcc = historyBuffer[historyBuffer.Length - 1].AccumulatedDistance + dist;
                        historyBuffer.Add(new SnakeSegmentBuffer
                        {
                            Position = headPos,
                            Rotation = headRot,
                            AccumulatedDistance = newAcc
                        });

                        // Trim history buffer if too long
                        int maxRequiredHistory = math.max(64, chain.ValueRO.TargetLength * 25 + 50);
                        while (historyBuffer.Length > maxRequiredHistory)
                        {
                            historyBuffer.RemoveAt(0);
                        }
                    }
                }

                // 2. Adjust instantiated follower count to match TargetLength
                int targetCount = math.max(0, chain.ValueRO.TargetLength);

                // Spawn new followers if needed
                int currentBufferCount = linkBuffer.Length;
                while (currentBufferCount < targetCount && chain.ValueRO.FollowerPrefab != Entity.Null)
                {
                    int index = currentBufferCount + 1;
                    Entity follower = ecb.Instantiate(chain.ValueRO.FollowerPrefab);

                    float3 spawnPos = headPos - new float3(0, 0, index * chain.ValueRO.SegmentSpacing);
                    ecb.SetComponent(follower, LocalTransform.FromPositionRotation(spawnPos, headRot));
                    ecb.SetComponent(follower, new SnakeFollowerComponent
                    {
                        LeaderEntity = headEntity,
                        SegmentIndex = index,
                        DistanceOffset = index * chain.ValueRO.SegmentSpacing,
                        FollowerRadius = 0.5f,
                        IsRecruited = true,
                        IsDying = false
                    });

                    ecb.AppendToBuffer(headEntity, new SnakeFollowerLinkBuffer { FollowerEntity = follower });
                    currentBufferCount++;
                    chain.ValueRW.CurrentLength = currentBufferCount;
                }

                // Remove excess followers if needed
                while (linkBuffer.Length > targetCount && linkBuffer.Length > 0)
                {
                    int lastIdx = linkBuffer.Length - 1;
                    Entity tailEntity = linkBuffer[lastIdx].FollowerEntity;
                    linkBuffer.RemoveAt(lastIdx);
                    if (tailEntity.Index >= 0 && transformLookup.HasComponent(tailEntity))
                    {
                        ecb.DestroyEntity(tailEntity);
                    }
                    chain.ValueRW.CurrentLength = linkBuffer.Length;
                }

                // 3. Position and rotate follower body segments along history trail
                if (historyBuffer.Length > 0 && linkBuffer.Length > 0)
                {
                    float headAccDist = historyBuffer[historyBuffer.Length - 1].AccumulatedDistance;

                    for (int i = 0; i < linkBuffer.Length; i++)
                    {
                        Entity followerEntity = linkBuffer[i].FollowerEntity;
                        if (followerEntity.Index < 0 || !transformLookup.HasComponent(followerEntity)) continue;

                        float targetDist = (i + 1) * chain.ValueRO.SegmentSpacing;
                        float targetAccDist = headAccDist - targetDist;

                        // Sample pose from history buffer
                        float3 targetPos = historyBuffer[0].Position;
                        quaternion targetRot = historyBuffer[0].Rotation;

                        for (int k = historyBuffer.Length - 1; k >= 1; k--)
                        {
                            float accNewer = historyBuffer[k].AccumulatedDistance;
                            float accOlder = historyBuffer[k - 1].AccumulatedDistance;

                            if (targetAccDist <= accNewer && targetAccDist >= accOlder)
                            {
                                float diff = accNewer - accOlder;
                                float t = (diff > 0.0001f) ? math.clamp((targetAccDist - accOlder) / diff, 0f, 1f) : 0f;
                                targetPos = math.lerp(historyBuffer[k - 1].Position, historyBuffer[k].Position, t);
                                targetRot = math.slerp(historyBuffer[k - 1].Rotation, historyBuffer[k].Rotation, t);
                                break;
                            }
                        }

                        // Smoothly interpolate follower position & rotation
                        var followerTransform = transformLookup[followerEntity];
                        followerTransform.Position = math.lerp(followerTransform.Position, targetPos, math.clamp(dt * chain.ValueRO.FollowSpeed, 0f, 1f));
                        followerTransform.Rotation = math.slerp(followerTransform.Rotation, targetRot, math.clamp(dt * chain.ValueRO.RotationSpeed, 0f, 1f));
                        transformLookup[followerEntity] = followerTransform;
                        
                        // Update follower index
                        if (followerCompLookup.HasComponent(followerEntity))
                        {
                            var folComp = followerCompLookup[followerEntity];
                            folComp.SegmentIndex = i + 1;
                            followerCompLookup[followerEntity] = folComp;
                        }
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
