using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using HyperCasualRunner.ECS.Authoring;
using HyperCasualRunner.ECS.Components;
using HyperCasualRunner.UI;

namespace HyperCasualRunner.Editor
{
    /// <summary>Generates playable idle MVP prefabs under Assets/ToolkitExamples/Idle/.</summary>
    public static class IdleToolkitSliceGenerator
    {
        private struct SliceDef
        {
            public string FileName;
            public IdleArchetype Archetype;
            public string DisplayName;
            public string HowTo;
            public double StartCurrency;
            public double ClickPower;
            public bool RequiresManager;
            public double ManagerCost;
        }

        private static readonly SliceDef[] Slices =
        {
            new SliceDef { FileName = "01_CookieClicker_Generators", Archetype = IdleArchetype.CookieClicker, DisplayName = "Cookie Clicker", HowTo = "Click cookies → buy generators (CPS) → prestige for heavenly mult.", StartCurrency = 0, ClickPower = 1 },
            new SliceDef { FileName = "02_ClickerHeroes_TapKill", Archetype = IdleArchetype.ClickerHeroes, DisplayName = "Clicker Heroes", HowTo = "Tap to kill → zone gold → buy hero DPS → prestige souls.", StartCurrency = 0, ClickPower = 5 },
            new SliceDef { FileName = "03_AdventureCapitalist_Managers", Archetype = IdleArchetype.AdventureCapitalist, DisplayName = "AdVenture Capitalist", HowTo = "Buy businesses → hire manager to automate → angel investor reset.", StartCurrency = 25, ClickPower = 0, RequiresManager = true, ManagerCost = 100 },
            new SliceDef { FileName = "04_UniversalPaperclips_Phase", Archetype = IdleArchetype.UniversalPaperclips, DisplayName = "Universal Paperclips", HowTo = "Make clips → buy autoclippers → phase-shift replaces the loop.", StartCurrency = 0, ClickPower = 1 },
            new SliceDef { FileName = "05_ADarkRoom_Narrative", Archetype = IdleArchetype.ADarkRoom, DisplayName = "A Dark Room", HowTo = "Stoke fire (x5) → Explore unlocks → Craft spends wood for passive.", StartCurrency = 0, ClickPower = 1 },
            new SliceDef { FileName = "06_AntimatterDimensions_Layers", Archetype = IdleArchetype.AntimatterDimensions, DisplayName = "Antimatter Dimensions", HowTo = "Buy dimensions → click antimatter → prestige into a higher layer.", StartCurrency = 10, ClickPower = 1 },
            new SliceDef { FileName = "07_RealmGrinder_Factions", Archetype = IdleArchetype.RealmGrinder, DisplayName = "Realm Grinder", HowTo = "Build income → pick Good/Evil faction for a different mult path.", StartCurrency = 20, ClickPower = 1 },
            new SliceDef { FileName = "08_NGUIdle_Energy", Archetype = IdleArchetype.NguIdle, DisplayName = "NGU Idle", HowTo = "Allocate energy to train → gain XP/levels → rebirth keeps mult.", StartCurrency = 0, ClickPower = 1 },
            new SliceDef { FileName = "09_MelvorIdle_Skills", Archetype = IdleArchetype.MelvorIdle, DisplayName = "Melvor Idle", HowTo = "Skills tick offline/online → claim offline → level skills linearly.", StartCurrency = 0, ClickPower = 1 },
            new SliceDef { FileName = "10_EggInc_Hatch", Archetype = IdleArchetype.EggInc, DisplayName = "Egg, Inc.", HowTo = "Hatch tap-burst → upgrade habitats (CPS) → soul egg prestige.", StartCurrency = 0, ClickPower = 2 },
            new SliceDef { FileName = "11_IdleMinerTycoon_Shafts", Archetype = IdleArchetype.IdleMinerTycoon, DisplayName = "Idle Miner Tycoon", HowTo = "Upgrade shaft → hire super-manager → prestige a new mine.", StartCurrency = 30, ClickPower = 0, RequiresManager = true, ManagerCost = 150 },
            new SliceDef { FileName = "12_TapTitans2_TapDps", Archetype = IdleArchetype.TapTitans2, DisplayName = "Tap Titans 2", HowTo = "Tap + hero DPS clear zones → prestige relics for power.", StartCurrency = 0, ClickPower = 4 },
            new SliceDef { FileName = "13_IdleHeroes_GachaCombat", Archetype = IdleArchetype.IdleHeroes, DisplayName = "Idle Heroes", HowTo = "Auto-combat → gacha pull for power → claim AFK chest.", StartCurrency = 50, ClickPower = 2 },
            new SliceDef { FileName = "14_AFKArena_Chest", Archetype = IdleArchetype.AfkArena, DisplayName = "AFK Arena", HowTo = "Campaign drip fills AFK chest → open chest for offline fantasy.", StartCurrency = 0, ClickPower = 1 },
            new SliceDef { FileName = "15_LegendOfMushroom_Lamp", Archetype = IdleArchetype.LegendOfMushroom, DisplayName = "Legend of Mushroom", HowTo = "Rub lamp (gacha gear) → farm gold → stage progression.", StartCurrency = 30, ClickPower = 1 },
            new SliceDef { FileName = "16_CapybaraGo_Steps", Archetype = IdleArchetype.CapybaraGo, DisplayName = "Capybara Go!", HowTo = "Advance narrative steps → lucky finds → milestone mult beats.", StartCurrency = 0, ClickPower = 1 },
            new SliceDef { FileName = "17_CatsAndSoup_Assign", Archetype = IdleArchetype.CatsAndSoup, DisplayName = "Cats & Soup", HowTo = "Assign cats to stations → they cook passively → unassign freely.", StartCurrency = 0, ClickPower = 1 },
            new SliceDef { FileName = "18_NekoAtsume_CheckIn", Archetype = IdleArchetype.NekoAtsume, DisplayName = "Neko Atsume", HowTo = "Place food → wait for cats → check in to collect (zen idle).", StartCurrency = 20, ClickPower = 1 },
            new SliceDef { FileName = "19_FalloutShelter_Dwellers", Archetype = IdleArchetype.FalloutShelter, DisplayName = "Fallout Shelter", HowTo = "Assign dwellers to rooms → production ticks → claim output.", StartCurrency = 0, ClickPower = 1 },
        };

        [MenuItem("IdleToolkit/Generate All Idle MVP Slices")]
        public static void GenerateAll()
        {
            const string folder = "Assets/ToolkitExamples/Idle";
            EnsureFolder(folder);

            PanelSettings panel = FindPanelSettings();

            int created = 0;
            foreach (var def in Slices)
            {
                string path = $"{folder}/{def.FileName}_Slice.prefab";
                var root = new GameObject(def.FileName + "_Slice");

                var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "Ground";
                ground.transform.SetParent(root.transform);
                ground.transform.localScale = new Vector3(2f, 1f, 2f);

                var hub = new GameObject("IdleHub");
                hub.transform.SetParent(root.transform);

                var bootstrap = hub.AddComponent<IdleSliceBootstrap>();
                bootstrap.Archetype = def.Archetype;
                bootstrap.DisplayName = def.DisplayName;
                bootstrap.HowToPlay = def.HowTo;
                bootstrap.StartingCurrency = def.StartCurrency;
                bootstrap.ClickPower = def.ClickPower > 0 ? def.ClickPower : 1;
                bootstrap.GeneratorRequiresManager = def.RequiresManager;
                bootstrap.ManagerHireCost = def.ManagerCost > 0 ? def.ManagerCost : 100;
                bootstrap.GeneratorBaseCost = 15;
                bootstrap.GeneratorBaseCps = 1;
                bootstrap.MaxWorkers = 5;
                bootstrap.PullCost = 10;

                var uiDoc = hub.AddComponent<UIDocument>();
                if (panel != null) uiDoc.panelSettings = panel;
                hub.AddComponent<IdleSliceUIController>();
                hub.AddComponent<IdleSaveManager>();

                PrefabUtility.SaveAsPrefabAsset(root, path);
                Object.DestroyImmediate(root);
                created++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[IdleToolkit] Generated {created} idle MVP slices in {folder}");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string[] parts = path.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }

        private static PanelSettings FindPanelSettings()
        {
            string[] guids = AssetDatabase.FindAssets("t:PanelSettings");
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<PanelSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        [MenuItem("IdleToolkit/Smoke Batch A (instructions)")]
        public static void SmokeBatchA()
        {
            Debug.Log(
                "[IdleToolkit] Batch A smoke path:\n" +
                "1) IdleToolkit/Run Batch A Smoke And Exit  (or batchmode -executeMethod IdleBatchATestRunner.RunAndExit)\n" +
                "2) Prefabs: Assets/ToolkitExamples/Idle/01..06 (Cookie/ClickerHeroes/Adventure/Paperclips/Antimatter)\n" +
                "3) Play: drop prefab → Play → HUD buttons for core verb + progression beat.");
            EditorApplication.ExecuteMenuItem("Window/General/Test Runner");
        }

        [MenuItem("IdleToolkit/Run Batch A Smoke And Exit")]
        public static void RunBatchASmokeMenu() => IdleBatchATestRunner.RunAndExit();

        [MenuItem("IdleToolkit/Open Idle Prefab Folder")]
        public static void OpenIdleFolder()
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>("Assets/ToolkitExamples/Idle");
            if (obj != null)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
        }

        /// <summary>Human Play Mode checklist (MCP cannot play-smoke while editor is on another project).</summary>
        [MenuItem("IdleToolkit/Human Play-Smoke Instructions")]
        public static void HumanPlaySmokeInstructions()
        {
            Debug.Log(
                "[IdleToolkit] HUMAN PLAY-SMOKE (marks Playable in docs/idle-toolkit-progress.md):\n" +
                "STATUS: EditMode-verified 19/19 · Play Mode 0/19 (needs this project open in Unity).\n" +
                "1) Open D:\\Git\\Hyper-Casual-Runner in Unity 6000.5.5f1 (not pcg-toolkit).\n" +
                "2) IdleToolkit/Open Idle Prefab Folder → drag 01_CookieClicker_* into an empty scene.\n" +
                "3) Enter Play → Click Cookie → Buy Generator → Prestige; currency/CPS/prestige must move.\n" +
                "4) Repeat for Batch A: 02 ClickerHeroes, 03 AdventureCapitalist, 04 Paperclips, 06 Antimatter.\n" +
                "5) Optional Batch B/C: any other Idle/*_Slice.prefab — one core HUD verb + one progression beat.\n" +
                "6) Update docs/idle-toolkit-progress.md Play column to Playable for each smoked title.\n" +
                "EditMode (no Play): IdleToolkit/Run Batch A Smoke And Exit or RunAllIdleSmokeAndExit via batchmode.");
            OpenIdleFolder();
        }
    }
}
