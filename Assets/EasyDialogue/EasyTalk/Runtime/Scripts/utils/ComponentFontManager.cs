using EasyTalk.Controller;
using EasyTalk.Display;
using EasyTalk.Localization;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EasyTalk.Display.AbstractDialogueDisplay;

namespace EasyTalk.Utils
{
    /// <summary>
    /// This class is used to manage font updates on Text and TextMeshPro components, such as the font size to use and the font to use for a particular language.
    /// </summary>
    public class ComponentFontManager
    {
        /// <summary>
        /// A mapping of component instance IDs to the fonts which they originally used. This is used to revert components when the language is changed.
        /// </summary>
        private static Dictionary<int, TextFontSettings> originalFonts = new Dictionary<int, TextFontSettings>();

#if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// A mapping of component instance IDs to the TextMeshPro font assets which they originally used. This is used to revert components when the language is changed.
        /// </summary>
        private static Dictionary<int, TMPTextFontSettings> originalTMPFonts = new Dictionary<int, TMPTextFontSettings>();
#endif

#if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// Updates the font of each TextMeshPro text component in the specified component and its children to the specified font settings, or the component's original font if no font is available.
        /// </summary>
        /// <param name="component">The component to update fonts on.</param>
        /// <param name="fontOverride">The font override settings to use.</param>
        public static void UpdateTextMeshProFonts(Component component, LanguageFontOverride fontOverride)
        {
            //Update the font of all text components in the component
            TMP_Text[] textComponents = component.GetComponentsInChildren<TMP_Text>(true);
            TMP_FontAsset font = null;
            float minSize = 0f;
            float maxSize = 32.0f;

            if (fontOverride != null)
            {
                font = fontOverride.tmpFont;
                //minSize = fontOverride.minFontSize;
                maxSize = fontOverride.maxFontSize;
            }

            Dictionary<int, LocalizableComponent> textParents = GetLocalizableTextMeshProComponentMap(component);

            foreach (TMP_Text text in textComponents)
            {
                if (!originalTMPFonts.ContainsKey(text.GetInstanceID()))
                {
                    originalTMPFonts.Add(text.GetInstanceID(), new TMPTextFontSettings(text.font, text.fontSizeMin, text.fontSizeMax));
                }

                //If a font override was provided, set the font on the text component
                if (font != null)
                {
                    text.font = font;
                }
                else //If no font override was provided, attempt to set the font to whatever the original font was
                {
                    if (originalTMPFonts.ContainsKey(text.GetInstanceID()))
                    {
                        TMPTextFontSettings textFontSettings = originalTMPFonts[text.GetInstanceID()];
                        TMP_FontAsset tmpFont = textFontSettings.font;
                        minSize = textFontSettings.minFontSize;
                        maxSize = textFontSettings.maxFontSize;
                        text.font = tmpFont;
                    }
                }

                text.fontSizeMin = minSize;
                text.fontSizeMax = maxSize;
                text.fontSize = Mathf.Clamp(text.fontSize, minSize, maxSize);

                //If there are overriden font settings on the parent (a LocalizableComponent) of the text component, apply those settings instead.
                HandleTextMeshProLocalizableFontCustomizations(text, textParents, fontOverride);
            }
        }

        /// <summary>
        /// Applies settings to the specified TextMeshPro text component based on the current language and overridden font settings, if the text component is mapped to  
        /// a LocalizableComponent which overrides settings.
        /// </summary>
        /// <param name="text">The TextMeshPro text component to modify.</param>
        /// <param name="componentMapping">A mapping of TextMeshPro text component instance IDs to LocalizableComponents. If no mapping exists for the provided TextMeshPro 
        /// text component, this method will return without making modifications.</param>
        /// <param name="baseSettingsFontOverride">The LanguageFontOverride set on EasyTalk Dialogue Settings.</param>
        private static void HandleTextMeshProLocalizableFontCustomizations(TMP_Text text, Dictionary<int, LocalizableComponent> componentMapping, LanguageFontOverride baseSettingsFontOverride)
        {
            int textInstanceId = text.GetInstanceID();
            if (componentMapping.ContainsKey(textInstanceId))
            {
                LocalizableComponent localizableComponent = componentMapping[textInstanceId];

                //Apply LocalizableComponent LanguageFontOverrides if they are set.
                if (localizableComponent.LanguageFontOverrides != null)
                {
                    LanguageFontOverride lfo;

                    if (baseSettingsFontOverride != null)
                    {
                        lfo = localizableComponent.LanguageFontOverrides.GetOverrideForLanguage(baseSettingsFontOverride.languageCode);
                    }
                    else
                    {
                        lfo = localizableComponent.LanguageFontOverrides.GetOverrideForLanguage(EasyTalkGameState.Instance.Language);
                    }

                    if (lfo != null)
                    {
                        text.font = lfo.tmpFont;
                        text.fontSizeMin = lfo.minFontSize;
                        text.fontSizeMax = lfo.maxFontSize;
                    }
                }

                //Apply font size overrides from LocalizableComponent if set to do so.
                if (localizableComponent.OverrideFontSizes)
                {
                    text.fontSizeMin = localizableComponent.MinFontSize;
                    text.fontSizeMax = localizableComponent.MaxFontSize;
                }
            }
        }
#endif

        /// <summary>
        /// Updates the font of each text component in the specified component and its children using the specified font settings, or the component's original font if no font is available.
        /// </summary>
        /// <param name="component">The component to update fonts on.</param>
        /// <param name="fontOverride">The font override settings to use.</param>
        public static void UpdateFonts(Component component, LanguageFontOverride fontOverride)
        {
            //Update the font of all text components in the component
            Text[] textComponents = component.GetComponentsInChildren<Text>(true);
            Font font = null;
            float minSize = 0f;
            float maxSize = 32.0f;

            if (fontOverride != null)
            {
                font = fontOverride.font;
                //minSize = fontOverride.minFontSize;
                maxSize = fontOverride.maxFontSize;
            }

            Dictionary<int, LocalizableComponent> textParents = GetLocalizableTextComponentMap(component);

            foreach (Text text in textComponents)
            {
                if (!originalFonts.ContainsKey(text.GetInstanceID()))
                {
                    originalFonts.Add(text.GetInstanceID(), new TextFontSettings(text.font, text.resizeTextMinSize, text.resizeTextMaxSize));
                }

                //If a font override was provided, set the font on the text component
                if (font != null)
                {
                    text.font = font;
                }
                else //If no font override was provided, attempt to set the font to whatever the original font was
                {
                    if (originalFonts.ContainsKey(text.GetInstanceID()))
                    {
                        TextFontSettings textFontSettings = originalFonts[text.GetInstanceID()];
                        minSize = textFontSettings.minFontSize;
                        maxSize = textFontSettings.maxFontSize;
                        text.font = textFontSettings.font;
                    }
                }

                text.resizeTextMinSize = (int)minSize;
                text.resizeTextMaxSize = (int)maxSize;
                text.fontSize = Mathf.Clamp(text.fontSize, text.resizeTextMinSize, text.resizeTextMaxSize);

                //If there are overriden font settings on the parent (a LocalizableComponent) of the text component, apply those settings instead.
                HandleTextLocalizableFontCustomizations(text, textParents, fontOverride);
            }
        }

        /// <summary>
        /// Applies settings to the specified Text component based on the current language and overridden font settings, if the text component is mapped to  
        /// a LocalizableComponent which overrides settings.
        /// </summary>
        /// <param name="text">The Text component to modify.</param>
        /// <param name="componentMapping">A mapping of Text component instance IDs to LocalizableComponents. If no mapping exists for the provided  
        /// Text component, this method will return without making modifications.</param>
        /// <param name="baseSettingsFontOverride">The LanguageFontOverride set on EasyTalk Dialogue Settings.</param>
        private static void HandleTextLocalizableFontCustomizations(Text text, Dictionary<int, LocalizableComponent> componentMapping, LanguageFontOverride baseSettingsFontOverride)
        {
            int textInstanceId = text.GetInstanceID();
            if (componentMapping.ContainsKey(textInstanceId))
            {
                LocalizableComponent localizableComponent = componentMapping[textInstanceId];

                //Apply LocalizableComponent LanguageFontOverrides if they are set.
                if (localizableComponent.LanguageFontOverrides != null)
                {
                    LanguageFontOverride lfo;

                    if (baseSettingsFontOverride != null)
                    {
                        lfo = localizableComponent.LanguageFontOverrides.GetOverrideForLanguage(baseSettingsFontOverride.languageCode);
                    }
                    else
                    {
                        lfo = localizableComponent.LanguageFontOverrides.GetOverrideForLanguage(EasyTalkGameState.Instance.Language);
                    }

                    if (lfo != null)
                    {
                        text.font = lfo.font;
                        text.resizeTextMinSize = (int)lfo.minFontSize;
                        text.resizeTextMaxSize = (int)lfo.maxFontSize;
                    }
                }

                //Apply font size overrides from LocalizableComponent if set to do so.
                if (localizableComponent.OverrideFontSizes)
                {
                    text.resizeTextMinSize = localizableComponent.MinFontSize;
                    text.resizeTextMaxSize = localizableComponent.MaxFontSize;
                }
            }
        }

#if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// Returns a mapping of Text component instance IDs to LocalizableComponent parents of those components, 
        /// where the LocalizableComponents are children of the component passed in to this method.
        /// </summary>
        /// <param name="component">The parent component to retrieve mappings for.</param>
        /// <returns>A mapping of Text component instance IDs to the LocalizableComponent which controls or parents them.</returns>
        private static Dictionary<int, LocalizableComponent> GetLocalizableTextMeshProComponentMap(Component component)
        {
            Dictionary<int, LocalizableComponent> textParents = new Dictionary<int, LocalizableComponent>();

            DialoguePanel[] dialoguePanels = component.GetComponentsInChildren<DialoguePanel>(true);
            foreach (DialoguePanel dialoguePanel in dialoguePanels)
            {
                TMP_Text[] panelTextComponents = dialoguePanel.GetComponentsInChildren<TMP_Text>();
                foreach (TMP_Text textComponent in panelTextComponents)
                {
                    int componentID = textComponent.GetInstanceID();
                    if (!textParents.ContainsKey(componentID))
                    {
                        textParents.Add(componentID, dialoguePanel);
                    }
                }
            }

            return textParents;
        }
#endif

        /// <summary>
        /// Returns a mapping of Text component instance IDs to LocalizableComponent parents of those components, 
        /// where the LocalizableComponents are children of the component passed in to this method.
        /// </summary>
        /// <param name="component">The parent component to retrieve mappings for.</param>
        /// <returns>A mapping of Text component instance IDs to the LocalizableComponent which controls or parents them.</returns>
        private static Dictionary<int, LocalizableComponent> GetLocalizableTextComponentMap(Component component)
        {
            Dictionary<int, LocalizableComponent> textParents = new Dictionary<int, LocalizableComponent>();

            DialoguePanel[] dialoguePanels = component.GetComponentsInChildren<DialoguePanel>(true);
            foreach (DialoguePanel dialoguePanel in dialoguePanels)
            {
                Text[] panelTextComponents = dialoguePanel.GetComponentsInChildren<Text>();
                foreach (Text textComponent in panelTextComponents)
                {
                    int componentID = textComponent.GetInstanceID();
                    if (!textParents.ContainsKey(componentID))
                    {
                        textParents.Add(componentID, dialoguePanel);
                    }
                }
            }

            return textParents;
        }
    }
}
