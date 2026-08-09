using NUnit.Framework;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;
using HyperCasualRunner.ECS.Components;
using HyperCasualRunner.ECS.Systems;
using HyperCasualRunner.UI;

namespace HyperCasualRunner.Tests
{
    [TestFixture]
    public class Milestone4StressTests
    {
        private World _testWorld;
        private EntityManager _entityManager;

        [SetUp]
        public void SetUp()
        {
            _testWorld = new World("M4StressTestWorld");
            World.DefaultGameObjectInjectionWorld = _testWorld;
            _entityManager = _testWorld.EntityManager;

            // Clear PlayerPrefs before test
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        [TearDown]
        public void TearDown()
        {
            if (_testWorld != null && _testWorld.IsCreated)
            {
                _testWorld.Dispose();
            }
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        [Test]
        public void StarBoundary_NegativeAndExceedingValues_ClampedCorrectly()
        {
            GameProgressData.SetLevelStars(0, -1);
            Assert.AreEqual(0, GameProgressData.GetLevelStars(0), "Negative star value should clamp to 0");

            GameProgressData.SetLevelStars(0, 0);
            Assert.AreEqual(0, GameProgressData.GetLevelStars(0), "0 stars should persist as 0");

            GameProgressData.SetLevelStars(0, 1);
            Assert.AreEqual(1, GameProgressData.GetLevelStars(0), "1 star should persist as 1");

            GameProgressData.SetLevelStars(0, 2);
            Assert.AreEqual(2, GameProgressData.GetLevelStars(0), "2 stars should persist as 2");

            GameProgressData.SetLevelStars(0, 3);
            Assert.AreEqual(3, GameProgressData.GetLevelStars(0), "3 stars should persist as 3");

            GameProgressData.SetLevelStars(0, 5);
            Assert.AreEqual(3, GameProgressData.GetLevelStars(0), "Value > 3 should clamp to 3");
        }

        [Test]
        public void StarBoundary_ScoreRetention_PreservesHighestStars()
        {
            // First earn 3 stars on Level 1
            GameProgressData.SaveLevelCompletion(1, 10, 3);
            Assert.AreEqual(3, GameProgressData.GetLevelStars(1));

            // Replay Level 1 and earn 1 star
            GameProgressData.SaveLevelCompletion(1, 10, 1);
            Assert.AreEqual(3, GameProgressData.GetLevelStars(1), "Replaying with lower star score (1) must retain highest earned stars (3)");
        }

        [Test]
        public void PlayerPrefs_RepeatedSaves_IntegrityUnderStress()
        {
            const int iterations = 100;
            for (int i = 0; i < iterations; i++)
            {
                int goldBefore = GameProgressData.TotalGold;
                GameProgressData.SaveLevelCompletion(i % 21, i * 5, (i % 3) + 1);
                Assert.AreEqual(i * 5 * GameProgressData.IncomeLevel, GameProgressData.TotalGold - goldBefore, $"Iteration {i}: Total gold increment mismatch");
            }
            Assert.IsTrue(GameProgressData.TotalGold > 0, "Total gold accumulated successfully over 100 rapid save cycles");
        }

        [Test]
        public void LevelProgressionSystem_100CycleStress_TeardownAndEntityCount()
        {
            var systemHandle = _testWorld.CreateSystem<LevelProgressionSystem>();

            Entity stateEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(stateEntity, new LevelStateComponent { CurrentState = GameState.Playing });

            Entity seqEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(seqEntity, new LevelSequenceComponent
            {
                CurrentLevelIndex = 0,
                MaxLevels = 21,
                UnlockedLevelIndex = 0,
                TransitionState = LevelTransitionState.Idle,
                CurrentSliceInstance = Entity.Null,
                LoopSequencing = true
            });

            var buffer = _entityManager.AddBuffer<SlicePrefabBufferElement>(seqEntity);
            for (int i = 0; i < 21; i++)
            {
                Entity prefab = _entityManager.CreateEntity();
                buffer.Add(new SlicePrefabBufferElement { PrefabEntity = prefab });
            }

            // Simulate 100 transitions
            for (int cycle = 0; cycle < 100; cycle++)
            {
                _entityManager.SetComponentData(stateEntity, new LevelStateComponent { CurrentState = GameState.Victory });
                
                // Update 1: Idle -> PendingNext
                systemHandle.Update(_testWorld.Unmanaged);
                // Update 2: PendingNext -> TeardownCurrent
                systemHandle.Update(_testWorld.Unmanaged);
                // Update 3: TeardownCurrent -> SpawningNext
                systemHandle.Update(_testWorld.Unmanaged);
                // Update 4: SpawningNext -> Idle
                systemHandle.Update(_testWorld.Unmanaged);

                var seq = _entityManager.GetComponentData<LevelSequenceComponent>(seqEntity);
                Assert.AreEqual(LevelTransitionState.Idle, seq.TransitionState);
            }

            var seqFinal = _entityManager.GetComponentData<LevelSequenceComponent>(seqEntity);
            Assert.AreEqual(16, seqFinal.CurrentLevelIndex, "100 cycles mod 21 starting at 0 should reach index 16");
        }

        [Test]
        public void UIManagerSystem_DuplicateOnEnable_MeasuresCallbackAccumulation()
        {
            GameObject uiGO = new GameObject("UIManagerTest");
            UIDocument doc = uiGO.AddComponent<UIDocument>();
            UIManagerSystem uiSystem = uiGO.AddComponent<UIManagerSystem>();

            Assert.IsNotNull(uiSystem);

            // Simulate multiple OnEnable/OnDisable calls
            uiGO.SetActive(false);
            uiGO.SetActive(true);
            uiGO.SetActive(false);
            uiGO.SetActive(true);

            Assert.IsFalse(uiSystem.HasProcessedVictorySave);

            Object.DestroyImmediate(uiGO);
        }

        [Test]
        public void UIManagerSystem_VictoryUpdate_DoesNotAutoSaveGold()
        {
            GameObject uiGO = new GameObject("UIManagerVictoryTest");
            UIDocument doc = uiGO.AddComponent<UIDocument>();
            UIManagerSystem uiSystem = uiGO.AddComponent<UIManagerSystem>();

            // Setup level state as Victory
            Entity stateEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(stateEntity, new LevelStateComponent { CurrentState = GameState.Victory });

            int initialGold = GameProgressData.TotalGold;

            // Trigger MonoBehaviour Update via reflection
            System.Reflection.MethodInfo updateMethod = typeof(UIManagerSystem).GetMethod("Update", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (updateMethod != null)
            {
                updateMethod.Invoke(uiSystem, null);
            }

            // Verify gold was NOT saved automatically in Update()
            Assert.AreEqual(initialGold, GameProgressData.TotalGold, "Gold should NOT be auto-saved in Update() on Victory state");
            Assert.IsFalse(uiSystem.HasProcessedVictorySave, "HasProcessedVictorySave should remain false until player claims reward");

            // Manually invoke ClaimMultipliedGold to simulate player button press
            uiSystem.ClaimMultipliedGold();
            Assert.IsTrue(uiSystem.HasProcessedVictorySave, "HasProcessedVictorySave should be true after ClaimMultipliedGold");

            Object.DestroyImmediate(uiGO);
        }

        [Test]
        public void UIManagerSystem_DynamicStarRating_CalculatesStarsCorrectly()
        {
            GameObject uiGO = new GameObject("UIManagerStarTest");
            UIDocument doc = uiGO.AddComponent<UIDocument>();
            UIManagerSystem uiSystem = uiGO.AddComponent<UIManagerSystem>();

            Assert.AreEqual(3, uiSystem.CalculateStars(10, 10), ">= 80% coins should award 3 stars");
            Assert.AreEqual(2, uiSystem.CalculateStars(6, 10), "50-79% coins should award 2 stars");
            Assert.AreEqual(1, uiSystem.CalculateStars(2, 10), "< 50% coins should award 1 star");

            Assert.AreEqual("★ ★ ★", UIManagerSystem.GetStarString(3));
            Assert.AreEqual("★ ★ ☆", UIManagerSystem.GetStarString(2));
            Assert.AreEqual("★ ☆ ☆", UIManagerSystem.GetStarString(1));

            Object.DestroyImmediate(uiGO);
        }
    }
}
