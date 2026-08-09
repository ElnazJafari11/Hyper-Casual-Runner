using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class LaneAuthoring : MonoBehaviour
    {
        public int StartingLane = 0;
        public float LaneWidth = 2.5f;
        public float LaneChangeSpeed = 15f;

        class Baker : Baker<LaneAuthoring>
        {
            public override void Bake(LaneAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new LaneComponent
                {
                    CurrentLane = authoring.StartingLane,
                    TargetLane = authoring.StartingLane,
                    LaneWidth = authoring.LaneWidth,
                    LaneChangeSpeed = authoring.LaneChangeSpeed
                });
            }
        }
    }
}
