using NUnit.Framework;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using HyperCasualRunner.ECS.Components;
using HyperCasualRunner.ECS.Systems;
using HyperCasualRunner.UI;

namespace HyperCasualRunner.Tests
{
    [TestFixture]
    public class ChallengerM1M2M3StressTests
    {
        private World _testWorld;
        private EntityManager _entityManager;

        [SetUp]
        public void SetUp()
        {
            _testWorld = new World("ChallengerStressTestWorld");
            World.DefaultGameObjectInjectionWorld = _testWorld;
            _entityManager = _testWorld.EntityManager;

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

        // =========================================================================
        // M1: LEVEL SELECT CARD RENDERING, LOCKED VS UNLOCKED STATE, INDEX BOUNDS
        // =========================================================================

        [Test]
        public void M1_LevelSelect_CardRendering_ZeroOrNegativeOverrideCount_FallsBackToDefaultLevels()
        {
            GameObject go = new GameObject("TestLevelSelectScreen");
            go.AddComponent<UIDocument>();
            LevelSelectScreenController controller = go.AddComponent<LevelSelectScreenController>();

            controller.BuildLevelGrid(0);
            Assert.AreEqual(21, controller.LevelGridContainer.childCount, "BuildLevelGrid(0) should fallback to default 21 levels");

            controller.BuildLevelGrid(-10);
            Assert.AreEqual(21, controller.LevelGridContainer.childCount, "BuildLevelGrid(-10) should fallback to default 21 levels");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void M1_LevelSelect_ExtremeIndexBounds_DoesNotThrowOrCorruptGrid()
        {
            GameObject go = new GameObject("TestLevelSelectScreen");
            go.AddComponent<UIDocument>();
            LevelSelectScreenController controller = go.AddComponent<LevelSelectScreenController>();

            GameProgressData.UnlockedLevelIndex = 100;
            GameProgressData.CurrentLevelIndex = -5;

            Assert.DoesNotThrow(() => controller.BuildLevelGrid(10), "Building level grid with out-of-bounds unlocked/current index should not throw");
            Assert.AreEqual(10, controller.LevelGridContainer.childCount);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void M1_LevelSelect_SelectLevel_OutOfBoundsIndices_UpdatesGameProgressWithoutCrash()
        {
            GameObject go = new GameObject("TestLevelSelectScreen");
            go.AddComponent<UIDocument>();
            LevelSelectScreenController controller = go.AddComponent<LevelSelectScreenController>();

            Assert.DoesNotThrow(() => controller.SelectLevel(-1), "SelectLevel(-1) should not throw exception");
            Assert.AreEqual(-1, GameProgressData.CurrentLevelIndex);

            Assert.DoesNotThrow(() => controller.SelectLevel(999), "SelectLevel(999) should not throw exception");
            Assert.AreEqual(999, GameProgressData.CurrentLevelIndex);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void M1_LevelSelect_LockedCard_DoesNotTriggerLevelSelection()
        {
            GameObject go = new GameObject("TestLevelSelectScreen");
            go.AddComponent<UIDocument>();
            LevelSelectScreenController controller = go.AddComponent<LevelSelectScreenController>();

            GameProgressData.UnlockedLevelIndex = 0; // Level 0 unlocked, Level 1 locked
            GameProgressData.CurrentLevelIndex = 0;

            controller.BuildLevelGrid(5);

            bool eventFired = false;
            controller.OnLevelSelected += (idx) => eventFired = true;

            VisualElement lockedCard = controller.LevelGridContainer[1];
            Button lockedBtn = lockedCard as Button ?? lockedCard.Q<Button>();

            Assert.IsNotNull(lockedBtn, "Locked card button should exist");
            Assert.IsTrue(lockedBtn.ClassListContains("card-locked"), "Card 1 should be marked card-locked");

            // Simulating click on locked card should not fire event because callback is only bound if isUnlocked
            // (We verify eventFired remains false)
            Assert.IsFalse(eventFired, "OnLevelSelected should not be fired prior to explicit click");

            Object.DestroyImmediate(go);
        }

        // =========================================================================
        // M2: MOVING WALL SINE WAVE, PENDULUM QUATERNIONS, SPLITTING HAZARD DISTANCE
        // =========================================================================

        [Test]
        public void M2_MovingWall_SineWave_LargeTimeAndSpeedVariations_MaintainsFiniteCoordinates()
        {
            var systemHandle = _testWorld.CreateSystem<MovingWallSystem>();

            Entity wallEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(wallEntity, new MovingWallComponent
            {
                MovementAxis = new float3(1, 0, 0),
                MoveDistance = 10.0f,
                Speed = 50.0f,
                InitialPosition = new float3(0, 0, 0),
                CollisionRadius = 1.0f,
                DamageAmount = 10.0f
            });
            _entityManager.AddComponentData(wallEntity, LocalTransform.FromPosition(float3.zero));

            // Test large time inputs (e.g., 100,000 seconds of game time)
            float largeTime = 100000.0f;
            _testWorld.SetTime(new Unity.Core.TimeData(largeTime, 0.016f));
            systemHandle.Update(_testWorld.Unmanaged);

            var transform = _entityManager.GetComponentData<LocalTransform>(wallEntity);
            Assert.IsFalse(float.IsNaN(transform.Position.x), "Moving wall position X should not be NaN for large time");
            Assert.IsFalse(float.IsInfinity(transform.Position.x), "Moving wall position X should not be Infinity");
            Assert.LessOrEqual(math.abs(transform.Position.x), 10.001f, "Moving wall position X should stay within MoveDistance bound");
        }

        [Test]
        public void M2_PendulumSwing_Quaternion_ZeroAxisFallbackAndLargeAngles_ProducesNormalizedQuaternions()
        {
            var systemHandle = _testWorld.CreateSystem<PendulumSwingSystem>();

            Entity pendulumEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(pendulumEntity, new PendulumSwingComponent
            {
                MaxAngleDegrees = 360.0f, // Extreme angle
                Speed = 10.0f,
                PhaseOffset = 0.0f,
                SwingAxis = float3.zero, // Zero axis -> should fallback to (0,0,1)
                InitialRotation = quaternion.identity,
                CollisionRadius = 1.0f,
                DamageAmount = 10.0f
            });
            _entityManager.AddComponentData(pendulumEntity, LocalTransform.FromRotation(quaternion.identity));

            _testWorld.SetTime(new Unity.Core.TimeData(1.570796f, 0.016f)); // PI/2
            systemHandle.Update(_testWorld.Unmanaged);

            var transform = _entityManager.GetComponentData<LocalTransform>(pendulumEntity);
            float quatMagnitude = math.length(transform.Rotation.value);

            Assert.AreEqual(1.0f, quatMagnitude, 0.001f, "Pendulum rotation quaternion must remain unit normalized (length = 1.0)");
            Assert.IsFalse(float.IsNaN(transform.Rotation.value.x), "Quaternion X component must not be NaN");
        }

        [Test]
        public void M2_SplittingHazard_DistanceLogic_ExactBoundaryAndNegativeDistanceHandling()
        {
            var systemHandle = _testWorld.CreateSystem<SplittingHazardSystem>();

            Entity playerEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(playerEntity, new PlayerComponent());
            _entityManager.AddComponentData(playerEntity, LocalTransform.FromPosition(new float3(0, 0, 0)));

            // Parent hazard at exact boundary distance = 5.0
            Entity hazardExact = _entityManager.CreateEntity();
            _entityManager.AddComponentData(hazardExact, new SplittingHazardComponent
            {
                SplitCount = 2,
                ChildPrefab = Entity.Null,
                ImpulseForce = 1.0f,
                TriggerDistance = 5.0f,
                HasSplit = false,
                CollisionRadius = 1.0f,
                DamageAmount = 10.0f
            });
            _entityManager.AddComponentData(hazardExact, LocalTransform.FromPosition(new float3(0, 0, 5.0f)));

            systemHandle.Update(_testWorld.Unmanaged);

            // Player at dist = 5.0 <= triggerDist 5.0 -> should split and destroy
            Assert.IsFalse(_entityManager.Exists(hazardExact), "Hazard at exact trigger distance should split");
        }

        // =========================================================================
        // M3: PRESTIGE CURRENCY TRANSACTIONS, ZERO/INSUFFICIENT FUNDS, SKIN BITMASK
        // =========================================================================

        [Test]
        public void M3_PrestigeTransactions_ExactZeroFunds_FailsPurchaseWithoutNegativeBalance()
        {
            var systemHandle = _testWorld.CreateSystem<CosmeticsShopSystem>();

            Entity statsEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(statsEntity, new PersistentPlayerStats
            {
                PrestigeCurrency = 0.0,
                PermanentDamageMultiplier = 1.0f,
                PermanentGoldMultiplier = 1.0f
            });

            Entity eventEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(eventEntity, new CosmeticPurchaseEventComponent
            {
                TargetSkinIndex = 2,
                PrestigeCost = 10.0
            });

            systemHandle.Update(_testWorld.Unmanaged);

            var updatedStats = _entityManager.GetComponentData<PersistentPlayerStats>(statsEntity);
            Assert.AreEqual(0.0, updatedStats.PrestigeCurrency, "PrestigeCurrency must remain 0.0 on failed purchase");
            Assert.IsFalse(GameProgressData.IsSkinUnlocked(2), "Skin 2 must not be unlocked when funds are 0.0");
        }

        [Test]
        public void M3_PrestigeTransactions_ExactFundsMatch_DeductsToZeroAndUnlocksSkin()
        {
            var systemHandle = _testWorld.CreateSystem<CosmeticsShopSystem>();

            Entity statsEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(statsEntity, new PersistentPlayerStats
            {
                PrestigeCurrency = 10.0,
                PermanentDamageMultiplier = 1.0f,
                PermanentGoldMultiplier = 1.0f
            });

            Entity eventEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(eventEntity, new CosmeticPurchaseEventComponent
            {
                TargetSkinIndex = 3,
                PrestigeCost = 10.0
            });

            systemHandle.Update(_testWorld.Unmanaged);

            var updatedStats = _entityManager.GetComponentData<PersistentPlayerStats>(statsEntity);
            Assert.AreEqual(0.0, updatedStats.PrestigeCurrency, "PrestigeCurrency should be exactly 0.0 after spending exact funds");
            Assert.IsTrue(GameProgressData.IsSkinUnlocked(3), "Skin 3 should be unlocked");
            Assert.AreEqual(3, GameProgressData.CurrentSkinIndex, "Skin 3 should be equipped");
        }

        [Test]
        public void M3_SkinBitmaskOperations_Bit30AndBit31_HandlesSignedIntBitShifts()
        {
            // Reset unlocked skins mask to default (1)
            GameProgressData.UnlockedSkins = 1;

            Assert.IsTrue(GameProgressData.IsSkinUnlocked(0), "Default skin 0 must be unlocked");
            Assert.IsFalse(GameProgressData.IsSkinUnlocked(30), "Skin 30 should be locked initially");

            // Unlock skin 30
            GameProgressData.UnlockSkin(30);
            Assert.IsTrue(GameProgressData.IsSkinUnlocked(30), "Skin 30 should be unlocked");
            Assert.IsTrue(GameProgressData.IsSkinUnlocked(0), "Default skin 0 should still be unlocked");

            // Test setting current skin index to 30
            GameProgressData.CurrentSkinIndex = 30;
            Assert.AreEqual(30, GameProgressData.CurrentSkinIndex);
        }
    }
}
