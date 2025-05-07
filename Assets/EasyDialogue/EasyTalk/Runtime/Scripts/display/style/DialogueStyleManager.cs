using System.Collections.Generic;
using System.Xml.Linq;

#if TEXTMESHPRO_INSTALLED
using TMPro;
#endif

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EasyTalk.Display.Style
{
    /// <summary>
    /// The DialogueStyleManager is used to modify the style of a Dialogue Display more easily than having to access individual components within the display.
    /// </summary>
    public class DialogueStyleManager
    {
        /// <summary>
        /// Applies the specified DialogueStyle to the DialogueDisplay provided.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <param name="display">The dialogue display to update.</param>
        public static void ApplyStyle(DialogueStyle style, DialogueDisplay display)
        {
            if (display.GetConversationDisplay() != null && style.conversationStyle != null)
            {
                ApplyStyleToConvoDisplay(style.conversationStyle, display.GetConversationDisplay());
            }

            if (display.GetOptionDisplay() != null && style.optionStyle != null)
            {
                ApplyStyleToOptionDisplay(style.optionStyle, display.GetOptionDisplay());
            }

            if(display.GetContinueDisplay() != null && style.continueStyle != null)
            {
                ApplyStyleToContinuePanel(style.continueStyle, display.GetContinueDisplay());
            }
        }

        /// <summary>
        /// Applies the specified OptionDisplayStyle to the provided OptionDisplay.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <param name="display">The option display to update.</param>
        public static void ApplyStyleToOptionDisplay(OptionDisplayStyle style, OptionDisplay display)
        {
            if (style == null) { return; }

            ApplyStyleToOptionPanel(style, display);

            foreach (DialogueButton button in display.GetOptionButtons())
            {
                ApplyStyleToOptionButton(style.optionButtonStyle, button);
            }

            ApplyStyleToDirectionalOptionDisplay(style, display);
        }

        /// <summary>
        /// Applies the specified OptionDisplayStyle to the provided OptionDisplay if it is a Directional Option Display.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <param name="display">The option display to update (changes only applied if it is a directional option display).</param>
        public static void ApplyStyleToDirectionalOptionDisplay(OptionDisplayStyle style, OptionDisplay display)
        {
            if (display is DirectionalOptionDisplay)
            {
                DirectionalOptionDisplay directionalDisplay = display as DirectionalOptionDisplay;

                if (directionalDisplay.MainImage != null)
                {
                    directionalDisplay.MainImage.color = style.directionalStyle.mainImageColor;

#if UNITY_EDITOR
                    EditorUtility.SetDirty(directionalDisplay.MainImage);
#endif
                }

                if (directionalDisplay.UseOptionButtonColors)
                {
                    foreach (DirectionalOptionElement element in directionalDisplay.OptionElements)
                    {
                        if (element != null && element.linkedImage != null && element.button != null)
                        {
                            element.linkedImage.color = element.button.normalButtonColor;

#if UNITY_EDITOR
                            EditorUtility.SetDirty(element.linkedImage);
#endif
                        }
                    }
                }
                else
                {
                    foreach (DirectionalOptionElement element in directionalDisplay.OptionElements)
                    {
                        if (element != null && element.linkedImage != null)
                        {
                            element.linkedImage.color = style.directionalStyle.normalColor;

#if UNITY_EDITOR
                            EditorUtility.SetDirty(element.linkedImage);
#endif
                        }
                    }
                }

                directionalDisplay.LinkNormalColor = style.directionalStyle.normalColor;
                directionalDisplay.LinkHighlightColor = style.directionalStyle.highlightColor;
                directionalDisplay.LinkPressedColor = style.directionalStyle.pressedColor;
                directionalDisplay.LinkDisabledColor = style.directionalStyle.disabledColor;

#if UNITY_EDITOR
                EditorUtility.SetDirty(directionalDisplay);
#endif
            }
        }

        /// <summary>
        /// Applies the specified ConversationDisplayStyle to the ConversationDisplay provided.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <param name="display">The conversation display to update.</param>
        public static void ApplyStyleToConvoDisplay(ConversationDisplayStyle style, ConversationDisplay display)
        {
            if (style == null) { return; }

            ApplyStyleToCharacterNameText(style, display);
            ApplyStyleToCharacterNameBackgroundImage(style, display.CharacterNameBackgroundImage);
            ApplyStyleToConvoImages(style, display);
            ApplyStyleToConvoText(style, display);
        }

        /// <summary>
        /// Applies styling to the character name text provided.
        /// </summary>
        /// <param name="convoStyle">The style to apply.</param>
        /// <param name="display">The Conversation Display to update.</param>
        protected static void ApplyStyleToCharacterNameText(ConversationDisplayStyle convoStyle, ConversationDisplay display)
        {
            ApplyStyleToText(convoStyle.characterNameStyle.textStyleSettings, display.StandardCharacterNameText);

#if TEXTMESHPRO_INSTALLED
            ApplyStyleToTMPText(convoStyle.characterNameStyle.textStyleSettings, display.TMPCharacterNameText);
#endif
        }

        /// <summary>
        /// Applies styling to the character name background image provided.
        /// </summary>
        /// <param name="convoStyle">The style to apply.</param>
        /// <param name="backgroundImage">The background image to update.</param>
        protected static void ApplyStyleToCharacterNameBackgroundImage(ConversationDisplayStyle convoStyle, Image backgroundImage)
        {
            if (backgroundImage != null && convoStyle != null)
            {
                backgroundImage.enabled = convoStyle.characterNameStyle.imageStyleSettings.enabled;
                backgroundImage.sprite = convoStyle.characterNameStyle.imageStyleSettings.sprite;
                backgroundImage.color = convoStyle.characterNameStyle.imageStyleSettings.color;
                backgroundImage.type = convoStyle.characterNameStyle.imageStyleSettings.imageType;
                backgroundImage.pixelsPerUnitMultiplier = convoStyle.characterNameStyle.imageStyleSettings.pixelsPerUnit;

            #if UNITY_EDITOR
                EditorUtility.SetDirty(backgroundImage);
            #endif
            }
        }

        /// <summary>
        /// Applies styling to the provided dialogue/conversation text.
        /// </summary>
        /// <param name="convoStyle">The style to apply.</param>
        /// <param name="display">The Conversation Display to update.</param>
        protected static void ApplyStyleToConvoText(ConversationDisplayStyle convoStyle, ConversationDisplay display)
        {
            ApplyStyleToText(convoStyle.convoTextStyle, display.StandardConvoText);

#if TEXTMESHPRO_INSTALLED
            ApplyStyleToTMPText(convoStyle.convoTextStyle, display.TMPConvoText);
#endif
        }

        /// <summary>
        /// Apply the style of the provided TextStyleSettings to the Unity Text component specified.
        /// </summary>
        /// <param name="textStyleSettings">The text style settings to apply.</param>
        /// <param name="text">The Text component to update.</param>
        protected static void ApplyStyleToText(TextStyleSettings textStyleSettings, Text text)
        {
            if (text != null && textStyleSettings != null)
            {
                text.fontSize = (int)textStyleSettings.fontSize;
                text.color = textStyleSettings.color;

                if (textStyleSettings.StandardFont != null)
                {
                    text.font = textStyleSettings.StandardFont;
                }

                if (textStyleSettings.autoSizeFont)
                {
                    text.resizeTextForBestFit = true;
                    text.resizeTextMinSize = (int)textStyleSettings.minFontSize;
                    text.resizeTextMaxSize = (int)textStyleSettings.maxFontSize;
                }
                else
                {
                    text.resizeTextForBestFit = false;
                }

#if UNITY_EDITOR
                EditorUtility.SetDirty(text);
#endif
            }
        }

#if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// Apply the style of the provided TextStyleSettings to the TextMeshPro Text component specified.
        /// </summary>
        /// <param name="textStyleSettings">The text style settings to apply.</param>
        /// <param name="text">The TextMeshPro component to update.</param>
        protected static void ApplyStyleToTMPText(TextStyleSettings textStyleSettings, TMP_Text text)
        {
            if (text != null && textStyleSettings != null)
            {
                text.fontSize = textStyleSettings.fontSize;
                text.color = textStyleSettings.color;

                if (textStyleSettings.TMPFont != null)
                {
                    text.font = textStyleSettings.TMPFont;
                }

                if (textStyleSettings.autoSizeFont)
                {
                    text.enableAutoSizing = true;
                    text.fontSizeMin = textStyleSettings.minFontSize;
                    text.fontSizeMax = textStyleSettings.maxFontSize;
                }
                else
                {
                    text.enableAutoSizing = false;
                }

#if UNITY_EDITOR
                EditorUtility.SetDirty(text);
#endif
            }
        }
#endif

        /// <summary>
        /// Applies styling to the images of the conversation display provided.
        /// </summary>
        /// <param name="convoStyle">The style to apply.</param>
        /// <param name="convoDisplay">The conversation display to update.</param>
        protected static void ApplyStyleToConvoImages(ConversationDisplayStyle convoStyle, ConversationDisplay convoDisplay)
        {
            if (convoStyle != null && convoDisplay != null)
            {
                for (int imageStyleIdx = 0; imageStyleIdx < convoStyle.convoImageStyles.Count; imageStyleIdx++)
                {
                    ImageStyleSettings imageStyle = convoStyle.convoImageStyles[imageStyleIdx];

                    if (convoDisplay.ConversationPanelImages.Count > imageStyleIdx)
                    {
                        Image convoImage = convoDisplay.ConversationPanelImages[imageStyleIdx];

                        if (convoImage != null)
                        {
                            convoImage.enabled = imageStyle.enabled;
                            convoImage.sprite = imageStyle.sprite;
                            convoImage.color = imageStyle.color;
                            convoImage.type = imageStyle.imageType;
                            convoImage.pixelsPerUnitMultiplier = imageStyle.pixelsPerUnit;

                        #if UNITY_EDITOR
                            EditorUtility.SetDirty(convoImage);
                        #endif
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies styling to the option button provided.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <param name="button">The dialogue button to update.</param>
        protected static void ApplyStyleToOptionButton(OptionButtonStyle style, DialogueButton button)
        {
            if (button != null && style != null)
            {
                button.normalButtonColor = style.buttonStyleSettings.normalImageColor;
                button.highlightedButtonColor = style.buttonStyleSettings.highlightedImageColor;
                button.disabledButtonColor = style.buttonStyleSettings.disabledImageColor;
                button.pressedButtonColor = style.buttonStyleSettings.pressedImageColor;
                button.normalTextColor = style.buttonStyleSettings.normalTextColor;
                button.highlightedTextColor = style.buttonStyleSettings.highlightedTextColor;
                button.disabledTextColor = style.buttonStyleSettings.disabledTextColor;
                button.pressedTextColor = style.buttonStyleSettings.pressedTextColor;

                button.hoverSound = style.hoverSound;

            #if UNITY_EDITOR
                EditorUtility.SetDirty(button);
            #endif

                Image optionImage = button.backgroundImage;
                if (optionImage != null)
                {
                    if (style.imageStyleSettings.enabled)
                    {
                        optionImage.enabled = true;

                        optionImage.sprite = style.imageStyleSettings.sprite;
                        optionImage.color = style.imageStyleSettings.color;
                        optionImage.type = style.imageStyleSettings.imageType;
                        optionImage.pixelsPerUnitMultiplier = style.imageStyleSettings.pixelsPerUnit;
                    }
                    else
                    {
                        optionImage.enabled = false;
                    }

                #if UNITY_EDITOR
                    EditorUtility.SetDirty(optionImage);
                #endif
                }


                ApplyStyleToText(style.textStyleSettings, button.StandardText);

#if TEXTMESHPRO_INSTALLED
                ApplyStyleToTMPText(style.textStyleSettings, button.TMPText);
#endif
            }
        }

        /// <summary>
        /// Applies styling to the option display provided.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <param name="optionDisplay">The option display to update.</param>
        protected static void ApplyStyleToOptionPanel(OptionDisplayStyle style, OptionDisplay optionDisplay)
        {
            if (style != null)
            {
                for(int i = 0; i < style.optionPanelImageStyles.Count; i++)
                {
                    ImageStyleSettings optionImageStyle = style.optionPanelImageStyles[i];
                    if(optionImageStyle != null && optionDisplay.Images.Count > i)
                    {
                        Image optionImage = optionDisplay.Images[i];
                        if (optionImage != null)
                        {
                            optionImage.enabled = optionImageStyle.enabled;
                            optionImage.sprite = optionImageStyle.sprite;
                            optionImage.color = optionImageStyle.color;
                            optionImage.type = optionImageStyle.imageType;
                            optionImage.pixelsPerUnitMultiplier = optionImageStyle.pixelsPerUnit;

                        #if UNITY_EDITOR
                            EditorUtility.SetDirty(optionImage);
                        #endif
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies styling to the continue display provided.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <param name="continueDisplay">The continue display to update.</param>
        public static void ApplyStyleToContinuePanel(ContinueDisplayStyle style, ContinueDisplay continueDisplay)
        {
            Image backgroundImage = continueDisplay.BackgroundImage;
            if (continueDisplay.BackgroundImage != null)
            {
                backgroundImage.enabled = style.imageStyleSettings.enabled;
                backgroundImage.sprite = style.imageStyleSettings.sprite;
                backgroundImage.color = style.imageStyleSettings.color;
                backgroundImage.type = style.imageStyleSettings.imageType;
                backgroundImage.pixelsPerUnitMultiplier = style.imageStyleSettings.pixelsPerUnit;

                #if UNITY_EDITOR
                    EditorUtility.SetDirty(backgroundImage);
                #endif
            }

            ApplyStyleToText(style.textStyleSettings, continueDisplay.StandardText);

#if TEXTMESHPRO_INSTALLED
            ApplyStyleToTMPText(style.textStyleSettings, continueDisplay.TMPText);
#endif

            DialogueButton button = continueDisplay.GetComponent<DialogueButton>();
            if(button != null)
            {
                button.normalButtonColor = style.buttonStyleSettings.normalImageColor;
                button.pressedButtonColor = style.buttonStyleSettings.pressedImageColor;
                button.disabledButtonColor = style.buttonStyleSettings.disabledImageColor;
                button.highlightedButtonColor = style.buttonStyleSettings.highlightedImageColor;

                button.normalTextColor = style.buttonStyleSettings.normalTextColor;
                button.pressedTextColor = style.buttonStyleSettings.pressedTextColor;
                button.disabledTextColor = style.buttonStyleSettings.disabledTextColor;
                button.highlightedTextColor = style.buttonStyleSettings.highlightedTextColor;


            #if UNITY_EDITOR
                EditorUtility.SetDirty(button);
            #endif
            }
        }

        /// <summary>
        /// Creates a conversation display style from the provided conversation display.
        /// </summary>
        /// <param name="display">The conversation display to create a style from.</param>
        /// <returns>The new ConversationDisplayStyle.</returns>
        public static ConversationDisplayStyle CreateConversationDisplayStyle(ConversationDisplay display)
        {
            ConversationDisplayStyle convoStyle = new ConversationDisplayStyle();

            //Apply settings to convo text style.
            ApplyTextSettingsToStyle(convoStyle.convoTextStyle, display.StandardConvoText);

#if TEXTMESHPRO_INSTALLED
            ApplyTMPTextSettingsToStyle(convoStyle.convoTextStyle, display.TMPConvoText);
#endif

            //Apply settings to character name style.
            ApplyTextSettingsToStyle(convoStyle.characterNameStyle.textStyleSettings, display.StandardCharacterNameText);

#if TEXTMESHPRO_INSTALLED
            ApplyTMPTextSettingsToStyle(convoStyle.characterNameStyle.textStyleSettings, display.TMPCharacterNameText);
#endif

            if (display.ConversationPanelImages != null)
            {
                if (convoStyle.convoImageStyles == null)
                {
                    convoStyle.convoImageStyles = new List<ImageStyleSettings>();
                }

                for (int i = 0; i < display.ConversationPanelImages.Count; i++)
                {
                    ImageStyleSettings imageStyle = new ImageStyleSettings();
                    Image convoImage = display.ConversationPanelImages[i];

                    if (convoImage != null)
                    {
                        imageStyle.enabled = convoImage.enabled;
                        imageStyle.color = convoImage.color;
                        imageStyle.imageType = convoImage.type;
                        imageStyle.pixelsPerUnit = convoImage.pixelsPerUnitMultiplier;
                        imageStyle.sprite = convoImage.sprite;
                    }
                    else
                    {
                        imageStyle.enabled = false;
                    }

                    convoStyle.convoImageStyles.Add(imageStyle);
                }
            }

            if (display.StandardCharacterNameText != null)
            {
                convoStyle.characterNameStyle.textStyleSettings.enabled = display.StandardCharacterNameText.enabled;
                convoStyle.characterNameStyle.textStyleSettings.color = display.StandardCharacterNameText.color;
                convoStyle.characterNameStyle.textStyleSettings.StandardFont = display.StandardCharacterNameText.font;
                convoStyle.characterNameStyle.textStyleSettings.fontSize = display.StandardCharacterNameText.fontSize;
                convoStyle.convoTextStyle.autoSizeFont = display.StandardCharacterNameText.resizeTextForBestFit;
                convoStyle.convoTextStyle.minFontSize = display.StandardCharacterNameText.resizeTextMinSize;
                convoStyle.convoTextStyle.maxFontSize = display.StandardCharacterNameText.resizeTextMaxSize;
            }

#if TEXTMESHPRO_INSTALLED
            if (display.TMPCharacterNameText != null)
            {
                convoStyle.characterNameStyle.textStyleSettings.enabled = display.TMPCharacterNameText.enabled;
                convoStyle.characterNameStyle.textStyleSettings.color = display.TMPCharacterNameText.color;
                convoStyle.characterNameStyle.textStyleSettings.TMPFont = display.TMPCharacterNameText.font;
                convoStyle.characterNameStyle.textStyleSettings.fontSize = display.TMPCharacterNameText.fontSize;
                convoStyle.characterNameStyle.textStyleSettings.autoSizeFont = display.TMPCharacterNameText.enableAutoSizing;
                convoStyle.characterNameStyle.textStyleSettings.minFontSize = display.TMPCharacterNameText.fontSizeMin;
                convoStyle.characterNameStyle.textStyleSettings.maxFontSize = display.TMPCharacterNameText.fontSizeMax;
            }
#endif

            if (display.CharacterNameBackgroundImage != null)
            {
                convoStyle.characterNameStyle.imageStyleSettings.enabled = display.CharacterNameBackgroundImage.enabled;
                convoStyle.characterNameStyle.imageStyleSettings.sprite = display.CharacterNameBackgroundImage.sprite;
                convoStyle.characterNameStyle.imageStyleSettings.color = display.CharacterNameBackgroundImage.color;
                convoStyle.characterNameStyle.imageStyleSettings.imageType = display.CharacterNameBackgroundImage.type;
                convoStyle.characterNameStyle.imageStyleSettings.pixelsPerUnit = display.CharacterNameBackgroundImage.pixelsPerUnitMultiplier;
            }

            return convoStyle;
        }

        /// <summary>
        /// Creates a option display style from the provided option display.
        /// </summary>
        /// <param name="display">The option display to create a style from.</param>
        /// <returns>The new OptionDisplayStyle.</returns>
        public static OptionDisplayStyle CreateOptionDisplayStyle(OptionDisplay display)
        {
            OptionDisplayStyle style = new OptionDisplayStyle();
            ApplyOptionPanelImageSettingsToStyle(display.Images, style);

            List<DialogueButton> buttons = display.GetOptionButtons();

            if(buttons != null && buttons.Count > 0) 
            { 
                OptionButtonStyle buttonStyle = new OptionButtonStyle();
                style.optionButtonStyle = buttonStyle;
                ApplyOptionButtonToStyle(buttons[0], buttonStyle);
            }

            ApplyDirectionalDisplaySettingsToStyle(display, style);

            return style;
        }

        /// <summary>
        /// Applies the settings for link image colors and the main image color to the style provided if it's a directional option display.
        /// </summary>
        /// <param name="display">The option display to apply settings from.</param>
        /// <param name="style">The style to apply settings to (only if the option display is a directional option display).</param>
        private static void ApplyDirectionalDisplaySettingsToStyle(OptionDisplay display, OptionDisplayStyle style)
        {
            if (display is DirectionalOptionDisplay)
            {
                DirectionalOptionDisplay directionalDisplay = display as DirectionalOptionDisplay;

                if (directionalDisplay.MainImage != null)
                {
                    style.directionalStyle.mainImageColor = directionalDisplay.MainImage.color;
                }

                style.directionalStyle.normalColor = directionalDisplay.LinkNormalColor;
                style.directionalStyle.highlightColor = directionalDisplay.LinkHighlightColor;
                style.directionalStyle.pressedColor = directionalDisplay.LinkPressedColor;
                style.directionalStyle.disabledColor = directionalDisplay.LinkDisabledColor;
            }
        }

        /// <summary>
        /// Creates a new continue display style from the provided continue display.
        /// </summary>
        /// <param name="display">The continue display to create a style from.</param>
        /// <returns>The new ContinueDisplayStyle.</returns>
        public static ContinueDisplayStyle CreateContinueDisplayStyle(ContinueDisplay display)
        {
            ContinueDisplayStyle style = new ContinueDisplayStyle();
            if(display.BackgroundImage != null)
            {
                style.imageStyleSettings.enabled = display.BackgroundImage.enabled;
                style.imageStyleSettings.color = display.BackgroundImage.color;
                style.imageStyleSettings.sprite = display.BackgroundImage.sprite;
                style.imageStyleSettings.imageType = display.BackgroundImage.type;
                style.imageStyleSettings.pixelsPerUnit = display.BackgroundImage.pixelsPerUnitMultiplier;
            }

            ApplyTextSettingsToStyle(style.textStyleSettings, display.StandardText);

#if TEXTMESHPRO_INSTALLED
            ApplyTMPTextSettingsToStyle(style.textStyleSettings, display.TMPText);
#endif

            DialogueButton button = display.GetComponent<DialogueButton>();
            if(button != null)
            {
                style.buttonStyleSettings.normalImageColor = button.normalButtonColor;
                style.buttonStyleSettings.pressedImageColor = button.pressedButtonColor;
                style.buttonStyleSettings.disabledImageColor = button.disabledButtonColor;
                style.buttonStyleSettings.highlightedImageColor = button.highlightedButtonColor;

                style.buttonStyleSettings.normalTextColor = button.normalTextColor;
                style.buttonStyleSettings.pressedTextColor = button.pressedTextColor;
                style.buttonStyleSettings.disabledTextColor = button.disabledTextColor;
                style.buttonStyleSettings.highlightedTextColor = button.highlightedTextColor;
            }

            return style;
        }

        /// <summary>
        /// Creates a complete dialogue display style from the provided dialogue display.
        /// </summary>
        /// <param name="display">The dialogue display to create a style from.</param>
        /// <returns>The new DialogueStyle.</returns>
        public static DialogueStyle CreateStyle(DialogueDisplay display)
        {
            DialogueStyle style = ScriptableObject.CreateInstance<DialogueStyle>();
            if (display.GetConversationDisplay() != null)
            {
                ConversationDisplayStyle convoStyle = CreateConversationDisplayStyle(display.GetConversationDisplay());
                style.conversationStyle = convoStyle;
            }

            if (display.GetOptionDisplay() != null)
            {
                OptionDisplayStyle optionStyle = CreateOptionDisplayStyle(display.GetOptionDisplay());
                style.optionStyle = optionStyle;
            }

            if(display.GetContinueDisplay() != null)
            {
                ContinueDisplayStyle continueStyle = CreateContinueDisplayStyle(display.GetContinueDisplay());
                style.continueStyle = continueStyle;
            }

            return style;
        }

        /// <summary>
        /// Applies and creates option panel image styles for the provided OptionDisplayStyle from the List of option display images provided.
        /// </summary>
        /// <param name="optionPanelImages">The list of option panel images to create styles from.</param>
        /// <param name="style">The option display style to update.</param>
        protected static void ApplyOptionPanelImageSettingsToStyle(List<Image> optionPanelImages, OptionDisplayStyle style)
        {
            for(int i = 0; i < optionPanelImages.Count; i++)
            {
                Image image = optionPanelImages[i];
                if(style.optionPanelImageStyles.Count <= i)
                {
                    style.optionPanelImageStyles.Add(new ImageStyleSettings());
                }

                ImageStyleSettings imageStyle = style.optionPanelImageStyles[i];

                if (image != null)
                {
                    imageStyle.enabled = image.enabled;
                    imageStyle.sprite = image.sprite;
                    imageStyle.color = image.color;
                    imageStyle.imageType = image.type;
                    imageStyle.pixelsPerUnit = image.pixelsPerUnitMultiplier;
                }
                else
                {
                    imageStyle.enabled = false;
                }
            }
        }

        /// <summary>
        /// Applies the settings of the specified dialogue button to the provided option button style.
        /// </summary>
        /// <param name="button">The dialogue button to use.</param>
        /// <param name="style">The style to update.</param>
        protected static void ApplyOptionButtonToStyle(DialogueButton button, OptionButtonStyle style)
        {
            style.buttonStyleSettings.normalTextColor = button.normalTextColor;
            style.buttonStyleSettings.highlightedTextColor = button.highlightedTextColor;
            style.buttonStyleSettings.pressedTextColor = button.pressedTextColor;
            style.buttonStyleSettings.disabledTextColor = button.disabledTextColor;

            if (button.StandardText != null)
            {
                style.textStyleSettings.color = button.StandardText.color;
                style.textStyleSettings.StandardFont = button.StandardText.font;
                style.textStyleSettings.fontSize = button.StandardText.fontSize;
            }

#if TEXTMESHPRO_INSTALLED
            if (button.TMPText != null)
            {
                style.textStyleSettings.color = button.TMPText.color;
                style.textStyleSettings.TMPFont = button.TMPText.font;
                style.textStyleSettings.fontSize = button.TMPText.fontSize;
            }
#endif

            ApplyTextSettingsToStyle(style.textStyleSettings, button.StandardText);

#if TEXTMESHPRO_INSTALLED
            ApplyTMPTextSettingsToStyle(style.textStyleSettings, button.TMPText);
#endif

            if (button.backgroundImage != null)
            {
                style.imageStyleSettings.enabled = button.backgroundImage.enabled;
                style.buttonStyleSettings.normalImageColor = button.normalButtonColor;
                style.buttonStyleSettings.highlightedImageColor = button.highlightedButtonColor;
                style.buttonStyleSettings.pressedImageColor = button.pressedButtonColor;
                style.buttonStyleSettings.disabledImageColor = button.disabledButtonColor;

                style.imageStyleSettings.color = button.backgroundImage.color;
                style.imageStyleSettings.pixelsPerUnit = button.backgroundImage.pixelsPerUnitMultiplier;
                style.imageStyleSettings.imageType = button.backgroundImage.type;
                style.imageStyleSettings.sprite = button.backgroundImage.sprite;
            }
            else
            {
                style.imageStyleSettings.enabled = false;
            }

            if (button.hoverSound != null)
            {
                style.hoverSound = button.hoverSound;
            }
        }

        /// <summary>
        /// Apply the settings of the provided Unity Text component to the TextStyleSettings specified.
        /// </summary>
        /// <param name="styleSettings">The text style settings to update.</param>
        /// <param name="text">The Text component to retrieve settings from.</param>
        protected static void ApplyTextSettingsToStyle(TextStyleSettings styleSettings, Text text)
        {
            if (text != null)
            {
                styleSettings.color = text.color;
                styleSettings.StandardFont = text.font;
                styleSettings.fontSize = text.fontSize;
                styleSettings.autoSizeFont = text.resizeTextForBestFit;
                styleSettings.minFontSize = text.resizeTextMinSize;
                styleSettings.maxFontSize = text.resizeTextMaxSize;
            }
        }

#if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// Apply the settings of the provided TextMeshPro Text component to the TextStyleSettings specified.
        /// </summary>
        /// <param name="styleSettings">The text style settings to update.</param>
        /// <param name="text">The TextMeshPro component to retrieve settings from.</param>
        protected static void ApplyTMPTextSettingsToStyle(TextStyleSettings styleSettings, TMP_Text text)
        {
            styleSettings.TMPFont = text.font;

            DialogueDisplay dialogueDisplay = text.GetComponentInParent<DialogueDisplay>();

            //Apply TextMeshPro settings if TextMeshPro is present.
            if (dialogueDisplay != null && text != null)
            {
                styleSettings.color = text.color;
                styleSettings.fontSize = text.fontSize;
                styleSettings.autoSizeFont = text.enableAutoSizing;
                styleSettings.minFontSize = text.fontSizeMin;
                styleSettings.maxFontSize = text.fontSizeMax;
            }
        }
#endif

        /// <summary>
        /// Applies the color theme specified to the provided dialogue display.
        /// </summary>
        /// <param name="colorTheme">The color theme to apply.</param>
        /// <param name="display">The dialogue display to update.</param>
        public static void ApplyColorTheme(ColorTheme colorTheme, DialogueDisplay display)
        {
            if (display != null && colorTheme != null)
            {
                ConversationDisplay convoDisplay = display.GetConversationDisplay();
                if (convoDisplay != null)
                {
                    for(int i = 0; i < convoDisplay.ConversationPanelImages.Count; i++)
                    {
                        Image convoImage = convoDisplay.ConversationPanelImages[i];

                        if(convoImage != null && colorTheme.convoImageColors.Count > i)
                        {
                            convoImage.color = colorTheme.convoImageColors[i];
                        }
                    }

                    if (convoDisplay.CharacterNameBackgroundImage != null)
                    {
                        convoDisplay.CharacterNameBackgroundImage.color = colorTheme.characterPanelColor;
                    }

                    if (convoDisplay.StandardConvoText != null)
                    {
                        convoDisplay.StandardConvoText.color = colorTheme.convoTextColor;
                    }

#if TEXTMESHPRO_INSTALLED
                    if (convoDisplay.TMPCharacterNameText != null)
                    {
                        convoDisplay.TMPCharacterNameText.color = colorTheme.convoTextColor;
                    }
#endif

                    if (convoDisplay.StandardCharacterNameText != null)
                    {
                        convoDisplay.StandardCharacterNameText.color = colorTheme.characterNameTextColor;
                    }

#if TEXTMESHPRO_INSTALLED
                    if (convoDisplay.TMPCharacterNameText != null)
                    {
                        convoDisplay.TMPCharacterNameText.color = colorTheme.characterNameTextColor;
                    }
#endif
                }

                OptionDisplay optionDisplay = display.GetOptionDisplay();
                if (optionDisplay != null)
                {
                    if (optionDisplay.Images != null)
                    {
                        for(int i = 0; i < optionDisplay.Images.Count; i++)
                        {
                            Image optionImage = optionDisplay.Images[i];

                            if(optionImage != null && colorTheme.optionImageColors.Count > i)
                            {
                                optionImage.color = colorTheme.optionImageColors[i];
                            }
                        }
                    }

                    foreach (DialogueButton button in optionDisplay.GetOptionButtons())
                    {
                        ApplyColorThemeToButton(colorTheme, button);
                    }
                }

                ContinueDisplay continueDisplay = display.GetContinueDisplay();
                if (continueDisplay != null)
                {
                    DialogueButton continueButton = continueDisplay.GetComponent<DialogueButton>();
                    ApplyColorThemeToContinueButton(colorTheme, continueButton);
                }
            }
        }

        /// <summary>
        /// Applies the color theme specified to the provided dialogue button.
        /// </summary>
        /// <param name="colorTheme">The color theme to apply.</param>
        /// <param name="button">The dialogue button to update.</param>
        public static void ApplyColorThemeToButton(ColorTheme colorTheme, DialogueButton button)
        {
            if (button != null && colorTheme != null)
            {
                button.backgroundImage.color = colorTheme.buttonNormalColor;
                button.normalButtonColor = colorTheme.buttonNormalColor;
                button.pressedButtonColor = colorTheme.buttonPressedColor;
                button.highlightedButtonColor = colorTheme.buttonHighlightColor;
                button.disabledButtonColor = colorTheme.buttonDisabledColor;

                if (button.StandardText != null)
                {
                    button.StandardText.color = colorTheme.buttonTextNormalColor;
                }

#if TEXTMESHPRO_INSTALLED
                if (button.TMPText != null)
                {
                    button.TMPText.color = colorTheme.buttonTextNormalColor;
                }
#endif

                button.normalTextColor = colorTheme.buttonTextNormalColor;
                button.pressedTextColor = colorTheme.buttonTextPressedColor;
                button.highlightedTextColor = colorTheme.buttonTextHighlightColor;
                button.disabledTextColor = colorTheme.buttonTextDisabledColor;
            }
        }

        /// <summary>
        /// Applies the specified color theme to the provided dialogue button.
        /// </summary>
        /// <param name="colorTheme">The color theme to apply.</param>
        /// <param name="button">The dialogue button to update.</param>
        public static void ApplyColorThemeToContinueButton(ColorTheme colorTheme, DialogueButton button)
        {
            if (button != null && colorTheme != null)
            {
                button.backgroundImage.color = colorTheme.continueButtonNormalColor;
                button.normalButtonColor = colorTheme.continueButtonNormalColor;
                button.pressedButtonColor = colorTheme.continueButtonPressedColor;
                button.highlightedButtonColor = colorTheme.continueButtonHighlightColor;
                button.disabledButtonColor = colorTheme.continueButtonDisabledColor;

                if (button.StandardText != null)
                {
                    button.StandardText.color = colorTheme.continueTextNormalColor;
                }

#if TEXTMESHPRO_INSTALLED
                if (button.TMPText != null)
                {
                    button.TMPText.color = colorTheme.continueTextNormalColor;
                }
#endif

                button.normalTextColor = colorTheme.continueTextNormalColor;
                button.pressedTextColor = colorTheme.continueTextPressedColor;
                button.highlightedTextColor = colorTheme.continueTextHighlightColor;
                button.disabledTextColor = colorTheme.continueTextDisabledColor;
            }
        }

        /// <summary>
        /// Applies the specified frame theme to the provided dialogue display.
        /// </summary>
        /// <param name="frameTheme">The frame theme to apply.</param>
        /// <param name="display">The dialogue display to update.</param>
        public static void ApplyFrameTheme(FrameTheme frameTheme, DialogueDisplay display)
        {
            if (display != null && frameTheme != null)
            {
                ConversationDisplay convoDisplay = display.GetConversationDisplay();
                if (convoDisplay != null)
                {
                    for (int i = 0; i < convoDisplay.ConversationPanelImages.Count; i++)
                    {
                        Image convoImage = convoDisplay.ConversationPanelImages[i];

                        if (convoImage != null && frameTheme.convoImageSettings.Count > i)
                        {
                            ImageStyleSettings imageSetting = frameTheme.convoImageSettings[i];
                            if(imageSetting != null)
                            {
                                convoImage.sprite = imageSetting.sprite;
                                convoImage.type = imageSetting.imageType;
                                convoImage.pixelsPerUnitMultiplier = imageSetting.pixelsPerUnit;
                            }
                        }
                    }

                    if (convoDisplay.CharacterNameBackgroundImage != null)
                    {
                        ImageStyleSettings imageSetting = frameTheme.characterPanelImageSettings;
                        convoDisplay.CharacterNameBackgroundImage.sprite = imageSetting.sprite;
                        convoDisplay.CharacterNameBackgroundImage.type = imageSetting.imageType;
                        convoDisplay.CharacterNameBackgroundImage.pixelsPerUnitMultiplier = imageSetting.pixelsPerUnit;
                    }
                }

                OptionDisplay optionDisplay = display.GetOptionDisplay();
                if (optionDisplay != null)
                {
                    if (optionDisplay.Images != null)
                    {
                        for(int i = 0; i < optionDisplay.Images.Count; i++)
                        {
                            Image optionImage = optionDisplay.Images[i];

                            if(optionImage != null && frameTheme.optionImageSettings.Count > i)
                            {
                                ImageStyleSettings imageSetting = frameTheme.optionImageSettings[i];
                                optionImage.sprite = imageSetting.sprite;
                                optionImage.type = imageSetting.imageType;
                                optionImage.pixelsPerUnitMultiplier = imageSetting.pixelsPerUnit;
                            }
                        }
                    }

                    foreach (DialogueButton button in optionDisplay.GetOptionButtons())
                    {
                        if (button != null && button.backgroundImage != null)
                        {
                            ImageStyleSettings imageSetting = frameTheme.buttonImageSettings;
                            button.backgroundImage.sprite = imageSetting.sprite;
                            button.backgroundImage.type = imageSetting.imageType;
                            button.backgroundImage.pixelsPerUnitMultiplier = imageSetting.pixelsPerUnit;
                        }
                    }
                }

                ContinueDisplay continueDisplay = display.GetContinueDisplay();
                if (continueDisplay != null)
                {
                    if (continueDisplay.BackgroundImage != null)
                    {
                        ImageStyleSettings imageSetting = frameTheme.continueImageSettings;
                        continueDisplay.BackgroundImage.sprite = imageSetting.sprite;
                        continueDisplay.BackgroundImage.type = imageSetting.imageType;
                        continueDisplay.BackgroundImage.pixelsPerUnitMultiplier = imageSetting.pixelsPerUnit;
                    }
                }
            }
        }
    }
}