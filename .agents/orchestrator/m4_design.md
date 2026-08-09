# Milestone 4 Technical Design Specification
## Multi-Level Progression Loader & UI Integration

### 1. Executive Overview
Milestone 4 delivers a robust, pure DOTS multi-level progression loader, clean slice entity teardown mechanics, PlayerPrefs meta-progression persistence, and a 100% Unity UI Toolkit interface layer.

---

### 2. Architecture & Data Layout

#### 2.1 Component Data & Enums (`Assets/Scripts/ECS/Components/LevelProgressionComponents.cs`)
```csharp
using Unity.Entities;

namespace HyperCasualRunner.ECS.Components
{
    public enum LevelTransitionState
    {
        Idle,            // Gameplay active or waiting in Pregame
        PendingNext,     // Victory reached, preparing transition
        TeardownCurrent, // Destroying current slice entities
        SpawningNext,    // Instantiating next slice prefab entity
        Failed           // Defeat state, awaiting retry/restart input
    }

    [InternalBufferCapacity(21)]
    public struct SlicePrefabBufferElement : IBufferElementData
    {
        public Entity PrefabEntity;
    }

    public struct LevelSequenceComponent : IComponentData
    {
        public int CurrentLevelIndex;
        public int MaxLevels;
        public int UnlockedLevelIndex;
        public LevelTransitionState TransitionState;
        public Entity CurrentSliceInstance;
        public bool LoopSequencing;
        public float AutoTransitionTimer;
    }

    public struct SliceEntityTag : IComponentData {}

    public struct MetaProgressionComponent : IComponentData
    {
        public int TotalGold;
        public int CurrentLevelIndex;
        public int UnlockedLevelIndex;
        public int SwarmLevel;
        public float IncomeMultiplier;
    }

    public struct SaveProgressEventComponent : IComponentData, IEnableableComponent
    {
        public int CompletedLevelIndex;
        public int CoinsEarnedInRun;
        public int StarsEarned;
        public bool IsVictory;
    }
}
```

#### 2.2 Level Sequence Authoring (`Assets/Scripts/ECS/Authoring/LevelSequenceAuthoring.cs`)
- Attached to `LevelManager` in the main scene.
- Accepts `GameObject[] SlicePrefabs` (21 slice prefabs).
- Baker creates `LevelSequenceComponent` and populates `SlicePrefabBufferElement` dynamic buffer with baked DOTS entity prefab handles via `GetEntity(slicePrefab, TransformUsageFlags.Dynamic)`.

---

### 3. ECS Systems Architecture

#### 3.1 `LevelProgressionSystem.cs` (`Assets/Scripts/ECS/Systems/LevelProgressionSystem.cs`)
- Group: `SimulationSystemGroup`
- Handles state machine updates (`LevelTransitionState`):
  - **`Idle`**: Listens to `LevelStateComponent.CurrentState`. Transitions to `PendingNext` on `Victory` or `Failed` on `Defeat`.
  - **`PendingNext`**: Advances `CurrentLevelIndex = (CurrentLevelIndex + 1) % MaxLevels`, updates `GameProgressData.CurrentLevelIndex`, transitions to `TeardownCurrent`.
  - **`TeardownCurrent`**: Uses `EndSimulationEntityCommandBufferSystem` ECB to destroy `CurrentSliceInstance` (triggering recursive `LinkedEntityGroup` cleanup) and destroys all loose runtime entities matching `SliceEntityTag` query.
  - **`SpawningNext`**: Instantiates entity prefab from `SlicePrefabBufferElement[CurrentLevelIndex]`, sets `CurrentSliceInstance`, resets `LevelStateComponent.CurrentState = GameState.Pregame`, transitions to `Idle`.

#### 3.2 `MetaProgressionSaveSystem.cs` (`Assets/Scripts/ECS/Systems/MetaProgressionSaveSystem.cs`)
- Group: `PresentationSystemGroup` (Main Thread)
- Processes enabled `SaveProgressEventComponent` entities:
  - Updates `GameProgressData.TotalGold += CoinsEarnedInRun * IncomeLevel`.
  - Unlocks next level: `if (CompletedLevelIndex + 1 > GameProgressData.UnlockedLevelIndex) GameProgressData.UnlockedLevelIndex = CompletedLevelIndex + 1`.
  - Records star rating: `GameProgressData.SetLevelStars(CompletedLevelIndex, StarsEarned)`.
  - Calls atomic `GameProgressData.Save()`.

---

### 4. Meta-Progression Persistence (`Assets/Scripts/GameProgressData.cs`)

Extends `GameProgressData.cs` with:
- `CurrentLevelIndex` (`"HCR_CurrentLevelIndex"`)
- `UnlockedLevelIndex` (`"HCR_UnlockedLevelIndex"`)
- `GetLevelStars(int levelIndex)` & `SetLevelStars(int levelIndex, int stars)` (`"HCR_LevelStars_{index}"`)
- `SaveLevelCompletion(int completedLevelIndex, int earnedCoins, int stars)`
- Explicit `Save()` method wrapping `PlayerPrefs.Save()`.

---

### 5. UI Toolkit Integration (`Assets/Scripts/UI/` & `Assets/UI/`)

#### 5.1 Rules & Constraints
- **Strict UI Toolkit**: 100% `UnityEngine.UIElements` (UXML/USS). Zero legacy `Canvas` / `UnityEngine.UI` usage.

#### 5.2 UI Controller & System (`UIManagerSystem.cs`)
- Bridges ECS simulation state (`LevelStateComponent`, `PlayerCoinRunnerComponent`, player Z position, endzone Z position) to UI Toolkit DOM.
- Controls 4 screens:
  1. **Game HUD** (`GameHUD.uxml`, `GameHUD.uss`): Coin badge, Level progress bar (`(PlayerZ / EndZoneZ) * 100%`), settings modal overlay, pregame upgrade panel.
  2. **Level Select** (`LevelSelect.uxml`, `LevelSelect.uss`, `LevelCardItem.uxml`): 3-column scrollable grid of 21 level buttons displaying unlocked state, play status, and 0-3 gold stars.
  3. **Victory Screen** (`VictoryScreen.uxml`, `VictoryScreen.uss`): Level completed banner, 1-3 animated stars, coins breakdown, multiplier bonus wheel ticker (1.5x - 5.0x ping-pong), "NEXT LEVEL" button.
  4. **Defeat Screen** (`DefeatScreen.uxml`, `DefeatScreen.uss`): Level failed banner, percentage completed label, revive button, retry button, level select button.

---

### 6. Acceptance & Verification Criteria
1. **Compilation & Assembly**: All new components, authoring scripts, systems, and UI controllers compile with zero warnings or errors.
2. **UI Toolkit Strictness**: No legacy uGUI types used.
3. **Clean Teardown**: Transitioning across levels destroys all slice runtime entities without leaks.
4. **Persistence Integrity**: `PlayerPrefs` accurately persists level progress, unlocked levels, gold, and stars across game sessions.
