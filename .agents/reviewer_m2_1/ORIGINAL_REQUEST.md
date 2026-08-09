## 2026-07-22T07:21:35Z
You are Reviewer 1 for Milestone 2: Snake Follower Chain & Collision (5_JoinClash_Snake).
Your working directory is d:\Git\Hyper-Casual-Runner\.agents\reviewer_m2_1.
Read d:\Git\Hyper-Casual-Runner\PROJECT.md, d:\Git\Hyper-Casual-Runner\docs\project-context.md, and Worker 1 handoff in d:\Git\Hyper-Casual-Runner\.agents\worker_m2_1\handoff.md.

Task:
1. Initialize BRIEFING.md and progress.md in d:\Git\Hyper-Casual-Runner\.agents\reviewer_m2_1.
2. Review code changes made by Worker 1:
   - Assets/Scripts/ECS/Components/SnakeComponents.cs
   - Assets/Scripts/ECS/Authoring/SnakeChainAuthoring.cs, SnakeFollowerAuthoring.cs
   - Assets/Scripts/ECS/Systems/SnakeFollowerSystem.cs, SnakeCollisionSystem.cs
   - Assets/Scripts/Editor/ToolkitExampleGenerator.cs
   - Assets/Scripts/Editor/Tests/ToolkitGeneratorTests.cs
3. Verify:
   - Pure DOTS ECS Entities 1.0+ compliance ([BurstCompile], unmanaged structs, ISystem, IComponentData).
   - System ordering ([UpdateInGroup(typeof(SimulationSystemGroup))], [UpdateAfter(...)]).
   - Memory & ECB safety (dynamic buffers, entity destruction, event tag generation).
   - Generator correctness and verification output in verification_report.txt.
4. Write your detailed review report in d:\Git\Hyper-Casual-Runner\.agents\reviewer_m2_1\handoff.md with explicit pass/veto verdict.

Send a message when your handoff report is ready. DO NOT write or edit source code files directly.
