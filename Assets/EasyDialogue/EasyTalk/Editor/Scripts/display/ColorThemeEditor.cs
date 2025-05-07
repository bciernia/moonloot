using EasyTalk.Display.Style;
using EasyTalk.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace EasyTalk.Editor.Display
{
    [CustomEditor(typeof(ColorTheme))]
    public class ColorThemeEditor : UnityEditor.Editor
    {
        private SerializedObject themeObj;

        public override void OnInspectorGUI()
        {
            ColorTheme colorTheme = target as ColorTheme;

            //if (themeObj == null || (themeObj != null && themeObj.targetObject != colorTheme))
            {
                themeObj = new SerializedObject(colorTheme);
            }

            EditorGUI.BeginChangeCheck();

            ETGUI.DrawLineSeparator();

            EditorGUILayout.LabelField("Theme Colors");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(themeObj.FindProperty("color"));

            EditorGUILayout.PropertyField(themeObj.FindProperty("complimentary1"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("complimentary2"));

            EditorGUILayout.PropertyField(themeObj.FindProperty("contrastColor1"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("contrastColor2"));
            EditorGUI.indentLevel--;

            ETGUI.DrawLineSeparator();

            EditorGUILayout.LabelField("Convo Panel Colors");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(themeObj.FindProperty("convoTextColor"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("convoImageColors"));
            EditorGUI.indentLevel--;

            ETGUI.DrawLineSeparator();

            EditorGUILayout.LabelField("Character Panel Colors");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(themeObj.FindProperty("characterNameTextColor"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("characterPanelColor"));
            EditorGUI.indentLevel--;

            ETGUI.DrawLineSeparator();

            EditorGUILayout.LabelField("Option Panel Colors");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(themeObj.FindProperty("optionImageColors"));
            EditorGUI.indentLevel--;

            ETGUI.DrawLineSeparator();

            EditorGUILayout.LabelField("Option Button Colors");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(themeObj.FindProperty("buttonTextNormalColor"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("buttonTextPressedColor"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("buttonTextDisabledColor"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("buttonTextHighlightColor"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("buttonNormalColor"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("buttonPressedColor"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("buttonDisabledColor"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("buttonHighlightColor"));
            EditorGUI.indentLevel--;

            ETGUI.DrawLineSeparator();

            EditorGUILayout.LabelField("Continue Button Colors");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(themeObj.FindProperty("continueTextNormalColor"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("continueTextPressedColor"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("continueTextDisabledColor"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("continueTextHighlightColor"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("continueButtonNormalColor"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("continueButtonPressedColor"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("continueButtonDisabledColor"));
            EditorGUILayout.PropertyField(themeObj.FindProperty("continueButtonHighlightColor"));
            EditorGUI.indentLevel--;

            ETGUI.DrawLineSeparator();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(colorTheme);
            }

            themeObj.ApplyModifiedProperties();
        }
    }
}
