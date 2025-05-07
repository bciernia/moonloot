using EasyTalk.Display;
using EasyTalk.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace EasyTalk.Editor.Display
{
    [CustomEditor(typeof(OptionListDisplay))]
    public class OptionListDisplayEditor : OptionDisplayEditor
    {
        private static bool expandOptionButtonSettings = false;

        public override void OnInspectorGUI()
        {
            OptionListDisplay display = target as OptionListDisplay;

            //if (displayObj == null || (displayObj != null && displayObj.targetObject != display))
            {
                displayObj = new SerializedObject(display);
            }

            EditorGUI.BeginChangeCheck();

            CreateDisplaySettings();
            CreateGeneralSettings();
            CreateFontSettings(display);
            CreateOptionButtonSettings(display);
            CreateAnimationSettings(display);
            CreateImageSettings();
            CreateOptionDisplayListenerSettings();
            CreateDialoguePanelEventSettings();
            CreateOptionDisplayEventSettings();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(display);
            }

            displayObj.ApplyModifiedProperties();
        }

        private static void CreateOptionButtonSettings(OptionListDisplay display)
        {
            ETGUI.DrawLineSeparator();
            expandOptionButtonSettings = EditorGUILayout.Foldout(expandOptionButtonSettings, new GUIContent("Option Buttons",
                "The list of Option Buttons available in the Option Display."), EditorStyles.foldoutHeader);

            if (expandOptionButtonSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Number of Options");
                int numberOfOptionButtons = EditorGUILayout.IntField(display.GetOptionButtons().Count);
                EditorGUILayout.EndHorizontal();

                while (numberOfOptionButtons > display.GetOptionButtons().Count)
                {
                    display.GetOptionButtons().Add(null);
                }

                while (numberOfOptionButtons < display.GetOptionButtons().Count)
                {
                    display.GetOptionButtons().RemoveAt(numberOfOptionButtons);
                }

                for (int i = 0; i < display.GetOptionButtons().Count; i++)
                {
                    display.GetOptionButtons()[i] = EditorGUILayout.ObjectField(new GUIContent("Option Button " + (i + 1)), display.GetOptionButtons()[i], typeof(DialogueButton), true) as DialogueButton;
                }
                EditorGUI.indentLevel--;
            }
        }

        protected override void CreateGeneralFields()
        {
            base.CreateGeneralFields();
            EditorGUILayout.PropertyField(displayObj.FindProperty("reverseControls"), new GUIContent("Reverse Controls"));
            EditorGUILayout.PropertyField(displayObj.FindProperty("isDynamic"), new GUIContent("Is Dynamic?"));
        }
    }
}