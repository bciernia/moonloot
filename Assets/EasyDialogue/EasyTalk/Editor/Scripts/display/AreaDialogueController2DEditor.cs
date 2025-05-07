using EasyTalk.Controller;
using EasyTalk.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace EasyTalk.Editor.Display
{
    [CustomEditor(typeof(AreaDialogueController2D))]
    public class AreaDialogueController2DEditor : DialogueControllerEditor
    {
        protected static bool expandActivationSettings = false;

        public override void OnInspectorGUI()
        {
            AreaDialogueController2D areaController = target as AreaDialogueController2D;

            //if (dialogueControllerObj == null || (dialogueControllerObj != null && dialogueControllerObj.targetObject != dialogueController))
            {
                dialogueControllerObj = new SerializedObject(areaController);
            }

            EditorGUI.BeginChangeCheck();

            CreateDialogueAssetSettings();
            CreateDialogueRegistrySettings();
            CreatePlaybackSettings();
            CreateGeneralSettings();
            CreateActivationSettings();
            CreateDialogueListenerSettings();
            CreateControllerEventSettings();
            CreateDialolgueEventSettings();

            ETGUI.DrawLineSeparator();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(areaController);
            }

            dialogueControllerObj.ApplyModifiedProperties();
        }

        protected override void AddSettingForEvents()
        {
            base.AddSettingForEvents();

            EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onPrompt"));
            EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onAreaEntered"));
            EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onAreaExited"));
        }

        private void CreateActivationSettings()
        {
            ETGUI.DrawLineSeparator();

            expandActivationSettings = EditorGUILayout.Foldout(expandActivationSettings, new GUIContent("Activation Settings"), EditorStyles.foldoutHeader);

            if (expandActivationSettings)
            {
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("activator"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("entryPoint"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("activationMode"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("deactivationMode"));
            }
        }
    }
}