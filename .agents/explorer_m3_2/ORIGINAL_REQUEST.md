## 2026-07-22T07:31:03Z
You are Explorer 2 for Milestone 3: Coin Multiplier & Splitting Physics (14_MoneyRush_Coins).
Your working directory is d:\Git\Hyper-Casual-Runner\.agents\explorer_m3_2.

Task:
Investigate system execution logic, coin splitting physics, and generator integration for slice 14.
1. Inspect existing systems in `Assets/Scripts/ECS/Systems/` and authoring in `Assets/Scripts/ECS/Authoring/` and `Assets/Scripts/Editor/ToolkitExampleGenerator.cs`.
2. Design pure DOTS systems:
   - `CoinMultiplierSystem` (processes player-gate trigger collisions, updates coin count, triggers spawning/splitting events).
   - `CoinPhysicsSystem` (calculates 3D parabolic/spread impulse trajectories for split coins, applies gravity/damping, updates positions, and collects split coins).
3. Design authoring bakers (`CoinGateAuthoring`, `CoinPhysicsAuthoring`) that convert Unity MonoBehaviours to DOTS entities.
4. Detail generator updates required in `Assets/Scripts/Editor/ToolkitExampleGenerator.cs` so that slice 14 (`14_MoneyRush_Coins_Slice.prefab`) is programmatically populated with valid multiplier gates (+2, x3) and splitting coin spawner components.
5. Document system signatures, physics formulas, and generator integration in `d:\Git\Hyper-Casual-Runner\.agents\explorer_m3_2\analysis.md`.
6. Deliver handoff report in `d:\Git\Hyper-Casual-Runner\.agents\explorer_m3_2\handoff.md`. Communicate completion to parent.
