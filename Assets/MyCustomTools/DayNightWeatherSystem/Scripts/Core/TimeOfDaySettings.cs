using UnityEngine;

namespace DayNightWeather
{
    /// <summary>
    /// Holds all visual settings for a specific time of day.
    /// Create one asset per time period (Dawn, Day, Dusk, Night).
    /// </summary>
    [System.Serializable]
    public class TimeOfDaySettings
    {
        [Header("Sun / Main Light")]
        [Tooltip("Color of the directional sun light at this time.")]
        public Color sunColor = Color.white;

        [Tooltip("Intensity of the directional sun light (0–8).")]
        [Range(0f, 8f)] public float sunIntensity = 1f;

        [Header("Moon Light")]
        [Tooltip("Color of the moon directional light at this time.")]
        public Color moonColor = new Color(0.5f, 0.6f, 0.8f, 1f);

        [Tooltip("Intensity of the moon light (0–2).")]
        [Range(0f, 2f)] public float moonIntensity = 0.15f;

        [Header("Ambient")]
        [Tooltip("Source mode: Skybox, Gradient, or Color.")]
        public UnityEngine.Rendering.AmbientMode ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;

        [Tooltip("Sky/top ambient color.")]
        public Color ambientSkyColor = new Color(0.2f, 0.3f, 0.5f);

        [Tooltip("Equator ambient color.")]
        public Color ambientEquatorColor = new Color(0.2f, 0.2f, 0.3f);

        [Tooltip("Ground ambient color.")]
        public Color ambientGroundColor = new Color(0.1f, 0.1f, 0.1f);

        [Tooltip("Overall ambient intensity multiplier.")]
        [Range(0f, 4f)] public float ambientIntensity = 1f;

        [Header("Fog")]
        [Tooltip("Enable fog at this time of day.")]
        public bool fogEnabled = true;

        [Tooltip("Fog color.")]
        public Color fogColor = new Color(0.7f, 0.8f, 0.9f);

        [Tooltip("Fog density (Exponential mode).")]
        [Range(0f, 0.1f)] public float fogDensity = 0.01f;

        [Tooltip("Fog start distance (Linear mode).")]
        public float fogStartDistance = 50f;

        [Tooltip("Fog end distance (Linear mode).")]
        public float fogEndDistance = 500f;

        [Header("Skybox")]
        [Tooltip("Tint applied to the procedural skybox at this time.")]
        public Color skyboxTint = Color.white;

        [Tooltip("Sky exposure.")]
        [Range(0f, 8f)] public float skyboxExposure = 1f;

        [Tooltip("Horizon color blend.")]
        public Color horizonColor = new Color(0.9f, 0.5f, 0.2f);

        [Header("Stars")]
        [Tooltip("Star brightness (0 = no stars, 1 = full stars).")]
        [Range(0f, 1f)] public float starBrightness = 0f;

        [Header("Cloud")]
        [Tooltip("Cloud coverage (0 = clear, 1 = overcast).")]
        [Range(0f, 1f)] public float cloudCoverage = 0.4f;

        [Tooltip("Cloud brightness tint.")]
        public Color cloudColor = Color.white;

        [Tooltip("Cloud opacity.")]
        [Range(0f, 1f)] public float cloudOpacity = 0.8f;
    }
}
