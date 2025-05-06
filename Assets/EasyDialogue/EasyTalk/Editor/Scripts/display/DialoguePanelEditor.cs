using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EasyTalk.Display;
using EasyTalk.Animation;
using EasyTalk.Editor.Utils;
namespace EasyTalk.Editor.Display
{

    [CustomEditor(typeof(DialoguePanel))]
    public class DialoguePanelEditor : UnityEditor.Editor
    {
        protected SerializedObject displayObj;

        protected static bool expandGeneralSettings = false;
        protected static bool expandAnimationSettings = false;
        protected static bool expandPanelEventSettings = false;
        protected static bool expandFontSettings = false;

        protected virtual void CreateDisplaySettings()
        {
            ETGUI.DrawLineSeparator();
            EditorGUILayout.PropertyField(displayObj.FindProperty("displayID"), new GUIContent("Display ID", "An identifier for the panel."));
        }

        protected void CreateGeneralSettings()
        {
            ETGUI.DrawLineSeparator();
            expandGeneralSettings = EditorGUILayout.Foldout(expandGeneralSettings, new GUIContent("General Settings"), EditorStyles.foldoutHeader);

            if (expandGeneralSettings)
            {
                EditorGUI.indentLevel++;
                CreateGeneralFields();
                EditorGUI.indentLevel--;
            }
        }

        protected virtual void CreateGeneralFields()
        {
            EditorGUILayout.PropertyField(displayObj.FindProperty("forceStandardText"), new GUIContent("Force Standard Text Use?"));
        }

        protected virtual void CreateFontSettings(DialoguePanel display)
        {
            ETGUI.DrawLineSeparator();
            expandFontSettings = EditorGUILayout.Foldout(expandFontSettings, new GUIContent("Font Settings"), EditorStyles.foldoutHeader);

            if (expandFontSettings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(displayObj.FindProperty("languageFontOverrides"), new GUIContent("Language Font Overrides"));

                bool overrideFontSizes = EditorGUILayout.BeginToggleGroup(new GUIContent("Override Font Size Settings"), display.OverrideFontSizes);
                if (overrideFontSizes != display.OverrideFontSizes)
                {
                    display.OverrideFontSizes = overrideFontSizes;
                    EditorUtility.SetDirty(display);
                }

                if (overrideFontSizes)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(displayObj.FindProperty("minFontSize"), new GUIContent("Min Font Size"));
                    EditorGUILayout.PropertyField(displayObj.FindProperty("maxFontSize"), new GUIContent("Max Font Size"));
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndToggleGroup();
                EditorGUI.indentLevel--;
            }
        }

        protected virtual void CreateAnimationSettings(DialoguePanel display)
        {
            ETGUI.DrawLineSeparator();
            expandAnimationSettings = EditorGUILayout.Foldout(expandAnimationSettings, new GUIContent("Animation Settings"), EditorStyles.foldoutHeader);

            if (expandAnimationSettings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(displayObj.FindProperty("animationType"), new GUIContent("Animation Type"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("animationCurve"), new GUIContent("Animation Curve"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("animationTime"), new GUIContent("Animation Time"));

                if (display.AnimationType == UIAnimator.Animation.SLIDE_RIGHT || display.AnimationType == UIAnimator.Animation.SLIDE_LEFT ||
                    display.AnimationType == UIAnimator.Animation.SLIDE_UP || display.AnimationType == UIAnimator.Animation.SLIDE_DOWN)
                {
                    EditorGUILayout.PropertyField(displayObj.FindProperty("returnToOriginalPosition"), new GUIContent("Return to Original Position"));
                }

                EditorGUI.indentLevel--;
            }
        }

        protected virtual void CreateDialoguePanelEventSettings()
        {
            ETGUI.DrawLineSeparator();

            expandPanelEventSettings = EditorGUILayout.Foldout(expandPanelEventSettings, new GUIContent("Display Panel Events"), EditorStyles.foldoutHeader);

            if (expandPanelEventSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(displayObj.FindProperty("onHideStart"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onHideComplete"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onShowStart"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onShowComplete"));
                EditorGUI.indentLevel--;
            }
        }
    }
}
