using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EasyTalk.Display;
using EasyTalk.Animation;
using EasyTalk.Editor.Utils;

namespace EasyTalk.Editor.Display
{
    [CustomEditor(typeof(ContinueDisplay))]
    public class ContinueDisplayEditor : DialoguePanelEditor
    {
        protected static bool expandTextSettings = false;
        protected static bool expandImageSettings = false;

        public override void OnInspectorGUI()
        {
            ContinueDisplay display = target as ContinueDisplay;
            displayObj = new SerializedObject(display);

            EditorGUI.BeginChangeCheck();

            CreateDisplaySettings();
            CreateGeneralSettings();
            CreateFontSettings(display);
            CreateTextSettings();
            CreateImageSettings();
            CreateAnimationSettings(display);
            CreateDialoguePanelEventSettings();

            ETGUI.DrawLineSeparator();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(display);
            }

            displayObj.ApplyModifiedProperties();
        }


        private void CreateImageSettings()
        {
            ETGUI.DrawLineSeparator();
            expandImageSettings = EditorGUILayout.Foldout(expandImageSettings, new GUIContent("Image Settings"), EditorStyles.foldoutHeader);

            if (expandImageSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(displayObj.FindProperty("backgroundImage"));
                EditorGUI.indentLevel--;
            }
        }

        private void CreateTextSettings()
        {
            ETGUI.DrawLineSeparator();
            expandTextSettings = EditorGUILayout.Foldout(expandTextSettings, new GUIContent("Text Settings"), EditorStyles.foldoutHeader);

            if (expandTextSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(displayObj.FindProperty("text"));
#if TEXTMESHPRO_INSTALLED
                EditorGUILayout.PropertyField(displayObj.FindProperty("textMeshProText"));
#endif
                EditorGUI.indentLevel--;
            }
        }

    }
}
