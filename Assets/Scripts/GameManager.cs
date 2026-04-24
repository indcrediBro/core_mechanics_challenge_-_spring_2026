using System;
using GameData;
using IncredibleAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { MainMenu, Playing, Paused, GameOver }

public class GameManager : Singleton<GameManager>
{
    [Space]
    [SerializeField, ReadOnly, BoxGroup("Game Settings")] private GameState currentState;

    public static event Action<GameState> OnStateChanged;
    public static event Action<int>       OnScoreChanged;

    [Space]
    [SerializeField, Expandable, BoxGroup("Game Settings")] private GameDefaultData defaultData;

    [SerializeField, BoxGroup("Game Settings")] private GameSession gameData;

    protected override void Awake()
    {
        base.Awake();
        LoadPrefs();
    }

    private void Start()
    {
        ChangeState(GameState.MainMenu);
    }

    public void StartGame()
    {
        gameData.Initialize(defaultData.HighScoreKey);
        ChangeState(GameState.Playing);
        OnScoreChanged?.Invoke(gameData.CurrentScore);
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;
        Time.timeScale = 0f;
        ChangeState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;
        Time.timeScale = 1f;
        ChangeState(GameState.Playing);
    }

    public void GameOver()
    {
        if (currentState == GameState.GameOver) return;
        ChangeState(GameState.GameOver);

        if (gameData.CurrentScore > gameData.HighScore)
        {
            gameData.SetHighScore(gameData.CurrentScore);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ChangeState(GameState newState)
    {
        currentState = newState;
        OnStateChanged?.Invoke(newState);
    }

    private void LoadPrefs()
    {
        gameData.Initialize(defaultData.HighScoreKey);
    }
}
