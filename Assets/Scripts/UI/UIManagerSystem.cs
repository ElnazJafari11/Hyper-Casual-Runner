using UnityEngine;
using UnityEngine.UIElements;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(UIDocument))]
    public class UIManagerSystem : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private VisualElement _root;

        // HUD Elements
        private VisualElement _hudContainer;
        private Label _levelLabel;
        private Label _goldLabel;
        private VisualElement _progressFill;
        private Label _startLevelLabel;
        private Label _endLevelLabel;
        private Button _settingsBtn;
        private VisualElement _settingsOverlay;
        private Button _soundToggleBtn;
        private Button _closeSettingsBtn;

        // Pregame Panel Elements
        private VisualElement _pregamePanel;
        private Button _upgradeSwarmBtn;
        private Button _upgradeIncomeBtn;
        private Button _levelSelectBtn;
        private Button _startBtn;

        // Level Select Elements
        private VisualElement _levelSelectOverlay;
        private VisualElement _levelGridContainer;
        private Button _closeLevelSelectBtn;

        // Victory Panel Elements
        private VisualElement _victoryOverlay;
        private Label _vicStarsLabel;
        private Label _coinsEarnedLabel;
        private Label _multiplierLabel;
        private VisualElement _tickerBar;
        private Button _claimMultipliedBtn;
        private Button _watchAdDoubleBtn;
        private Button _nextLevelBtn;

        // Defeat Panel Elements
        private VisualElement _defeatOverlay;
        private Label _progressPctLabel;
        private Button _reviveAdBtn;
        private Button _retryBtn;
        private Button _defeatLevelSelectBtn;

        // Public Properties for Testing & External Access
        public VisualElement LevelGridContainer => _levelGridContainer;
        public VisualElement RootElement => _root;
        public int LastEarnedStars => _lastEarnedStars;
        public bool HasProcessedVictorySave => _hasProcessedVictorySave;

        // Internal State
        private float _multiplierTimer = 0f;
        private float _currentMultiplier = 1.5f;
        private int _lastRunCoins = 0;
        private int _lastEarnedStars = 3;
        private bool _isSoundOn = true;
        private bool _hasProcessedVictorySave = false;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            InitializeUI();
        }

        private void OnEnable()
        {
            InitializeUI();
        }

        private void OnDisable()
        {
            UnbindCallbacks();
        }

        private void OnDestroy()
        {
            UnbindCallbacks();
        }

        private void InitializeUI()
        {
            if (_uiDocument != null)
            {
                try
                {
                    _root = _uiDocument.rootVisualElement;
                }
                catch
                {
                    _root = null;
                }
            }
            if (_root == null)
            {
                _root = new VisualElement();
            }

            // Query UXML Elements
            _hudContainer = _root.Q<VisualElement>("MainHUDContainer");
            _levelLabel = _root.Q<Label>("LevelLabel");
            _goldLabel = _root.Q<Label>("GoldLabel");
            _progressFill = _root.Q<VisualElement>("ProgressFill");
            _startLevelLabel = _root.Q<Label>("StartLevelLabel");
            _endLevelLabel = _root.Q<Label>("EndLevelLabel");
            _settingsBtn = _root.Q<Button>("SettingsBtn");
            _settingsOverlay = _root.Q<VisualElement>("SettingsOverlay");
            _soundToggleBtn = _root.Q<Button>("SoundToggleBtn");
            _closeSettingsBtn = _root.Q<Button>("CloseSettingsBtn");

            _pregamePanel = _root.Q<VisualElement>("PregamePanel");
            _upgradeSwarmBtn = _root.Q<Button>("UpgradeSwarmBtn");
            _upgradeIncomeBtn = _root.Q<Button>("UpgradeIncomeBtn");
            _levelSelectBtn = _root.Q<Button>("LevelSelectBtn");
            _startBtn = _root.Q<Button>("StartBtn");

            _levelSelectOverlay = _root.Q<VisualElement>("LevelSelectOverlay");
            _levelGridContainer = _root.Q<VisualElement>("LevelGridContainer");
            _closeLevelSelectBtn = _root.Q<Button>("CloseBtn");

            _victoryOverlay = _root.Q<VisualElement>("VictoryOverlay");
            _vicStarsLabel = _root.Q<Label>("VicStarsLabel");
            _coinsEarnedLabel = _root.Q<Label>("CoinsEarnedLabel");
            _multiplierLabel = _root.Q<Label>("MultiplierLabel");
            _tickerBar = _root.Q<VisualElement>("TickerBar");
            _claimMultipliedBtn = _root.Q<Button>("ClaimMultipliedBtn");
            _watchAdDoubleBtn = _root.Q<Button>("WatchAdDoubleBtn");
            _nextLevelBtn = _root.Q<Button>("NextLevelBtn");

            _defeatOverlay = _root.Q<VisualElement>("DefeatOverlay");
            _progressPctLabel = _root.Q<Label>("ProgressPctLabel");
            _reviveAdBtn = _root.Q<Button>("ReviveAdBtn");
            _retryBtn = _root.Q<Button>("RetryBtn");
            _defeatLevelSelectBtn = _root.Q<Button>("DefeatLevelSelectBtn");

            // Build programmatically if root has no children
            if (_hudContainer == null)
            {
                BuildFallbackUI();
            }

            BindCallbacks();
            RefreshUpgradeButtons();
            BuildLevelGrid();
        }

        private void UnbindCallbacks()
        {
            if (_startBtn != null) _startBtn.clicked -= OnStartClicked;
            if (_upgradeSwarmBtn != null) _upgradeSwarmBtn.clicked -= OnUpgradeSwarmClicked;
            if (_upgradeIncomeBtn != null) _upgradeIncomeBtn.clicked -= OnUpgradeIncomeClicked;
            if (_levelSelectBtn != null) _levelSelectBtn.clicked -= OnLevelSelectClicked;
            if (_closeLevelSelectBtn != null) _closeLevelSelectBtn.clicked -= OnCloseLevelSelectClicked;
            if (_settingsBtn != null) _settingsBtn.clicked -= OnOpenSettingsClicked;
            if (_closeSettingsBtn != null) _closeSettingsBtn.clicked -= OnCloseSettingsClicked;
            if (_soundToggleBtn != null) _soundToggleBtn.clicked -= OnSoundToggleClicked;

            if (_claimMultipliedBtn != null) _claimMultipliedBtn.clicked -= OnClaimMultipliedClicked;
            if (_watchAdDoubleBtn != null) _watchAdDoubleBtn.clicked -= OnWatchAdDoubleClicked;
            if (_nextLevelBtn != null) _nextLevelBtn.clicked -= OnNextLevelClicked;

            if (_reviveAdBtn != null) _reviveAdBtn.clicked -= OnReviveAdClicked;
            if (_retryBtn != null) _retryBtn.clicked -= OnRetryClicked;
            if (_defeatLevelSelectBtn != null) _defeatLevelSelectBtn.clicked -= OnDefeatLevelSelectClicked;
        }

        private void BindCallbacks()
        {
            UnbindCallbacks();

            if (_startBtn != null) _startBtn.clicked += OnStartClicked;
            if (_upgradeSwarmBtn != null) _upgradeSwarmBtn.clicked += OnUpgradeSwarmClicked;
            if (_upgradeIncomeBtn != null) _upgradeIncomeBtn.clicked += OnUpgradeIncomeClicked;
            if (_levelSelectBtn != null) _levelSelectBtn.clicked += OnLevelSelectClicked;
            if (_closeLevelSelectBtn != null) _closeLevelSelectBtn.clicked += OnCloseLevelSelectClicked;
            if (_settingsBtn != null) _settingsBtn.clicked += OnOpenSettingsClicked;
            if (_closeSettingsBtn != null) _closeSettingsBtn.clicked += OnCloseSettingsClicked;
            if (_soundToggleBtn != null) _soundToggleBtn.clicked += OnSoundToggleClicked;

            if (_claimMultipliedBtn != null) _claimMultipliedBtn.clicked += OnClaimMultipliedClicked;
            if (_watchAdDoubleBtn != null) _watchAdDoubleBtn.clicked += OnWatchAdDoubleClicked;
            if (_nextLevelBtn != null) _nextLevelBtn.clicked += OnNextLevelClicked;

            if (_reviveAdBtn != null) _reviveAdBtn.clicked += OnReviveAdClicked;
            if (_retryBtn != null) _retryBtn.clicked += OnRetryClicked;
            if (_defeatLevelSelectBtn != null) _defeatLevelSelectBtn.clicked += OnDefeatLevelSelectClicked;
        }

        private void OnUpgradeSwarmClicked() => TryUpgrade(true);
        private void OnUpgradeIncomeClicked() => TryUpgrade(false);
        private void OnLevelSelectClicked() => OpenLevelSelect();
        private void OnCloseLevelSelectClicked() => CloseLevelSelect();
        private void OnOpenSettingsClicked() => ShowSettings(true);
        private void OnCloseSettingsClicked() => ShowSettings(false);
        private void OnSoundToggleClicked() => ToggleSound();
        private void OnClaimMultipliedClicked() => ClaimMultipliedGold();
        private void OnWatchAdDoubleClicked() => WatchAdDoubleGold();
        private void OnNextLevelClicked() => ClaimBaseGoldAndNextLevel();
        private void OnReviveAdClicked() => WatchAdRevive();
        private void OnRetryClicked() => RetryLevel();
        private void OnDefeatLevelSelectClicked() => OpenLevelSelect();

        private void BuildFallbackUI()
        {
            _root.Clear();

            // Root HUD Container
            _hudContainer = new VisualElement();
            _hudContainer.style.flexGrow = 1;
            _hudContainer.style.justifyContent = Justify.SpaceBetween;
            _hudContainer.style.paddingLeft = 10; _hudContainer.style.paddingRight = 10;
            _hudContainer.style.paddingTop = 10; _hudContainer.style.paddingBottom = 10;
            _root.Add(_hudContainer);

            // Top Bar
            VisualElement topBar = new VisualElement();
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.justifyContent = Justify.SpaceBetween;
            topBar.style.alignItems = Align.Center;

            _levelLabel = new Label($"LEVEL {GameProgressData.CurrentLevelIndex + 1}");
            _levelLabel.style.fontSize = 20; _levelLabel.style.color = Color.white;
            topBar.Add(_levelLabel);

            _goldLabel = new Label($"Gold: {GameProgressData.TotalGold}");
            _goldLabel.style.fontSize = 20; _goldLabel.style.color = Color.yellow;
            topBar.Add(_goldLabel);

            _settingsBtn = new Button { text = "⚙" };
            _settingsBtn.style.fontSize = 24;
            topBar.Add(_settingsBtn);

            _hudContainer.Add(topBar);

            // Progress Bar
            VisualElement progressContainer = new VisualElement();
            progressContainer.style.flexDirection = FlexDirection.Row;
            progressContainer.style.alignItems = Align.Center;
            progressContainer.style.marginTop = 10;

            _startLevelLabel = new Label($"{GameProgressData.CurrentLevelIndex + 1}");
            _startLevelLabel.style.color = Color.white;
            progressContainer.Add(_startLevelLabel);

            VisualElement track = new VisualElement();
            track.style.flexGrow = 1; track.style.height = 16;
            track.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.5f));

            _progressFill = new VisualElement();
            _progressFill.style.height = Length.Percent(100);
            _progressFill.style.width = Length.Percent(0);
            _progressFill.style.backgroundColor = new StyleColor(Color.green);
            track.Add(_progressFill);
            progressContainer.Add(track);

            _endLevelLabel = new Label($"{GameProgressData.CurrentLevelIndex + 2}");
            _endLevelLabel.style.color = Color.white;
            progressContainer.Add(_endLevelLabel);

            _hudContainer.Add(progressContainer);

            // Pregame Panel
            _pregamePanel = new VisualElement();
            _pregamePanel.style.alignItems = Align.Center;
            _pregamePanel.style.justifyContent = Justify.FlexEnd;
            _pregamePanel.style.paddingBottom = 40;

            VisualElement upgradeBox = new VisualElement();
            upgradeBox.style.flexDirection = FlexDirection.Row;
            upgradeBox.style.justifyContent = Justify.SpaceAround;
            upgradeBox.style.width = Length.Percent(100);

            _upgradeSwarmBtn = new Button();
            _upgradeIncomeBtn = new Button();
            upgradeBox.Add(_upgradeSwarmBtn);
            upgradeBox.Add(_upgradeIncomeBtn);
            _pregamePanel.Add(upgradeBox);

            _levelSelectBtn = new Button { text = "LEVEL SELECT" };
            _levelSelectBtn.style.fontSize = 18; _levelSelectBtn.style.marginBottom = 10;
            _pregamePanel.Add(_levelSelectBtn);

            _startBtn = new Button { text = "TAP TO START" };
            _startBtn.style.fontSize = 28;
            _pregamePanel.Add(_startBtn);

            _root.Add(_pregamePanel);

            // Settings Overlay
            _settingsOverlay = new VisualElement();
            _settingsOverlay.style.position = Position.Absolute;
            _settingsOverlay.style.top = 0; _settingsOverlay.style.bottom = 0;
            _settingsOverlay.style.left = 0; _settingsOverlay.style.right = 0;
            _settingsOverlay.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.8f));
            _settingsOverlay.style.justifyContent = Justify.Center;
            _settingsOverlay.style.alignItems = Align.Center;
            _settingsOverlay.style.display = DisplayStyle.None;

            VisualElement settingsCard = new VisualElement();
            settingsCard.style.width = 300;
            settingsCard.style.paddingLeft = 20; settingsCard.style.paddingRight = 20;
            settingsCard.style.paddingTop = 20; settingsCard.style.paddingBottom = 20;
            settingsCard.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.2f));

            Label title = new Label("SETTINGS") { style = { fontSize = 24, color = Color.yellow } };
            settingsCard.Add(title);

            _soundToggleBtn = new Button { text = "Sound: ON" };
            settingsCard.Add(_soundToggleBtn);

            _closeSettingsBtn = new Button { text = "CLOSE" };
            settingsCard.Add(_closeSettingsBtn);
            _settingsOverlay.Add(settingsCard);
            _root.Add(_settingsOverlay);

            // Level Select Overlay
            _levelSelectOverlay = new VisualElement();
            _levelSelectOverlay.style.position = Position.Absolute;
            _levelSelectOverlay.style.top = 0; _levelSelectOverlay.style.bottom = 0;
            _levelSelectOverlay.style.left = 0; _levelSelectOverlay.style.right = 0;
            _levelSelectOverlay.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.85f));
            _levelSelectOverlay.style.justifyContent = Justify.Center;
            _levelSelectOverlay.style.alignItems = Align.Center;
            _levelSelectOverlay.style.display = DisplayStyle.None;

            VisualElement selectModal = new VisualElement();
            selectModal.style.width = Length.Percent(90); selectModal.style.height = Length.Percent(80);
            selectModal.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.18f));
            selectModal.style.paddingLeft = 15; selectModal.style.paddingRight = 15;
            selectModal.style.paddingTop = 15; selectModal.style.paddingBottom = 15;

            VisualElement header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;

            Label selTitle = new Label("SELECT LEVEL") { style = { fontSize = 24, color = Color.yellow } };
            header.Add(selTitle);

            _closeLevelSelectBtn = new Button { text = "✕" };
            header.Add(_closeLevelSelectBtn);
            selectModal.Add(header);

            ScrollView sv = new ScrollView();
            sv.style.flexGrow = 1;
            _levelGridContainer = new VisualElement { name = "LevelGridContainer" };
            _levelGridContainer.style.flexDirection = FlexDirection.Row;
            _levelGridContainer.style.flexWrap = Wrap.Wrap;
            _levelGridContainer.style.justifyContent = Justify.SpaceBetween;
            sv.Add(_levelGridContainer);
            selectModal.Add(sv);

            _levelSelectOverlay.Add(selectModal);
            _root.Add(_levelSelectOverlay);

            // Victory Overlay
            _victoryOverlay = new VisualElement();
            _victoryOverlay.style.position = Position.Absolute;
            _victoryOverlay.style.top = 0; _victoryOverlay.style.bottom = 0;
            _victoryOverlay.style.left = 0; _victoryOverlay.style.right = 0;
            _victoryOverlay.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.85f));
            _victoryOverlay.style.justifyContent = Justify.Center;
            _victoryOverlay.style.alignItems = Align.Center;
            _victoryOverlay.style.display = DisplayStyle.None;

            VisualElement vicCard = new VisualElement();
            vicCard.style.width = Length.Percent(85);
            vicCard.style.paddingLeft = 20; vicCard.style.paddingRight = 20;
            vicCard.style.paddingTop = 20; vicCard.style.paddingBottom = 20;
            vicCard.style.backgroundColor = new StyleColor(new Color(0.07f, 0.1f, 0.16f));
            vicCard.style.alignItems = Align.Center;

            Label vicTitle = new Label("LEVEL COMPLETE!") { style = { fontSize = 32, color = Color.yellow } };
            vicCard.Add(vicTitle);

            _vicStarsLabel = new Label("★ ★ ★") { style = { fontSize = 36, color = Color.yellow } };
            vicCard.Add(_vicStarsLabel);

            _coinsEarnedLabel = new Label("Coins Earned: +0") { style = { fontSize = 20, color = Color.white } };
            vicCard.Add(_coinsEarnedLabel);

            VisualElement multBox = new VisualElement();
            multBox.style.width = Length.Percent(100);
            multBox.style.paddingLeft = 10; multBox.style.paddingRight = 10;
            multBox.style.paddingTop = 10; multBox.style.paddingBottom = 10;
            multBox.style.backgroundColor = new StyleColor(new Color(0.12f, 0.16f, 0.23f));
            multBox.style.alignItems = Align.Center;

            _multiplierLabel = new Label("MULTIPLIER: 1.5x") { style = { fontSize = 22, color = new Color(0.22f, 0.74f, 0.97f) } };
            multBox.Add(_multiplierLabel);

            VisualElement tickerTrack = new VisualElement();
            tickerTrack.style.width = Length.Percent(100); tickerTrack.style.height = 12;
            tickerTrack.style.backgroundColor = new StyleColor(new Color(0.05f, 0.09f, 0.16f));

            _tickerBar = new VisualElement();
            _tickerBar.style.width = 20; _tickerBar.style.height = Length.Percent(100);
            _tickerBar.style.backgroundColor = new StyleColor(new Color(0.96f, 0.62f, 0.04f));
            tickerTrack.Add(_tickerBar);
            multBox.Add(tickerTrack);
            vicCard.Add(multBox);

            _claimMultipliedBtn = new Button { text = "CLAIM MULTIPLIED GOLD" };
            _claimMultipliedBtn.style.fontSize = 18; _claimMultipliedBtn.style.width = Length.Percent(100);
            vicCard.Add(_claimMultipliedBtn);

            _watchAdDoubleBtn = new Button { text = "WATCH AD FOR 2x GOLD" };
            _watchAdDoubleBtn.style.fontSize = 16; _watchAdDoubleBtn.style.width = Length.Percent(100);
            vicCard.Add(_watchAdDoubleBtn);

            _nextLevelBtn = new Button { text = "NEXT LEVEL" };
            _nextLevelBtn.style.fontSize = 16; _nextLevelBtn.style.width = Length.Percent(100);
            vicCard.Add(_nextLevelBtn);

            _victoryOverlay.Add(vicCard);
            _root.Add(_victoryOverlay);

            // Defeat Overlay
            _defeatOverlay = new VisualElement();
            _defeatOverlay.style.position = Position.Absolute;
            _defeatOverlay.style.top = 0; _defeatOverlay.style.bottom = 0;
            _defeatOverlay.style.left = 0; _defeatOverlay.style.right = 0;
            _defeatOverlay.style.backgroundColor = new StyleColor(new Color(0.3f, 0, 0, 0.85f));
            _defeatOverlay.style.justifyContent = Justify.Center;
            _defeatOverlay.style.alignItems = Align.Center;
            _defeatOverlay.style.display = DisplayStyle.None;

            VisualElement defCard = new VisualElement();
            defCard.style.width = Length.Percent(85);
            defCard.style.paddingLeft = 20; defCard.style.paddingRight = 20;
            defCard.style.paddingTop = 20; defCard.style.paddingBottom = 20;
            defCard.style.backgroundColor = new StyleColor(new Color(0.18f, 0.08f, 0.09f));
            defCard.style.alignItems = Align.Center;

            Label defTitle = new Label("LEVEL FAILED!") { style = { fontSize = 32, color = Color.red } };
            defCard.Add(defTitle);

            _progressPctLabel = new Label("Completed: 0%") { style = { fontSize = 20, color = Color.white } };
            defCard.Add(_progressPctLabel);

            _reviveAdBtn = new Button { text = "WATCH AD TO REVIVE" };
            _reviveAdBtn.style.fontSize = 18; _reviveAdBtn.style.width = Length.Percent(100);
            defCard.Add(_reviveAdBtn);

            _retryBtn = new Button { text = "RETRY LEVEL" };
            _retryBtn.style.fontSize = 16; _retryBtn.style.width = Length.Percent(100);
            defCard.Add(_retryBtn);

            _defeatLevelSelectBtn = new Button { text = "LEVEL SELECT" };
            _defeatLevelSelectBtn.style.fontSize = 16; _defeatLevelSelectBtn.style.width = Length.Percent(100);
            defCard.Add(_defeatLevelSelectBtn);

            _defeatOverlay.Add(defCard);
            _root.Add(_defeatOverlay);
        }

        public void BuildLevelGrid()
        {
            if (_levelGridContainer == null)
            {
                InitializeUI();
            }
            if (_levelGridContainer == null) return;
            _levelGridContainer.Clear();

            int unlockedIndex = GameProgressData.UnlockedLevelIndex;
            int currentIndex = GameProgressData.CurrentLevelIndex;

            for (int i = 0; i < 21; i++)
            {
                int levelNum = i + 1;
                bool isUnlocked = i <= unlockedIndex;
                int stars = GameProgressData.GetLevelStars(i);

                Button card = new Button();
                card.AddToClassList("level-card-button");
                if (!isUnlocked) card.AddToClassList("card-locked");
                if (i == currentIndex) card.AddToClassList("card-playing");

                card.style.width = Length.Percent(30);
                card.style.height = 80;
                card.style.marginBottom = 10;
                card.style.alignItems = Align.Center;
                card.style.justifyContent = Justify.SpaceAround;

                Label numLabel = new Label($"Lvl {levelNum}");
                numLabel.style.fontSize = 16; numLabel.style.color = Color.white;
                card.Add(numLabel);

                VisualElement starsBox = new VisualElement();
                starsBox.style.flexDirection = FlexDirection.Row;
                for (int s = 1; s <= 3; s++)
                {
                    Label starLabel = new Label("★");
                    starLabel.style.fontSize = 14;
                    starLabel.style.color = s <= stars ? Color.yellow : Color.gray;
                    starsBox.Add(starLabel);
                }
                card.Add(starsBox);

                Label statusLabel = new Label(isUnlocked ? (i == currentIndex ? "PLAYING" : "UNLOCKED") : "LOCKED");
                statusLabel.style.fontSize = 10;
                statusLabel.style.color = isUnlocked ? Color.green : Color.gray;
                card.Add(statusLabel);

                int selectedLevel = i;
                if (isUnlocked)
                {
                    card.clicked += () => SelectLevel(selectedLevel);
                }

                _levelGridContainer.Add(card);
            }
        }

        private void RefreshUpgradeButtons()
        {
            if (_upgradeSwarmBtn != null)
                _upgradeSwarmBtn.text = $"Upgrade Swarm (${GameProgressData.GetUpgradeCost(GameProgressData.SwarmLevel)})";
            if (_upgradeIncomeBtn != null)
                _upgradeIncomeBtn.text = $"Upgrade Income (${GameProgressData.GetUpgradeCost(GameProgressData.IncomeLevel)})";
            if (_goldLabel != null)
                _goldLabel.text = $"Gold: {GameProgressData.TotalGold}";
            if (_levelLabel != null)
                _levelLabel.text = $"LEVEL {GameProgressData.CurrentLevelIndex + 1}";
            if (_startLevelLabel != null)
                _startLevelLabel.text = $"{GameProgressData.CurrentLevelIndex + 1}";
            if (_endLevelLabel != null)
                _endLevelLabel.text = $"{GameProgressData.CurrentLevelIndex + 2}";
        }

        private void OnStartClicked()
        {
            if (_pregamePanel != null) _pregamePanel.style.display = DisplayStyle.None;

            if (World.DefaultGameObjectInjectionWorld != null)
            {
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                var query = em.CreateEntityQuery(typeof(LevelStateComponent));
                if (query.CalculateEntityCount() > 0)
                {
                    var entity = query.GetSingletonEntity();
                    em.SetComponentData(entity, new LevelStateComponent { CurrentState = GameState.Playing });
                }
            }
        }

        private void TryUpgrade(bool isSwarm)
        {
            int currentLvl = isSwarm ? GameProgressData.SwarmLevel : GameProgressData.IncomeLevel;
            int cost = GameProgressData.GetUpgradeCost(currentLvl);

            if (GameProgressData.TotalGold >= cost)
            {
                GameProgressData.TotalGold -= cost;
                if (isSwarm) GameProgressData.SwarmLevel++;
                else GameProgressData.IncomeLevel++;

                RefreshUpgradeButtons();
            }
        }

        private void OpenLevelSelect()
        {
            BuildLevelGrid();
            if (_levelSelectOverlay != null) _levelSelectOverlay.style.display = DisplayStyle.Flex;
        }

        private void CloseLevelSelect()
        {
            if (_levelSelectOverlay != null) _levelSelectOverlay.style.display = DisplayStyle.None;
        }

        private void SelectLevel(int levelIndex)
        {
            GameProgressData.CurrentLevelIndex = levelIndex;
            CloseLevelSelect();
            RefreshUpgradeButtons();

            if (World.DefaultGameObjectInjectionWorld != null)
            {
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                var query = em.CreateEntityQuery(typeof(LevelSequenceComponent));
                if (query.CalculateEntityCount() > 0)
                {
                    var entity = query.GetSingletonEntity();
                    var seq = em.GetComponentData<LevelSequenceComponent>(entity);
                    seq.CurrentLevelIndex = levelIndex;
                    seq.TransitionState = LevelTransitionState.TeardownCurrent;
                    em.SetComponentData(entity, seq);
                }
            }
        }

        private void ShowSettings(bool show)
        {
            if (_settingsOverlay != null)
                _settingsOverlay.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void ToggleSound()
        {
            _isSoundOn = !_isSoundOn;
            if (_soundToggleBtn != null)
                _soundToggleBtn.text = $"Sound: {(_isSoundOn ? "ON" : "OFF")}";
            AudioListener.volume = _isSoundOn ? 1.0f : 0.0f;
        }

        public void ClaimMultipliedGold()
        {
            if (!_hasProcessedVictorySave)
            {
                _hasProcessedVictorySave = true;
                int earnedCoins = (int)(_lastRunCoins * _currentMultiplier);
                GameProgressData.SaveLevelCompletion(GameProgressData.CurrentLevelIndex, earnedCoins, _lastEarnedStars);
            }

            TriggerNextLevel();
        }

        public void ClaimBaseGoldAndNextLevel()
        {
            if (!_hasProcessedVictorySave)
            {
                _hasProcessedVictorySave = true;
                GameProgressData.SaveLevelCompletion(GameProgressData.CurrentLevelIndex, _lastRunCoins, _lastEarnedStars);
            }

            TriggerNextLevel();
        }

        private void WatchAdDoubleGold()
        {
            var adManager = Object.FindFirstObjectByType<MockAdsManager>();
            if (adManager != null)
            {
                adManager.ShowRewardedAd(() =>
                {
                    _currentMultiplier *= 2.0f;
                    _multiplierLabel.text = $"GOLD DOUBLED! ({_currentMultiplier:F1}x)";
                    if (_watchAdDoubleBtn != null) _watchAdDoubleBtn.style.display = DisplayStyle.None;
                });
            }
        }

        private void TriggerNextLevel()
        {
            if (World.DefaultGameObjectInjectionWorld != null)
            {
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                var query = em.CreateEntityQuery(typeof(LevelSequenceComponent));
                if (query.CalculateEntityCount() > 0)
                {
                    var entity = query.GetSingletonEntity();
                    var seq = em.GetComponentData<LevelSequenceComponent>(entity);
                    seq.TransitionState = LevelTransitionState.PendingNext;
                    em.SetComponentData(entity, seq);
                }
            }
            if (_victoryOverlay != null) _victoryOverlay.style.display = DisplayStyle.None;
        }

        private void WatchAdRevive()
        {
            var adManager = Object.FindFirstObjectByType<MockAdsManager>();
            if (adManager != null)
            {
                adManager.ShowRewardedAd(() =>
                {
                    if (_defeatOverlay != null) _defeatOverlay.style.display = DisplayStyle.None;
                    if (World.DefaultGameObjectInjectionWorld != null)
                    {
                        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                        var query = em.CreateEntityQuery(typeof(LevelStateComponent));
                        if (query.CalculateEntityCount() > 0)
                        {
                            var entity = query.GetSingletonEntity();
                            em.SetComponentData(entity, new LevelStateComponent { CurrentState = GameState.Playing });
                        }
                    }
                });
            }
        }

        private void RetryLevel()
        {
            if (World.DefaultGameObjectInjectionWorld != null)
            {
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                var query = em.CreateEntityQuery(typeof(LevelSequenceComponent));
                if (query.CalculateEntityCount() > 0)
                {
                    var entity = query.GetSingletonEntity();
                    var seq = em.GetComponentData<LevelSequenceComponent>(entity);
                    seq.TransitionState = LevelTransitionState.TeardownCurrent;
                    em.SetComponentData(entity, seq);
                }
            }
            if (_defeatOverlay != null) _defeatOverlay.style.display = DisplayStyle.None;
        }

        public int CalculateStars(int runCoins, int targetCoins = 10)
        {
            if (World.DefaultGameObjectInjectionWorld != null)
            {
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                var swarmQuery = em.CreateEntityQuery(typeof(SwarmComponent));
                if (swarmQuery.CalculateEntityCount() > 0)
                {
                    var swarm = swarmQuery.GetSingleton<SwarmComponent>();
                    int target = swarm.TargetCount > 0 ? swarm.TargetCount : targetCoins;
                    float swarmRatio = (float)swarm.CurrentCount / math.max(1, target);
                    if (swarmRatio >= 0.80f) return 3;
                    if (swarmRatio >= 0.50f) return 2;
                    return 1;
                }
            }

            float coinRatio = (float)runCoins / math.max(1, targetCoins);
            if (coinRatio >= 0.80f || runCoins >= 8) return 3;
            if (coinRatio >= 0.50f || runCoins >= 5) return 2;
            return 1;
        }

        public static string GetStarString(int stars)
        {
            return stars switch
            {
                3 => "★ ★ ★",
                2 => "★ ★ ☆",
                1 => "★ ☆ ☆",
                _ => "☆ ☆ ☆"
            };
        }

        private void Update()
        {
            if (World.DefaultGameObjectInjectionWorld == null) return;
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            // 1. Read Live Coin Count
            int runCoins = 0;
            var coinQuery = em.CreateEntityQuery(typeof(PlayerCoinRunnerComponent));
            if (coinQuery.CalculateEntityCount() > 0)
            {
                var runner = coinQuery.GetSingleton<PlayerCoinRunnerComponent>();
                runCoins = runner.CurrentCoinCount;
            }
            _lastRunCoins = runCoins;

            if (_goldLabel != null)
            {
                _goldLabel.text = $"Gold: {GameProgressData.TotalGold + (runCoins * GameProgressData.IncomeLevel)}";
            }

            // 2. Read Progress (Player Z vs EndZone Z)
            float playerZ = 0f;
            float endZoneZ = 100f; // Default level length fallback

            var playerQuery = em.CreateEntityQuery(typeof(PlayerComponent), typeof(LocalTransform));
            if (playerQuery.CalculateEntityCount() > 0)
            {
                var pTransform = playerQuery.GetSingleton<LocalTransform>();
                playerZ = pTransform.Position.z;
            }

            var endZoneQuery = em.CreateEntityQuery(typeof(EndZoneComponent), typeof(LocalTransform));
            if (endZoneQuery.CalculateEntityCount() > 0)
            {
                var ezTransform = endZoneQuery.GetSingleton<LocalTransform>();
                endZoneZ = ezTransform.Position.z;
            }

            float progressPct = Mathf.Clamp01(playerZ / math.max(1f, endZoneZ)) * 100f;
            if (_progressFill != null)
            {
                _progressFill.style.width = Length.Percent(progressPct);
            }

            // 3. Handle Game State Panels
            var stateQuery = em.CreateEntityQuery(typeof(LevelStateComponent));
            if (stateQuery.CalculateEntityCount() > 0)
            {
                var state = stateQuery.GetSingleton<LevelStateComponent>();

                if (state.CurrentState == GameState.Pregame)
                {
                    if (_pregamePanel != null) _pregamePanel.style.display = DisplayStyle.Flex;
                    if (_victoryOverlay != null) _victoryOverlay.style.display = DisplayStyle.None;
                    if (_defeatOverlay != null) _defeatOverlay.style.display = DisplayStyle.None;
                    _hasProcessedVictorySave = false;
                }
                else if (state.CurrentState == GameState.Playing)
                {
                    if (_pregamePanel != null) _pregamePanel.style.display = DisplayStyle.None;
                    if (_victoryOverlay != null) _victoryOverlay.style.display = DisplayStyle.None;
                    if (_defeatOverlay != null) _defeatOverlay.style.display = DisplayStyle.None;
                }
                else if (state.CurrentState == GameState.Victory)
                {
                    if (_victoryOverlay != null) _victoryOverlay.style.display = DisplayStyle.Flex;

                    // Animate Multiplier Wheel Ticker
                    _multiplierTimer += Time.deltaTime * 3.0f;
                    float t = Mathf.PingPong(_multiplierTimer, 1.0f);
                    _currentMultiplier = Mathf.Lerp(1.5f, 5.0f, t);

                    if (_multiplierLabel != null)
                        _multiplierLabel.text = $"MULTIPLIER: {_currentMultiplier:F1}x";
                    if (_tickerBar != null)
                        _tickerBar.style.left = Length.Percent(t * 85f);
                    if (_coinsEarnedLabel != null)
                        _coinsEarnedLabel.text = $"Coins Earned: +{runCoins * GameProgressData.IncomeLevel}";

                    // Calculate stars dynamically based on run performance
                    _lastEarnedStars = CalculateStars(runCoins);
                    if (_vicStarsLabel != null)
                    {
                        _vicStarsLabel.text = GetStarString(_lastEarnedStars);
                    }
                }
                else if (state.CurrentState == GameState.Defeat)
                {
                    if (_defeatOverlay != null) _defeatOverlay.style.display = DisplayStyle.Flex;
                    if (_progressPctLabel != null)
                        _progressPctLabel.text = $"Completed: {progressPct:F0}%";
                }
            }
        }
    }
}
