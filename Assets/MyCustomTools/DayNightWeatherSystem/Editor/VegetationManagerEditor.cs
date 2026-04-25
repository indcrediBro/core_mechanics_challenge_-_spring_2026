#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DayNightWeather.Editor
{
    /// <summary>
    /// Custom Inspector for VegetationManager.
    /// Displays color-coded feature toggles, per-feature parameter groups,
    /// and a mini legend of the global shader properties that vegetation shaders should consume.
    /// </summary>
    [CustomEditor(typeof(VegetationManager))]
    public class VegetationManagerEditor : UnityEditor.Editor
    {
        // Foldout states
        private bool _foldTerrain  = true;
        private bool _foldSlope    = true;
        private bool _foldScale    = true;
        private bool _foldFade     = true;
        private bool _foldWind     = true;
        private bool _foldShaderRef= false;

        private static readonly string[] ShaderProperties = new[]
        {
            "_TerrainBlendStrength    – float  – terrain color blend amount",
            "_TerrainBlendFalloff     – float  – blend falloff sharpness",
            "_SlopeCorrectionAngle   – float  – radians of slope correction",
            "_VegetationScaleMin     – float  – minimum random scale",
            "_VegetationScaleMax     – float  – maximum random scale",
            "_VegetationScaleNoiseFreq – float – noise tile frequency",
            "_VegetationFadeStart    – float  – distance where fade begins",
            "_VegetationFadeEnd      – float  – distance where fully faded",
            "_SwayFreqPrimary        – float  – primary wind frequency",
            "_SwayFreqSecondary      – float  – secondary wind frequency",
            "_MaxSwayAngle           – float  – max sway angle (radians)",
            "_WindDirection          – float4 – global wind direction (from WindManager)",
            "_WindStrength           – float  – global wind strength",
            "_WindTime               – float  – global wind elapsed time",
            "_TerrainNormalMap       – Tex2D  – terrain normal map for blending",
            "_InstanceScaleVariation – float  – per-instance scale (MaterialPropertyBlock)",
            "_InstanceFadeAlpha      – float  – per-instance fade alpha (MaterialPropertyBlock)",
        };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var vm = (VegetationManager)target;

            DrawBanner();
            EditorGUILayout.Space(4);

            // Terrain reference
            DrawProp("terrain", "Terrain", "The scene terrain used for color and normal sampling.");
            EditorGUILayout.Space(4);

            // ── Terrain Color Blend ──────────────────────────────────────────
            DrawFeatureFoldout(ref _foldTerrain, "Terrain Color Blend", "enableTerrainBlend", new Color(0.4f, 0.9f, 0.5f));
            if (_foldTerrain)
            {
                EditorGUI.indentLevel++;
                DrawProp("terrainBlendStrength", "Blend Strength",
                    "How strongly vegetation base color is tinted toward the underlying terrain splat color.\n0 = no change, 1 = fully terrain color.");
                DrawProp("blendFalloff", "Blend Falloff",
                    "Controls how sharply the blend fades with slope angle. Higher values create a sharper transition.");
                EditorGUI.indentLevel--;
            }

            // ── Slope Correction ────────────────────────────────────────────
            DrawFeatureFoldout(ref _foldSlope, "Slope Correction", "enableSlopeCorrection", new Color(0.9f, 0.7f, 0.3f));
            if (_foldSlope)
            {
                EditorGUI.indentLevel++;
                DrawProp("slopeCorrectionAngle", "Max Correction Angle",
                    "Maximum degrees of normal tilt applied to vegetation normals on steep slopes,\nkeeping lighting from looking too dark on hillsides.");
                EditorGUI.indentLevel--;
            }

            // ── Scale Variation ─────────────────────────────────────────────
            DrawFeatureFoldout(ref _foldScale, "Scale Variation", "enableScaleVariation", new Color(0.4f, 0.8f, 1f));
            if (_foldScale)
            {
                EditorGUI.indentLevel++;
                DrawProp("scaleMin",           "Scale Min",        "Minimum scale multiplier driven by per-instance noise.");
                DrawProp("scaleMax",           "Scale Max",        "Maximum scale multiplier driven by per-instance noise.");
                DrawProp("scaleNoiseFrequency","Noise Frequency",  "World-space frequency of the noise controlling scale variation.");
                EditorGUI.indentLevel--;
            }

            // ── Distance Fade ───────────────────────────────────────────────
            DrawFeatureFoldout(ref _foldFade, "Distance Fade", "enableDistanceFade", new Color(1f, 0.5f, 0.5f));
            if (_foldFade)
            {
                EditorGUI.indentLevel++;
                DrawProp("fadeStartDistance", "Fade Start (m)", "Distance from camera at which vegetation begins to fade out.");
                DrawProp("fadeEndDistance",   "Fade End   (m)", "Distance from camera at which vegetation is fully transparent.");
                EditorGUI.indentLevel--;
            }

            // ── Wind Response ───────────────────────────────────────────────
            DrawFeatureFoldout(ref _foldWind, "Wind Response", null, new Color(0.7f, 0.7f, 1f));
            if (_foldWind)
            {
                EditorGUI.indentLevel++;
                DrawProp("primarySwayFrequency",  "Primary Freq",  "Frequency of primary sway oscillation (trunk / main stem).");
                DrawProp("secondarySwayFrequency","Secondary Freq","Frequency of secondary sway oscillation (leaves / branches).");
                DrawProp("maxSwayAngle",          "Max Sway Angle","Maximum sway in degrees at full wind strength.");
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(4);

            // Manual tracked renderers
            DrawProp("trackedRenderers", "Tracked Renderers",
                "Optional list of Renderer components that receive per-instance MaterialPropertyBlock overrides (scale variation and distance fade alpha).");

            EditorGUILayout.Space(6);

            // ── Shader Property Reference ───────────────────────────────────
            _foldShaderRef = EditorGUILayout.Foldout(_foldShaderRef, "Shader Property Reference", true, EditorStyles.foldoutHeader);
            if (_foldShaderRef)
            {
                EditorGUILayout.HelpBox(
                    "The following global shader properties are set each frame.\n" +
                    "Your vegetation shader must consume them for features to work:\n\n" +
                    string.Join("\n", ShaderProperties),
                    MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }

        // ─── Helpers ─────────────────────────────────────────────────────────
        private void DrawBanner()
        {
            var rect = EditorGUILayout.GetControlRect(false, 28);
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.18f, 0.12f, 0.85f));
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                normal   = { textColor = new Color(0.6f, 1f, 0.6f) }
            };
            EditorGUI.LabelField(rect, "  🌿 Vegetation Manager", style);
        }

        private void DrawFeatureFoldout(ref bool foldout, string label, string toggleProp, Color tint)
        {
            var rect = EditorGUILayout.GetControlRect(false, 22);
            EditorGUI.DrawRect(rect, tint * 0.15f);

            // Toggle
            float toggleX = rect.xMax - 20f;
            if (toggleProp != null)
            {
                var prop = serializedObject.FindProperty(toggleProp);
                if (prop != null)
                    prop.boolValue = EditorGUI.Toggle(new Rect(toggleX, rect.y + 3, 18, 18), prop.boolValue);
            }

            foldout = EditorGUI.Foldout(
                new Rect(rect.x, rect.y, toggleX - rect.x, rect.height),
                foldout, "  " + label, true,
                new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold,
                    normal = { textColor = tint } });

            EditorGUILayout.Space(1);
        }

        private void DrawProp(string propName, string displayName, string tooltip)
        {
            var prop = serializedObject.FindProperty(propName);
            if (prop != null)
                EditorGUILayout.PropertyField(prop, new GUIContent(displayName, tooltip));
        }
    }
}
#endif
