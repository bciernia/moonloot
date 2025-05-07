using EasyTalk.Animation;
using EasyTalk.Display;
using EasyTalk.Editor.Utils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EasyTalk.Editor.Display
{
    [CustomEditor(typeof(ConversationDisplay))]
    public class ConversationDisplayEditor : DialoguePanelEditor
    {
        private static bool expandImageSettings = false;

        private static bool expandConverationSettings = false;
        private static bool expandCharacterNameSettings = false;
        private static bool expandEventSettings = false;
        private static bool expandGibberishSettings = false;

        public override void OnInspectorGUI()
        {
            ConversationDisplay display = target as ConversationDisplay;

            //if (displayObj == null || (displayObj != null && displayObj.targetObject != display))
            {
                displayObj = new SerializedObject(display);
            }

            EditorGUI.BeginChangeCheck();

            CreateDisplaySettings();
            CreateGeneralSettings();
            CreateFontSettings(display);
            CreateConversationTextSettings(display);
            CreateCharacterNameSettings(display);
            CreateAnimationSettings(display);
            CreateGibberishSettings(display);
            CreateImageSettings(display);
            CreateConvoListenerSettings();
            CreateDialoguePanelEventSettings();
            CreateConversationDisplayEventSettings();

            ETGUI.DrawLineSeparator();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(display);
            }

            displayObj.ApplyModifiedProperties();
        }

        protected override void CreateGeneralFields()
        {
            base.CreateGeneralFields();
            EditorGUILayout.PropertyField(displayObj.FindProperty("hideOnAwake"), new GUIContent("Hide on Awake?"));
        }

        protected virtual void CreateConversationTextSettings(ConversationDisplay display)
        {
            ETGUI.DrawLineSeparator();
            expandConverationSettings = EditorGUILayout.Foldout(expandConverationSettings, new GUIContent("Conversation Text Settings"), EditorStyles.foldoutHeader);

            if (expandConverationSettings)
            {
                EditorGUI.indentLevel++;

#if TEXTMESHPRO_INSTALLED
                EditorGUILayout.PropertyField(displayObj.FindProperty("textMeshProConvoText"), new GUIContent("TMP Convo Text"));
#endif

                EditorGUILayout.PropertyField(displayObj.FindProperty("convoText"), new GUIContent("Convo Text"));

                EditorGUILayout.Separator();
                display.TextPresentationMode = (ConversationDisplay.TextDisplayMode)EditorGUILayout.EnumPopup(new GUIContent("Text Display Mode",
                    "The way text should be displayed on the Conversation Display." +
                    "\nFULL: Lines of dialogue will be shown in their entirety." +
                    "\nBY_WORD: Lines of dialogue will be shown one word at a time." +
                    "\nBY_CHARACTER: Lines of dialogue will be shown one character at a time."),
                    display.TextPresentationMode);

                EditorGUI.indentLevel++;

                if (display.TextPresentationMode == ConversationDisplay.TextDisplayMode.BY_WORD)
                {
                    EditorGUILayout.PropertyField(displayObj.FindProperty("wordsPerSecond"), new GUIContent("Words Per Second"));
                }
                else if (display.TextPresentationMode == ConversationDisplay.TextDisplayMode.BY_CHARACTER)
                {
                    EditorGUILayout.PropertyField(displayObj.FindProperty("charactersPerSecond"), new GUIContent("Characters Per Second"));
                }

                EditorGUI.indentLevel--;

                EditorGUILayout.Separator();
                EditorGUILayout.PropertyField(displayObj.FindProperty("appendDelimiter"));

                EditorGUI.indentLevel--;
            }
        }

        private void CreateCharacterNameSettings(ConversationDisplay display)
        {
            ETGUI.DrawLineSeparator();
            expandCharacterNameSettings = EditorGUILayout.Foldout(expandCharacterNameSettings, new GUIContent("Character Name Settings"), EditorStyles.foldoutHeader);

            if (expandCharacterNameSettings)
            {
                EditorGUI.indentLevel++;

#if TEXTMESHPRO_INSTALLED
                EditorGUILayout.PropertyField(displayObj.FindProperty("textMeshProCharacterNameText"), new GUIContent("TMP Character Name Text"));
#endif

                EditorGUILayout.PropertyField(displayObj.FindProperty("characterNameText"), new GUIContent("Character Name Text"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("characterNameBackgroundImage"), new GUIContent("Character Name Background Image"));

                EditorGUI.indentLevel--;
            }
        }
        
        private void CreateImageSettings(ConversationDisplay display)
        {
            ETGUI.DrawLineSeparator();
            expandImageSettings = EditorGUILayout.Foldout(expandImageSettings, new GUIContent("Images", "A list of images used in the Conversation Display."), EditorStyles.foldoutHeader);

            if (expandImageSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Number of Images");
                int numberOfImages = EditorGUILayout.IntField(display.ConversationPanelImages.Count);
                EditorGUILayout.EndHorizontal();

                while (numberOfImages > display.ConversationPanelImages.Count)
                {
                    display.ConversationPanelImages.Add(null);
                }

                while (numberOfImages < display.ConversationPanelImages.Count)
                {
                    display.ConversationPanelImages.RemoveAt(numberOfImages);
                }

                for (int i = 0; i < display.ConversationPanelImages.Count; i++)
                {
                    Image bgImage = EditorGUILayout.ObjectField(new GUIContent("Image " + (i + 1)), display.ConversationPanelImages[i], typeof(Image), true) as Image;
                    if (display.ConversationPanelImages[i] != bgImage)
                    {
                        display.ConversationPanelImages[i] = bgImage;
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        private void CreateConvoListenerSettings()
        {
            ETGUI.DrawLineSeparator();
            EditorGUILayout.PropertyField(displayObj.FindProperty("conversationDisplayListeners"));
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

        protected void CreateConversationDisplayEventSettings()
        {
            ETGUI.DrawLineSeparator();

            expandEventSettings = EditorGUILayout.Foldout(expandEventSettings, new GUIContent("Conversation Display Events"), EditorStyles.foldoutHeader);

            if (expandEventSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(displayObj.FindProperty("onCharacterNameUpdated"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onConversationTextUpdated"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("onReset"));
                EditorGUI.indentLevel--;
            }
        }

        protected void CreateGibberishSettings(ConversationDisplay display)
        {
            SerializedProperty textDisplayModeProperty = displayObj.FindProperty("textDisplayMode");
            ConversationDisplay.TextDisplayMode textDisplayMode = 
                (ConversationDisplay.TextDisplayMode)Enum.GetValues(typeof(ConversationDisplay.TextDisplayMode)).GetValue(textDisplayModeProperty.enumValueIndex);

            if (textDisplayMode == ConversationDisplay.TextDisplayMode.BY_CHARACTER || textDisplayMode == ConversationDisplay.TextDisplayMode.BY_WORD)
            {
                ETGUI.DrawLineSeparator();

                expandGibberishSettings = EditorGUILayout.Foldout(expandGibberishSettings, new GUIContent("Gibberish Settings"), EditorStyles.foldoutHeader);

                if (expandGibberishSettings)
                {
                    EditorGUI.indentLevel++;
                    SerializedProperty gibberishModeProperty = displayObj.FindProperty("gibberishMode");
                    EditorGUILayout.PropertyField(gibberishModeProperty);

                    GibberishMode mode = (GibberishMode)Enum.GetValues(typeof(GibberishMode)).GetValue(gibberishModeProperty.enumValueIndex);

                    if (mode != GibberishMode.NONE)
                    {
                        EditorGUILayout.PropertyField(displayObj.FindProperty("gibberishAudioSource"));
                        EditorGUILayout.PropertyField(displayObj.FindProperty("gibberishAudioClips"));
                    }

                    EditorGUI.indentLevel--;
                }
            }
        }
    }
}