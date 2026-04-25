using System;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DayNightWeather
{
    /// <summary>
    /// Core manager that drives the day/night cycle.
    /// Controls: Main Light (sun), Ambient Light, Fog, and Skybox material.
    /// Works in both Edit Mode (via [ExecuteAlways]) and Play Mode.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("Day Night System/Environment Manager")]
    public class EnvironmentManager : MonoBehaviour
    {
        // ─── Events ──────────────────────────────────────────────────────────
        /// <summary>Fired when the time-of-day phase changes (Day/Sunset/Night/Sunrise).</summary>
        public static event Action<TimeOfDayPhase> OnPhaseChanged;

        // ─── Inspector ───────────────────────────────────────────────────────
        [Header("Settings Asset")]
        [Tooltip("Drag a EnvironmentSettings ScriptableObject here.")]
        public EnvironmentSettings settings;

        [Header("Scene References")]
        [Tooltip("The directional light acting as the Sun.")]
        public Light sunLight;

        [Tooltip("The directional light acting as the Moon. Managed by MoonLightController.")]
        public Light moonLight;

        [Tooltip("The skybox material (must use the ProceduralSkybox shader).")]
        public Material skyboxMaterial;

        [Header("Runtime Control")]
        [Tooltip("Advance time in Play Mode.")]
        public bool running = true;

        [Tooltip("Override the time of day manually (0 = midnight, 0.5 = noon).")]
        [Range(0f, 1f)]
        public float timeOfDay = 0.25f;

        [Tooltip("Multiplier on the passage of time (1 = normal speed).")]
        [Min(0f)]
        public float timeScale = 1f;

        // ─── Public Read-Only ────────────────────────────────────────────────
        /// <summary>Current phase of the day (Day / Sunset / Night / Sunrise).</summary>
        public TimeOfDayPhase CurrentPhase { get; private set; } = TimeOfDayPhase.Day;

        /// <summary>Normalized blend weight for Sunrise post-processing.</summary>
        public float SunriseWeight { get; private set; }

        /// <summary>Normalized blend weight for Day post-processing.</summary>
        public float DayWeight { get; private set; }

        /// <summary>Normalized blend weight for Sunset post-processing.</summary>
        public float SunsetWeight { get; private set; }

        /// <summary>Normalized blend weight for Night post-processing.</summary>
        public float NightWeight { get; private set; }

        // ─── Private ─────────────────────────────────────────────────────────
        private TimeOfDayPhase _previousPhase = TimeOfDayPhase.Day;
        private static readonly int _SkyTop      = Shader.PropertyToID("_SkyTopColor");
        private static readonly int _SkyHorizon  = Shader.PropertyToID("_SkyHorizonColor");
        private static readonly int _SkyBottom   = Shader.PropertyToID("_SkyBottomColor");
        private static readonly int _SunDir      = Shader.PropertyToID("_SunDirection");
        private static readonly int _StarBright  = Shader.PropertyToID("_StarBrightness");
        private static readonly int _CloudColor  = Shader.PropertyToID("_CloudColor");
        private static readonly int _CloudCover  = Shader.PropertyToID("_CloudCoverage");
        private static readonly int _CloudSpeed  = Shader.PropertyToID("_CloudSpeed");
        private static readonly int _CloudSoft   = Shader.PropertyToID("_CloudSoftness");
        private static readonly int _MoonDir     = Shader.PropertyToID("_MoonDirection");

        // ─── Unity ───────────────────────────────────────────────────────────
        private void OnEnable()
        {
            if (settings == null) return;
            timeOfDay = settings.startTimeNormalized;
            Apply(timeOfDay);
        }

        private void Update()
        {
            if (settings == null) return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Apply(timeOfDay);
                return;
            }
#endif
            if (running && settings.dayDurationSeconds > 0f)
            {
                timeOfDay += (Time.deltaTime / settings.dayDurationSeconds) * timeScale;
                if (timeOfDay >= 1f) timeOfDay -= 1f;
            }

            Apply(timeOfDay);
        }

        // ─── Core Apply ──────────────────────────────────────────────────────
        /// <summary>Applies all environment properties for the given normalized time (0–1).</summary>
        public void Apply(float t)
        {
            if (settings == null) return;

            UpdateSun(t);
            UpdateMoon(t);
            UpdateAmbient(t);
            UpdateFog(t);
            UpdateSkybox(t);
            UpdatePhaseWeights(t);
            FirePhaseEvent();
        }

        // ─── Sun ─────────────────────────────────────────────────────────────
        private void UpdateSun(float t)
        {
            if (sunLight == null) return;

            float intensity = settings.sunIntensity.Evaluate(t) * settings.sunMaxIntensity;
            sunLight.color     = settings.sunColor.Evaluate(t);
            sunLight.intensity = intensity;

            // Rotate sun: -90° at midnight, +90° at noon
            float angleDeg = (t * 360f) - 90f;
            sunLight.transform.rotation = Quaternion.Euler(angleDeg, -30f, 0f);

            // Disable renderer when below horizon to save shadow cost
            sunLight.enabled = intensity > 0.001f;
        }

        // ─── Moon ────────────────────────────────────────────────────────────
        private void UpdateMoon(float t)
        {
            if (moonLight == null) return;

            float intensity = settings.moonIntensity.Evaluate(t) * settings.moonMaxIntensity;
            moonLight.color     = settings.moonColor.Evaluate(t);
            moonLight.intensity = intensity;

            // Moon is opposite the sun
            float angleDeg = ((t + 0.5f) % 1f * 360f) - 90f;
            moonLight.transform.rotation = Quaternion.Euler(angleDeg, -30f, 0f);

            moonLight.enabled = intensity > 0.001f;

            // Skybox moon direction
            if (skyboxMaterial != null)
                skyboxMaterial.SetVector(_MoonDir, -moonLight.transform.forward);
        }

        // ─── Ambient ─────────────────────────────────────────────────────────
        private void UpdateAmbient(float t)
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;

            float mult = settings.ambientIntensity.Evaluate(t) * settings.ambientMaxIntensity;

            RenderSettings.ambientSkyColor     = settings.ambientSkyColor.Evaluate(t)     * mult;
            RenderSettings.ambientEquatorColor = settings.ambientEquatorColor.Evaluate(t) * mult;
            RenderSettings.ambientGroundColor  = settings.ambientGroundColor.Evaluate(t)  * mult;
        }

        // ─── Fog ─────────────────────────────────────────────────────────────
        private void UpdateFog(float t)
        {
            if (!settings.controlFog) return;

            RenderSettings.fog        = true;
            RenderSettings.fogMode    = settings.fogMode;
            RenderSettings.fogColor   = settings.fogColor.Evaluate(t);
            RenderSettings.fogDensity = settings.fogDensity.Evaluate(t) * settings.fogMaxDensity;
        }

        // ─── Skybox ──────────────────────────────────────────────────────────
        private void UpdateSkybox(float t)
        {
            if (skyboxMaterial == null) return;

            RenderSettings.skybox = skyboxMaterial;

            skyboxMaterial.SetColor(_SkyTop,     settings.skyTopColor.Evaluate(t));
            skyboxMaterial.SetColor(_SkyHorizon, settings.skyHorizonColor.Evaluate(t));
            skyboxMaterial.SetColor(_SkyBottom,  settings.skyBottomColor.Evaluate(t));
            skyboxMaterial.SetFloat(_StarBright, settings.starVisibility.Evaluate(t) * settings.starMaxBrightness);
            skyboxMaterial.SetColor(_CloudColor, settings.cloudColor.Evaluate(t));
            skyboxMaterial.SetFloat(_CloudCover, settings.cloudCoverage);
            skyboxMaterial.SetFloat(_CloudSpeed, settings.cloudSpeed);
            skyboxMaterial.SetFloat(_CloudSoft,  settings.cloudSoftness);

            if (sunLight != null)
                skyboxMaterial.SetVector(_SunDir, -sunLight.transform.forward);

            DynamicGI.UpdateEnvironment();
        }

        // ─── Phase Weights ───────────────────────────────────────────────────
        private void UpdatePhaseWeights(float t)
        {
            if (settings == null) return;

            float sr0 = settings.sunriseStart,  sr1 = settings.sunriseEnd;
            float ss0 = settings.sunsetStart,   ss1 = settings.sunsetEnd;

            // Sunrise blend: [sr0..sr1]
            if (t >= sr0 && t <= sr1)
            {
                float blend = Mathf.InverseLerp(sr0, sr1, t);
                SunriseWeight = 1f - blend;
                DayWeight     = blend;
                SunsetWeight  = 0f;
                NightWeight   = 0f;
                CurrentPhase  = TimeOfDayPhase.Sunrise;
            }
            // Full Day: [sr1..ss0]
            else if (t > sr1 && t < ss0)
            {
                SunriseWeight = 0f;
                DayWeight     = 1f;
                SunsetWeight  = 0f;
                NightWeight   = 0f;
                CurrentPhase  = TimeOfDayPhase.Day;
            }
            // Sunset blend: [ss0..ss1]
            else if (t >= ss0 && t <= ss1)
            {
                float blend = Mathf.InverseLerp(ss0, ss1, t);
                SunriseWeight = 0f;
                DayWeight     = 1f - blend;
                SunsetWeight  = blend;
                NightWeight   = 0f;
                CurrentPhase  = TimeOfDayPhase.Sunset;
            }
            // Full Night: everything else
            else
            {
                float nightT = t > ss1
                    ? Mathf.InverseLerp(ss1, 1f, t)
                    : Mathf.InverseLerp(0f, sr0, t);
                SunriseWeight = 0f;
                DayWeight     = 0f;
                SunsetWeight  = 0f;
                NightWeight   = 1f;
                CurrentPhase  = TimeOfDayPhase.Night;
            }
        }

        // ─── Phase Event ─────────────────────────────────────────────────────
        private void FirePhaseEvent()
        {
            if (CurrentPhase != _previousPhase)
            {
                _previousPhase = CurrentPhase;
                OnPhaseChanged?.Invoke(CurrentPhase);
            }
        }

        // ─── Public Helpers ──────────────────────────────────────────────────
        /// <summary>Returns the approximate real-world time string, e.g. "14:35".</summary>
        public string GetTimeString()
        {
            float totalMinutes = timeOfDay * 24f * 60f;
            int hours   = Mathf.FloorToInt(totalMinutes / 60f) % 24;
            int minutes = Mathf.FloorToInt(totalMinutes % 60f);
            return $"{hours:00}:{minutes:00}";
        }

        /// <summary>Sets the time of day from an hours value (0–24).</summary>
        public void SetTimeHours(float hours)
        {
            timeOfDay = Mathf.Repeat(hours / 24f, 1f);
        }
    }

    /// <summary>The broad phase of the current time of day.</summary>
    public enum TimeOfDayPhase { Day, Sunset, Night, Sunrise }
}
