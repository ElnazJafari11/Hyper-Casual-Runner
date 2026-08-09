using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public enum LevelTransitionState
    {
        Idle,            // Gameplay active or waiting in Pregame
        PendingNext,     // Victory reached, preparing transition
        TeardownCurrent, // Destroying current slice entities
        SpawningNext,    // Instantiating next slice prefab entity
        Failed           // Defeat state, awaiting retry/restart input
    }

    [InternalBufferCapacity(21)]
    public struct SlicePrefabBufferElement : IBufferElementData
    {
        public Entity PrefabEntity;
    }

    public struct LevelSequenceComponent : IComponentData
    {
        public int CurrentLevelIndex;
        public int MaxLevels;
        public int UnlockedLevelIndex;
        public LevelTransitionState TransitionState;
        public Entity CurrentSliceInstance;
        public bool LoopSequencing;
        public float AutoTransitionTimer;
    }

    public struct SliceEntityTag : IComponentData {}

    public struct MetaProgressionComponent : IComponentData
    {
        public int TotalGold;
        public int CurrentLevelIndex;
        public int UnlockedLevelIndex;
        public int SwarmLevel;
        public float IncomeMultiplier;
    }

    public struct SaveProgressEventComponent : IComponentData, IEnableableComponent
    {
        public int CompletedLevelIndex;
        public int CoinsEarnedInRun;
        public int StarsEarned;
        public bool IsVictory;
    }
}
