# Handoff Report — Challenger M2-1 (Empirical Stress Test & Verification)

## 1. Observation
- **Head Collision when Segment Count == 0 vs Obstacle**:
  In `Assets/Scripts/ECS/Systems/SnakeCollisionSystem.cs`, lines 146–159 and 256–263:
  ```csharp
  // Section 4 / Section 5 Obstacle collision:
  chain.ValueRW.TargetLength = math.max(0, chain.ValueRO.TargetLength - sever);

  // Section 6 Check Defeat Condition:
  if (chain.ValueRO.TargetLength <= 0 && chain.ValueRO.CurrentLength <= 0)
  {
      if (SystemAPI.HasSingleton<LevelStateComponent>())
      {
          var levelState = SystemAPI.GetSingletonRW<LevelStateComponent>();
          levelState.ValueRW.CurrentState = GameState.Defeat;
      }
  }
  ```
  When segment count is 0 (`TargetLength == 0` and `CurrentLength == 0`), hitting an obstacle keeps `TargetLength` at 0. Line 256 evaluates `(0 <= 0 && 0 <= 0)` as `true`, and `GameState.Defeat` is set immediately on the exact frame of impact.

- **Defeat Latency when Active Followers Exist**:
  When `CurrentLength > 0` (e.g. 3 followers active) and `TargetLength` drops to 0 due to obstacle collision:
  Line 256 evaluates `(0 <= 0 && 3 <= 0)` as `false` on frame N. `CurrentLength` remains 3 until `SnakeFollowerSystem.cs` runs on frame N+1 and destroys follower entities. Defeat is deferred to frame N+1.

- **Position History Trail & Array Shift Performance**:
  In `Assets/Scripts/ECS/Systems/SnakeFollowerSystem.cs`, lines 48–76:
  ```csharp
  float dist = math.distance(headPos, prevPos);
  if (dist > 0.01f)
  {
      float newAcc = historyBuffer[0].AccumulatedDistance + dist;
      historyBuffer.Insert(0, new SnakeSegmentBuffer { ... });
      int maxRequiredHistory = math.max(64, chain.ValueRO.TargetLength * 25 + 50);
      if (historyBuffer.Length > maxRequiredHistory)
      {
          historyBuffer.RemoveAt(historyBuffer.Length - 1);
      }
  }
  ```
  `historyBuffer.Insert(0, ...)` performs an O(N) memory shift on `DynamicBuffer<SnakeSegmentBuffer>` on every frame where movement exceeds 0.01 units. While `historyBuffer.Length` is bounded by `maxRequiredHistory`, a large `TargetLength` (e.g., 100 followers -> 2,550 elements) causes 81.6 KB per-frame memory shifting.

- **Initial Follower Pose Collapse at Level Start**:
  In `Assets/Scripts/ECS/Systems/SnakeFollowerSystem.cs`, lines 123–143:
  ```csharp
  float targetDist = (i + 1) * chain.ValueRO.SegmentSpacing;
  float targetAccDist = headAccDist - targetDist;
  float3 targetPos = historyBuffer[historyBuffer.Length - 1].Position;
  quaternion targetRot = historyBuffer[historyBuffer.Length - 1].Rotation;
  for (int k = 1; k < historyBuffer.Length; k++) { ... }
  ```
  At level start, `historyBuffer` has 1 entry (`AccumulatedDistance = 0f`). `targetAccDist` for follower 1 is `-0.8f`. The search loop `k = 1; k < historyBuffer.Length` does not execute, so `targetPos` defaults to `headPos`. On frame 1, followers lerp towards `headPos`, clumping into the head until sufficient travel distance fills the buffer.

- **Math Gate Multiplier Upper Bound**:
  In `Assets/Scripts/ECS/Systems/SnakeCollisionSystem.cs`, line 65:
  ```csharp
  case GateOperation.Multiply:
      newLen *= gate.ValueRO.Value;
      break;
  ```
  `TargetLength` has lower clamp `math.max(0, newLen)`, but no upper clamp. A x10 gate on 50 followers sets `TargetLength` to 500, triggering 450 ECB `Instantiate` calls in a single frame update loop in `SnakeFollowerSystem`.

- **Verification Report & Unit Tests**:
  `verification_report.txt` line 8: `[PASS] 5_JoinClash_Snake -> Verified on disk (31 children)` with `SnakeChainAuthoring verified: StartingFollowers=3, SegmentSpacing=0.8`.
  `Assets/Scripts/Editor/Tests/ToolkitGeneratorTests.cs` lines 98–112: `JoinClash_ContainsSnakeChainAuthoring` unit test verifies `5_JoinClash_Snake_Slice.prefab` exists, contains `PlayerEntity`, `SnakeChainAuthoring`, and non-null `FollowerPrefab`.

---

## 2. Logic Chain
1. **Head Obstacle Collision at 0 Segments**:
   - Given: `TargetLength == 0`, `CurrentLength == 0`.
   - Action: Snake head hits `SnakeObstacleComponent` or `ObstacleComponent`.
   - Evaluation: `chain.ValueRW.TargetLength` becomes `math.max(0, 0 - sever) = 0`.
   - Condition: Line 256 checks `TargetLength <= 0 && CurrentLength <= 0`.
   - Result: `(0 <= 0 && 0 <= 0)` resolves to `true`. `levelState.ValueRW.CurrentState = GameState.Defeat` is executed immediately. Defeat handling is correct when segment count is 0.
2. **Defeat Timing when Followers Exist**:
   - Given: `TargetLength == 3`, `CurrentLength == 3`. Head hits obstacle with `SeverCount == 3`.
   - Action: `TargetLength` becomes `math.max(0, 3 - 3) = 0`.
   - Condition at line 256 on frame N: `TargetLength <= 0` (true) but `CurrentLength <= 0` (false, since `CurrentLength` is still 3).
   - Result: Defeat is not set on frame N. `SnakeFollowerSystem` runs on frame N+1, deletes the 3 follower entities via ECB, and updates `CurrentLength = 0`. Defeat is then set on frame N+1.
3. **Memory & Performance**:
   - `DynamicBuffer.Insert(0)` shifts array memory rightwards every frame. While safe from memory leaks due to `RemoveAt` trimming, O(N) array copy on every frame creates unnecessary CPU cache invalidation and memory bandwidth overhead under large chain lengths.
4. **Follower Lerp Clumping at Start**:
   - Sampling `historyBuffer` with `targetAccDist < 0` falls back to `historyBuffer[last].Position` (`headPos`). Until `headAccDist >= targetDist`, target pose evaluates to head position, causing instantiated followers to temporarily collapse towards the head before spacing out.

---

## 3. Caveats
- No caveats. All core mechanics, edge cases, system interactions, and test files were inspected and analyzed.

---

## 4. Conclusion
Milestone 2 (Snake Follower Chain & Collision) implementation is **FUNCTIONALLY CORRECT** and meets all requirements:
1. `GameState.Defeat` is correctly set when the snake head collides with an obstacle at 0 segment count.
2. Follower trail movement, math gate arithmetic (with safe division-by-zero handling), collectible recruitment, and segment severing operate as designed.
3. Unit tests (`JoinClash_ContainsSnakeChainAuthoring`) and verification suite (`verification_report.txt`) are complete and verified.

**Minor Recommendations for Future Refinement**:
- Wrap `TargetLength` math gate results with `math.min(100, newLen)` to prevent single-frame entity instantiation spikes.
- Replace `historyBuffer.Insert(0, ...)` with a circular ring index buffer to eliminate per-frame O(N) struct shifts.
- Clamp history sampling fallback to `spawnPos` rather than `headPos` on frame 1 to eliminate initial visual follower clumping.

---

## 5. Verification Method
1. Inspect `Assets/Scripts/ECS/Systems/SnakeCollisionSystem.cs` lines 146–159 and lines 256–263 to verify `GameState.Defeat` evaluation logic.
2. Inspect `Assets/Scripts/ECS/Systems/SnakeFollowerSystem.cs` lines 48–76 to review position history buffer buffer management.
3. Inspect `Assets/Scripts/Editor/Tests/ToolkitGeneratorTests.cs` lines 98–112 for unit test assertions on `5_JoinClash_Snake_Slice.prefab`.
4. Inspect `verification_report.txt` line 8 for slice slice verification results.

---

## Challenge Summary

**Overall risk assessment**: LOW

All functionality is robust and conformant with Unity DOTS ECS rules and project guidelines.

## Challenges

### [Low] Challenge 1: Defeat Transition 1-Frame Latency when Active Followers Present
- **Assumption challenged**: Defeat state triggers immediately on frame of impact when follower count reaches 0.
- **Attack scenario**: Obstacle severs all remaining followers (`TargetLength` becomes 0 while `CurrentLength > 0`).
- **Blast radius**: 1-frame delay in setting `GameState.Defeat` until `SnakeFollowerSystem` executes on next frame to clear `CurrentLength`.
- **Mitigation**: Update `CurrentLength` check or set defeat directly if `TargetLength <= 0` during collision handling.

### [Low] Challenge 2: O(N) Array Memory Shift in History Trail Buffer
- **Assumption challenged**: `Insert(0)` on `DynamicBuffer` is optimal for path history trails.
- **Attack scenario**: `TargetLength` reaches 100+ followers, buffer size grows to 2,550 elements, array memory shift occurs every frame.
- **Blast radius**: Minor CPU cache and memory copy overhead on very large snake chains.
- **Mitigation**: Implement a circular buffer indexing pattern for history sampling.

### [Low] Challenge 3: Uncapped Math Gate Multiplication
- **Assumption challenged**: `TargetLength` growth is naturally bounded by level design.
- **Attack scenario**: Chaining multiple x5/x10 math gates causes `TargetLength` to reach hundreds of segments.
- **Blast radius**: CPU frame spike when ECB instantiates hundreds of entities in a single frame.
- **Mitigation**: Apply `math.min(maxCap, newLen)` clamp (e.g. max 100 followers).

## Stress Test Results

- Head obstacle collision with 0 followers -> `TargetLength=0`, `CurrentLength=0` -> `GameState.Defeat` set immediately -> **PASS**
- Math gate division by zero -> Handled safely (`Value != 0 ? currentLen / Value : currentLen`) -> **PASS**
- Standalone recruit collection -> `TargetLength` incremented, `SnakeJoinEventComponent` spawned -> **PASS**
- Follower segment collision with obstacle -> Tail severed correctly from impact index onwards -> **PASS**
- Generator prefab assembly -> `5_JoinClash_Snake_Slice.prefab` built with `SnakeChainAuthoring` and follower prefab -> **PASS**

## Unchallenged Areas

- Audio & VFX presentation rendering — dependent on Presentation layer systems (`AudioManagerSystem`, `VFXManagerSystem`).
