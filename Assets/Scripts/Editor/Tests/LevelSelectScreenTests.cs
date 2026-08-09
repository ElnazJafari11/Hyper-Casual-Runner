using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using Unity.Entities;
using HyperCasualRunner.UI;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.Tests
{
    [TestFixture]
    public class LevelSelectScreenTests
    {
        private World _testWorld;
        private EntityManager _entityManager;

        [SetUp]
        public void SetUp()
        {
            _testWorld = new World("LevelSelectTestWorld");
            World.DefaultGameObjectInjectionWorld = _testWorld;
            _entityManager = _testWorld.EntityManager;

            PlayerPrefs.DeleteKey("HCR_CurrentLevelIndex");
            PlayerPrefs.DeleteKey("HCR_UnlockedLevelIndex");
            for (int i = 0; i < 21; i++)
            {
                PlayerPrefs.DeleteKey($"HCR_LevelStars_{i}");
            }
            PlayerPrefs.Save();
        }

        [TearDown]
        public void TearDown()
        {
            if (_testWorld != null && _testWorld.IsCreated)
            {
                _testWorld.Dispose();
            }
        }

        [Test]
        public void LevelSelectScreen_InitializationAndFallback_CreatesValidRootAndGrid()
        {
            GameObject go = new GameObject("TestLevelSelectScreen");
            UIDocument doc = go.AddComponent<UIDocument>();
            LevelSelectScreenController controller = go.AddComponent<LevelSelectScreenController>();

            Assert.IsNotNull(controller.RootElement, "RootElement should be initialized.");
            Assert.IsNotNull(controller.LevelGridContainer, "LevelGridContainer should be created.");
            Assert.AreEqual(21, controller.LevelGridContainer.childCount, "Grid should contain 21 level cards by default.");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void LevelSelectScreen_GridPopulation_BindsUnlockedAndLockedCardsCorrectly()
        {
            GameObject go = new GameObject("TestLevelSelectScreen");
            go.AddComponent<UIDocument>();
            LevelSelectScreenController controller = go.AddComponent<LevelSelectScreenController>();

            GameProgressData.UnlockedLevelIndex = 2; // Levels 0, 1, 2 unlocked (Lvl 1, Lvl 2, Lvl 3)
            GameProgressData.CurrentLevelIndex = 1;  // Lvl 2 currently playing
            GameProgressData.SetLevelStars(0, 3);
            GameProgressData.SetLevelStars(1, 2);
            GameProgressData.SetLevelStars(2, 1);

            controller.BuildLevelGrid(5);

            VisualElement grid = controller.LevelGridContainer;
            Assert.AreEqual(5, grid.childCount, "Grid count should match requested 5 levels.");

            // Card 0 (Lvl 1): Unlocked, 3 Stars, Not Playing
            VisualElement card0 = grid[0];
            Button btn0 = card0 as Button ?? card0.Q<Button>();
            Assert.IsNotNull(btn0);
            Assert.IsFalse(btn0.ClassListContains("card-locked"), "Card 0 should not be locked.");
            Assert.IsFalse(btn0.ClassListContains("card-playing"), "Card 0 should not be playing.");
            Label status0 = card0.Q<Label>("StatusLabel");
            if (status0 != null) Assert.AreEqual("UNLOCKED", status0.text);

            // Card 1 (Lvl 2): Unlocked, 2 Stars, Playing
            VisualElement card1 = grid[1];
            Button btn1 = card1 as Button ?? card1.Q<Button>();
            Assert.IsNotNull(btn1);
            Assert.IsFalse(btn1.ClassListContains("card-locked"), "Card 1 should not be locked.");
            Assert.IsTrue(btn1.ClassListContains("card-playing"), "Card 1 should have card-playing class.");
            Label status1 = card1.Q<Label>("StatusLabel");
            if (status1 != null) Assert.AreEqual("PLAYING", status1.text);

            // Card 3 (Lvl 4): Locked
            VisualElement card3 = grid[3];
            Button btn3 = card3 as Button ?? card3.Q<Button>();
            Assert.IsNotNull(btn3);
            Assert.IsTrue(btn3.ClassListContains("card-locked"), "Card 3 should have card-locked class.");
            Label status3 = card3.Q<Label>("StatusLabel");
            if (status3 != null) Assert.AreEqual("LOCKED", status3.text);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void LevelSelectScreen_LevelClick_UpdatesCurrentLevelIndexAndTriggersTransition()
        {
            // Setup LevelSequenceComponent singleton in ECS World
            Entity seqEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(seqEntity, new LevelSequenceComponent
            {
                CurrentLevelIndex = 0,
                UnlockedLevelIndex = 5,
                MaxLevels = 10,
                TransitionState = LevelTransitionState.Idle
            });

            GameObject go = new GameObject("TestLevelSelectScreen");
            go.AddComponent<UIDocument>();
            LevelSelectScreenController controller = go.AddComponent<LevelSelectScreenController>();

            GameProgressData.UnlockedLevelIndex = 5;
            bool eventFired = false;
            int selectedLevelReceived = -1;
            controller.OnLevelSelected += (lvl) =>
            {
                eventFired = true;
                selectedLevelReceived = lvl;
            };

            controller.SelectLevel(3);

            Assert.AreEqual(3, GameProgressData.CurrentLevelIndex, "GameProgressData.CurrentLevelIndex should be updated to 3.");
            Assert.IsTrue(eventFired, "OnLevelSelected event should fire.");
            Assert.AreEqual(3, selectedLevelReceived, "Event should convey selected level index 3.");

            LevelSequenceComponent updatedSeq = _entityManager.GetComponentData<LevelSequenceComponent>(seqEntity);
            Assert.AreEqual(3, updatedSeq.CurrentLevelIndex, "ECS LevelSequenceComponent CurrentLevelIndex should be updated to 3.");
            Assert.AreEqual(LevelTransitionState.TeardownCurrent, updatedSeq.TransitionState, "ECS LevelSequenceComponent TransitionState should be TeardownCurrent.");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void LevelSelectScreen_UxmlAssetBinding_InstantiatesUxmlTemplate()
        {
            GameObject go = new GameObject("TestLevelSelectScreen");
            UIDocument doc = go.AddComponent<UIDocument>();
            LevelSelectScreenController controller = go.AddComponent<LevelSelectScreenController>();

            VisualTreeAsset screenAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/LevelSelectScreen.uxml");
            VisualTreeAsset cardAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/LevelCardItem.uxml");

            if (screenAsset != null && cardAsset != null)
            {
                SerializedObject serializedObj = new SerializedObject(controller);
                serializedObj.FindProperty("_levelSelectScreenAsset").objectReferenceValue = screenAsset;
                serializedObj.FindProperty("_levelCardItemAsset").objectReferenceValue = cardAsset;
                serializedObj.ApplyModifiedProperties();

                controller.InitializeUI();
                controller.BuildLevelGrid(10);

                Assert.IsNotNull(controller.LevelGridContainer);
                Assert.AreEqual(10, controller.LevelGridContainer.childCount, "Grid should instantiate 10 card UXML instances.");
            }

            Object.DestroyImmediate(go);
        }
    }
}
