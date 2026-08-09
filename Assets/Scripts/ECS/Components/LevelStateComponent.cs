using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public enum GameState
    {
        Pregame,
        Playing,
        Victory,
        Defeat
    }

    public struct LevelStateComponent : IComponentData
    {
        public GameState CurrentState;
    }

    public struct EndZoneComponent : IComponentData
    {
        public float TriggerRadius;
    }
}
