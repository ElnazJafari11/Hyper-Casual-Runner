# Technical Analysis Report: Structural Change Remediation in CosmeticsShopSystem

**Target File**: `Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs`  
**Related Test File**: `Assets/Scripts/Editor/Tests/CosmeticsShopTests.cs`  
**Issue Type**: Forensic Audit Integrity Failure (`InvalidOperationException: Structural changes are not allowed while iterating over entities`)

---

## 1. Executive Summary

During Iteration 1 forensic auditing, unit test `HyperCasualRunner.Tests.CosmeticsShopTests.CosmeticsShopSystem_SufficientPrestige_DeductsPrestigeCurrencyAndUnlocksSkin` failed with an unhandled exception:
`InvalidOperationException: Structural changes are not allowed while iterating over entities. Please use EntityCommandBuffer instead.`

The failure stems from directly calling `state.EntityManager.CreateEntity()` and `state.EntityManager.AddComponentData()` inside nested `SystemAPI.Query` loops within `CosmeticsShopSystem.OnUpdate`. This report provides a complete root cause breakdown and specifies a non-circumventing fix strategy utilizing `EntityCommandBuffer(Allocator.Temp)`.

---

## 2. Technical Root Cause Breakdown

### Exact Code Location
In `Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs`:
```csharp
16: foreach (var (purchaseEvent, entity) in SystemAPI.Query<RefRO<CosmeticPurchaseEventComponent>>().WithEntityAccess())
17: {
...
22:     foreach (var stats in SystemAPI.Query<RefRW<PersistentPlayerStats>>())
23:     {
...
37:         var soundEntity = state.EntityManager.CreateEntity();
38:         state.EntityManager.AddComponentData(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
...
40:     }
...
56: }
```

### Safety Contract Violation in Unity Entities 1.0+
1. Direct `EntityManager` structural changes (e.g., `CreateEntity`, `DestroyEntity`, `AddComponent`, `RemoveComponent`) modify entity storage layout, chunk pointers, and archetype definitions.
2. When executing inside `foreach (var ... in SystemAPI.Query<...>())`, Unity Entities locks entity access to guarantee iterator safety and data integrity.
3. Invoking `state.EntityManager.CreateEntity()` during an active query invalidates safety handles and triggers `CheckIsStructuralChange()`, which immediately throws `InvalidOperationException`.

---

## 3. Evaluation of Fix Strategies

| Metric / Requirement | Strategy A: `new EntityCommandBuffer(Allocator.Temp)` | Strategy B: `EndSimulationEntityCommandBufferSystem.Singleton` |
| :--- | :--- | :--- |
| **DOTS Safety Rule Compliance** | Fully Compliant (defers operations until after loop) | Fully Compliant (defers operations to system group end) |
| **Unit Test Compatibility** | **100% Compatible** — requires no external World singletons | **Incompatible** without test rewrite — `CosmeticsShopTests` creates a bare `World` lacking `EndSimulationEntityCommandBufferSystem` singleton |
| **Codebase Consistency** | Matches 23 existing systems (`CollisionSystem`, `CombatSystem`, `AudioManagerSystem`) | Deviates from project standards |
| **Execution Timing** | Immediate playback after `Query` loop finishes within same frame | Deferred playback at end of `SimulationSystemGroup` |

### Recommended Strategy: Strategy A (`new EntityCommandBuffer(Allocator.Temp)`)
`new EntityCommandBuffer(Allocator.Temp)` records entity creation and component additions safely into buffer memory while iterating. Immediately following query completion, calling `ecb.Playback(state.EntityManager)` applies all recorded structural changes safely without violating iteration constraints, followed by `ecb.Dispose()`.

---

## 4. Proposed Code Fix Specification

### Proposed Full File Replacement for `Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs`

```csharp
using Unity.Entities;
using Unity.Collections;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.ECS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct CosmeticsShopSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CosmeticPurchaseEventComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (purchaseEvent, entity) in SystemAPI.Query<RefRO<CosmeticPurchaseEventComponent>>().WithEntityAccess())
            {
                int skinIndex = purchaseEvent.ValueRO.TargetSkinIndex;
                double cost = purchaseEvent.ValueRO.PrestigeCost;

                bool handled = false;
                foreach (var stats in SystemAPI.Query<RefRW<PersistentPlayerStats>>())
                {
                    handled = true;
                    if (GameProgressData.IsSkinUnlocked(skinIndex))
                    {
                        // Already unlocked: equip skin without deducting currency
                        GameProgressData.CurrentSkinIndex = skinIndex;
                    }
                    else if (stats.ValueRO.PrestigeCurrency >= cost)
                    {
                        // Deduct PrestigeCurrency, unlock skin, and equip skin
                        stats.ValueRW.PrestigeCurrency -= cost;
                        GameProgressData.UnlockSkin(skinIndex);
                        GameProgressData.CurrentSkinIndex = skinIndex;

                        var soundEntity = ecb.CreateEntity();
                        ecb.AddComponent(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                    }
                }

                // If no PersistentPlayerStats entity exists, fall back to checking/unlocking via GameProgressData directly if free/unlocked
                if (!handled)
                {
                    if (GameProgressData.IsSkinUnlocked(skinIndex))
                    {
                        GameProgressData.CurrentSkinIndex = skinIndex;
                    }
                }

                if (state.EntityManager.HasComponent<CosmeticPurchaseEventComponent>(entity))
                {
                    SystemAPI.SetComponentEnabled<CosmeticPurchaseEventComponent>(entity, false);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
```

### Proposed Unified Diff Patch
```patch
--- a/Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs
+++ b/Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs
@@ -1,4 +1,5 @@
 using Unity.Entities;
+using Unity.Collections;
 using HyperCasualRunner.ECS.Components;
 
 namespace HyperCasualRunner.ECS.Systems
@@ -13,6 +14,8 @@ namespace HyperCasualRunner.ECS.Systems
 
         public void OnUpdate(ref SystemState state)
         {
+            var ecb = new EntityCommandBuffer(Allocator.Temp);
+
             foreach (var (purchaseEvent, entity) in SystemAPI.Query<RefRO<CosmeticPurchaseEventComponent>>().WithEntityAccess())
             {
                 int skinIndex = purchaseEvent.ValueRO.TargetSkinIndex;
@@ -34,8 +37,8 @@ namespace HyperCasualRunner.ECS.Systems
                         GameProgressData.UnlockSkin(skinIndex);
                         GameProgressData.CurrentSkinIndex = skinIndex;
 
-                        var soundEntity = state.EntityManager.CreateEntity();
-                        state.EntityManager.AddComponentData(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
+                        var soundEntity = ecb.CreateEntity();
+                        ecb.AddComponent(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
                     }
                 }
 
@@ -53,6 +56,9 @@ namespace HyperCasualRunner.ECS.Systems
                     SystemAPI.SetComponentEnabled<CosmeticPurchaseEventComponent>(entity, false);
                 }
             }
+
+            ecb.Playback(state.EntityManager);
+            ecb.Dispose();
         }
     }
 }
```

---

## 5. Secondary Architectural Discovery

Codebase inspection revealed that `Assets/Scripts/ECS/Systems/Idle/PrestigeSystem.cs` (lines 45-46) contains the exact same anti-pattern:
```csharp
var soundEntity = state.EntityManager.CreateEntity();
state.EntityManager.AddComponentData(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Victory });
```
inside a `SystemAPI.Query<RefRO<PrestigeEventComponent>>()` loop. Implementers are strongly advised to apply an identical `EntityCommandBuffer(Allocator.Temp)` pattern to `PrestigeSystem.cs` to prevent future audit integrity failures.
