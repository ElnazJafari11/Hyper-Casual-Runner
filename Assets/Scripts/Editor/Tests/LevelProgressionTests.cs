using NUnit.Framework;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using HyperCasualRunner.ECS.Components;
using HyperCasualRunner.ECS.Systems;
using HyperCasualRunner.UI;
using UnityEngine.UIElements;

namespace HyperCasualRunner.Tests
{
    [TestFixture]
    public class LevelProgressionTests
    {
        private World testWorld;
        private EntityManager entityManager;

        [SetUp]
        public void SetUp()
        {
            testWorld = new World("LevelProgressionTestWorld");
            World.DefaultGameObjectInjectionWorld = testWorld;
            entityManager = testWorld.EntityManager;

            // Clear PlayerPrefs keys before test
            PlayerPrefs.DeleteKey("HCR_CurrentLevelIndex");
            PlayerPrefs.DeleteKey("HCR_UnlockedLevelIndex");
            PlayerPrefs.DeleteKey("HCR_TotalGold");
            for (int i = 0; i < 21; i++)
            {
                PlayerPrefs.DeleteKey($"HCR_LevelStars_{i}");
            }
            PlayerPrefs.Save();
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
        public void LevelProgressionComponents_LayoutAndEnum_ValuesMatchSpec()
        {
            Assert.AreEqual(0, (int)LevelTransitionState.Idle);
            Assert.AreEqual(1, (int)LevelTransitionState.PendingNext);
            Assert.AreEqual(2, (int)LevelTransitionState.TeardownCurrent);
            Assert.AreEqual(3, (int)LevelTransitionState.SpawningNext);
            Assert.AreEqual(4, (int)LevelTransitionState.Failed);

            Assert.AreEqual(8, Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<SlicePrefabBufferElement>(), "SlicePrefabBufferElement native size mismatch");
            Assert.AreEqual(20, Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<MetaProgressionComponent>(), "MetaProgressionComponent native size mismatch");
            Assert.AreEqual(16, Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<SaveProgressEventComponent>(), "SaveProgressEventComponent native size mismatch");
        }

        [Test]
        public void GameProgressData_Extensions_PersistLevelStateAndStars()
        {
            GameProgressData.CurrentLevelIndex = 3;
            Assert.AreEqual(3, GameProgressData.CurrentLevelIndex);

            GameProgressData.UnlockedLevelIndex = 0;
            Assert.AreEqual(0, GameProgressData.UnlockedLevelIndex);

            GameProgressData.SetLevelStars(2, 3);
            Assert.AreEqual(3, GameProgressData.GetLevelStars(2));

            GameProgressData.TotalGold = 100;
            GameProgressData.IncomeLevel = 2;
            GameProgressData.SaveLevelCompletion(3, 50, 2);

            Assert.AreEqual(200, GameProgressData.TotalGold, "TotalGold should increase by 50 * 2 = 100 (100 + 100 = 200)");
            Assert.AreEqual(2, GameProgressData.GetLevelStars(3));
            Assert.AreEqual(4, GameProgressData.UnlockedLevelIndex, "Unlocked level index should advance to 4 (level index 3 completed -> level index 4 unlocked)");
        }

        [Test]
        public void LevelProgressionSystem_TeardownAndSpawning_TransitionsCorrectly()
        {
            var systemHandle = testWorld.CreateSystem<LevelProgressionSystem>();

            // Create Level State Singleton
            Entity stateEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(stateEntity, new LevelStateComponent { CurrentState = GameState.Victory });

            // Create Slice Prefabs
            Entity prefab0 = entityManager.CreateEntity();
            Entity prefab1 = entityManager.CreateEntity();

            // Create Slice Entity Instance to Teardown
            Entity activeSliceInstance = entityManager.CreateEntity();
            Entity sliceTaggedEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(sliceTaggedEntity, new SliceEntityTag());

            // Create Level Sequence Entity
            Entity seqEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(seqEntity, new LevelSequenceComponent
            {
                CurrentLevelIndex = 0,
                MaxLevels = 2,
                UnlockedLevelIndex = 0,
                TransitionState = LevelTransitionState.Idle,
                CurrentSliceInstance = activeSliceInstance,
                LoopSequencing = true,
                AutoTransitionTimer = 0f
            });

            var buffer = entityManager.AddBuffer<SlicePrefabBufferElement>(seqEntity);
            buffer.Add(new SlicePrefabBufferElement { PrefabEntity = prefab0 });
            buffer.Add(new SlicePrefabBufferElement { PrefabEntity = prefab1 });

            // 1st Update: Idle -> PendingNext (because CurrentState == Victory)
            systemHandle.Update(testWorld.Unmanaged);
            var seq = entityManager.GetComponentData<LevelSequenceComponent>(seqEntity);
            Assert.AreEqual(LevelTransitionState.PendingNext, seq.TransitionState);

            // 2nd Update: PendingNext -> TeardownCurrent (advances CurrentLevelIndex to 1)
            systemHandle.Update(testWorld.Unmanaged);
            seq = entityManager.GetComponentData<LevelSequenceComponent>(seqEntity);
            Assert.AreEqual(LevelTransitionState.TeardownCurrent, seq.TransitionState);
            Assert.AreEqual(1, seq.CurrentLevelIndex);

            // 3rd Update: TeardownCurrent -> SpawningNext (destroys activeSliceInstance and sliceTaggedEntity)
            systemHandle.Update(testWorld.Unmanaged);
            seq = entityManager.GetComponentData<LevelSequenceComponent>(seqEntity);
            Assert.AreEqual(LevelTransitionState.SpawningNext, seq.TransitionState);
            Assert.IsFalse(entityManager.Exists(activeSliceInstance), "activeSliceInstance should be destroyed");
            Assert.IsFalse(entityManager.Exists(sliceTaggedEntity), "sliceTaggedEntity should be destroyed");

            // 4th Update: SpawningNext -> Idle (instantiates prefab1 and resets state to Pregame)
            systemHandle.Update(testWorld.Unmanaged);
            seq = entityManager.GetComponentData<LevelSequenceComponent>(seqEntity);
            Assert.AreEqual(LevelTransitionState.Idle, seq.TransitionState);
            Assert.IsTrue(seq.CurrentSliceInstance != Entity.Null && entityManager.Exists(seq.CurrentSliceInstance), "New slice instance should be created");

            var updatedGameState = entityManager.GetComponentData<LevelStateComponent>(stateEntity);
            Assert.AreEqual(GameState.Pregame, updatedGameState.CurrentState, "GameState should reset to Pregame");
        }

        [Test]
        public void MetaProgressionSaveSystem_ProcessesSaveEvents()
        {
            var systemHandle = testWorld.CreateSystem<MetaProgressionSaveSystem>();

            Entity saveEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(saveEntity, new SaveProgressEventComponent
            {
                CompletedLevelIndex = 1,
                CoinsEarnedInRun = 40,
                StarsEarned = 3,
                IsVictory = true
            });

            GameProgressData.TotalGold = 0;
            GameProgressData.IncomeLevel = 1;

            systemHandle.Update(testWorld.Unmanaged);

            Assert.AreEqual(40, GameProgressData.TotalGold, "TotalGold should increase to 40");
            Assert.AreEqual(3, GameProgressData.GetLevelStars(1), "Stars for level 1 should be 3");
            Assert.AreEqual(2, GameProgressData.UnlockedLevelIndex, "Unlocked level should be 2");
            Assert.IsFalse(entityManager.IsComponentEnabled<SaveProgressEventComponent>(saveEntity), "SaveProgressEventComponent should be disabled after processing");
        }

        [Test]
        public void UIManagerSystem_InstantiatesAndGenerates21LevelGrid()
        {
            GameObject uiGO = new GameObject("UIManagerSystemGO");
            UIDocument doc = uiGO.AddComponent<UIDocument>();
            UIManagerSystem uiSystem = uiGO.AddComponent<UIManagerSystem>();

            Assert.IsNotNull(uiSystem, "UIManagerSystem component should exist");
            uiSystem.BuildLevelGrid();

            var grid = uiSystem.LevelGridContainer ?? doc.rootVisualElement.Q<VisualElement>("LevelGridContainer");
            Assert.IsNotNull(grid, "LevelGridContainer should exist in UIManagerSystem");
            Assert.AreEqual(21, grid.childCount, "LevelGridContainer should contain exactly 21 level cards");

            Object.DestroyImmediate(uiGO);
        }
    }
}
