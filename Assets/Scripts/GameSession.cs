using UnityEngine;
using System;
using IncredibleAttributes;

namespace GameData
{
    [CreateAssetMenu(fileName = "GameSessionData", menuName = "Game/Session Data", order = 1)]
    public class GameSession: ScriptableObject
    {
        [ShowNonSerializedField] private int currentScore;
        [ShowNonSerializedField] private int highScore;
        [ShowNonSerializedField] private float playTime;

        [ShowNonSerializedField] private string highscoreKey;

        public void Initialize(string _highscoreKey)
        {
            currentScore = 0;
            highscoreKey = _highscoreKey;
            highScore = GamePrefs.GetInt(highscoreKey, 0);
        }

        public int CurrentScore => currentScore;
        public int HighScore => highScore;
        public float PlayTime => playTime;

        public void AddScore(int _scoreToAdd)
        {
            currentScore += _scoreToAdd;
        }

        public void RemoveScore(int _scoreToRemove)
        {
            currentScore -= _scoreToRemove;
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