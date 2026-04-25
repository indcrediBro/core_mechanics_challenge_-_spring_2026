using UnityEngine;

namespace DayNightWeather
{
    /// <summary>
    /// Companion component to EnvironmentManager that creates and manages a runtime
    /// instance of the ProceduralSkybox material so changes are not saved back to
    /// the shared asset — avoiding dirtying the project on every play.
    ///
    /// Also exposes additional skybox tweaks not covered by EnvironmentSettings
    /// (sun/moon disc sizes, star parameters, atmospheric fog).
    ///
    /// Attach to the same GameObject as EnvironmentManager.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("Day Night System/Procedural Skybox Controller")]
    [RequireComponent(typeof(EnvironmentManager))]
    public class ProceduralSkyboxController : MonoBehaviour
    {
        // ─── Inspector ───────────────────────────────────────────────────────
        [Header("Source Material")]
        [Tooltip("The ProceduralSkybox material asset. A runtime instance is created automatically.")]
        public Material skyboxSourceMaterial;

        [Header("Sun Disc")]
        [Tooltip("Angular radius of the visible sun disc.")]
        [Range(0.001f, 0.1f)]
        public float sunSize = 0.04f;

        [Tooltip("Bloom / glare radius around the sun.")]
        [Range(0f, 5f)]
        public float sunBloom = 2.0f;

        [Header("Moon Disc")]
        [Tooltip("Angular radius of the visible moon disc.")]
        [Range(0.001f, 0.05f)]
        public float moonSize = 0.025f;

        [Tooltip("Optional moon surface texture (greyscale detail).")]
        public Texture2D moonTexture;

        [Header("Stars")]
        [Tooltip("How densely stars are packed across the sky.")]
        [Range(1f, 100f)]
        public float starDensity = 40f;

        [Tooltip("Base size of each star point.")]
        [Range(0.001f, 0.05f)]
        public float starSize = 0.008f;

        [Tooltip("Speed of star twinkle.")]
        [Range(0f, 5f)]
        public float starTwinkleSpeed = 1.5f;

        [Tooltip("Magnitude of star twinkle brightness variation.")]
        [Range(0f, 1f)]
        public float starTwinkleMagnitude = 0.3f;

        [Header("Cloud Layer")]
        [Tooltip("Elevation of the cloud layer above the horizon (0 = horizon, 1 = zenith).")]
        [Range(0f, 0.5f)]
        public float cloudHeight = 0.1f;

        [Tooltip("Noise tile scale for cloud detail.")]
        [Range(0.5f, 10f)]
        public float cloudScale = 3f;

        [Header("Horizon Glow")]
        [Tooltip("Color of the horizon atmospheric scatter glow.")]
        public Color horizonFogColor = new Color(0.8f, 0.9f, 1f, 1f);

        [Tooltip("Strength of the horizon glow.")]
        [Range(0f, 1f)]
        public float horizonFogStrength = 0.4f;

        [Tooltip("Angular width of the horizon glow band.")]
        [Range(0.001f, 0.5f)]
        public float horizonFogWidth = 0.08f;

        // ─── Private ─────────────────────────────────────────────────────────
        private Material         _runtimeMaterial;
        private EnvironmentManager _env;

        // Shader IDs
        private static readonly int _SunSize    = Shader.PropertyToID("_SunSize");
        private static readonly int _SunBloom   = Shader.PropertyToID("_SunBloom");
        private static readonly int _MoonSize   = Shader.PropertyToID("_MoonSize");
        private static readonly int _MoonTex    = Shader.PropertyToID("_MoonTexture");
        private static readonly int _StarDens   = Shader.PropertyToID("_StarDensity");
        private static readonly int _StarSize   = Shader.PropertyToID("_StarSize");
        private static readonly int _StarTwSpd  = Shader.PropertyToID("_StarTwinkleSpeed");
        private static readonly int _StarTwMag  = Shader.PropertyToID("_StarTwinkleMagnitude");
        private static readonly int _CloudH     = Shader.PropertyToID("_CloudHeight");
        private static readonly int _CloudScale = Shader.PropertyToID("_CloudScale");
        private static readonly int _HFogCol    = Shader.PropertyToID("_HorizonFogColor");
        private static readonly int _HFogStr    = Shader.PropertyToID("_HorizonFogStrength");
        private static readonly int _HFogWidth  = Shader.PropertyToID("_HorizonFogWidth");

        // ─── Unity ───────────────────────────────────────────────────────────
        private void OnEnable()
        {
            _env = GetComponent<EnvironmentManager>();
            CreateRuntimeInstance();
        }

        private void OnDisable()
        {
            if (_runtimeMaterial != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(_runtimeMaterial);
#else
                Destroy(_runtimeMaterial);
#endif
                _runtimeMaterial = null;
            }
        }

        private void Update()
        {
            if (_runtimeMaterial == null)
                CreateRuntimeInstance();

            ApplyProperties();
        }

        // ─── Create Runtime Instance ─────────────────────────────────────────
        private void CreateRuntimeInstance()
        {
            if (skyboxSourceMaterial == null) return;

            _runtimeMaterial = new Material(skyboxSourceMaterial)
            {
                name = skyboxSourceMaterial.name + "_Runtime"
            };

            // Hand the runtime material to EnvironmentManager
            if (_env != null)
                _env.skyboxMaterial = _runtimeMaterial;

            RenderSettings.skybox = _runtimeMaterial;
        }

        // ─── Apply ───────────────────────────────────────────────────────────
        private void ApplyProperties()
        {
            if (_runtimeMaterial == null) return;

            _runtimeMaterial.SetFloat(_SunSize,    sunSize);
            _runtimeMaterial.SetFloat(_SunBloom,   sunBloom);
            _runtimeMaterial.SetFloat(_MoonSize,   moonSize);
            _runtimeMaterial.SetFloat(_StarDens,   starDensity);
            _runtimeMaterial.SetFloat(_StarSize,   starSize);
            _runtimeMaterial.SetFloat(_StarTwSpd,  starTwinkleSpeed);
            _runtimeMaterial.SetFloat(_StarTwMag,  starTwinkleMagnitude);
            _runtimeMaterial.SetFloat(_CloudH,     cloudHeight);
            _runtimeMaterial.SetFloat(_CloudScale, cloudScale);
            _runtimeMaterial.SetColor(_HFogCol,    horizonFogColor);
            _runtimeMaterial.SetFloat(_HFogStr,    horizonFogStrength);
            _runtimeMaterial.SetFloat(_HFogWidth,  horizonFogWidth);

            if (moonTexture != null)
                _runtimeMaterial.SetTexture(_MoonTex, moonTexture);
        }
    }
}
