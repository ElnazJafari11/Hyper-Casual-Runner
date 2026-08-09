using UnityEngine;
using UnityEngine.UIElements;
using Unity.Entities;
using HyperCasualRunner.ECS.Components;

namespace HyperCasualRunner.UI
{
    public class UIManager : MonoBehaviour
    {
        private Label goldLabel;
        private Label distanceLabel;
        private Button upgradeDamageBtn;
        private Button upgradeGoldBtn;
        private Button prestigeBtn;
        private VisualElement prestigeOverlay;

        private EntityManager entityManager;
        private EntityQuery currentStatsQuery;

        void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            
            goldLabel = root.Q<Label>("GoldLabel");
            distanceLabel = root.Q<Label>("DistanceLabel");
            upgradeDamageBtn = root.Q<Button>("UpgradeDamageButton");
            upgradeGoldBtn = root.Q<Button>("UpgradeGoldButton");
            prestigeBtn = root.Q<Button>("PrestigeButton");
            prestigeOverlay = root.Q<VisualElement>("PrestigeOverlay");

            upgradeDamageBtn.clicked += OnUpgradeDamage;
            upgradeGoldBtn.clicked += OnUpgradeGold;
            prestigeBtn.clicked += OnPrestige;

            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            currentStatsQuery = entityManager.CreateEntityQuery(typeof(CurrentRunStats));
        }

        void Update()
        {
            // Sync UI with ECS Data
            if (currentStatsQuery.CalculateEntityCount() > 0)
            {
                var stats = currentStatsQuery.GetSingleton<CurrentRunStats>();
                goldLabel.text = $"Gold: {stats.CurrentGold:F0}";
                distanceLabel.text = $"Distance: {stats.CurrentDistance}m";
            }
        }

        private void OnUpgradeDamage()
        {
            // Event to trigger damage upgrade in DOTS
            Debug.Log("Damage Upgrade Requested");
        }

        private void OnUpgradeGold()
        {
            // Event to trigger gold upgrade in DOTS
            Debug.Log("Gold Upgrade Requested");
        }

        private void OnPrestige()
        {
            // Reset distance and award permanent currency
            Debug.Log("Prestige Activated!");
            prestigeOverlay.style.display = DisplayStyle.None;
        }

        public void ShowPrestigeScreen()
        {
            prestigeOverlay.style.display = DisplayStyle.Flex;
        }
    }
}
