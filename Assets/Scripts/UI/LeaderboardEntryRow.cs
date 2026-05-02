using TMPro;
using UnityEngine;

/// <summary>
/// Attach to the row prefab used inside the leaderboard scroll list.
/// LeaderboardManager calls Populate() on each instance.
/// </summary>
public class LeaderboardEntryRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankLabel;
    [SerializeField] private TextMeshProUGUI playerNameLabel;
    [SerializeField] private TextMeshProUGUI scoreLabel;

    [Header("Own-score highlight")]
    [SerializeField] private GameObject highlightOverlay; // optional tinted panel behind the row

    public void Populate(int rank, string playerName, double score, bool isOwnScore = false)
    {
        if (rankLabel       != null) rankLabel.text       = $"#{rank}";
        if (playerNameLabel != null) playerNameLabel.text = playerName;
        if (scoreLabel      != null) scoreLabel.text      = $"{score:N0}";
        if (highlightOverlay != null) highlightOverlay.SetActive(isOwnScore);
    }
}
