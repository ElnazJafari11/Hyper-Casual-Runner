# Empirical Verification and Stress Test Handoff Report

**Agent**: Challenger Agent 1 (`teamwork_preview_challenger_1`)  
**Project**: Hyper-Casual Runner Toolkit  
**Date**: 2026-07-22  

---

## 1. Observation

### Command Executed
```powershell
"C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe" -batchmode -nographics -silent-crashes -projectPath "d:\Git\Hyper-Casual-Runner" -runTests -testPlatform EditMode -testResults "d:\Git\Hyper-Casual-Runner\.agents\teamwork_preview_challenger_1\editmode_results.xml" -logFile "d:\Git\Hyper-Casual-Runner\.agents\teamwork_preview_challenger_1\editmode_run.log"
```

### Test Suite Execution Summary (`editmode_results.xml`)
- **Total Test Cases**: 47
- **Passed**: 45
- **Failed**: 2
- **Test Assemblies**: `Assembly-CSharp-Editor.dll`

### Test Case Results Breakdown

#### M1: Level Select Card Rendering, Locked/Unlocked State, Index Bounds
- `LevelSelectScreen_InitializationAndFallback_CreatesValidRootAndGrid` -> **PASS**
- `LevelSelectScreen_GridPopulation_BindsUnlockedAndLockedCardsCorrectly` -> **PASS**
- `LevelSelectScreen_LevelClick_UpdatesCurrentLevelIndexAndTriggersTransition` -> **PASS**
- `LevelSelectScreen_UxmlAssetBinding_InstantiatesUxmlTemplate` -> **PASS**
- `M1_LevelSelect_CardRendering_ZeroOrNegativeOverrideCount_FallsBackToDefaultLevels` -> **PASS**
- `M1_LevelSelect_ExtremeIndexBounds_DoesNotThrowOrCorruptGrid` -> **PASS**
- `M1_LevelSelect_SelectLevel_OutOfBoundsIndices_UpdatesGameProgressWithoutCrash` -> **PASS**
- `M1_LevelSelect_LockedCard_DoesNotTriggerLevelSelection` -> **PASS**

#### M2: Moving Wall Sine Wave, Pendulum Swing Quaternions, Splitting Hazard Distance
- `ObstacleComponents_StructLayoutsAndDefaults_AreValid` -> **PASS**
- `MovingWallSystem_TranslatesPositionViaSineWave` -> **PASS**
- `PendulumSwingSystem_RotatesAroundSwingAxis` -> **PASS**
- `SplittingHazardSystem_TriggersSplit_WhenPlayerInDistance` -> **PASS**
- `SplittingHazardSystem_DoesNotTrigger_WhenPlayerOutsideDistance` -> **PASS**
- `M2_MovingWall_SineWave_LargeTimeAndSpeedVariations_MaintainsFiniteCoordinates` -> **PASS** (`time = 100000.0f`, `speed = 50.0f`, position bounded within `[-10, 10]` without float overflow or NaN)
- `M2_PendulumSwing_Quaternion_ZeroAxisFallbackAndLargeAngles_ProducesNormalizedQuaternions` -> **PASS** (Zero swing axis falls back to `(0, 0, 1)`, quaternion magnitude `= 1.000 ± 0.001`)
- `M2_SplittingHazard_DistanceLogic_ExactBoundaryAndNegativeDistanceHandling` -> **PASS** (Player at `distSq == trigDistSq` triggers split and spawns audio/child entities correctly)

#### M3: Prestige Currency Transactions, Zero/Insufficient Funds, Skin Bitmask Operations
- `CosmeticPurchaseEventComponent_LayoutAndSize_MatchesSpecifications` -> **PASS**
- `CosmeticsShopSystem_AlreadyUnlockedSkin_EquipsWithoutDeductingCurrency` -> **PASS**
- `CosmeticsShopSystem_InsufficientPrestige_FailsPurchaseAndPreservesCurrency` -> **PASS**
- `GameProgressData_SkinBitmaskUnlocking_WorksCorrectly` -> **PASS**
- `M3_PrestigeTransactions_ExactZeroFunds_FailsPurchaseWithoutNegativeBalance` -> **PASS**
- `M3_SkinBitmaskOperations_Bit30AndBit31_HandlesSignedIntBitShifts` -> **PASS** (Bits 30 and 31 operate correctly in `int` bitmask)
- `CosmeticsShopSystem_SufficientPrestige_DeductsPrestigeCurrencyAndUnlocksSkin` -> **FAIL**
- `M3_PrestigeTransactions_ExactFundsMatch_DeductsToZeroAndUnlocksSkin` -> **FAIL**

### Verbatim Failure Stack Trace (`CosmeticsShopSystem.cs`)
```text
Unhandled log message: '[Exception] InvalidOperationException: Structural changes are not allowed while iterating over entities. Please use EntityCommandBuffer instead.'. Use UnityEngine.TestTools.LogAssert.Expect
  at Unity.Entities.EntityDataAccess.CheckIsStructuralChange () [0x00000] in ./Library/PackageCache/com.unity.entities@1eebe0437e56/Unity.Entities/EntityDataAccess.cs:338
  at Unity.Entities.EntityDataAccess.BeforeStructuralChange () [0x00000] in ./Library/PackageCache/com.unity.entities@1eebe0437e56/Unity.Entities/EntityDataAccess.cs:410
  at Unity.Entities.EntityDataAccess.BeginStructuralChanges () [0x00000] in ./Library/PackageCache/com.unity.entities@1eebe0437e56/Unity.Entities/EntityDataAccess.cs:420
  at Unity.Entities.EntityManager.AddComponent (Unity.Entities.Entity entity, Unity.Entities.ComponentType componentType) [0x00000] in ./Library/PackageCache/com.unity.entities@1eebe0437e56/Unity.Entities/EntityManager.cs:1569
  at Unity.Entities.EntityManager.AddComponentData[T] (Unity.Entities.Entity entity, T componentData) [0x00000] in ./Library/PackageCache/com.unity.entities@1eebe0437e56/Unity.Entities/EntityManager.cs:2450
  at HyperCasualRunner.ECS.Systems.CosmeticsShopSystem.OnUpdate (Unity.Entities.SystemState& state) [0x00000] in Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs:38
```

---

## 2. Logic Chain

1. **Observation 1**: Direct inspection of `Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs` lines 22–40 reveals:
   ```csharp
   foreach (var stats in SystemAPI.Query<RefRW<PersistentPlayerStats>>())
   {
       ...
       var soundEntity = state.EntityManager.CreateEntity();
       state.EntityManager.AddComponentData(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
   }
   ```
2. **Observation 2**: Running Unity Edit-Mode tests `CosmeticsShopSystem_SufficientPrestige_DeductsPrestigeCurrencyAndUnlocksSkin` and `M3_PrestigeTransactions_ExactFundsMatch_DeductsToZeroAndUnlocksSkin` triggers this code block upon a successful cosmetic purchase.
3. **Logic Step**: Calling `state.EntityManager.CreateEntity()` and `state.EntityManager.AddComponentData()` mutates entity archetype tables directly while an active `SystemAPI.Query` enumerator is evaluating `PersistentPlayerStats`. In Unity DOTS, direct `EntityManager` structural mutations during query iteration violate entity safety and throw `InvalidOperationException: Structural changes are not allowed while iterating over entities`.
4. **Observation 3**: In contrast, failed transactions (e.g. `CosmeticsShopSystem_InsufficientPrestige_FailsPurchaseAndPreservesCurrency`) and re-equipping already unlocked skins (`CosmeticsShopSystem_AlreadyUnlockedSkin_EquipsWithoutDeductingCurrency`) do not execute lines 37–38, allowing those tests to pass.
5. **Logic Step**: Therefore, the core transaction logic (currency subtraction, skin bitmask bitwise OR, equipped skin index update) is mathematically sound, but any valid purchase attempt immediately crashes the DOTS simulation due to immediate `EntityManager` structural modification inside the query loop.

---

## 3. Caveats

- **Scope**: Verification was performed in Unity Edit-Mode (batchmode execution). Runtime Standalone Mono/IL2CPP build targets were not compiled in this test pass.
- **Assumptions**: `EntityCommandBuffer` is the required pattern for spawning audio/VFX event entities in presentation systems per project directives (`d:\Git\Hyper-Casual-Runner\.agents\AGENTS.md` Rule 2 for Hybrid ECS Architecture).

---

## 4. Conclusion

- **M1 (Level Select UI)**: **VERIFIED & PASSING**. Level card instantiation, locked/unlocked visual state binding, and extreme index bounds (negative/overflow indices) operate without errors or memory corruption.
- **M2 (Obstacles & Hazards)**: **VERIFIED & PASSING**. Kinematic moving wall sine wave calculations under high time values ($t = 10^5\text{s}$), pendulum swing quaternion normalization under extreme rotation angles ($360^\circ$) with zero-axis fallback, and splitting hazard distance trigger math pass all stress tests.
- **M3 (Meta-Progression & Cosmetics)**: **FAILED (1 Structural Change Bug)**.
  - Skin bitmask bitwise logic (`IsSkinUnlocked`, `UnlockSkin` for bits 0–31) and currency balance checks pass.
  - **CRITICAL BUG IDENTIFIED**: `CosmeticsShopSystem.cs:37-38` performs direct `EntityManager.CreateEntity()` / `AddComponentData()` structural changes inside an active `SystemAPI.Query` foreach loop, causing `InvalidOperationException`.
- **Requirement 4 (Edit-Mode Suite Execution)**: Completed. 45 out of 47 tests passed (2 failed due to the `CosmeticsShopSystem` structural change bug).

### Recommended Fix for Implementer
In `Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs`:
Replace direct `EntityManager` entity creation with `EntityCommandBuffer`:
```csharp
var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
// Inside query loop:
Entity soundEntity = ecb.CreateEntity();
ecb.AddComponent(soundEntity, new PlaySoundEventComponent { SoundToPlay = SoundType.Pickup });
// After loop:
ecb.Playback(state.EntityManager);
ecb.Dispose();
```

---

## 5. Verification Method

To independently verify these findings:

1. Run the Unity CLI Edit-Mode test command from PowerShell:
   ```powershell
   & "C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe" -batchmode -nographics -silent-crashes -projectPath "d:\Git\Hyper-Casual-Runner" -runTests -testPlatform EditMode -testResults "d:\Git\Hyper-Casual-Runner\.agents\teamwork_preview_challenger_1\editmode_results.xml" -logFile "d:\Git\Hyper-Casual-Runner\.agents\teamwork_preview_challenger_1\editmode_run.log"
   ```
2. Inspect `editmode_results.xml` at line 116 and line 248 for the `InvalidOperationException` in `CosmeticsShopSystem.cs:38`.
3. Inspect source file `Assets/Scripts/ECS/Systems/Idle/CosmeticsShopSystem.cs` lines 37-38 to confirm direct `state.EntityManager` call inside query loop.
