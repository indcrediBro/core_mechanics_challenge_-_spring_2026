using System.Collections.Generic;
using GameData;
using Leadr;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles all Leadr SDK calls and drives the leaderboard panel UI.
///
/// Setup:
///   1. Create a LeadrSettings asset (right-click > Create > LEADR > Settings),
///      fill in your GameId, then assign the asset in the LEADR SDK's LeadrClient.
///   2. Set boardId to your board's "brd_..." slug from the LEADR dashboard.
///   3. Wire all Inspector references below.
///   4. Add this component to a persistent GameObject (e.g. alongside GameManager).
///
/// Flow:
///   - Game Over  → auto-submits FinalScore if player name has been set.
///   - Panel open → fetches top N scores and rebuilds the row list.
///   - Player can type a name in nameInputField and hit submitButton at any time.
/// </summary>
public class LeaderboardManager : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    // Config
    // ─────────────────────────────────────────────────────────────

    [Header("Leadr Config")]
    [SerializeField] private string boardId = "brd_your_board_id";
    [SerializeField] private int    fetchLimit = 10;

    // ─────────────────────────────────────────────────────────────
    // Panels
    // ─────────────────────────────────────────────────────────────

    [Header("Panels")]
    [SerializeField] private GameObject leaderboardPanel;

    // ─────────────────────────────────────────────────────────────
    // Score list
    // ─────────────────────────────────────────────────────────────

    [Header("Score List")]
    [SerializeField] private Transform          rowContainer;   // Content object inside a ScrollRect
    [SerializeField] private LeaderboardEntryRow rowPrefab;
    [SerializeField] private GameObject         loadingIndicator;
    [SerializeField] private TextMeshProUGUI    statusLabel;    // "Loading…" / error messages

    // ─────────────────────────────────────────────────────────────
    // Submission
    // ─────────────────────────────────────────────────────────────

    [Header("Submission")]
    [SerializeField] private TMP_InputField     nameInputField;
    [SerializeField] private Button             submitButton;
    [SerializeField] private TextMeshProUGUI    yourRankLabel;  // "Your rank: #4" shown after submit

    // ─────────────────────────────────────────────────────────────
    // Internal
    // ─────────────────────────────────────────────────────────────

    private readonly List<LeaderboardEntryRow> _rows = new List<LeaderboardEntryRow>();
    private GameSessionData _data;
    private bool _submitPending; // score waiting to go up as soon as name is confirmed
    private long _pendingScore;
    private string _lastOwnPlayerName;

    // ─────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        _data = GameManager.Instance.GameData;
        SetPanelVisible(false);
    }

    private void OnEnable()
    {
        GameManager.OnStateChanged += HandleStateChanged;

        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmitClicked);
    }

    private void OnDisable()
    {
        GameManager.OnStateChanged -= HandleStateChanged;

        if (submitButton != null)
            submitButton.onClick.RemoveListener(OnSubmitClicked);
    }

    // ─────────────────────────────────────────────────────────────
    // State handling
    // ─────────────────────────────────────────────────────────────

    private void HandleStateChanged(GameState state)
    {
        if (state != GameState.GameOver) return;

        _pendingScore = _data.FinalScore;

        // If the player already has a saved name from a previous run, submit immediately.
        // Otherwise, surface the panel so they can enter a name first.
        if (!string.IsNullOrWhiteSpace(_lastOwnPlayerName))
        {
            SetNameField(_lastOwnPlayerName);
            _ = SubmitAndRefreshAsync(_lastOwnPlayerName, _pendingScore);
        }
        else
        {
            _submitPending = true;
            SetPanelVisible(true);
            SetStatus("Enter your name to submit your score.");
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Public API  (call from UI buttons in the Inspector)
    // ─────────────────────────────────────────────────────────────

    /// <summary>Open / close the leaderboard panel and refresh scores when opening.</summary>
    public void TogglePanel()
    {
        bool nowVisible = !leaderboardPanel.activeSelf;
        SetPanelVisible(nowVisible);
        if (nowVisible) _ = FetchScoresAsync();
    }

    public void OpenPanel()
    {
        SetPanelVisible(true);
        _ = FetchScoresAsync();
    }

    public void ClosePanel() => SetPanelVisible(false);

    // ─────────────────────────────────────────────────────────────
    // Submit button
    // ─────────────────────────────────────────────────────────────

    private void OnSubmitClicked()
    {
        string playerName = nameInputField != null
            ? nameInputField.text.Trim()
            : string.Empty;

        if (string.IsNullOrWhiteSpace(playerName))
        {
            SetStatus("Please enter a name before submitting.");
            return;
        }

        long scoreToSubmit = _submitPending ? _pendingScore : _data.FinalScore;
        _submitPending = false;

        _ = SubmitAndRefreshAsync(playerName, scoreToSubmit);
    }

    // ─────────────────────────────────────────────────────────────
    // Leadr API calls
    // ─────────────────────────────────────────────────────────────

    private async System.Threading.Tasks.Task SubmitAndRefreshAsync(string playerName, long score)
    {
        SetLoading(true);
        SetStatus("Submitting score…");
        SetSubmitInteractable(false);

        var result = await LeadrClient.Instance.SubmitScoreAsync(boardId, score, playerName);

        if (result.IsSuccess)
        {
            _lastOwnPlayerName = playerName;
            int rank = result.Data.Rank;
            SetYourRank($"Your rank: #{rank}");
            SetStatus(string.Empty);
        }
        else
        {
            SetStatus($"Submit failed: {result.Error}");
            Debug.LogWarning($"[LeaderboardManager] Submit error: {result.Error}");
        }

        SetSubmitInteractable(true);

        // Always refresh the list so the player sees where they landed
        await FetchScoresAsync();
    }

    private async System.Threading.Tasks.Task FetchScoresAsync()
    {
        SetLoading(true);
        SetStatus("Loading scores…");
        ClearRows();

        var result = await LeadrClient.Instance.GetScoresAsync(boardId, limit: fetchLimit);

        SetLoading(false);

        if (!result.IsSuccess)
        {
            SetStatus($"Could not load scores: {result.Error}");
            Debug.LogWarning($"[LeaderboardManager] Fetch error: {result.Error}");
            return;
        }

        SetStatus(string.Empty);

        foreach (var entry in result.Data.Items)
        {
            bool isOwn = !string.IsNullOrEmpty(_lastOwnPlayerName) &&
                         entry.PlayerName == _lastOwnPlayerName;

            LeaderboardEntryRow row = Instantiate(rowPrefab, rowContainer);
            row.Populate(entry.Rank, entry.PlayerName, entry.Value, isOwn);
            _rows.Add(row);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // UI helpers
    // ─────────────────────────────────────────────────────────────

    private void SetPanelVisible(bool visible)
    {
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(visible);
    }

    private void SetLoading(bool isLoading)
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(isLoading);
    }

    private void SetStatus(string message)
    {
        if (statusLabel != null)
            statusLabel.text = message;
    }

    private void SetYourRank(string message)
    {
        if (yourRankLabel != null)
            yourRankLabel.text = message;
    }

    private void SetNameField(string name)
    {
        if (nameInputField != null)
            nameInputField.text = name;
    }

    private void SetSubmitInteractable(bool interactable)
    {
        if (submitButton != null)
            submitButton.interactable = interactable;
    }

    private void ClearRows()
    {
        foreach (var row in _rows)
            if (row != null) Destroy(row.gameObject);
        _rows.Clear();
    }
}
