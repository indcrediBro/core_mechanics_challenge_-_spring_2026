using System.Collections.Generic;
using UnityEngine;

namespace DayNightWeather
{
    /// <summary>
    /// Manages vegetation shading features:
    ///   1. Terrain Color Blend  — tints vegetation base color toward terrain splat color.
    ///   2. Slope Correction     — rotates vegetation normals toward vertical on steep slopes.
    ///   3. Scale Variation      — randomizes height/width per-instance using a gradient noise.
    ///   4. Distance Fade        — smoothly fades vegetation alpha at a configurable range.
    ///
    /// All features are applied as global shader properties so any custom vegetation
    /// shader (or the provided VegetationLit URP shader graph) can consume them.
    ///
    /// Register vegetation renderers via RegisterRenderer() or drag them into the
    /// trackedRenderers list in the Inspector for manual control.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("Day Night System/Vegetation Manager")]
    public class VegetationManager : MonoBehaviour
    {
        // ─── Inspector ───────────────────────────────────────────────────────
        [Header("Terrain Reference")]
        [Tooltip("The terrain whose splat colors are sampled for vegetation blending.")]
        public Terrain terrain;

        [Header("Terrain Color Blend")]
        [Tooltip("Enable blending vegetation base color toward the underlying terrain color.")]
        public bool enableTerrainBlend = true;

        [Tooltip("Blend strength: 0 = vegetation color unchanged, 1 = fully terrain color.")]
        [Range(0f, 1f)]
        public float terrainBlendStrength = 0.3f;

        [Tooltip("Sharpness of the blend falloff with slope angle.")]
        [Range(0f, 5f)]
        public float blendFalloff = 1.5f;

        [Header("Slope Correction")]
        [Tooltip("Tilt vegetation normals toward up-axis to counteract steep terrain slopes.")]
        public bool enableSlopeCorrection = true;

        [Tooltip("Maximum tilt correction applied in degrees.")]
        [Range(0f, 45f)]
        public float slopeCorrectionAngle = 15f;

        [Header("Scale Variation")]
        [Tooltip("Apply per-instance random scale variation driven by noise.")]
        public bool enableScaleVariation = true;

        [Tooltip("Minimum scale multiplier.")]
        [Range(0.1f, 1f)]
        public float scaleMin = 0.7f;

        [Tooltip("Maximum scale multiplier.")]
        [Range(1f, 3f)]
        public float scaleMax = 1.3f;

        [Tooltip("Noise tile frequency controlling how fine the scale variation pattern is.")]
        [Range(0.001f, 1f)]
        public float scaleNoiseFrequency = 0.05f;

        [Header("Distance Fade")]
        [Tooltip("Enable alpha fade for vegetation beyond a certain distance.")]
        public bool enableDistanceFade = true;

        [Tooltip("Distance at which fade begins (world units).")]
        [Min(1f)]
        public float fadeStartDistance = 40f;

        [Tooltip("Distance at which vegetation is fully invisible.")]
        [Min(2f)]
        public float fadeEndDistance = 60f;

        [Header("Wind Response")]
        [Tooltip("Primary wind sway frequency for the trunk/main stem.")]
        [Range(0f, 5f)]
        public float primarySwayFrequency = 1.2f;

        [Tooltip("Secondary wind sway frequency for leaves/branches.")]
        [Range(0f, 10f)]
        public float secondarySwayFrequency = 3.5f;

        [Tooltip("Maximum sway angle in degrees.")]
        [Range(0f, 30f)]
        public float maxSwayAngle = 8f;

        [Header("Manual Tracked Renderers")]
        [Tooltip("Optionally list renderers to apply per-object material property blocks to.")]
        public List<Renderer> trackedRenderers = new List<Renderer>();

        // ─── Shader Property IDs ──────────────────────────────────────────────
        private static readonly int _TerrainBlend      = Shader.PropertyToID("_TerrainBlendStrength");
        private static readonly int _TerrainBlendFalloff= Shader.PropertyToID("_TerrainBlendFalloff");
        private static readonly int _SlopeCorrection   = Shader.PropertyToID("_SlopeCorrectionAngle");
        private static readonly int _ScaleMin          = Shader.PropertyToID("_VegetationScaleMin");
        private static readonly int _ScaleMax          = Shader.PropertyToID("_VegetationScaleMax");
        private static readonly int _ScaleNoiseFreq    = Shader.PropertyToID("_VegetationScaleNoiseFreq");
        private static readonly int _FadeStart         = Shader.PropertyToID("_VegetationFadeStart");
        private static readonly int _FadeEnd           = Shader.PropertyToID("_VegetationFadeEnd");
        private static readonly int _SwayFreqPrimary   = Shader.PropertyToID("_SwayFreqPrimary");
        private static readonly int _SwayFreqSecondary = Shader.PropertyToID("_SwayFreqSecondary");
        private static readonly int _MaxSwayAngle      = Shader.PropertyToID("_MaxSwayAngle");
        private static readonly int _TerrainNormalMap  = Shader.PropertyToID("_TerrainNormalMap");

        // ─── Private ─────────────────────────────────────────────────────────
        private MaterialPropertyBlock _mpb;
        private Camera               _mainCam;

        // ─── Unity ───────────────────────────────────────────────────────────
        private void OnEnable()
        {
            _mpb = new MaterialPropertyBlock();
            _mainCam = Camera.main;
        }

        private void Update()
        {
            PushGlobalProperties();
            if (trackedRenderers.Count > 0)
                UpdateTrackedRenderers();
        }

        // ─── Global Shader Push ───────────────────────────────────────────────
        private void PushGlobalProperties()
        {
            // Terrain color blend
            Shader.SetGlobalFloat(_TerrainBlend,       enableTerrainBlend       ? terrainBlendStrength : 0f);
            Shader.SetGlobalFloat(_TerrainBlendFalloff, blendFalloff);

            // Slope correction
            Shader.SetGlobalFloat(_SlopeCorrection,    enableSlopeCorrection    ? slopeCorrectionAngle * Mathf.Deg2Rad : 0f);

            // Scale variation
            Shader.SetGlobalFloat(_ScaleMin,           enableScaleVariation     ? scaleMin : 1f);
            Shader.SetGlobalFloat(_ScaleMax,           enableScaleVariation     ? scaleMax : 1f);
            Shader.SetGlobalFloat(_ScaleNoiseFreq,     scaleNoiseFrequency);

            // Distance fade
            Shader.SetGlobalFloat(_FadeStart,          enableDistanceFade       ? fadeStartDistance : 9999f);
            Shader.SetGlobalFloat(_FadeEnd,            enableDistanceFade       ? fadeEndDistance   : 10000f);

            // Wind sway
            Shader.SetGlobalFloat(_SwayFreqPrimary,    primarySwayFrequency);
            Shader.SetGlobalFloat(_SwayFreqSecondary,  secondarySwayFrequency);
            Shader.SetGlobalFloat(_MaxSwayAngle,       maxSwayAngle * Mathf.Deg2Rad);

            // Terrain normal map for blending
            if (terrain != null)
            {
                var normalMap = terrain.normalmapTexture;
                if (normalMap != null)
                    Shader.SetGlobalTexture(_TerrainNormalMap, normalMap);
            }
        }

        // ─── Per-Renderer MPB ────────────────────────────────────────────────
        private void UpdateTrackedRenderers()
        {
            if (_mainCam == null) _mainCam = Camera.main;

            foreach (var r in trackedRenderers)
            {
                if (r == null) continue;

                // Per-object scale variation seed (stable by world position)
                Vector3 pos = r.transform.position;
                float seed = Mathf.PerlinNoise(pos.x * scaleNoiseFrequency, pos.z * scaleNoiseFrequency);
                float scale = Mathf.Lerp(scaleMin, scaleMax, seed);

                // Distance fade alpha
                float alpha = 1f;
                if (enableDistanceFade && _mainCam != null)
                {
                    float dist = Vector3.Distance(_mainCam.transform.position, pos);
                    alpha = 1f - Mathf.InverseLerp(fadeStartDistance, fadeEndDistance, dist);
                }

                r.GetPropertyBlock(_mpb);
                _mpb.SetFloat("_InstanceScaleVariation", scale);
                _mpb.SetFloat("_InstanceFadeAlpha",      alpha);
                r.SetPropertyBlock(_mpb);
            }
        }

        // ─── Public API ──────────────────────────────────────────────────────
        /// <summary>Register a renderer to receive per-instance property overrides.</summary>
        public void RegisterRenderer(Renderer r)
        {
            if (r != null && !trackedRenderers.Contains(r))
                trackedRenderers.Add(r);
        }

        /// <summary>Unregister a renderer.</summary>
        public void UnregisterRenderer(Renderer r)
        {
            trackedRenderers.Remove(r);
        }

        // ─── Gizmos ──────────────────────────────────────────────────────────
        private void OnDrawGizmosSelected()
        {
            if (!enableDistanceFade) return;
            Gizmos.color = new Color(0.4f, 1f, 0.4f, 0.15f);
            if (_mainCam == null) _mainCam = Camera.main;
            if (_mainCam == null) return;
            Gizmos.DrawWireSphere(_mainCam.transform.position, fadeStartDistance);
            Gizmos.color = new Color(1f, 0.4f, 0.4f, 0.1f);
            Gizmos.DrawWireSphere(_mainCam.transform.position, fadeEndDistance);
        }
    }
}
