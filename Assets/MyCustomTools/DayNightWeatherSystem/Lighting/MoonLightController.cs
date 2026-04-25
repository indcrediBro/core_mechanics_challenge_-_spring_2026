using UnityEngine;

namespace DayNightWeather
{
    /// <summary>
    /// Controls the Moon directional light with lunar phase simulation
    /// and cookie-based surface detail.
    ///
    /// Attach alongside a Directional Light component.
    /// The EnvironmentManager also sets moon color/intensity, so this
    /// component focuses on phase and visual detail.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Light))]
    [AddComponentMenu("Day Night System/Moon Light Controller")]
    public class MoonLightController : MonoBehaviour
    {
        // ─── Inspector ───────────────────────────────────────────────────────
        [Header("Lunar Phase")]
        [Tooltip("Enable simulated lunar phases over a 29.5-day cycle.")]
        public bool simulateLunarPhases = true;

        [Tooltip("Duration of one lunar cycle in real-world seconds (default = 29.5 game-days × dayDuration).")]
        [Min(1f)]
        public float lunarCycleDuration = 29.5f * 120f;

        [Tooltip("Current progress through the lunar cycle (0–1). 0 = new moon, 0.5 = full moon.")]
        [Range(0f, 1f)]
        public float lunarPhase = 0.5f;

        [Header("Light Flicker (atmospheric turbulence)")]
        [Tooltip("Enable a subtle intensity flicker to mimic atmospheric scintillation.")]
        public bool enableFlicker = true;

        [Tooltip("Maximum flicker magnitude as a fraction of the base intensity.")]
        [Range(0f, 0.1f)]
        public float flickerMagnitude = 0.015f;

        [Tooltip("Speed of the flicker oscillation.")]
        [Range(0f, 10f)]
        public float flickerSpeed = 2f;

        [Header("Halo")]
        [Tooltip("Show a light halo around the moon object (requires the Light Halo component).")]
        public bool showHalo = true;

        [Range(0f, 2f)]
        [Tooltip("Size of the moon halo.")]
        public float haloSize = 0.5f;

        // ─── Private ─────────────────────────────────────────────────────────
        private Light    _light;
        private float    _baseIntensity;
        private float    _flickerOffset;

        // ─── Unity ───────────────────────────────────────────────────────────
        private void Awake()
        {
            _light = GetComponent<Light>();
            _flickerOffset = Random.Range(0f, 100f);
        }

        private void Update()
        {
            if (_light == null) return;

            // Cache base intensity before flicker
            _baseIntensity = _light.intensity;

            if (simulateLunarPhases && Application.isPlaying)
            {
                lunarPhase = Mathf.Repeat(lunarPhase + Time.deltaTime / lunarCycleDuration, 1f);
                ApplyLunarPhase();
            }

            if (enableFlicker && Application.isPlaying)
                ApplyFlicker();

#if UNITY_EDITOR
            if (!Application.isPlaying)
                ApplyLunarPhase();
#endif
        }

        // ─── Lunar Phase ─────────────────────────────────────────────────────
        private void ApplyLunarPhase()
        {
            // Full moon at 0.5, new moon at 0 and 1
            // Intensity curve: sine wave peak at 0.5
            float phaseIntensityMult = Mathf.Sin(lunarPhase * Mathf.PI);
            _light.intensity = _baseIntensity * phaseIntensityMult;

            // Tint: new moon = cooler blue, full moon = warm silver
            float warmth = phaseIntensityMult;
            _light.color = Color.Lerp(
                new Color(0.4f, 0.5f, 0.8f),   // new moon tint
                new Color(0.85f, 0.9f, 1.0f),  // full moon tint
                warmth);
        }

        // ─── Flicker ─────────────────────────────────────────────────────────
        private void ApplyFlicker()
        {
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed + _flickerOffset, 0f);
            float delta = (noise - 0.5f) * 2f * flickerMagnitude;
            _light.intensity = Mathf.Max(0f, _light.intensity + _light.intensity * delta);
        }

        // ─── Gizmos ──────────────────────────────────────────────────────────
        private void OnDrawGizmosSelected()
        {
            // Draw a simple moon-disc gizmo in editor
            Gizmos.color = new Color(0.8f, 0.9f, 1f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }

        // ─── Public ──────────────────────────────────────────────────────────
        /// <returns>Human-readable lunar phase name.</returns>
        public string GetPhaseName()
        {
            if (lunarPhase < 0.0625f || lunarPhase > 0.9375f) return "New Moon";
            if (lunarPhase < 0.1875f) return "Waxing Crescent";
            if (lunarPhase < 0.3125f) return "First Quarter";
            if (lunarPhase < 0.4375f) return "Waxing Gibbous";
            if (lunarPhase < 0.5625f) return "Full Moon";
            if (lunarPhase < 0.6875f) return "Waning Gibbous";
            if (lunarPhase < 0.8125f) return "Last Quarter";
            return "Waning Crescent";
        }
    }
}
