using EasyTalk.Display;
using EasyTalk.Display.Style;
using EasyTalk.Editor.Utils;
using System;
using System.Collections.Generic;

#if TEXTMESHPRO_INSTALLED
using TMPro;
#endif

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EasyTalk.Editor.Display
{
    public class DialogueStyleManagerEditor : UnityEditor.EditorWindow
    {
        private int styleIndex = -1;
        private List<string> styles = new List<string>();
        private Dictionary<string, string> stylePaths = new Dictionary<string, string>();

        private int colorThemeIndex = -1;
        private List<string> colorThemes = new List<string>();
        private Dictionary<string, string> colorThemePaths = new Dictionary<string, string>();

        private int frameThemeIndex = -1;
        private List<string> frameThemes = new List<string>();
        private Dictionary<string, string> frameThemePaths = new Dictionary<string, string>();

        private static bool expandConvoSettings = true;
        private static bool expandOptionSettings = true;
        private static bool expandContinueSettings = true;

        private static bool expandConvoImageSettings = false;
        private static bool expandConvoTextSettings = false;
        private static bool expandCharacterNameSettings = true;
        private static bool expandCharacterNameTextSettings = false;
        private static bool expandCharacterNameImageSettings = false;
        private static bool expandOptionPanelSettings = false;
        private static bool expandOptionButtonTextSettings = false;
        private static bool expandOptionButtonImageSettings = false;
        private static bool expandOptionButtonAudioSettings = false;
        private static bool expandDirectionalSettings = false;
        private static bool expandContinueImageSettings = false;
        private static bool expandContinueTextSettings = false;

        private static Vector2 scrollPos = Vector2.zero;

        [UnityEditor.MenuItem("Tools/EasyTalk/Dialogue Style Manager")]
        static void OpenEditor()
        {
            DialogueStyleManagerEditor styleEditor = GetWindow<DialogueStyleManagerEditor>();
            styleEditor.position = new Rect(new Vector2(0.0f, 100.0f), new Vector2(800.0f, 600.0f));
            styleEditor.titleContent = new GUIContent("Dialogue Style Manager");
            styleEditor.titleContent.image = Resources.Load<Texture>("images/icons/nodes_title_icon");
        }

        private static DialogueDisplay dialogueDisplay;

        private void OnGUI()
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;
            if (selectedGameObjects != null && selectedGameObjects.Length == 1)
            {
                dialogueDisplay = selectedGameObjects[0].GetComponent<DialogueDisplay>();

                if (dialogueDisplay != null)
                {
                    FindStyles();
                    FindColorThemes();
                    //FindFrameThemes();

                    CreateStyleSaveButton(dialogueDisplay);
                    CreateStyleDropdown();
                    CreateColorThemeDropdown();
                    //CreateFrameThemeDropdown();

                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                    ETGUI.DrawLineSeparator();
                    CreateConversationDisplaySettings(dialogueDisplay);

                    ETGUI.DrawLineSeparator();
                    CreateOptionDisplaySettings(dialogueDisplay);

                    ETGUI.DrawLineSeparator();
                    CreateContinuePanelSettings(dialogueDisplay);

                    PrefabUtility.RecordPrefabInstancePropertyModifications(dialogueDisplay);

                    EditorGUILayout.EndScrollView();
                }
            }
        }

        private void CreateConversationDisplaySettings(DialogueDisplay display)
        {
            ConversationDisplay convoDisplay = display.GetConversationDisplay() as ConversationDisplay;
            EditorGUI.indentLevel = 0;

            if (convoDisplay != null)
            {
                expandConvoSettings = EditorGUILayout.BeginFoldoutHeaderGroup(expandConvoSettings, new GUIContent("Conversation Panel Style"));

                if (expandConvoSettings)
                {
                    CreateConvoPanelTextSettings(convoDisplay);
                    CreateConvoImageSettings(convoDisplay);
                    CreateCharacterNameSettings(convoDisplay);
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        private void OnSelectionChange()
        {
            Repaint();
        }

        private void CreateStyleDropdown()
        {
            try
            {
                EditorGUILayout.BeginHorizontal();
                bool clickedStyleDropdown = EditorGUILayout.DropdownButton(new GUIContent("Change Style To..."), FocusType.Passive, new GUILayoutOption[] { });

                if (clickedStyleDropdown)
                {
                    ShowStyleDropdown();
                }

                if (GUILayout.Button("<", new GUILayoutOption[] { GUILayout.Width(30.0f) }))
                {
                    styleIndex--;
                    if (styleIndex < 0) { styleIndex = styles.Count - 1; }
                    ApplyStyleToDisplay(stylePaths[styles[styleIndex]]);
                }

                if (GUILayout.Button(">", new GUILayoutOption[] { GUILayout.Width(30.0f) }))
                {
                    styleIndex++;
                    if (styleIndex >= styles.Count) { styleIndex = 0; }
                    ApplyStyleToDisplay(stylePaths[styles[styleIndex]]);
                }

                EditorGUILayout.EndHorizontal();
            }
            catch { }
        }

        private void CreateColorThemeDropdown()
        {
            try
            {
                EditorGUILayout.BeginHorizontal();
                bool clickedColorThemeDropdown = EditorGUILayout.DropdownButton(new GUIContent("Change Color Theme..."), FocusType.Passive, new GUILayoutOption[] { });

                if (clickedColorThemeDropdown)
                {
                    ShowColorThemeDropdown();
                }

                if (GUILayout.Button("<", new GUILayoutOption[] { GUILayout.Width(30.0f) }))
                {
                    colorThemeIndex--;
                    if (colorThemeIndex < 0) { colorThemeIndex = colorThemes.Count - 1; }
                    ApplyColorThemeToDisplay(colorThemePaths[colorThemes[colorThemeIndex]]);
                }

                if (GUILayout.Button(">", new GUILayoutOption[] { GUILayout.Width(30.0f) }))
                {
                    colorThemeIndex++;
                    if (colorThemeIndex >= colorThemes.Count) { colorThemeIndex = 0; }
                    ApplyColorThemeToDisplay(colorThemePaths[colorThemes[colorThemeIndex]]);
                }

                EditorGUILayout.EndHorizontal();
            }
            catch { }
        }

        private void CreateFrameThemeDropdown()
        {
            try
            {
                EditorGUILayout.BeginHorizontal();
                bool clickedFrameThemeDropdown = EditorGUILayout.DropdownButton(new GUIContent("Change Frame Theme..."), FocusType.Passive, new GUILayoutOption[] { });

                if (clickedFrameThemeDropdown)
                {
                    ShowFrameThemeDropdown();
                }

                if (GUILayout.Button("<", new GUILayoutOption[] { GUILayout.Width(30.0f) }))
                {
                    frameThemeIndex--;
                    if (frameThemeIndex < 0) { frameThemeIndex = frameThemes.Count - 1; }
                    ApplyFrameThemeToDisplay(frameThemePaths[frameThemes[frameThemeIndex]]);
                }

                if (GUILayout.Button(">", new GUILayoutOption[] { GUILayout.Width(30.0f) }))
                {
                    frameThemeIndex++;
                    if (frameThemeIndex >= frameThemes.Count) { frameThemeIndex = 0; }
                    ApplyFrameThemeToDisplay(frameThemePaths[frameThemes[frameThemeIndex]]);
                }

                EditorGUILayout.EndHorizontal();
            }
            catch { }
        }

        private void CreateContinuePanelSettings(DialogueDisplay display)
        {
            ContinueDisplay continueDisplay = display.GetContinueDisplay();
            if (continueDisplay != null)
            {
                expandContinueSettings = EditorGUILayout.Foldout(expandContinueSettings, new GUIContent("Continue Panel Style"));

                if (expandContinueSettings)
                {
                    EditorGUI.indentLevel++;
                    expandContinueTextSettings = EditorGUILayout.Foldout(expandContinueTextSettings, new GUIContent("Text Settings"));

                    if (expandContinueTextSettings)
                    {
                        CreateContinueTextSettings(continueDisplay);
                    }

                    expandContinueImageSettings = EditorGUILayout.Foldout(expandContinueImageSettings, new GUIContent("Image Settings"));

                    if (expandContinueImageSettings)
                    {
                        CreateContinueImageSettings(continueDisplay);
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }


        private void CreateContinueImageSettings(ContinueDisplay continueDisplay)
        {
            DialogueButton continueButton = continueDisplay.GetComponent<DialogueButton>();
            Image backgroundImage = continueDisplay.BackgroundImage;

            if (backgroundImage != null)
            {
                EditorGUI.indentLevel++;

                if (continueButton != null)
                {
                    this.ShowButtonImageSettings(continueButton);
                }
                else
                {
                    ShowImageSettings(backgroundImage);
                }

                EditorGUI.indentLevel--;
            }
        }

        private void CreateContinueTextSettings(ContinueDisplay continueDisplay)
        {
            DialogueButton continueButton = continueDisplay.GetComponent<DialogueButton>();

            if (continueButton != null)
            {
                ShowButtonTextSettings(continueButton);
            }
            else
            {
#if TEXTMESHPRO_INSTALLED
                TMP_Text tmpText = continueDisplay.TMPText;
#endif

                Text text = continueDisplay.StandardText;

#if TEXTMESHPRO_INSTALLED
                ShowTextSettings(tmpText, text);
#else
                ShowTextSettings(text);
#endif
            }
        }

        private void FindStyles()
        {
            styles.Clear();
            stylePaths.Clear();

            string[] stylesInProject = AssetDatabase.FindAssets("t:DialogueStyle");
            foreach (string style in stylesInProject)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(style);
                string filename = assetPath;
                if (filename.IndexOf("/") > 0)
                {
                    filename = filename.Substring(filename.LastIndexOf("/") + 1);
                }

                styles.Add(filename);
                stylePaths.Add(filename, assetPath);
            }
        }

        private void FindColorThemes()
        {
            colorThemes.Clear();
            colorThemePaths.Clear();

            string[] colorThemesInProject = AssetDatabase.FindAssets("t:ColorTheme");
            foreach (string colorTheme in colorThemesInProject)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(colorTheme);
                string filename = assetPath;
                if (filename.IndexOf("/") > 0)
                {
                    filename = filename.Substring(filename.LastIndexOf("/") + 1);
                }

                colorThemes.Add(filename);
                colorThemePaths.Add(filename, assetPath);
            }
        }

        private void FindFrameThemes()
        {
            frameThemes.Clear();
            frameThemePaths.Clear();

            string[] frameThemesInProject = AssetDatabase.FindAssets("t:FrameTheme");
            foreach (string frameTheme in frameThemesInProject)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(frameTheme);
                string filename = assetPath;
                if (filename.IndexOf("/") > 0)
                {
                    filename = filename.Substring(filename.LastIndexOf("/") + 1);
                }

                frameThemes.Add(filename);
                frameThemePaths.Add(filename, assetPath);
            }
        }

        private void ShowStyleDropdown()
        {
            GenericMenu styleMenu = new GenericMenu();
            foreach (string style in styles)
            {
                styleMenu.AddItem(new GUIContent(style), false, ApplyStyleToDisplay, stylePaths[style]);

            }
            styleMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
        }

        private void ShowColorThemeDropdown()
        {
            GenericMenu colorThemeMenu = new GenericMenu();
            foreach (string colorTheme in colorThemes)
            {
                colorThemeMenu.AddItem(new GUIContent(colorTheme), false, ApplyColorThemeToDisplay, colorThemePaths[colorTheme]);

            }
            colorThemeMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
        }

        private void ShowFrameThemeDropdown()
        {
            GenericMenu frameThemeMenu = new GenericMenu();
            foreach (string frameTheme in frameThemes)
            {
                frameThemeMenu.AddItem(new GUIContent(frameTheme), false, ApplyFrameThemeToDisplay, frameThemePaths[frameTheme]);

            }
            frameThemeMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
        }

        private void ApplyColorThemeToDisplay(object menuTarget)
        {
            if (Selection.activeGameObject != null)
            {
                DialogueDisplay dialogueDisplay = Selection.activeGameObject.GetComponent<DialogueDisplay>();
                if (dialogueDisplay != null)
                {
                    RecordUndoableChanges(dialogueDisplay);

                    ColorTheme colorTheme = AssetDatabase.LoadAssetAtPath<ColorTheme>(menuTarget.ToString());

                    DialogueStyleManager.ApplyColorTheme(colorTheme, dialogueDisplay);
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
            }
        }

        private void ApplyFrameThemeToDisplay(object menuTarget)
        {
            if (Selection.activeGameObject != null)
            {
                DialogueDisplay dialogueDisplay = Selection.activeGameObject.GetComponent<DialogueDisplay>();
                if (dialogueDisplay != null)
                {
                    RecordUndoableChanges(dialogueDisplay);

                    FrameTheme frameTheme = AssetDatabase.LoadAssetAtPath<FrameTheme>(menuTarget.ToString());
                    DialogueStyleManager.ApplyFrameTheme(frameTheme, dialogueDisplay);
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
            }
        }

        private void ApplyStyleToDisplay(object menuTarget)
        {
            if (Selection.activeGameObject != null)
            {
                DialogueDisplay dialogueDisplay = Selection.activeGameObject.GetComponent<DialogueDisplay>();
                if (dialogueDisplay != null)
                {
                    RecordUndoableChanges(dialogueDisplay);

                    DialogueStyle style = AssetDatabase.LoadAssetAtPath<DialogueStyle>(menuTarget.ToString());

                    DialogueStyleManager.ApplyStyle(style, dialogueDisplay);
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
            }
        }

        private static void RecordUndoableChanges(DialogueDisplay dialogueDisplay)
        {
            try
            {
                Undo.RecordObject(dialogueDisplay, "Switched dialogue display style");
                Undo.RecordObject(dialogueDisplay.GetConversationDisplay(), "Switched conversation display style");
                Undo.RecordObject(dialogueDisplay.GetOptionDisplay(), "Switched option display style");
                Undo.RecordObject(dialogueDisplay.GetConversationDisplay().StandardConvoText, "Switched conversation panel text style");

#if TEXTMESHPRO_INSTALLED
                Undo.RecordObject(dialogueDisplay.GetConversationDisplay().TMPConvoText, "Switched conversation panel TMP text style");
#endif

                foreach (Image image in dialogueDisplay.GetConversationDisplay().ConversationPanelImages)
                {
                    Undo.RecordObject(image, "Switched conversation panel background image style");
                }

                Undo.RecordObject(dialogueDisplay.GetConversationDisplay().CharacterNameBackgroundImage, "Switched character name background image style");
                Undo.RecordObject(dialogueDisplay.GetConversationDisplay().StandardCharacterNameText, "Switched character name text style");

#if TEXTMESHPRO_INSTALLED
                Undo.RecordObject(dialogueDisplay.GetConversationDisplay().TMPCharacterNameText, "Switched character name TMP text style");
#endif

                foreach (Image optionImage in dialogueDisplay.GetOptionDisplay().Images)
                {
                    Undo.RecordObject(optionImage, "Switched option panel background image style");
                }

                foreach (DialogueButton button in ((OptionDisplay)dialogueDisplay.GetOptionDisplay()).GetOptionButtons())
                {
                    Undo.RecordObject(button, "Switched option button style");
                    Undo.RecordObject(button.backgroundImage, "Switched option button background image style");

                    Undo.RecordObject(button.StandardText, "Switched option button text style");

#if TEXTMESHPRO_INSTALLED
                    Undo.RecordObject(button.TMPText, "Switched option button text style");
#endif
                }

                Undo.RecordObject(dialogueDisplay.GetContinueDisplay().BackgroundImage, "Switched continue panel background image style");
                Undo.RecordObject(dialogueDisplay.GetContinueDisplay().StandardText, "Switched continue panel text style");

#if TEXTMESHPRO_INSTALLED
                Undo.RecordObject(dialogueDisplay.GetContinueDisplay().TMPText, "Switched continue panel text style");
#endif
            }
            catch (Exception e)
            {
                Debug.LogWarning("Encountered an issue with recording undoable actions setting dialogue display style: " + e);
            }
        }

        private static void CreateStyleSaveButton(DialogueDisplay display)
        {
            EditorGUILayout.Separator();

            if (GUILayout.Button("Save Style..."))
            {
                try
                {
                    
                    string savePath = EditorUtility.SaveFilePanelInProject("Save Dialogue Display Style...", "NewDialogueStyle", "asset", "Save the style of the currently selected dialogue display.");

                    if (savePath != null && savePath.Length > 0)
                    {
                        EditorUtility.DisplayProgressBar("Saving style...", "Please wait while the style is being saved.", 0.5f);

                        string assetName = savePath;
                        string folder = savePath;
                        if (assetName.IndexOf("/") > 0)
                        {
                            assetName = assetName.Substring(assetName.LastIndexOf("/") + 1);
                        }

                        if (assetName.Contains(".asset"))
                        {
                            assetName = assetName.Substring(0, assetName.IndexOf(".asset"));
                        }

                        if (folder.IndexOf("/") > 0)
                        {
                            folder = folder.Substring(0, folder.LastIndexOf("/"));
                        }

                        DialogueStyle style = DialogueStyleManager.CreateStyle(display);
                        AssetDatabase.CreateAsset(style, folder + "/" + assetName + ".asset");
                        AssetDatabase.SaveAssets();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("There was a problem with saving the style: " + e);
                }

                EditorUtility.ClearProgressBar();
            }
        }

        private void CreateConvoPanelTextSettings(ConversationDisplay convoDisplay)
        {
            EditorGUI.indentLevel++;
            expandConvoTextSettings = EditorGUILayout.Foldout(expandConvoTextSettings, new GUIContent("Conversation Text Settings"));
            if (expandConvoTextSettings)
            {
            #if TEXTMESHPRO_INSTALLED
                TMP_Text tmpText = convoDisplay.TMPConvoText;
            #endif

                Text text = convoDisplay.StandardConvoText;

            #if TEXTMESHPRO_INSTALLED
                ShowTextSettings(tmpText, text);
            #else
                ShowTextSettings(text);
            #endif
            }
            EditorGUI.indentLevel--;
        }

        private void CreateConvoImageSettings(ConversationDisplay convoDisplay)
        {
            EditorGUI.indentLevel = 1;
            expandConvoImageSettings = EditorGUILayout.Foldout(expandConvoImageSettings, new GUIContent("Image Settings"));
            if (expandConvoImageSettings)
            {
                for (int i = 0; i < convoDisplay.ConversationPanelImages.Count; i++)
                {
                    EditorGUI.indentLevel = 2;
                    Image convoImage = convoDisplay.ConversationPanelImages[i];

                    Undo.RecordObject(convoImage, "Modified convo panel image (" + i + ")");
                    EditorGUILayout.LabelField(new GUIContent("Image " + (i + 1) + " Settings"), EditorStyles.boldLabel);
                    ShowImageSettings(convoImage);
                }
            }
        }

        private void CreateCharacterNameSettings(ConversationDisplay convoDisplay)
        {
            EditorGUI.indentLevel = 1;
            expandCharacterNameSettings = EditorGUILayout.Foldout(expandCharacterNameSettings, new GUIContent("Character Name Settings"));
            if (expandCharacterNameSettings)
            {
                CreateCharacterNameTextSettings(convoDisplay);
                CreateCharacterNameBackgroundImageSettings(convoDisplay);
            }
        }

        private void CreateCharacterNameTextSettings(ConversationDisplay convoDisplay)
        {
            EditorGUI.indentLevel++;
            expandCharacterNameTextSettings = EditorGUILayout.Foldout(expandCharacterNameTextSettings, new GUIContent("Text Settings"));
            if (expandCharacterNameTextSettings)
            {
                Text text = convoDisplay.StandardCharacterNameText;

            #if TEXTMESHPRO_INSTALLED
                TMP_Text tmpText = convoDisplay.TMPCharacterNameText;
            #endif

            #if TEXTMESHPRO_INSTALLED
                ShowTextSettings(tmpText, text);
            #else
                ShowTextSettings(text);
            #endif
            }
            EditorGUI.indentLevel--;
        }

        private void CreateCharacterNameBackgroundImageSettings(ConversationDisplay convoDisplay)
        {
            EditorGUI.indentLevel++;
            expandCharacterNameImageSettings = EditorGUILayout.Foldout(expandCharacterNameImageSettings, new GUIContent("Background Image Settings"));
            if (expandCharacterNameImageSettings)
            {
                if (convoDisplay.CharacterNameBackgroundImage != null)
                {
                    Image characterNameBackgroundImage = convoDisplay.CharacterNameBackgroundImage;
                    ShowImageSettings(characterNameBackgroundImage);
                }
            }
            EditorGUI.indentLevel--;
        }

        private void CreateOptionDisplaySettings(DialogueDisplay display)
        {
            EditorGUI.indentLevel = 0;
            OptionDisplay optionDisplay = display.GetOptionDisplay() as OptionDisplay;

            if (optionDisplay != null)
            {
                expandOptionSettings = EditorGUILayout.BeginFoldoutHeaderGroup(expandOptionSettings, new GUIContent("Option Panel Style"));

                if (expandOptionSettings)
                {
                    CreateOptionPanelImageSettings(optionDisplay);

                    List<DialogueButton> buttons = optionDisplay.GetOptionButtons();
                    CreateOptionButtonTextSettings(buttons);
                    CreateOptionButtonImageSettings(buttons);
                    CreateOptionButtonAudioSettings(buttons);

                    if(optionDisplay is DirectionalOptionDisplay)
                    {
                        ETGUI.DrawLineSeparator();
                        CreateDirectionalOptionDisplaySettings(optionDisplay as DirectionalOptionDisplay);
                    }
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        private void CreateDirectionalOptionDisplaySettings(DirectionalOptionDisplay directionalDisplay)
        {
            expandDirectionalSettings = EditorGUILayout.Foldout(expandDirectionalSettings, new GUIContent("Directional Display Settings"));

            if (expandDirectionalSettings)
            {
                EditorGUI.indentLevel++;
                if (directionalDisplay.MainImage != null)
                {
                    directionalDisplay.MainImage.color = EditorGUILayout.ColorField(new GUIContent("Main Image Color"), directionalDisplay.MainImage.color);
                }

                directionalDisplay.UseOptionButtonColors = EditorGUILayout.Toggle(new GUIContent("Inherit Option Button Colors"), directionalDisplay.UseOptionButtonColors);

                if (directionalDisplay.UseOptionButtonColors)
                {
                    List<DialogueButton> optionButtons = directionalDisplay.GetOptionButtons();

                    if (optionButtons.Count > 0 && optionButtons[0] != null) 
                    {
                        for (int i = 0; i < directionalDisplay.OptionElements.Count; i++)
                        {
                            Image linkImage = directionalDisplay.OptionElements[i].linkedImage;

                            if(linkImage != null)
                            {
                                linkImage.color = optionButtons[0].normalButtonColor;
                            }
                        }
                    }
                }
                else
                {
                    EditorGUI.indentLevel++;
                    DirectionalOptionElement element = directionalDisplay.OptionElements[0];

                    directionalDisplay.LinkNormalColor = EditorGUILayout.ColorField(new GUIContent("Link Normal Color"), directionalDisplay.LinkNormalColor);
                    directionalDisplay.LinkHighlightColor = EditorGUILayout.ColorField(new GUIContent("Link Highlight Color"), directionalDisplay.LinkHighlightColor);
                    directionalDisplay.LinkPressedColor = EditorGUILayout.ColorField(new GUIContent("Link Pressed Color"), directionalDisplay.LinkPressedColor);
                    directionalDisplay.LinkDisabledColor = EditorGUILayout.ColorField(new GUIContent("Link Disabled Color"), directionalDisplay.LinkDisabledColor);

                    for (int i = 0; i < directionalDisplay.OptionElements.Count; i++)
                    {
                        Image linkImage = directionalDisplay.OptionElements[i].linkedImage;

                        if (linkImage != null)
                        {
                            linkImage.color = directionalDisplay.LinkNormalColor;
                        }
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }

        private void CreateSpriteFieldForImage(Image image, string labelText)
        {
            Sprite sprite = EditorGUILayout.ObjectField(labelText, image.sprite, typeof(Sprite), true, GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Sprite;
            if(image.sprite != sprite)
            {
                image.sprite = sprite;
                EditorUtility.SetDirty(image);
            }
        }

        private void CreateOptionPanelImageSettings(OptionDisplay optionDisplay)
        {
            EditorGUI.indentLevel++;
            if (optionDisplay.Images != null)
            {
                expandOptionPanelSettings = EditorGUILayout.Foldout(expandOptionPanelSettings, new GUIContent("Image Settings"));
                if (expandOptionPanelSettings)
                {
                    for(int i = 0; i < optionDisplay.Images.Count; i++)
                    {
                        EditorGUI.indentLevel++;
                        Image optionPanelImage = optionDisplay.Images[i];
                        EditorGUILayout.LabelField(new GUIContent("Image " + (i + 1) + " Settings"), EditorStyles.boldLabel);
                        if (ShowImageSettings(optionPanelImage))
                        {
                            EditorUtility.SetDirty(optionDisplay);
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }
            EditorGUI.indentLevel--;
        }

        private void CreateOptionButtonTextSettings(List<DialogueButton> buttons)
        {
            DialogueButton optionButton = buttons[0];

            if (optionButton != null)
            {
                EditorGUI.indentLevel++;
                expandOptionButtonTextSettings = EditorGUILayout.Foldout(expandOptionButtonTextSettings, new GUIContent("Button Text Settings"));

                if (expandOptionButtonTextSettings)
                {
                    ShowButtonTextSettings(optionButton);
                    ApplyTextSettingsToButtons(buttons, optionButton);
                }

                EditorGUI.indentLevel--;
            }
        }

        private void CreateOptionButtonAudioSettings(List<DialogueButton> buttons)
        {
            DialogueButton optionButton = buttons[0];

            if(optionButton != null)
            {
                EditorGUI.indentLevel++;
                expandOptionButtonAudioSettings = EditorGUILayout.Foldout(expandOptionButtonAudioSettings, new GUIContent("Button Audio Settings"));

                if (expandOptionButtonAudioSettings)
                {
                    EditorGUI.indentLevel++;
                    AudioClip audioClip = EditorGUILayout.ObjectField("Hover Sound", optionButton.hoverSound, typeof(AudioClip), true) as AudioClip;

                    foreach (DialogueButton button in buttons)
                    {
                        button.hoverSound = audioClip;
                        EditorUtility.SetDirty(button);
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }

        private void ShowButtonTextSettings(DialogueButton button)
        {
        #if TEXTMESHPRO_INSTALLED
            TMP_Text tmpText = button.TMPText;
        #endif
            Text text = button.StandardText;

        #if TEXTMESHPRO_INSTALLED
            ShowTextSettings(tmpText, text, false);
        #else
            ShowTextSettings(text, false);
        #endif

            EditorGUI.indentLevel++;
            SerializedObject buttonObj = new SerializedObject(button);
            EditorGUILayout.PropertyField(buttonObj.FindProperty("normalTextColor"));
            EditorGUILayout.PropertyField(buttonObj.FindProperty("disabledTextColor"));
            EditorGUILayout.PropertyField(buttonObj.FindProperty("highlightedTextColor"));
            EditorGUILayout.PropertyField(buttonObj.FindProperty("pressedTextColor"));
            buttonObj.ApplyModifiedProperties();

            Color normalTextColor = buttonObj.FindProperty("normalTextColor").colorValue;
            if ( button.StandardText != null && button.StandardText.color != normalTextColor)
            {
                button.StandardText.color = normalTextColor;
                EditorUtility.SetDirty(button.StandardText);
            }

#if TEXTMESHPRO_INSTALLED
            if (button.TMPText != null && button.TMPText.color != normalTextColor)
            {
                button.TMPText.color = normalTextColor;
                EditorUtility.SetDirty(button.TMPText);
            }
#endif

            EditorGUI.indentLevel--;
        }

        private void ApplyTextSettingsToButtons(List<DialogueButton> buttons, DialogueButton fromButton)
        {
            for (int i = 1; i < buttons.Count; i++)
            {
                DialogueButton button = buttons[i];

                if (fromButton.StandardText != null && button.StandardText != null)
                {
                    ApplyTextSettingsToText(fromButton.StandardText, button.StandardText);
                }

#if TEXTMESHPRO_INSTALLED
                if (fromButton.TMPText != null && button.TMPText != null)
                {
                    ApplyTextSettingsToTMPText(fromButton.TMPText, button.TMPText);
                }
#endif

                SerializedObject buttonObj = new SerializedObject(button);
                buttonObj.FindProperty("normalTextColor").colorValue = fromButton.normalTextColor;
                buttonObj.FindProperty("disabledTextColor").colorValue = fromButton.disabledTextColor;
                buttonObj.FindProperty("highlightedTextColor").colorValue = fromButton.highlightedTextColor;
                buttonObj.FindProperty("pressedTextColor").colorValue = fromButton.pressedTextColor;
                buttonObj.ApplyModifiedProperties();

                if(button.StandardText != null && button.StandardText.color != fromButton.StandardText.color)
                {
                    button.StandardText.color = fromButton.StandardText.color;
                    EditorUtility.SetDirty(button.StandardText);
                }

#if TEXTMESHPRO_INSTALLED
                if (button.TMPText != null && button.TMPText.color != fromButton.TMPText.color)
                {
                    button.TMPText.color = fromButton.TMPText.color;
                    EditorUtility.SetDirty(button.TMPText);
                }
#endif
            }
        }

        private void ApplyImageSettingsToButtons(List<DialogueButton> buttons, DialogueButton fromButton)
        {
            for (int i = 1; i < buttons.Count; i++)
            {
                DialogueButton button = buttons[i];

                SerializedObject buttonObj = new SerializedObject(button);
                buttonObj.FindProperty("normalButtonColor").colorValue = fromButton.normalButtonColor;
                buttonObj.FindProperty("disabledButtonColor").colorValue = fromButton.disabledButtonColor;
                buttonObj.FindProperty("highlightedButtonColor").colorValue = fromButton.highlightedButtonColor;
                buttonObj.FindProperty("pressedButtonColor").colorValue = fromButton.pressedButtonColor;
                buttonObj.ApplyModifiedProperties();
                
                if (button.backgroundImage.enabled != fromButton.backgroundImage.enabled)
                {
                    button.backgroundImage.enabled = fromButton.backgroundImage.enabled;
                    EditorUtility.SetDirty(button);
                }

                if(button.backgroundImage.color != fromButton.backgroundImage.color)
                {
                    button.backgroundImage.color = fromButton.backgroundImage.color;
                    EditorUtility.SetDirty(button.backgroundImage);
                }

                if(button.backgroundImage.sprite != fromButton.backgroundImage.sprite)
                {
                    button.backgroundImage.sprite = fromButton.backgroundImage.sprite;
                    EditorUtility.SetDirty(button.backgroundImage);
                }

                if(button.backgroundImage.type != fromButton.backgroundImage.type)
                {
                    button.backgroundImage.type = fromButton.backgroundImage.type;
                    EditorUtility.SetDirty(button.backgroundImage);
                }

                if(button.backgroundImage.pixelsPerUnitMultiplier != fromButton.backgroundImage.pixelsPerUnitMultiplier)
                {
                    button.backgroundImage.pixelsPerUnitMultiplier = fromButton.backgroundImage.pixelsPerUnitMultiplier;
                    EditorUtility.SetDirty(button.backgroundImage);
                }
            }
        }

#if TEXTMESHPRO_INSTALLED
        private void ApplyTextSettingsToTMPText(TMP_Text fromText, TMP_Text toText)
        {
            Undo.RecordObject(toText, "Modified text");

            if (toText.color != fromText.color)
            {
                toText.color = fromText.color;
                EditorUtility.SetDirty(toText);
            }

            if (toText.font != fromText.font)
            {
                toText.font = fromText.font;
                EditorUtility.SetDirty(toText);
            }

            if (toText.fontSize != fromText.fontSize)
            {
                toText.fontSize = fromText.fontSize;
                EditorUtility.SetDirty(toText);
            }

            if (toText.enableAutoSizing != fromText.enableAutoSizing)
            {
                toText.enableAutoSizing = fromText.enableAutoSizing;
                EditorUtility.SetDirty(toText);
            }

            if (toText.fontSizeMin != fromText.fontSizeMin)
            {
                toText.fontSizeMin = fromText.fontSizeMin;
                EditorUtility.SetDirty(toText);
            }

            if (toText.fontSizeMax != fromText.fontSizeMax)
            {
                toText.fontSizeMax = fromText.fontSizeMax;
                EditorUtility.SetDirty(toText);
            }
        }
#endif

        private void ApplyTextSettingsToText(Text fromText, Text toText)
        {
            Undo.RecordObject(toText, "Modified text");

            if (toText.color != fromText.color)
            {
                toText.color = fromText.color;
                EditorUtility.SetDirty(toText);
            }

            if (toText.font != fromText.font)
            {
                toText.font = fromText.font;
                EditorUtility.SetDirty(toText);
            }

            if (toText.fontSize != fromText.fontSize)
            {
                toText.fontSize = fromText.fontSize;
                EditorUtility.SetDirty(toText);
            }

            if(toText.resizeTextForBestFit != fromText.resizeTextForBestFit)
            {
                toText.resizeTextForBestFit = fromText.resizeTextForBestFit;
                EditorUtility.SetDirty(toText);
            }

            if(toText.resizeTextMinSize != fromText.resizeTextMinSize)
            {
                toText.resizeTextMinSize = fromText.resizeTextMinSize;
                EditorUtility.SetDirty(toText);
            }

            if(toText.resizeTextMaxSize != fromText.resizeTextMaxSize)
            {
                toText.resizeTextMaxSize = fromText.resizeTextMaxSize;
                EditorUtility.SetDirty(toText);
            }
        }

        private void CreateOptionButtonImageSettings(List<DialogueButton> buttons)
        {
            DialogueButton optionButton = buttons[0];

            if (optionButton.backgroundImage != null)
            {
                EditorGUI.indentLevel++;
                expandOptionButtonImageSettings = EditorGUILayout.Foldout(expandOptionButtonImageSettings, new GUIContent("Button Image Settings"));
                if (expandOptionButtonImageSettings)
                {
                    ShowButtonImageSettings(optionButton);
                    ApplyImageSettingsToButtons(buttons, optionButton);
                }
                EditorGUI.indentLevel--;
            }
        }

        private void ShowButtonImageSettings(DialogueButton button)
        {
            Image buttonImage = button.backgroundImage;

            ShowImageSettings(buttonImage, false);

            if (buttonImage != null && buttonImage.enabled)
            {
                if (buttonImage.enabled)
                {
                    EditorGUI.indentLevel++;
                    SerializedObject optionButtonObj = new SerializedObject(button);
                    EditorGUILayout.PropertyField(optionButtonObj.FindProperty("normalButtonColor"), new GUIContent("Normal Color"));
                    EditorGUILayout.PropertyField(optionButtonObj.FindProperty("disabledButtonColor"), new GUIContent("Disabled Color"));
                    EditorGUILayout.PropertyField(optionButtonObj.FindProperty("highlightedButtonColor"), new GUIContent("Highlighted Color"));
                    EditorGUILayout.PropertyField(optionButtonObj.FindProperty("pressedButtonColor"), new GUIContent("Pressed Color"));
                    optionButtonObj.ApplyModifiedProperties();

                    Color normalButtonColor = optionButtonObj.FindProperty("normalButtonColor").colorValue;

                    if (buttonImage.color != normalButtonColor)
                    {
                        buttonImage.color = normalButtonColor;
                        EditorUtility.SetDirty(buttonImage);
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }

        private bool ShowImageSettings(Image image, bool showColor = true)
        {
            bool imagesChanged = false;

            if (image != null)
            {
                Undo.RecordObject(image, "Modified panel image");
                EditorGUI.indentLevel++;

                bool imageEnabled = EditorGUILayout.Toggle("Enabled", image.enabled, new GUILayoutOption[] { });
                if (image.enabled != imageEnabled)
                {
                    imagesChanged = true;
                    image.enabled = imageEnabled;
                    EditorUtility.SetDirty(image);
                }

                if (image.enabled)
                {
                    Sprite sprite = EditorGUILayout.ObjectField("Image", image.sprite, typeof(Sprite), true, GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Sprite;
                    if (image.sprite != sprite)
                    {
                        imagesChanged = true;
                        image.sprite = sprite;
                        EditorUtility.SetDirty(image);
                    }

                    if (showColor)
                    {
                        Color bgColor = EditorGUILayout.ColorField(new GUIContent("Image Tint"), image.color, null);
                        if (image.color != bgColor)
                        {
                            imagesChanged = true;
                            image.color = bgColor;
                            EditorUtility.SetDirty(image);
                        }
                    }

                    Image.Type imageType = (Image.Type)EditorGUILayout.EnumPopup(new GUIContent("Image Type"), image.type, new GUILayoutOption[] { });
                    if (imageType != image.type)
                    {
                        imagesChanged = true;
                        image.type = imageType;
                        EditorUtility.SetDirty(image);
                    }

                    float pixelsPerUnit = EditorGUILayout.FloatField(new GUIContent("Pixels Per Unit"), image.pixelsPerUnitMultiplier, new GUILayoutOption[] { });
                    if (image.pixelsPerUnitMultiplier != pixelsPerUnit)
                    {
                        imagesChanged = true;
                        image.pixelsPerUnitMultiplier = pixelsPerUnit;
                        EditorUtility.SetDirty(image);
                    }
                }

                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.LabelField(new GUIContent("Image is null. Check Option Display Images..."), EditorStyles.boldLabel);
            }

            return imagesChanged;
        }


#if TEXTMESHPRO_INSTALLED
        private void ShowTextSettings(TMP_Text tmpText, Text text, bool showColor = true)
        {
            if (tmpText != null)
            {
                Undo.RecordObject(text, "Modified panel text");
                Undo.RecordObject(tmpText, "Modified panel TMP text");

                EditorGUI.indentLevel++;

                bool enabled = false;
                if(tmpText != null) { enabled = tmpText.enabled; }
                else if(text != null ) { enabled = text.enabled; }

                bool textEnabled = EditorGUILayout.Toggle("Enabled", enabled);
                if (enabled != textEnabled)
                {
                    if (text != null) 
                    { 
                        text.enabled = textEnabled;
                        EditorUtility.SetDirty(text);
                    }

                    if (tmpText != null) 
                    { 
                        tmpText.enabled = textEnabled;
                        EditorUtility.SetDirty(tmpText);
                    }
                }
                
                if (textEnabled)
                {
                    if (showColor)
                    {
                        Color color = Color.white;
                        if(tmpText != null) { color = tmpText.color; }
                        else if(text != null) { color = text.color; }

                        Color textColor = EditorGUILayout.ColorField(new GUIContent("Text Color"), color, new GUILayoutOption[] { });
                        if (color != textColor)
                        {
                            if (text != null)
                            {
                                text.color = textColor;
                                EditorUtility.SetDirty(text);
                            }

                            if (tmpText != null)
                            {
                                tmpText.color = textColor;
                                EditorUtility.SetDirty(tmpText);
                            }
                        }
                    }

                    ShowFontSettingsForText(text, false);
                    ShowFontSettingsForTMPText(text, tmpText, false);

                    if (tmpText != null)
                    {
                        ShowSizeSettingsForTMPText(text, tmpText);
                    }
                    else if(text != null)
                    {
                        ShowSizeSettingsForText(text);
                    }
                }

                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.LabelField("Text is null.");
            }
        }
#endif

        private void ShowTextSettings(Text text, bool showColor = true)
        {
            if (text != null)
            {
                Undo.RecordObject(text, "Modified panel text");
                EditorGUI.indentLevel++;

                bool textEnabled = EditorGUILayout.Toggle("Enabled", text.enabled);
                if (text.enabled != textEnabled)
                {
                    text.enabled = textEnabled;
                    EditorUtility.SetDirty(text);
                }

                if (textEnabled)
                {
                    if (showColor)
                    {
                        Color textColor = EditorGUILayout.ColorField(new GUIContent("Text Color"), text.color, new GUILayoutOption[] { });
                        if (text.color != textColor)
                        {
                            text.color = textColor;
                            EditorUtility.SetDirty(text);
                        }
                    }

                    ShowFontSettingsForText(text);
                }

                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.LabelField("Text is null.");
            }
        }

#if TEXTMESHPRO_INSTALLED
        private void ShowFontSettingsForTMPText(Text text, TMP_Text tmpText, bool includeSizeSettings = true)
        {
            if (tmpText != null)
            {
                TMP_FontAsset textFont = EditorGUILayout.ObjectField("TextMeshPro Font", tmpText.font, typeof(TMP_FontAsset), true, null) as TMP_FontAsset;
                if (tmpText.font != textFont)
                {
                    tmpText.font = textFont;
                    EditorUtility.SetDirty(tmpText);
                }

                if (includeSizeSettings)
                {
                    ShowSizeSettingsForTMPText(text, tmpText);
                }
            }
        }

        private void ShowSizeSettingsForTMPText(Text text, TMP_Text tmpText)
        {
            if (tmpText != null)
            {
                bool enableTextAutosizing = EditorGUILayout.BeginToggleGroup(new GUIContent("Auto-size Text?"), tmpText.enableAutoSizing);

                if (tmpText.enableAutoSizing != enableTextAutosizing)
                {
                    if (text != null)
                    {
                        text.resizeTextForBestFit = enableTextAutosizing;
                        EditorUtility.SetDirty(text);
                    }

                    tmpText.enableAutoSizing = enableTextAutosizing;
                    EditorUtility.SetDirty(tmpText);
                }

                if (enableTextAutosizing)
                {
                    EditorGUI.indentLevel++;
                    float minFontSize = EditorGUILayout.FloatField(new GUIContent("Min Font Size"), tmpText.fontSizeMin, new GUILayoutOption[] { });
                    if (tmpText.fontSizeMin != minFontSize)
                    {
                        if (text != null)
                        {
                            text.resizeTextMinSize = (int)minFontSize;
                            EditorUtility.SetDirty(text);
                        }

                        tmpText.fontSizeMin = minFontSize;
                        EditorUtility.SetDirty(tmpText);
                    }

                    float maxFontSize = EditorGUILayout.FloatField(new GUIContent("Max Font Size"), tmpText.fontSizeMax, new GUILayoutOption[] { });
                    if (tmpText.fontSizeMax != maxFontSize)
                    {
                        if (text != null)
                        {
                            text.resizeTextMaxSize = (int)maxFontSize;
                            EditorUtility.SetDirty(text);
                        }

                        tmpText.fontSizeMax = maxFontSize;
                        EditorUtility.SetDirty(tmpText);
                    }
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndToggleGroup();

                if (!enableTextAutosizing)
                {
                    EditorGUI.indentLevel++;

                    float fontSize = EditorGUILayout.FloatField(new GUIContent("Font Size"), tmpText.fontSize, new GUILayoutOption[] { });
                    if (tmpText.fontSize != fontSize)
                    {
                        if (text != null)
                        {
                            text.fontSize = (int)fontSize;
                            EditorUtility.SetDirty(text);
                        }

                        tmpText.fontSize = fontSize;
                        EditorUtility.SetDirty(tmpText);
                    }

                    EditorGUI.indentLevel--;
                }
            }
            else if(text != null)
            {
                ShowFontSettingsForText(text);
            }
        }
#endif

        private void ShowFontSettingsForText(Text text, bool includeSizeSettings = true)
        {
            if (text != null)
            {
                Font textFont = EditorGUILayout.ObjectField("Font", text.font, typeof(Font), true, null) as Font;
                if (text.font != textFont)
                {
                    text.font = textFont;
                    EditorUtility.SetDirty(text);
                }

                if (includeSizeSettings)
                {
                    ShowSizeSettingsForText(text);
                }
            }
        }

        private static void ShowSizeSettingsForText(Text text)
        {
            if (text != null)
            {
                bool resizeForBestFit = EditorGUILayout.BeginToggleGroup(new GUIContent("Auto-size Text?"), text.resizeTextForBestFit);
                if (text.resizeTextForBestFit != resizeForBestFit)
                {
                    text.resizeTextForBestFit = resizeForBestFit;
                    EditorUtility.SetDirty(text);
                }

                if (resizeForBestFit)
                {
                    EditorGUI.indentLevel++;
                    int resetTextMinSize = EditorGUILayout.IntField(new GUIContent("Min Font Size"), text.resizeTextMinSize);
                    if (text.resizeTextMinSize != resetTextMinSize)
                    {
                        text.resizeTextMinSize = resetTextMinSize;
                        EditorUtility.SetDirty(text);
                    }

                    int resetTextMaxSize = EditorGUILayout.IntField(new GUIContent("Max Font Size"), text.resizeTextMaxSize);
                    if (text.resizeTextMaxSize != resetTextMaxSize)
                    {
                        text.resizeTextMaxSize = resetTextMaxSize;
                        EditorUtility.SetDirty(text);
                    }
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndToggleGroup();

                if (!text.resizeTextForBestFit)
                {
                    EditorGUI.indentLevel++;

                    float fontSize = EditorGUILayout.FloatField(new GUIContent("Font Size"), text.fontSize, new GUILayoutOption[] { });
                    if (text.fontSize != fontSize)
                    {
                        text.fontSize = (int)fontSize;
                        EditorUtility.SetDirty(text);
                    }

                    EditorGUI.indentLevel--;
                }
            }
        }
    }

    public enum SpriteDisplayType 
    { 
        SIMPLE, SLICED, TILED, FILLED
    }
}