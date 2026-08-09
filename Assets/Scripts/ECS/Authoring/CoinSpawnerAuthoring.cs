using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class CoinSpawnerAuthoring : MonoBehaviour
    {
        public GameObject CoinVisualPrefab;
        public int StartingCoinCount = 1;
        public float StackSpacing = 0.15f;
        public float MaxStackHeight = 2.0f;
        public float SwerveSensitivity = 1.0f;

        class Baker : Baker<CoinSpawnerAuthoring>
        {
            public override void Bake(CoinSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                Entity coinPrefabEntity = Entity.Null;
                if (authoring.CoinVisualPrefab != null)
                {
                    coinPrefabEntity = GetEntity(authoring.CoinVisualPrefab, TransformUsageFlags.Dynamic);
                }

                AddComponent(entity, new PlayerCoinRunnerComponent
                {
                    CoinVisualPrefab = coinPrefabEntity,
                    CurrentCoinCount = authoring.StartingCoinCount,
                    StackSpacing = authoring.StackSpacing,
                    MaxStackHeight = authoring.MaxStackHeight,
                    SwerveSensitivity = authoring.SwerveSensitivity
                });
            }
        }
    }
}
