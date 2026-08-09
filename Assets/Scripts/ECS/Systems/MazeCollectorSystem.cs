using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    /// <summary>
    /// Pure DOTS ECS system for detecting player pickup of maze grid tiles.
    /// Runs in SimulationSystemGroup after PlayerMovementSystem.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PlayerMovementSystem))]
    public partial struct MazeCollectorSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerComponent>();
            state.RequireForUpdate<MazeCollectorComponent>();
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

            // Query active grid tile entities
            foreach (var (transform, tile, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<GridTileComponent>>().WithEntityAccess())
            {
                if (tile.ValueRO.IsCollected || !tile.ValueRO.HasTileItem)
                    continue;

                float distSq = math.distancesq(playerPos.xz, transform.ValueRO.Position.xz);
                float pickupRadius = tile.ValueRO.PickupRadius;

                if (distSq <= pickupRadius * pickupRadius)
                {
                    // Mark collected immediately in memory to prevent double collection
                    tile.ValueRW.IsCollected = true;

                    // Increment tile count on collector
                    foreach (var collector in SystemAPI.Query<RefRW<MazeCollectorComponent>>())
                    {
                        collector.ValueRW.StackedTiles += tile.ValueRO.TileValue;
                        collector.ValueRW.TotalCollected += tile.ValueRO.TileValue;
                    }

                    // Create audio presentation event entity
                    var soundEntity = ecb.CreateEntity();
                    ecb.AddComponent(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });

                    // Destroy collected tile entity
                    ecb.DestroyEntity(entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
