using System.Collections.Generic;
using UIFramework.Components;
using UIFramework.Core;
using UIFramework.Localization;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Settings Screen — audio volume controls, language switcher, and theme picker.
///
/// Scene hierarchy:
/// ┌─ SettingsScreen (this + CanvasGroup)
/// │  ├─ MasterVolumeSlider   (Slider)
/// │  ├─ MusicVolumeSlider    (Slider)
/// │  ├─ SFXVolumeSlider      (Slider)
/// │  ├─ LanguageButtons[]    (UIButton[]) — one per language in your database
/// │  ├─ ThemeButtons[]       (UIButton[]) — optional, one per UIThemeData
/// │  └─ BackButton           (UIButton)
///
/// PlayerPrefs keys used:
///   "vol_master"  float 0–1
///   "vol_music"   float 0–1
///   "vol_sfx"     float 0–1
///   "language"    string e.g. "en"
/// </summary>
public class SettingsScreen : UIScreen
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Audio")]
    [Tooltip("Optional AudioMixer to drive exposed parameters. Leave null to use AudioListener.volume for master.")]
    [SerializeField] private AudioMixer audioMixer;

    [Tooltip("Exposed parameter name in the AudioMixer for master volume.")]
    [SerializeField] private string mixerParamMaster = "VolMaster";

    [Tooltip("Exposed parameter name in the AudioMixer for music volume.")]
    [SerializeField] private string mixerParamMusic  = "VolMusic";

    [Tooltip("Exposed parameter name in the AudioMixer for SFX volume.")]
    [SerializeField] private string mixerParamSFX    = "VolSFX";

    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Language")]
    [Tooltip("One UIButton per language. Set each button's localizationKey to its display name key, " +
             "and languageCode to the matching ISO code (set via LanguageButton helper or code).")]
    [SerializeField] private List<LanguageButtonEntry> languageButtons = new();

    [Header("Themes")]
    [SerializeField] private List<ThemeButtonEntry> themeButtons = new();

    [Header("Navigation")]
    [SerializeField] private UIButton backButton;

    // ── PlayerPrefs Keys ──────────────────────────────────────────────────────
    private const string PrefMaster   = "vol_master";
    private const string PrefMusic    = "vol_music";
    private const string PrefSFX      = "vol_sfx";
    private const string PrefLanguage = "language";

    // ── Unity Lifecycle ───────────────────────────────────────────────────────
    protected override void Awake()
    {
        base.Awake();

        // Sliders
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (musicVolumeSlider  != null) musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider    != null) sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        // Language buttons
        foreach (var entry in languageButtons)
        {
            var code = entry.languageCode; // capture for lambda
            if (entry.button != null)
                entry.button.onClick.AddListener(() => OnLanguageSelected(code));
        }

        // Theme buttons
        foreach (var entry in themeButtons)
        {
            var theme = entry.theme; // capture for lambda
            if (entry.button != null)
                entry.button.onClick.AddListener(() => OnThemeSelected(theme));
        }

        // Back
        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
    }

    // ── UIScreen Overrides ────────────────────────────────────────────────────
    public override void OnShow()
    {
        base.OnShow();
        // LoadAndApplyPrefs();
    }

    public override void OnHide()
    {
        base.OnHide();
        // SavePrefs(); // Persist any unsaved changes when the screen closes
    }

    public override void OnThemeApplied(UIThemeData theme)
    {
        if (backButton != null) backButton.ApplyTheme(theme);

        foreach (var entry in languageButtons)
            if (entry.button != null) entry.button.ApplyTheme(theme);

        foreach (var entry in themeButtons)
            if (entry.button != null) entry.button.ApplyTheme(theme);
    }

    // ── Volume Handlers ───────────────────────────────────────────────────────

    /// <summary>Converts a 0–1 linear slider value to decibels and sets the mixer / fallback.</summary>
    private void OnMasterVolumeChanged(float value)
    {
        if (audioMixer != null)
            audioMixer.SetFloat(mixerParamMaster, LinearToDecibels(value));
        else
            AudioListener.volume = value;

        // PlayerPrefs.SetFloat(PrefMaster, value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (audioMixer != null)
            audioMixer.SetFloat(mixerParamMusic, LinearToDecibels(value));

        // PlayerPrefs.SetFloat(PrefMusic, value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (audioMixer != null)
            audioMixer.SetFloat(mixerParamSFX, LinearToDecibels(value));

        // PlayerPrefs.SetFloat(PrefSFX, value);
    }

    // ── Language Handler ──────────────────────────────────────────────────────
    private void OnLanguageSelected(string languageCode)
    {
        if (!LocalizationManager.IsInitialised) return;

        bool changed = LocalizationManager.Instance.SetLanguage(languageCode);
        if (changed)
        {
            // PlayerPrefs.SetString(PrefLanguage, languageCode);
            Debug.Log($"[SettingsScreen] Language changed to: {languageCode}");
        }
    }

    // ── Theme Handler ─────────────────────────────────────────────────────────
    private void OnThemeSelected(UIThemeData theme)
    {
        if (theme == null) return;
        UIManager.Instance.ApplyTheme(theme);
        Debug.Log($"[SettingsScreen] Theme changed to: {theme.name}");
    }

    // ── Navigation ────────────────────────────────────────────────────────────
    private void OnBackClicked()
    {
        // SavePrefs();
        UIManager.Instance.GoBack();
    }

    // ── Prefs Helpers ─────────────────────────────────────────────────────────

    /// <summary>Read saved preferences and update the sliders + audio without triggering saves.</summary>
    // private void LoadAndApplyPrefs()
    // {
    //     float master = PlayerPrefs.GetFloat(PrefMaster, 1f);
    //     float music  = PlayerPrefs.GetFloat(PrefMusic,  1f);
    //     float sfx    = PlayerPrefs.GetFloat(PrefSFX,    1f);
    //
    //     // Set slider values silently (the onValueChanged listeners will fire and apply audio)
    //     SetSliderSilent(masterVolumeSlider, master);
    //     SetSliderSilent(musicVolumeSlider,  music);
    //     SetSliderSilent(sfxVolumeSlider,    sfx);
    //
    //     // Apply audio directly in case sliders didn't change value (no delta = no event)
    //     OnMasterVolumeChanged(master);
    //     OnMusicVolumeChanged(music);
    //     OnSFXVolumeChanged(sfx);
    // }

    // private void SavePrefs()
    // {
    //     PlayerPrefs.Save();
    // }

    /// <summary>
    /// Assign a value to a slider without broadcasting its onValueChanged event.
    /// Useful for loading saved values without triggering side effects.
    /// </summary>
    private static void SetSliderSilent(Slider slider, float value)
    {
        if (slider == null) return;
        slider.SetValueWithoutNotify(Mathf.Clamp01(value));
    }

    /// <summary>
    /// Convert a linear 0–1 volume to decibels.
    /// Clamps to –80 dB at zero to avoid AudioMixer's -infinity warning.
    /// </summary>
    private static float LinearToDecibels(float linear)
        => linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;

    // ── Nested Types ──────────────────────────────────────────────────────────
    [System.Serializable]
    public class LanguageButtonEntry
    {
        [Tooltip("ISO 639-1 code, e.g. 'en', 'fr', 'de'.")]
        public string   languageCode;
        public UIButton button;
    }

    [System.Serializable]
    public class ThemeButtonEntry
    {
        public UIThemeData theme;
        public UIButton    button;
    }
}