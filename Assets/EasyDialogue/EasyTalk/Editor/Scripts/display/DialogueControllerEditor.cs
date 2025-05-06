using EasyTalk.Controller;
using EasyTalk.Display;
using EasyTalk.Editor.Utils;
using System;
using UnityEditor;
using UnityEngine;

namespace EasyTalk.Editor.Display
{
    [CustomEditor(typeof(DialogueController))]
    public class DialogueControllerEditor : UnityEditor.Editor
    {
        protected static SerializedObject dialogueControllerObj;

        protected static bool expandControllerEventSettings = false;
        protected static bool expandDialogueEventSettings = false;

        public override void OnInspectorGUI()
        {
            DialogueController dialogueController = target as DialogueController;

            //if (dialogueControllerObj == null || (dialogueControllerObj != null && dialogueControllerObj.targetObject != dialogueController))
            {
                dialogueControllerObj = new SerializedObject(dialogueController);
            }

            EditorGUI.BeginChangeCheck();
            CreateDialogueAssetSettings();
            CreateDialogueRegistrySettings();
            CreatePlaybackSettings();
            CreateGeneralSettings();
            CreateDialogueListenerSettings();
            CreateControllerEventSettings();
            CreateDialolgueEventSettings();

            ETGUI.DrawLineSeparator();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(dialogueController);
            }

            dialogueControllerObj.ApplyModifiedProperties();
        }

        protected static void CreateDialogueListenerSettings()
        {
            ETGUI.DrawLineSeparator();
            EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("dialogueListeners"));
        }

        protected static void CreateGeneralSettings()
        {
            ETGUI.DrawLineSeparator();
            EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("useDialogueDisplay"), new GUIContent("Use Dialogue Display?"));

            if (dialogueControllerObj.FindProperty("useDialogueDisplay").boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("dialogueDisplay"), new GUIContent("Dialogue Display"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("conversationDisplay"), new GUIContent("Conversation Display"));

                if (dialogueControllerObj.FindProperty("conversationDisplay").objectReferenceValue == null)
                {
                    EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("conversationDisplayID"), new GUIContent("Conversation Display ID"));
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("audioSource"), new GUIContent("Audio Source"));
        }

        protected static void CreatePlaybackSettings()
        {
            ETGUI.DrawLineSeparator();
            EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("playbackType"));
        }

        protected static void CreateDialogueAssetSettings()
        {
            ETGUI.DrawLineSeparator();
            EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("dialogue"), new GUIContent("Dialogue"));
        }

        protected static void CreateDialogueRegistrySettings()
        {
            ETGUI.DrawLineSeparator();
            EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("dialogueRegistry"), new GUIContent("Registry"));
        }

        protected void CreateControllerEventSettings()
        {
            ETGUI.DrawLineSeparator();

            expandControllerEventSettings = EditorGUILayout.Foldout(expandControllerEventSettings, new GUIContent("Controller Events"), EditorStyles.foldoutHeader);

            if (expandControllerEventSettings)
            {
                AddSettingForEvents();
            }
        }

        protected virtual void AddSettingForEvents()
        {
            EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onPlay"));
            EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onStop"));
        }

        protected void CreateDialolgueEventSettings()
        {
            ETGUI.DrawLineSeparator();

            expandDialogueEventSettings = EditorGUILayout.Foldout(expandDialogueEventSettings, new GUIContent("Dialogue Events"), EditorStyles.foldoutHeader);

            if (expandDialogueEventSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onContinue"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onDisplayOptions"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onOptionChosen"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onDisplayLine"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onDialogueEntered"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onDialogueExited"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onExitCompleted"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onStory"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onVariableUpdated"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onCharacterChanged"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onAudioStarted"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onAudioCompleted"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onActivateKey"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onWait"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onConversationEnding"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onNodeChanged"));
                EditorGUILayout.PropertyField(dialogueControllerObj.FindProperty("onPause"));
                EditorGUI.indentLevel--;
            }
        }
    }
}