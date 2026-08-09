## 2026-07-22T20:09:35Z
<USER_REQUEST>
You are Explorer Agent 2 for the Hyper-Casual Runner Toolkit project (Iteration 2 Remediation).
Your working directory is `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_explorer_2/`.

A FORENSIC AUDIT FAILURE (INTEGRITY VIOLATION) occurred during Iteration 1. Below is the Forensic Auditor's FULL evidence report:

--------------------------------------------------------------------------------
FULL AUDIT EVIDENCE REPORT:
Work Product: Hyper-Casual Runner Toolkit (Milestones 1, 2, 3)
Verdict: INTEGRITY VIOLATION

Observation:
1. Batchmode Test Execution & Compilation:
   - Compilation: 0 compilation errors.
   - Test Summary: 37 total tests executed, 36 passed, 1 failed.
   - Failed Test: `HyperCasualRunner.Tests.CosmeticsShopTests.CosmeticsShopSystem_SufficientPrestige_DeductsPrestigeCurrencyAndUnlocksSkin`
   - Verbatim Error Output:
     `Unhandled log message: '[Exception] InvalidOperationException: Structural changes are not allowed while iterating over entities. Please use EntityCommandBuffer instead.'. Use UnityEngine.TestTools.LogAssert.Expect`
     `Unity.Entities.EntityDataAccess.CheckIsStructuralChange ()`
     `Unity.Entities.EntityDataAccess.BeforeStructuralChange ()`
     `Unity.Entities.EntityDataAccess.BeginStructuralChanges ()`
     `Unity.Entities.EntityManager.AddComponent (Unity.Entities.Entity entity, Unity.Entities.ComponentType componentType)`
     `Unity.Entities.EntityManager.AddComponentData[T] (Unity.Entities.Entity entity, T componentData)`
     `HyperCasualRunner.ECS.Systems.CosmeticsShopSystem.OnUpdate (Unity.Entities.SystemState& state) (at Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs:38)`

2. Source File & Structural Violations:
   - File: `Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs:37-38`
   - Code Snippet:
     ```csharp
     foreach (var (purchaseEvent, entity) in SystemAPI.Query<RefRO<CosmeticPurchaseEventComponent>>().WithEntityAccess())
     {
         foreach (var stats in SystemAPI.Query<RefRW<PersistentPlayerStats>>())
         {
             var soundEntity = state.EntityManager.CreateEntity();
             state.EntityManager.AddComponentData(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
         }
     }
     ```
   - Calling `state.EntityManager.CreateEntity()` and `state.EntityManager.AddComponentData()` directly inside a `SystemAPI.Query` loop attempts an immediate structural change while iterating over entities, violating Unity DOTS Entities 1.0+ safety contracts.
--------------------------------------------------------------------------------

Your task:
1. Analyze `Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs` and the exact structural change violation identified by the auditor.
2. Produce a clear, non-circumventing fix strategy using an `EntityCommandBuffer` (ECB) — e.g. `var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);` or `new EntityCommandBuffer(Allocator.Temp)` — to safely record entity creation and component additions without violating DOTS entity iteration rules.
3. Write your analysis and fix specification to `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_explorer_2/analysis.md` and deliver `handoff.md`. Send completion message back to orchestrator.
</USER_REQUEST>
