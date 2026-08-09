using UnityEngine;
using UnityEngine.UIElements;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(UIDocument))]
    public class LevelSelectScreenController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private VisualTreeAsset _levelSelectScreenAsset;
        [SerializeField] private VisualTreeAsset _levelCardItemAsset;
        [SerializeField] private int _totalLevels = 21;

        private VisualElement _root;
        private VisualElement _levelSelectOverlay;
        private VisualElement _levelGridContainer;
        private Button _closeButton;

        public UIDocument UIDocument => _uiDocument;
        public VisualElement RootElement => _root;
        public VisualElement LevelGridContainer => _levelGridContainer;
        public Button CloseButton => _closeButton;
        public VisualElement LevelSelectOverlay => _levelSelectOverlay;

        public event System.Action<int> OnLevelSelected;

        private void Awake()
        {
            EnsureComponents();
            InitializeUI();
        }

        private void OnEnable()
        {
            EnsureComponents();
            InitializeUI();
        }

        private void OnDisable()
        {
            UnbindCallbacks();
        }

        public void EnsureComponents()
        {
            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
            }
        }

        public void InitializeUI()
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

            // If root has no LevelSelectOverlay and asset is assigned, instantiate it
            _levelSelectOverlay = _root.Q<VisualElement>("LevelSelectOverlay");
            if (_levelSelectOverlay == null && _levelSelectScreenAsset != null)
            {
                TemplateContainer instance = _levelSelectScreenAsset.Instantiate();
                _root.Add(instance);
                _levelSelectOverlay = _root.Q<VisualElement>("LevelSelectOverlay");
            }

            _levelGridContainer = _root.Q<VisualElement>("LevelGridContainer");
            _closeButton = _root.Q<Button>("CloseBtn");

            // Fallback layout if elements are missing
            if (_levelSelectOverlay == null || _levelGridContainer == null)
            {
                BuildFallbackUI();
            }

            BindCallbacks();
            BuildLevelGrid();
        }

        private void UnbindCallbacks()
        {
            if (_closeButton != null)
            {
                _closeButton.clicked -= CloseScreen;
            }
        }

        private void BindCallbacks()
        {
            UnbindCallbacks();
            if (_closeButton != null)
            {
                _closeButton.clicked += CloseScreen;
            }
        }

        private void BuildFallbackUI()
        {
            _root.Clear();
            _levelSelectOverlay = new VisualElement();
            _levelSelectOverlay.name = "LevelSelectOverlay";
            _levelSelectOverlay.AddToClassList("level-select-overlay");
            _levelSelectOverlay.style.position = Position.Absolute;
            _levelSelectOverlay.style.top = 0; _levelSelectOverlay.style.bottom = 0;
            _levelSelectOverlay.style.left = 0; _levelSelectOverlay.style.right = 0;
            _levelSelectOverlay.style.backgroundColor = new Color(0f, 0f, 0f, 0.85f);
            _levelSelectOverlay.style.justifyContent = Justify.Center;
            _levelSelectOverlay.style.alignItems = Align.Center;

            VisualElement modal = new VisualElement();
            modal.name = "LevelSelectModal";
            modal.AddToClassList("level-select-modal");
            modal.style.width = Length.Percent(90);
            modal.style.height = Length.Percent(80);
            modal.style.backgroundColor = new Color(0.1f, 0.1f, 0.18f);

            VisualElement header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;

            Label title = new Label("SELECT LEVEL");
            title.style.fontSize = 24;
            title.style.color = Color.yellow;
            header.Add(title);

            _closeButton = new Button { text = "✕" };
            _closeButton.name = "CloseBtn";
            _closeButton.style.width = 30;
            _closeButton.style.height = 30;
            header.Add(_closeButton);

            modal.Add(header);

            ScrollView scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;

            _levelGridContainer = new VisualElement();
            _levelGridContainer.name = "LevelGridContainer";
            _levelGridContainer.style.flexDirection = FlexDirection.Row;
            _levelGridContainer.style.flexWrap = Wrap.Wrap;
            _levelGridContainer.style.justifyContent = Justify.SpaceBetween;

            scrollView.Add(_levelGridContainer);
            modal.Add(scrollView);
            _levelSelectOverlay.Add(modal);
            _root.Add(_levelSelectOverlay);
        }

        public void BuildLevelGrid(int overrideCount = -1)
        {
            if (_levelGridContainer == null)
            {
                InitializeUI();
            }
            if (_levelGridContainer == null) return;

            _levelGridContainer.Clear();
            int total = overrideCount > 0 ? overrideCount : _totalLevels;
            int unlockedIndex = GameProgressData.UnlockedLevelIndex;
            int currentIndex = GameProgressData.CurrentLevelIndex;

            for (int i = 0; i < total; i++)
            {
                int levelNum = i + 1;
                bool isUnlocked = i <= unlockedIndex;
                bool isPlaying = i == currentIndex;
                int stars = GameProgressData.GetLevelStars(i);

                VisualElement cardContainer;
                Button cardBtn;
                Label numLabel;
                Label statusLabel;
                Label star1, star2, star3;

                if (_levelCardItemAsset != null)
                {
                    TemplateContainer cardInstance = _levelCardItemAsset.Instantiate();
                    cardBtn = cardInstance.Q<Button>("CardButton");
                    if (cardBtn == null)
                    {
                        cardBtn = cardInstance.Q<Button>();
                    }
                    numLabel = cardInstance.Q<Label>("LevelNumberLabel");
                    statusLabel = cardInstance.Q<Label>("StatusLabel");
                    star1 = cardInstance.Q<Label>("Star1");
                    star2 = cardInstance.Q<Label>("Star2");
                    star3 = cardInstance.Q<Label>("Star3");
                    cardContainer = cardInstance;
                }
                else
                {
                    cardBtn = new Button();
                    cardBtn.AddToClassList("level-card-button");
                    cardBtn.style.width = Length.Percent(30);
                    cardBtn.style.height = 80;
                    cardBtn.style.marginBottom = 10;
                    cardBtn.style.alignItems = Align.Center;
                    cardBtn.style.justifyContent = Justify.SpaceAround;

                    numLabel = new Label($"Lvl {levelNum}");
                    numLabel.name = "LevelNumberLabel";
                    numLabel.style.fontSize = 16;
                    numLabel.style.color = Color.white;
                    cardBtn.Add(numLabel);

                    VisualElement starsBox = new VisualElement();
                    starsBox.name = "StarsContainer";
                    starsBox.style.flexDirection = FlexDirection.Row;

                    star1 = new Label("★") { name = "Star1" };
                    star2 = new Label("★") { name = "Star2" };
                    star3 = new Label("★") { name = "Star3" };

                    starsBox.Add(star1);
                    starsBox.Add(star2);
                    starsBox.Add(star3);
                    cardBtn.Add(starsBox);

                    statusLabel = new Label();
                    statusLabel.name = "StatusLabel";
                    statusLabel.style.fontSize = 10;
                    cardBtn.Add(statusLabel);

                    cardContainer = cardBtn;
                }

                if (numLabel != null) numLabel.text = $"Lvl {levelNum}";

                if (cardBtn != null)
                {
                    cardBtn.RemoveFromClassList("card-locked");
                    cardBtn.RemoveFromClassList("card-playing");
                    if (!isUnlocked) cardBtn.AddToClassList("card-locked");
                    if (isPlaying) cardBtn.AddToClassList("card-playing");
                }

                if (statusLabel != null)
                {
                    statusLabel.text = isUnlocked ? (isPlaying ? "PLAYING" : "UNLOCKED") : "LOCKED";
                    statusLabel.style.color = isUnlocked ? (isPlaying ? Color.yellow : Color.green) : Color.gray;
                }

                ApplyStarStyle(star1, 1 <= stars);
                ApplyStarStyle(star2, 2 <= stars);
                ApplyStarStyle(star3, 3 <= stars);

                int selectedLevel = i;
                if (isUnlocked && cardBtn != null)
                {
                    cardBtn.clicked += () => SelectLevel(selectedLevel);
                }

                _levelGridContainer.Add(cardContainer);
            }
        }

        private void ApplyStarStyle(Label starLabel, bool active)
        {
            if (starLabel == null) return;
            starLabel.RemoveFromClassList("star-active");
            starLabel.RemoveFromClassList("star-inactive");
            if (active)
            {
                starLabel.AddToClassList("star-active");
                starLabel.style.color = Color.yellow;
            }
            else
            {
                starLabel.AddToClassList("star-inactive");
                starLabel.style.color = Color.gray;
            }
        }

        public void SelectLevel(int levelIndex)
        {
            GameProgressData.CurrentLevelIndex = levelIndex;
            CloseScreen();

            // Trigger DOTS ECS level sequence transition
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

            OnLevelSelected?.Invoke(levelIndex);
        }

        public void OpenScreen()
        {
            BuildLevelGrid();
            if (_levelSelectOverlay != null)
            {
                _levelSelectOverlay.style.display = DisplayStyle.Flex;
            }
        }

        public void CloseScreen()
        {
            if (_levelSelectOverlay != null)
            {
                _levelSelectOverlay.style.display = DisplayStyle.None;
            }
        }
    }
}
