using UnityEngine;
using UnityEngine.UIElements;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.UI
{
    /// <summary>
    /// Deferred sandbox binder for IdleGameHUD.uxml. Idle slice play uses IdleSliceUIController.
    /// Resolves UIDocument only via IdleGameHudBinder.Active (never FindObjectOfType).
    /// UI3-05: binder stays unused on live Idle prefabs (instance count 0). Currency labels still
    /// read CurrentRunStats gold — do not revive without switching primary/prestige to IdleSliceState.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class IdleUIManagerSystem : SystemBase
    {
        private UIDocument _uiDocument;
        private Label _goldLabel;
        private Label _prestigeLabel;
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

        // Named handlers so rebind / stop can unsubscribe (UI2-05).
        private System.Action _onPrestigeClicked;
        private System.Action _onBuyUpgradeClicked;
        private System.Action _onUpgradesTabClicked;
        private System.Action _onCosmeticsTabClicked;
        private System.Action _onSkin0Clicked;
        private System.Action _onSkin1Clicked;
        private System.Action _onSkin2Clicked;
        private System.Action _onSkin3Clicked;

        protected override void OnStopRunning()
        {
            UnbindButtons();
        }

        protected override void OnUpdate()
        {
            var binder = IdleGameHudBinder.Active;
            if (binder == null || binder.Document == null)
            {
                UnbindButtons();
                return;
            }

            if (_uiDocument != binder.Document)
            {
                UnbindButtons();
                _uiDocument = binder.Document;
            }

            var root = _uiDocument.rootVisualElement;
            if (root == null)
                return;

            if (!_buttonBound)
            {
                BindOnce(root);
                // Only latch if this is actually IdleGameHUD (has PrestigeButton or GoldLabel).
                if (_goldLabel == null && _prestigeButton == null)
                {
                    UnbindButtons();
                    _uiDocument = null;
                    return;
                }

                _buttonBound = true;
            }

            // TODO: [STUB] Runner-shaped sandbox currency (UI3-05). While IdleGameHudBinder
            // instance count is 0 this path is dead. If revived, bind Gold/Prestige from
            // IdleSliceState.PrimaryCurrency / PrestigeCurrency (not CurrentRunStats alone).
            foreach (var runStats in SystemAPI.Query<RefRO<CurrentRunStats>>())
            {
                if (_goldLabel != null)
                    _goldLabel.text = $"Gold: {runStats.ValueRO.CurrentGold:N0}";
            }

            foreach (var persistentStats in SystemAPI.Query<RefRO<PersistentPlayerStats>>())
            {
                if (_prestigeLabel != null)
                    _prestigeLabel.text = $"Prestige: {persistentStats.ValueRO.PrestigeCurrency:N0}";
            }

            UpdateSkinButtonsUI();
        }

        private void BindOnce(VisualElement root)
        {
            UnbindButtons();

            _goldLabel = root.Q<Label>("GoldLabel");
            _prestigeLabel = root.Q<Label>("PrestigeLabel");

            _prestigeButton = root.Q<Button>("PrestigeButton");
            if (_prestigeButton != null)
            {
                _onPrestigeClicked = OnPrestigeButtonClicked;
                _prestigeButton.clicked += _onPrestigeClicked;
            }

            _buyUpgradeButton = root.Q<Button>("BuyUpgradeButton");
            if (_buyUpgradeButton != null)
            {
                _onBuyUpgradeClicked = OnBuyUpgradeButtonClicked;
                _buyUpgradeButton.clicked += _onBuyUpgradeClicked;
            }

            _upgradesTabButton = root.Q<Button>("UpgradesTabButton");
            _cosmeticsTabButton = root.Q<Button>("CosmeticsTabButton");
            _upgradesContainer = root.Q<VisualElement>("UpgradesContainer");
            _cosmeticsContainer = root.Q<VisualElement>("CosmeticsContainer");

            if (_upgradesTabButton != null && _cosmeticsTabButton != null &&
                _upgradesContainer != null && _cosmeticsContainer != null)
            {
                _onUpgradesTabClicked = OnUpgradesTabClicked;
                _onCosmeticsTabClicked = OnCosmeticsTabClicked;
                _upgradesTabButton.clicked += _onUpgradesTabClicked;
                _cosmeticsTabButton.clicked += _onCosmeticsTabClicked;
            }

            _equipSkin0Button = root.Q<Button>("EquipSkin0Button");
            if (_equipSkin0Button != null)
            {
                _onSkin0Clicked = () => OnSkinButtonClicked(0, 0.0);
                _equipSkin0Button.clicked += _onSkin0Clicked;
            }

            _buySkin1Button = root.Q<Button>("BuySkin1Button");
            if (_buySkin1Button != null)
            {
                _onSkin1Clicked = () => OnSkinButtonClicked(1, 5.0);
                _buySkin1Button.clicked += _onSkin1Clicked;
            }

            _buySkin2Button = root.Q<Button>("BuySkin2Button");
            if (_buySkin2Button != null)
            {
                _onSkin2Clicked = () => OnSkinButtonClicked(2, 15.0);
                _buySkin2Button.clicked += _onSkin2Clicked;
            }

            _buySkin3Button = root.Q<Button>("BuySkin3Button");
            if (_buySkin3Button != null)
            {
                _onSkin3Clicked = () => OnSkinButtonClicked(3, 30.0);
                _buySkin3Button.clicked += _onSkin3Clicked;
            }
        }

        private void UnbindButtons()
        {
            if (_prestigeButton != null && _onPrestigeClicked != null)
                _prestigeButton.clicked -= _onPrestigeClicked;
            if (_buyUpgradeButton != null && _onBuyUpgradeClicked != null)
                _buyUpgradeButton.clicked -= _onBuyUpgradeClicked;
            if (_upgradesTabButton != null && _onUpgradesTabClicked != null)
                _upgradesTabButton.clicked -= _onUpgradesTabClicked;
            if (_cosmeticsTabButton != null && _onCosmeticsTabClicked != null)
                _cosmeticsTabButton.clicked -= _onCosmeticsTabClicked;
            if (_equipSkin0Button != null && _onSkin0Clicked != null)
                _equipSkin0Button.clicked -= _onSkin0Clicked;
            if (_buySkin1Button != null && _onSkin1Clicked != null)
                _buySkin1Button.clicked -= _onSkin1Clicked;
            if (_buySkin2Button != null && _onSkin2Clicked != null)
                _buySkin2Button.clicked -= _onSkin2Clicked;
            if (_buySkin3Button != null && _onSkin3Clicked != null)
                _buySkin3Button.clicked -= _onSkin3Clicked;

            _onPrestigeClicked = null;
            _onBuyUpgradeClicked = null;
            _onUpgradesTabClicked = null;
            _onCosmeticsTabClicked = null;
            _onSkin0Clicked = null;
            _onSkin1Clicked = null;
            _onSkin2Clicked = null;
            _onSkin3Clicked = null;

            _prestigeButton = null;
            _buyUpgradeButton = null;
            _upgradesTabButton = null;
            _cosmeticsTabButton = null;
            _upgradesContainer = null;
            _cosmeticsContainer = null;
            _equipSkin0Button = null;
            _buySkin1Button = null;
            _buySkin2Button = null;
            _buySkin3Button = null;
            _goldLabel = null;
            _prestigeLabel = null;
            _uiDocument = null;
            _buttonBound = false;
        }

        private void OnUpgradesTabClicked()
        {
            if (_upgradesContainer != null) _upgradesContainer.style.display = DisplayStyle.Flex;
            if (_cosmeticsContainer != null) _cosmeticsContainer.style.display = DisplayStyle.None;
        }

        private void OnCosmeticsTabClicked()
        {
            if (_cosmeticsContainer != null) _cosmeticsContainer.style.display = DisplayStyle.Flex;
            if (_upgradesContainer != null) _upgradesContainer.style.display = DisplayStyle.None;
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
                btn.style.backgroundColor = new StyleColor(new Color(0.55f, 0.15f, 0.15f, 1f));
            }
        }

        private void OnPrestigeButtonClicked()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(entity, new PrestigeEventComponent
            {
                TargetSlice = ResolveSoleIdleSlice()
            });
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
                TargetSlice = ResolveSoleIdleSlice(),
                TargetSkinIndex = targetSkinIndex,
                PrestigeCost = cost
            });
        }

        private Entity ResolveSoleIdleSlice()
        {
            using var q = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<IdleSliceState>());
            if (q.CalculateEntityCount() == 1)
                return q.GetSingletonEntity();
            return Entity.Null;
        }
    }
}
