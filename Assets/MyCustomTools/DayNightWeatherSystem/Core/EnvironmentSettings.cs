using System;
using UnityEngine;

namespace DayNightWeather
{
    /// <summary>
    /// ScriptableObject that stores all environment settings for the Day/Night cycle.
    /// Create via: Assets > Create > Day Night System > Environment Settings
    /// </summary>
    [CreateAssetMenu(fileName = "EnvironmentSettings", menuName = "Day Night System/Environment Settings")]
    public class EnvironmentSettings : ScriptableObject
    {
        [Header("Time Settings")]
        [Tooltip("Duration of a full day/night cycle in real-world seconds.")]
        public float dayDurationSeconds = 120f;

        [Tooltip("Starting time of day (0 = midnight, 0.25 = 6AM, 0.5 = noon, 0.75 = 6PM).")]
        [Range(0f, 1f)]
        public float startTimeNormalized = 0.25f;

        // ─── Sun (Main Light) ─────────────────────────────────────────────────
        [Header("Sun Settings")]
        [Tooltip("Gradient controlling sun light color over the full day cycle.")]
        public Gradient sunColor = DefaultSunGradient();

        [Tooltip("Curve controlling sun light intensity (0–1 normalized) over the day cycle.")]
        public AnimationCurve sunIntensity = DefaultSunIntensityCurve();

        [Tooltip("Maximum intensity of the sun light at peak noon.")]
        public float sunMaxIntensity = 3.14f;

        // ─── Moon ─────────────────────────────────────────────────────────────
        [Header("Moon Settings")]
        [Tooltip("Gradient controlling moon light color over the night cycle.")]
        public Gradient moonColor = DefaultMoonGradient();

        [Tooltip("Curve controlling moon light intensity over the night cycle.")]
        public AnimationCurve moonIntensity = DefaultMoonIntensityCurve();

        [Tooltip("Maximum intensity of the moon light at peak night.")]
        public float moonMaxIntensity = 0.3f;

        // ─── Ambient Light ────────────────────────────────────────────────────
        [Header("Ambient Light")]
        [Tooltip("Gradient of sky ambient color across the day cycle.")]
        public Gradient ambientSkyColor = DefaultAmbientSkyGradient();

        [Tooltip("Gradient of equator ambient color across the day cycle.")]
        public Gradient ambientEquatorColor = DefaultAmbientEquatorGradient();

        [Tooltip("Gradient of ground ambient color across the day cycle.")]
        public Gradient ambientGroundColor = DefaultAmbientGroundGradient();

        [Tooltip("Curve controlling ambient intensity multiplier across the day cycle.")]
        public AnimationCurve ambientIntensity = DefaultAmbientIntensityCurve();

        [Tooltip("Maximum ambient intensity.")]
        public float ambientMaxIntensity = 1.2f;

        // ─── Fog ──────────────────────────────────────────────────────────────
        [Header("Fog")]
        [Tooltip("Whether fog is controlled by the environment system.")]
        public bool controlFog = true;

        [Tooltip("Gradient of fog color across the day cycle.")]
        public Gradient fogColor = DefaultFogGradient();

        [Tooltip("Curve controlling fog density across the day cycle.")]
        public AnimationCurve fogDensity = DefaultFogDensityCurve();

        [Tooltip("Maximum fog density at night.")]
        public float fogMaxDensity = 0.02f;

        [Tooltip("Fog mode used by the scene.")]
        public FogMode fogMode = FogMode.Exponential;

        // ─── Skybox ───────────────────────────────────────────────────────────
        [Header("Skybox")]
        [Tooltip("Gradient for the sky top color across the day cycle.")]
        public Gradient skyTopColor = DefaultSkyTopGradient();

        [Tooltip("Gradient for the sky horizon color across the day cycle.")]
        public Gradient skyHorizonColor = DefaultSkyHorizonGradient();

        [Tooltip("Gradient for the sky bottom (ground) color across the day cycle.")]
        public Gradient skyBottomColor = DefaultSkyBottomGradient();

        [Tooltip("Curve controlling star visibility — fully visible only at night.")]
        public AnimationCurve starVisibility = DefaultStarVisibilityCurve();

        [Tooltip("Maximum star brightness.")]
        [Range(0f, 2f)]
        public float starMaxBrightness = 1f;

        // ─── Clouds ───────────────────────────────────────────────────────────
        [Header("Clouds")]
        [Tooltip("Gradient of cloud color across the day cycle.")]
        public Gradient cloudColor = DefaultCloudGradient();

        [Tooltip("Cloud coverage (0 = clear, 1 = overcast).")]
        [Range(0f, 1f)]
        public float cloudCoverage = 0.4f;

        [Tooltip("Speed at which clouds move across the sky.")]
        public float cloudSpeed = 0.005f;

        [Tooltip("Cloud detail / softness.")]
        [Range(0f, 1f)]
        public float cloudSoftness = 0.5f;

        // ─── Time-of-Day Thresholds ───────────────────────────────────────────
        [Header("Time-of-Day Thresholds (0–1)")]
        [Tooltip("Normalized time when sunrise begins.")]
        [Range(0f, 1f)] public float sunriseStart = 0.2f;

        [Tooltip("Normalized time when sunrise ends / full day begins.")]
        [Range(0f, 1f)] public float sunriseEnd = 0.28f;

        [Tooltip("Normalized time when sunset begins.")]
        [Range(0f, 1f)] public float sunsetStart = 0.72f;

        [Tooltip("Normalized time when sunset ends / night begins.")]
        [Range(0f, 1f)] public float sunsetEnd = 0.80f;

        // ─── Default Gradient / Curve Factories ──────────────────────────────
        private static Gradient DefaultSunGradient()
        {
            var g = new Gradient();
            g.SetKeys(
                new[] {
                    new GradientColorKey(new Color(1f, 0.5f, 0.1f), 0.20f),
                    new GradientColorKey(new Color(1f, 0.95f, 0.8f), 0.28f),
                    new GradientColorKey(new Color(1f, 0.98f, 0.9f), 0.50f),
                    new GradientColorKey(new Color(1f, 0.7f, 0.2f), 0.75f),
                    new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 0.82f),
                },
                new[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                });
            return g;
        }

        private static Gradient DefaultMoonGradient()
        {
            var g = new Gradient();
            g.SetKeys(
                new[] {
                    new GradientColorKey(new Color(0.7f, 0.8f, 1f), 0f),
                    new GradientColorKey(new Color(0.8f, 0.9f, 1f), 0.5f),
                    new GradientColorKey(new Color(0.7f, 0.8f, 1f), 1f),
                },
                new[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                });
            return g;
        }

        private static Gradient DefaultAmbientSkyGradient()
        {
            var g = new Gradient();
            g.SetKeys(
                new[] {
                    new GradientColorKey(new Color(0.05f, 0.05f, 0.15f), 0f),
                    new GradientColorKey(new Color(0.6f, 0.4f, 0.2f), 0.22f),
                    new GradientColorKey(new Color(0.5f, 0.7f, 1f),    0.30f),
                    new GradientColorKey(new Color(0.4f, 0.6f, 0.9f),  0.50f),
                    new GradientColorKey(new Color(0.6f, 0.3f, 0.15f), 0.75f),
                    new GradientColorKey(new Color(0.05f, 0.05f, 0.15f), 1f),
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return g;
        }

        private static Gradient DefaultAmbientEquatorGradient()
        {
            var g = new Gradient();
            g.SetKeys(
                new[] {
                    new GradientColorKey(new Color(0.08f, 0.06f, 0.12f), 0f),
                    new GradientColorKey(new Color(0.8f, 0.5f, 0.2f),   0.25f),
                    new GradientColorKey(new Color(0.7f, 0.85f, 1f),    0.50f),
                    new GradientColorKey(new Color(0.8f, 0.4f, 0.1f),   0.75f),
                    new GradientColorKey(new Color(0.08f, 0.06f, 0.12f), 1f),
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return g;
        }

        private static Gradient DefaultAmbientGroundGradient()
        {
            var g = new Gradient();
            g.SetKeys(
                new[] {
                    new GradientColorKey(new Color(0.03f, 0.04f, 0.06f), 0f),
                    new GradientColorKey(new Color(0.15f, 0.1f, 0.05f),  0.25f),
                    new GradientColorKey(new Color(0.2f, 0.18f, 0.12f),  0.50f),
                    new GradientColorKey(new Color(0.12f, 0.08f, 0.04f), 0.75f),
                    new GradientColorKey(new Color(0.03f, 0.04f, 0.06f), 1f),
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return g;
        }

        private static Gradient DefaultFogGradient()
        {
            var g = new Gradient();
            g.SetKeys(
                new[] {
                    new GradientColorKey(new Color(0.05f, 0.06f, 0.15f), 0f),
                    new GradientColorKey(new Color(0.9f, 0.6f, 0.3f),   0.23f),
                    new GradientColorKey(new Color(0.8f, 0.9f, 1f),     0.50f),
                    new GradientColorKey(new Color(0.9f, 0.5f, 0.2f),   0.75f),
                    new GradientColorKey(new Color(0.05f, 0.06f, 0.15f), 1f),
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return g;
        }

        private static Gradient DefaultSkyTopGradient()
        {
            var g = new Gradient();
            g.SetKeys(
                new[] {
                    new GradientColorKey(new Color(0.02f, 0.02f, 0.08f), 0f),
                    new GradientColorKey(new Color(0.1f, 0.15f, 0.5f),  0.22f),
                    new GradientColorKey(new Color(0.1f, 0.4f, 0.9f),   0.30f),
                    new GradientColorKey(new Color(0.05f, 0.3f, 0.85f), 0.50f),
                    new GradientColorKey(new Color(0.15f, 0.2f, 0.6f),  0.75f),
                    new GradientColorKey(new Color(0.02f, 0.02f, 0.08f), 1f),
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return g;
        }

        private static Gradient DefaultSkyHorizonGradient()
        {
            var g = new Gradient();
            g.SetKeys(
                new[] {
                    new GradientColorKey(new Color(0.05f, 0.06f, 0.2f),  0f),
                    new GradientColorKey(new Color(1f, 0.5f, 0.1f),     0.22f),
                    new GradientColorKey(new Color(0.6f, 0.8f, 1f),     0.30f),
                    new GradientColorKey(new Color(0.5f, 0.75f, 1f),    0.50f),
                    new GradientColorKey(new Color(1f, 0.4f, 0.08f),    0.75f),
                    new GradientColorKey(new Color(0.05f, 0.06f, 0.2f),  1f),
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return g;
        }

        private static Gradient DefaultSkyBottomGradient()
        {
            var g = new Gradient();
            g.SetKeys(
                new[] {
                    new GradientColorKey(new Color(0.02f, 0.03f, 0.08f), 0f),
                    new GradientColorKey(new Color(0.15f, 0.12f, 0.1f), 0.30f),
                    new GradientColorKey(new Color(0.18f, 0.16f, 0.12f), 0.50f),
                    new GradientColorKey(new Color(0.12f, 0.1f, 0.08f), 0.75f),
                    new GradientColorKey(new Color(0.02f, 0.03f, 0.08f), 1f),
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return g;
        }

        private static Gradient DefaultCloudGradient()
        {
            var g = new Gradient();
            g.SetKeys(
                new[] {
                    new GradientColorKey(new Color(0.2f, 0.2f, 0.3f),  0f),
                    new GradientColorKey(new Color(1f, 0.6f, 0.3f),   0.22f),
                    new GradientColorKey(Color.white,                  0.30f),
                    new GradientColorKey(new Color(0.95f, 0.97f, 1f), 0.50f),
                    new GradientColorKey(new Color(1f, 0.5f, 0.2f),   0.75f),
                    new GradientColorKey(new Color(0.2f, 0.2f, 0.3f),  1f),
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return g;
        }

        private static AnimationCurve DefaultSunIntensityCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f,    0f),
                new Keyframe(0.20f, 0f),
                new Keyframe(0.25f, 0.2f),
                new Keyframe(0.50f, 1f),
                new Keyframe(0.75f, 0.2f),
                new Keyframe(0.80f, 0f),
                new Keyframe(1f,    0f));
        }

        private static AnimationCurve DefaultMoonIntensityCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f,    1f),
                new Keyframe(0.20f, 0.5f),
                new Keyframe(0.25f, 0f),
                new Keyframe(0.75f, 0f),
                new Keyframe(0.80f, 0.5f),
                new Keyframe(1f,    1f));
        }

        private static AnimationCurve DefaultAmbientIntensityCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f,    0.2f),
                new Keyframe(0.25f, 0.5f),
                new Keyframe(0.50f, 1f),
                new Keyframe(0.75f, 0.5f),
                new Keyframe(1f,    0.2f));
        }

        private static AnimationCurve DefaultFogDensityCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f,    1f),
                new Keyframe(0.25f, 0.5f),
                new Keyframe(0.50f, 0.3f),
                new Keyframe(0.75f, 0.5f),
                new Keyframe(1f,    1f));
        }

        private static AnimationCurve DefaultStarVisibilityCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f,    1f),
                new Keyframe(0.22f, 0.5f),
                new Keyframe(0.28f, 0f),
                new Keyframe(0.72f, 0f),
                new Keyframe(0.78f, 0.5f),
                new Keyframe(1f,    1f));
        }
    }
}
