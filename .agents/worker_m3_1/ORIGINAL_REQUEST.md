## 2026-07-22T10:32:49Z
<USER_REQUEST>
You are Worker 1 for Milestone 3: Coin Multiplier & Splitting Physics (14_MoneyRush_Coins).
Your working directory is d:\Git\Hyper-Casual-Runner\.agents\worker_m3_1.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

Task Scope & Technical Requirements:
Implement Milestone 3 according to the technical specification in `d:\Git\Hyper-Casual-Runner\.agents\orchestrator\m3_design.md` and Explorer reports in `d:\Git\Hyper-Casual-Runner\.agents\explorer_m3_1\analysis.md` and `d:\Git\Hyper-Casual-Runner\.agents\explorer_m3_2\analysis.md`.

1. **ECS Components** (`Assets/Scripts/ECS/Components/CoinMultiplierComponents.cs`):
   - `CoinMultiplierGateComponent` (MultiplierType, IsTriggered, Value, GateWidth, TriggerDepth, MinimumOutput)
   - `CoinSplitPhysicsComponent` (CurrentVelocity, SpreadAngle, ImpulseSpeed, StackHeightOffset, Lifetime, MaxLifetime, GravityMultiplier, CoinCount, IsGrounded, IsCollectible)
   - `PlayerCoinRunnerComponent` (CoinVisualPrefab, CurrentCoinCount, StackSpacing, MaxStackHeight, SwerveSensitivity)
   - `CoinTag`, `CoinMultiplierGateTag`, `CoinMultiplierEventComponent`, `CoinSplitEventComponent`, `CoinStackElement`
2. **Authoring & Bakers** (`Assets/Scripts/ECS/Authoring/`):
   - `CoinGateAuthoring.cs` (bakes `CoinMultiplierGateComponent` and `CoinMultiplierGateTag`)
   - `CoinPhysicsAuthoring.cs` (bakes `CoinSplitPhysicsComponent` and `CoinTag`)
   - `CoinSpawnerAuthoring.cs` (bakes `PlayerCoinRunnerComponent`)
3. **Pure DOTS Systems** (`Assets/Scripts/ECS/Systems/`):
   - `CoinMultiplierSystem.cs` (`SimulationSystemGroup`, `[UpdateBefore(typeof(CoinPhysicsSystem))]`): Bounding overlap detection, updates `CurrentCoinCount`, marks gate triggered, emits audio/VFX tags (`PlaySoundEventComponent`, `DestroyEventComponent`) and `CoinSplitEventComponent`.
   - `CoinPhysicsSystem.cs` (`SimulationSystemGroup`): 3-phase physics (airborne parabolic, ground bounce/damping, magnetic collection towards player).
4. **Generator Update** (`Assets/Scripts/Editor/ToolkitExampleGenerator.cs`):
   - Update slice 14 generator logic to attach `CoinSpawnerAuthoring`, `CoinGateAuthoring` (+2, x3, +10, x2, x4), and split coin templates to `14_MoneyRush_Coins_Slice.prefab`.
   - Update `RunVerificationSuite()` to verify slice 14 prefab and authoring components.
5. **NUnit Tests**:
   - Update/add tests in `Assets/Scripts/Editor/Tests/` validating `CoinMultiplierSystem`, `CoinPhysicsSystem`, and slice 14 prefab verification.
6. **Build & Test Verification**:
   - Verify code compiles cleanly with zero compilation errors.
   - Run tests and log results in your handoff report.
7. Document all changes and build/test outputs in `d:\Git\Hyper-Casual-Runner\.agents\worker_m3_1\handoff.md` and notify parent.
</USER_REQUEST>
