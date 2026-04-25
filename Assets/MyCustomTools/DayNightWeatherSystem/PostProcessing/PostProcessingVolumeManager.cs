using UnityEngine;
using UnityEngine.Rendering;

namespace DayNightWeather
{
    /// <summary>
    /// Manages the weights of four Post-Processing Volumes (Day, Sunset, Night, Sunrise)
    /// by reading blend weights from EnvironmentManager each frame.
    ///
    /// Requirements:
    ///   - URP with the Volume system enabled.
    ///   - Assign one Volume component per time-of-day phase.
    ///   - An EnvironmentManager must exist in the scene.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("Day Night System/Post Processing Volume Manager")]
    public class PostProcessingVolumeManager : MonoBehaviour
    {
        [Header("Post Processing Volumes")]
        [Tooltip("Volume used during full daytime.")]
        public Volume dayVolume;

        [Tooltip("Volume used during the sunset transition.")]
        public Volume sunsetVolume;

        [Tooltip("Volume used during full night.")]
        public Volume nightVolume;

        [Tooltip("Volume used during the sunrise transition.")]
        public Volume sunriseVolume;

        [Header("Blend Settings")]
        [Tooltip("Smooth the volume weight transitions over this time (seconds).")]
        [Min(0f)]
        public float blendSmoothing = 0.5f;

        [Header("Debug")]
        [Tooltip("Show current volume weights in the Inspector at runtime.")]
        public bool showDebugWeights = false;
        [SerializeField, HideInInspector] private float _debugDay;
        [SerializeField, HideInInspector] private float _debugSunset;
        [SerializeField, HideInInspector] private float _debugNight;
        [SerializeField, HideInInspector] private float _debugSunrise;

        // ─── Private ─────────────────────────────────────────────────────────
        private EnvironmentManager _env;
        private float _currentDay, _currentSunset, _currentNight, _currentSunrise;

        // ─── Unity ───────────────────────────────────────────────────────────
        private void OnEnable()
        {
            _env = FindFirstObjectByType<EnvironmentManager>();
            if (_env == null)
                Debug.LogWarning("[PostProcessingVolumeManager] No EnvironmentManager found in scene.", this);
        }

        private void Update()
        {
            if (_env == null)
            {
                _env = FindFirstObjectByType<EnvironmentManager>();
                return;
            }

            float targetDay     = _env.DayWeight;
            float targetSunset  = _env.SunsetWeight;
            float targetNight   = _env.NightWeight;
            float targetSunrise = _env.SunriseWeight;

            float speed = blendSmoothing > 0f ? Time.deltaTime / blendSmoothing : 1f;

            _currentDay     = Mathf.MoveTowards(_currentDay,     targetDay,     speed);
            _currentSunset  = Mathf.MoveTowards(_currentSunset,  targetSunset,  speed);
            _currentNight   = Mathf.MoveTowards(_currentNight,   targetNight,   speed);
            _currentSunrise = Mathf.MoveTowards(_currentSunrise, targetSunrise, speed);

            ApplyWeight(dayVolume,     _currentDay);
            ApplyWeight(sunsetVolume,  _currentSunset);
            ApplyWeight(nightVolume,   _currentNight);
            ApplyWeight(sunriseVolume, _currentSunrise);

            if (showDebugWeights)
            {
                _debugDay     = _currentDay;
                _debugSunset  = _currentSunset;
                _debugNight   = _currentNight;
                _debugSunrise = _currentSunrise;
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────────────
        private static void ApplyWeight(Volume volume, float weight)
        {
            if (volume == null) return;
            volume.weight = Mathf.Clamp01(weight);
            volume.enabled = volume.weight > 0.001f;
        }

        /// <summary>
        /// Manually set the target time-of-day weights without an EnvironmentManager.
        /// Useful for cinematic scripting.
        /// </summary>
        public void SetWeightsManual(float day, float sunset, float night, float sunrise)
        {
            ApplyWeight(dayVolume,     day);
            ApplyWeight(sunsetVolume,  sunset);
            ApplyWeight(nightVolume,   night);
            ApplyWeight(sunriseVolume, sunrise);
        }
    }
}
