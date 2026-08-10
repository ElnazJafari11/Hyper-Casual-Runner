using NUnit.Framework;
using Unity.Entities;
using UnityEngine;
using HyperCasualRunner.ECS.Components;
using HyperCasualRunner.ECS.Systems;

namespace HyperCasualRunner.Tests
{
    [TestFixture]
    public class CosmeticsShopTests
    {
        private World testWorld;
        private EntityManager entityManager;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey("HCR_SkinIndex");
            PlayerPrefs.DeleteKey("HCR_UnlockedSkins");
            PlayerPrefs.Save();

            testWorld = new World("CosmeticsShopTestWorld");
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

            PlayerPrefs.DeleteKey("HCR_SkinIndex");
            PlayerPrefs.DeleteKey("HCR_UnlockedSkins");
            PlayerPrefs.Save();
        }

        [Test]
        public void CosmeticPurchaseEventComponent_LayoutAndSize_MatchesSpecifications()
        {
            // Entity(8) + int(4) + pad(4) + double(8) = 24
            Assert.AreEqual(24, System.Runtime.InteropServices.Marshal.SizeOf(typeof(CosmeticPurchaseEventComponent)), "CosmeticPurchaseEventComponent size mismatch");
            Assert.AreEqual(24, Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<CosmeticPurchaseEventComponent>(), "CosmeticPurchaseEventComponent native size mismatch");
        }

        [Test]
        public void CosmeticsShopSystem_SufficientPrestige_DeductsPrestigeCurrencyAndUnlocksSkin()
        {
            var systemHandle = testWorld.CreateSystem<CosmeticsShopSystem>();

            // Create Player Stats Entity with 20 PrestigeCurrency
            Entity statsEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(statsEntity, new PersistentPlayerStats
            {
                PrestigeCurrency = 20.0,
                PermanentDamageMultiplier = 1.0f,
                PermanentGoldMultiplier = 1.0f
            });

            // Create Cosmetic Purchase Event Entity (Skin 1 for 5 Prestige)
            Entity eventEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(eventEntity, new CosmeticPurchaseEventComponent
            {
                TargetSkinIndex = 1,
                PrestigeCost = 5.0
            });

            systemHandle.Update(testWorld.Unmanaged);

            // Assert PrestigeCurrency was reduced by 5.0 (20.0 -> 15.0)
            var updatedStats = entityManager.GetComponentData<PersistentPlayerStats>(statsEntity);
            Assert.AreEqual(15.0, updatedStats.PrestigeCurrency, "PrestigeCurrency should be reduced to 15.0");

            // Assert skin 1 is unlocked in GameProgressData
            Assert.IsTrue(GameProgressData.IsSkinUnlocked(1), "Skin 1 should be unlocked");

            // Assert skin 1 is currently equipped
            Assert.AreEqual(1, GameProgressData.CurrentSkinIndex, "Current skin index should be 1");
        }

        [Test]
        public void CosmeticsShopSystem_InsufficientPrestige_FailsPurchaseAndPreservesCurrency()
        {
            var systemHandle = testWorld.CreateSystem<CosmeticsShopSystem>();

            // Create Player Stats Entity with only 3 PrestigeCurrency
            Entity statsEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(statsEntity, new PersistentPlayerStats
            {
                PrestigeCurrency = 3.0,
                PermanentDamageMultiplier = 1.0f,
                PermanentGoldMultiplier = 1.0f
            });

            // Create Cosmetic Purchase Event Entity (Skin 1 for 5 Prestige)
            Entity eventEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(eventEntity, new CosmeticPurchaseEventComponent
            {
                TargetSkinIndex = 1,
                PrestigeCost = 5.0
            });

            systemHandle.Update(testWorld.Unmanaged);

            // Assert PrestigeCurrency remains unchanged (3.0)
            var updatedStats = entityManager.GetComponentData<PersistentPlayerStats>(statsEntity);
            Assert.AreEqual(3.0, updatedStats.PrestigeCurrency, "PrestigeCurrency should remain 3.0 when funds are insufficient");

            // Assert skin 1 is NOT unlocked
            Assert.IsFalse(GameProgressData.IsSkinUnlocked(1), "Skin 1 should not be unlocked when funds are insufficient");

            // Assert current skin index remains 0
            Assert.AreEqual(0, GameProgressData.CurrentSkinIndex, "Current skin index should remain default (0)");
        }

        [Test]
        public void CosmeticsShopSystem_AlreadyUnlockedSkin_EquipsWithoutDeductingCurrency()
        {
            var systemHandle = testWorld.CreateSystem<CosmeticsShopSystem>();

            // Unlock skin 1 first
            GameProgressData.UnlockSkin(1);
            GameProgressData.CurrentSkinIndex = 0;

            // Create Player Stats Entity with 20 PrestigeCurrency
            Entity statsEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(statsEntity, new PersistentPlayerStats
            {
                PrestigeCurrency = 20.0,
                PermanentDamageMultiplier = 1.0f,
                PermanentGoldMultiplier = 1.0f
            });

            // Create Event Entity to equip skin 1
            Entity eventEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(eventEntity, new CosmeticPurchaseEventComponent
            {
                TargetSkinIndex = 1,
                PrestigeCost = 5.0
            });

            systemHandle.Update(testWorld.Unmanaged);

            // Assert PrestigeCurrency was NOT deducted again
            var updatedStats = entityManager.GetComponentData<PersistentPlayerStats>(statsEntity);
            Assert.AreEqual(20.0, updatedStats.PrestigeCurrency, "PrestigeCurrency should not be deducted for an already unlocked skin");

            // Assert current skin index is now 1
            Assert.AreEqual(1, GameProgressData.CurrentSkinIndex, "Skin 1 should be equipped");
        }

        [Test]
        public void CosmeticsShopSystem_TargetSlice_SpendsOwningSliceOnly()
        {
            var systemHandle = testWorld.CreateSystem<CosmeticsShopSystem>();

            Entity sliceA = entityManager.CreateEntity();
            entityManager.AddComponentData(sliceA, new IdleSliceState
            {
                Archetype = IdleArchetype.CookieClicker,
                PrestigeCurrency = 20,
                GlobalMultiplier = 1f,
                ClickPower = 1,
                EnergyPool = 50
            });
            entityManager.AddComponentData(sliceA, new PersistentPlayerStats
            {
                PrestigeCurrency = 20,
                PermanentDamageMultiplier = 1f,
                PermanentGoldMultiplier = 1f
            });

            Entity sliceB = entityManager.CreateEntity();
            entityManager.AddComponentData(sliceB, new IdleSliceState
            {
                Archetype = IdleArchetype.CookieClicker,
                PrestigeCurrency = 50,
                GlobalMultiplier = 1f,
                ClickPower = 1,
                EnergyPool = 50
            });
            entityManager.AddComponentData(sliceB, new PersistentPlayerStats
            {
                PrestigeCurrency = 50,
                PermanentDamageMultiplier = 1f,
                PermanentGoldMultiplier = 1f
            });

            Entity eventEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(eventEntity, new CosmeticPurchaseEventComponent
            {
                TargetSlice = sliceB,
                TargetSkinIndex = 1,
                PrestigeCost = 5.0
            });

            systemHandle.Update(testWorld.Unmanaged);

            Assert.AreEqual(20.0, entityManager.GetComponentData<IdleSliceState>(sliceA).PrestigeCurrency, 0.001);
            Assert.AreEqual(45.0, entityManager.GetComponentData<IdleSliceState>(sliceB).PrestigeCurrency, 0.001);
            Assert.AreEqual(45.0, entityManager.GetComponentData<PersistentPlayerStats>(sliceB).PrestigeCurrency, 0.001);
            Assert.IsTrue(GameProgressData.IsSkinUnlocked(1));
        }

        [Test]
        public void GameProgressData_SkinBitmaskUnlocking_WorksCorrectly()
        {
            Assert.IsTrue(GameProgressData.IsSkinUnlocked(0), "Default skin 0 should be unlocked by default");
            Assert.IsFalse(GameProgressData.IsSkinUnlocked(2), "Skin 2 should be locked initially");

            GameProgressData.UnlockSkin(2);
            Assert.IsTrue(GameProgressData.IsSkinUnlocked(2), "Skin 2 should be unlocked after calling UnlockSkin(2)");

            GameProgressData.CurrentSkinIndex = 2;
            Assert.AreEqual(2, GameProgressData.CurrentSkinIndex, "CurrentSkinIndex should reflect the set value");
        }
    }
}
