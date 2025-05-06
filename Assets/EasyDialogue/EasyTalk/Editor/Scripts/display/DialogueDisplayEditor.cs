using EasyTalk.Controller;
using EasyTalk.Display;
using EasyTalk.Editor.Utils;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EasyTalk.Editor.Display
{
    [CustomEditor(typeof(DialogueDisplay))]
    public class DialogueDisplayEditor : UnityEditor.Editor
    {
        protected SerializedObject displayObj;

        private static bool expandGeneralSettings = false;
        private static bool expandConversationDisplaySettings = false;
        private static bool expandOptionDisplaySettings = false;
        private static bool expandContinueDisplaySettings = false;
        private static bool expandDialogueEventSettings = false;
        private static bool expandDisplayEventSettings = false;

        public override void OnInspectorGUI()
        {
            DialogueDisplay display = target as DialogueDisplay;

            //if (displayObj == null || (displayObj != null && displayObj.targetObject != display))
            {
                displayObj = new SerializedObject(display);
            }

            EditorGUI.BeginChangeCheck();

            CreateDialogueSettings();
            CreateSubDisplaySettings();
            CreateGeneralSettings();
            CreateConversationSettings();
            CreateOptionSettings(display);
            CreateContinuationSettings(display);
            CreateDialogueListenerSettings();
            CreateDisplayEventSettings();
            CreateDialolgueEventSettings();

            ETGUI.DrawLineSeparator();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(display);
            }

            displayObj.ApplyModifiedProperties();
        }

        protected void CreateDialogueSettings()
        {
            ETGUI.DrawLineSeparator();
            EditorGUILayout.PropertyField(displayObj.FindProperty("dialogueSettings"), new GUIContent("Dialogue Settings"));
        }

        protected void CreateDialogueListenerSettings()
        {
            ETGUI.DrawLineSeparator();
            EditorGUILayout.PropertyField(displayObj.FindProperty("dialogueListeners"), new GUIContent("Dialogue Listeners"));
        }

        protected void CreateContinuationSettings(DialogueDisplay display)
        {
            ETGUI.DrawLineSeparator();

            expandContinueDisplaySettings = EditorGUILayout.Foldout(expandContinueDisplaySettings, new GUIContent("Continuation Settings"), EditorStyles.foldoutHeader);

            if (expandContinueDisplaySettings)
            {
                EditorGUI.indentLevel++;
                display.UseContinueDisplay = EditorGUILayout.Toggle(new GUIContent("Use Continue Display?",
                "If set to 'true', the Continue Display will be shown to the player each time they are allowed to continue along to the next line in the conversation."),
                display.UseContinueDisplay);

                EditorGUILayout.PropertyField(displayObj.FindProperty("continuationMode"));

                DialogueDelayMode delayMode = (DialogueDelayMode)displayObj.FindProperty("continuationMode").enumValueIndex;

                if (delayMode == DialogueDelayMode.AFTER_DELAY ||
                    delayMode == DialogueDelayMode.AFTER_AUDIO_AND_DELAY ||
                    delayMode == DialogueDelayMode.AFTER_AUDIO_OR_DELAY)
                {
                    EditorGUILayout.PropertyField(displayObj.FindProperty("continuationDelay"), new GUIContent("Continuation Delay"));
                }
                EditorGUI.indentLevel--;
            }
        }

        protected void CreateOptionSettings(DialogueDisplay display)
        {
            ETGUI.DrawLineSeparator();

            expandOptionDisplaySettings = EditorGUILayout.Foldout(expandOptionDisplaySettings, new GUIContent("Option Settings"), EditorStyles.foldoutHeader);

            if (expandOptionDisplaySettings)
            {
                display.PresentOptionsAutomatically = EditorGUILayout.Toggle(new GUIContent("Present Options Automatically?",
                    "If set to true, options will be automatically presented after either the prior conversation line's audio clip has played, " +
                    "or if there is no audio clip, after the configured option delay time. When set to false, options will not be shown until " +
                    "the player continues via input."), display.PresentOptionsAutomatically);

                if (display.PresentOptionsAutomatically)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(displayObj.FindProperty("optionDelayMode"));

                    int enumIdx = displayObj.FindProperty("optionDelayMode").enumValueIndex;
                    DialogueDelayMode delayMode = (DialogueDelayMode)Enum.GetValues(typeof(DialogueDelayMode)).GetValue(enumIdx);

                    if (delayMode == DialogueDelayMode.AFTER_AUDIO_AND_DELAY || delayMode == DialogueDelayMode.AFTER_AUDIO_OR_DELAY || delayMode == DialogueDelayMode.AFTER_DELAY)
                    {
                        EditorGUILayout.PropertyField(displayObj.FindProperty("optionDelay"), new GUIContent("Option Delay"));
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }

        protected void CreateConversationSettings()
        {
            ETGUI.DrawLineSeparator();

            expandConversationDisplaySettings = EditorGUILayout.Foldout(expandConversationDisplaySettings, new GUIContent("Conversation Settings"), EditorStyles.foldoutHeader);

            if (expandConversationDisplaySettings)
            {
                EditorGUILayout.PropertyField(displayObj.FindProperty("defaultConvoDisplayID"), new GUIContent("Default Conversation Display ID"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("areLinesSkippable"), new GUIContent("Are Lines Skippable?"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("hideConvoWhenShowingOptions"), new GUIContent("Hide Convo When Showing Options?"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("clearConvoWhenShowingOptions"), new GUIContent("Clear Convo When Showing Options?"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("refreshConvoOnCharacterChange"), new GUIContent("Refresh Convo on Character Change?"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("refreshConvoOnTextChange"), new GUIContent("Refresh Convo on Text Change?"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("switchConvoDisplayOnCharacterChange"), new GUIContent("Switch Convo Display on Character Change?"));

                EditorGUILayout.LabelField(new GUIContent("Autoplay Settings"));

                //Autoplay settings
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(displayObj.FindProperty("timePerWord"), new GUIContent("Time Per Word"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("minConvoTime"), new GUIContent("Min Convo Time"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("convoLineDelay"), new GUIContent("Convo Line Delay"));
                EditorGUI.indentLevel--;
            }
        }

        protected void CreateGeneralSettings()
        {
            ETGUI.DrawLineSeparator();

            expandGeneralSettings = EditorGUILayout.Foldout(expandGeneralSettings, new GUIContent("General Settings"), EditorStyles.foldoutHeader);

            if (expandGeneralSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(displayObj.FindProperty("destroyOnLoad"), new GUIContent("Destroy on Load?"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("hideDisplayOnExit"), new GUIContent("Hide on Exit?"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("hideOnPause"), new GUIContent("Hide on Pause?"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("continueOnStory"), new GUIContent("Continue on Story?"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("allowQuickExit"), new GUIContent("Allow Quick Exit?"));
                EditorGUI.indentLevel--;
            }
        }

        protected void CreateSubDisplaySettings()
        {
            ETGUI.DrawLineSeparator();
            
            EditorGUILayout.LabelField("Sub-Displays");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(displayObj.FindProperty("convoDisplay"), new GUIContent("Conversation Display"));
            EditorGUILayout.PropertyField(displayObj.FindProperty("optionDisplay"), new GUIContent("Option Display"));
            EditorGUILayout.PropertyField(displayObj.FindProperty("continueDisplay"), new GUIContent("Continue Display"));
            EditorGUILayout.PropertyField(displayObj.FindProperty("textInputDisplay"), new GUIContent("Text Input Display"));
            EditorGUILayout.PropertyField(displayObj.FindProperty("characterDisplay"), new GUIContent("Character Display"));
            EditorGUILayout.PropertyField(displayObj.FindProperty("iconDisplay"), new GUIContent("Icon Display"));
            EditorGUILayout.PropertyField(displayObj.FindProperty("loadingIcon"), new GUIContent("Loading Icon"));
            EditorGUI.indentLevel--;
        }

        protected void CreateDisplayEventSettings()
        {
            ETGUI.DrawLineSeparator();

            expandDisplayEventSettings = EditorGUILayout.Foldout(expandDisplayEventSettings, new GUIContent("Display Events"), EditorStyles.foldoutHeader);

            if (expandDisplayEventSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(displayObj.FindProperty("onContinueEnabled"), new GUIContent("On Continue Enabled"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onContinueDisabled"), new GUIContent("On Continue Disabled"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onOptionSelectionEnabled"), new GUIContent("On Option Selection Enabled"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onOptionSelectionDisabled"), new GUIContent("On Option Selection Disabled"));
                EditorGUI.indentLevel--;
            }
        }

        protected void CreateDialolgueEventSettings()
        {
            ETGUI.DrawLineSeparator();

            expandDialogueEventSettings = EditorGUILayout.Foldout(expandDialogueEventSettings, new GUIContent("Dialogue Events"), EditorStyles.foldoutHeader);

            if (expandDialogueEventSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(displayObj.FindProperty("onContinue"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onDisplayOptions"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onOptionChosen"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onDisplayLine"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onDialogueEntered"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onDialogueExited"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onExitCompleted"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onStory"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onVariableUpdated"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onCharacterChanged"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onAudioStarted"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onAudioCompleted"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onActivateKey"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onWait"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onConversationEnding"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onNodeChanged"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onPause"));
                EditorGUI.indentLevel--;
            }
        }
    }
}