using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class LevelSequenceAuthoring : MonoBehaviour
    {
        public GameObject[] SlicePrefabs;
        public bool LoopSequencing = true;

        public class Baker : Baker<LevelSequenceAuthoring>
        {
            public override void Bake(LevelSequenceAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                int maxLevels = authoring.SlicePrefabs != null ? authoring.SlicePrefabs.Length : 0;

                AddComponent(entity, new LevelSequenceComponent
                {
                    CurrentLevelIndex = 0,
                    MaxLevels = maxLevels,
                    UnlockedLevelIndex = 0,
                    TransitionState = LevelTransitionState.Idle,
                    CurrentSliceInstance = Entity.Null,
                    LoopSequencing = authoring.LoopSequencing,
                    AutoTransitionTimer = 0f
                });

                var buffer = AddBuffer<SlicePrefabBufferElement>(entity);
                if (authoring.SlicePrefabs != null)
                {
                    for (int i = 0; i < authoring.SlicePrefabs.Length; i++)
                    {
                        if (authoring.SlicePrefabs[i] != null)
                        {
                            Entity prefabEntity = GetEntity(authoring.SlicePrefabs[i], TransformUsageFlags.Dynamic);
                            buffer.Add(new SlicePrefabBufferElement { PrefabEntity = prefabEntity });
                        }
                    }
                }

                // Add SaveProgressEventComponent disabled by default
                AddComponent(entity, new SaveProgressEventComponent());
                SetComponentEnabled<SaveProgressEventComponent>(entity, false);
            }
        }
    }
}
