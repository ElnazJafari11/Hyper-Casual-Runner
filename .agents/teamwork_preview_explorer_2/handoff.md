# Handoff Report: Structural Change Violation Remediation in CosmeticsShopSystem

## 1. Observation
- **Audit Failure**: Test `HyperCasualRunner.Tests.CosmeticsShopTests.CosmeticsShopSystem_SufficientPrestige_DeductsPrestigeCurrencyAndUnlocksSkin` failed with exception:
  `Unhandled log message: '[Exception] InvalidOperationException: Structural changes are not allowed while iterating over entities. Please use EntityCommandBuffer instead.'.`
- **Location**: `Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs:37-38`
- **Verbatim Code Snippet**:
  ```csharp
  foreach (var (purchaseEvent, entity) in SystemAPI.Query<RefRO<CosmeticPurchaseEventComponent>>().WithEntityAccess())
  {
      foreach (var stats in SystemAPI.Query<RefRW<PersistentPlayerStats>>())
      {
          ...
          var soundEntity = state.EntityManager.CreateEntity();
          state.EntityManager.AddComponentData(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
      }
  }
  ```
- **Codebase Pattern Analysis**: 23 systems across `Assets/Scripts/ECS/Systems/` (such as `CollisionSystem.cs:13`, `CombatSystem.cs:13`, and `AudioManagerSystem.cs:13`) instantiate `new EntityCommandBuffer(Allocator.Temp)`, record structural modifications inside entity queries, and execute `ecb.Playback(state.EntityManager); ecb.Dispose();` after query completion.
- **Secondary Finding**: `Assets/Scripts/ECS/Systems/Idle/PrestigeSystem.cs:45-46` exhibits the exact same violation pattern inside its `PrestigeEventComponent` query loop.

## 2. Logic Chain
1. *From Observation 1*: Direct calls to `state.EntityManager.CreateEntity()` and `state.EntityManager.AddComponentData()` occur inside nested `SystemAPI.Query` iterations.
2. *From Logic Step 1*: In Unity Entities 1.0+, performing structural changes during active query iteration invalidates chunk allocations and iterator state, triggering `InvalidOperationException`.
3. *From Observation 2 & 3*: Using `new EntityCommandBuffer(Allocator.Temp)` defers `CreateEntity` and `AddComponent` commands into thread-safe buffer memory without modifying `EntityManager` archetype state during query iteration.
4. *From Logic Step 3*: Executing `ecb.Playback(state.EntityManager)` immediately after the `foreach` loops finish applies recorded structural changes safely when no query iterations are active.
5. *From Logic Step 4 & Observation 2*: Unlike `EndSimulationEntityCommandBufferSystem.Singleton` (which requires `EndSimulationEntityCommandBufferSystem` singleton in the `World`), `new EntityCommandBuffer(Allocator.Temp)` operates standalone and is 100% compatible with unit tests like `CosmeticsShopTests` which instantiate minimal test worlds.

## 3. Caveats
- **Read-Only Investigation Scope**: Explorer Agent 2 is restricted to read-only analysis. Source code edits to `CosmeticsShopSystem.cs` must be executed by an Implementer agent.
- **Secondary File Warning**: `PrestigeSystem.cs` was identified as having the same defect, but fix implementation for `PrestigeSystem.cs` is recommended alongside `CosmeticsShopSystem.cs` to prevent future audit failures.

## 4. Conclusion
The structural change violation in `CosmeticsShopSystem.cs` is resolved by wrapping entity/component creation in `new EntityCommandBuffer(Allocator.Temp)` and invoking `ecb.Playback(state.EntityManager)` post-iteration. Detailed analysis and diff patch are produced in `.agents/teamwork_preview_explorer_2/analysis.md`.

## 5. Verification Method
1. Apply the diff patch specified in `.agents/teamwork_preview_explorer_2/analysis.md` to `Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs`.
2. Execute Unity Test Runner (or batchmode test run):
   - Command: Run test `HyperCasualRunner.Tests.CosmeticsShopTests.CosmeticsShopSystem_SufficientPrestige_DeductsPrestigeCurrencyAndUnlocksSkin`
3. Invalidation condition: If `InvalidOperationException` or any structural change error is logged, the verification fails. Passing test with 0 exception logs confirms successful remediation.
