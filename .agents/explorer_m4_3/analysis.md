# Analysis Report: Meta-Progression Persistence & `GameProgressData.cs` Integration

**Author**: Explorer 3  
**Milestone**: Milestone 4 — Multi-Level Progression Loader & UI Integration  
**Date**: 2026-07-22  
**Target Folder**: `d:\Git\Hyper-Casual-Runner\.agents\explorer_m4_3`

---

## 1. Executive Summary

This report establishes the meta-progression persistence architecture and ECS data binding for Milestone 4 of the Hyper-Casual Runner project. The goal is to provide a clean, lightweight, and modular persistence pipeline that:
1. Reads saved progression state (Level Index, Unlocked Levels, Total Coins/Gold) from `PlayerPrefs` via `GameProgressData.cs` on game startup.
2. Synchronizes persistence state into pure DOTS ECS singletons (`MetaProgressionComponent` and `LevelSequenceComponent`).
3. Saves updated coin counts and unlocked level indices atomically upon level completion (`GameProgressData.Save()`).
4. Adheres strictly to project directives (`.agents/AGENTS.md`): maintaining modular persistence without interlocking unrelated game mechanics and using UI Toolkit for interface binding.

---

## 2. Inspection of `GameProgressData.cs` & Meta-Progression Mechanics

### 2.1 Current Implementation Overview
`Assets/Scripts/GameProgressData.cs` is a static helper class serving as a wrapper over Unity's `PlayerPrefs`.

#### Existing Keys & Properties:
- `GoldKey` (`"HCR_TotalGold"`, `int`, default `0`): Total accumulated coins/gold meta-currency.
- `SwarmLevelKey` (`"HCR_SwarmLevel"`, `int`, default `1`): Starting runner count upgrade level.
- `IncomeLevelKey` (`"HCR_IncomeLevel"`, `int`, default `1`): Run coin multiplier upgrade level.
- `CurrentSkinIndex` (`"HCR_SkinIndex"`, `int`, default `0`): Equipped player skin index.
- `UnlockedSkins` (`"HCR_UnlockedSkins"`, `int`, default `1`): Bitmask of unlocked skin indices.
- `LastLoginDate` (`"HCR_LastLoginDate"`, `string`, default `""`): Timestamp for daily reward check.
- `LoginStreak` (`"HCR_LoginStreak"`, `int`, default `0`): Daily login streak count.
- `CheckDailyReward()` / `ClaimDailyReward()`: Daily login retention reward mechanics.
- `GetUpgradeCost(int currentLevel)`: Linear upgrade cost formula ($50 \times \text{level}$).

### 2.2 Identified Gaps for Milestone 4
The existing `GameProgressData.cs` lacks level progression fields required for multi-level sequencing (21 playable slices):
1. **Current Level Index**: No key for tracking the active/selected level slice (0-based index `0..20`).
2. **Unlocked Level Index**: No key for tracking the maximum unlocked level reached by the player.
3. **Per-Level Stars / High Scores**: No structured storage for level completion quality (1-3 stars, max coins earned per level).
4. **Explicit `Save()` API**: Current property setters call `PlayerPrefs.Save()` individually, which is inefficient and non-atomic during multi-field updates upon level victory.

---

## 3. PlayerPrefs Key Matrix & Persistence Contracts

### 3.1 Standardized PlayerPrefs Key Specification

| Parameter | PlayerPrefs Key | Type | Default Value | Description |
|---|---|---|---|---|
| Total Coins / Gold | `"HCR_TotalGold"` | `int` | `0` | Accumulated meta-currency across all runs |
| Current Level Index | `"HCR_CurrentLevelIndex"` | `int` | `0` | 0-based index of the currently selected/active level slice (0..20) |
| Unlocked Level Index | `"HCR_UnlockedLevelIndex"` | `int` | `0` | 0-based index of the highest level unlocked (0..20) |
| Level Stars | `"HCR_LevelStars_{index}"` | `int` | `0` | Stars earned on slice `{index}` (0 to 3) |
| Level High Score | `"HCR_LevelHighScore_{index}"` | `int` | `0` | Highest coin count achieved on slice `{index}` |
| Swarm Level | `"HCR_SwarmLevel"` | `int` | `1` | Meta-upgrade level for runner swarm count |
| Income Level | `"HCR_IncomeLevel"` | `int` | `1` | Meta-upgrade multiplier for run income |
| Equipped Skin Index | `"HCR_SkinIndex"` | `int` | `0` | Active visual skin index |
| Unlocked Skins Bitmask | `"HCR_UnlockedSkins"` | `int` | `1` | Bitfield representing unlocked skin indices |
| Last Login Date | `"HCR_LastLoginDate"` | `string` | `""` | ISO timestamp of last daily login check |
| Daily Login Streak | `"HCR_LoginStreak"` | `int` | `0` | Consecutive days logged in |

---

## 4. ECS Data Binding & Persistence Bridge Architecture

Because pure DOTS jobified systems running on worker threads cannot safely access Unity's `PlayerPrefs` directly, a **Bridge Architecture** is required to connect static C# `GameProgressData` to pure DOTS ECS components.

```
[ PlayerPrefs Storage ]
          ▲
          │ PlayerPrefs.SetInt() / PlayerPrefs.Save()
          ▼
[ GameProgressData.cs (Static C# Wrapper) ]
     ▲                             ▲
     │ (Startup Read)              │ (Victory Save)
     ▼                             │
[ MetaProgressionBootstrapSystem ] [ MetaProgressionSaveSystem ]
(InitializationSystemGroup)        (PresentationSystemGroup)
     │                             ▲
     ▼                             │ Spawns / Enables
[ MetaProgressionComponent ] ──────┼───────────────────────┐
(Singleton IComponentData)         │                       │
                                   │                       ▼
                      [ SaveProgressEventComponent ] ◄── [ VictorySystem / LevelProgressionSystem ]
```

### 4.1 ECS Component Layout Specifications

#### `MetaProgressionComponent` (Singleton `IComponentData`)
Stores runtime meta-progression state in the ECS World for fast job access.

```csharp
using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public struct MetaProgressionComponent : IComponentData
    {
        public int TotalGold;             // Offset 0 (4 bytes)
        public int CurrentLevelIndex;     // Offset 4 (4 bytes)
        public int UnlockedLevelIndex;    // Offset 8 (4 bytes)
        public int SwarmLevel;            // Offset 12 (4 bytes)
        public float IncomeMultiplier;    // Offset 16 (4 bytes)
    }
}
```

#### `SaveProgressEventComponent` (Event `IComponentData`, `IEnableableComponent`)
Spawns or enables on an event entity upon level completion to request a persistence write.

```csharp
using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public struct SaveProgressEventComponent : IComponentData, IEnableableComponent
    {
        public int CompletedLevelIndex;   // Offset 0 (4 bytes)
        public int CoinsEarnedInRun;      // Offset 4 (4 bytes)
        public int StarsEarned;           // Offset 8 (4 bytes)
        public bool IsVictory;            // Offset 12 (1 byte)
        public byte Padding0;             // Offset 13 (1 byte)
        public ushort Padding1;           // Offset 14 (2 bytes)
    }
}
```

---

## 5. Persistence System Specifications & System Lifecycle

### 5.1 Game Startup Initialization Flow (`MetaProgressionBootstrapSystem`)
- **System Group**: `InitializationSystemGroup`
- **Execution**: Runs once on startup or scene load.
- **Responsibilities**:
  1. Reads initial data from `GameProgressData`:
     - `GameProgressData.TotalGold`
     - `GameProgressData.CurrentLevelIndex`
     - `GameProgressData.UnlockedLevelIndex`
     - `GameProgressData.SwarmLevel`
     - `GameProgressData.IncomeLevel`
  2. Creates or updates the `MetaProgressionComponent` singleton entity in the ECS World.
  3. Initializes `LevelSequenceComponent` (managed by Explorer 1's `LevelProgressionSystem`) with `CurrentLevelIndex` and `UnlockedLevelIndex`.

### 5.2 Level Completion Save Flow (`MetaProgressionSaveSystem`)
- **System Group**: `PresentationSystemGroup` (Main Thread)
- **Execution**: Evaluated every frame, active when a `SaveProgressEventComponent` entity is present and enabled.
- **Responsibilities**:
  1. Queries entities with `SaveProgressEventComponent`.
  2. For each enabled event:
     - If `IsVictory == true`:
       - Calculates total coins earned: `earnedCoins = eventData.CoinsEarnedInRun * GameProgressData.IncomeLevel`.
       - Updates total gold: `GameProgressData.TotalGold += earnedCoins`.
       - Checks level unlocking: `if (eventData.CompletedLevelIndex + 1 > GameProgressData.UnlockedLevelIndex) GameProgressData.UnlockedLevelIndex = eventData.CompletedLevelIndex + 1;`
       - Updates level stars: `GameProgressData.SetLevelStars(eventData.CompletedLevelIndex, eventData.StarsEarned)`.
       - Calls explicit `GameProgressData.Save()` to write PlayerPrefs to disk atomically.
     - Synchronizes updated values back to ECS `MetaProgressionComponent` singleton.
     - Disables or consumes `SaveProgressEventComponent`.

---

## 6. Event Lifecycle & Flow Diagrams

### 6.1 Startup Reading Flow
```
1. Unity Engine Startup
   └── Default World Created
       └── MetaProgressionBootstrapSystem.OnUpdate()
           ├── Reads GameProgressData.TotalGold
           ├── Reads GameProgressData.CurrentLevelIndex
           ├── Reads GameProgressData.UnlockedLevelIndex
           └── Creates / Sets Singleton Entity:
               ├── MetaProgressionComponent { TotalGold, CurrentLevelIndex, UnlockedLevelIndex }
               └── LevelSequenceComponent { CurrentLevelIndex, UnlockedLevelIndex }
```

### 6.2 Level Completion Save Flow
```
1. Player crosses End Zone -> LevelStateComponent.CurrentState = GameState.Victory
2. LevelProgressionSystem / VictorySystem calculates stats:
   └── Spawns Entity with SaveProgressEventComponent { CompletedLevelIndex = N, CoinsEarnedInRun = X, StarsEarned = 3, IsVictory = true }
3. MetaProgressionSaveSystem (PresentationSystemGroup) detects event:
   ├── GameProgressData.TotalGold += X
   ├── GameProgressData.UnlockedLevelIndex = math.max(UnlockedLevelIndex, N + 1)
   ├── GameProgressData.SetLevelStars(N, 3)
   ├── GameProgressData.Save()  <-- Atomic PlayerPrefs.Save() call
   └── MetaProgressionComponent singleton updated in ECS World
4. UIManagerSystem updates UI Toolkit VisualElements:
   ├── HUD Gold Label updated to new TotalGold
   └── Level Select Grid unlocks button for Level N + 1
```

---

## 7. Modular Persistence Contracts & Project Rules Compliance

### 7.1 Compliance with Project Rules (`.agents/AGENTS.md`)
1. **Rule 3 — Meta-Progression Modularity**:
   - `GameProgressData.cs` operates strictly as a lightweight `PlayerPrefs` wrapper.
   - It contains zero references to DOTS types (`Entity`, `EntityManager`, `IComponentData`), rendering pipelines, or UI classes.
   - Separate mechanics (Swarm upgrade, Income upgrade, Skin shop, Daily rewards, Level progress) maintain isolated properties and key namespaces (`"HCR_*"`).
2. **Rule 1 — UI Architecture Constraint**:
   - All visual representations of persisted data (coins, level select buttons, stars) are rendered via Unity UI Toolkit (`UIDocument`, `VisualElement`, `Label`, `Button`). No legacy uGUI or Canvas code is introduced.
3. **Rule 2 — Hybrid ECS Architecture**:
   - Pure simulation systems handle gameplay logic; Presentation layer systems (`MetaProgressionSaveSystem`) bridge DOTS event components to classic C# `PlayerPrefs`.

---

## 8. Implementation Blueprints & Code Snippets

### 8.1 Proposed `GameProgressData.cs` Extension

```csharp
using UnityEngine;

namespace HyperCasualRunner
{
    public static class GameProgressData
    {
        private const string GoldKey = "HCR_TotalGold";
        private const string SwarmLevelKey = "HCR_SwarmLevel";
        private const string IncomeLevelKey = "HCR_IncomeLevel";
        private const string CurrentLevelIndexKey = "HCR_CurrentLevelIndex";
        private const string UnlockedLevelIndexKey = "HCR_UnlockedLevelIndex";

        public static int TotalGold
        {
            get => PlayerPrefs.GetInt(GoldKey, 0);
            set { PlayerPrefs.SetInt(GoldKey, value); PlayerPrefs.Save(); }
        }

        public static int CurrentLevelIndex
        {
            get => PlayerPrefs.GetInt(CurrentLevelIndexKey, 0);
            set { PlayerPrefs.SetInt(CurrentLevelIndexKey, value); PlayerPrefs.Save(); }
        }

        public static int UnlockedLevelIndex
        {
            get => PlayerPrefs.GetInt(UnlockedLevelIndexKey, 0);
            set { PlayerPrefs.SetInt(UnlockedLevelIndexKey, value); PlayerPrefs.Save(); }
        }

        public static int SwarmLevel
        {
            get => PlayerPrefs.GetInt(SwarmLevelKey, 1);
            set { PlayerPrefs.SetInt(SwarmLevelKey, value); PlayerPrefs.Save(); }
        }

        public static int IncomeLevel
        {
            get => PlayerPrefs.GetInt(IncomeLevelKey, 1);
            set { PlayerPrefs.SetInt(IncomeLevelKey, value); PlayerPrefs.Save(); }
        }

        public static int CurrentSkinIndex
        {
            get => PlayerPrefs.GetInt("HCR_SkinIndex", 0);
            set { PlayerPrefs.SetInt("HCR_SkinIndex", value); PlayerPrefs.Save(); }
        }

        public static int UnlockedSkins
        {
            get => PlayerPrefs.GetInt("HCR_UnlockedSkins", 1);
            set { PlayerPrefs.SetInt("HCR_UnlockedSkins", value); PlayerPrefs.Save(); }
        }

        public static int GetLevelStars(int levelIndex)
        {
            return PlayerPrefs.GetInt($"HCR_LevelStars_{levelIndex}", 0);
        }

        public static void SetLevelStars(int levelIndex, int stars)
        {
            int currentStars = GetLevelStars(levelIndex);
            if (stars > currentStars)
            {
                PlayerPrefs.SetInt($"HCR_LevelStars_{levelIndex}", Mathf.Clamp(stars, 0, 3));
            }
        }

        public static void SaveLevelCompletion(int completedLevelIndex, int earnedCoins, int stars = 0)
        {
            TotalGold += earnedCoins;
            if (completedLevelIndex + 1 > UnlockedLevelIndex)
            {
                UnlockedLevelIndex = completedLevelIndex + 1;
            }
            SetLevelStars(completedLevelIndex, stars);
            PlayerPrefs.Save();
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }

        public static void ResetProgress()
        {
            PlayerPrefs.DeleteKey(GoldKey);
            PlayerPrefs.DeleteKey(CurrentLevelIndexKey);
            PlayerPrefs.DeleteKey(UnlockedLevelIndexKey);
            PlayerPrefs.DeleteKey(SwarmLevelKey);
            PlayerPrefs.DeleteKey(IncomeLevelKey);
            PlayerPrefs.DeleteKey("HCR_SkinIndex");
            PlayerPrefs.DeleteKey("HCR_UnlockedSkins");
            PlayerPrefs.Save();
        }
    }
}
```

---

## 9. Verification & Testing Strategy

### 9.1 Machine-Checkable Verification Criteria
1. **Key Preservation**: All PlayerPrefs keys must follow the `"HCR_*"` prefix convention.
2. **Startup Consistency**: Modifying `PlayerPrefs` before ECS world initialization must result in matching values in `MetaProgressionComponent` singleton.
3. **Atomic Level Save Verification**: Triggering `SaveProgressEventComponent` on level completion must increment `TotalGold` by run coins, advance `UnlockedLevelIndex` if `CompletedLevelIndex == UnlockedLevelIndex`, and persist state to disk via `GameProgressData.Save()`.
4. **Invalidation / Edge Conditions**:
   - `CompletedLevelIndex` out of range (e.g. negative or $> 20$): Clamped to valid level sequence bounds $0..20$.
   - Pre-existing higher unlocked level: `UnlockedLevelIndex` must not decrease when replaying earlier levels.

---

## 10. Conclusion & Handoff Summary
This analysis details the complete meta-progression persistence architecture and ECS binding contract for Milestone 4. By establishing a clear separation between `GameProgressData.cs` (storage), `MetaProgressionComponent` / `SaveProgressEventComponent` (ECS bridge), and `UIManagerSystem` (UI Toolkit presentation), the system ensures high performance, data safety, and full compliance with project rules.
