## 2026-07-22T20:12:07Z
You are Worker Agent 4 (Milestone 3 Audit Remediation).
Your working directory is `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_worker_m3_remediation/`.
Read `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_explorer_2/analysis.md` and `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_explorer_2/handoff.md` for context.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

Your task:
1. Update `Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs` to use `new EntityCommandBuffer(Allocator.Temp)` for recording `CreateEntity()` and `AddComponent()` operations during query iteration, playing back via `ecb.Playback(state.EntityManager)` post-iteration per the exact code specification in `analysis.md`.
2. Update `Assets/Scripts/ECS/Systems/Idle/PrestigeSystem.cs` to use the same `EntityCommandBuffer(Allocator.Temp)` pattern for entity creation inside `OnUpdate`.
3. Execute the Unity CLI edit-mode test suite to confirm 100% passing tests (37/37 passed, 0 failed, 0 exceptions).
4. Write your handoff report to `d:/Git/Hyper-Casual-Runner/.agents/teamwork_preview_worker_m3_remediation/handoff.md` and update `progress.md`. Send completion message back to orchestrator.
