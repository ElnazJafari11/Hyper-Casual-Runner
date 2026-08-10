using System;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.Entities;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEditor;
using HyperCasualRunner;
using HyperCasualRunner.UI;
using HyperCasualRunner.ECS.Authoring;
using HyperCasualRunner.ECS.Components;
using HyperCasualRunner.ECS.Systems;

namespace HyperCasualRunner.Tests
{
    /// <summary>
    /// EditMode HUD surface smoke (UI2-02 / UI3-04 / UI4-04 / UI5-04 / UI6-04): named Actions/Cosmetics tabs + skin buttons
    /// without Enter Play Mode. UI4-06 wiring + UI5-06 shop-outcome bridge + R5 LoM Farm hide while
    /// UI6-01 Play Mode cosmetics remains blocked without an HCR interactive editor on MCP.
    /// </summary>
    [TestFixture]
    public class IdleSliceHudSmokeTests
    {
        private World _world;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey("HCR_SkinIndex");
            PlayerPrefs.DeleteKey("HCR_UnlockedSkins");
            PlayerPrefs.Save();

            _world = new World("IdleSliceHudSmokeWorld");
            World.DefaultGameObjectInjectionWorld = _world;
        }

        [TearDown]
        public void TearDown()
        {
            if (_world != null && _world.IsCreated)
                _world.Dispose();
            if (World.DefaultGameObjectInjectionWorld == _world)
                World.DefaultGameObjectInjectionWorld = null;
            _world = null;

            PlayerPrefs.DeleteKey("HCR_SkinIndex");
            PlayerPrefs.DeleteKey("HCR_UnlockedSkins");
            PlayerPrefs.Save();
        }

        [Test]
        public void IdleSliceUIController_EnsureHudBuilt_CreatesNamedTabsAndSkinButtons()
        {
            var go = new GameObject("IdleSliceHudSmoke");
            try
            {
                var doc = go.AddComponent<UIDocument>();
                AssignPanelSettings(doc);

                var bootstrap = go.AddComponent<IdleSliceBootstrap>();
                bootstrap.DisplayName = "HUD Smoke Slice";
                bootstrap.HowToPlay = "EditMode smoke";
                bootstrap.Archetype = IdleArchetype.CookieClicker;

                var controller = go.AddComponent<IdleSliceUIController>();

                bool needsRuntimePanel = doc.panelSettings == null;
                if (needsRuntimePanel)
                {
                    // Runtime PanelSettings CreateInstance may Error about "Assets/UI Toolkit.meta"
                    // in batchmode; expect so named-element asserts still run.
                    LogAssert.Expect(LogType.Error, new Regex("UI Toolkit\\.meta"));
                }

                controller.EnsureHudBuilt();

                // Swallow optional follow-up write failure if Unity emitted a second Error.
                LogAssert.ignoreFailingMessages = true;
                var root = controller.RootVisualElement;
                LogAssert.ignoreFailingMessages = false;
                Assert.IsNotNull(root, "UIDocument rootVisualElement should exist after PanelSettings.");

                Assert.IsNotNull(root.Q<Button>("ActionsTabButton"), "ActionsTabButton missing");
                Assert.IsNotNull(root.Q<Button>("CosmeticsTabButton"), "CosmeticsTabButton missing");
                Assert.IsNotNull(root.Q<Button>("EquipSkin0Button"), "EquipSkin0Button missing");
                Assert.IsNotNull(root.Q<Button>("BuySkin1Button"), "BuySkin1Button missing");
                Assert.IsNotNull(root.Q<Button>("BuySkin2Button"), "BuySkin2Button missing");
                Assert.IsNotNull(root.Q<Button>("BuySkin3Button"), "BuySkin3Button missing");

                Assert.IsNotNull(root.Q<VisualElement>("ActionsContainer"), "ActionsContainer missing");
                Assert.IsNotNull(root.Q<VisualElement>("CosmeticsContainer"), "CosmeticsContainer missing");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// UI4-06: Cosmetics tab show + BuySkin1 click emits CosmeticPurchaseEventComponent.
        /// Substitute while UI4-01 Play Mode cosmetics smoke is blocked (no HCR interactive MCP).
        /// </summary>
        [Test]
        public void IdleSliceUIController_CosmeticsBuy_EmitsCosmeticPurchaseEvent()
        {
            var go = new GameObject("IdleSliceHudCosmeticsWiring");
            try
            {
                var doc = go.AddComponent<UIDocument>();
                AssignPanelSettings(doc);

                var bootstrap = go.AddComponent<IdleSliceBootstrap>();
                bootstrap.DisplayName = "Cosmetics Wiring Slice";
                bootstrap.HowToPlay = "UI4-06 EditMode wiring";
                bootstrap.Archetype = IdleArchetype.CookieClicker;

                var controller = go.AddComponent<IdleSliceUIController>();

                bool needsRuntimePanel = doc.panelSettings == null;
                if (needsRuntimePanel)
                    LogAssert.Expect(LogType.Error, new Regex("UI Toolkit\\.meta"));

                controller.EnsureHudBuilt();

                LogAssert.ignoreFailingMessages = true;
                var root = controller.RootVisualElement;
                LogAssert.ignoreFailingMessages = false;
                Assert.IsNotNull(root, "UIDocument rootVisualElement should exist after PanelSettings.");

                var cosmeticsTab = root.Q<Button>("CosmeticsTabButton");
                Assert.IsNotNull(cosmeticsTab, "CosmeticsTabButton missing");
                SimulateClick(cosmeticsTab);

                var cosmetics = root.Q<VisualElement>("CosmeticsContainer");
                Assert.IsNotNull(cosmetics, "CosmeticsContainer missing");
                Assert.AreEqual(DisplayStyle.Flex, cosmetics.style.display.value,
                    "Cosmetics tab click should show CosmeticsContainer");

                using (var before = _world.EntityManager.CreateEntityQuery(typeof(CosmeticPurchaseEventComponent)))
                    Assert.AreEqual(0, before.CalculateEntityCount(), "No cosmetic events before buy click");

                var buy = root.Q<Button>("BuySkin1Button");
                Assert.IsNotNull(buy, "BuySkin1Button missing");
                SimulateClick(buy);

                using var q = _world.EntityManager.CreateEntityQuery(typeof(CosmeticPurchaseEventComponent));
                Assert.AreEqual(1, q.CalculateEntityCount(),
                    "BuySkin1 click should create CosmeticPurchaseEventComponent");
                // Enableable IComponentData cannot use GetSingleton — read via array.
                var arr = q.ToComponentDataArray<CosmeticPurchaseEventComponent>(Unity.Collections.Allocator.Temp);
                try
                {
                    Assert.AreEqual(1, arr.Length);
                    Assert.AreEqual(1, arr[0].TargetSkinIndex, "TargetSkinIndex should be skin 1 (Crimson Red)");
                    Assert.AreEqual(5.0, arr[0].PrestigeCost, "PrestigeCost should match SkinRows cost for skin 1");
                }
                finally
                {
                    arr.Dispose();
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// UI5-06: while UI6-01 Play Mode cosmetics is blocked — fund prestige, Cosmetics buy click,
        /// tick CosmeticsShopSystem, assert prestige debit + GameProgressData.CurrentSkinIndex.
        /// </summary>
        [Test]
        public void IdleSliceUIController_CosmeticsBuy_ShopSpendsPrestigeAndSetsCurrentSkin()
        {
            PlayerPrefs.DeleteKey("HCR_SkinIndex");
            PlayerPrefs.DeleteKey("HCR_UnlockedSkins");
            PlayerPrefs.Save();

            var em = _world.EntityManager;
            var slice = em.CreateEntity();
            em.AddComponentData(slice, new IdleSliceState
            {
                Archetype = IdleArchetype.CookieClicker,
                PrestigeCurrency = 20,
                GlobalMultiplier = 1f,
                ClickPower = 1,
                EnergyPool = 50
            });
            em.AddComponentData(slice, new PersistentPlayerStats
            {
                PrestigeCurrency = 20,
                PermanentDamageMultiplier = 1f,
                PermanentGoldMultiplier = 1f
            });

            var shop = _world.CreateSystem<CosmeticsShopSystem>();

            var go = new GameObject("IdleSliceHudCosmeticsShopOutcome");
            try
            {
                var doc = go.AddComponent<UIDocument>();
                AssignPanelSettings(doc);

                var bootstrap = go.AddComponent<IdleSliceBootstrap>();
                bootstrap.DisplayName = "Cosmetics Shop Outcome";
                bootstrap.HowToPlay = "UI5-06 EditMode shop bridge";
                bootstrap.Archetype = IdleArchetype.CookieClicker;
                bootstrap.LoadPersistedProgress = false;

                var controller = go.AddComponent<IdleSliceUIController>();

                bool needsRuntimePanel = doc.panelSettings == null;
                if (needsRuntimePanel)
                    LogAssert.Expect(LogType.Error, new Regex("UI Toolkit\\.meta"));

                controller.EnsureHudBuilt();

                LogAssert.ignoreFailingMessages = true;
                var root = controller.RootVisualElement;
                LogAssert.ignoreFailingMessages = false;
                Assert.IsNotNull(root, "UIDocument rootVisualElement should exist after PanelSettings.");

                SimulateClick(root.Q<Button>("CosmeticsTabButton"));
                Assert.AreEqual(DisplayStyle.Flex,
                    root.Q<VisualElement>("CosmeticsContainer").style.display.value,
                    "Cosmetics tab should show before buy");

                Assert.AreEqual(0, GameProgressData.CurrentSkinIndex, "Skin index starts at 0");
                Assert.IsFalse(GameProgressData.IsSkinUnlocked(1), "Skin 1 locked before buy");

                SimulateClick(root.Q<Button>("BuySkin1Button"));

                using (var q = em.CreateEntityQuery(typeof(CosmeticPurchaseEventComponent)))
                {
                    Assert.AreEqual(1, q.CalculateEntityCount(), "Buy should emit CosmeticPurchaseEventComponent");
                    var arr = q.ToComponentDataArray<CosmeticPurchaseEventComponent>(Unity.Collections.Allocator.Temp);
                    try
                    {
                        Assert.AreEqual(1, arr[0].TargetSkinIndex);
                        Assert.AreEqual(5.0, arr[0].PrestigeCost);
                        Assert.AreEqual(slice, arr[0].TargetSlice,
                            "Sole IdleSliceState should resolve as TargetSlice");
                    }
                    finally
                    {
                        arr.Dispose();
                    }
                }

                shop.Update(_world.Unmanaged);

                Assert.AreEqual(15.0, em.GetComponentData<IdleSliceState>(slice).PrestigeCurrency, 0.001,
                    "Shop should debit skin-1 cost (5) from funded prestige (20)");
                Assert.AreEqual(15.0, em.GetComponentData<PersistentPlayerStats>(slice).PrestigeCurrency, 0.001,
                    "PersistentPlayerStats prestige should stay synced");
                Assert.IsTrue(GameProgressData.IsSkinUnlocked(1), "Skin 1 unlocked after shop tick");
                Assert.AreEqual(1, GameProgressData.CurrentSkinIndex,
                    "CurrentSkinIndex should match purchased skin after shop tick");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                PlayerPrefs.DeleteKey("HCR_SkinIndex");
                PlayerPrefs.DeleteKey("HCR_UnlockedSkins");
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// R5-F1: CH/TT2/IH stats must show HeroDps, TapDamage, and true IdleCombatState.Zone
        /// (not Stage-inflated ProgressionLevel alone).
        /// </summary>
        [TestCase(IdleArchetype.ClickerHeroes)]
        [TestCase(IdleArchetype.TapTitans2)]
        [TestCase(IdleArchetype.IdleHeroes)]
        public void IdleSliceUIController_CombatHud_ShowsHeroDpsTapDamageTrueZone(IdleArchetype arch)
        {
            var em = _world.EntityManager;
            var slice = em.CreateEntity();
            em.AddComponentData(slice, new IdleSliceState
            {
                Archetype = arch,
                PrimaryCurrency = 10,
                PrestigeCurrency = 0,
                GlobalMultiplier = 1f,
                ProgressionLevel = 8, // Stage-inflated — HUD Zone must still read combat.Zone=5
                ClickPower = 5,
                PassiveRate = 2,
                OwnedGenerators = 1,
                EnemyHp = 20,
                EnemyMaxHp = 20,
                MaxWorkers = 5
            });
            em.AddComponentData(slice, new IdleCombatState
            {
                TapDamage = 4.5,
                HeroDps = 12.0,
                Zone = 5,
                GoldPerKill = 15,
                EnemyHp = 40,
                EnemyMaxHp = 145
            });

            var go = new GameObject("IdleSliceHudCombat_" + arch);
            try
            {
                var doc = go.AddComponent<UIDocument>();
                AssignPanelSettings(doc);

                var bootstrap = go.AddComponent<IdleSliceBootstrap>();
                bootstrap.DisplayName = "Combat HUD " + arch;
                bootstrap.HowToPlay = "R5-F1 EditMode";
                bootstrap.Archetype = arch;
                bootstrap.LoadPersistedProgress = false;

                var controller = go.AddComponent<IdleSliceUIController>();

                bool needsRuntimePanel = doc.panelSettings == null;
                if (needsRuntimePanel)
                    LogAssert.Expect(LogType.Error, new Regex("UI Toolkit\\.meta"));

                controller.EnsureHudBuilt();

                LogAssert.ignoreFailingMessages = true;
                var refresh = typeof(IdleSliceUIController).GetMethod(
                    "RefreshStats",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(refresh, "RefreshStats must exist for EditMode invoke");
                refresh.Invoke(controller, null);

                var root = controller.RootVisualElement;
                LogAssert.ignoreFailingMessages = false;
                Assert.IsNotNull(root, "UIDocument rootVisualElement should exist after PanelSettings.");

                var stats = root.Q<Label>("StatsLabel");
                Assert.IsNotNull(stats, "StatsLabel missing");
                string text = stats.text ?? "";

                StringAssert.Contains("HeroDps: 12.0", text, arch + " HUD must show HeroDps");
                StringAssert.Contains("TapDamage: 4.5", text, arch + " HUD must show TapDamage");
                StringAssert.Contains("Zone: 5", text, arch + " HUD must show true combat Zone");
                StringAssert.Contains("Lv/Zone: 5", text,
                    arch + " Lv/Zone label must prefer IdleCombatState.Zone over ProgressionLevel=8");
                StringAssert.DoesNotContain("Lv/Zone: 8", text,
                    arch + " must not display Stage-inflated Level as Zone when combat SoT present");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// R5 gacha P1: LoM Farm Stage Gold hides once Stage≥1 (single-verb after auto-lamp unlock).
        /// </summary>
        [Test]
        public void LegendOfMushroom_FarmButton_HidesAfterStageUnlock()
        {
            GameProgressData.ClearIdleSlice((int)IdleArchetype.LegendOfMushroom);
            var go = new GameObject("LoM_FarmHide_HudSmoke");
            try
            {
                var doc = go.AddComponent<UIDocument>();
                AssignPanelSettings(doc);

                var bootstrap = go.AddComponent<IdleSliceBootstrap>();
                bootstrap.DisplayName = "LoM Farm Hide";
                bootstrap.HowToPlay = "Rub Lamp";
                bootstrap.Archetype = IdleArchetype.LegendOfMushroom;
                bootstrap.LoadPersistedProgress = false;
                bootstrap.StartingCurrency = 20;

                var trySpawn = typeof(IdleSliceBootstrap).GetMethod(
                    "TrySpawn",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(trySpawn, "TrySpawn must exist");
                trySpawn.Invoke(bootstrap, null);
                Assert.IsTrue(bootstrap.IsSpawned, "LoM bootstrap must spawn");

                var controller = go.AddComponent<IdleSliceUIController>();
                if (doc.panelSettings == null)
                    LogAssert.Expect(LogType.Error, new Regex("UI Toolkit\\.meta"));

                controller.EnsureHudBuilt();

                LogAssert.ignoreFailingMessages = true;
                var root = controller.RootVisualElement;
                LogAssert.ignoreFailingMessages = false;
                Assert.IsNotNull(root);

                var farm = root.Q<Button>("FarmStageGoldButton");
                Assert.IsNotNull(farm, "FarmStageGoldButton should exist at Stage 0");

                var refresh = typeof(IdleSliceUIController).GetMethod(
                    "RefreshStats",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(refresh, "RefreshStats must exist");
                refresh.Invoke(controller, null);
                Assert.AreEqual(DisplayStyle.Flex, farm.style.display.value,
                    "Farm should stay visible before Stage≥1");

                var em = _world.EntityManager;
                Assert.IsTrue(em.HasComponent<IdleGachaState>(bootstrap.SliceEntity));
                var g = em.GetComponentData<IdleGachaState>(bootstrap.SliceEntity);
                g.Stage = 1;
                em.SetComponentData(bootstrap.SliceEntity, g);

                refresh.Invoke(controller, null);
                Assert.AreEqual(DisplayStyle.None, farm.style.display.value,
                    "Farm must hide at Stage≥1 (single-verb UI)");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                GameProgressData.ClearIdleSlice((int)IdleArchetype.LegendOfMushroom);
            }
        }

        /// <summary>
        /// EditMode panels often do not route ClickEvent through Clickable; invoke clicked subscribers directly.
        /// </summary>
        private static void SimulateClick(Button button)
        {
            Assert.IsNotNull(button, "Button null");
            var clickable = button.clickable;
            Assert.IsNotNull(clickable, "Button.clickable missing on " + button.name);

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var field = typeof(Clickable).GetField("clicked", flags);
            Assert.IsNotNull(field, "Clickable.clicked backing field missing (Unity API change?)");
            var del = field.GetValue(clickable) as Action;
            Assert.IsNotNull(del, "No clicked subscribers on " + button.name);
            del.Invoke();
        }

        private static void AssignPanelSettings(UIDocument doc)
        {
            if (doc.panelSettings != null) return;

            string[] guids = AssetDatabase.FindAssets("t:PanelSettings");
            if (guids != null && guids.Length > 0)
            {
                var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(
                    AssetDatabase.GUIDToAssetPath(guids[0]));
                if (existing != null)
                {
                    doc.panelSettings = existing;
                    return;
                }
            }

            // No project PanelSettings asset — leave null; EnsureHudBuilt creates a runtime one.
        }
    }
}
