using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct MovingWallSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float time = (float)SystemAPI.Time.ElapsedTime;

            foreach (var (transform, wall) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<MovingWallComponent>>())
            {
                float sineWave = math.sin(time * wall.ValueRO.Speed);
                float3 offset = wall.ValueRO.MovementAxis * (sineWave * wall.ValueRO.MoveDistance);
                transform.ValueRW.Position = wall.ValueRO.InitialPosition + offset;
            }
        }
    }
}
