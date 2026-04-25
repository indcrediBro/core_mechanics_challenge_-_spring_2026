using UnityEngine;

namespace DayNightWeather
{
    /// <summary>
    /// Controls a Unity WindZone and exposes wind parameters to shaders as global
    /// properties (_WindDirection, _WindStrength, _WindTurbulence) so vegetation
    /// shaders can react without individual component setup.
    ///
    /// Features:
    ///  - Time-of-day wind variation (calmer at night, stronger afternoon).
    ///  - Gust system with random pulses.
    ///  - Weather state transitions (calm → breeze → windy → storm).
    ///  - Optional wind audio source pitch/volume control.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("Day Night System/Wind Manager")]
    public class WindManager : MonoBehaviour
    {
        // ─── Inspector ───────────────────────────────────────────────────────
        [Header("Wind Zone")]
        [Tooltip("The Unity WindZone component to drive.")]
        public WindZone windZone;

        [Header("Base Wind")]
        [Tooltip("Base wind strength at calm state.")]
        [Range(0f, 3f)]
        public float baseStrength = 0.5f;

        [Tooltip("Turbulence added on top of base strength.")]
        [Range(0f, 1f)]
        public float turbulence = 0.2f;

        [Tooltip("How quickly wind direction slowly rotates (degrees per second).")]
        [Range(0f, 10f)]
        public float directionDriftSpeed = 1f;

        [Header("Gusts")]
        [Tooltip("Enable random wind gusts.")]
        public bool enableGusts = true;

        [Tooltip("Minimum time between gusts (seconds).")]
        [Min(0.5f)]
        public float gustMinInterval = 5f;

        [Tooltip("Maximum time between gusts (seconds).")]
        [Min(1f)]
        public float gustMaxInterval = 20f;

        [Tooltip("Multiplier applied to wind strength during a gust.")]
        [Range(1f, 5f)]
        public float gustStrengthMultiplier = 2.5f;

        [Tooltip("Duration of a gust in seconds.")]
        [Range(0.1f, 5f)]
        public float gustDuration = 1.5f;

        [Header("Weather State")]
        [Tooltip("Current weather state driving wind intensity.")]
        public WeatherState weatherState = WeatherState.Calm;

        [Tooltip("How quickly weather state transitions blend (seconds).")]
        [Min(0.1f)]
        public float weatherTransitionTime = 5f;

        [Header("Day/Night Variation")]
        [Tooltip("Animate wind based on time of day (requires EnvironmentManager in scene).")]
        public bool useDayNightVariation = true;

        [Tooltip("Curve multiplying wind by time of day (0 = midnight, 0.5 = noon).")]
        public AnimationCurve dayNightWindCurve = DefaultDayNightCurve();

        [Header("Audio")]
        [Tooltip("Optional AudioSource for ambient wind sound.")]
        public AudioSource windAudioSource;

        [Tooltip("Volume at maximum wind strength.")]
        [Range(0f, 1f)]
        public float maxAudioVolume = 0.8f;

        // ─── Shader Property IDs ──────────────────────────────────────────────
        private static readonly int _WindDir       = Shader.PropertyToID("_WindDirection");
        private static readonly int _WindStrength  = Shader.PropertyToID("_WindStrength");
        private static readonly int _WindTurbulence= Shader.PropertyToID("_WindTurbulence");
        private static readonly int _WindTime      = Shader.PropertyToID("_WindTime");

        // ─── Private ─────────────────────────────────────────────────────────
        private float          _currentDirection = 0f;
        private float          _currentStrength  = 0f;
        private float          _targetStrength   = 0f;
        private float          _gustTimer        = 0f;
        private float          _gustTimeRemaining= 0f;
        private bool           _inGust           = false;
        private EnvironmentManager _env;
        private float          _windTime         = 0f;

        // Weather-state base strength table
        private static readonly float[] WeatherStrengths = { 0.1f, 0.5f, 1.2f, 2.5f };

        // ─── Unity ───────────────────────────────────────────────────────────
        private void OnEnable()
        {
            _env = FindFirstObjectByType<EnvironmentManager>();
            _gustTimer = Random.Range(gustMinInterval, gustMaxInterval);
        }

        private void Update()
        {
            float dt = Application.isPlaying ? Time.deltaTime : 0f;
            _windTime += dt;

            UpdateTargetStrength();
            UpdateGust(dt);
            UpdateDirection(dt);
            ApplyToWindZone();
            PushToShaders();
            UpdateAudio();
        }

        // ─── Target Strength ─────────────────────────────────────────────────
        private void UpdateTargetStrength()
        {
            float stateStrength = WeatherStrengths[Mathf.Clamp((int)weatherState, 0, 3)];
            _targetStrength = baseStrength * stateStrength;

            if (useDayNightVariation && _env != null)
                _targetStrength *= dayNightWindCurve.Evaluate(_env.timeOfDay);

            float speed = Application.isPlaying ? Time.deltaTime / Mathf.Max(0.1f, weatherTransitionTime) : 1f;
            _currentStrength = Mathf.MoveTowards(_currentStrength, _targetStrength, speed * 5f);
        }

        // ─── Gusts ───────────────────────────────────────────────────────────
        private void UpdateGust(float dt)
        {
            if (!enableGusts || !Application.isPlaying) return;

            if (_inGust)
            {
                _gustTimeRemaining -= dt;
                if (_gustTimeRemaining <= 0f)
                {
                    _inGust = false;
                    _gustTimer = Random.Range(gustMinInterval, gustMaxInterval);
                }
            }
            else
            {
                _gustTimer -= dt;
                if (_gustTimer <= 0f)
                {
                    _inGust = true;
                    _gustTimeRemaining = gustDuration;
                }
            }
        }

        // ─── Direction Drift ─────────────────────────────────────────────────
        private void UpdateDirection(float dt)
        {
            float noise = Mathf.PerlinNoise(_windTime * 0.05f, 0f) - 0.5f;
            _currentDirection += noise * directionDriftSpeed * dt;
        }

        // ─── Apply to WindZone ───────────────────────────────────────────────
        private void ApplyToWindZone()
        {
            if (windZone == null) return;

            float gustMult   = (_inGust && Application.isPlaying) ? gustStrengthMultiplier : 1f;
            float finalStrength = _currentStrength * gustMult;

            windZone.windMain        = finalStrength;
            windZone.windTurbulence  = turbulence;
            windZone.windPulseMagnitude = _inGust ? 0.5f : 0.1f;

            transform.rotation = Quaternion.Euler(0f, _currentDirection, 0f);
        }

        // ─── Shader Globals ──────────────────────────────────────────────────
        private void PushToShaders()
        {
            float gustMult      = (_inGust && Application.isPlaying) ? gustStrengthMultiplier : 1f;
            float finalStrength = _currentStrength * gustMult;

            Vector4 windDir = new Vector4(
                Mathf.Sin(_currentDirection * Mathf.Deg2Rad),
                0f,
                Mathf.Cos(_currentDirection * Mathf.Deg2Rad),
                0f);

            Shader.SetGlobalVector(_WindDir,        windDir);
            Shader.SetGlobalFloat(_WindStrength,    finalStrength);
            Shader.SetGlobalFloat(_WindTurbulence,  turbulence);
            Shader.SetGlobalFloat(_WindTime,        _windTime);
        }

        // ─── Audio ───────────────────────────────────────────────────────────
        private void UpdateAudio()
        {
            if (windAudioSource == null) return;

            float normalizedStrength = Mathf.Clamp01(_currentStrength / 2.5f);
            windAudioSource.volume = normalizedStrength * maxAudioVolume;

            // Subtle pitch variation for realism
            windAudioSource.pitch = 0.9f + normalizedStrength * 0.3f;

            if (normalizedStrength > 0.01f && !windAudioSource.isPlaying)
                windAudioSource.Play();
            else if (normalizedStrength <= 0.01f && windAudioSource.isPlaying)
                windAudioSource.Stop();
        }

        // ─── Default Curve ───────────────────────────────────────────────────
        private static AnimationCurve DefaultDayNightCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f,    0.3f),
                new Keyframe(0.25f, 0.5f),
                new Keyframe(0.50f, 1.0f),
                new Keyframe(0.75f, 0.8f),
                new Keyframe(1f,    0.3f));
        }

        // ─── Gizmos ──────────────────────────────────────────────────────────
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.6f, 0.9f, 1f, 0.8f);
            Vector3 dir = transform.forward * (_currentStrength + 0.5f);
            Gizmos.DrawRay(transform.position, dir);
            Gizmos.DrawWireSphere(transform.position + dir, 0.15f);
        }
    }

    /// <summary>Weather state controlling overall wind intensity.</summary>
    public enum WeatherState { Calm, Breezy, Windy, Storm }
}
