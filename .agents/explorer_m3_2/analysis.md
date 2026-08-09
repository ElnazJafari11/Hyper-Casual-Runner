# Analysis Report: System Execution Logic, Coin Splitting Physics & Generator Integration (Milestone 3 - Slice 14)

**Author**: Explorer 2  
**Date**: 2026-07-22  
**Target Milestone**: Milestone 3 (`14_MoneyRush_Coins`)  
**Working Directory**: `d:\Git\Hyper-Casual-Runner\.agents\explorer_m3_2`  

---

## 1. Executive Summary & Architecture Overview

Milestone 3 focuses on the **Coin Multiplier & Splitting Physics** core loop (`14_MoneyRush_Coins_Slice.prefab`).
In this runner archetype:
1. The Player entity navigates a running track containing **Multiplier Gates** (e.g. `+2`, `x3`, `+10`, `x2`).
2. Passing through a multiplier gate triggers a coin balance increase and spawns physical split coin entities into 3D space.
3. Burst-spawned split coins burst outward along **3D parabolic trajectories**, bounce on the track, undergo linear/angular damping, enter a **magnetic attraction phase**, and return to the player/collection pool.

This analysis provides the complete pure DOTS system design for `CoinMultiplierSystem` and `CoinPhysicsSystem`, authoring bakers (`CoinGateAuthoring`, `CoinPhysicsAuthoring`, `CoinSpawnerAuthoring`), exact 3D physics formulas, and programmatic generator integration into `Assets/Scripts/Editor/ToolkitExampleGenerator.cs`.

---

## 2. System Execution Logic & Pure DOTS System Designs

### 2.1 System 1: `CoinMultiplierSystem`

#### Signature & Attributes
```csharp
namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(CoinPhysicsSystem))]
    [BurstCompile]
    public partial struct CoinMultiplierSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerComponent>();
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Execution logic
        }
    }
}
```

#### Detailed Execution Workflow
1. **Player & Gate Query**:
   - Query Player entity transform (`LocalTransform`), inventory (`CoinInventoryComponent` / `SwarmComponent`), and spawner configuration (`CoinSpawnerComponent`).
   - Query all active gates (`CoinGateComponent`, `LocalTransform`).
2. **Trigger Collision Check**:
   - Compute squared distance $d^2 = \|\vec{x}_{player} - \vec{x}_{gate}\|^2$.
   - Collision occurs when $d^2 < (R_{player} + R_{gate})^2$.
3. **Math Operation Resolution**:
   - Given current count $C_{old}$ and gate value $V_{gate}$:
     - `GateOperation.Add`: $C_{new} = C_{old} + V_{gate}$, spawn count $N = V_{gate}$.
     - `GateOperation.Multiply`: $C_{new} = C_{old} \times V_{gate}$, spawn count $N = C_{old} \times (V_{gate} - 1)$.
     - `GateOperation.Subtract`: $C_{new} = \max(1, C_{old} - V_{gate})$, spawn count $N = 0$.
     - `GateOperation.Divide`: $C_{new} = \max(1, \lfloor C_{old} / V_{gate} \rfloor)$, spawn count $N = 0$.
4. **Burst Coin Spawning & Impulse Assignment**:
   - For each spawned coin ($i = 0 \dots N-1$):
     - Instantiate entity from `CoinSpawnerComponent.CoinPrefabEntity` via `EntityCommandBuffer`.
     - Set initial transform: $\vec{x}_{start} = \vec{x}_{player} + \vec{\delta}_{offset}$.
     - Calculate 3D parabolic impulse vector $\vec{v}_0 = (v_x, v_y, v_z)$ (see Section 3.1).
     - Attach `CoinPhysicsComponent` with initial velocity $\vec{v}_0$, phase `CoinPhysicsPhase.Airborne`, and lifetime timer.
5. **Event Emission & Gate Consumption**:
   - Instantiate presentation tags for hybrid audio/VFX (e.g. `PlaySoundEventComponent` with coin multiplier SFX ID, `VFXManagerAuthoring` burst tag).
   - Mark gate `IsTriggered = true` or destroy gate entity via `EntityCommandBuffer.DestroyEntity(gateEntity)`.

---

### 2.2 System 2: `CoinPhysicsSystem`

#### Signature & Attributes
```csharp
namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CoinMultiplierSystem))]
    [BurstCompile]
    public partial struct CoinPhysicsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerComponent>();
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Execution logic
        }
    }
}
```

#### Detailed Execution Workflow & Phase Transitions
`CoinPhysicsSystem` processes split coins through 3 distinct state phases stored in `CoinPhysicsComponent.Phase`:

1. **Phase 1: Airborne Parabolic Trajectory (`CoinPhysicsPhase.Airborne`)**:
   - Gravity application: $\vec{v}(t + \Delta t) = \vec{v}(t) + \begin{pmatrix} 0 \\ -g \\ 0 \end{pmatrix} \Delta t$.
   - Air damping: $\vec{v}(t + \Delta t) = \vec{v}(t + \Delta t) \times (1 - \gamma_{air} \Delta t)$.
   - Position integration: $\vec{x}(t + \Delta t) = \vec{x}(t) + \vec{v}(t + \Delta t) \Delta t$.
   - Ground detection check: if $y(t + \Delta t) \le y_{ground}$:
     - Transition to `Phase 2: Bounce`.

2. **Phase 2: Ground Bounce & Friction (`CoinPhysicsPhase.Grounded`)**:
   - Rebound vertical velocity: $v_y = -v_y \times e_{rebound}$.
   - Horizontal velocity damping: $\vec{v}_{xz} = \vec{v}_{xz} \times (1 - \mu_{friction} \Delta t)$.
   - If $|v_y| < v_{threshold}$ (e.g. $0.5 \text{ m/s}$), zero out vertical velocity and set $y = y_{ground}$.
   - Increment ground timer $t_{ground} += \Delta t$.
   - When $t_{ground} \ge t_{attract\_delay}$, transition to `Phase 3: Magnet`.

3. **Phase 3: Magnetic Target Attraction & Collection (`CoinPhysicsPhase.Magnetic`)**:
   - Query current Player position $\vec{x}_{player}$.
   - Calculate normalized direction vector: $\hat{u} = \frac{\vec{x}_{player} - \vec{x}_{coin}}{\|\vec{x}_{player} - \vec{x}_{coin}\|}$.
   - Calculate distance $d = \|\vec{x}_{player} - \vec{x}_{coin}\|$.
   - Magnet acceleration velocity: $\vec{v}_{mag} = \hat{u} \times (v_{base} + \frac{K_{magnet}}{d + \epsilon})$.
   - Position update: $\vec{x}_{coin} += \vec{v}_{mag} \Delta t$.
   - **Collection Trigger**: When distance $d < R_{collect}$ (e.g. $0.8\text{m}$):
     - Increment player run gold stats (`CurrentRunStats.CurrentGold += 1.0`).
     - Spawn audio event entity (`PlaySoundEventComponent`).
     - Destroy coin entity via `EntityCommandBuffer.DestroyEntity(coinEntity)`.

---

## 3. Physics Formulas & Mathematical Derivations

### 3.1 3D Parabolic Impulse & Spherical Spread Angle
To create a satisfying burst fan effect when coins split upon gate collision:

- Let $\theta$ be the horizontal spread angle relative to the forward direction $\hat{f} = (0,0,1)$:
  $$\theta \sim U(-\frac{\Theta_{spread}}{2}, +\frac{\Theta_{spread}}{2})$$
- Let $v_{burst}$ be the scalar horizontal burst impulse and $v_{up}$ be the upward initial impulse.
- For coin index $i \in \{0, \dots, N-1\}$:
  $$\theta_i = -\frac{\Theta_{spread}}{2} + \frac{i}{N-1} \Theta_{spread}$$
  $$v_{x,i} = v_{burst} \cdot \sin(\theta_i) + \delta_{rand,x}$$
  $$v_{z,i} = v_{forward\_carry} + v_{burst} \cdot \cos(\theta_i) + \delta_{rand,z}$$
  $$v_{y,i} = v_{up} + \delta_{rand,y}$$

Where $v_{forward\_carry}$ is equal to the player's forward running velocity (e.g. $8.0 \text{ m/s}$), ensuring coins inherit momentum.

### 3.2 Airborne Integration & Gravity
Explicit semi-implicit Euler integration:
$$\vec{v}^{(t+\Delta t)} = \vec{v}^{(t)} + \vec{g} \Delta t$$
$$\vec{x}^{(t+\Delta t)} = \vec{x}^{(t)} + \vec{v}^{(t+\Delta t)} \Delta t$$
where $\vec{g} = (0, -25.0, 0) \text{ m/s}^2$.

### 3.3 Coefficient of Restitution & Ground Friction
For ground plane $y = y_{ground} = 0.2\text{m}$:
$$v_y \leftarrow -e \cdot v_y \quad \text{where } e = 0.4$$
$$\vec{v}_{xz} \leftarrow \vec{v}_{xz} \cdot (1 - \mu_{friction} \Delta t) \quad \text{where } \mu_{friction} = 5.0$$

### 3.4 Magnetic Attraction Acceleration
During magnetic phase:
$$\vec{v}_{coin} = \frac{\vec{x}_{player} - \vec{x}_{coin}}{\|\vec{x}_{player} - \vec{x}_{coin}\|} \cdot \left( v_{magnet\_base} + \frac{k_{boost}}{\max(0.1, d)} \right)$$
where $v_{magnet\_base} = 18.0 \text{ m/s}$ and $k_{boost} = 10.0 \text{ m}^2/\text{s}$.

---

## 4. Authoring & Baker Designs

### 4.1 `CoinGateAuthoring.cs`
Converts MonoBehaviours on Multiplier Gate prefabs into `CoinGateComponent`.

```csharp
using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class CoinGateAuthoring : MonoBehaviour
    {
        public GateOperation Operation = GateOperation.Multiply;
        public int Value = 2;
        public float CollisionRadius = 1.5f;
        public bool SingleUse = true;

        class Baker : Baker<CoinGateAuthoring>
        {
            public override void Bake(CoinGateAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CoinGateComponent
                {
                    Operation = authoring.Operation,
                    Value = authoring.Value,
                    Radius = authoring.CollisionRadius,
                    IsTriggered = false,
                    SingleUse = authoring.SingleUse
                });
            }
        }
    }
}
```

---

### 4.2 `CoinPhysicsAuthoring.cs`
Converts individual coin visuals into split coin entities.

```csharp
using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class CoinPhysicsAuthoring : MonoBehaviour
    {
        public float Gravity = 25.0f;
        public float AirDamping = 1.5f;
        public float BounceRestitution = 0.4f;
        public float Friction = 5.0f;
        public float MagnetSpeed = 18.0f;
        public float CollectRadius = 0.8f;

        class Baker : Baker<CoinPhysicsAuthoring>
        {
            public override void Bake(CoinPhysicsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CoinPhysicsComponent
                {
                    Velocity = Unity.Mathematics.float3.zero,
                    Gravity = authoring.Gravity,
                    AirDamping = authoring.AirDamping,
                    BounceRestitution = authoring.BounceRestitution,
                    Friction = authoring.Friction,
                    MagnetSpeed = authoring.MagnetSpeed,
                    CollectRadius = authoring.CollectRadius,
                    Phase = CoinPhysicsPhase.Airborne,
                    GroundTimer = 0f
                });
            }
        }
    }
}
```

---

### 4.3 `CoinSpawnerAuthoring.cs`
Attached to the Player entity or Level Manager to configure coin split spawning.

```csharp
using UnityEngine;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Authoring
{
    public class CoinSpawnerAuthoring : MonoBehaviour
    {
        public GameObject CoinPrefab;
        public float BurstImpulse = 7.0f;
        public float UpwardImpulse = 5.0f;
        public float SpreadAngle = 60.0f;

        class Baker : Baker<CoinSpawnerAuthoring>
        {
            public override void Bake(CoinSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                Entity coinPrefabEntity = Entity.Null;
                if (authoring.CoinPrefab != null)
                {
                    coinPrefabEntity = GetEntity(authoring.CoinPrefab, TransformUsageFlags.Dynamic);
                }

                AddComponent(entity, new CoinSpawnerComponent
                {
                    CoinPrefabEntity = coinPrefabEntity,
                    BurstImpulse = authoring.BurstImpulse,
                    UpwardImpulse = authoring.UpwardImpulse,
                    SpreadAngle = authoring.SpreadAngle
                });
            }
        }
    }
}
```

---

## 5. Generator Integration Details (`ToolkitExampleGenerator.cs`)

### 5.1 Programmatic Population of Slice 14 (`14_MoneyRush_Coins_Slice.prefab`)
In `Assets/Scripts/Editor/ToolkitExampleGenerator.cs`:

1. Separate `Coins` generation from generic `Swarm` block.
2. Build Player entity with `CoinSpawnerAuthoring` and split coin template prefab.
3. Spawn Multiplier Gates along track with `CoinGateAuthoring`.

```csharp
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
    coinPhys.Gravity = 25.0f;
    coinPhys.AirDamping = 1.5f;
    coinPhys.BounceRestitution = 0.4f;
    coinPhys.MagnetSpeed = 18.0f;
    coinPhys.CollectRadius = 0.8f;
    coinTemplate.SetActive(false);

    // 2. Attach CoinSpawnerAuthoring to Player
    var coinSpawner = player.AddComponent<CoinSpawnerAuthoring>();
    coinSpawner.CoinPrefab = coinTemplate;
    coinSpawner.BurstImpulse = 7.0f;
    coinSpawner.UpwardImpulse = 5.0f;
    coinSpawner.SpreadAngle = 60.0f;

    // 3. Spawn Multiplier Gates (+2, x3, +10, x2, x4)
    CreateCoinGate(root, new Vector3(-2, 1, -25), GateOperation.Add, 2, Color.cyan);
    CreateCoinGate(root, new Vector3(2, 1, -25), GateOperation.Multiply, 3, Color.green);
    CreateCoinGate(root, new Vector3(-2, 1, 0), GateOperation.Add, 10, Color.cyan);
    CreateCoinGate(root, new Vector3(2, 1, 0), GateOperation.Multiply, 2, Color.green);
    CreateCoinGate(root, new Vector3(0, 1, 20), GateOperation.Multiply, 4, Color.green);
}
```

Helper function in `ToolkitExampleGenerator.cs`:
```csharp
private static void CreateCoinGate(GameObject root, Vector3 pos, GateOperation op, int val, Color col)
{
    GameObject gate = GameObject.CreatePrimitive(PrimitiveType.Cube);
    gate.name = $"CoinGate_{op}_{val}";
    gate.transform.SetParent(root.transform);
    gate.transform.position = pos;
    gate.transform.localScale = new Vector3(3.5f, 2.5f, 0.5f);
    gate.GetComponent<Renderer>().sharedMaterial.color = col;

    var gateAuth = gate.AddComponent<CoinGateAuthoring>();
    gateAuth.Operation = op;
    gateAuth.Value = val;
    gateAuth.CollisionRadius = 1.8f;

    var tween = gate.AddComponent<MathTweenAuthoring>();
    tween.Property = TweenProperty.PositionY;
    tween.BaseValue = pos.y;
    tween.Amplitude = 0.3f;
    tween.Speed = 2f;
}
```

### 5.2 Verification Suite Update
In `RunVerificationSuite()`:
```csharp
else if (sliceName.Contains("Coins") || sliceName.Contains("MoneyRush"))
{
    var spawner = player.GetComponent<CoinSpawnerAuthoring>();
    report += $"       - CoinSpawnerAuthoring verified: BurstImpulse={spawner?.BurstImpulse}, UpwardImpulse={spawner?.UpwardImpulse}\n";
}
```

---

## 6. Verification Method

1. **System Signatures & Burst Compilation**:
   - Inspect signatures for `CoinMultiplierSystem` and `CoinPhysicsSystem`.
   - Verify `[UpdateInGroup(typeof(SimulationSystemGroup))]`, `[BurstCompile]`, and `EntityCommandBuffer` safety.
2. **Authoring Baker Equivalence**:
   - Verify authoring fields map 1:1 to DOTS components with correct `TransformUsageFlags.Dynamic`.
3. **Generator Execution**:
   - Verify `ToolkitExampleGenerator.cs` generates `14_MoneyRush_Coins_Slice.prefab` containing `CoinSpawnerAuthoring`, `CoinGateAuthoring`, and template `CoinPhysicsAuthoring`.
