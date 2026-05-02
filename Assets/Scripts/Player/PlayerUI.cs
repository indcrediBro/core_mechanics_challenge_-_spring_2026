using GameData;
using HealthSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives all in-game HUD panels using GameManager events and polled session data.
///
/// Panel wiring (assign in Inspector):
///   hudPanel        — shown while Playing
///   pausePanel      — shown while Paused
///   gameOverPanel   — shown on GameOver
/// </summary>
public class PlayerUI : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    // Panels
    // ─────────────────────────────────────────────────────────────

    [Header("Panels")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    // ─────────────────────────────────────────────────────────────
    // HUD — live play labels
    // ─────────────────────────────────────────────────────────────

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI scoreLabel;
    [SerializeField] private TextMeshProUGUI highScoreLabel;
    [SerializeField] private TextMeshProUGUI platformsLabel;
    [SerializeField] private TextMeshProUGUI killsLabel;
    [SerializeField] private TextMeshProUGUI playtimeLabel;
    [SerializeField] private TextMeshProUGUI weaponNameLabel;

    // ─────────────────────────────────────────────────────────────
    // Health
    // ─────────────────────────────────────────────────────────────

    [Header("Health")]
    [SerializeField] private Slider  healthBar;
    [SerializeField] private TextMeshProUGUI healthLabel; // optional — e.g. "75 / 100"

    // ─────────────────────────────────────────────────────────────
    // Game Over panel labels
    // ─────────────────────────────────────────────────────────────

    [Header("Game Over")]
    [SerializeField] private TextMeshProUGUI goFinalScoreLabel;
    [SerializeField] private TextMeshProUGUI goPlatformsLabel;
    [SerializeField] private TextMeshProUGUI goKillsLabel;
    [SerializeField] private TextMeshProUGUI goPlaytimeLabel;
    [SerializeField] private TextMeshProUGUI goHighScoreLabel;

    // ─────────────────────────────────────────────────────────────
    // Internal
    // ─────────────────────────────────────────────────────────────

    private GameSessionData _data;
    private bool _isPlaying;

    // ─────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        _data = GameManager.Instance.GameData;
    }

    private void OnEnable()
    {
        GameManager.OnStateChanged    += HandleStateChanged;
        GameManager.OnScoreChanged    += HandleScoreChanged;
        GameManager.OnWeaponSwitched  += HandleWeaponSwitched;
        PlayerHealth.OnHealthChanged  += HandleHealthChanged;
        PlayerHealth.OnPlayerDied     += HandlePlayerDied;
    }

    private void OnDisable()
    {
        GameManager.OnStateChanged    -= HandleStateChanged;
        GameManager.OnScoreChanged    -= HandleScoreChanged;
        GameManager.OnWeaponSwitched  -= HandleWeaponSwitched;
        PlayerHealth.OnHealthChanged  -= HandleHealthChanged;
        PlayerHealth.OnPlayerDied     -= HandlePlayerDied;
    }

    private void Start()
    {
        RefreshPanels(GameManager.Instance.CurrentState);
        RefreshHUD();
    }

    /// <summary>Polls playtime every frame — avoids event overhead for a value that changes every frame.</summary>
    private void Update()
    {
        if (!_isPlaying) return;
        SetText(playtimeLabel, FormatTime(_data.PlayTime));
    }

    // ─────────────────────────────────────────────────────────────
    // Event handlers
    // ─────────────────────────────────────────────────────────────

    private void HandleStateChanged(GameState newState)
    {
        RefreshPanels(newState);

        if (newState == GameState.GameOver)
            PopulateGameOverPanel();
    }

    private void HandleScoreChanged(int currentScore)
    {
        SetText(scoreLabel,     $"{currentScore}");
        SetText(platformsLabel, $"{_data.PlatformsBounced}");
        SetText(killsLabel,     $"{_data.EnemiesKilled}");
    }

    private void HandleWeaponSwitched(string gunName)
    {
        SetText(weaponNameLabel, gunName);
    }

    private void HandleHealthChanged(float normalizedHealth)
    {
        if (healthBar  != null) healthBar.value = normalizedHealth;

        // Label shows e.g. "75 / 100" — derive current HP from normalized ratio
        if (healthLabel != null)
        {
            // PlayerHealth.StartingHealth is the source of truth;
            // we reconstruct the integer for display only
            int max     = FindFirstObjectByType<PlayerHealth>()?.StartingHealth ?? 100;
            int current = Mathf.RoundToInt(normalizedHealth * max);
            SetText(healthLabel, $"{current} / {max}");
        }
    }

    private void HandlePlayerDied()
    {
        GameManager.Instance.GameOver();
    }

    // ─────────────────────────────────────────────────────────────
    // Panel management
    // ─────────────────────────────────────────────────────────────

    private void RefreshPanels(GameState state)
    {
        _isPlaying = state == GameState.Playing;

        SetActive(hudPanel,      state == GameState.Playing);
        SetActive(pausePanel,    state == GameState.Paused);
        SetActive(gameOverPanel, state == GameState.GameOver);
    }

    // ─────────────────────────────────────────────────────────────
    // HUD population
    // ─────────────────────────────────────────────────────────────

    private void RefreshHUD()
    {
        SetText(scoreLabel,      "0");
        SetText(highScoreLabel,  $"BEST  {_data.HighScore}");
        SetText(platformsLabel,  "0");
        SetText(killsLabel,      "0");
        SetText(playtimeLabel,   FormatTime(0));
        SetText(weaponNameLabel, "—");

        // Health bar starts full
        if (healthBar   != null) healthBar.value = 1f;
        SetText(healthLabel, "100 / 100");
    }

    // ─────────────────────────────────────────────────────────────
    // Game Over population
    // ─────────────────────────────────────────────────────────────

    private void PopulateGameOverPanel()
    {
        SetText(goFinalScoreLabel, $"{_data.FinalScore}");
        SetText(goPlatformsLabel,  $"Platforms  ×{_data.PlatformsBounced}   +{_data.PlatformsBounced * GameDefaultData.ScorePerPlatform}");
        SetText(goKillsLabel,      $"Kills      ×{_data.EnemiesKilled}   +{_data.EnemiesKilled * GameDefaultData.ScorePerKill}");
        SetText(goPlaytimeLabel,   $"Survived   {FormatTime(_data.PlayTime)}   +{Mathf.FloorToInt(_data.PlayTime)}");
        SetText(goHighScoreLabel,  _data.FinalScore >= _data.HighScore ? "NEW BEST!" : $"BEST  {_data.HighScore}");
    }

    // ─────────────────────────────────────────────────────────────
    // Utilities
    // ─────────────────────────────────────────────────────────────

    private static void SetText(TextMeshProUGUI label, string text)
    {
        if (label != null) label.text = text;
    }

    private static void SetActive(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }

    private static string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }
}