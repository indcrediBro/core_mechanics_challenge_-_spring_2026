using UnityEngine;
using Core.Settings;
using IncredibleAttributes;

namespace GameData
{
    [CreateAssetMenu(fileName = "GameSessionData", menuName = "Game/Session Data", order = 1)]
    public class GameSessionData: ScriptableObject
    {

        [Title("Gameplay")]
        [ShowNonSerializedField] private int currentScore;
        [ShowNonSerializedField] private int highScore;
        [ShowNonSerializedField] private float playTime;

        [ShowNonSerializedField] private string highscoreKey;


        public void Initialize()
        {
            currentScore = 0;
            highscoreKey = GameDefaultData.HighScoreKey;
            highScore = GamePrefs.GetInt(highscoreKey, 0);
        }

        public int CurrentScore => currentScore;
        public int HighScore => highScore;
        public float PlayTime => playTime;
        public float Sensitivity => SettingsManager.LiveGameplay.MouseSensitivity;


        public void AddScore(int _scoreToAdd)
        {
            currentScore += _scoreToAdd;
        }

        public void RemoveScore(int _scoreToRemove)
        {
            currentScore -= Mathf.Clamp(_scoreToRemove, 0, int.MaxValue);
        }

        public void SetHighScore(int _highScore)
        {
            highScore = _highScore;
            GamePrefs.SetInt(highscoreKey, highScore);
        }

        public void SetPlayTime(float _time)
        {
            playTime = _time;
        }
    }
}