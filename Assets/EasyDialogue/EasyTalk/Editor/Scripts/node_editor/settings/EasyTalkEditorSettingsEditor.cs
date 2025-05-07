using EasyTalk.Editor.Utils;
using System;
using UnityEditor;
using UnityEngine;

namespace EasyTalk.Editor.Settings
{
    [CustomEditor(typeof(EasyTalkEditorSettings))]
    public class EasyTalkEditorSettingsEditor : UnityEditor.Editor
    {
        private static SerializedObject settingsObj;

        public override void OnInspectorGUI()
        {
            EasyTalkEditorSettings editorSettings = target as EasyTalkEditorSettings;
            settingsObj = new SerializedObject(editorSettings);

            EditorGUILayout.LabelField(new GUIContent("Volume Settings"), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(settingsObj.FindProperty("defaultVolume"));
            EditorGUI.indentLevel--;
            EditorGUILayout.EndFoldoutHeaderGroup();
            ETGUI.DrawLineSeparator();

            EditorGUILayout.LabelField(new GUIContent("Autosave Settings"), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            SerializedProperty autosaveModeProp = settingsObj.FindProperty("autosaveMode");
            EditorGUILayout.PropertyField(autosaveModeProp);

            ETFileManager.AutosaveMode autosaveMode = (ETFileManager.AutosaveMode)Enum.GetValues(typeof(ETFileManager.AutosaveMode)).GetValue(autosaveModeProp.enumValueIndex);

            if (autosaveMode == ETFileManager.AutosaveMode.TIMED) 
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(settingsObj.FindProperty("autosaveDelayMs"));
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndFoldoutHeaderGroup();
            ETGUI.DrawLineSeparator();

            EditorGUILayout.LabelField(new GUIContent("Stylesheet Settings"), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(settingsObj.FindProperty("stylesheets"));
            EditorGUI.indentLevel--;
            EditorGUILayout.EndFoldoutHeaderGroup();
            ETGUI.DrawLineSeparator();

            EditorGUILayout.LabelField(new GUIContent("Localization Settings"), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(settingsObj.FindProperty("defaultOriginalLanguage"));
            EditorGUILayout.PropertyField(settingsObj.FindProperty("defaultFont"));
            EditorGUILayout.PropertyField(settingsObj.FindProperty("googleTranslateProjectId"));
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(settingsObj.FindProperty("localizableLanguages"));
            EditorGUILayout.PropertyField(settingsObj.FindProperty("languageFontOverrides"));

            EditorGUILayout.PropertyField(settingsObj.FindProperty("defaultLocalizationLanguages"));
            EditorGUI.indentLevel--;
            EditorGUILayout.EndFoldoutHeaderGroup();
            ETGUI.DrawLineSeparator();

            EditorGUILayout.LabelField(new GUIContent("Fine Tuning"), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(settingsObj.FindProperty("uiReloadSessionDelay"));
            EditorGUILayout.PropertyField(settingsObj.FindProperty("dialogueLoadDelay"));
            EditorGUI.indentLevel--;
            EditorGUI.EndFoldoutHeaderGroup();
            ETGUI.DrawLineSeparator();

            EditorGUILayout.LabelField(new GUIContent("TextMeshPro Settings"), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(settingsObj.FindProperty("autoDetectTMP"));
            EditorGUI.indentLevel--;

            ETGUI.DrawLineSeparator();
            EditorGUILayout.LabelField(new GUIContent("Dialogue Registry"), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(settingsObj.FindProperty("dialogueRegistry"));
            EditorGUI.indentLevel--;

            settingsObj.ApplyModifiedProperties();
            settingsObj.Dispose();
        }
    }
}
