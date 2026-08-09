using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class GapZoneAuthoring : MonoBehaviour
    {
        public Vector3 Size = new Vector3(10f, 5f, 20f);

        class Baker : Baker<GapZoneAuthoring>
        {
            public override void Bake(GapZoneAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                Vector3 pos = authoring.transform.position;
                Vector3 halfSize = authoring.Size * 0.5f;

                AddComponent(entity, new GapZoneComponent
                {
                    MinBounds = pos - halfSize,
                    MaxBounds = pos + halfSize
                });
            }
        }
    }
}
