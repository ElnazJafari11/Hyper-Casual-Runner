using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct TransformationSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // 1. Process Height/Width Gates (re-using MathGate logic for MVP)
            foreach (var (transform, transComp) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<TransformationComponent>>())
            {
                foreach (var (gateTransform, gate, gateEntity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<MathGateComponent>>().WithEntityAccess())
                {
                    float distSq = math.distancesq(transform.ValueRO.Position, gateTransform.ValueRO.Position);
                    if (distSq < gate.ValueRO.Radius * gate.ValueRO.Radius)
                    {
                        // MVP Hack: Positive values add Height, Negative add Width
                        if (gate.ValueRO.Value > 0) transComp.ValueRW.TargetHeight += gate.ValueRO.Value * 0.1f;
                        else transComp.ValueRW.TargetWidth += math.abs(gate.ValueRO.Value) * 0.1f;
                        
                        ecb.DestroyEntity(gateEntity);
                    }
                }
            }

            // 2. Smooth Lerp visuals
            foreach (var (transform, transComp) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<TransformationComponent>>())
            {
                float3 targetScale = new float3(transComp.ValueRO.TargetWidth, transComp.ValueRO.TargetHeight, transComp.ValueRO.TargetWidth);
                
                // Currently LocalTransform uniformly scales via a single float in Unity DOTS 1.0 out of the box,
                // but PostTransformMatrix allows non-uniform. For MVP we'll just uniformly scale based on average.
                float avgScale = (transComp.ValueRO.TargetWidth + transComp.ValueRO.TargetHeight) / 2f;
                transform.ValueRW.Scale = math.lerp(transform.ValueRO.Scale, avgScale, SystemAPI.Time.DeltaTime * 5f);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
