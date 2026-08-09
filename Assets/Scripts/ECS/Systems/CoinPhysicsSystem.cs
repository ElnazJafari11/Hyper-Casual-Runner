using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    /// <summary>
    /// Pure DOTS ECS system driving 3-phase physics (Airborne parabolic -> Ground bounce/damping -> Magnetic collection).
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct CoinPhysicsSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<LevelStateComponent>())
            {
                if (SystemAPI.GetSingleton<LevelStateComponent>().CurrentState != GameState.Playing)
                {
                    return;
                }
            }

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            float dt = SystemAPI.Time.DeltaTime;
            if (dt <= 0f)
            {
                dt = 0.0166667f;
            }

            // 1. Process CoinSplitEventComponent entities to instantiate burst coins
            foreach (var (splitEvent, eventEntity) in SystemAPI.Query<RefRO<CoinSplitEventComponent>>().WithEntityAccess())
            {
                Entity prefab = splitEvent.ValueRO.CoinPrefab;
                if (prefab != Entity.Null && state.EntityManager.Exists(prefab))
                {
                    int count = splitEvent.ValueRO.QuantityToSpawn;
                    float spreadDeg = splitEvent.ValueRO.SpreadAngle;
                    float impulse = splitEvent.ValueRO.ImpulseSpeed;
                    float3 spawnPos = splitEvent.ValueRO.SpawnPosition;

                    for (int i = 0; i < count; i++)
                    {
                        Entity newCoin = ecb.Instantiate(prefab);
                        
                        float t = (count > 1) ? ((float)i / (count - 1)) : 0.5f;
                        float angleDeg = math.lerp(-spreadDeg * 0.5f, spreadDeg * 0.5f, t);
                        float rad = math.radians(angleDeg);

                        float vx = impulse * math.sin(rad);
                        float vy = impulse * math.cos(rad) * 0.5f + 3.0f;
                        float vz = impulse * math.cos(rad);

                        ecb.SetComponent(newCoin, LocalTransform.FromPosition(spawnPos));
                        ecb.AddComponent(newCoin, new CoinSplitPhysicsComponent
                        {
                            CurrentVelocity = new float3(vx, vy, vz),
                            SpreadAngle = spreadDeg,
                            ImpulseSpeed = impulse,
                            StackHeightOffset = 0.5f,
                            Lifetime = 3.0f,
                            MaxLifetime = 3.0f,
                            GravityMultiplier = 2.5f,
                            CoinCount = 1,
                            IsGrounded = false,
                            IsCollectible = false,
                            ReservedPadding = 0
                        });
                        ecb.AddComponent<CoinTag>(newCoin);
                    }
                }
                ecb.DestroyEntity(eventEntity);
            }

            // Obtain Player position for magnetic collection
            float3 playerPos = float3.zero;
            bool playerFound = false;

            foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>().WithAny<PlayerCoinRunnerComponent, PlayerComponent>())
            {
                playerPos = transform.ValueRO.Position;
                playerFound = true;
                break;
            }

            // 2. Simulate 3-phase physics for active coin entities
            foreach (var (coinPhys, transform, coinEntity) in SystemAPI.Query<RefRW<CoinSplitPhysicsComponent>, RefRW<LocalTransform>>().WithAll<CoinTag>().WithEntityAccess())
            {
                if (!coinPhys.ValueRO.IsGrounded)
                {
                    // Phase 1: Airborne Parabolic Trajectory
                    float3 vel = coinPhys.ValueRO.CurrentVelocity;
                    vel.y += (-25.0f * coinPhys.ValueRO.GravityMultiplier) * dt;
                    vel *= math.max(0f, 1.0f - 1.5f * dt);

                    float3 newPos = transform.ValueRO.Position + vel * dt;

                    // Ground collision check at y = 0.2f
                    if (newPos.y <= 0.2f)
                    {
                        newPos.y = 0.2f;
                        vel.y = -vel.y * 0.4f; // Bounce restitution

                        if (math.abs(vel.y) < 0.5f)
                        {
                            vel.y = 0f;
                            coinPhys.ValueRW.IsGrounded = true;
                            coinPhys.ValueRW.IsCollectible = true;
                        }
                    }

                    coinPhys.ValueRW.CurrentVelocity = vel;
                    transform.ValueRW.Position = newPos;
                }
                else
                {
                    // Phase 2 & 3: Grounded & Magnetic Attraction
                    coinPhys.ValueRW.Lifetime -= dt;
                    if (coinPhys.ValueRO.Lifetime <= 0f)
                    {
                        ecb.DestroyEntity(coinEntity);
                        continue;
                    }

                    if (playerFound)
                    {
                        float3 toPlayer = playerPos - transform.ValueRO.Position;
                        float dist = math.length(toPlayer);

                        if (dist < 0.8f)
                        {
                            // Collection reached!
                            var soundEntity = ecb.CreateEntity();
                            ecb.AddComponent(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });

                            if (SystemAPI.HasSingleton<CurrentRunStats>())
                            {
                                var runStats = SystemAPI.GetSingleton<CurrentRunStats>();
                                runStats.CurrentGold += coinPhys.ValueRO.CoinCount;
                                SystemAPI.SetSingleton(runStats);
                            }

                            ecb.DestroyEntity(coinEntity);
                        }
                        else
                        {
                            float3 dir = math.normalize(toPlayer);
                            float magSpeed = 18.0f + 10.0f / math.max(0.1f, dist);
                            transform.ValueRW.Position += dir * magSpeed * dt;
                        }
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
