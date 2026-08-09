using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct WinConditionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // Only run if we are currently Playing
            bool isPlaying = false;
            foreach (var levelState in SystemAPI.Query<RefRO<LevelStateComponent>>())
            {
                if (levelState.ValueRO.CurrentState == GameState.Playing) isPlaying = true;
            }
            if (!isPlaying) return;

            bool hitEndZone = false;

            // Check distance between player and EndZone
            foreach (var playerTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerComponent>())
            {
                foreach (var (endZoneTransform, endZone) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<EndZoneComponent>>())
                {
                    float distSq = math.distancesq(playerTransform.ValueRO.Position, endZoneTransform.ValueRO.Position);
                    if (distSq < endZone.ValueRO.TriggerRadius * endZone.ValueRO.TriggerRadius)
                    {
                        hitEndZone = true;
                        break;
                    }
                }
                if (hitEndZone) break;
            }

            // Update State
            if (hitEndZone)
            {
                foreach (var levelState in SystemAPI.Query<RefRW<LevelStateComponent>>())
                {
                    levelState.ValueRW.CurrentState = GameState.Victory;
                }
            }
        }
    }
}
