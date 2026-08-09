# Handoff Report — Explorer 3 (Milestone 2: Snake Follower Chain & Collision Systems Architecture & Safety)

## 1. Observation
- **Project Structure & Target Files**:
  - `PROJECT.md` line 14: Milestone 2 scope: "Snake Follower chain movement, follower spawning, collision handling, authoring component, baker, and slice integration (`5_JoinClash_Snake`)".
  - `PROJECT.md` lines 25-27: Simulation ↔ Hybrid Presentation:
    - `PlaySoundEventComponent` (Entity tag with AudioType enum) -> `AudioManagerSystem` triggers `AudioSource.PlayOneShot()`.
    - `DestroyEventComponent` or `SpawnVFXEventComponent` -> `VFXManagerSystem` instantiates/plays `ParticleSystem`.
  - `docs/project-context.md` line 17: Critical-Lane Domains includes "DOTS ECS Systems (Simulation Group, SystemAPI Query, EntityCommandBuffer safety)" and "Collision & Damage calculations".
- **Existing Hybrid Presentation Systems**:
  - `Assets/Scripts/ECS/Components/PlaySoundEventComponent.cs` lines 12-15:
    ```csharp
    public struct PlaySoundEventComponent : IComponentData
    {
        public SoundType SoundToPlay;
    }
    ```
  - `Assets/Scripts/ECS/Components/DestroyEventComponent.cs` lines 7-8:
    ```csharp
    public struct DestroyEventComponent : IComponentData, IEnableableComponent
    {
    }
    ```
  - `Assets/Scripts/ECS/Systems/AudioManagerSystem.cs` lines 27-44:
    ```csharp
    foreach (var (soundEvent, entity) in SystemAPI.Query<RefRO<PlaySoundEventComponent>>().WithEntityAccess())
    {
        // Plays sound and consumes event entity via ecb.DestroyEntity(entity)
    }
    ```
  - `Assets/Scripts/ECS/Systems/VFXManagerSystem.cs` lines 22-32:
    ```csharp
    foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<DestroyEventComponent>().WithEntityAccess())
    {
        // Instantiates VFX prefab at LocalTransform.Position and destroys tag entity via ecb.DestroyEntity(entity)
    }
    ```
- **Existing Systems & Conventions**:
  - `Assets/Scripts/ECS/Systems/PlayerMovementSystem.cs` lines 8-10:
    ```csharp
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct PlayerMovementSystem : ISystem
    ```
  - Systems run as unmanaged `partial struct` implementing `ISystem`, compiled with `[BurstCompile]` in `SimulationSystemGroup`.

---

## 2. Logic Chain
1. **System Definition & Group Scheduling**:
   - `SnakeFollowerSystem` and `SnakeCollisionSystem` must be implemented as unmanaged `partial struct` implementing `ISystem` decorated with `[BurstCompile]` inside `namespace HyperCasualRunner.ECS.Systems`.
   - `SnakeFollowerSystem` must be grouped under `[UpdateInGroup(typeof(SimulationSystemGroup))]` and scheduled `[UpdateAfter(typeof(PlayerMovementSystem))]` to ensure the player's updated transform for the frame is recorded into history before calculating follower positions.
   - `SnakeCollisionSystem` must be grouped under `[UpdateInGroup(typeof(SimulationSystemGroup))]` and scheduled `[UpdateAfter(typeof(SnakeFollowerSystem))]` to resolve collisions with collected followers or obstacles after movement.

2. **Position History Recording Algorithm**:
   - The head entity contains `SnakeChainComponent` and a dynamic buffer `DynamicBuffer<SnakePositionHistoryElement>`.
   - Each frame in `SnakeFollowerSystem.OnUpdate()`, read the head's `LocalTransform`.
   - Check distance against the most recent history element (`math.distancesq(currentPos, lastRecordedPos) > minDistanceSq`, e.g. `0.0025f` for `0.05f` threshold).
   - If distance threshold is exceeded, prepend/insert at index 0 or append to the history buffer:
     ```csharp
     historyBuffer.Insert(0, new SnakePositionHistoryElement { Position = headPos, Rotation = headRot });
     ```
   - Trim history buffer when length exceeds maximum required length based on active segment count:
     ```csharp
     int maxRequiredHistory = chain.ValueRO.SegmentCount * maxSamplesPerSegment + 50;
     if (historyBuffer.Length > maxRequiredHistory)
     {
         historyBuffer.RemoveAt(historyBuffer.Length - 1);
     }
     ```

3. **Follower Lerp Spacing Calculation Algorithm**:
   - For each active segment entity recorded in `DynamicBuffer<SnakeSegmentElement>` (or queried via `SnakeFollowerComponent`):
   - Target distance along history curve for follower index $i$ ($1$-based index): $D_i = i \times \text{FollowerSpacing}$.
   - Iterate through history buffer elements, accumulating step distances: $\Delta d_k = \|\text{Pos}_k - \text{Pos}_{k-1}\|$.
   - Find history segment $[k-1, k]$ where accumulated distance spans $D_i$.
   - Compute fraction $t = \frac{D_i - d_{k-1}}{d_k - d_{k-1}}$.
   - Compute target transform:
     - `targetPos = math.lerp(history[k-1].Position, history[k].Position, t)`
     - `targetRot = math.slerp(history[k-1].Rotation, history[k].Rotation, t)`
   - Apply smoothed follower transform update:
     - `followerTransform.Position = math.lerp(followerTransform.Position, targetPos, dt * followSpeed)`
     - `followerTransform.Rotation = math.slerp(followerTransform.Rotation, targetRot, dt * followSpeed)`

4. **Segment Spawning on Collecting Followers**:
   - `SnakeCollisionSystem` iterates over uncollected `SnakeCollectibleComponent` entities within collision radius of Snake head or leading followers.
   - Upon collision:
     1. Instantiate follower prefab entity via ECB: `Entity follower = ecb.Instantiate(chain.ValueRO.FollowerPrefab);`.
     2. Add/set `SnakeFollowerComponent`: `ecb.SetComponent(follower, new SnakeFollowerComponent { HeadEntity = headEntity, SegmentIndex = newCount });`.
     3. Place follower initially at last segment position or head position: `ecb.SetComponent(follower, LocalTransform.FromPositionRotation(spawnPos, spawnRot));`.
     4. Append follower entity reference to `SnakeSegmentElement` buffer: `segmentBuffer.Add(new SnakeSegmentElement { Value = follower });`.
     5. Increment `chain.ValueRW.SegmentCount`.
     6. Destroy collectible entity: `ecb.DestroyEntity(collectibleEntity);`.
     7. Trigger Pickup Audio Tag:
        ```csharp
        Entity audioTag = ecb.CreateEntity();
        ecb.AddComponent(audioTag, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
        ```

5. **Segment Destruction on Hitting Obstacles**:
   - `SnakeCollisionSystem` iterates over `ObstacleComponent` entities colliding with Snake head or segments.
   - Upon collision:
     1. Determine number of segments to dismantle/destroy (e.g. 1 per hit or segment damage).
     2. Retrieve tail segment from `segmentBuffer`: `Entity tailEntity = segmentBuffer[segmentBuffer.Length - 1].Value;`.
     3. Read tail segment transform `tailTransform` for explosion VFX placement.
     4. Remove tail segment from buffer: `segmentBuffer.RemoveAt(segmentBuffer.Length - 1);`.
     5. Destroy tail segment entity: `ecb.DestroyEntity(tailEntity);`.
     6. Decrement `chain.ValueRW.SegmentCount`.
     7. Destroy obstacle entity: `ecb.DestroyEntity(obstacleEntity);`.
     8. Trigger Explosion VFX Tag Entity at tail position:
        ```csharp
        Entity vfxTag = ecb.CreateEntity();
        ecb.AddComponent(vfxTag, LocalTransform.FromPosition(tailTransform.Position));
        ecb.AddComponent<DestroyEventComponent>(vfxTag);
        ```
     9. Trigger Explosion Audio Tag:
        ```csharp
        Entity audioTag = ecb.CreateEntity();
        ecb.AddComponent(audioTag, new PlaySoundEventComponent { SoundToPlay = SoundType.Explosion });
        ```
     10. If `SegmentCount == 0` and head collides with obstacle, transition `LevelStateComponent.CurrentState` to `GameState.Defeat`.

6. **Burst & ECB Safety Guidelines**:
   - **Burst Compatibility**: Ensure all code paths in `ISystem` structs use native types (`NativeArray`, `DynamicBuffer`, `float3`, `quaternion`, `math`). Avoid managed allocations or non-Burst methods.
   - **ECB Management**: Use `var ecb = new EntityCommandBuffer(Allocator.Temp);` within `OnUpdate()`, execute structural changes via `ecb`, playback `ecb.Playback(state.EntityManager);` and call `ecb.Dispose();` before exiting `OnUpdate()`.
   - **Buffer References**: Do not hold raw pointers across ECB playback. Perform buffer updates after or before structural ECB playback.

---

## 3. Caveats
- **Fixed Frame Rate / Variable Delta Time**: If frame rate fluctuates dramatically, position sampling density may vary. Using distance-based history interpolation (rather than fixed frame counts) guarantees smooth geometric spacing regardless of FPS.
- **Segment Spawning Delay**: Newly instantiated entities via ECB are created after `ecb.Playback()`. Position transforms and buffer additions should be applied consistently within the ECB transaction or in the subsequent frame update.
- **VFX Presentation Systems**: `VFXManagerSystem` requires `VFXManagerComponent` singleton with a valid `HitVFXPrefab`. If missing in a slice, tag entities are safely cleaned up without crashing.

---

## 4. Conclusion
- The proposed architecture for `SnakeFollowerSystem` and `SnakeCollisionSystem` satisfies all pure DOTS ECS Entities 1.0+ requirements.
- Uses `[BurstCompile]` unmanaged `ISystem` structs in `SimulationSystemGroup`.
- Implements robust distance-based trajectory recording in `DynamicBuffer<SnakePositionHistoryElement>`.
- Provides smooth follower lerping along history curve.
- Fully integrates segment spawning, tail destruction, and Hybrid Audio/VFX tag triggering (`PlaySoundEventComponent` and `DestroyEventComponent`).

---

## 5. Verification Method
- **Compilation Check**: Verify clean compilation of systems once created by implementer agent.
- **Slice Verification Command**: Run Unity Editor menu `IdleToolkit -> Run Verification Suite` or invoke `ToolkitExampleGenerator.RunVerificationSuite()` to confirm slice setup.
- **System Query Inspection**: Verify `SnakeFollowerSystem` and `SnakeCollisionSystem` appear in Unity ECS Entity Debugger in `SimulationSystemGroup` between `PlayerMovementSystem` and `PresentationSystemGroup`.
- **Hybrid Tag Verification**: Inspect Entity Debugger during gameplay to observe transient `PlaySoundEventComponent` and `DestroyEventComponent` entities created in simulation and consumed by `AudioManagerSystem` and `VFXManagerSystem`.
