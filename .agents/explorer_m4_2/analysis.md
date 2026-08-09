# M4 UI Architecture Analysis: UI Toolkit Integration & DOTS Entity Event Binding

## 1. Executive Summary

This report establishes the complete architecture and technical specification for Unity UI Toolkit (UI Elements) screens and DOTS entity bindings for **Milestone 4: Multi-Level Progression Loader & UI Integration**.

### Core Constraints & Principles
1. **Mandatory UI Toolkit Rule**: 100% of the game UI must be built using Unity UI Toolkit (`UnityEngine.UIElements`, `.uxml` documents, `.uss` stylesheets, `UIDocument`). **Legacy uGUI (`Canvas`, `UnityEngine.UI.Image`, `Text`) is strictly forbidden.**
2. **Hybrid DOTS/UI Presentation Layer**: `UIManagerSystem.cs` bridges the ECS simulation domain (`LevelStateComponent`, `PlayerCoinRunnerComponent`, `PlayerComponent`, `LocalTransform`) to the managed UI Toolkit DOM (`VisualElement`, `Label`, `ProgressBar`, `Button`).
3. **Modular Screen Controllers**: Separate visual document ownership into modular, decoupled controllers (`HUDController`, `LevelSelectController`, `VictoryController`, `DefeatController`) bound by a master `UIManagerSystem`.

---

## 2. Current Codebase Audit

### Existing Artifacts
- **`Assets/UI/GameHUD.uxml` & `GameHUD.uss`**: Basic initial UXML/USS containing placeholders for `GoldLabel`, `DistanceLabel`, `UpgradeDamageButton`, `UpgradeGoldButton`, and a hidden `PrestigeOverlay`.
- **`Assets/Scripts/UI/UIManager.cs`**: Primitive MonoBehaviour reading `CurrentRunStats` via direct `EntityManager` queries.
- **`Assets/Scripts/UI/UIManagerSystem.cs`**: Prototype MonoBehaviour creating UI elements programmatically (e.g. `_pregamePanel`, `_defeatPanel`, `_victoryPanel`) in C# rather than leveraging declarative UXML/USS documents.
- **`Assets/Scripts/UI/ToolkitHubManager.cs`**: Programmatic Hub UI providing level spawning and a skin shop.
- **`Assets/Scripts/GameProgressData.cs`**: `PlayerPrefs` wrapper storing `TotalGold`, `SwarmLevel`, `IncomeLevel`, `CurrentSkinIndex`, `UnlockedSkins`, `LastLoginDate`, and `LoginStreak`.
- **`Assets/Scripts/ECS/Components/LevelStateComponent.cs`**: ECS component storing `GameState CurrentState` (`Pregame`, `Playing`, `Victory`, `Defeat`).
- **`Assets/Scripts/ECS/Components/CoinMultiplierComponents.cs`**: `PlayerCoinRunnerComponent` storing `int CurrentCoinCount`.

### Architectural Gaps & Transition Required
1. **Programmatic UI vs UXML/USS**: Existing `UIManagerSystem.cs` builds UI elements dynamically in C# inline styles. M4 requires migrating all screens to clean UXML templates and USS stylesheets for styling and animation flexibility.
2. **Level Progression State Missing in Persistence**: `GameProgressData` lacks `HighestUnlockedLevel` and per-level `StarsEarned` properties (needed for the 1–21 Level Select grid).
3. **Progress Bar Data Source**: The level progress bar requires calculating the player's Z-axis position relative to the level start (Z = 0) and the `EndZoneComponent` position (Z = EndZone.Z).
4. **Multiplier Wheel & Victory Mechanics**: M4 Victory Screen requires an animated multiplier bonus wheel ticker and stars earned (1–3 stars based on coin retention/survival).

---

## 3. UI Toolkit Screen Architecture & Layout Specifications

We define 4 primary UI screens, modularized into reusable UXML documents and styled via `GameHUD.uss` and screen-specific USS files.

```
Assets/UI/
├── Documents/
│   ├── GameHUD.uxml
│   ├── LevelSelect.uxml
│   ├── VictoryScreen.uxml
│   ├── DefeatScreen.uxml
│   └── Components/
│       └── LevelCardItem.uxml
└── Styles/
    ├── Common.uss
    ├── GameHUD.uss
    ├── LevelSelect.uss
    ├── VictoryScreen.uss
    └── DefeatScreen.uss
```

---

### 3.1 Screen 1: Game HUD (In-Run & Pregame)

#### Functional Requirements
- **Level Progress Bar**: Centered at the top. Displays current level number (e.g. "LEVEL 4"), start icon, end icon, and fill percentage `(PlayerZ / EndZoneZ) * 100%`.
- **Coin Count Badge**: Top-left corner. Displays golden coin icon and live coin count (`PlayerCoinRunnerComponent.CurrentCoinCount + GameProgressData.TotalGold`).
- **Settings Button**: Top-right corner. Toggles a modal overlay with sound FX toggle, haptics toggle, and return-to-hub option.
- **Pregame Upgrade Overlay**: Bottom overlay active during `GameState.Pregame`, featuring "Upgrade Swarm", "Upgrade Income", and "TAP TO RUN".

#### UXML Structure (`GameHUD.uxml`)
```xml
<?xml version="1.0" encoding="utf-8"?>
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:uie="UnityEditor.UIElements">
    <ui:VisualElement name="HUDContainer" class="hud-container">
        <!-- Top Bar -->
        <ui:VisualElement name="TopBar" class="top-bar">
            <!-- Coin Badge -->
            <ui:VisualElement name="CoinBadge" class="coin-badge">
                <ui:VisualElement name="CoinIcon" class="icon-coin" />
                <ui:Label name="CoinCountLabel" text="0" class="badge-text" />
            </ui:VisualElement>

            <!-- Level Progress Bar -->
            <ui:VisualElement name="ProgressContainer" class="progress-container">
                <ui:Label name="CurrentLevelLabel" text="LVL 1" class="level-badge-text" />
                <ui:VisualElement name="ProgressBarTrack" class="progress-bar-track">
                    <ui:VisualElement name="ProgressBarFill" class="progress-bar-fill" style="width: 0%;" />
                </ui:VisualElement>
                <ui:Label name="NextLevelLabel" text="LVL 2" class="level-badge-text" />
            </ui:VisualElement>

            <!-- Settings Button -->
            <ui:Button name="SettingsButton" class="btn-settings">
                <ui:VisualElement name="SettingsIcon" class="icon-gear" />
            </ui:Button>
        </ui:VisualElement>

        <!-- Pregame Overlay -->
        <ui:VisualElement name="PregameOverlay" class="pregame-overlay">
            <ui:VisualElement name="UpgradePanel" class="upgrade-panel">
                <ui:Button name="UpgradeSwarmButton" class="btn-upgrade" text="Upgrade Swarm ($50)" />
                <ui:Button name="UpgradeIncomeButton" class="btn-upgrade" text="Upgrade Income ($50)" />
            </ui:VisualElement>
            <ui:Label name="TapToStartLabel" text="TAP TO RUN" class="pulse-text" />
        </ui:VisualElement>

        <!-- Settings Modal Overlay (Hidden by default) -->
        <ui:VisualElement name="SettingsModal" class="modal-overlay hidden">
            <ui:VisualElement name="ModalWindow" class="modal-window">
                <ui:Label text="SETTINGS" class="modal-title" />
                <ui:Button name="ToggleSoundBtn" class="btn-modal" text="Sound: ON" />
                <ui:Button name="ToggleHapticsBtn" class="btn-modal" text="Haptics: ON" />
                <ui:Button name="ReturnToLevelSelectBtn" class="btn-modal btn-danger" text="Level Select" />
                <ui:Button name="CloseSettingsBtn" class="btn-modal btn-close" text="Resume" />
            </ui:VisualElement>
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

---

### 3.2 Screen 2: Level Select Screen (Levels 1–21)

#### Functional Requirements
- Displays a 3-column scrollable grid of 21 level buttons.
- State rendering per button:
  - **Unlocked / Completed**: Solid colored card, level number, 0 to 3 gold stars earned. Clickable to launch level.
  - **Current Active Level**: Highlighted pulsing border, level number, "PLAY" tag.
  - **Locked Level**: Dimmed grey card, padlock icon, non-clickable.
- Header showing total gold and total stars collected across all levels (e.g. "STARS: 42/63").

#### UXML Structure (`LevelSelect.uxml`)
```xml
<?xml version="1.0" encoding="utf-8"?>
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="LevelSelectContainer" class="level-select-container">
        <!-- Header -->
        <ui:VisualElement name="HeaderBar" class="header-bar">
            <ui:Label text="SELECT LEVEL" class="header-title" />
            <ui:VisualElement name="HeaderStats" class="header-stats">
                <ui:Label name="TotalStarsLabel" text="★ 0/63" class="stat-text-star" />
                <ui:Label name="TotalGoldLabel" text="💰 0" class="stat-text-gold" />
            </ui:VisualElement>
        </ui:VisualElement>

        <!-- Scrollable Grid -->
        <ui:ScrollView name="GridScrollView" class="grid-scroll-view">
            <ui:VisualElement name="LevelGridContainer" class="level-grid" />
        </ui:ScrollView>

        <!-- Back to Hub / Shop -->
        <ui:VisualElement name="FooterBar" class="footer-bar">
            <ui:Button name="ShopButton" class="btn-footer" text="SKIN SHOP" />
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

#### Level Card Template (`LevelCardItem.uxml`)
```xml
<?xml version="1.0" encoding="utf-8"?>
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:Button name="LevelCardButton" class="level-card level-unlocked">
        <ui:Label name="LevelNumberLabel" text="1" class="level-number" />
        <ui:VisualElement name="StarContainer" class="star-container">
            <ui:VisualElement name="Star1" class="star-icon star-off" />
            <ui:VisualElement name="Star2" class="star-icon star-off" />
            <ui:VisualElement name="Star3" class="star-icon star-off" />
        </ui:VisualElement>
        <ui:VisualElement name="LockOverlay" class="lock-overlay hidden">
            <ui:VisualElement class="icon-lock" />
        </ui:VisualElement>
    </ui:Button>
</ui:UXML>
```

---

### 3.3 Screen 3: Victory Screen

#### Functional Requirements
- **Victory Banner**: "LEVEL COMPLETED!" header with particle/glow background.
- **Star Rating**: 1 to 3 animated stars filling up based on run performance (e.g. 1 star for reaching goal, 2 stars for saving 50%+ coins/swarm, 3 stars for saving 80%+).
- **Coins Summary**: Displays Base Coins Collected + Income Level Bonus.
- **Multiplier Wheel Mini-Game**: Oscillating bonus indicator (1.5x, 2.0x, 3.0x, 5.0x). Pressing "CLAIM x5" stopped at peak awards multiplied coins (or triggers optional rewarded ad double).
- **Navigation Buttons**: "NEXT LEVEL" (primary call-to-action button) and "LEVEL SELECT".

#### UXML Structure (`VictoryScreen.uxml`)
```xml
<?xml version="1.0" encoding="utf-8"?>
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="VictoryContainer" class="modal-overlay screen-victory">
        <ui:VisualElement name="VictoryCard" class="card-modal">
            <ui:Label text="VICTORY!" class="title-victory" />
            
            <!-- Animated Stars -->
            <ui:VisualElement name="StarsEarnedGroup" class="stars-earned-group">
                <ui:VisualElement name="VicStar1" class="star-big star-off" />
                <ui:VisualElement name="VicStar2" class="star-big star-off" />
                <ui:VisualElement name="VicStar3" class="star-big star-off" />
            </ui:VisualElement>

            <!-- Rewards Breakdown -->
            <ui:VisualElement name="RewardSummary" class="reward-summary">
                <ui:Label name="CoinsCollectedLabel" text="+120 COINS" class="reward-text" />
            </ui:VisualElement>

            <!-- Multiplier Wheel Bonus -->
            <ui:VisualElement name="MultiplierWheelContainer" class="multiplier-container">
                <ui:VisualElement name="MultiplierTrack" class="multiplier-track">
                    <ui:VisualElement name="MultiplierPointer" class="multiplier-pointer" style="left: 0%;" />
                </ui:VisualElement>
                <ui:Label name="CurrentMultiplierLabel" text="2.5x" class="multiplier-label" />
            </ui:VisualElement>

            <!-- Action Buttons -->
            <ui:Button name="ClaimMultiplierBtn" class="btn-action btn-gold" text="CLAIM WITH MULTIPLIER" />
            <ui:Button name="NextLevelBtn" class="btn-action btn-primary" text="NEXT LEVEL" />
            <ui:Button name="VictoryLevelSelectBtn" class="btn-action btn-secondary" text="LEVEL SELECT" />
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

---

### 3.4 Screen 4: Defeat Screen

#### Functional Requirements
- **Defeat Banner**: "LEVEL FAILED" with warning styling.
- **Progress Reached**: Displays percentage of level completed (e.g. "COMPLETED 82% OF LEVEL 5").
- **Coins Saved**: Coins collected up to death point.
- **Revive Button**: Prominent "REVIVE (+10 SWARM / CONTINUE)" button (triggers mock ad or spend gold).
- **Retry Button**: Instantly restarts current level.
- **Level Select Button**: Returns to Level Select menu.

#### UXML Structure (`DefeatScreen.uxml`)
```xml
<?xml version="1.0" encoding="utf-8"?>
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="DefeatContainer" class="modal-overlay screen-defeat">
        <ui:VisualElement name="DefeatCard" class="card-modal">
            <ui:Label text="LEVEL FAILED!" class="title-defeat" />

            <ui:Label name="DefeatProgressLabel" text="Reached 75% of Level" class="subtitle-defeat" />
            <ui:Label name="DefeatCoinsLabel" text="Coins Gained: 45" class="reward-text" />

            <!-- Action Buttons -->
            <ui:Button name="ReviveButton" class="btn-action btn-revive" text="REVIVE & CONTINUE" />
            <ui:Button name="RetryButton" class="btn-action btn-primary" text="RETRY LEVEL" />
            <ui:Button name="DefeatLevelSelectBtn" class="btn-action btn-secondary" text="LEVEL SELECT" />
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

---

## 4. USS Styling Specifications (`Styles/Common.uss`)

```css
/* Hyper-Casual Vibrant UI Palette */
:root {
    --primary-color: #4CAF50;
    --primary-hover: #45a049;
    --gold-color: #FFC107;
    --gold-hover: #FFB300;
    --danger-color: #F44336;
    --bg-overlay: rgba(15, 23, 42, 0.85);
    --card-bg: rgba(30, 41, 59, 0.95);
    --font-bold: 'Inter-Bold';
}

.hud-container {
    flex-grow: 1;
    justify-content: space-between;
}

.top-bar {
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    padding: 16px 24px;
    background-color: rgba(0, 0, 0, 0.4);
}

.coin-badge {
    flex-direction: row;
    align-items: center;
    background-color: rgba(0, 0, 0, 0.6);
    border-radius: 20px;
    padding: 6px 16px;
    border-width: 2px;
    border-color: var(--gold-color);
}

.badge-text {
    font-size: 22px;
    color: #FFFFFF;
    -unity-font-style: bold;
    margin-left: 8px;
}

.progress-container {
    flex-direction: row;
    align-items: center;
    width: 45%;
}

.progress-bar-track {
    flex-grow: 1;
    height: 18px;
    background-color: rgba(255, 255, 255, 0.2);
    border-radius: 9px;
    margin: 0 10px;
    overflow: hidden;
}

.progress-bar-fill {
    height: 100%;
    background-color: var(--primary-color);
    border-radius: 9px;
    transition-property: width;
    transition-duration: 0.15s;
}

.level-badge-text {
    font-size: 18px;
    color: #FFFFFF;
    -unity-font-style: bold;
}

/* Level Grid Styling */
.level-grid {
    flex-direction: row;
    flex-wrap: wrap;
    justify-content: center;
    padding: 20px;
}

.level-card {
    width: 90px;
    height: 100px;
    margin: 12px;
    border-radius: 16px;
    background-color: #334155;
    align-items: center;
    justify-content: center;
    border-width: 3px;
    border-color: #475569;
}

.level-card-unlocked {
    background-color: #2563EB;
    border-color: #60A5FA;
}

.level-card-active {
    background-color: #059669;
    border-color: #34D399;
}

.level-card-locked {
    background-color: #1E293B;
    border-color: #334155;
    opacity: 0.6;
}

/* Modal Windows */
.modal-overlay {
    position: absolute;
    top: 0; left: 0; right: 0; bottom: 0;
    background-color: var(--bg-overlay);
    align-items: center;
    justify-content: center;
}

.card-modal {
    width: 420px;
    background-color: var(--card-bg);
    border-radius: 24px;
    padding: 32px;
    align-items: center;
    border-width: 2px;
    border-color: rgba(255, 255, 255, 0.1);
}

.title-victory {
    font-size: 40px;
    color: var(--gold-color);
    -unity-font-style: bold;
    margin-bottom: 20px;
}

.title-defeat {
    font-size: 40px;
    color: var(--danger-color);
    -unity-font-style: bold;
    margin-bottom: 20px;
}

.btn-action {
    width: 100%;
    height: 54px;
    border-radius: 14px;
    font-size: 20px;
    -unity-font-style: bold;
    color: #FFFFFF;
    margin-top: 12px;
}

.btn-primary { background-color: var(--primary-color); }
.btn-gold { background-color: var(--gold-color); color: #000000; }
.btn-secondary { background-color: #64748B; }
.btn-revive { background-color: #8B5CF6; }

.hidden {
    display: none;
}
```

---

## 5. DOTS Entity Event & `UIManagerSystem.cs` Binding Architecture

To maintain high performance in DOTS Entities 1.0+, `UIManagerSystem` serves as a managed UI bridge (or Presentation System) that queries the ECS world without creating GC allocations on every frame.

### 5.1 Query Targets in ECS
1. **`LevelStateComponent`**: Singleton storing `GameState CurrentState` (`Pregame`, `Playing`, `Victory`, `Defeat`).
2. **`PlayerCoinRunnerComponent`**: Component on player entity storing `CurrentCoinCount`.
3. **`PlayerComponent` & `LocalTransform`**: Queries player position Z (`LocalTransform.Position.z`).
4. **`EndZoneComponent` & `LocalTransform`**: Queries level goal position Z (`LocalTransform.Position.z`).

### 5.2 State Transition State Machine
The `UIManagerSystem` monitors changes to `LevelStateComponent.CurrentState` and updates visual displays accordingly:

| GameState | HUD | Pregame Overlay | Victory Screen | Defeat Screen | Level Select Screen |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Pregame** | Visible | Visible | Hidden | Hidden | Hidden |
| **Playing** | Visible | Hidden | Hidden | Hidden | Hidden |
| **Victory** | Hidden | Hidden | Visible | Hidden | Hidden |
| **Defeat** | Hidden | Hidden | Hidden | Visible | Hidden |
| **LevelSelect** | Hidden | Hidden | Hidden | Hidden | Visible |

### 5.3 Multiplier Wheel Animation Logic
During the Victory screen:
- A local timer `_multiplierTimer` advances back and forth across a sine wave: `float pingPong = math.pingpong(Time.time * 2.5f, 1.0f);`.
- Pointer visual offset: `pointerElement.style.left = Length.Percent(pingPong * 100f);`.
- Multiplier value calculation: `float multiplier = 1.5f + (math.sin(pingPong * math.PI) * 3.5f);` (ranges from 1.5x to 5.0x).
- Stopping/clicking "CLAIM WITH MULTIPLIER" freezes the calculation and adds `(CurrentCoinCount * multiplier)` to `GameProgressData.TotalGold`.

---

## 6. Meta-Progression Persistence (`GameProgressData.cs`) Extensions

To support the 21-level progression grid and star tracking, `GameProgressData` must be extended with:

```csharp
namespace HyperCasualRunner
{
    public static class GameProgressData
    {
        // Existing keys...

        private const string HighestUnlockedLevelKey = "HCR_HighestUnlockedLevel";
        private const string LevelStarsPrefix = "HCR_LevelStars_";

        public static int HighestUnlockedLevel
        {
            get => PlayerPrefs.GetInt(HighestUnlockedLevelKey, 1);
            set { PlayerPrefs.SetInt(HighestUnlockedLevelKey, value); PlayerPrefs.Save(); }
        }

        public static int GetLevelStars(int levelIndex)
        {
            return PlayerPrefs.GetInt(LevelStarsPrefix + levelIndex, 0);
        }

        public static void SetLevelStars(int levelIndex, int stars)
        {
            int current = GetLevelStars(levelIndex);
            if (stars > current)
            {
                PlayerPrefs.SetInt(LevelStarsPrefix + levelIndex, stars);
                PlayerPrefs.Save();
            }
        }

        public static int GetTotalStarsCollected()
        {
            int total = 0;
            for (int i = 1; i <= 21; i++)
            {
                total += GetLevelStars(i);
            }
            return total;
        }

        public static void UnlockNextLevel(int completedLevelIndex)
        {
            if (completedLevelIndex >= HighestUnlockedLevel && HighestUnlockedLevel < 21)
            {
                HighestUnlockedLevel = completedLevelIndex + 1;
            }
        }
    }
}
```

---

## 7. Verification Method

1. **Hierarchy Verification**: Validate that all UXML documents compile cleanly under Unity UI Toolkit without syntax or schema errors.
2. **UI Constraint Compliance**: Verify 0 references to `UnityEngine.UI` or `Canvas` across all UI files.
3. **State Transition Test**: In Unity Play Mode, trigger state changes on `LevelStateComponent` (`Pregame` -> `Playing` -> `Victory` / `Defeat`) and verify corresponding VisualElements set `DisplayStyle.Flex` / `DisplayStyle.None`.
4. **Progress Bar Binding Test**: Verify that moving player Z position updates `ProgressBarFill.style.width` smoothly from 0% to 100%.
5. **Level Grid Verification**: Ensure Level Select populates exactly 21 cards, unlocks level $N+1$ when level $N$ is completed, and saves star ratings in `PlayerPrefs`.

---
