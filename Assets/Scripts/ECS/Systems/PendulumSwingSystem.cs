using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct PendulumSwingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float time = (float)SystemAPI.Time.ElapsedTime;

            foreach (var (transform, pendulum) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PendulumSwingComponent>>())
            {
                float angle = math.sin((time * pendulum.ValueRO.Speed) + pendulum.ValueRO.PhaseOffset) * math.radians(pendulum.ValueRO.MaxAngleDegrees);
                float3 normAxis = math.lengthsq(pendulum.ValueRO.SwingAxis) > 0.0001f 
                    ? math.normalize(pendulum.ValueRO.SwingAxis) 
                    : new float3(0, 0, 1);
                
                quaternion deltaRot = quaternion.AxisAngle(normAxis, angle);
                transform.ValueRW.Rotation = math.mul(pendulum.ValueRO.InitialRotation, deltaRot);
            }
        }
    }
}
