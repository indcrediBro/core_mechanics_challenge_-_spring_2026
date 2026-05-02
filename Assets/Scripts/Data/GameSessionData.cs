using UnityEngine;
using Core.Settings;
using GameData;
using IncredibleAttributes;

namespace GameData
{
    [CreateAssetMenu(fileName = "GameSessionData", menuName = "Game/Session Data", order = 1)]
    public class GameSessionData : ScriptableObject
    {
        // ─────────────────────────────────────────────────────────────
        // Run components  (each drives part of the final score)
        // ─────────────────────────────────────────────────────────────

        [Title("Run Stats")]
        [ShowNonSerializedField] private int  platformsBounced; // unique platforms only
        [ShowNonSerializedField] private int  enemiesKilled;
        [ShowNonSerializedField] private float playTime;        // seconds

        // ─────────────────────────────────────────────────────────────
        // Persistence
        // ─────────────────────────────────────────────────────────────

        [Title("Records")]
        [ShowNonSerializedField] private int  highScore;
        [ShowNonSerializedField] private string highscoreKey;

        // ─────────────────────────────────────────────────────────────
        // Lifecycle
        // ─────────────────────────────────────────────────────────────

        public void Initialize()
        {
            platformsBounced = 0;
            enemiesKilled    = 0;
            playTime         = 0f;

            highscoreKey = GameDefaultData.HighScoreKey;
            highScore    = GamePrefs.GetInt(highscoreKey, 0);
        }

        // ─────────────────────────────────────────────────────────────
        // Read-only accessors
        // ─────────────────────────────────────────────────────────────

        public int   PlatformsBounced => platformsBounced;
        public int   EnemiesKilled    => enemiesKilled;
        public float PlayTime         => playTime;
        public int   HighScore        => highScore;
        public float Sensitivity      => SettingsManager.LiveGameplay.MouseSensitivity;

        /// <summary>Live score shown during gameplay (no playtime bonus yet).</summary>
        public int CurrentScore =>
            platformsBounced * GameDefaultData.ScorePerPlatform +
            enemiesKilled    * GameDefaultData.ScorePerKill;

        /// <summary>
        /// Final score shown on the Game Over screen.
        /// Adds 1 point per second survived on top of the live score.
        /// </summary>
        public int FinalScore => CurrentScore + Mathf.FloorToInt(playTime);

        // ─────────────────────────────────────────────────────────────
        // Mutators
        // ─────────────────────────────────────────────────────────────

        /// <summary>Call once per unique platform bounce.</summary>
        public void AddUniquePlatformBounce() => platformsBounced++;

        /// <summary>Call when an enemy dies.</summary>
        public void AddEnemyKill() => enemiesKilled++;

        /// <summary>Call every frame while Playing to accumulate run time.</summary>
        public void AddPlayTime(float delta) => playTime += delta;

        public void SetHighScore(int score)
        {
            highScore = score;
            GamePrefs.SetInt(highscoreKey, highScore);
        }
    }
}