using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SnakeFollowerSystem))]
    [BurstCompile]
    public partial struct SnakeCollisionSystem : ISystem
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

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

            foreach (var (chain, headTransform, linkBuffer, headEntity) in 
                     SystemAPI.Query<RefRW<SnakeChainComponent>, RefRO<LocalTransform>, DynamicBuffer<SnakeFollowerLinkBuffer>>()
                     .WithEntityAccess())
            {
                float3 headPos = headTransform.ValueRO.Position;
                float headRadius = chain.ValueRO.HeadRadius > 0.05f ? chain.ValueRO.HeadRadius : 0.8f;

                // 1. Math Gate Collisions
                foreach (var (gateTransform, gate, gateEntity) in 
                         SystemAPI.Query<RefRO<LocalTransform>, RefRO<MathGateComponent>>()
                         .WithEntityAccess())
                {
                    float distSq = math.distancesq(headPos, gateTransform.ValueRO.Position);
                    float gateRad = gate.ValueRO.Radius > 0.05f ? gate.ValueRO.Radius : 1.5f;

                    if (distSq < (headRadius + gateRad) * (headRadius + gateRad))
                    {
                        int currentLen = chain.ValueRO.TargetLength;
                        int newLen = currentLen;

                        switch (gate.ValueRO.Operation)
                        {
                            case GateOperation.Add:
                                newLen += gate.ValueRO.Value;
                                break;
                            case GateOperation.Subtract:
                                newLen -= gate.ValueRO.Value;
                                break;
                            case GateOperation.Multiply:
                                newLen *= gate.ValueRO.Value;
                                break;
                            case GateOperation.Divide:
                                newLen = gate.ValueRO.Value != 0 ? currentLen / gate.ValueRO.Value : currentLen;
                                break;
                        }

                        chain.ValueRW.TargetLength = math.max(0, newLen);
                        ecb.DestroyEntity(gateEntity);

                        // Trigger pickup sound
                        Entity soundTag = ecb.CreateEntity();
                        ecb.AddComponent(soundTag, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                    }
                }

                // 2. Standalone Recruitment Collectibles
                foreach (var (recruitTransform, recruit, recruitEntity) in 
                         SystemAPI.Query<RefRO<LocalTransform>, RefRW<SnakeJoinCollectibleComponent>>()
                         .WithEntityAccess())
                {
                    if (recruit.ValueRO.IsCollected) continue;

                    float distSq = math.distancesq(headPos, recruitTransform.ValueRO.Position);
                    float radSum = headRadius + recruit.ValueRO.CollisionRadius;

                    if (distSq < radSum * radSum)
                    {
                        recruit.ValueRW.IsCollected = true;
                        int added = math.max(1, recruit.ValueRO.JoinCount);
                        chain.ValueRW.TargetLength += added;

                        ecb.DestroyEntity(recruitEntity);

                        // Audio & Presentation events
                        Entity soundTag = ecb.CreateEntity();
                        ecb.AddComponent(soundTag, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });

                        Entity joinEvent = ecb.CreateEntity();
                        ecb.AddComponent(joinEvent, new SnakeJoinEventComponent
                        {
                            Position = recruitTransform.ValueRO.Position,
                            JoinedAmount = added
                        });
                    }
                }

                // 3. Standard Collectible Gold Pickups
                foreach (var (colTransform, collectible, colEntity) in 
                         SystemAPI.Query<RefRO<LocalTransform>, RefRO<CollectibleComponent>>()
                         .WithEntityAccess())
                {
                    float distSq = math.distancesq(headPos, colTransform.ValueRO.Position);
                    float radSum = headRadius + collectible.ValueRO.CollisionRadius;

                    if (distSq < radSum * radSum)
                    {
                        foreach (var runStats in SystemAPI.Query<RefRW<CurrentRunStats>>())
                        {
                            runStats.ValueRW.CurrentGold += collectible.ValueRO.GoldValue;
                        }

                        ecb.DestroyEntity(colEntity);

                        Entity soundTag = ecb.CreateEntity();
                        ecb.AddComponent(soundTag, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                    }
                }

                // 4. Snake Obstacles (SnakeObstacleComponent)
                foreach (var (obsTransform, obstacle, obsEntity) in 
                         SystemAPI.Query<RefRO<LocalTransform>, RefRO<SnakeObstacleComponent>>()
                         .WithEntityAccess())
                {
                    float obsRadius = obstacle.ValueRO.CollisionRadius > 0.05f ? obstacle.ValueRO.CollisionRadius : 1.0f;
                    float3 obsPos = obsTransform.ValueRO.Position;

                    // Check head collision
                    bool hit = false;
                    float3 impactPos = headPos;

                    if (math.distancesq(headPos, obsPos) < (headRadius + obsRadius) * (headRadius + obsRadius))
                    {
                        hit = true;
                        impactPos = headPos;
                        int sever = obstacle.ValueRO.SeverCount;
                        if (sever <= 0)
                        {
                            chain.ValueRW.TargetLength = 0;
                        }
                        else
                        {
                            chain.ValueRW.TargetLength = math.max(0, chain.ValueRO.TargetLength - sever);
                        }
                    }
                    else
                    {
                        // Check follower segment collisions
                        for (int i = 0; i < linkBuffer.Length; i++)
                        {
                            Entity folEntity = linkBuffer[i].FollowerEntity;
                            if (folEntity.Index < 0 || !transformLookup.HasComponent(folEntity)) continue;

                            float3 folPos = transformLookup[folEntity].Position;
                            float folRad = 0.5f;

                            if (math.distancesq(folPos, obsPos) < (folRad + obsRadius) * (folRad + obsRadius))
                            {
                                hit = true;
                                impactPos = folPos;
                                int severedCount = linkBuffer.Length - i;
                                chain.ValueRW.TargetLength = math.max(0, chain.ValueRO.TargetLength - severedCount);
                                break;
                            }
                        }
                    }

                    if (hit)
                    {
                        if (obstacle.ValueRO.DestroyOnImpact)
                        {
                            ecb.DestroyEntity(obsEntity);
                        }

                        // VFX and Audio
                        Entity vfxTag = ecb.CreateEntity();
                        ecb.AddComponent(vfxTag, LocalTransform.FromPosition(impactPos));
                        ecb.AddComponent<DestroyEventComponent>(vfxTag);

                        Entity soundTag = ecb.CreateEntity();
                        ecb.AddComponent(soundTag, new PlaySoundEventComponent { SoundToPlay = SoundType.Explosion });

                        Entity severEvent = ecb.CreateEntity();
                        ecb.AddComponent(severEvent, new SnakeSeverEventComponent
                        {
                            Position = impactPos,
                            SeveredAmount = 1
                        });
                    }
                }

                // 5. Standard Obstacles (ObstacleComponent)
                foreach (var (obsTransform, obstacle, obsEntity) in 
                         SystemAPI.Query<RefRO<LocalTransform>, RefRO<ObstacleComponent>>()
                         .WithEntityAccess())
                {
                    float obsRadius = obstacle.ValueRO.CollisionRadius > 0.05f ? obstacle.ValueRO.CollisionRadius : 1.0f;
                    float3 obsPos = obsTransform.ValueRO.Position;

                    bool hit = false;
                    float3 impactPos = headPos;

                    if (math.distancesq(headPos, obsPos) < (headRadius + obsRadius) * (headRadius + obsRadius))
                    {
                        hit = true;
                        impactPos = headPos;
                        chain.ValueRW.TargetLength = math.max(0, chain.ValueRO.TargetLength - 1);
                    }
                    else
                    {
                        for (int i = 0; i < linkBuffer.Length; i++)
                        {
                            Entity folEntity = linkBuffer[i].FollowerEntity;
                            if (folEntity.Index < 0 || !transformLookup.HasComponent(folEntity)) continue;

                            float3 folPos = transformLookup[folEntity].Position;
                            if (math.distancesq(folPos, obsPos) < (0.5f + obsRadius) * (0.5f + obsRadius))
                            {
                                hit = true;
                                impactPos = folPos;
                                int severedCount = linkBuffer.Length - i;
                                chain.ValueRW.TargetLength = math.max(0, chain.ValueRO.TargetLength - severedCount);
                                break;
                            }
                        }
                    }

                    if (hit)
                    {
                        ecb.DestroyEntity(obsEntity);

                        Entity vfxTag = ecb.CreateEntity();
                        ecb.AddComponent(vfxTag, LocalTransform.FromPosition(impactPos));
                        ecb.AddComponent<DestroyEventComponent>(vfxTag);

                        Entity soundTag = ecb.CreateEntity();
                        ecb.AddComponent(soundTag, new PlaySoundEventComponent { SoundToPlay = SoundType.Explosion });
                    }
                }

                // 6. Check Defeat Condition
                if (chain.ValueRO.TargetLength <= 0 && chain.ValueRO.CurrentLength <= 0)
                {
                    if (SystemAPI.HasSingleton<LevelStateComponent>())
                    {
                        var levelState = SystemAPI.GetSingletonRW<LevelStateComponent>();
                        levelState.ValueRW.CurrentState = GameState.Defeat;
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
