using EasyTalk.Display;
using EasyTalk.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace EasyTalk.Editor.Display
{
    [CustomEditor(typeof(CharacterSpritePanel))]
    public class CharacterSpritePanelEditor : DialoguePanelEditor
    {
        public override void OnInspectorGUI()
        {
            CharacterSpritePanel display = target as CharacterSpritePanel;

            //if (displayObj == null || (displayObj != null && displayObj.targetObject != display))
            {
                displayObj = new SerializedObject(display);
            }

            EditorGUI.BeginChangeCheck();

            CreateDisplaySettings();
            CreateAnimationSettings(display);
            CreateDialoguePanelEventSettings();

            ETGUI.DrawLineSeparator();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(display);
            }

            displayObj.ApplyModifiedProperties();
        }

        protected override void CreateDisplaySettings()
        {
            base.CreateDisplaySettings();

            EditorGUILayout.PropertyField(displayObj.FindProperty("spriteMode"), new GUIContent("Sprite Mode", 
                "The image mode to use." +
                "\nPORTRAYAL mode: the chosen sprite(s) will be retrieved from a character's configured portrayals." +
                "\nICON mode, the sprite(s) come from the character's configured icons."));

            EditorGUILayout.PropertyField(displayObj.FindProperty("showWhenInvalid"), new GUIContent("Show When Invalid?", "Whether or not the panel's image should be updated when a null or empty image ID is used."));
        }
    }
}
