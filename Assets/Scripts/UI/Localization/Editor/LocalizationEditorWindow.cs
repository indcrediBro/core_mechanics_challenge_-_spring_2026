#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UIFramework.Localization;
using UnityEditor;
using UnityEngine;

namespace UIFramework.Editor
{
    /// <summary>
    /// Editor window for managing a LocalizationDatabase asset.
    /// Open via  Tools → UIFramework → Localization Editor.
    ///
    /// Features
    /// ────────
    /// • Add / remove / rename localisation keys.
    /// • Add / remove languages.
    /// • Search / filter keys.
    /// • Create a new database asset from within the window.
    /// • Auto-saves dirty state on every change; explicit Save button flushes to disk.
    /// </summary>
    public class LocalizationEditorWindow : EditorWindow
    {
        // ── State ─────────────────────────────────────────────────────────────────
        private LocalizationDatabase _database;
        private Vector2              _scroll;
        private string               _newKey      = "";
        private string               _newLanguage = "";
        private string               _search      = "";

        // Column widths
        private const float KeyColumnWidth  = 220f;
        private const float ValColumnWidth  = 200f;
        private const float DeleteBtnWidth  =  60f;
        private const float RemoveLangWidth =  22f;

        // ── Menu Item ─────────────────────────────────────────────────────────────
        [MenuItem("Tools/UIFramework/Localization Editor")]
        public static void Open() => GetWindow<LocalizationEditorWindow>("Localization Editor");

        // ── GUI ───────────────────────────────────────────────────────────────────
        private void OnGUI()
        {
            DrawToolbar();

            if (_database == null)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(
                    "Assign an existing LocalizationDatabase or create a new one using the toolbar above.",
                    MessageType.Info);
                return;
            }

            EditorGUILayout.Space(4);
            DrawSearchBar();
            EditorGUILayout.Space(4);
            DrawTable();
            EditorGUILayout.Space(8);
            DrawAddControls();

            if (GUI.changed)
                EditorUtility.SetDirty(_database);
        }

        // ── Toolbar ───────────────────────────────────────────────────────────────
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var next = (LocalizationDatabase)EditorGUILayout.ObjectField(
                _database, typeof(LocalizationDatabase), false, GUILayout.Width(260));

            if (next != _database)
                _database = next;

            if (GUILayout.Button("Create New", EditorStyles.toolbarButton, GUILayout.Width(90)))
                CreateNewDatabase();

            GUILayout.FlexibleSpace();

            if (_database != null &&
                GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                EditorUtility.SetDirty(_database);
                AssetDatabase.SaveAssets();
                Debug.Log("[LocalizationEditor] Saved.");
            }

            EditorGUILayout.EndHorizontal();
        }

        // ── Search ────────────────────────────────────────────────────────────────
        private void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search", GUILayout.Width(50));
            _search = EditorGUILayout.TextField(_search);
            if (GUILayout.Button("✕", GUILayout.Width(22)))
                _search = "";
            EditorGUILayout.EndHorizontal();
        }

        // ── Table ─────────────────────────────────────────────────────────────────
        private void DrawTable()
        {
            var allKeys = CollectAllKeys();

            if (!string.IsNullOrEmpty(_search))
                allKeys = allKeys
                    .Where(k => k.IndexOf(_search, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

            _scroll = EditorGUILayout.BeginScrollView(_scroll,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            DrawTableHeader();

            // Draw each row; break immediately if a structural change occurred
            // (remove key / rename) to avoid iterating a modified collection.
            bool dirty = false;
            foreach (var key in allKeys)
            {
                if (DrawRow(key, out bool removed))
                {
                    dirty = true;
                    if (removed) break;
                }
            }

            EditorGUILayout.EndScrollView();

            if (dirty)
                EditorUtility.SetDirty(_database);
        }

        private void DrawTableHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Key", EditorStyles.boldLabel, GUILayout.Width(KeyColumnWidth));

            foreach (var lang in _database.languages.ToList())
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Width(ValColumnWidth + RemoveLangWidth + 4));

                string newCode = EditorGUILayout.TextField(lang.languageCode,
                    GUILayout.Width(ValColumnWidth - RemoveLangWidth - 4));

                if (newCode != lang.languageCode)
                    lang.languageCode = newCode;

                if (GUILayout.Button("✕", GUILayout.Width(RemoveLangWidth)))
                {
                    if (EditorUtility.DisplayDialog("Remove Language",
                            $"Remove language '{lang.languageCode}' and all its translations?",
                            "Remove", "Cancel"))
                    {
                        _database.languages.Remove(lang);
                        EditorUtility.SetDirty(_database);
                        break;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Label("", GUILayout.Width(DeleteBtnWidth)); // spacer for delete column
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>Returns true if the row changed, sets removed=true if the key was deleted.</summary>
        private bool DrawRow(string key, out bool removed)
        {
            removed = false;
            bool changed = false;

            EditorGUILayout.BeginHorizontal();

            // ── Key field ──────────────────────────────────────────────────────
            string newKeyName = EditorGUILayout.TextField(key, GUILayout.Width(KeyColumnWidth));
            if (newKeyName != key && !string.IsNullOrWhiteSpace(newKeyName))
            {
                RenameKey(key, newKeyName);
                changed = true;
            }

            // ── Per-language value fields ──────────────────────────────────────
            foreach (var lang in _database.languages)
            {
                var entry = lang.entries.Find(e => e.key == key);
                if (entry == null)
                {
                    entry = new LocalizationDatabase.Entry { key = key, value = "" };
                    lang.entries.Add(entry);
                    changed = true;
                }

                string newVal = EditorGUILayout.TextField(entry.value, GUILayout.Width(ValColumnWidth));
                if (newVal != entry.value)
                {
                    entry.value = newVal;
                    changed     = true;
                }
            }

            // ── Delete button ─────────────────────────────────────────────────
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Delete", GUILayout.Width(DeleteBtnWidth)))
            {
                RemoveKey(key);
                removed = true;
                changed = true;
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
            return changed;
        }

        // ── Add Controls ──────────────────────────────────────────────────────────
        private void DrawAddControls()
        {
            EditorGUILayout.LabelField("Add New", EditorStyles.boldLabel);

            // Add key row
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Key", GUILayout.Width(80));
            _newKey = EditorGUILayout.TextField(_newKey);
            GUI.enabled = !string.IsNullOrWhiteSpace(_newKey);
            if (GUILayout.Button("Add Key", GUILayout.Width(90)))
            {
                AddKey(_newKey.Trim());
                _newKey = "";
                GUI.FocusControl(null);
                EditorUtility.SetDirty(_database);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            // Add language row
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Language", GUILayout.Width(80));
            _newLanguage = EditorGUILayout.TextField(_newLanguage);
            GUI.enabled = !string.IsNullOrWhiteSpace(_newLanguage);
            if (GUILayout.Button("Add Language", GUILayout.Width(110)))
            {
                AddLanguage(_newLanguage.Trim());
                _newLanguage = "";
                GUI.FocusControl(null);
                EditorUtility.SetDirty(_database);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        // ── Data Operations ───────────────────────────────────────────────────────
        private List<string> CollectAllKeys()
        {
            var keys = new HashSet<string>();
            foreach (var lang in _database.languages)
                foreach (var entry in lang.entries)
                    if (!string.IsNullOrEmpty(entry.key))
                        keys.Add(entry.key);
            return keys.OrderBy(k => k).ToList();
        }

        private void AddKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            foreach (var lang in _database.languages)
                if (!lang.entries.Any(e => e.key == key))
                    lang.entries.Add(new LocalizationDatabase.Entry { key = key, value = "" });
        }

        private void RemoveKey(string key)
        {
            foreach (var lang in _database.languages)
                lang.entries.RemoveAll(e => e.key == key);
        }

        private void RenameKey(string oldKey, string newKey)
        {
            if (string.IsNullOrEmpty(newKey)) return;
            foreach (var lang in _database.languages)
            {
                var entry = lang.entries.Find(e => e.key == oldKey);
                if (entry != null) entry.key = newKey;
            }
        }

        private void AddLanguage(string langCode)
        {
            if (string.IsNullOrEmpty(langCode)) return;
            if (_database.languages.Any(l => l.languageCode == langCode))
            {
                Debug.LogWarning($"[LocalizationEditor] Language '{langCode}' already exists.");
                return;
            }

            var keys    = CollectAllKeys();
            var newLang = new LocalizationDatabase.LanguageTable
            {
                languageCode = langCode,
                displayName  = langCode,
                entries      = keys.Select(k => new LocalizationDatabase.Entry
                {
                    key = k, value = ""
                }).ToList(),
            };
            _database.languages.Add(newLang);
        }

        private void CreateNewDatabase()
        {
            var asset = ScriptableObject.CreateInstance<LocalizationDatabase>();
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Localization Database", "LocalizationDatabase", "asset", "Choose location");

            if (string.IsNullOrEmpty(path)) return;

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            _database = asset;
        }
    }
}
#endif
