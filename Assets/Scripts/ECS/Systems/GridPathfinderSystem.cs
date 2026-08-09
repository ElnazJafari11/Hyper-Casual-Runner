using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    /// <summary>
    /// Pure DOTS ECS system that calculates grid cell coordinates and handles automatic gap paving.
    /// Runs in SimulationSystemGroup after MazeCollectorSystem.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(MazeCollectorSystem))]
    public partial struct GridPathfinderSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerComponent>();
            state.RequireForUpdate<GridPathfinderComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Halt if game state is not Playing
            if (SystemAPI.HasSingleton<LevelStateComponent>())
            {
                if (SystemAPI.GetSingleton<LevelStateComponent>().CurrentState != GameState.Playing)
                {
                    return;
                }
            }

            float3 playerPos = float3.zero;
            bool playerFound = false;

            foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerComponent>())
            {
                playerPos = transform.ValueRO.Position;
                playerFound = true;
                break;
            }

            if (!playerFound) return;

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // Check if player is currently within any GapZone AABB
            bool isOverGap = false;
            foreach (var gap in SystemAPI.Query<RefRO<GapZoneComponent>>())
            {
                float3 minBounds = gap.ValueRO.MinBounds;
                float3 maxBounds = gap.ValueRO.MaxBounds;

                if (playerPos.x >= minBounds.x && playerPos.x <= maxBounds.x &&
                    playerPos.z >= minBounds.z && playerPos.z <= maxBounds.z)
                {
                    isOverGap = true;
                    break;
                }
            }

            foreach (var (pathfinder, collector) in SystemAPI.Query<RefRW<GridPathfinderComponent>, RefRW<MazeCollectorComponent>>())
            {
                // Update cell coordinates
                float step = math.select(pathfinder.ValueRO.StepDistance, 1.0f, pathfinder.ValueRO.StepDistance <= 0.001f);
                float3 relPos = playerPos - pathfinder.ValueRO.GridOrigin;
                int2 cell = new int2(
                    (int)math.floor((relPos.x + step * 0.5f) / step),
                    (int)math.floor((relPos.z + step * 0.5f) / step)
                );
                pathfinder.ValueRW.CurrentCell = cell;

                if (isOverGap)
                {
                    pathfinder.ValueRW.IsCrossingGap = true;

                    // Initialize last tile position if reset
                    if (pathfinder.ValueRO.LastTilePosition.x < -9000f)
                    {
                        pathfinder.ValueRW.LastTilePosition = playerPos;
                    }

                    float dist = math.distance(playerPos.xz, pathfinder.ValueRO.LastTilePosition.xz);
                    if (dist >= step)
                    {
                        if (collector.ValueRO.StackedTiles > 0)
                        {
                            collector.ValueRW.StackedTiles--;
                            pathfinder.ValueRW.LastTilePosition = playerPos;

                            // Instantiate paved path tile if prefab is bound
                            if (pathfinder.ValueRO.PathTilePrefab != Entity.Null)
                            {
                                Entity pathTile = ecb.Instantiate(pathfinder.ValueRO.PathTilePrefab);
                                ecb.SetComponent(pathTile, LocalTransform.FromPosition(new float3(playerPos.x, pathfinder.ValueRO.PathYHeight, playerPos.z)));
                            }

                            // Trigger sound tag event
                            var soundEntity = ecb.CreateEntity();
                            ecb.AddComponent(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                        }
                        else
                        {
                            // Out of tiles while crossing gap -> Defeat
                            if (SystemAPI.HasSingleton<LevelStateComponent>())
                            {
                                var levelState = SystemAPI.GetSingleton<LevelStateComponent>();
                                levelState.CurrentState = GameState.Defeat;
                                SystemAPI.SetSingleton(levelState);
                            }
                        }
                    }
                }
                else
                {
                    pathfinder.ValueRW.IsCrossingGap = false;
                    pathfinder.ValueRW.LastTilePosition = new float3(-9999f, -9999f, -9999f);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
