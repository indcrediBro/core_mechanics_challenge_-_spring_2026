using System;
using System.Collections.Generic;
using UIFramework.Core;
using UnityEngine;

namespace UIFramework.Localization
{
    /// <summary>
    /// Manages the active language and provides translated strings.
    ///
    /// Setup
    /// ─────
    /// 1. Add LocalizationManager to your bootstrap/persistent scene.
    /// 2. Assign a LocalizationDatabase ScriptableObject in the Inspector.
    /// 3. Set defaultLanguage (e.g. "en").
    ///
    /// Usage
    /// ─────
    ///   string text = LocalizationManager.Instance.Get("ui.play_button");
    ///   LocalizationManager.Instance.SetLanguage("fr");
    ///
    /// Subscribing to language changes
    /// ────────────────────────────────
    ///   LocalizationManager.Instance.OnLanguageChanged += MyRefreshMethod;
    ///
    /// UIButton and LocalizedTextTMP subscribe automatically.
    /// </summary>
    public class LocalizationManager : Singleton<LocalizationManager>
    {
        // ── Inspector ─────────────────────────────────────────────────────────────
        [Header("Database")]
        [Tooltip("Assign the LocalizationDatabase ScriptableObject here.")]
        [SerializeField] private LocalizationDatabase database;

        [Header("Language")]
        [Tooltip("Language code loaded on startup, e.g. 'en'.")]
        [SerializeField] private string defaultLanguage = "en";

        [Tooltip("Fallback language if a key is missing in the active language.")]
        [SerializeField] private string fallbackLanguage = "en";

        // ── Runtime ───────────────────────────────────────────────────────────────
        private Dictionary<string, string> _activeTable   = new();
        private Dictionary<string, string> _fallbackTable = new();
        private string                     _currentLanguage;

        // ── Events ────────────────────────────────────────────────────────────────
        /// <summary>
        /// Fired after a new language is loaded. Subscribe to refresh any text
        /// that is NOT using LocalizedTextTMP or UIButton's localizationKey.
        /// </summary>
        public event Action OnLanguageChanged;

        // ── Properties ────────────────────────────────────────────────────────────
        public string CurrentLanguage => _currentLanguage;

        /// <summary>All language codes present in the database.</summary>
        public IEnumerable<string> AvailableLanguages
        {
            get
            {
                if (database == null) yield break;
                foreach (var t in database.languages)
                    yield return t.languageCode;
            }
        }

        // ── Unity Lifecycle ───────────────────────────────────────────────────────
        protected override void Awake()
        {
            base.Awake(); // Singleton setup

            if (database == null)
            {
                Debug.LogError("[LocalizationManager] No LocalizationDatabase assigned.");
                return;
            }

            // Load fallback first so missing keys always resolve
            LoadTableInto(fallbackLanguage, _fallbackTable);
            LoadTableInto(defaultLanguage,  _activeTable);
            _currentLanguage = defaultLanguage;
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Switch to a different language. Fires OnLanguageChanged when done.
        /// Returns false if the language code is not in the database.
        /// </summary>
        public bool SetLanguage(string languageCode)
        {
            if (database == null)
            {
                Debug.LogWarning("[LocalizationManager] No database assigned.");
                return false;
            }

            if (!database.HasLanguage(languageCode))
            {
                Debug.LogWarning($"[LocalizationManager] Language '{languageCode}' not found in database.");
                return false;
            }

            LoadTableInto(languageCode, _activeTable);
            _currentLanguage = languageCode;

            OnLanguageChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Returns the localised string for the given key in the active language.
        /// Falls back to the fallback language, then returns the raw key if still missing.
        /// </summary>
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;

            if (_activeTable.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
                return value;

            if (_fallbackTable.TryGetValue(key, out var fallback) && !string.IsNullOrEmpty(fallback))
            {
                Debug.LogWarning($"[LocalizationManager] Key '{key}' missing in '{_currentLanguage}'. Using fallback.");
                return fallback;
            }

            Debug.LogWarning($"[LocalizationManager] Key '{key}' not found in any language table.");
            return key; // Return raw key as last resort — easy to spot during development
        }

        /// <summary>
        /// Returns true and the value if the key exists in the active language.
        /// </summary>
        public bool TryGet(string key, out string value)
        {
            if (_activeTable.TryGetValue(key, out value) && !string.IsNullOrEmpty(value))
                return true;

            value = key;
            return false;
        }

        // ── Internal ─────────────────────────────────────────────────────────────
        private void LoadTableInto(string languageCode, Dictionary<string, string> target)
        {
            target.Clear();

            var table = database.GetTable(languageCode);
            if (table == null)
            {
                Debug.LogWarning($"[LocalizationManager] No table found for '{languageCode}'.");
                return;
            }

            foreach (var entry in table.entries)
            {
                if (!string.IsNullOrEmpty(entry.key))
                    target[entry.key] = entry.value;
            }
        }
    }
}
