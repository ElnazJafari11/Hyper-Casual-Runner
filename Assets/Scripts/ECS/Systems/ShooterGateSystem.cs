using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct ShooterGateSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<LevelStateComponent>(out var levelState) || levelState.CurrentState != GameState.Playing)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

            foreach (var (shooter, playerTransform) in SystemAPI.Query<RefRW<ShooterComponent>, RefRO<LocalTransform>>().WithAll<PlayerComponent>())
            {
                foreach (var (gateTransform, gate, gateEntity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<MathGateComponent>>().WithEntityAccess())
                {
                    float distSq = math.distancesq(playerTransform.ValueRO.Position, gateTransform.ValueRO.Position);
                    if (distSq < gate.ValueRO.Radius * gate.ValueRO.Radius)
                    {
                        // Apply upgrade based on operation
                        switch (gate.ValueRO.Operation)
                        {
                            case GateOperation.Add:
                                shooter.ValueRW.SpreadCount += gate.ValueRO.Value;
                                break;
                            case GateOperation.Multiply:
                                shooter.ValueRW.SpreadCount *= gate.ValueRO.Value;
                                break;
                            case GateOperation.Subtract:
                                // Use Subtract/Divide for FireRate reduction (faster shooting)
                                shooter.ValueRW.FireRate = math.max(0.05f, shooter.ValueRO.FireRate - (gate.ValueRO.Value * 0.01f));
                                break;
                            case GateOperation.Divide:
                                shooter.ValueRW.FireRate = math.max(0.05f, shooter.ValueRO.FireRate / gate.ValueRO.Value);
                                break;
                        }

                        // Destroy gate to prevent multiple triggers
                        ecb.AddComponent<DestroyEventComponent>(gateEntity);
                        
                        var audioEntity = ecb.CreateEntity();
                        ecb.AddComponent(audioEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                        
                        break; // One gate at a time
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
