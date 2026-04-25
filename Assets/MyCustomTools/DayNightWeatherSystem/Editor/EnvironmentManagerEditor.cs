#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DayNightWeather.Editor
{
    /// <summary>
    /// Custom Inspector for EnvironmentManager.
    /// Features:
    ///   - Time scrubber with real-time preview.
    ///   - Phase indicator strip.
    ///   - Quick-jump buttons (Dawn / Noon / Dusk / Midnight).
    ///   - Live clock display.
    ///   - All fields organized in foldout groups with rich tooltips.
    /// </summary>
    [CustomEditor(typeof(EnvironmentManager))]
    public class EnvironmentManagerEditor : UnityEditor.Editor
    {
        // Serialized properties
        private SerializedProperty _settings;
        private SerializedProperty _sunLight;
        private SerializedProperty _moonLight;
        private SerializedProperty _skyboxMaterial;
        private SerializedProperty _running;
        private SerializedProperty _timeOfDay;
        private SerializedProperty _timeScale;

        // Foldout states
        private bool _showRefs    = true;
        private bool _showRuntime = true;
        private bool _showInfo    = true;

        // Styles
        private GUIStyle _headerStyle;
        private GUIStyle _phaseStyle;
        private GUIStyle _clockStyle;
        private bool     _stylesReady;

        // ─── Enable ──────────────────────────────────────────────────────────
        private void OnEnable()
        {
            _settings       = serializedObject.FindProperty("settings");
            _sunLight       = serializedObject.FindProperty("sunLight");
            _moonLight      = serializedObject.FindProperty("moonLight");
            _skyboxMaterial = serializedObject.FindProperty("skyboxMaterial");
            _running        = serializedObject.FindProperty("running");
            _timeOfDay      = serializedObject.FindProperty("timeOfDay");
            _timeScale      = serializedObject.FindProperty("timeScale");
        }

        // ─── Styles ──────────────────────────────────────────────────────────
        private void EnsureStyles()
        {
            if (_stylesReady) return;
            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                normal   = { textColor = new Color(0.9f, 0.85f, 0.6f) }
            };
            _phaseStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            _clockStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 22,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = new Color(1f, 0.95f, 0.7f) }
            };
            _stylesReady = true;
        }

        // ─── OnInspectorGUI ──────────────────────────────────────────────────
        public override void OnInspectorGUI()
        {
            EnsureStyles();
            serializedObject.Update();

            var env = (EnvironmentManager)target;

            DrawBanner();
            EditorGUILayout.Space(4);

            // ── Settings Asset ───────────────────────────────────────────────
            EditorGUILayout.PropertyField(_settings, new GUIContent(
                "Environment Settings",
                "ScriptableObject containing all day/night cycle parameters.\n" +
                "Create via: Assets > Create > Day Night System > Environment Settings."));

            EditorGUILayout.Space(4);

            // ── Scene References ─────────────────────────────────────────────
            _showRefs = EditorGUILayout.BeginFoldoutHeaderGroup(_showRefs, "Scene References");
            if (_showRefs)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_sunLight,       new GUIContent("Sun Light",       "Directional light used as the sun."));
                EditorGUILayout.PropertyField(_moonLight,      new GUIContent("Moon Light",       "Directional light used as the moon (also needs a MoonLightController)."));
                EditorGUILayout.PropertyField(_skyboxMaterial, new GUIContent("Skybox Material", "Material using the ProceduralSkybox shader."));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(4);

            // ── Runtime Control ──────────────────────────────────────────────
            _showRuntime = EditorGUILayout.BeginFoldoutHeaderGroup(_showRuntime, "Runtime Control");
            if (_showRuntime)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_running,   new GUIContent("Running",    "Advance time automatically in Play Mode."));
                EditorGUILayout.PropertyField(_timeScale, new GUIContent("Time Scale", "Multiplier on the passage of time (1 = normal)."));
                EditorGUILayout.Space(2);

                // Time scrubber
                EditorGUI.BeginChangeCheck();
                float newTime = EditorGUILayout.Slider(
                    new GUIContent("Time of Day", "Normalized time: 0 = midnight, 0.25 = 6AM, 0.5 = noon, 0.75 = 6PM."),
                    _timeOfDay.floatValue, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    _timeOfDay.floatValue = newTime;
                    if (!Application.isPlaying) env.Apply(newTime);
                }

                // Quick-jump buttons
                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField("Jump To:", EditorStyles.miniLabel);
                EditorGUILayout.BeginHorizontal();
                if (QuickButton("🌅 Dawn",     new Color(0.9f, 0.6f, 0.3f))) SetTime(env, 0.22f);
                if (QuickButton("☀ Noon",      new Color(1f, 0.95f, 0.6f)))  SetTime(env, 0.50f);
                if (QuickButton("🌇 Dusk",     new Color(0.9f, 0.5f, 0.2f))) SetTime(env, 0.74f);
                if (QuickButton("🌙 Midnight", new Color(0.3f, 0.35f, 0.6f))) SetTime(env, 0f);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(6);

            // ── Live Info ────────────────────────────────────────────────────
            _showInfo = EditorGUILayout.BeginFoldoutHeaderGroup(_showInfo, "Live Info");
            if (_showInfo)
            {
                DrawClockAndPhase(env);
                DrawPhaseBar(env);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();

            // Force repaint in edit mode so the time slider stays live
            if (!Application.isPlaying)
                Repaint();
        }

        // ─── Banner ──────────────────────────────────────────────────────────
        private void DrawBanner()
        {
            var rect = EditorGUILayout.GetControlRect(false, 28);
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.18f, 0.8f));
            EditorGUI.LabelField(rect, "  ☀ Day / Night Weather System  —  Environment Manager", _headerStyle);
        }

        // ─── Clock & Phase ───────────────────────────────────────────────────
        private void DrawClockAndPhase(EnvironmentManager env)
        {
            EditorGUILayout.Space(4);

            // Clock
            string timeStr = env.GetTimeString();
            var clockRect  = EditorGUILayout.GetControlRect(false, 36);
            EditorGUI.DrawRect(clockRect, new Color(0.08f, 0.08f, 0.14f));
            EditorGUI.LabelField(clockRect, timeStr, _clockStyle);

            // Phase label
            string phase    = env.CurrentPhase.ToString();
            Color  phaseCol = PhaseColor(env.CurrentPhase);
            var phaseRect   = EditorGUILayout.GetControlRect(false, 20);
            EditorGUI.DrawRect(phaseRect, phaseCol * 0.4f);
            _phaseStyle.normal.textColor = phaseCol;
            EditorGUI.LabelField(phaseRect, phase.ToUpper(), _phaseStyle);

            EditorGUILayout.Space(4);

            // Blend weight bars
            DrawWeightBar("Day",     env.DayWeight,     new Color(1f, 0.95f, 0.5f));
            DrawWeightBar("Sunset",  env.SunsetWeight,  new Color(1f, 0.5f, 0.1f));
            DrawWeightBar("Night",   env.NightWeight,   new Color(0.3f, 0.3f, 0.7f));
            DrawWeightBar("Sunrise", env.SunriseWeight, new Color(0.8f, 0.5f, 0.3f));
        }

        // ─── Phase Timeline Bar ──────────────────────────────────────────────
        private void DrawPhaseBar(EnvironmentManager env)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("24-Hour Timeline", EditorStyles.miniLabel);

            var bar = EditorGUILayout.GetControlRect(false, 16);
            float w = bar.width;

            // Background
            EditorGUI.DrawRect(bar, new Color(0.05f, 0.05f, 0.15f));

            // Night segments (before sunrise and after sunset)
            var s = env.settings;
            if (s != null)
            {
                DrawBarSegment(bar, 0f,               s.sunriseStart, new Color(0.2f, 0.2f, 0.5f));
                DrawBarSegment(bar, s.sunriseStart,   s.sunriseEnd,   new Color(0.8f, 0.4f, 0.2f));
                DrawBarSegment(bar, s.sunriseEnd,     s.sunsetStart,  new Color(0.9f, 0.85f, 0.4f));
                DrawBarSegment(bar, s.sunsetStart,    s.sunsetEnd,    new Color(0.8f, 0.3f, 0.1f));
                DrawBarSegment(bar, s.sunsetEnd,      1f,             new Color(0.2f, 0.2f, 0.5f));
            }

            // Current time cursor
            float cursorX = bar.x + env.timeOfDay * w;
            EditorGUI.DrawRect(new Rect(cursorX - 1f, bar.y, 2f, bar.height), Color.white);

            EditorGUILayout.Space(2);

            // Hour labels
            var labelsRect = EditorGUILayout.GetControlRect(false, 12);
            for (int h = 0; h <= 24; h += 6)
            {
                float nx = (float)h / 24f;
                float lx = labelsRect.x + nx * labelsRect.width - 8f;
                EditorGUI.LabelField(new Rect(lx, labelsRect.y, 24f, 12f),
                    $"{h:00}", EditorStyles.centeredGreyMiniLabel);
            }
        }

        // ─── Helper Draws ────────────────────────────────────────────────────
        private void DrawWeightBar(string label, float value, Color color)
        {
            var row = EditorGUILayout.GetControlRect(false, 14);
            float labelW = 60f;
            // Label
            EditorGUI.LabelField(new Rect(row.x, row.y, labelW, row.height), label, EditorStyles.miniLabel);
            // Background
            var barRect = new Rect(row.x + labelW, row.y + 2, row.width - labelW - 35f, 10f);
            EditorGUI.DrawRect(barRect, new Color(0.1f, 0.1f, 0.1f));
            // Fill
            EditorGUI.DrawRect(new Rect(barRect.x, barRect.y, barRect.width * value, barRect.height), color);
            // Value text
            EditorGUI.LabelField(new Rect(row.xMax - 35f, row.y, 35f, row.height),
                $"{value:F2}", EditorStyles.miniLabel);
        }

        private void DrawBarSegment(Rect bar, float t0, float t1, Color color)
        {
            var seg = new Rect(bar.x + t0 * bar.width, bar.y, (t1 - t0) * bar.width, bar.height);
            EditorGUI.DrawRect(seg, color);
        }

        private bool QuickButton(string label, Color tint)
        {
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = tint;
            bool clicked = GUILayout.Button(label, EditorStyles.miniButton);
            GUI.backgroundColor = prev;
            return clicked;
        }

        private void SetTime(EnvironmentManager env, float t)
        {
            Undo.RecordObject(env, "Set Time of Day");
            env.timeOfDay = t;
            env.Apply(t);
            EditorUtility.SetDirty(env);
        }

        private Color PhaseColor(TimeOfDayPhase phase)
        {
            return phase switch
            {
                TimeOfDayPhase.Day     => new Color(1f, 0.95f, 0.5f),
                TimeOfDayPhase.Sunset  => new Color(1f, 0.5f, 0.1f),
                TimeOfDayPhase.Night   => new Color(0.5f, 0.55f, 1f),
                TimeOfDayPhase.Sunrise => new Color(0.9f, 0.6f, 0.3f),
                _                      => Color.white
            };
        }
    }
}
#endif
