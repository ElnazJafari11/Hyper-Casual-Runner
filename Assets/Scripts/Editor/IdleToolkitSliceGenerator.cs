using System.IO;
using System.Text;
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
        private const string ChecklistRelativePath = "docs/idle-play-smoke-checklist.md";

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
            new SliceDef { FileName = "15_LegendOfMushroom_Lamp", Archetype = IdleArchetype.LegendOfMushroom, DisplayName = "Legend of Mushroom", HowTo = "Rub lamp (gacha gear) → unlock auto-lamp at stage 1 → farm gold.", StartCurrency = 30, ClickPower = 1 },
            new SliceDef { FileName = "16_CapybaraGo_Steps", Archetype = IdleArchetype.CapybaraGo, DisplayName = "Capybara Go!", HowTo = "Advance narrative steps → lucky finds → milestone mult beats.", StartCurrency = 0, ClickPower = 1 },
            new SliceDef { FileName = "17_CatsAndSoup_Assign", Archetype = IdleArchetype.CatsAndSoup, DisplayName = "Cats & Soup", HowTo = "Assign cats to stations → they cook passively → unassign freely.", StartCurrency = 0, ClickPower = 1 },
            new SliceDef { FileName = "18_NekoAtsume_CheckIn", Archetype = IdleArchetype.NekoAtsume, DisplayName = "Neko Atsume", HowTo = "Place food/toys → wait for cats → check in clears cats and pays (zen idle).", StartCurrency = 20, ClickPower = 1 },
            new SliceDef { FileName = "19_FalloutShelter_Dwellers", Archetype = IdleArchetype.FalloutShelter, DisplayName = "Fallout Shelter", HowTo = "Assign dwellers → room ticks buffer Pending → Claim Production drains it.", StartCurrency = 0, ClickPower = 1 },
        };

        [MenuItem("IdleToolkit/MVP/Generate All Idle MVP Slices")]
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
            Debug.Log($"[IdleToolkit/MVP] Generated {created} idle MVP slices in {folder}");
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

        [MenuItem("IdleToolkit/MVP/Smoke Batch A (instructions)")]
        public static void SmokeBatchA()
        {
            Debug.Log(
                "[IdleToolkit/MVP] Batch A smoke path:\n" +
                "1) IdleToolkit/MVP/Run Batch A Smoke And Exit  (or batchmode -executeMethod IdleBatchATestRunner.RunAllIdleSmokeAndExit)\n" +
                "2) Prefabs Batch A: 01 CookieClicker, 02 ClickerHeroes, 03 AdventureCapitalist, 04 Paperclips, 06 Antimatter (05 is Batch B/C)\n" +
                "3) Play: drop prefab → Play → HUD buttons for core verb + progression beat.\n" +
                "Compose map: docs/idle-toolkit-compose.md");
            EditorApplication.ExecuteMenuItem("Window/General/Test Runner");
        }

        [MenuItem("IdleToolkit/MVP/Run Batch A Smoke And Exit")]
        public static void RunBatchASmokeMenu() => IdleBatchATestRunner.RunAndExit();

        [MenuItem("IdleToolkit/MVP/Open Idle Prefab Folder")]
        public static void OpenIdleFolder()
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>("Assets/ToolkitExamples/Idle");
            if (obj != null)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
        }

        /// <summary>Writes durable Play Mode checklist under docs/ (MCP cannot play-smoke while editor is on another project).</summary>
        [MenuItem("IdleToolkit/MVP/Human Play-Smoke Instructions")]
        public static void HumanPlaySmokeInstructions()
        {
            string absolutePath = WritePlaySmokeChecklist();
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string unityVersion = Application.unityVersion;

            Debug.Log(
                "[IdleToolkit/MVP] HUMAN PLAY-SMOKE checklist written to:\n" +
                absolutePath + "\n" +
                $"STATUS: EditMode-verified 19/19 · Play Mode 0/19 (needs THIS project open in Unity {unityVersion}).\n" +
                $"Project root: {projectRoot}\n" +
                "1) IdleToolkit/MVP/Open Idle Prefab Folder → drag 01_CookieClicker_* into an empty scene.\n" +
                "2) Enter Play → exercise expected HUD verbs (see checklist).\n" +
                "3) Repeat Batch A then optional B/C; only then mark Playable in docs/idle-toolkit-progress.md.\n" +
                "EditMode (no Play): IdleToolkit/MVP/Run Batch A Smoke And Exit.");

            OpenIdleFolder();
            var checklistAsset = AssetDatabase.LoadAssetAtPath<Object>(ChecklistRelativePath);
            if (checklistAsset != null)
            {
                Selection.activeObject = checklistAsset;
                EditorGUIUtility.PingObject(checklistAsset);
            }

            EditorUtility.DisplayDialog(
                "Idle MVP Play-Smoke",
                "Checklist written to:\n" + ChecklistRelativePath + "\n\n" +
                "Play Mode is still 0/19 until you smoke this project and update docs/idle-toolkit-progress.md.\n" +
                "Do not claim Playable from EditMode alone.",
                "OK");
        }

        private static string WritePlaySmokeChecklist()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string absolutePath = Path.Combine(projectRoot ?? ".", ChecklistRelativePath.Replace('/', Path.DirectorySeparatorChar));
            string dir = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var sb = new StringBuilder();
            sb.AppendLine("# Idle MVP — Human Play-Smoke Checklist");
            sb.AppendLine();
            sb.AppendLine($"Generated: {System.DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
            sb.AppendLine($"Unity: {Application.unityVersion}");
            sb.AppendLine($"Project: `{projectRoot}`");
            sb.AppendLine();
            sb.AppendLine("## Status (honest)");
            sb.AppendLine();
            sb.AppendLine("- EditMode-verified: **19/19** (`IdleBatchASmokeTests` + `IdleBatchBCSmokeTests`)");
            sb.AppendLine("- Play Mode: **0/19** until each row below is smoked in **this** project and `docs/idle-toolkit-progress.md` Play column is updated");
            sb.AppendLine("- Piece catalog: `docs/idle-toolkit-compose.md`");
            sb.AppendLine("- Menus: `IdleToolkit/MVP/…` (idle) vs `IdleToolkit/RunnerSlices/…` (21 runner slices)");
            sb.AppendLine();
            sb.AppendLine("## How to smoke one prefab");
            sb.AppendLine();
            sb.AppendLine("1. `IdleToolkit/MVP/Open Idle Prefab Folder`");
            sb.AppendLine("2. Drag the listed prefab into an empty scene");
            sb.AppendLine("3. Enter Play → press each **Expected HUD verbs** at least once");
            sb.AppendLine("4. Confirm HUD currency / CPS / prestige / level / workers move for that beat");
            sb.AppendLine("5. Exit Play → mark that game **Playable** in `docs/idle-toolkit-progress.md`");
            sb.AppendLine();
            sb.AppendLine("## Batch A (priority)");
            sb.AppendLine();
            sb.AppendLine("| Prefab | Expected HUD verbs | Core beat | Done? |");
            sb.AppendLine("|--------|--------------------|-----------|-------|");
            sb.AppendLine("| `01_CookieClicker_Generators_Slice` | Click Cookie → Buy Generator → Prestige | currency + gens + prestige | [ ] |");
            sb.AppendLine("| `02_ClickerHeroes_TapKill_Slice` | Tap / Attack → Buy Hero DPS → Prestige | zone / gold | [ ] |");
            sb.AppendLine("| `03_AdventureCapitalist_Managers_Slice` | Collect → Buy Business → Hire Manager → Angel Reset | manager automates | [ ] |");
            sb.AppendLine("| `04_UniversalPaperclips_Phase_Slice` | Make Paperclip → Buy Autoclipper → Phase Shift | phase prestige | [ ] |");
            sb.AppendLine("| `06_AntimatterDimensions_Layers_Slice` | Buy Dimension → Click Antimatter → Prestige Layer | owned + layer | [ ] |");
            sb.AppendLine();
            sb.AppendLine("Note: `05_ADarkRoom_*` is Batch B/C, not Batch A.");
            sb.AppendLine();
            sb.AppendLine("## Batch B/C (optional)");
            sb.AppendLine();
            sb.AppendLine("| Prefab | Expected HUD verbs | Done? |");
            sb.AppendLine("|--------|--------------------|-------|");
            sb.AppendLine("| `05_ADarkRoom_Narrative_Slice` | Stoke Fire → Explore → Craft | [ ] |");
            sb.AppendLine("| `07_RealmGrinder_Factions_Slice` | Build → Align Good / Align Evil | [ ] |");
            sb.AppendLine("| `08_NGUIdle_Energy_Slice` | Allocate Energy → Idle Tick Boost → Rebirth | [ ] |");
            sb.AppendLine("| `09_MelvorIdle_Skills_Slice` | Train Skill → Claim Offline | [ ] |");
            sb.AppendLine("| `10_EggInc_Hatch_Slice` | Hatch Burst → Upgrade Habitat → Soul Prestige | [ ] |");
            sb.AppendLine("| `11_IdleMinerTycoon_Shafts_Slice` | Collect Shaft → Upgrade Shaft → Hire Super-Manager → New Mine | [ ] |");
            sb.AppendLine("| `12_TapTitans2_TapDps_Slice` | Tap / Attack → Buy Hero DPS → Prestige | [ ] |");
            sb.AppendLine("| `13_IdleHeroes_GachaCombat_Slice` | Auto Fight → Gacha Pull → Claim AFK | [ ] |");
            sb.AppendLine("| `14_AFKArena_Chest_Slice` | Push Campaign → Open AFK Chest | [ ] |");
            sb.AppendLine("| `15_LegendOfMushroom_Lamp_Slice` | Rub Lamp → Farm Stage Gold | [ ] |");
            sb.AppendLine("| `16_CapybaraGo_Steps_Slice` | Next Step → Lucky Find | [ ] |");
            sb.AppendLine("| `17_CatsAndSoup_Assign_Slice` | Assign Cat → Unassign | [ ] |");
            sb.AppendLine("| `18_NekoAtsume_CheckIn_Slice` | Place Food → Place Toys → Check In | [ ] |");
            sb.AppendLine("| `19_FalloutShelter_Dwellers_Slice` | Assign Dweller → wait Pending → Claim Production | [ ] |");
            sb.AppendLine();
            sb.AppendLine("## Marking Playable");
            sb.AppendLine();
            sb.AppendLine("Only after a human/MCP Play session on **Hyper-Casual-Runner** (not another project):");
            sb.AppendLine("set the matching Play column in `docs/idle-toolkit-progress.md` to `Playable`.");
            sb.AppendLine();

            File.WriteAllText(absolutePath, sb.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();
            return absolutePath;
        }
    }
}
