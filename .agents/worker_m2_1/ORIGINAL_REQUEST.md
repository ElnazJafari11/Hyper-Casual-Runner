## 2026-07-22T07:05:39Z
You are Worker 1 for Milestone 2: Snake Follower Chain & Collision (5_JoinClash_Snake).
Your working directory is d:\Git\Hyper-Casual-Runner\.agents\worker_m2_1.
Read d:\Git\Hyper-Casual-Runner\PROJECT.md, d:\Git\Hyper-Casual-Runner\.agents\orchestrator\plan.md, and the three Explorer reports in:
- d:\Git\Hyper-Casual-Runner\.agents\explorer_m2_1\handoff.md
- d:\Git\Hyper-Casual-Runner\.agents\explorer_m2_2\handoff.md
- d:\Git\Hyper-Casual-Runner\.agents\explorer_m2_3\handoff.md

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

Tasks for Milestone 2:
1. Initialize your working directory: create BRIEFING.md and progress.md in d:\Git\Hyper-Casual-Runner\.agents\worker_m2_1.
2. Create component definitions in `Assets/Scripts/ECS/Components/SnakeComponents.cs`:
   - `SnakeChainComponent` (unmanaged IComponentData)
   - `SnakeSegmentBuffer` (IBufferElementData)
   - `SnakeFollowerLinkBuffer` (IBufferElementData)
   - `SnakeFollowerComponent` (unmanaged IComponentData)
   - `SnakeJoinCollectibleComponent` (unmanaged IComponentData)
   - `SnakeObstacleComponent` (unmanaged IComponentData)
   - `SnakeJoinEventComponent` & `SnakeSeverEventComponent` (unmanaged IComponentData tags)
3. Create authoring scripts in `Assets/Scripts/ECS/Authoring/`:
   - `SnakeChainAuthoring.cs` (MonoBehaviour + Baker<SnakeChainAuthoring>)
   - `SnakeFollowerAuthoring.cs` (MonoBehaviour + Baker<SnakeFollowerAuthoring>)
4. Create pure DOTS ECS systems in `Assets/Scripts/ECS/Systems/`:
   - `SnakeFollowerSystem.cs` ([BurstCompile] unmanaged ISystem struct in SimulationSystemGroup, [UpdateAfter(typeof(PlayerMovementSystem))])
   - `SnakeCollisionSystem.cs` ([BurstCompile] unmanaged ISystem struct in SimulationSystemGroup, [UpdateAfter(typeof(SnakeFollowerSystem))])
5. Update `Assets/Scripts/Editor/ToolkitExampleGenerator.cs`:
   - Fix generator misrouting for Snake vs Stack at line 221.
   - Add dedicated `else if (prefabName.Contains("Snake") || prefabName.Contains("JoinClash"))` generator logic for 5_JoinClash_Snake slice.
   - Add verification clause for `5_JoinClash_Snake` in `RunVerificationSuite()`.
6. Add unit test `JoinClash_ContainsSnakeChainAuthoring` in `Assets/Scripts/Editor/Tests/ToolkitGeneratorTests.cs`.
7. Run Unity verification suite & unit tests to confirm `5_JoinClash_Snake_Slice.prefab` passes with 0 compilation errors and 21/21 slices pass.
8. Write complete handoff report in `d:\Git\Hyper-Casual-Runner\.agents\worker_m2_1\handoff.md` detailing changes, file paths, build/test results, and verification output.

Send a message when your handoff report is ready.
