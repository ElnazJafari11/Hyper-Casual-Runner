using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class StiltsAuthoring : MonoBehaviour
    {
        public float HeelHeight = 0.5f;
        public int StartingHeels = 0;

        class Baker : Baker<StiltsAuthoring>
        {
            public override void Bake(StiltsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new StiltsComponent
                {
                    HeelHeight = authoring.HeelHeight,
                    CurrentHeels = authoring.StartingHeels
                });
            }
        }
    }
}
