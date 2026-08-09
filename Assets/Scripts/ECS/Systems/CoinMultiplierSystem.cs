using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    /// <summary>
    /// Pure DOTS ECS system for detecting player interaction with coin multiplier gates.
    /// Updates coin balance, marks gate triggered, emits presentation events and split requests.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(CoinPhysicsSystem))]
    public partial struct CoinMultiplierSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerCoinRunnerComponent>();
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

            foreach (var (runner, playerTransform, playerEntity) in SystemAPI.Query<RefRW<PlayerCoinRunnerComponent>, RefRO<LocalTransform>>().WithEntityAccess())
            {
                float3 pPos = playerTransform.ValueRO.Position;

                foreach (var (gate, gateTransform, gateEntity) in SystemAPI.Query<RefRW<CoinMultiplierGateComponent>, RefRO<LocalTransform>>().WithAll<CoinMultiplierGateTag>().WithEntityAccess())
                {
                    if (gate.ValueRO.IsTriggered)
                        continue;

                    float3 gPos = gateTransform.ValueRO.Position;
                    float halfWidth = gate.ValueRO.GateWidth * 0.5f;
                    float halfDepth = gate.ValueRO.TriggerDepth * 0.5f;

                    // Transverse X and longitudinal Z bounding overlap check
                    if (math.abs(pPos.z - gPos.z) <= halfDepth && math.abs(pPos.x - gPos.x) <= halfWidth)
                    {
                        gate.ValueRW.IsTriggered = true;
                        int coinsBefore = runner.ValueRO.CurrentCoinCount;
                        int coinsAfter = coinsBefore;

                        if (gate.ValueRO.GateType == MultiplierType.Additive)
                        {
                            double calculated = (double)coinsBefore + gate.ValueRO.Value;
                            coinsAfter = (int)math.clamp(calculated, (double)gate.ValueRO.MinimumOutput, (double)int.MaxValue);
                        }
                        else if (gate.ValueRO.GateType == MultiplierType.Multiplicative)
                        {
                            double calculated = (double)coinsBefore * gate.ValueRO.Value;
                            coinsAfter = (int)math.clamp(calculated, (double)gate.ValueRO.MinimumOutput, (double)int.MaxValue);
                        }

                        int delta = coinsAfter - coinsBefore;
                        runner.ValueRW.CurrentCoinCount = coinsAfter;

                        // Emit CoinMultiplierEventComponent
                        var eventEntity = ecb.CreateEntity();
                        ecb.AddComponent(eventEntity, new CoinMultiplierEventComponent
                        {
                            GateEntity = gateEntity,
                            InstigatorEntity = playerEntity,
                            GateType = gate.ValueRO.GateType,
                            Padding0 = 0,
                            Padding1 = 0,
                            MultiplierValue = gate.ValueRO.Value,
                            CoinsBefore = coinsBefore,
                            CoinsAfter = coinsAfter
                        });

                        // Emit Audio Event
                        var soundEntity = ecb.CreateEntity();
                        ecb.AddComponent(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });

                        // Emit CoinSplitEventComponent if coins gained
                        if (delta > 0 && runner.ValueRO.CoinVisualPrefab != Entity.Null)
                        {
                            var splitEntity = ecb.CreateEntity();
                            ecb.AddComponent(splitEntity, new CoinSplitEventComponent
                            {
                                SpawnPosition = gPos + new float3(0, 0.5f, 0),
                                SpreadAngle = 60.0f,
                                ImpulseSpeed = 8.0f,
                                QuantityToSpawn = math.min(delta, 20),
                                CoinPrefab = runner.ValueRO.CoinVisualPrefab
                            });
                        }

                        // Emit DestroyEventComponent on gate entity to trigger visual removal
                        ecb.AddComponent<DestroyEventComponent>(gateEntity);
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
