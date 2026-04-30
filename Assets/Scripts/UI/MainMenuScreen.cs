using UIFramework.Components;
using UIFramework.Core;
using UIFramework.Localization;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Example UIScreen implementation — Main Menu.
///
/// Scene setup:
/// ┌─ Canvas
/// │  ├─ MainMenuScreen (this component + CanvasGroup)
/// │  │  ├─ PlayButton   (UIButton)
/// │  │  ├─ OptionsButton(UIButton)
/// │  │  └─ QuitButton   (UIButton)
///
/// Wire the UIButton.onClick events to the methods below in the Inspector,
/// OR assign them in OnInitialise as shown.
/// </summary>
public class MainMenuScreen : UIScreen
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Buttons")]
    [SerializeField] private UIButton playButton;
    [SerializeField] private UIButton optionsButton;
    [SerializeField] private UIButton leaderboardButton;
    [SerializeField] private UIButton quitButton;

    [Header("Screen IDs to navigate to")]
    [SerializeField] private string gameplayScreenId = "GameplayScreen";
    [SerializeField] private string optionsScreenId  = "OptionsScreen";
    [SerializeField] private string leaderboardScreenId  = "LeaderboardScreen";

    // ── UIScreen Overrides ────────────────────────────────────────────────────
    protected override void Awake()
    {
        base.Awake(); // Always call base.Awake() — it sets up the CanvasGroup

        // Wire button events from code (alternative to wiring them in the Inspector)
        if (playButton    != null) playButton.onClick.AddListener(OnPlayClicked);
        if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsClicked);
        if (leaderboardButton != null) leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        if (quitButton    != null) quitButton.onClick.AddListener(OnQuitClicked);
    }

    public override void OnShow()
    {
        base.OnShow();
        Debug.Log("[MainMenuScreen] Shown.");
    }

    public override void OnHide()
    {
        base.OnHide();
        Debug.Log("[MainMenuScreen] Hidden.");
    }

    public override void OnPause()
    {
        base.OnPause();
        // Optional: dim or disable interactive elements while another screen is on top
    }

    public override void OnResume()
    {
        base.OnResume();
        Debug.Log("[MainMenuScreen] Resumed (returned from another screen).");
    }

    public override void OnThemeApplied(UIThemeData theme)
    {
        // Forward theme to each button (UIScreen does NOT auto-collect buttons).
        // Add any screen-level colour changes here too (e.g. background image).
        if (playButton    != null) playButton.ApplyTheme(theme);
        if (optionsButton != null) optionsButton.ApplyTheme(theme);
        if (leaderboardButton    != null) leaderboardButton.ApplyTheme(theme);
        if (quitButton    != null) quitButton.ApplyTheme(theme);
    }

    // ── Button Handlers ───────────────────────────────────────────────────────
    private void OnPlayClicked()
    {
        Debug.Log("[MainMenuScreen] Play clicked.");
        UIManager.Instance.ShowScreen(gameplayScreenId);
    }

    private void OnOptionsClicked()
    {
        Debug.Log("[MainMenuScreen] Options clicked.");
        UIManager.Instance.ShowScreen(optionsScreenId);
    }

    private void OnLeaderboardClicked()
    {
        Debug.Log("[MainMenuScreen] Settings clicked.");
        UIManager.Instance.ShowScreen(leaderboardScreenId);
    }

    private void OnQuitClicked()
    {
        Debug.Log("[MainMenuScreen] Quit clicked.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
