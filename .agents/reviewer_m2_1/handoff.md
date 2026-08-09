# Handoff & Review Report — Milestone 2: Snake Follower Chain & Collision (5_JoinClash_Snake)

**Reviewer**: Reviewer 1 (`reviewer_m2_1`)  
**Date**: 2026-07-22  
**Verdict**: **REQUEST_CHANGES**  

---

## 1. Observation

### 1.1 Reviewed Files & Verbatim Code Inspection
- **Components** (`Assets/Scripts/ECS/Components/SnakeComponents.cs`):
  - Structs `SnakeChainComponent`, `SnakeSegmentBuffer`, `SnakeFollowerLinkBuffer`, `SnakeFollowerComponent`, `SnakeJoinCollectibleComponent`, `SnakeObstacleComponent`, `SnakeJoinEventComponent`, `SnakeSeverEventComponent` are defined as unmanaged C# structs implementing `IComponentData` or `IBufferElementData`.
- **Authoring** (`Assets/Scripts/ECS/Authoring/SnakeChainAuthoring.cs`, `SnakeFollowerAuthoring.cs`):
  - `Baker<SnakeChainAuthoring>` properly converts GameObject references and attaches `SnakeChainComponent`, `SnakeSegmentBuffer`, and `SnakeFollowerLinkBuffer` with `TransformUsageFlags.Dynamic`.
- **Systems** (`Assets/Scripts/ECS/Systems/SnakeFollowerSystem.cs` & `SnakeCollisionSystem.cs`):
  - `SnakeFollowerSystem.cs`:
    - Lines 10–13: Attributes `[UpdateInGroup(typeof(SimulationSystemGroup))]`, `[UpdateAfter(typeof(PlayerMovementSystem))]`, `[BurstCompile]`. Implements `ISystem`.
    - Lines 85–99:
      ```csharp
      Entity follower = ecb.Instantiate(chain.ValueRO.FollowerPrefab);
      ...
      linkBuffer.Add(new SnakeFollowerLinkBuffer { FollowerEntity = follower });
      ```
    - Lines 108–109:
      ```csharp
      Entity tailEntity = linkBuffer[lastIdx].FollowerEntity;
      ecb.DestroyEntity(tailEntity);
      ```
    - Lines 121–122:
      ```csharp
      Entity followerEntity = linkBuffer[i].FollowerEntity;
      if (!transformLookup.HasComponent(followerEntity)) continue;
      ```
  - `SnakeCollisionSystem.cs`:
    - Lines 10–13: Attributes `[UpdateInGroup(typeof(SimulationSystemGroup))]`, `[UpdateAfter(typeof(SnakeFollowerSystem))]`, `[BurstCompile]`. Implements `ISystem`.
    - Lines 165–166:
      ```csharp
      Entity folEntity = linkBuffer[i].FollowerEntity;
      if (!transformLookup.HasComponent(folEntity)) continue;
      ```
- **Generator & Tests** (`Assets/Scripts/Editor/ToolkitExampleGenerator.cs` & `ToolkitGeneratorTests.cs`):
  - `ToolkitExampleGenerator.cs`: Correctly sets up `5_JoinClash_Snake` slice with `SnakeChainAuthoring`, standalone recruit collectibles, math gates, and hazard obstacles. Generates `verification_report.txt` with `21/21 Playable Slices verified`.
  - `ToolkitGeneratorTests.cs`: Unit test `JoinClash_ContainsSnakeChainAuthoring` passes by checking prefab existence and component attachment on disk.

---

## 2. Logic Chain

1. **DOTS EntityCommandBuffer & Deferred Entity Lifetime**:
   - Calling `ecb.Instantiate(prefab)` returns a **deferred placeholder Entity handle** (where `Index < 0`, e.g., `Index = -1`).
   - When `linkBuffer.Add(new SnakeFollowerLinkBuffer { FollowerEntity = follower })` is called directly on the live `DynamicBuffer` in `EntityManager` (line 99 of `SnakeFollowerSystem.cs`), the dynamic buffer stores `Entity { Index = -1 }`.
   - `EntityCommandBuffer.Playback` (line 162) ONLY remaps entity handles contained inside commands recorded within that ECB (e.g. `ecb.AddComponent`, `ecb.SetComponent`, `ecb.AppendToBuffer`). It **does not** scan or remap entity handles inside dynamic buffers modified directly by user C# code (`linkBuffer.Add`).
2. **Impact on Runtime Simulation**:
   - **Follower Movement Failure**: In `SnakeFollowerSystem.cs` line 121, `transformLookup.HasComponent(followerEntity)` is queried on `Entity { Index = -1 }`, which evaluates to `false` in every frame. The follower entity is instantiated at `spawnPos` during ECB playback, but its transform is **never updated** along the history path curve.
   - **Follower Destruction Failure**: In `SnakeFollowerSystem.cs` line 109, `ecb.DestroyEntity(tailEntity)` receives `Entity { Index = -1 }` instead of a valid entity handle, resulting in failed destructions and entity memory leaks.
   - **Collision Detection Failure**: In `SnakeCollisionSystem.cs` line 166, `transformLookup.HasComponent(folEntity)` evaluates to `false` for all followers in `linkBuffer`. Follower body segments become completely ghost-like and pass through obstacles without triggering collisions or severing.
3. **Test Masking**:
   - `ToolkitGeneratorTests.cs` only asserts that the editor GameObject prefab has `SnakeChainAuthoring` attached. It does not execute the ECS runtime simulation (`SnakeFollowerSystem`), allowing this critical ECB corruption bug to go undetected in the test suite.

---

## 3. Findings & Review Summary

### Review Summary
- **Verdict**: **REQUEST_CHANGES**
- **Quality Score**: 6/10 (Clean authoring and component structure, correct system attributes, but critical DOTS ECB handle corruption bug rendering runtime mechanics non-functional).

---

### Detailed Findings

#### [Critical] Finding 1: Deferred ECB Handle Stored Directly in DynamicBuffer (`SnakeFollowerSystem.cs`)
- **Location**: `Assets/Scripts/ECS/Systems/SnakeFollowerSystem.cs`, lines 85–99 & 103–111
- **Why this is a problem**: `linkBuffer.Add(...)` stores an un-remapped deferred `Entity` handle (`Index = -1`) directly into the entity's dynamic buffer. ECB playback does not remap direct buffer edits. This permanently corrupts `linkBuffer`, breaking follower transform positioning, entity destruction, and follower collision detection.
- **Required Fix**:
  1. Replace `linkBuffer.Add(new SnakeFollowerLinkBuffer { FollowerEntity = follower });` with:
     ```csharp
     ecb.AppendToBuffer(headEntity, new SnakeFollowerLinkBuffer { FollowerEntity = follower });
     ```
  2. When iterating over `linkBuffer` in step 3 of `SnakeFollowerSystem.cs` and section 4 of `SnakeCollisionSystem.cs`, filter out un-remapped placeholder entities (e.g., check `followerEntity.Index >= 0` and `transformLookup.HasComponent(followerEntity)`).

#### [Major] Finding 2: Inefficient History Buffer Shifting (`SnakeFollowerSystem.cs`)
- **Location**: `Assets/Scripts/ECS/Systems/SnakeFollowerSystem.cs`, lines 48–75
- **Why this is a problem**: `historyBuffer.Insert(0, ...)` performs an $O(N)$ memory copy shift of all elements in the `DynamicBuffer` every time head movement exceeds 0.01 units. Under high segment counts or long trails, this creates unnecessary memory churn and reduces Burst execution throughput.
- **Suggested Fix**: Append new samples to the end of `historyBuffer` (`historyBuffer.Add(...)`) and compute trail distances backwards from the latest index, or maintain a ring buffer index.

#### [Minor] Finding 3: Math Gate Division Truncation Inconsistency (`SnakeCollisionSystem.cs`)
- **Location**: `Assets/Scripts/ECS/Systems/SnakeCollisionSystem.cs`, line 68
- **Why this is a problem**: `newLen = gate.ValueRO.Value != 0 ? currentLen / gate.ValueRO.Value : currentLen;` uses standard C# integer division truncating fractions. If `currentLen < gate.ValueRO.Value`, length drops directly to `0`. Ensure this behavior aligns with `SwarmMechanicsSystem` math gate semantics across the toolkit.

---

## 4. Verified Claims

| Claim | Verified Via | Result |
|-------|--------------|--------|
| Pure DOTS ECS Entities 1.0+ compliance (`[BurstCompile]`, `ISystem`, `IComponentData`, `IBufferElementData`) | Static code analysis of `SnakeComponents.cs`, `SnakeFollowerSystem.cs`, `SnakeCollisionSystem.cs` | **PASS** |
| System Ordering (`SimulationSystemGroup`, `[UpdateAfter(...)]`) | Inspection of `[UpdateInGroup]` and `[UpdateAfter]` attributes on systems | **PASS** |
| Generator Correctness & `verification_report.txt` | Inspection of `ToolkitExampleGenerator.cs` and `verification_report.txt` | **PASS** |
| Unit Test `JoinClash_ContainsSnakeChainAuthoring` | Inspection of `ToolkitGeneratorTests.cs` | **PASS** (Editor asset level only) |
| Runtime ECB & Dynamic Buffer Safety | Structural trace of ECB entity instantiation and `linkBuffer.Add` remapping | **FAIL** (Deferred handle corruption) |

---

## 5. Caveats
- No caveats. Investigation of ECB entity handle lifetime, dynamic buffer mutation, component lookups, and test scope is complete and verified.

---

## 6. Conclusion
Worker 1 implemented clean component structs, authoring scripts, and editor generator integration. However, the runtime implementation in `SnakeFollowerSystem.cs` contains a critical DOTS Entities 1.0+ ECB handle lifetime bug where deferred entity handles (`Index = -1`) are added directly to `linkBuffer` via `linkBuffer.Add(...)`. This prevents ECB entity remapping upon playback, breaking follower trailing movement, follower segment destruction, and follower collision detection.

**Verdict**: **REQUEST_CHANGES**. Worker 1 must update `SnakeFollowerSystem.cs` to use `ecb.AppendToBuffer(...)` and add valid entity filtering before re-submitting for review.

---

## 7. Verification Method (For Independent Verification)
1. Inspect `Assets/Scripts/ECS/Systems/SnakeFollowerSystem.cs` at line 99: observe `linkBuffer.Add(new SnakeFollowerLinkBuffer { FollowerEntity = follower });` where `follower` was created via `ecb.Instantiate`.
2. Trace `follower` handle: `ecb.Instantiate` returns a deferred entity handle with negative index. Direct `linkBuffer.Add` bypasses ECB command remapping during `ecb.Playback`.
3. Check `transformLookup.HasComponent(followerEntity)` at line 121: observe that `followerEntity` retains `Index = -1`, causing `HasComponent` to return `false` on every frame.
