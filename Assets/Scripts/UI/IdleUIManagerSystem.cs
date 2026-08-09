using UnityEngine;
using UnityEngine.UIElements;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.UI
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class IdleUIManagerSystem : SystemBase
    {
        private UIDocument _uiDocument;
        private Button _prestigeButton;
        private Button _buyUpgradeButton;
        private Button _upgradesTabButton;
        private Button _cosmeticsTabButton;
        private VisualElement _upgradesContainer;
        private VisualElement _cosmeticsContainer;
        private Button _equipSkin0Button;
        private Button _buySkin1Button;
        private Button _buySkin2Button;
        private Button _buySkin3Button;
        private bool _buttonBound;

        protected override void OnUpdate()
        {
            if (_uiDocument == null)
            {
                _uiDocument = Object.FindObjectOfType<UIDocument>();
                _buttonBound = false;
                if (_uiDocument == null)
                {
                    return;
                }
            }

            var root = _uiDocument.rootVisualElement;
            if (root == null)
            {
                return;
            }

            if (!_buttonBound)
            {
                _prestigeButton = root.Q<Button>("PrestigeButton");
                if (_prestigeButton != null)
                {
                    _prestigeButton.clicked += OnPrestigeButtonClicked;
                }
                
                _buyUpgradeButton = root.Q<Button>("BuyUpgradeButton");
                if (_buyUpgradeButton != null)
                {
                    _buyUpgradeButton.clicked += OnBuyUpgradeButtonClicked;
                }

                _upgradesTabButton = root.Q<Button>("UpgradesTabButton");
                _cosmeticsTabButton = root.Q<Button>("CosmeticsTabButton");
                _upgradesContainer = root.Q<VisualElement>("UpgradesContainer");
                _cosmeticsContainer = root.Q<VisualElement>("CosmeticsContainer");

                if (_upgradesTabButton != null && _cosmeticsTabButton != null && _upgradesContainer != null && _cosmeticsContainer != null)
                {
                    _upgradesTabButton.clicked += () =>
                    {
                        _upgradesContainer.style.display = DisplayStyle.Flex;
                        _cosmeticsContainer.style.display = DisplayStyle.None;
                    };

                    _cosmeticsTabButton.clicked += () =>
                    {
                        _cosmeticsContainer.style.display = DisplayStyle.Flex;
                        _upgradesContainer.style.display = DisplayStyle.None;
                    };
                }

                _equipSkin0Button = root.Q<Button>("EquipSkin0Button");
                if (_equipSkin0Button != null)
                {
                    _equipSkin0Button.clicked += () => OnSkinButtonClicked(0, 0.0);
                }

                _buySkin1Button = root.Q<Button>("BuySkin1Button");
                if (_buySkin1Button != null)
                {
                    _buySkin1Button.clicked += () => OnSkinButtonClicked(1, 5.0);
                }

                _buySkin2Button = root.Q<Button>("BuySkin2Button");
                if (_buySkin2Button != null)
                {
                    _buySkin2Button.clicked += () => OnSkinButtonClicked(2, 15.0);
                }

                _buySkin3Button = root.Q<Button>("BuySkin3Button");
                if (_buySkin3Button != null)
                {
                    _buySkin3Button.clicked += () => OnSkinButtonClicked(3, 30.0);
                }
                
                _buttonBound = true;
            }

            foreach (var runStats in SystemAPI.Query<RefRO<CurrentRunStats>>())
            {
                var goldLabel = root.Q<Label>("GoldLabel");
                if (goldLabel != null)
                {
                    goldLabel.text = $"Gold: {runStats.ValueRO.CurrentGold:N0}";
                }
            }

            foreach (var persistentStats in SystemAPI.Query<RefRO<PersistentPlayerStats>>())
            {
                var prestigeLabel = root.Q<Label>("PrestigeLabel");
                if (prestigeLabel != null)
                {
                    prestigeLabel.text = $"Prestige: {persistentStats.ValueRO.PrestigeCurrency:N0}";
                }
            }

            UpdateSkinButtonsUI();
        }

        private void UpdateSkinButtonsUI()
        {
            UpdateSkinButtonState(_equipSkin0Button, 0, 0.0);
            UpdateSkinButtonState(_buySkin1Button, 1, 5.0);
            UpdateSkinButtonState(_buySkin2Button, 2, 15.0);
            UpdateSkinButtonState(_buySkin3Button, 3, 30.0);
        }

        private void UpdateSkinButtonState(Button btn, int skinIndex, double cost)
        {
            if (btn == null) return;

            int currentEquipped = GameProgressData.CurrentSkinIndex;
            bool isUnlocked = GameProgressData.IsSkinUnlocked(skinIndex);

            if (currentEquipped == skinIndex)
            {
                btn.text = "EQUIPPED";
                btn.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));
            }
            else if (isUnlocked)
            {
                btn.text = "EQUIP";
                btn.style.backgroundColor = new StyleColor(new Color(0f, 0.48f, 1f, 1f));
            }
            else
            {
                btn.text = $"BUY ({cost:N0} P)";
            }
        }

        private void OnPrestigeButtonClicked()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(entity, new PrestigeEventComponent());
        }

        private void OnBuyUpgradeButtonClicked()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(entity, new ShopPurchaseEventComponent { Cost = 50.0, TargetProducerId = 1 });
        }

        private void OnSkinButtonClicked(int targetSkinIndex, double cost)
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(entity, new CosmeticPurchaseEventComponent
            {
                TargetSkinIndex = targetSkinIndex,
                PrestigeCost = cost
            });
        }
    }
}
