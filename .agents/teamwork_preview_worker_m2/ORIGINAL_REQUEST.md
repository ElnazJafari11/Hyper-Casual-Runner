## 2026-07-22T19:55:12Z

You are the Worker agent for Milestone 2: Advanced Obstacle Variants (DOTS ECS).
Your working directory is `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_worker_m2/`.
Read `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_explorer_1/analysis.md` and `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_explorer_1/handoff.md` for context.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

Requirements for Milestone 2:
1. Implement 3 new Authoring scripts in `Assets/Scripts/ECS/Authoring/`:
   - `MovingWallAuthoring.cs`
   - `PendulumSwingAuthoring.cs`
   - `SplittingHazardAuthoring.cs`
2. Implement 3 new DOTS Systems in `Assets/Scripts/ECS/Systems/`:
   - `MovingWallSystem.cs` (translates wall along configured movement axis vector via sine wave)
   - `PendulumSwingSystem.cs` (rotates pendulum around swing axis arc)
   - `SplittingHazardSystem.cs` (instantiates child hazard prefabs via EntityCommandBuffer on trigger distance and destroys parent entity with audio/vfx event tags)
3. Implement required unmanaged `IComponentData` structs (`MovingWallComponent`, `PendulumSwingComponent`, `SplittingHazardComponent`) in `Assets/Scripts/ECS/Components/` or within authoring/system files using namespace `HyperCasualRunner.ECS.Components`.
4. Create unit tests for obstacle components and math calculations in `Assets/Scripts/Editor/Tests/AdvancedObstaclesTests.cs`.
5. Run compilation check via Unity CLI or test suite and include exact test output.
6. Write your handoff report to `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_worker_m2/handoff.md` and update `progress.md`. Send completion message back to orchestrator.
