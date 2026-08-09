using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using HyperCasualRunner.Editor;
using HyperCasualRunner.ECS.Authoring;

namespace HyperCasualRunner.Tests
{
    public class ToolkitGeneratorTests
    {
        [SetUp]
        public void Setup()
        {
            // Ensure prefabs are generated fresh before tests
            ToolkitExampleGenerator.GenerateExamples();
            ToolkitExampleGenerator.RunVerificationSuite();
        }

        [Test]
        public void Generator_OutputsAll20Prefabs()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/ToolkitExamples" });
            Assert.GreaterOrEqual(guids.Length, 20, "Generator did not output at least 20 prefabs.");
        }

        [Test]
        public void CountMasters_ContainsSwarmMechanicsAndGates()
        {
            string path = "Assets/ToolkitExamples/4_CountMasters_Swarm_Slice.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            Assert.IsNotNull(prefab, "CountMasters prefab not found.");
            
            var player = prefab.transform.Find("PlayerEntity");
            Assert.IsNotNull(player, "PlayerEntity not found in prefab.");
            
            var swarmAuthoring = player.GetComponent<SwarmMechanicsAuthoring>();
            Assert.IsNotNull(swarmAuthoring, "SwarmMechanicsAuthoring not found on PlayerEntity.");

            int gateCount = 0;
            foreach (Transform child in prefab.transform)
            {
                if (child.GetComponent<MathGateAuthoring>() != null)
                {
                    gateCount++;
                }
            }
            Assert.AreEqual(3, gateCount, "CountMasters prefab does not contain exactly 3 Math Gates.");
        }

        [Test]
        public void MyMiniMart_ContainsArcadeIdleNodes()
        {
            string path = "Assets/ToolkitExamples/16_MyMiniMart_Supply_Slice.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            Assert.IsNotNull(prefab, "MyMiniMart prefab not found.");

            int nodeCount = 0;
            foreach (Transform child in prefab.transform)
            {
                if (child.GetComponent<ArcadeIdleAuthoring>() != null)
                {
                    nodeCount++;
                }
            }
            Assert.AreEqual(3, nodeCount, "MyMiniMart prefab does not contain exactly 3 Arcade Idle nodes.");
        }

        [Test]
        public void StackyDash_ContainsMazeCollectorAndGridPathfinder()
        {
            string path = "Assets/ToolkitExamples/12_StackyDash_Grid_Slice.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            Assert.IsNotNull(prefab, "StackyDash prefab not found.");

            var player = prefab.transform.Find("PlayerEntity");
            Assert.IsNotNull(player, "PlayerEntity not found in prefab.");

            var collector = player.GetComponent<MazeCollectorAuthoring>();
            Assert.IsNotNull(collector, "MazeCollectorAuthoring not found on PlayerEntity.");

            var pathfinder = player.GetComponent<GridPathfinderAuthoring>();
            Assert.IsNotNull(pathfinder, "GridPathfinderAuthoring not found on PlayerEntity.");

            int tileCount = 0;
            foreach (Transform child in prefab.transform)
            {
                if (child.GetComponent<GridTileAuthoring>() != null)
                {
                    tileCount++;
                }
            }
            Assert.Greater(tileCount, 0, "StackyDash prefab does not contain GridTileAuthoring tiles.");
        }

        [Test]
        public void JoinClash_ContainsSnakeChainAuthoring()
        {
            string path = "Assets/ToolkitExamples/5_JoinClash_Snake_Slice.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            Assert.IsNotNull(prefab, "JoinClash_Snake prefab not found.");

            var player = prefab.transform.Find("PlayerEntity");
            Assert.IsNotNull(player, "PlayerEntity not found in prefab.");

            var snakeAuthoring = player.GetComponent<SnakeChainAuthoring>();
            Assert.IsNotNull(snakeAuthoring, "SnakeChainAuthoring not found on PlayerEntity.");
            Assert.IsNotNull(snakeAuthoring.FollowerPrefab, "FollowerPrefab reference is null on SnakeChainAuthoring.");
        }

        [Test]
        public void MoneyRush_ContainsCoinSpawnerAndGates()
        {
            string path = "Assets/ToolkitExamples/14_MoneyRush_Coins_Slice.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            Assert.IsNotNull(prefab, "MoneyRush_Coins prefab not found.");

            var player = prefab.transform.Find("PlayerEntity");
            Assert.IsNotNull(player, "PlayerEntity not found in prefab.");

            var spawnerAuthoring = player.GetComponent<CoinSpawnerAuthoring>();
            Assert.IsNotNull(spawnerAuthoring, "CoinSpawnerAuthoring not found on PlayerEntity.");
            Assert.IsNotNull(spawnerAuthoring.CoinVisualPrefab, "CoinVisualPrefab reference is null on CoinSpawnerAuthoring.");

            int gateCount = 0;
            foreach (Transform child in prefab.transform)
            {
                if (child.GetComponent<CoinGateAuthoring>() != null)
                {
                    gateCount++;
                }
            }
            Assert.AreEqual(5, gateCount, "MoneyRush_Coins prefab does not contain exactly 5 CoinGateAuthoring gates.");
        }
    }
}
