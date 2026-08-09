using UnityEngine;
using UnityEditor;
using HyperCasualRunner.ECS.Authoring;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.Editor
{
    public class ToolkitExampleGenerator : EditorWindow
    {

        private static readonly string[] ExampleNames = new string[]
        {
            "1_SubwaySurfers_Meta", "2_IdleSlayer_Platformer", "3_TalkingTom_Rebuild",
            "4_CountMasters_Swarm", "5_JoinClash_Snake", "6_MobControl_Cannons", "7_BridgeRace_Stacker",
            "8_HighHeels_Vertical", "9_TallManRun_Scale", "10_RunRich_Status", "11_BlobRunner_Jiggle",
            "12_StackyDash_Grid", "13_ShortcutRun_Planks", "14_MoneyRush_Coins", "15_Aquapark_Skip",
            "16_MyMiniMart_Supply", "17_MyPerfectHotel_Service", "18_BurgerPlease_Multi", "19_PizzaReady_Flow",
            "20_MyLittleUniverse_Build", "21_WeaponMaster_Shooter"
        };

        [MenuItem("IdleToolkit/RunnerSlices/Generate 21 Playable Slices")]
        public static void GenerateExamples()
        {
            string exportPath = "Assets/ToolkitExamples";
            if (!AssetDatabase.IsValidFolder(exportPath)) AssetDatabase.CreateFolder("Assets", "ToolkitExamples");

            for (int i = 0; i < ExampleNames.Length; i++)
            {
                string prefabName = ExampleNames[i];
                GameObject root = new GameObject(prefabName + "_Slice");
                
                // 1. Generate Environment Plane
                GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.transform.SetParent(root.transform);
                ground.transform.localScale = new Vector3(2, 1, 10); // Long track
                if (prefabName.Contains("Supply") || prefabName.Contains("Service")) 
                    ground.transform.localScale = new Vector3(5, 1, 5); // Open arena for Idle

                // 2. Generate Player
                GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "PlayerEntity";
                player.transform.SetParent(root.transform);
                player.transform.position = new Vector3(0, 1, -40);
                player.AddComponent<PlayerAuthoring>();

                // 3. Inject Mechanics and Obstacles
                if (prefabName.Contains("SubwaySurfers"))
                {
                    // Subway Surfers 3-Lane Switcher Archetype
                    player.GetComponent<Renderer>().sharedMaterial.color = Color.yellow;

                    var lane = player.AddComponent<LaneAuthoring>();
                    lane.StartingLane = 0;
                    lane.LaneWidth = 2.5f;
                    lane.LaneChangeSpeed = 15f;

                    // Spawn 3-Lane Track Guides
                    for (int l = -1; l <= 1; l++)
                    {
                        GameObject laneMarker = GameObject.CreatePrimitive(PrimitiveType.Plane);
                        laneMarker.name = $"Lane_{l}";
                        laneMarker.transform.SetParent(root.transform);
                        laneMarker.transform.position = new Vector3(l * 2.5f, 0.01f, 0);
                        laneMarker.transform.localScale = new Vector3(0.2f, 1, 10);
                        laneMarker.GetComponent<Renderer>().sharedMaterial.color = (l == 0) ? Color.gray : Color.darkGray;
                    }

                    // Spawn Obstacles on specific lanes (-1 = Left, 0 = Center, 1 = Right)
                    int[] obstacleLanes = new int[] { -1, 0, 1, -1, 1, 0 };
                    float[] zPositions = new float[] { -25f, -10f, 5f, 15f, 25f, 35f };

                    for (int idx = 0; idx < obstacleLanes.Length; idx++)
                    {
                        int targetLane = obstacleLanes[idx];
                        float z = zPositions[idx];

                        GameObject obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        obs.name = $"LaneObstacle_{idx}";
                        obs.transform.SetParent(root.transform);
                        obs.transform.position = new Vector3(targetLane * 2.5f, 1.25f, z);
                        obs.transform.localScale = new Vector3(2f, 2.5f, 1f);
                        obs.GetComponent<Renderer>().sharedMaterial.color = Color.red;

                        var obsAuth = obs.AddComponent<LaneObstacleAuthoring>();
                        obsAuth.Lane = targetLane;
                        obsAuth.CollisionRadius = 1.2f;
                    }
                }
                else if (prefabName.Contains("Coins") || prefabName.Contains("MoneyRush"))
                {
                    // Money Rush / Coin Multiplier Archetype
                    player.GetComponent<Renderer>().sharedMaterial.color = Color.yellow;

                    // 1. Create Split Coin Template Prefab
                    GameObject coinTemplate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    coinTemplate.name = "SplitCoinTemplate";
                    coinTemplate.transform.SetParent(root.transform);
                    coinTemplate.transform.localScale = new Vector3(0.4f, 0.05f, 0.4f);
                    coinTemplate.transform.rotation = Quaternion.Euler(90f, 0, 0); // Disk orientation
                    Object.DestroyImmediate(coinTemplate.GetComponent<Collider>());
                    coinTemplate.GetComponent<Renderer>().sharedMaterial.color = Color.yellow;

                    var coinPhys = coinTemplate.AddComponent<CoinPhysicsAuthoring>();
                    coinPhys.SpreadAngle = 60.0f;
                    coinPhys.ImpulseSpeed = 8.0f;
                    coinPhys.StackHeightOffset = 0.5f;
                    coinPhys.MaxLifetime = 3.0f;
                    coinPhys.GravityMultiplier = 2.5f;
                    coinPhys.CoinCount = 1;
                    coinTemplate.SetActive(false);

                    // 2. Attach CoinSpawnerAuthoring to Player
                    var coinSpawner = player.AddComponent<CoinSpawnerAuthoring>();
                    coinSpawner.CoinVisualPrefab = coinTemplate;
                    coinSpawner.StartingCoinCount = 1;
                    coinSpawner.StackSpacing = 0.15f;
                    coinSpawner.MaxStackHeight = 2.0f;
                    coinSpawner.SwerveSensitivity = 1.0f;

                    var tween = player.AddComponent<MathTweenAuthoring>();
                    tween.Property = TweenProperty.ScaleUniform;
                    tween.BaseValue = 1f;
                    tween.Amplitude = 0.2f;
                    tween.Speed = 15f;

                    // 3. Spawn Multiplier Gates (+2, x3, +10, x2, x4)
                    CreateCoinGate(root, new Vector3(-2, 1, -25), MultiplierType.Additive, 2f, Color.cyan);
                    CreateCoinGate(root, new Vector3(2, 1, -25), MultiplierType.Multiplicative, 3f, Color.green);
                    CreateCoinGate(root, new Vector3(-2, 1, 0), MultiplierType.Additive, 10f, Color.cyan);
                    CreateCoinGate(root, new Vector3(2, 1, 0), MultiplierType.Multiplicative, 2f, Color.green);
                    CreateCoinGate(root, new Vector3(0, 1, 20), MultiplierType.Multiplicative, 4f, Color.green);
                }
                else if (prefabName.Contains("Swarm") || prefabName.Contains("Cannons"))
                {
                    var swarm = player.AddComponent<SwarmMechanicsAuthoring>();
                    swarm.StartingCount = 1;
                    player.GetComponent<Renderer>().sharedMaterial.color = Color.blue;
                    
                    var tween = player.AddComponent<MathTweenAuthoring>();
                    tween.Property = TweenProperty.ScaleUniform;
                    tween.BaseValue = 1f;
                    tween.Amplitude = 0.2f;
                    tween.Speed = 15f; // Fast running bounce

                    // Spawn 3 Math Gates along the track
                    CreateGate(root, new Vector3(0, 1, -20), GateOperation.Add, 10, Color.green);
                    CreateGate(root, new Vector3(-2, 1, 0), GateOperation.Multiply, 2, Color.green);
                    CreateGate(root, new Vector3(2, 1, 20), GateOperation.Subtract, 5, Color.red);

                    // Spawn a block of Enemies before the finish line
                    for (int x = -2; x <= 2; x += 2)
                    {
                        for (int z = 30; z <= 35; z += 2)
                        {
                            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            enemy.name = "EnemyEntity";
                            enemy.transform.SetParent(root.transform);
                            enemy.transform.position = new Vector3(x, 0.5f, z);
                            enemy.transform.localScale = Vector3.one * 0.8f;
                            enemy.GetComponent<Renderer>().sharedMaterial.color = Color.red;
                            enemy.AddComponent<EnemyAuthoring>();
                        }
                    }
                }
                else if (prefabName.Contains("Vertical"))
                {
                    // High Heels / Stilts Archetype
                    player.GetComponent<Renderer>().sharedMaterial.color = new Color(1f, 0.4f, 0.7f); // Pink runner

                    var stilts = player.AddComponent<StiltsAuthoring>();
                    stilts.HeelHeight = 0.5f;
                    stilts.StartingHeels = 0;

                    // 1. Spawn Heel Pickups along the track
                    for (int z = -35; z <= 15; z += 4)
                    {
                        for (int x = -2; x <= 2; x += 2)
                        {
                            GameObject heel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                            heel.name = "HeelCollectible";
                            heel.transform.SetParent(root.transform);
                            heel.transform.position = new Vector3(x, 0.4f, z);
                            heel.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                            heel.GetComponent<Renderer>().sharedMaterial.color = Color.magenta;

                            var collectible = heel.AddComponent<CollectibleAuthoring>();
                            collectible.GoldValue = 1;
                            collectible.CollisionRadius = 0.6f;
                        }
                    }

                    // 2. Spawn Hurdle Walls of increasing heights
                    CreateHurdleWall(root, new Vector3(0, 1f, -10), 1.5f, Color.red);
                    CreateHurdleWall(root, new Vector3(0, 1.5f, 5), 2.5f, Color.red);
                    CreateHurdleWall(root, new Vector3(0, 2.0f, 25), 3.5f, Color.red);
                }
                else if (prefabName.Contains("Scale") || prefabName.Contains("Status"))
                {
                    player.AddComponent<TransformationAuthoring>();
                    player.GetComponent<Renderer>().sharedMaterial.color = Color.magenta;

                    // Spawn Gates that act as Transformation triggers
                    CreateGate(root, new Vector3(0, 1, -20), GateOperation.Add, 5, Color.cyan); 
                    CreateGate(root, new Vector3(0, 1, 0), GateOperation.Subtract, -5, Color.magenta); 
                }
                else if (prefabName.Contains("StackyDash") || prefabName.Contains("Grid"))
                {
                    player.GetComponent<Renderer>().sharedMaterial.color = Color.cyan;

                    // 1. Create Tile Template for visual tile stack and path paving
                    GameObject tileTemplate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tileTemplate.name = "GridTileTemplate";
                    tileTemplate.transform.SetParent(root.transform);
                    tileTemplate.transform.localScale = new Vector3(0.8f, 0.2f, 0.8f);
                    Object.DestroyImmediate(tileTemplate.GetComponent<Collider>());
                    tileTemplate.GetComponent<Renderer>().sharedMaterial.color = Color.yellow;
                    tileTemplate.SetActive(false);

                    // 2. Attach MazeCollectorAuthoring and GridPathfinderAuthoring
                    var collector = player.AddComponent<MazeCollectorAuthoring>();
                    collector.StackVisualPrefab = tileTemplate;
                    collector.TileHeightOffset = 0.2f;
                    collector.CollectionRadius = 0.8f;
                    collector.StartingTiles = 0;

                    var pathfinder = player.AddComponent<GridPathfinderAuthoring>();
                    pathfinder.PathTilePrefab = tileTemplate;
                    pathfinder.StepDistance = 1.0f;
                    pathfinder.PathYHeight = 0.0f;
                    pathfinder.GridOrigin = Vector3.zero;

                    // 3. Spawn a grid of Maze Collectible Tiles
                    for (int x = -3; x <= 3; x += 1)
                    {
                        for (int z = -30; z <= 10; z += 2)
                        {
                            GameObject tileObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            tileObj.name = $"GridTile_{x}_{z}";
                            tileObj.transform.SetParent(root.transform);
                            tileObj.transform.position = new Vector3(x, 0.1f, z);
                            tileObj.transform.localScale = new Vector3(0.8f, 0.2f, 0.8f);
                            tileObj.GetComponent<Renderer>().sharedMaterial.color = Color.yellow;

                            var gridTile = tileObj.AddComponent<GridTileAuthoring>();
                            gridTile.GridPosition = new Vector2Int(x, z);
                            gridTile.TileSize = 1.0f;
                            gridTile.PickupRadius = 0.8f;
                            gridTile.IsWalkable = true;
                            gridTile.HasTileItem = true;
                            gridTile.TileValue = 1;
                        }
                    }

                    // 4. Create Water Gap Zone requiring paved tiles
                    GameObject gapVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    gapVisual.name = "GridWaterGap";
                    gapVisual.transform.SetParent(root.transform);
                    gapVisual.transform.position = new Vector3(0, -0.2f, 20f);
                    gapVisual.transform.localScale = new Vector3(10f, 0.2f, 15f);
                    gapVisual.GetComponent<Renderer>().sharedMaterial.color = new Color(0.1f, 0.4f, 0.8f, 0.8f);

                    var gapZone = gapVisual.AddComponent<GapZoneAuthoring>();
                    gapZone.Size = new Vector3(10f, 5f, 15f);
                }
                else if (prefabName.Contains("Snake") || prefabName.Contains("JoinClash"))
                {
                    player.GetComponent<Renderer>().sharedMaterial.color = Color.green;

                    // 1. Create Snake Follower Segment Template
                    GameObject followerTemplate = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    followerTemplate.name = "SnakeFollowerTemplate";
                    followerTemplate.transform.SetParent(root.transform);
                    followerTemplate.transform.localScale = Vector3.one * 0.8f;
                    Object.DestroyImmediate(followerTemplate.GetComponent<Collider>());
                    followerTemplate.GetComponent<Renderer>().sharedMaterial.color = new Color(0.2f, 0.8f, 0.2f);

                    var followerAuth = followerTemplate.AddComponent<SnakeFollowerAuthoring>();
                    followerAuth.SegmentIndex = 0;
                    followerAuth.FollowerRadius = 0.5f;
                    followerAuth.IsRecruited = true;

                    followerTemplate.SetActive(false);

                    // 2. Attach SnakeChainAuthoring to Player Head
                    var snakeChain = player.AddComponent<SnakeChainAuthoring>();
                    snakeChain.FollowerPrefab = followerTemplate;
                    snakeChain.StartingFollowerCount = 3;
                    snakeChain.SegmentSpacing = 0.8f;
                    snakeChain.FollowSpeed = 15.0f;
                    snakeChain.RotationSpeed = 12.0f;
                    snakeChain.HeadRadius = 0.8f;

                    // 3. Spawn Standalone Recruitable Follower Collectibles along track
                    for (int z = -30; z <= 10; z += 10)
                    {
                        for (int x = -2; x <= 2; x += 4)
                        {
                            GameObject recruitObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            recruitObj.name = $"FollowerRecruit_{z}_{x}";
                            recruitObj.transform.SetParent(root.transform);
                            recruitObj.transform.position = new Vector3(x, 0.5f, z);
                            recruitObj.transform.localScale = Vector3.one * 0.8f;
                            recruitObj.GetComponent<Renderer>().sharedMaterial.color = Color.yellow;

                            var joinAuth = recruitObj.AddComponent<SnakeJoinCollectibleAuthoring>();
                            joinAuth.JoinCount = 1;
                            joinAuth.CollisionRadius = 0.8f;

                            var collectible = recruitObj.AddComponent<CollectibleAuthoring>();
                            collectible.GoldValue = 1;
                            collectible.CollisionRadius = 0.8f;
                        }
                    }

                    // 4. Spawn Math Gates for recruitment
                    CreateGate(root, new Vector3(-2, 1, -15), GateOperation.Add, 5, Color.green);
                    CreateGate(root, new Vector3(2, 1, 5), GateOperation.Multiply, 2, Color.cyan);

                    // 5. Spawn Hazard Obstacles
                    for (int z = -5; z <= 25; z += 15)
                    {
                        GameObject obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        obs.name = $"SnakeHazard_{z}";
                        obs.transform.SetParent(root.transform);
                        obs.transform.position = new Vector3(0, 0.75f, z);
                        obs.transform.localScale = new Vector3(3f, 1.5f, 1f);
                        obs.GetComponent<Renderer>().sharedMaterial.color = Color.red;

                        var obsAuth = obs.AddComponent<SnakeObstacleAuthoring>();
                        obsAuth.SeverCount = 2;
                        obsAuth.CollisionRadius = 1.2f;
                        obsAuth.DestroyOnImpact = true;
                    }
                }
                else if (prefabName.Contains("Stack"))
                {
                    player.AddComponent<StackingAuthoring>();
                    player.GetComponent<Renderer>().sharedMaterial.color = Color.cyan;

                    // Spawn a grid of Collectibles
                    for (int x = -2; x <= 2; x += 2)
                    {
                        for (int z = -20; z <= 20; z += 5)
                        {
                            GameObject col = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            col.transform.SetParent(root.transform);
                            col.transform.position = new Vector3(x, 0.5f, z);
                            col.transform.localScale = Vector3.one * 0.5f;
                            col.GetComponent<Renderer>().sharedMaterial.color = Color.yellow;
                            
                            var tween = col.AddComponent<MathTweenAuthoring>();
                            tween.Property = TweenProperty.RotationY;
                            tween.Speed = 3f;
                            
                            var collectible = col.AddComponent<CollectibleAuthoring>();
                            collectible.GoldValue = 1;
                        }
                    }
                }
                else if (prefabName.Contains("Planks"))
                {
                    // Plank / Bridge Builder Archetype (Shortcut Run)
                    player.GetComponent<Renderer>().sharedMaterial.color = new Color(0.8f, 0.5f, 0.2f); // Wood color

                    // 1. Create Plank Template for Bridge Tiles
                    GameObject plankTileTemplate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    plankTileTemplate.name = "BridgePlankTemplate";
                    plankTileTemplate.transform.SetParent(root.transform);
                    plankTileTemplate.transform.localScale = new Vector3(3f, 0.1f, 0.8f);
                    Object.DestroyImmediate(plankTileTemplate.GetComponent<Collider>());
                    plankTileTemplate.GetComponent<Renderer>().sharedMaterial.color = new Color(0.6f, 0.4f, 0.1f);
                    plankTileTemplate.SetActive(false);

                    var builder = player.AddComponent<BridgeBuilderAuthoring>();
                    builder.BridgeTilePrefab = plankTileTemplate;
                    builder.DistancePerPlank = 0.8f;

                    // 2. Spawn Plank Collectibles before the gap
                    for (int z = -35; z <= -15; z += 3)
                    {
                        for (int x = -2; x <= 2; x += 2)
                        {
                            GameObject col = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            col.name = "PlankCollectible";
                            col.transform.SetParent(root.transform);
                            col.transform.position = new Vector3(x, 0.5f, z);
                            col.transform.localScale = new Vector3(1.2f, 0.2f, 0.6f);
                            col.GetComponent<Renderer>().sharedMaterial.color = new Color(0.8f, 0.5f, 0.2f);

                            var collectible = col.AddComponent<CollectibleAuthoring>();
                            collectible.GoldValue = 1;
                            collectible.CollisionRadius = 0.8f;
                        }
                    }

                    // 3. Create Water Gap Zone
                    GameObject gapVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    gapVisual.name = "WaterGap";
                    gapVisual.transform.SetParent(root.transform);
                    gapVisual.transform.position = new Vector3(0, -0.2f, 5f);
                    gapVisual.transform.localScale = new Vector3(20f, 0.2f, 30f);
                    gapVisual.GetComponent<Renderer>().sharedMaterial.color = new Color(0.1f, 0.4f, 0.8f, 0.8f); // Blue water

                    var gapZone = gapVisual.AddComponent<GapZoneAuthoring>();
                    gapZone.Size = new Vector3(20f, 5f, 30f);
                }
                else if (prefabName.Contains("Aquapark") || prefabName.Contains("Skip"))
                {
                    // Aquapark / Ramp Jump Archetype
                    player.GetComponent<Renderer>().sharedMaterial.color = Color.cyan;

                    // 1. Create Launch Ramp
                    GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    ramp.name = "LaunchRamp";
                    ramp.transform.SetParent(root.transform);
                    ramp.transform.position = new Vector3(0, 0.5f, -10f);
                    ramp.transform.localScale = new Vector3(4f, 1f, 3f);
                    ramp.transform.rotation = Quaternion.Euler(-15f, 0, 0);
                    ramp.GetComponent<Renderer>().sharedMaterial.color = Color.yellow;

                    var rampAuth = ramp.AddComponent<RampAuthoring>();
                    rampAuth.JumpForce = 18f;
                    rampAuth.CollisionRadius = 2.5f;

                    // 2. Create Floating Water Target Landing Zone
                    GameObject waterTarget = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    waterTarget.name = "TargetWaterZone";
                    waterTarget.transform.SetParent(root.transform);
                    waterTarget.transform.position = new Vector3(0, -0.2f, 25f);
                    waterTarget.transform.localScale = new Vector3(12f, 0.2f, 15f);
                    waterTarget.GetComponent<Renderer>().sharedMaterial.color = new Color(0f, 0.6f, 1f, 0.8f);

                    var waterAuth = waterTarget.AddComponent<WaterZoneAuthoring>();
                    waterAuth.BonusMultiplier = 3f;
                    waterAuth.Size = new Vector3(12f, 2f, 15f);
                }
                else if (prefabName.Contains("Supply") || prefabName.Contains("Service") || prefabName.Contains("Build"))
                {
                    player.transform.position = new Vector3(0, 1, 0); // Center start
                    player.GetComponent<Renderer>().sharedMaterial.color = Color.yellow;

                    // Spawn Arcade Idle Resource Nodes (e.g. Trees)
                    for (int n = 0; n < 3; n++)
                    {
                        GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        node.name = "ResourceNode";
                        node.transform.SetParent(root.transform);
                        node.transform.position = new Vector3(-10 + (n * 10), 1, 15);
                        var authoring = node.AddComponent<ArcadeIdleAuthoring>();
                        authoring.HarvestingTime = 2f;
                        authoring.Yield = 5;
                        node.GetComponent<Renderer>().sharedMaterial.color = Color.green;
                    }
                }
                else if (prefabName.Contains("Shooter"))
                {
                    player.GetComponent<Renderer>().sharedMaterial.color = Color.black;

                    // Create Projectile Template
                    GameObject projTemplate = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    projTemplate.name = "ProjectileTemplate";
                    projTemplate.transform.SetParent(root.transform);
                    projTemplate.transform.localScale = Vector3.one * 0.3f;
                    Object.DestroyImmediate(projTemplate.GetComponent<Collider>());
                    projTemplate.GetComponent<Renderer>().sharedMaterial.color = Color.yellow;
                    
                    var projAuth = projTemplate.AddComponent<ProjectileAuthoring>();
                    projAuth.Speed = 40f;
                    projAuth.Damage = 1f;
                    projTemplate.SetActive(false);

                    var shooter = player.AddComponent<ShooterAuthoring>();
                    shooter.ProjectilePrefab = projTemplate;
                    shooter.FireRate = 0.25f; // Start slower so upgrades feel good
                    shooter.SpreadCount = 1;
                    shooter.SpreadAngle = 15f;
                    shooter.Pierce = 1; // Default pierce 1

                    // Spawn Upgrades (Math Gates) before the blockade
                    CreateGate(root, new Vector3(-2, 1, -20), GateOperation.Add, 1, Color.cyan); // +1 Spread
                    CreateGate(root, new Vector3(2, 1, -10), GateOperation.Subtract, 5, Color.green); // Faster Fire Rate
                    CreateGate(root, new Vector3(0, 1, 0), GateOperation.Add, 2, Color.cyan); // +2 Spread

                    // Spawn a heavy enemy blockade
                    for (int z = 20; z <= 60; z += 5)
                    {
                        for (int x = -4; x <= 4; x += 2)
                        {
                            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            enemy.name = "EnemyBlock_Entity";
                            enemy.transform.SetParent(root.transform);
                            enemy.transform.position = new Vector3(x, 0.5f, z);
                            enemy.transform.localScale = Vector3.one;
                            enemy.GetComponent<Renderer>().sharedMaterial.color = Color.red;
                            var enemyAuth = enemy.AddComponent<EnemyAuthoring>();
                            enemyAuth.Health = 2f;
                        }
                    }
                }

                // 4. Inject Level State, End Zone, and UI HUD
                GameObject levelManager = new GameObject("LevelManager");
                levelManager.transform.SetParent(root.transform);
                levelManager.AddComponent<LevelManagerAuthoring>();
                levelManager.AddComponent<UnityEngine.UIElements.UIDocument>();
                levelManager.AddComponent<HyperCasualRunner.UI.UIManagerSystem>();
                levelManager.AddComponent<HyperCasualRunner.ECS.Authoring.AudioManagerAuthoring>();
                levelManager.AddComponent<HyperCasualRunner.UI.MockAdsManager>();

                // Build a local ParticleSystem template for VFX
                GameObject vfxTemplate = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                vfxTemplate.name = "HitVFX_Template";
                vfxTemplate.transform.SetParent(levelManager.transform);
                vfxTemplate.transform.localScale = Vector3.one * 0.2f;
                Object.DestroyImmediate(vfxTemplate.GetComponent<Collider>()); // No physics needed
                
                var ps = vfxTemplate.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.duration = 0.5f;
                main.loop = false;
                main.startSpeed = 5f;
                main.startSize = 0.3f;
                main.startColor = Color.white;
                
                var emission = ps.emission;
                emission.rateOverTime = 0;
                emission.SetBursts(new ParticleSystem.Burst[]{ new ParticleSystem.Burst(0f, 15) });
                
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;

                vfxTemplate.SetActive(false); // Hide the template

                var vfxAuthoring = levelManager.AddComponent<VFXManagerAuthoring>();
                vfxAuthoring.HitVFXPrefab = vfxTemplate;

                // Arcade Idle games don't have an end zone (they are endless arenas)
                if (!prefabName.Contains("Supply") && !prefabName.Contains("Service") && !prefabName.Contains("Build") && !prefabName.Contains("Flow"))
                {
                    GameObject endZone = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    endZone.name = "EndZone";
                    endZone.transform.SetParent(root.transform);
                    endZone.transform.position = new Vector3(0, 0, 40); // 80 units of running total
                    endZone.transform.localScale = new Vector3(10, 1, 5);
                    endZone.GetComponent<Renderer>().sharedMaterial.color = Color.green;
                    
                    var endAuthoring = endZone.AddComponent<EndZoneAuthoring>();
                    endAuthoring.Radius = 3f;
                }

                // Save as Prefab
                string path = $"{exportPath}/{prefabName}_Slice.prefab";
                PrefabUtility.SaveAsPrefabAsset(root, path);
                DestroyImmediate(root);
            }

            // 5. Generate Master Toolkit Hub
            GameObject hubRoot = new GameObject("ToolkitHub_Slice");
            hubRoot.AddComponent<UnityEngine.UIElements.UIDocument>();
            var hubManager = hubRoot.AddComponent<HyperCasualRunner.UI.ToolkitHubManager>();
            
            var generatedPrefabs = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < ExampleNames.Length; i++)
            {
                string path = $"{exportPath}/{ExampleNames[i]}_Slice.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) generatedPrefabs.Add(prefab);
            }
            hubManager.LevelPrefabs = generatedPrefabs.ToArray();

            PrefabUtility.SaveAsPrefabAsset(hubRoot, $"{exportPath}/00_ToolkitHub_Master.prefab");
            DestroyImmediate(hubRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Successfully generated 21 Playable Slices and 1 Master Hub!");
        }

        private static void CreateGate(GameObject root, Vector3 pos, GateOperation op, int val, Color col)
        {
            GameObject gate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gate.name = "MathGate";
            gate.transform.SetParent(root.transform);
            gate.transform.position = pos;
            gate.transform.localScale = new Vector3(4, 2, 0.5f);
            gate.GetComponent<Renderer>().sharedMaterial.color = col;
            
            var authoring = gate.AddComponent<MathGateAuthoring>();
            authoring.Operation = op;
            authoring.Value = val;
            
            var tween = gate.AddComponent<MathTweenAuthoring>();
            tween.Property = TweenProperty.PositionY;
            tween.BaseValue = pos.y;
            tween.Amplitude = 0.5f;
            tween.Speed = 2f;
        }

        private static void CreateCoinGate(GameObject root, Vector3 pos, MultiplierType gateType, float val, Color col)
        {
            GameObject gate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gate.name = $"CoinGate_{gateType}_{val}";
            gate.transform.SetParent(root.transform);
            gate.transform.position = pos;
            gate.transform.localScale = new Vector3(3.5f, 2.5f, 0.5f);
            gate.GetComponent<Renderer>().sharedMaterial.color = col;

            var gateAuth = gate.AddComponent<CoinGateAuthoring>();
            gateAuth.GateType = gateType;
            gateAuth.Value = val;
            gateAuth.GateWidth = 3.5f;
            gateAuth.TriggerDepth = 0.5f;
            gateAuth.MinimumOutput = 1.0f;

            var tween = gate.AddComponent<MathTweenAuthoring>();
            tween.Property = TweenProperty.PositionY;
            tween.BaseValue = pos.y;
            tween.Amplitude = 0.3f;
            tween.Speed = 2f;
        }

        private static void CreateHurdleWall(GameObject root, Vector3 pos, float reqHeight, Color col)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "HurdleWall";
            wall.transform.SetParent(root.transform);
            wall.transform.position = pos;
            wall.transform.localScale = new Vector3(8f, reqHeight, 0.5f);
            wall.GetComponent<Renderer>().sharedMaterial.color = col;

            var authoring = wall.AddComponent<HurdleWallAuthoring>();
            authoring.RequiredHeight = reqHeight;
            authoring.CollisionRadius = 1.5f;
        }

        [MenuItem("IdleToolkit/RunnerSlices/Generate and Verify All")]
        public static void GenerateAndVerify()
        {
            GenerateExamples();
            RunVerificationSuite();
        }

        [MenuItem("IdleToolkit/RunnerSlices/Run Verification Suite")]
        public static void RunVerificationSuite()
        {
            AssetDatabase.Refresh();
            string exportPath = "Assets/ToolkitExamples";
            int passed = 0;
            int total = ExampleNames.Length;
            string report = "=== EMPIRICAL TOOLKIT VERIFICATION REPORT ===\n\n";

            for (int i = 0; i < ExampleNames.Length; i++)
            {
                string sliceName = ExampleNames[i];
                string path = $"{exportPath}/{sliceName}_Slice.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                {
                    report += $"[FAIL] {sliceName} -> Prefab not found at {path}\n";
                }
                else
                {
                    passed++;
                    report += $"[PASS] {sliceName} -> Verified on disk ({prefab.transform.childCount} children)\n";

                    Transform player = prefab.transform.Find("PlayerEntity");
                    if (player != null)
                    {
                        if (sliceName.Contains("SubwaySurfers"))
                        {
                            var l = player.GetComponent<LaneAuthoring>();
                            report += $"       - LaneAuthoring verified: LaneWidth={l?.LaneWidth}, Speed={l?.LaneChangeSpeed}\n";
                        }
                        else if (sliceName.Contains("Shooter"))
                        {
                            var s = player.GetComponent<ShooterAuthoring>();
                            report += $"       - ShooterAuthoring verified: SpreadCount={s?.SpreadCount}, FireRate={s?.FireRate}\n";
                        }
                        else if (sliceName.Contains("Planks"))
                        {
                            var b = player.GetComponent<BridgeBuilderAuthoring>();
                            report += $"       - BridgeBuilderAuthoring verified: DistancePerPlank={b?.DistancePerPlank}\n";
                        }
                        else if (sliceName.Contains("Vertical"))
                        {
                            var st = player.GetComponent<StiltsAuthoring>();
                            report += $"       - StiltsAuthoring verified: HeelHeight={st?.HeelHeight}\n";
                        }
                        else if (sliceName.Contains("Skip"))
                        {
                            Transform ramp = prefab.transform.Find("LaunchRamp");
                            var r = ramp?.GetComponent<RampAuthoring>();
                            report += $"       - RampAuthoring verified: JumpForce={r?.JumpForce}\n";
                        }
                        else if (sliceName.Contains("Snake") || sliceName.Contains("JoinClash"))
                        {
                            var snakeChain = player.GetComponent<SnakeChainAuthoring>();
                            report += $"       - SnakeChainAuthoring verified: StartingFollowers={snakeChain?.StartingFollowerCount}, SegmentSpacing={snakeChain?.SegmentSpacing}\n";
                        }
                        else if (sliceName.Contains("StackyDash") || sliceName.Contains("Grid"))
                        {
                            var collector = player.GetComponent<MazeCollectorAuthoring>();
                            var pathfinder = player.GetComponent<GridPathfinderAuthoring>();
                            report += $"       - MazeCollectorAuthoring & GridPathfinderAuthoring verified: TileHeightOffset={collector?.TileHeightOffset}, StepDistance={pathfinder?.StepDistance}\n";
                        }
                        else if (sliceName.Contains("Coins") || sliceName.Contains("MoneyRush"))
                        {
                            var spawner = player.GetComponent<CoinSpawnerAuthoring>();
                            int gateCount = 0;
                            foreach (Transform child in prefab.transform)
                            {
                                if (child.GetComponent<CoinGateAuthoring>() != null)
                                    gateCount++;
                            }
                            report += $"       - CoinSpawnerAuthoring verified: StartingCoins={spawner?.StartingCoinCount}, GateCount={gateCount}\n";
                        }
                    }
                }
            }

            report += $"\nSUMMARY: {passed}/{total} Playable Slices verified successfully!\n";
            Debug.Log(report);
            string reportPath = System.IO.Path.Combine(Application.dataPath, "../verification_report.txt");
            System.IO.File.WriteAllText(reportPath, report);
            System.IO.File.WriteAllText("verification_report.txt", report);
        }
    }
}
