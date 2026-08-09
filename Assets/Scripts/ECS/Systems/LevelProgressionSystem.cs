using Unity.Entities;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct LevelProgressionSystem : ISystem
    {
        private EntityQuery _sliceEntityQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LevelSequenceComponent>();
            _sliceEntityQuery = state.GetEntityQuery(typeof(SliceEntityTag));
        }

        public void OnUpdate(ref SystemState state)
        {
            var levelSeqEntity = SystemAPI.GetSingletonEntity<LevelSequenceComponent>();
            var levelSeq = SystemAPI.GetComponent<LevelSequenceComponent>(levelSeqEntity);
            var buffer = SystemAPI.GetBuffer<SlicePrefabBufferElement>(levelSeqEntity);

            GameState currentGameState = GameState.Pregame;
            Entity levelStateEntity = Entity.Null;
            if (SystemAPI.HasSingleton<LevelStateComponent>())
            {
                levelStateEntity = SystemAPI.GetSingletonEntity<LevelStateComponent>();
                currentGameState = SystemAPI.GetComponent<LevelStateComponent>(levelStateEntity).CurrentState;
            }

            switch (levelSeq.TransitionState)
            {
                case LevelTransitionState.Idle:
                    if (currentGameState == GameState.Victory)
                    {
                        levelSeq.TransitionState = LevelTransitionState.PendingNext;
                    }
                    else if (currentGameState == GameState.Defeat)
                    {
                        levelSeq.TransitionState = LevelTransitionState.Failed;
                    }
                    break;

                case LevelTransitionState.PendingNext:
                    levelSeq.CurrentLevelIndex++;
                    if (levelSeq.MaxLevels > 0)
                    {
                        if (levelSeq.LoopSequencing)
                        {
                            levelSeq.CurrentLevelIndex %= levelSeq.MaxLevels;
                        }
                        else
                        {
                            levelSeq.CurrentLevelIndex = math.min(levelSeq.CurrentLevelIndex, levelSeq.MaxLevels - 1);
                        }
                    }
                    GameProgressData.CurrentLevelIndex = levelSeq.CurrentLevelIndex;
                    levelSeq.TransitionState = LevelTransitionState.TeardownCurrent;
                    break;

                case LevelTransitionState.TeardownCurrent:
                    if (levelSeq.CurrentSliceInstance != Entity.Null && state.EntityManager.Exists(levelSeq.CurrentSliceInstance))
                    {
                        state.EntityManager.DestroyEntity(levelSeq.CurrentSliceInstance);
                        levelSeq.CurrentSliceInstance = Entity.Null;
                    }

                    if (!_sliceEntityQuery.IsEmptyIgnoreFilter)
                    {
                        state.EntityManager.DestroyEntity(_sliceEntityQuery);
                    }

                    levelSeq.TransitionState = LevelTransitionState.SpawningNext;
                    break;

                case LevelTransitionState.SpawningNext:
                    if (buffer.Length > 0 && levelSeq.CurrentLevelIndex >= 0 && levelSeq.CurrentLevelIndex < buffer.Length)
                    {
                        Entity prefab = buffer[levelSeq.CurrentLevelIndex].PrefabEntity;
                        if (prefab != Entity.Null && state.EntityManager.Exists(prefab))
                        {
                            Entity instance = state.EntityManager.Instantiate(prefab);
                            levelSeq.CurrentSliceInstance = instance;
                        }
                    }

                    if (levelStateEntity != Entity.Null)
                    {
                        state.EntityManager.SetComponentData(levelStateEntity, new LevelStateComponent { CurrentState = GameState.Pregame });
                    }

                    levelSeq.TransitionState = LevelTransitionState.Idle;
                    break;

                case LevelTransitionState.Failed:
                    // Waiting for retry or level select input
                    break;
            }

            SystemAPI.SetComponent(levelSeqEntity, levelSeq);
        }
    }
}
