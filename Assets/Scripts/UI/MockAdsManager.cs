using UnityEngine;
using UnityEngine.UIElements;

namespace HyperCasualRunner.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MockAdsManager : MonoBehaviour
    {
        private VisualElement _adPanel;
        private System.Action _onSuccessCallback;
        private float _adTimer = 0f;
        private bool _isPlayingAd = false;
        private Label _adLabel;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _adPanel = new VisualElement();
            _adPanel.style.position = Position.Absolute;
            _adPanel.style.top = 0; _adPanel.style.bottom = 0; _adPanel.style.left = 0; _adPanel.style.right = 0;
            _adPanel.style.backgroundColor = new StyleColor(Color.black);
            _adPanel.style.display = DisplayStyle.None;
            _adPanel.style.alignItems = Align.Center;
            _adPanel.style.justifyContent = Justify.Center;

            _adLabel = new Label("WATCHING SPONSORED AD...");
            _adLabel.style.fontSize = 48;
            _adLabel.style.color = Color.white;
            _adLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _adPanel.Add(_adLabel);

            root.Add(_adPanel);
        }

        public void ShowRewardedAd(System.Action onSuccess)
        {
            _onSuccessCallback = onSuccess;
            _adPanel.style.display = DisplayStyle.Flex;
            _isPlayingAd = true;
            _adTimer = 3.0f; // Mock 3 second ad
        }

        private void Update()
        {
            if (_isPlayingAd)
            {
                _adTimer -= Time.deltaTime;
                _adLabel.text = $"WATCHING SPONSORED AD...\n{Mathf.CeilToInt(_adTimer)}s";
                
                if (_adTimer <= 0)
                {
                    _isPlayingAd = false;
                    _adPanel.style.display = DisplayStyle.None;
                    _onSuccessCallback?.Invoke();
                }
            }
        }
    }
}
