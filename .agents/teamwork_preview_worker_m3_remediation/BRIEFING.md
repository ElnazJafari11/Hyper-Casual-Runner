# BRIEFING — 2026-07-22T23:12:07+03:00

## Mission
Remediate structural change violations in `CosmeticsShopSystem.cs` and `PrestigeSystem.cs` using `EntityCommandBuffer(Allocator.Temp)` and verify with Unity CLI edit-mode test suite.

## 🔒 My Identity
- Archetype: implementer/qa
- Roles: implementer, qa, specialist
- Working directory: d:\Git\Hyper-Casual-Runner\.agents\teamwork_preview_worker_m3_remediation
- Original parent: 77518b05-575e-4a6f-8a64-beed938515a6
- Milestone: Milestone 3 Audit Remediation

## 🔒 Key Constraints
- Use `new EntityCommandBuffer(Allocator.Temp)` for recording `CreateEntity()` and `AddComponent()` operations during query iteration.
- Play back via `ecb.Playback(state.EntityManager)` and `ecb.Dispose()` post-iteration.
- Execute Unity CLI edit-mode test suite to confirm 100% passing tests (37/37 passed, 0 failed, 0 exceptions).
- Write handoff report to `handoff.md` and update `progress.md`.
- Integrity Mandate: No hardcoded test results, fake implementations, or shortcuts.

## Current Parent
- Conversation ID: 77518b05-575e-4a6f-8a64-beed938515a6
- Updated: 2026-07-22T23:12:07+03:00

## Task Summary
- **What to build**: Update `CosmeticsShopSystem.cs` and `PrestigeSystem.cs` to use `EntityCommandBuffer(Allocator.Temp)` to avoid `InvalidOperationException` due to structural changes during entity iteration.
- **Success criteria**: 37/37 Unity edit-mode tests pass with 0 errors/exceptions.
- **Interface contracts**: `EntityCommandBuffer(Allocator.Temp)` pattern post-iteration playback.
- **Code layout**: `Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs`, `Assets/Scripts/ECS/Systems/Idle/PrestigeSystem.cs`.

## Key Decisions Made
- Use Strategy A (`EntityCommandBuffer(Allocator.Temp)`) as specified in analysis.md and prompt.

## Change Tracker
- **Files modified**: [TBD]
- **Build status**: [TBD]
- **Pending issues**: [TBD]

## Quality Status
- **Build/test result**: [TBD]
- **Lint status**: [TBD]
- **Tests added/modified**: [TBD]

## Loaded Skills
- None required for this task.

## Artifact Index
- `handoff.md` — Final handoff report
- `progress.md` — Progress tracker
