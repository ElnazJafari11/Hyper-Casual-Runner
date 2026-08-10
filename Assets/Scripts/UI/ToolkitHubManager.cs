using UnityEngine;
using UnityEngine.UIElements;
using Unity.Entities;

namespace HyperCasualRunner.UI
{
    public class ToolkitHubManager : MonoBehaviour
    {
        public GameObject[] LevelPrefabs;
        
        private VisualElement _hubPanel;
        private GameObject _currentLevelInstance;
        private readonly System.Collections.Generic.List<System.Action> _skinButtonRefreshers =
            new System.Collections.Generic.List<System.Action>();

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            
            _hubPanel = new VisualElement();
            _hubPanel.style.position = Position.Absolute;
            _hubPanel.style.top = 0; _hubPanel.style.bottom = 0; _hubPanel.style.left = 0; _hubPanel.style.right = 0;
            _hubPanel.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.15f, 1f));
            
            // 1. Title
            var title = new Label($"Hyper-Casual Toolkit 1.0  |  Gold: {GameProgressData.TotalGold}");
            title.style.fontSize = 48;
            title.style.color = Color.yellow;
            title.style.alignSelf = Align.Center;
            title.style.marginTop = 40;
            _hubPanel.Add(title);

            // Container for split screen
            var splitContainer = new VisualElement();
            splitContainer.style.flexDirection = FlexDirection.Row;
            splitContainer.style.flexGrow = 1;
            _hubPanel.Add(splitContainer);

            // 2. Level Select (Left Side)
            var scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;
            scrollView.style.marginTop = 20;
            scrollView.style.paddingLeft = 50;
            scrollView.style.paddingRight = 50;
            splitContainer.Add(scrollView);

            foreach (var prefab in LevelPrefabs)
            {
                if (prefab == null) continue;
                var btn = new Button(() => LoadLevel(prefab)) { text = prefab.name.Replace("_Slice", "").Replace("_", " ") };
                btn.style.fontSize = 24;
                btn.style.height = 50;
                btn.style.marginBottom = 10;
                scrollView.Add(btn);
            }

            // 3. Shop UI (Right Side)
            var shopContainer = new VisualElement();
            shopContainer.style.width = 400;
            shopContainer.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.25f, 1f));
            shopContainer.style.paddingTop = 20;
            shopContainer.style.alignItems = Align.Center;
            splitContainer.Add(shopContainer);

            // TODO: [STUB] Hub Skin Shop is gold-runner hub chrome only — NOT a second unlock economy.
            // Shared GameProgressData skin unlock / CurrentSkinIndex is owned by idle Cosmetics
            // (CosmeticPurchaseEventComponent + prestige). Hub may equip already-unlocked skins only;
            // locked skins point players to Idle Cosmetics (no gold UnlockSkin).
            // Hub equip list is skins 0–2 only; Emerald Neon (index 3) is Idle Cosmetics–only
            // (intentional prototype asymmetry — not a second unlock path).
            var shopTitle = new Label("Skin Shop");
            shopTitle.style.fontSize = 36;
            shopTitle.style.color = Color.white;
            shopContainer.Add(shopTitle);

            var shopHint = new Label("Unlock skins via Idle Cosmetics (prestige). Hub equips unlocked only.");
            shopHint.style.fontSize = 14;
            shopHint.style.color = new Color(0.75f, 0.75f, 0.8f);
            shopHint.style.whiteSpace = WhiteSpace.Normal;
            shopHint.style.marginBottom = 8;
            shopHint.style.unityTextAlign = TextAnchor.MiddleCenter;
            shopContainer.Add(shopHint);

            _skinButtonRefreshers.Clear();
            AddShopItem(shopContainer, 0, "Default Blue", shopTitle);
            AddShopItem(shopContainer, 1, "Crimson Red", shopTitle);
            AddShopItem(shopContainer, 2, "Solid Gold", shopTitle);

            root.Add(_hubPanel);

            // 4. Daily Reward Modal
            int pendingReward = GameProgressData.CheckDailyReward();
            if (pendingReward > 0)
            {
                var dailyModal = new VisualElement();
                dailyModal.style.position = Position.Absolute;
                dailyModal.style.top = 0; dailyModal.style.bottom = 0; dailyModal.style.left = 0; dailyModal.style.right = 0;
                dailyModal.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.9f));
                dailyModal.style.alignItems = Align.Center;
                dailyModal.style.justifyContent = Justify.Center;

                var dailyTitle = new Label("DAILY REWARD");
                dailyTitle.style.fontSize = 64;
                dailyTitle.style.color = Color.yellow;
                dailyTitle.style.marginBottom = 20;
                dailyModal.Add(dailyTitle);

                var dailyInfo = new Label($"Day {GameProgressData.LoginStreak + 1} Streak Bonus!\n+{pendingReward} GOLD");
                dailyInfo.style.fontSize = 32;
                dailyInfo.style.color = Color.white;
                dailyInfo.style.unityTextAlign = TextAnchor.MiddleCenter;
                dailyInfo.style.marginBottom = 40;
                dailyModal.Add(dailyInfo);

                var claimBtn = new Button(() => 
                {
                    GameProgressData.ClaimDailyReward();
                    title.text = $"Hyper-Casual Toolkit 1.0  |  Gold: {GameProgressData.TotalGold}";
                    dailyModal.style.display = DisplayStyle.None;
                }) { text = "CLAIM" };
                claimBtn.style.fontSize = 36;
                claimBtn.style.height = 80;
                claimBtn.style.width = 300;
                claimBtn.style.backgroundColor = new StyleColor(Color.green);
                dailyModal.Add(claimBtn);

                root.Add(dailyModal);
            }
        }

        private void AddShopItem(VisualElement container, int index, string name, Label titleRef)
        {
            var btn = new Button();
            btn.style.fontSize = 24;
            btn.style.height = 60;
            btn.style.width = 300;
            btn.style.marginTop = 20;

            System.Action updateBtnUI = null;
            updateBtnUI = () =>
            {
                bool isUnlocked = GameProgressData.IsSkinUnlocked(index);
                bool isEquipped = GameProgressData.CurrentSkinIndex == index;

                if (isEquipped) btn.text = $"{name} (EQUIPPED)";
                else if (isUnlocked) btn.text = $"{name} (EQUIP)";
                else btn.text = $"{name} (Idle Cosmetics)";

                btn.style.backgroundColor = isEquipped ? new StyleColor(Color.green) : new StyleColor(Color.gray);
            };

            btn.clicked += () =>
            {
                // Equip-only: never gold-unlock shared skins (avoids hub vs prestige dual economy).
                if (GameProgressData.IsSkinUnlocked(index))
                    GameProgressData.CurrentSkinIndex = index;

                titleRef.text = $"Hyper-Casual Toolkit 1.0  |  Gold: {GameProgressData.TotalGold}";
                RefreshAllSkinButtons();
                ReturnToHub();
            };

            updateBtnUI();
            _skinButtonRefreshers.Add(updateBtnUI);
            container.Add(btn);
        }

        private void RefreshAllSkinButtons()
        {
            for (int i = 0; i < _skinButtonRefreshers.Count; i++)
                _skinButtonRefreshers[i]?.Invoke();
        }

        private void LoadLevel(GameObject prefab)
        {
            _hubPanel.style.display = DisplayStyle.None;
            if (_currentLevelInstance != null) Destroy(_currentLevelInstance);
            
            _currentLevelInstance = Instantiate(prefab);
        }

        public void ReturnToHub()
        {
            if (_currentLevelInstance != null) Destroy(_currentLevelInstance);
            
            // Clean up any lingering ECS entities from the level!
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var allEntities = em.GetAllEntities(Unity.Collections.Allocator.Temp);
            em.DestroyEntity(allEntities);
            allEntities.Dispose();
            
            _hubPanel.style.display = DisplayStyle.Flex;
        }
    }
}
