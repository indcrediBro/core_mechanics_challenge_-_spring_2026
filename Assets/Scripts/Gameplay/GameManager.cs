using System;
using System.Collections.Generic;
using GameData;
using IncredibleAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { MainMenu, Playing, Paused, GameOver, Cutscene }

public class GameManager : Singleton<GameManager>
{
    [Space]
    [SerializeField, ReadOnly, BoxGroup("Game Settings")] public GameState CurrentState;
    [Space]
    [SerializeField, BoxGroup("Game Settings")] private GameSessionData gameData;
    public GameSessionData GameData => gameData;

    public static event Action<GameState> OnStateChanged;
    public static event Action<int>       OnScoreChanged;
    public static event Action<string>    OnWeaponSwitched;
    public static event Action<Platform>  OnPlatformBounced;
    public static event Action            OnEvery10Bounces;

    private readonly HashSet<Platform> _bouncedPlatforms = new HashSet<Platform>();
    private int _uniqueBounceCount;

    public static void SwitchWeapon(string gunName) => OnWeaponSwitched?.Invoke(gunName);
    public static void PlatformBounced(Platform p)  => OnPlatformBounced?.Invoke(p);

    protected override void Awake()
    {
        base.Awake();
        gameData.Initialize();
    }

    private void Start() => ChangeState(GameState.MainMenu);

    private void Update()
    {
        if (CurrentState == GameState.Playing)
            gameData.AddPlayTime(Time.deltaTime);
    }

    public void StartGame()
    {
        ChangeState(GameState.Cutscene);
        SceneManager.LoadScene("CutScene");

    }

    public void LoadGameScene()
    {
        _bouncedPlatforms.Clear();
        _uniqueBounceCount = 0;
        gameData.Initialize();
        OnScoreChanged?.Invoke(gameData.CurrentScore);
        ChangeState(GameState.Playing);
        SceneManager.LoadScene("Game");
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        Time.timeScale = 0f;
        ChangeState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        Time.timeScale = 1f;
        ChangeState(GameState.Playing);
    }

    public void GameOver()
    {
        if (CurrentState == GameState.GameOver) return;
        ChangeState(GameState.GameOver);
        if (gameData.FinalScore > gameData.HighScore)
            gameData.SetHighScore(gameData.FinalScore);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Registers a platform bounce. Score and weapon-switch counter only advance
    /// the first time a given platform is landed on per run.
    /// </summary>
    public void RegisterBounce(Platform platform)
    {
        if (platform == null)              return; // non-platform ground, skip
        if (_bouncedPlatforms.Contains(platform)) return; // already counted

        _bouncedPlatforms.Add(platform);
        gameData.AddUniquePlatformBounce();
        OnScoreChanged?.Invoke(gameData.CurrentScore);

        _uniqueBounceCount++;
        if (_uniqueBounceCount % 10 == 0)
            OnEvery10Bounces?.Invoke();
    }

    /// <summary>Awards kill score. Called by EnemyHealth on death.</summary>
    public void RegisterKill()
    {
        gameData.AddEnemyKill();
        OnScoreChanged?.Invoke(gameData.CurrentScore);
    }

    private void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }
}