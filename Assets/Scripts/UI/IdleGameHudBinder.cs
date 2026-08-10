using UnityEngine;
using UnityEngine.UIElements;

namespace HyperCasualRunner.UI
{
    /// <summary>
    /// Explicit UIDocument authoring for the deferred IdleGameHUD sandbox path.
    /// Idle MVP slices use <see cref="IdleSliceUIController"/> instead.
    /// Attach only when a scene/prefab intentionally hosts IdleGameHUD.uxml
    /// AND currency labels have been switched to IdleSliceState (UI3-05).
    /// Registers itself as the active binder — no FindObjectOfType.
    /// Live Idle prefabs intentionally keep instance count at 0.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class IdleGameHudBinder : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        /// <summary>Active sandbox HUD binder, set on enable. Null when unused.</summary>
        public static IdleGameHudBinder Active { get; private set; }

        public UIDocument Document => _document != null ? _document : GetComponent<UIDocument>();

        private void Awake()
        {
            if (_document == null)
                _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (_document == null)
                _document = GetComponent<UIDocument>();
            Active = this;
        }

        private void OnDisable()
        {
            if (Active == this)
                Active = null;
        }
    }
}
