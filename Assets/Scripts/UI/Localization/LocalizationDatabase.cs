using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Localization
{
    /// <summary>
    /// ScriptableObject that holds all translation strings for every supported language.
    ///
    /// Create one via  Assets → Create → UIFramework → Localization Database.
    /// Assign it to LocalizationManager in the Inspector.
    /// Edit it comfortably via  Tools → UIFramework → Localization Editor.
    /// </summary>
    [CreateAssetMenu(menuName = "UIFramework/Localization Database", fileName = "LocalizationDatabase")]
    public class LocalizationDatabase : ScriptableObject
    {
        [Tooltip("One entry per supported language.")]
        public List<LanguageTable> languages = new();

        // ── Nested Types ─────────────────────────────────────────────────────────

        [System.Serializable]
        public class LanguageTable
        {
            [Tooltip("ISO 639-1 code, e.g. 'en', 'fr', 'de', 'ja'.")]
            public string languageCode;

            [Tooltip("Human-readable name shown in language picker UI.")]
            public string displayName;

            public List<Entry> entries = new();
        }

        [System.Serializable]
        public class Entry
        {
            public string key;
            [TextArea(1, 4)]
            public string value;
        }

        // ── Runtime Helpers ───────────────────────────────────────────────────────

        /// <summary>Returns the LanguageTable for a given code, or null if not found.</summary>
        public LanguageTable GetTable(string languageCode)
            => languages.Find(l => l.languageCode == languageCode);

        /// <summary>True if the database contains a table for the given language code.</summary>
        public bool HasLanguage(string languageCode)
            => languages.Exists(l => l.languageCode == languageCode);
    }
}
