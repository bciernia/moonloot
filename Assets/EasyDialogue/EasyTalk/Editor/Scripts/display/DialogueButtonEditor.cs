using EasyTalk.Display;
using EasyTalk.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace EasyTalk.Editor.Display
{
    [CustomEditor(typeof(DialogueButton))]
    public class DialogueButtonEditor : UnityEditor.Editor
    {
        protected static bool expandTextSettings = false;
        protected static bool expandImageSettings = false;
        protected static bool expandAudioSettings = false;
        protected static bool expandEventSettings = false;

        protected static SerializedObject buttonObj;

        public override void OnInspectorGUI()
        {
            DialogueButton button = target as DialogueButton;
            buttonObj = new SerializedObject(button);

            EditorGUI.BeginChangeCheck();

            CreateButtonTextSettings();
            CreateButtonImageSettings();
            CreateAudioSettings();
            CreateUnityEventSettings();

            ETGUI.DrawLineSeparator();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(button);
            }

            buttonObj.ApplyModifiedProperties();
        }

        private static void CreateButtonTextSettings()
        {
            ETGUI.DrawLineSeparator();
            expandTextSettings = EditorGUILayout.Foldout(expandTextSettings, new GUIContent("Text Settings"), EditorStyles.foldoutHeader);

            if (expandTextSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(buttonObj.FindProperty("text"));
#if TEXTMESHPRO_INSTALLED
                EditorGUILayout.PropertyField(buttonObj.FindProperty("textMeshProText"));
#endif
                EditorGUILayout.PropertyField(buttonObj.FindProperty("normalTextColor"));
                EditorGUILayout.PropertyField(buttonObj.FindProperty("highlightedTextColor"));
                EditorGUILayout.PropertyField(buttonObj.FindProperty("pressedTextColor"));
                EditorGUILayout.PropertyField(buttonObj.FindProperty("disabledTextColor"));
                EditorGUI.indentLevel--;
            }
        }

        private static void CreateButtonImageSettings()
        {
            ETGUI.DrawLineSeparator();
            expandImageSettings = EditorGUILayout.Foldout(expandImageSettings, new GUIContent("Image Settings"), EditorStyles.foldoutHeader);

            if (expandImageSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(buttonObj.FindProperty("backgroundImage"));

                EditorGUILayout.PropertyField(buttonObj.FindProperty("normalButtonColor"));
                EditorGUILayout.PropertyField(buttonObj.FindProperty("highlightedButtonColor"));
                EditorGUILayout.PropertyField(buttonObj.FindProperty("pressedButtonColor"));
                EditorGUILayout.PropertyField(buttonObj.FindProperty("disabledButtonColor"));
                EditorGUI.indentLevel--;
            }
        }

        private static void CreateAudioSettings()
        {
            ETGUI.DrawLineSeparator();
            expandAudioSettings = EditorGUILayout.Foldout(expandAudioSettings, new GUIContent("Audio Settings"), EditorStyles.foldoutHeader);

            if (expandAudioSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(buttonObj.FindProperty("audioSource"));
                EditorGUILayout.PropertyField(buttonObj.FindProperty("hoverSound"));
                EditorGUI.indentLevel--;
            }
        }

        private static void CreateUnityEventSettings()
        {
            ETGUI.DrawLineSeparator();
            expandEventSettings = EditorGUILayout.Foldout(expandEventSettings, new GUIContent("Button Events"), EditorStyles.foldoutHeader);

            if (expandEventSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(buttonObj.FindProperty("onClick"));
                EditorGUILayout.PropertyField(buttonObj.FindProperty("onEnter"));
                EditorGUILayout.PropertyField(buttonObj.FindProperty("onLeave"));
                EditorGUILayout.PropertyField(buttonObj.FindProperty("onPress"));
                EditorGUILayout.PropertyField(buttonObj.FindProperty("onNormal"));
                EditorGUILayout.PropertyField(buttonObj.FindProperty("onHighlighted"));
                EditorGUI.indentLevel--;
            }
        }
    }
}