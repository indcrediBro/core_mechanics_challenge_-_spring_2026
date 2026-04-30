// ============================================================
//  SettingsArrowSelector.cs
//  Place anywhere in Assets/ (except an Editor/ folder).
//
//  Add to any GameObject. Wire two Buttons and a TMP_Text
//  in the Inspector — the script handles everything else.
//
//  INSPECTOR SETUP:
//    1. Pick Binding Type (Int or Bool)
//    2. Pick the matching Setting enum
//    3. Fill Options[] with display strings  OR  tick one
//       of the AutoPopulate flags (Quality, Difficulty, AA)
//    4. Wire LeftButton, RightButton, ValueLabel
//    5. Optional: tick WrapAround for circular navigation
//
//  EXAMPLE LAYOUTS:
//    Quality:     ◀  High  ▶
//    Difficulty:  ◀  Normal  ▶
//    AA:          ◀  4× MSAA  ▶
//    VSync:       ◀  On  ▶
// ============================================================

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Settings
{
    /// <summary>
    /// A ◀ Label ▶ arrow selector bound to an <see cref="IntSetting"/> or
    /// <see cref="BoolSetting"/> in <see cref="SettingsManager"/>.<br/>
    /// Matches the subscribe/unsubscribe and syncing pattern of all other binders.
    /// </summary>
    public class SettingsArrowSelector : MonoBehaviour
    {
        // ── Binding type ───────────────────────────────────────────────────

        public enum BindingType { Int, Bool }

        // ── Inspector ──────────────────────────────────────────────────────

        [Header("Binding")]
        [Tooltip("Int  = cycles through an ordered list of integer values.\n" +
                 "Bool = toggles between two states (On / Off).")]
        public BindingType Type = BindingType.Int;

        [Tooltip("Which IntSetting to bind. Only used when Type = Int.")]
        public IntSetting IntPath = IntSetting.Graphics_QualityLevel;

        [Tooltip("Which BoolSetting to bind. Only used when Type = Bool.")]
        public BoolSetting BoolPath = BoolSetting.Graphics_VSyncEnabled;

        [Header("Options")]
        [Tooltip("Display strings shown in the label for each value.\n\n" +
                 "For Int bindings the list index matches the stored integer value.\n" +
                 "  Index 0 → stored value 0, Index 1 → stored value 1, etc.\n\n" +
                 "For Bool bindings:\n" +
                 "  Index 0 → false  (shown when OFF)\n" +
                 "  Index 1 → true   (shown when ON)\n\n" +
                 "Leave empty and tick one AutoPopulate flag instead.")]
        public List<string> Options = new();

        [Tooltip("Auto-fill with QualitySettings.names.\n" +
                 "Use with IntPath = Graphics_QualityLevel.")]
        public bool AutoPopulateQualityLevels = false;

        [Tooltip("Auto-fill with Off / 2× MSAA / 4× MSAA / 8× MSAA.\n" +
                 "Use with IntPath = Graphics_AntiAliasingLevel.")]
        public bool AutoPopulateAntiAliasing = false;

        [Tooltip("Auto-fill with Easy / Normal / Hard.\n" +
                 "Use with IntPath = Gameplay_Difficulty.")]
        public bool AutoPopulateDifficulty = false;

        [Tooltip("Auto-fill with Off / On for Bool bindings.\n" +
                 "Ignored when Options has entries.")]
        public bool AutoPopulateBool = true;

        [Header("UI References")]
        [Tooltip("The ◀ button. Decrements the selection.")]
        public Button LeftButton;

        [Tooltip("The ▶ button. Increments the selection.")]
        public Button RightButton;

        [Tooltip("TMP_Text that displays the current option string.")]
        public TMP_Text ValueLabel;

        [Header("Behaviour")]
        [Tooltip("When true, pressing ▶ on the last option wraps back to the first,\n" +
                 "and pressing ◀ on the first wraps to the last.")]
        public bool WrapAround = true;

        // ── MSAA: dropdown index ↔ Unity sample count ──────────────────────
        private static readonly int[]    MsaaSampleCounts = { 0, 2, 4, 8 };
        private static readonly string[] MsaaLabels       = { "Off", "2×  MSAA", "4×  MSAA", "8×  MSAA" };

        // ── Runtime ────────────────────────────────────────────────────────

        private List<string> _resolvedOptions = new();
        private int          _currentIndex;
        private bool         _isSyncing;

        // ── Unity lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            ResolveOptions();
        }

        private void OnEnable()
        {
            LeftButton? .onClick.AddListener(OnLeftClicked);
            RightButton?.onClick.AddListener(OnRightClicked);

            SettingsManager.OnReverted       += PullFromManager;
            SettingsManager.OnPendingChanged += OnExternalChanged;

            PullFromManager();
        }

        private void OnDisable()
        {
            LeftButton? .onClick.RemoveListener(OnLeftClicked);
            RightButton?.onClick.RemoveListener(OnRightClicked);

            SettingsManager.OnReverted       -= PullFromManager;
            SettingsManager.OnPendingChanged -= OnExternalChanged;
        }

        // ── Button handlers ────────────────────────────────────────────────

        private void OnLeftClicked()  => Navigate(-1);
        private void OnRightClicked() => Navigate(+1);

        private void Navigate(int direction)
        {
            if (_resolvedOptions.Count == 0) return;

            int next = _currentIndex + direction;

            if (WrapAround)
            {
                next = (next % _resolvedOptions.Count + _resolvedOptions.Count)
                       % _resolvedOptions.Count;
            }
            else
            {
                next = Mathf.Clamp(next, 0, _resolvedOptions.Count - 1);
            }

            _currentIndex = next;
            PushToManager();
            UpdateLabel();
            UpdateArrowState();
        }

        // ── Sync: Manager → UI ─────────────────────────────────────────────

        private void PullFromManager()
        {
            _isSyncing = true;
            _currentIndex = IndexFromManager();
            UpdateLabel();
            UpdateArrowState();
            _isSyncing = false;
        }

        private void OnExternalChanged(object changed)
        {
            bool isOurs = Type == BindingType.Bool
                ? changed is BoolSetting b && b == BoolPath
                : changed is IntSetting  i && i == IntPath;

            if (isOurs) PullFromManager();
        }

        // ── Sync: UI → Manager ─────────────────────────────────────────────

        private void PushToManager()
        {
            if (_isSyncing) return;

            if (Type == BindingType.Bool)
            {
                // Index 0 = false, Index 1 = true
                SettingsManager.SetBool(BoolPath, _currentIndex == 1);
            }
            else
            {
                // For MSAA the stored value is a sample count, not the list index
                int storedValue = AutoPopulateAntiAliasing
                    ? (_currentIndex < MsaaSampleCounts.Length ? MsaaSampleCounts[_currentIndex] : 0)
                    : _currentIndex;

                SettingsManager.SetInt(IntPath, storedValue);
            }
        }

        // ── Read current index from manager ────────────────────────────────

        private int IndexFromManager()
        {
            if (Type == BindingType.Bool)
                return SettingsManager.GetBool(BoolPath) ? 1 : 0;

            int stored = SettingsManager.GetInt(IntPath);

            if (AutoPopulateAntiAliasing)
            {
                int idx = System.Array.IndexOf(MsaaSampleCounts, stored);
                return Mathf.Max(0, idx);
            }

            return Mathf.Clamp(stored, 0, Mathf.Max(0, _resolvedOptions.Count - 1));
        }

        // ── UI helpers ─────────────────────────────────────────────────────

        private void UpdateLabel()
        {
            if (ValueLabel == null) return;
            ValueLabel.text = (_resolvedOptions.Count > 0 && _currentIndex < _resolvedOptions.Count)
                ? _resolvedOptions[_currentIndex]
                : _currentIndex.ToString();
        }

        private void UpdateArrowState()
        {
            if (WrapAround) return; // arrows always active when wrapping

            if (LeftButton  != null) LeftButton.interactable  = _currentIndex > 0;
            if (RightButton != null) RightButton.interactable = _currentIndex < _resolvedOptions.Count - 1;
        }

        // ── Option population ──────────────────────────────────────────────

        private void ResolveOptions()
        {
            _resolvedOptions = new List<string>();

            if (AutoPopulateQualityLevels)
            {
                _resolvedOptions.AddRange(QualitySettings.names);
            }
            else if (AutoPopulateAntiAliasing)
            {
                _resolvedOptions.AddRange(MsaaLabels);
            }
            else if (AutoPopulateDifficulty)
            {
                _resolvedOptions.AddRange(new[] { "Easy", "Normal", "Hard" });
            }
            else if (Type == BindingType.Bool && Options.Count == 0 && AutoPopulateBool)
            {
                _resolvedOptions.AddRange(new[] { "Off", "On" });
            }
            else if (Options != null && Options.Count > 0)
            {
                _resolvedOptions.AddRange(Options);
            }
        }
    }
}
