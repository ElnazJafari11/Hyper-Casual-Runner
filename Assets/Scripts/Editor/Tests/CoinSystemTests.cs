using NUnit.Framework;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using HyperCasualRunner.ECS.Components;
using HyperCasualRunner.ECS.Systems;
using HyperCasualRunner.ECS.Authoring;
using HyperCasualRunner.Editor;

namespace HyperCasualRunner.Tests
{
    [TestFixture]
    public class CoinSystemTests
    {
        private World testWorld;
        private EntityManager entityManager;

        [SetUp]
        public void SetUp()
        {
            testWorld = new World("CoinSystemTestWorld");
            World.DefaultGameObjectInjectionWorld = testWorld;
            entityManager = testWorld.EntityManager;
        }

        [TearDown]
        public void TearDown()
        {
            if (testWorld != null && testWorld.IsCreated)
            {
                testWorld.Dispose();
            }
        }

        [Test]
        public void CoinComponents_LayoutAndSizes_MatchSpecifications()
        {
            Assert.AreEqual(28, System.Runtime.InteropServices.Marshal.SizeOf(typeof(CoinMultiplierGateComponent)), "CoinMultiplierGateComponent size mismatch");
            Assert.AreEqual(20, Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<CoinMultiplierGateComponent>(), "CoinMultiplierGateComponent native unmanaged size mismatch");
            Assert.AreEqual(52, System.Runtime.InteropServices.Marshal.SizeOf(typeof(CoinSplitPhysicsComponent)), "CoinSplitPhysicsComponent size mismatch");
            Assert.AreEqual(44, Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<CoinSplitPhysicsComponent>(), "CoinSplitPhysicsComponent native unmanaged size mismatch");
            Assert.AreEqual(24, System.Runtime.InteropServices.Marshal.SizeOf(typeof(PlayerCoinRunnerComponent)), "PlayerCoinRunnerComponent size mismatch");
            Assert.AreEqual(32, System.Runtime.InteropServices.Marshal.SizeOf(typeof(CoinMultiplierEventComponent)), "CoinMultiplierEventComponent size mismatch");
            Assert.AreEqual(32, System.Runtime.InteropServices.Marshal.SizeOf(typeof(CoinSplitEventComponent)), "CoinSplitEventComponent size mismatch");
            Assert.AreEqual(16, System.Runtime.InteropServices.Marshal.SizeOf(typeof(CoinStackElement)), "CoinStackElement size mismatch");
        }

        [Test]
        public void CoinMultiplierSystem_AdditiveGate_IncreasesCoinCountAndTriggers()
        {
            var systemHandle = testWorld.CreateSystem<CoinMultiplierSystem>();

            // Create Player Entity
            Entity playerEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(playerEntity, new PlayerCoinRunnerComponent
            {
                CurrentCoinCount = 5,
                StackSpacing = 0.15f,
                MaxStackHeight = 2.0f,
                SwerveSensitivity = 1.0f,
                CoinVisualPrefab = Entity.Null
            });
            entityManager.AddComponentData(playerEntity, LocalTransform.FromPosition(new float3(0, 1, 10)));

            // Create Additive Multiplier Gate Entity (+10)
            Entity gateEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(gateEntity, new CoinMultiplierGateComponent
            {
                GateType = MultiplierType.Additive,
                IsTriggered = false,
                Value = 10f,
                GateWidth = 4f,
                TriggerDepth = 1f,
                MinimumOutput = 1f
            });
            entityManager.AddComponentData(gateEntity, new CoinMultiplierGateTag());
            entityManager.AddComponentData(gateEntity, LocalTransform.FromPosition(new float3(0, 1, 10)));

            systemHandle.Update(testWorld.Unmanaged);

            // Assert gate was marked triggered
            var updatedGate = entityManager.GetComponentData<CoinMultiplierGateComponent>(gateEntity);
            Assert.IsTrue(updatedGate.IsTriggered, "Gate should be marked IsTriggered");

            // Assert player coin count increased by 10 (5 -> 15)
            var updatedPlayer = entityManager.GetComponentData<PlayerCoinRunnerComponent>(playerEntity);
            Assert.AreEqual(15, updatedPlayer.CurrentCoinCount, "Player coin count should be 15");

            // Assert gate was marked with DestroyEventComponent
            Assert.IsTrue(entityManager.HasComponent<DestroyEventComponent>(gateEntity), "Gate entity should have DestroyEventComponent attached");
        }

        [Test]
        public void CoinMultiplierSystem_MultiplicativeGate_MultipliesCoinCount()
        {
            var systemHandle = testWorld.CreateSystem<CoinMultiplierSystem>();

            // Create Player Entity
            Entity playerEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(playerEntity, new PlayerCoinRunnerComponent
            {
                CurrentCoinCount = 4,
                StackSpacing = 0.15f,
                MaxStackHeight = 2.0f,
                SwerveSensitivity = 1.0f,
                CoinVisualPrefab = Entity.Null
            });
            entityManager.AddComponentData(playerEntity, LocalTransform.FromPosition(new float3(0, 1, 20)));

            // Create Multiplicative Gate Entity (x3)
            Entity gateEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(gateEntity, new CoinMultiplierGateComponent
            {
                GateType = MultiplierType.Multiplicative,
                IsTriggered = false,
                Value = 3f,
                GateWidth = 4f,
                TriggerDepth = 1f,
                MinimumOutput = 1f
            });
            entityManager.AddComponentData(gateEntity, new CoinMultiplierGateTag());
            entityManager.AddComponentData(gateEntity, LocalTransform.FromPosition(new float3(0, 1, 20)));

            systemHandle.Update(testWorld.Unmanaged);

            // Assert player coin count multiplied by 3 (4 -> 12)
            var updatedPlayer = entityManager.GetComponentData<PlayerCoinRunnerComponent>(playerEntity);
            Assert.AreEqual(12, updatedPlayer.CurrentCoinCount, "Player coin count should be 12 after x3 gate");
        }

        [Test]
        public void CoinPhysicsSystem_SimulatesAirborneGravityAndBounce()
        {
            var systemHandle = testWorld.CreateSystem<CoinPhysicsSystem>();

            // Create Coin Entity in airborne state at Y=5.0
            Entity coinEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(coinEntity, new CoinSplitPhysicsComponent
            {
                CurrentVelocity = new float3(0, 2f, 0),
                SpreadAngle = 60f,
                ImpulseSpeed = 8f,
                StackHeightOffset = 0.5f,
                Lifetime = 3f,
                MaxLifetime = 3f,
                GravityMultiplier = 2.5f,
                CoinCount = 1,
                IsGrounded = false,
                IsCollectible = false
            });
            entityManager.AddComponentData(coinEntity, new CoinTag());
            entityManager.AddComponentData(coinEntity, LocalTransform.FromPosition(new float3(0, 5f, 0)));

            // Run system updates to simulate physics frames
            for (int i = 0; i < 20; i++)
            {
                testWorld.SetTime(new Unity.Core.TimeData((i + 1) * 0.0166667f, 0.0166667f));
                systemHandle.Update(testWorld.Unmanaged);
            }

            // Assert position moved downward due to gravity
            var transform = entityManager.GetComponentData<LocalTransform>(coinEntity);
            Assert.Less(transform.Position.y, 5.0f, "Coin Y position should decrease due to gravity");
        }

        [Test]
        public void MoneyRush_Slice14_PrefabContainsCoinSpawnerAndGates()
        {
            ToolkitExampleGenerator.GenerateExamples();
            string path = "Assets/ToolkitExamples/14_MoneyRush_Coins_Slice.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            Assert.IsNotNull(prefab, "14_MoneyRush_Coins_Slice.prefab should exist");

            Transform player = prefab.transform.Find("PlayerEntity");
            Assert.IsNotNull(player, "PlayerEntity missing from slice 14");

            var spawner = player.GetComponent<CoinSpawnerAuthoring>();
            Assert.IsNotNull(spawner, "CoinSpawnerAuthoring missing from PlayerEntity");
            Assert.IsNotNull(spawner.CoinVisualPrefab, "CoinVisualPrefab missing on CoinSpawnerAuthoring");

            int gateCount = 0;
            foreach (Transform child in prefab.transform)
            {
                if (child.GetComponent<CoinGateAuthoring>() != null)
                {
                    gateCount++;
                }
            }

            Assert.AreEqual(5, gateCount, "Slice 14 prefab should contain exactly 5 CoinGateAuthoring gates (+2, x3, +10, x2, x4)");
        }
    }
}
