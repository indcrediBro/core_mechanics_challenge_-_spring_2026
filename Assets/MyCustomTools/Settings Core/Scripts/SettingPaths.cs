// ============================================================
//  SettingPaths.cs
//  Place anywhere in Assets/ (except an Editor/ folder).
//
//  One enum per value type — enforces correct usage at compile
//  time. You cannot accidentally pass a BoolSetting to SetFloat.
// ============================================================

namespace Core.Settings
{
    /// <summary>All float settings accessible through <see cref="SettingsManager"/>.</summary>
    public enum FloatSetting
    {
        // ── Audio ───────────────────────────────────────────────────────────────
        Audio_MasterVolume,
        Audio_MusicVolume,
        Audio_SFXVolume,
        Audio_UIVolume,

        // ── Gameplay ────────────────────────────────────────────────────────────
        Gameplay_MouseSensitivity,
    }

    /// <summary>All integer settings accessible through <see cref="SettingsManager"/>.</summary>
    public enum IntSetting
    {
        // ── Graphics ─────────────────────────────────────────────────────────────
        Graphics_QualityLevel,
    }

    /// <summary>All boolean settings accessible through <see cref="SettingsManager"/>.</summary>
    public enum BoolSetting
    {
        // ── Graphics ─────────────────────────────────────────────────────────────
        Graphics_Fullscreen,
        // ── Gameplay ─────────────────────────────────────────────────────────────
        Gameplay_InvertY,
        Gameplay_EnableProfanity,
    }

    /// <summary>All string settings accessible through <see cref="SettingsManager"/>.</summary>
    public enum StringSetting
    {
        // ── Gameplay ─────────────────────────────────────────────────────────────
        Gameplay_Language,
    }
}