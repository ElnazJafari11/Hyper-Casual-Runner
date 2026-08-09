# Handoff Report: Milestone 2 Authoring & Generator Strategy (5_JoinClash_Snake)

**Author**: Explorer 2 (M2: Snake Follower Chain & Collision)  
**Target Directory**: `d:\Git\Hyper-Casual-Runner\.agents\explorer_m2_2`  
**Date**: 2026-07-22  

---

## 1. Observation

### Current Prefab & Generator Inspection
1. **Prefab Inspection**:
   - `5_JoinClash_Snake_Slice.prefab` is located at `Assets/ToolkitExamples/5_JoinClash_Snake_Slice.prefab`.
   - `ToolkitExampleGenerator.cs` (lines 14, 89, 221) defines generation logic for all 21 example slices.

2. **Misrouting in `ToolkitExampleGenerator.cs`**:
   - **Line 14**: `ExampleNames` array includes `"5_JoinClash_Snake"`.
   - **Line 89**: `else if (prefabName.Contains("Swarm") || prefabName.Contains("Coins") || prefabName.Contains("Cannons"))` -> Does **not** match `5_JoinClash_Snake`.
   - **Line 221**: `else if (prefabName.Contains("Stack") || prefabName.Contains("Snake"))` -> **Matches `5_JoinClash_Snake`**!
   - **Lines 223–244**: Due to matching line 221, `5_JoinClash_Snake_Slice.prefab` currently receives `StackingAuthoring` on `PlayerEntity` and spawns generic rotating collectible cubes, treating the Snake slice as a Stack runner.

3. **Missing Authoring Components**:
   - `Assets/Scripts/ECS/Authoring/SnakeChainAuthoring.cs` does **not** exist.
   - `Assets/Scripts/ECS/Authoring/SnakeFollowerAuthoring.cs` does **not** exist.

4. **Missing Verification Checks in `RunVerificationSuite`**:
   - In `ToolkitExampleGenerator.cs` (lines 500–565), `RunVerificationSuite()` contains conditional checks for `SubwaySurfers`, `Shooter`, `Planks`, `Vertical`, `Skip`, `StackyDash`, but has **no** check for `Snake` or `JoinClash`.

---

## 2. Logic Chain

1. **Misrouting Root Cause**:
   - The generator condition at line 221 of `ToolkitExampleGenerator.cs` (`else if (prefabName.Contains("Stack") || prefabName.Contains("Snake"))`) improperly grouped `Snake` with `Stack`. 
   - `7_BridgeRace_Stacker` requires `StackingAuthoring`, whereas `5_JoinClash_Snake` requires follower chain movement and collision logic (`SnakeChainAuthoring`).

2. **Authoring Requirements**:
   - **Snake Chain Head (`PlayerEntity`)**: Needs `SnakeChainAuthoring.cs` to bake `SnakeChainComponent` and attach dynamic buffers (`SnakeSegmentBuffer` for position/rotation history, `SnakeFollowerElement` for active follower entity handles).
   - **Snake Follower / Recruit (`SnakeFollowerTemplate` & Recruit Pickups)**: Needs `SnakeFollowerAuthoring.cs` to bake `SnakeFollowerComponent` (holding `FollowerIndex`, `LeaderEntity`, `FollowerRadius`, `IsRecruited`, `TargetPosition`, `TargetRotation`).

3. **Generator Strategy**:
   - Modify line 221 to `else if (prefabName.Contains("Stack") && !prefabName.Contains("Snake"))` (or `else if (prefabName.Contains("Stack"))`).
   - Add a dedicated `else if (prefabName.Contains("Snake") || prefabName.Contains("JoinClash"))` block that:
     1. Color-codes `PlayerEntity` (green).
     2. Instantiates a `SnakeFollowerTemplate` sphere (green), adds `SnakeFollowerAuthoring` (`IsRecruited = true`), and deactivates it.
     3. Adds `SnakeChainAuthoring` to `PlayerEntity`, referencing `SnakeFollowerTemplate`.
     4. Spawns standalone recruitable follower pickups along the track with `SnakeFollowerAuthoring` (`IsRecruited = false`) and `CollectibleAuthoring`.
     5. Spawns Math Multiplier Gates (`+5 Followers`, `x2 Followers`) for recruiting chain units.
     6. Spawns obstacle hazards (`LaneObstacleAuthoring`) for chain dismantling on collision.
   - Update `RunVerificationSuite()` to assert `SnakeChainAuthoring` exists on `5_JoinClash_Snake_Slice.prefab`.

---

## 3. Caveats

- **Read-Only Explorer Constraint**: No source code files in `Assets/` have been modified during this investigation. Implementation must be carried out by the assigned Implementer agent.
- **Component Data Dependency**: Authoring bakers depend on component definitions (`SnakeChainComponent`, `SnakeFollowerComponent`, `SnakeSegmentBuffer`, `SnakeFollowerElement`) designed by Explorer 1 (`Assets/Scripts/ECS/Components/SnakeComponents.cs`).
- **TransformUsageFlags**: `Baker<T>` calls must use `TransformUsageFlags.Dynamic` for both the Head and Follower entities to enable dynamic movement system updates.

---

## 4. Conclusion & Proposed Code Implementation

### A. Authoring Script 1: `Assets/Scripts/ECS/Authoring/SnakeChainAuthoring.cs`
```csharp
using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    /// <summary>
    /// Authoring component for the Snake Head entity (PlayerEntity).
    /// Bakes SnakeChainComponent and attaches dynamic history and follower entity buffers.
    /// </summary>
    public class SnakeChainAuthoring : MonoBehaviour
    {
        [Header("Prefab References")]
        public GameObject FollowerPrefab;

        [Header("Chain Settings")]
        public int StartingFollowerCount = 3;
        public float SegmentSpacing = 1.0f;
        public float FollowSpeed = 15.0f;
        public int HistoryBufferSize = 100;

        class Baker : Baker<SnakeChainAuthoring>
        {
            public override void Bake(SnakeChainAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                Entity followerPrefabEntity = Entity.Null;
                if (authoring.FollowerPrefab != null)
                {
                    followerPrefabEntity = GetEntity(authoring.FollowerPrefab, TransformUsageFlags.Dynamic);
                }

                // Bake main chain component onto head entity
                AddComponent(entity, new SnakeChainComponent
                {
                    FollowerPrefab = followerPrefabEntity,
                    TargetCount = authoring.StartingFollowerCount,
                    CurrentCount = 0,
                    SegmentSpacing = authoring.SegmentSpacing,
                    FollowSpeed = authoring.FollowSpeed,
                    HistoryBufferSize = authoring.HistoryBufferSize
                });

                // Attach dynamic buffer for position/rotation history
                AddBuffer<SnakeSegmentBuffer>(entity);

                // Attach dynamic buffer for active follower entity handles
                AddBuffer<SnakeFollowerElement>(entity);
            }
        }
    }
}
```

---

### B. Authoring Script 2: `Assets/Scripts/ECS/Authoring/SnakeFollowerAuthoring.cs`
```csharp
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    /// <summary>
    /// Authoring component for Snake Follower segment entities and recruit pickups.
    /// </summary>
    public class SnakeFollowerAuthoring : MonoBehaviour
    {
        public int FollowerIndex = 0;
        public float FollowerRadius = 0.5f;
        public bool IsRecruited = false;

        class Baker : Baker<SnakeFollowerAuthoring>
        {
            public override void Bake(SnakeFollowerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new SnakeFollowerComponent
                {
                    FollowerIndex = authoring.FollowerIndex,
                    LeaderEntity = Entity.Null,
                    FollowerRadius = authoring.FollowerRadius,
                    IsRecruited = authoring.IsRecruited,
                    TargetPosition = float3.zero,
                    TargetRotation = quaternion.identity
                });
            }
        }
    }
}
```

---

### C. Generator Updates in `Assets/Scripts/Editor/ToolkitExampleGenerator.cs`

1. **Fix Line 221 Misrouting**:
```csharp
// Change from:
// else if (prefabName.Contains("Stack") || prefabName.Contains("Snake"))
// To:
else if (prefabName.Contains("Stack") && !prefabName.Contains("Snake"))
```

2. **Add Dedicated Snake/JoinClash Generator Block**:
```csharp
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
    followerAuth.FollowerIndex = 0;
    followerAuth.FollowerRadius = 0.5f;
    followerAuth.IsRecruited = true;

    followerTemplate.SetActive(false);

    // 2. Attach SnakeChainAuthoring to Player Head
    var snakeChain = player.AddComponent<SnakeChainAuthoring>();
    snakeChain.FollowerPrefab = followerTemplate;
    snakeChain.StartingFollowerCount = 3;
    snakeChain.SegmentSpacing = 1.0f;
    snakeChain.FollowSpeed = 15.0f;
    snakeChain.HistoryBufferSize = 100;

    // 3. Spawn Recruitable Follower Collectibles along the track
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

            var recFollower = recruitObj.AddComponent<SnakeFollowerAuthoring>();
            recFollower.FollowerIndex = -1;
            recFollower.FollowerRadius = 0.5f;
            recFollower.IsRecruited = false;

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

        var obsAuth = obs.AddComponent<LaneObstacleAuthoring>();
        obsAuth.Lane = 0;
        obsAuth.CollisionRadius = 1.2f;
    }
}
```

3. **Update `RunVerificationSuite()` Verification Checks**:
Add inside `RunVerificationSuite()`:
```csharp
else if (sliceName.Contains("Snake") || sliceName.Contains("JoinClash"))
{
    var snakeChain = player.GetComponent<SnakeChainAuthoring>();
    report += $"       - SnakeChainAuthoring verified: StartingFollowers={snakeChain?.StartingFollowerCount}, SegmentSpacing={snakeChain?.SegmentSpacing}\n";
}
```

4. **Add Unit Test in `Assets/Scripts/Editor/Tests/ToolkitGeneratorTests.cs`**:
```csharp
[Test]
public void JoinClash_ContainsSnakeChainAuthoring()
{
    string path = "Assets/ToolkitExamples/5_JoinClash_Snake_Slice.prefab";
    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

    Assert.IsNotNull(prefab, "JoinClash_Snake prefab not found.");

    var player = prefab.transform.Find("PlayerEntity");
    Assert.IsNotNull(player, "PlayerEntity not found in prefab.");

    var snakeAuthoring = player.GetComponent<SnakeChainAuthoring>();
    Assert.IsNotNull(snakeAuthoring, "SnakeChainAuthoring not found on PlayerEntity.");
    Assert.IsNotNull(snakeAuthoring.FollowerPrefab, "FollowerPrefab reference is null on SnakeChainAuthoring.");
}
```

---

## 5. Verification Method

1. **File Verification**:
   - Inspect that `Assets/Scripts/ECS/Authoring/SnakeChainAuthoring.cs` and `Assets/Scripts/ECS/Authoring/SnakeFollowerAuthoring.cs` exist on disk.
   - Verify that `ToolkitExampleGenerator.cs` has the dedicated `Snake` generator block and line 221 misrouting fix.

2. **Generator Test**:
   - In Unity Editor menu, execute `IdleToolkit -> Generate 21 Playable Slices`.
   - Inspect `Assets/ToolkitExamples/5_JoinClash_Snake_Slice.prefab` to confirm `PlayerEntity` contains `SnakeChainAuthoring` with a valid `SnakeFollowerTemplate` reference.

3. **Suite Verification**:
   - In Unity Editor menu, execute `IdleToolkit -> Run Verification Suite`.
   - Confirm `verification_report.txt` contains `[PASS] 5_JoinClash_Snake -> Verified on disk` with `SnakeChainAuthoring verified`.
