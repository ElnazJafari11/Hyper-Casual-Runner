using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class EndZoneAuthoring : MonoBehaviour
    {
        public float Radius = 3f;

        class Baker : Baker<EndZoneAuthoring>
        {
            public override void Bake(EndZoneAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EndZoneComponent { TriggerRadius = authoring.Radius });
            }
        }
    }

    // A simple singleton authoring to establish the level state
    public class LevelManagerAuthoring : MonoBehaviour
    {
        class Baker : Baker<LevelManagerAuthoring>
        {
            public override void Bake(LevelManagerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new LevelStateComponent { CurrentState = GameState.Pregame });
            }
        }
    }
}
