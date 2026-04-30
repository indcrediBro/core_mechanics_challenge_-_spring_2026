// ============================================================
//  SettingsData.cs
//  Place anywhere in Assets/ (except an Editor/ folder).
//
//  Pure data classes — no Unity lifecycle, no MonoBehaviour.
//  All fields are [Serializable] so GamePrefs can store them
//  via JsonUtility.
// ============================================================

using System;

namespace Core.Settings
{
// ── Audio ──────────────────────────────────────────────────────────────────

/// <summary>All audio-related settings. Volumes are linear 0–1.</summary>
[Serializable]
public class AudioSettings
{
    public float MasterVolume = 1f;
    public float MusicVolume  = 0.8f;
    public float SFXVolume    = 1f;
    public float UIVolume     = 1f;

    public AudioSettings Clone() => (AudioSettings)MemberwiseClone();

    public void CopyFrom(AudioSettings src)
    {
        MasterVolume = src.MasterVolume;
        MusicVolume  = src.MusicVolume;
        SFXVolume    = src.SFXVolume;
        UIVolume     = src.UIVolume;
    }
}

// ── Graphics ───────────────────────────────────────────────────────────────

/// <summary>All graphics-related settings.</summary>
[Serializable]
public class GraphicsSettings
{
    /// <summary>Index into QualitySettings.names (0 = lowest).</summary>
    public int   QualityLevel      = 2;

    public bool  Fullscreen        = true;

    public GraphicsSettings Clone() => (GraphicsSettings)MemberwiseClone();

    public void CopyFrom(GraphicsSettings src)
    {
        QualityLevel      = src.QualityLevel;
        Fullscreen        = src.Fullscreen;
    }
}

// ── Gameplay ───────────────────────────────────────────────────────────────

/// <summary>Gameplay and control preferences.</summary>
[Serializable]
public class GameplaySettings
{
    public float  MouseSensitivity  = 1f;
    public bool   InvertY           = false;
    public bool   ProfanityEnabled     = true;

    public string Language          = "English";
    /// <summary>0 = Easy, 1 = Normal, 2 = Hard.</summary>
    // public int    Difficulty        = 1;


    public GameplaySettings Clone() => (GameplaySettings)MemberwiseClone();

    public void CopyFrom(GameplaySettings src)
    {
        MouseSensitivity  = src.MouseSensitivity;
        InvertY           = src.InvertY;
        ProfanityEnabled     = src.ProfanityEnabled;
        Language          = src.Language;
    }
}

// ── Accessibility ──────────────────────────────────────────────────────────

/// <summary>Accessibility preferences.</summary>
[Serializable]
public class AccessibilitySettings
{

    public AccessibilitySettings Clone() => (AccessibilitySettings)MemberwiseClone();

    public void CopyFrom(AccessibilitySettings src)
    {
    }
}
}