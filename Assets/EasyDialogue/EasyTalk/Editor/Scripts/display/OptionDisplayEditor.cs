using EasyTalk.Animation;
using EasyTalk.Display;
using EasyTalk.Editor.Utils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyTalk.Editor.Display
{
    [CustomEditor(typeof(OptionDisplay))]
    public class OptionDisplayEditor : DialoguePanelEditor
    {
        protected static bool expandEventSettings = false;

        public override void OnInspectorGUI()
        {
            OptionDisplay display = target as OptionDisplay;

            //if (displayObj == null || (displayObj != null && displayObj.targetObject != display))
            {
                displayObj = new SerializedObject(display);
            }

            EditorGUI.BeginChangeCheck();

            CreateDisplaySettings();
            CreateGeneralSettings();
            CreateFontSettings(display);
            CreateAnimationSettings(display);
            CreateImageSettings();
            CreateOptionDisplayListenerSettings();
            CreateDialoguePanelEventSettings();
            CreateOptionDisplayEventSettings();

            ETGUI.DrawLineSeparator();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(display);
            }

            displayObj.ApplyModifiedProperties();
        }

        protected void CreateImageSettings()
        {
            ETGUI.DrawLineSeparator();
            EditorGUILayout.PropertyField(displayObj.FindProperty("images"), new GUIContent("Images"));
        }

        protected virtual List<UIAnimator.Animation> GetSupportedAnimationTypes()
        {
            List<UIAnimator.Animation> animTypes = new List<UIAnimator.Animation>();
            foreach (UIAnimator.Animation anim in Enum.GetValues(typeof(UIAnimator.Animation)))
            {
                animTypes.Add(anim);
            }

            return animTypes;
        }

        protected virtual void CreateOptionDisplayListenerSettings()
        {
            ETGUI.DrawLineSeparator();

            EditorGUILayout.PropertyField(displayObj.FindProperty("optionDisplayListeners"), new GUIContent("Option Display Listeners"));
        }

        protected void CreateOptionDisplayEventSettings()
        {
            ETGUI.DrawLineSeparator();

            expandEventSettings = EditorGUILayout.Foldout(expandEventSettings, new GUIContent("Option Display Events"), EditorStyles.foldoutHeader);

            if (expandEventSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(displayObj.FindProperty("onOptionsSet"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onOptionSelected"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onOptionChanged"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onOptionChosen"));
                EditorGUI.indentLevel--;
            }
        }
    }
}