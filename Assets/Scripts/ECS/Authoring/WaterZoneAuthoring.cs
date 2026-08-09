using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class WaterZoneAuthoring : MonoBehaviour
    {
        public float BonusMultiplier = 2f;
        public Vector3 Size = new Vector3(15f, 2f, 20f);

        class Baker : Baker<WaterZoneAuthoring>
        {
            public override void Bake(WaterZoneAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                Vector3 pos = authoring.transform.position;
                Vector3 halfSize = authoring.Size * 0.5f;

                AddComponent(entity, new WaterZoneComponent
                {
                    BonusMultiplier = authoring.BonusMultiplier,
                    MinBounds = pos - halfSize,
                    MaxBounds = pos + halfSize
                });
            }
        }
    }
}
