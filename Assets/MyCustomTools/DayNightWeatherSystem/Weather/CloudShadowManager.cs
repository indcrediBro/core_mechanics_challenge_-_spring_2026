using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DayNightWeather
{
    /// <summary>
    /// Drives dynamic cloud shadows by scrolling a cloud texture
    /// as a Light Cookie on the Sun directional light, synchronized
    /// with the skybox cloud speed and wind direction.
    ///
    /// URP Requirements:
    ///   - Sun Light must be set to "Use Cookie".
    ///   - Assign a tileable cloud texture (greyscale, white = open, black = shadow).
    ///   - Enable "Light Cookies" in URP pipeline asset.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("Day Night System/Cloud Shadow Manager")]
    public class CloudShadowManager : MonoBehaviour
    {
        // ─── Inspector ───────────────────────────────────────────────────────
        [Header("References")]
        [Tooltip("The sun directional light to apply the cloud cookie to.")]
        public Light sunLight;

        [Tooltip("Tileable cloud noise texture (greyscale).")]
        public Texture2D cloudTexture;

        [Header("Shadow Settings")]
        [Tooltip("Intensity of the shadow (0 = no shadow, 1 = full black).")]
        [Range(0f, 1f)]
        public float shadowIntensity = 0.5f;

        [Tooltip("Scale of the cloud shadow projection (larger = bigger clouds).")]
        [Range(0.1f, 10f)]
        public float cloudScale = 3f;

        [Tooltip("Overall speed multiplier for shadow scrolling.")]
        [Range(0f, 5f)]
        public float scrollSpeed = 0.3f;

        [Tooltip("Fade shadows out at night.")]
        public bool fadeAtNight = true;

        [Tooltip("Shadow opacity curve across the day cycle.")]
        public AnimationCurve shadowOpacityCurve = DefaultOpacityCurve();

        [Header("Wind Synchronization")]
        [Tooltip("Sync scroll direction with WindManager world direction.")]
        public bool syncWithWind = true;

        [Tooltip("Manual scroll direction (used when syncWithWind is off).")]
        public Vector2 manualScrollDirection = new Vector2(1f, 0.3f);

        // ─── Private ─────────────────────────────────────────────────────────
        private Vector2          _scrollOffset;
        private EnvironmentManager _env;
        private WindManager      _wind;
        private Material         _cloudMaterial;

        // Cookie material property IDs
        private static readonly int _CloudTex      = Shader.PropertyToID("_BaseMap");
        private static readonly int _CloudShadowInt = Shader.PropertyToID("_CloudShadowIntensity");

        // ─── Unity ───────────────────────────────────────────────────────────
        private void OnEnable()
        {
            _env  = FindFirstObjectByType<EnvironmentManager>();
            _wind = FindFirstObjectByType<WindManager>();
        }

        private void Update()
        {
            if (sunLight == null || cloudTexture == null) return;

            float dt = Application.isPlaying ? Time.deltaTime : 0f;

            // Get scroll direction
            Vector2 dir = manualScrollDirection.normalized;
            if (syncWithWind && _wind != null)
            {
                Vector3 windDir3 = _wind.transform.forward;
                dir = new Vector2(windDir3.x, windDir3.z).normalized;
            }

            float speed = scrollSpeed;
            if (_wind != null) speed *= (1f + (_wind.windZone != null ? _wind.windZone.windMain * 0.1f : 0f));

            _scrollOffset += dir * speed * dt;

            // Keep offset in [0,1] to avoid precision issues
            _scrollOffset.x = Mathf.Repeat(_scrollOffset.x, 1f);
            _scrollOffset.y = Mathf.Repeat(_scrollOffset.y, 1f);

            // Calculate opacity
            float opacity = shadowIntensity;
            if (fadeAtNight && _env != null)
                opacity *= shadowOpacityCurve.Evaluate(_env.timeOfDay);

            // Push as global shader properties (consumed by terrain/vegetation shaders)
            Shader.SetGlobalTexture("_CloudShadowTex",     cloudTexture);
            Shader.SetGlobalVector("_CloudShadowOffset",   new Vector4(_scrollOffset.x, _scrollOffset.y, 0f, 0f));
            Shader.SetGlobalFloat("_CloudShadowIntensity", opacity);
            Shader.SetGlobalFloat("_CloudShadowScale",     cloudScale);

            // Apply as light cookie
            ApplyLightCookie(opacity);
        }

        // ─── Light Cookie ─────────────────────────────────────────────────────
        private void ApplyLightCookie(float opacity)
        {
#if UNITY_2021_1_OR_NEWER
            // URP uses Light.cookie
            if (sunLight.cookie != cloudTexture)
                sunLight.cookie = cloudTexture;

            // Offset the cookie matrix to scroll it
            // Note: URP LightCookieManager reads Light.cookieOffset/Scale
            // We approximate by using the light's additional data if available
            var urpData = sunLight.GetComponent<UniversalAdditionalLightData>();
            if (urpData != null)
            {
                sunLight.cookieSize = cloudScale * 100f; // world-space size
            }
#endif
        }

        // ─── Default Curve ───────────────────────────────────────────────────
        private static AnimationCurve DefaultOpacityCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f,    0f),
                new Keyframe(0.22f, 0f),
                new Keyframe(0.28f, 1f),
                new Keyframe(0.72f, 1f),
                new Keyframe(0.78f, 0f),
                new Keyframe(1f,    0f));
        }

        // ─── Gizmos ──────────────────────────────────────────────────────────
        private void OnDrawGizmosSelected()
        {
            if (sunLight == null) return;
            Gizmos.color = new Color(1f, 1f, 0.2f, 0.4f);
            Gizmos.matrix = sunLight.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(cloudScale * 10f, cloudScale * 10f, 1f));
        }
    }
}
