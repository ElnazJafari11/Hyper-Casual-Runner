using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct LaneSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<LevelStateComponent>(out var levelState) || levelState.CurrentState != GameState.Playing)
            {
                return;
            }

            float dt = SystemAPI.Time.DeltaTime;

            foreach (var (lane, transform, input) in SystemAPI.Query<RefRW<LaneComponent>, RefRW<LocalTransform>, RefRO<InputComponent>>().WithAll<PlayerComponent>())
            {
                // Process Swipe Lane Switching (-1 = Left, 0 = Center, 1 = Right)
                float swipe = input.ValueRO.SwipeDeltaX;
                if (swipe < -0.2f)
                {
                    lane.ValueRW.TargetLane = math.max(-1, lane.ValueRO.TargetLane - 1);
                }
                else if (swipe > 0.2f)
                {
                    lane.ValueRW.TargetLane = math.min(1, lane.ValueRO.TargetLane + 1);
                }

                // Interpolate Player X position to Target Lane
                float targetX = lane.ValueRO.TargetLane * lane.ValueRO.LaneWidth;
                float3 pos = transform.ValueRO.Position;
                pos.x = math.lerp(pos.x, targetX, dt * lane.ValueRO.LaneChangeSpeed);
                transform.ValueRW.Position = pos;

                // Check collision against Lane Obstacles
                foreach (var (obstacleTransform, obstacle) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<LaneObstacleComponent>>())
                {
                    float distSq = math.distancesq(pos, obstacleTransform.ValueRO.Position);
                    float radSq = obstacle.ValueRO.CollisionRadius * obstacle.ValueRO.CollisionRadius;

                    if (distSq < radSq)
                    {
                        // Collision on same lane -> Defeat
                        foreach (var stateComp in SystemAPI.Query<RefRW<LevelStateComponent>>())
                        {
                            stateComp.ValueRW.CurrentState = GameState.Defeat;
                        }
                        break;
                    }
                }
            }
        }
    }
}
