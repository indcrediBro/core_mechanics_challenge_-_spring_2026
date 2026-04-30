// ============================================================
//  SettingsArrowSelectorEditor.cs
//  MUST be inside an Editor/ folder.
//  e.g. Assets/Core.Settings/Editor/SettingsArrowSelectorEditor.cs
// ============================================================

using UnityEditor;
using UnityEngine;

namespace Core.Settings
{
    [CustomEditor(typeof(SettingsArrowSelector))]
    public class SettingsArrowSelectorEditor : Editor
    {
        // public override void OnInspectorGUI()
        // {
        //     serializedObject.Update();
        //
        //     // ── Binding ────────────────────────────────────────────────────
        //     var typeProp     = serializedObject.FindProperty("Type");
        //     var intPathProp  = serializedObject.FindProperty("IntPath");
        //     var boolPathProp = serializedObject.FindProperty("BoolPath");
        //     var stringPathProp = serializedObject.FindProperty("StringPath");
        //
        //     EditorGUILayout.LabelField("Binding", EditorStyles.boldLabel);
        //     EditorGUILayout.PropertyField(typeProp);
        //
        //     var bindingType = (SettingsArrowSelector.BindingType)typeProp.enumValueIndex;
        //
        //     switch (bindingType)
        //     {
        //         case SettingsArrowSelector.BindingType.Int:
        //             EditorGUILayout.PropertyField(intPathProp);
        //             break;
        //         case SettingsArrowSelector.BindingType.Bool:
        //             break;
        //         case SettingsArrowSelector.BindingType.String:
        //             EditorGUILayout.PropertyField(stringPathProp);
        //             break;
        //         default:
        //             EditorGUILayout.PropertyField(boolPathProp);
        //             break;
        //     }
        //
        //     EditorGUILayout.Space(6);
        //
        //     // ── Options ────────────────────────────────────────────────────
        //     EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
        //
        //     var autoQuality    = serializedObject.FindProperty("AutoPopulateQualityLevels");
        //     var autoAA         = serializedObject.FindProperty("AutoPopulateAntiAliasing");
        //     var autoDifficulty = serializedObject.FindProperty("AutoPopulateDifficulty");
        //     var autoBool       = serializedObject.FindProperty("AutoPopulateBool");
        //     var optionsProp    = serializedObject.FindProperty("Options");
        //
        //     if (bindingType == SettingsArrowSelector.BindingType.Int)
        //     {
        //         // Show int-relevant auto-populate flags
        //         EditorGUILayout.PropertyField(autoQuality,    new GUIContent("Auto: Quality Levels"));
        //         EditorGUILayout.PropertyField(autoAA,         new GUIContent("Auto: Anti-Aliasing"));
        //         EditorGUILayout.PropertyField(autoDifficulty, new GUIContent("Auto: Difficulty"));
        //
        //         // Only show manual Options list when no auto-flag is ticked
        //         bool anyAuto = autoQuality.boolValue || autoAA.boolValue || autoDifficulty.boolValue;
        //         if (!anyAuto)
        //         {
        //             EditorGUILayout.Space(2);
        //             EditorGUILayout.HelpBox(
        //                 "No AutoPopulate flag selected.\n" +
        //                 "Fill Options[] manually — index 0 = stored value 0, index 1 = stored value 1, etc.",
        //                 MessageType.Info);
        //             EditorGUILayout.PropertyField(optionsProp, true);
        //         }
        //         else
        //         {
        //             // Hide manual options — auto-populate owns them
        //             EditorGUI.BeginDisabledGroup(true);
        //             EditorGUILayout.PropertyField(optionsProp, new GUIContent("Options (auto-filled)"), true);
        //             EditorGUI.EndDisabledGroup();
        //         }
        //     }
        //     else
        //     {
        //         // Bool mode
        //         EditorGUILayout.PropertyField(autoBool, new GUIContent("Auto: Off / On labels"));
        //
        //         if (!autoBool.boolValue || optionsProp.arraySize > 0)
        //         {
        //             EditorGUILayout.HelpBox(
        //                 "Index 0 = false (Off), Index 1 = true (On).\n" +
        //                 "Override the labels here, e.g. \"Disabled\" / \"Enabled\".",
        //                 MessageType.Info);
        //             EditorGUILayout.PropertyField(optionsProp, true);
        //         }
        //     }
        //
        //     EditorGUILayout.Space(6);
        //
        //     // ── UI References ──────────────────────────────────────────────
        //     EditorGUILayout.LabelField("UI References", EditorStyles.boldLabel);
        //     EditorGUILayout.PropertyField(serializedObject.FindProperty("LeftButton"));
        //     EditorGUILayout.PropertyField(serializedObject.FindProperty("RightButton"));
        //     EditorGUILayout.PropertyField(serializedObject.FindProperty("ValueLabel"));
        //
        //     EditorGUILayout.Space(6);
        //
        //     // ── Behaviour ─────────────────────────────────────────────────
        //     EditorGUILayout.LabelField("Behaviour", EditorStyles.boldLabel);
        //     var wrapProp = serializedObject.FindProperty("WrapAround");
        //     EditorGUILayout.PropertyField(wrapProp);
        //
        //     if (!wrapProp.boolValue)
        //     {
        //         EditorGUILayout.HelpBox(
        //             "WrapAround is OFF — left/right buttons will be disabled at the ends of the list.",
        //             MessageType.None);
        //     }
        //
        //     serializedObject.ApplyModifiedProperties();
        // }
    }
}
