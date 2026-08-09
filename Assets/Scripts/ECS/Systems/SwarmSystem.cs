using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct SwarmSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // 1. Process Math Gates
            foreach (var (playerTransform, swarm) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<SwarmComponent>>())
            {
                foreach (var (gateTransform, gate, gateEntity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<MathGateComponent>>().WithEntityAccess())
                {
                    float distSq = math.distancesq(playerTransform.ValueRO.Position, gateTransform.ValueRO.Position);
                    if (distSq < gate.ValueRO.Radius * gate.ValueRO.Radius)
                    {
                        int c = swarm.ValueRO.TargetCount;
                        if (gate.ValueRO.Operation == GateOperation.Add) c += gate.ValueRO.Value;
                        else if (gate.ValueRO.Operation == GateOperation.Subtract) c -= gate.ValueRO.Value;
                        else if (gate.ValueRO.Operation == GateOperation.Multiply) c *= gate.ValueRO.Value;
                        else if (gate.ValueRO.Operation == GateOperation.Divide) c /= gate.ValueRO.Value;

                        swarm.ValueRW.TargetCount = math.max(1, c);
                        ecb.DestroyEntity(gateEntity);
                    }
                }
            }

            // 2. Resolve visual crowd (instantiate/destroy)
            foreach (var (swarm, playerEntity) in SystemAPI.Query<RefRW<SwarmComponent>>().WithEntityAccess())
            {
                if (swarm.ValueRO.CurrentCount < swarm.ValueRO.TargetCount)
                {
                    int toSpawn = swarm.ValueRO.TargetCount - swarm.ValueRO.CurrentCount;
                    var rand = new Unity.Mathematics.Random((uint)SystemAPI.Time.ElapsedTime * 1000 + 1);
                    
                    for (int i = 0; i < toSpawn; i++)
                    {
                        Entity e = ecb.Instantiate(swarm.ValueRO.SwarmMemberPrefab);
                        float3 offset = new float3(rand.NextFloat(-2f, 2f), 0, rand.NextFloat(-2f, 2f));
                        ecb.SetComponent(e, LocalTransform.FromPosition(offset));
                    }
                    swarm.ValueRW.CurrentCount = swarm.ValueRO.TargetCount;
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
