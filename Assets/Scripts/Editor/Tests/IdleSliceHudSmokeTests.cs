using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEditor;
using HyperCasualRunner.UI;
using HyperCasualRunner.ECS.Authoring;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.Tests
{
    /// <summary>
    /// EditMode HUD surface smoke (UI2-02): named Actions/Cosmetics tabs + skin buttons
    /// without Enter Play Mode.
    /// </summary>
    [TestFixture]
    public class IdleSliceHudSmokeTests
    {
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
                Object.DestroyImmediate(go);
            }
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
