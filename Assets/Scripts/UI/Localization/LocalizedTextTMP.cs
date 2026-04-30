using TMPro;
using UIFramework.Localization;
using UnityEngine;

namespace UIFramework.Components
{
    /// <summary>
    /// Attach to any GameObject that has a TextMeshProUGUI component.
    /// Set a localisation key in the Inspector; the text refreshes automatically
    /// when the active language changes.
    ///
    /// You can also use format arguments to inject dynamic values:
    ///   key: "ui.score"  →  database value: "Score: {0}"
    ///   SetArguments(playerScore)  →  "Score: 42"
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    [AddComponentMenu("UIFramework/Localized Text (TMP)")]
    public class LocalizedTextTMP : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────────
        [Tooltip("The localisation key to look up in the active language table.")]
        [SerializeField] private string key;

        // ── Runtime ───────────────────────────────────────────────────────────────
        private TextMeshProUGUI _text;
        private object[]        _formatArgs;

        // ── Properties ────────────────────────────────────────────────────────────
        public string Key
        {
            get => key;
            set { key = value; Refresh(); }
        }

        // ── Unity Lifecycle ───────────────────────────────────────────────────────
        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (LocalizationManager.IsInitialised)
                LocalizationManager.Instance.OnLanguageChanged += Refresh;

            Refresh();
        }

        private void OnDisable()
        {
            if (LocalizationManager.IsInitialised)
                LocalizationManager.Instance.OnLanguageChanged -= Refresh;
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Provide format arguments so the translated string can include dynamic data.
        /// Example: key value is "Lives: {0}" → SetArguments(3) → "Lives: 3"
        /// </summary>
        public void SetArguments(params object[] args)
        {
            _formatArgs = args;
            Refresh();
        }

        /// <summary>Force a manual refresh (normally automatic on language change).</summary>
        public void Refresh()
        {
            if (_text == null) return;
            if (!LocalizationManager.IsInitialised) return;

            var raw = LocalizationManager.Instance.Get(key);

            if (_formatArgs != null && _formatArgs.Length > 0)
            {
                try   { _text.text = string.Format(raw, _formatArgs); }
                catch { _text.text = raw; } // Safety: don't crash on malformed format string
            }
            else
            {
                _text.text = raw;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Preview in editor without entering Play mode when LocalizationManager is live
            if (!Application.isPlaying) return;
            Refresh();
        }
#endif
    }
}
