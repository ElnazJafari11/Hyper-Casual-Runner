using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class SwarmMechanicsAuthoring : MonoBehaviour
    {
        public GameObject SwarmMemberPrefab;
        public int StartingCount = 1;

        class Baker : Baker<SwarmMechanicsAuthoring>
        {
            public override void Bake(SwarmMechanicsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                Entity prefabEntity = Entity.Null;
                if (authoring.SwarmMemberPrefab != null)
                {
                    prefabEntity = GetEntity(authoring.SwarmMemberPrefab, TransformUsageFlags.Dynamic);
                }

                int totalStartingCount = authoring.StartingCount + (GameProgressData.SwarmLevel - 1);

                AddComponent(entity, new SwarmComponent
                {
                    TargetCount = totalStartingCount,
                    CurrentCount = 1, // Always start with 1 physically instantiated (the root)
                    SwarmMemberPrefab = prefabEntity
                });
            }
        }
    }
}
