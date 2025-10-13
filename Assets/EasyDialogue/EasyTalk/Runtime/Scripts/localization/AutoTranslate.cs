using EasyTalk.Controller;
using EasyTalk.Display;
using EasyTalk.Settings;

#if TEXTMESHPRO_INSTALLED
using TMPro;
#endif

using UnityEngine;
using UnityEngine.UI;

namespace EasyTalk.Localization
{
    /// <summary>
    /// AutoTranslate is a component which can be placed on a GameObject with a text component. When used with an appropriate TranslationLibrary, it
    /// enables the text to be translated automatically whenever the language is changes on the EasyTalkGameState.
    /// </summary>
    public class AutoTranslate : MonoBehaviour
    {
    #if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// The text component to translate.
        /// </summary>
        [Tooltip("The text component to apply automatic translation to. Automatic translations are performed whenever the EasyTalkGameState language is " +
            "changed, or SetText() is called on this component.")]
        [SerializeField]
        private TMP_Text textMeshProText;
    #endif

        /// <summary>
        /// The text component to translate.
        /// </summary>
        [Tooltip("The text component to apply automatic translation to. Automatic translations are performed whenever the EasyTalkGameState language is " +
            "changed, or SetText() is called on this component.")]
        [SerializeField]
        private Text text;

        /// <summary>
        /// The translation library to use for translations.
        /// </summary>
        [Tooltip("The translation library to use when translating the text component's text.")]
        [SerializeField]
        private TranslationLibrary library;

        /// <summary>
        /// The set of fonts to use for various languages.
        /// </summary>
        [Tooltip("(Optional) A set of font overrides to use when the EasyTalkGameState language is changed to various languages. If left null, the AutoTranslate will attempt to pull " +
            "the language font overrides from an existing instance of EasyTalkDialogueSettings, which should exist if there is an active Dialogue Display.")]
        [SerializeField]
        private LanguageFontOverrides languageFontOverrides;

        /// <summary>
        /// The original text value.
        /// </summary>
        private string originalText;

        /// <summary>
        /// INitializes the AutoTranslate component.
        /// </summary>
        private void Awake()
        {
            if (text == null)
            {
                #if TEXTMESHPRO_INSTALLED
                    textMeshProText = this.GetComponent<TMP_Text>();
                #else
                    text = this.GetComponent<Text>();
                #endif
            }

            originalText = Text.text;

            EasyTalkGameState.Instance.onLanguageChanged += LanguageChanged;
        }

        /// <summary>
        /// Unregister the LanguageChanged method frm the onLanguageChanged delegate so that the EasyTalkGameState instance will no longer try to call this object.
        /// </summary>
        private void OnDestroy()
        {
            EasyTalkGameState.Instance.onLanguageChanged -= LanguageChanged;
        }

        /// <summary>
        /// Called whenever the language is changed on the EasyTalkGameState. This method will attempt to update the text to the translated version and update the 
        /// font of the text element if necessary.
        /// </summary>
        /// <param name="oldLanguage">The prior language being used.</param>
        /// <param name="newLanguage">The new language to use.</param>
        protected void LanguageChanged(string oldLanguage, string newLanguage)
        {
            UpdateText();
            UpdateFont();
        }

        /// <summary>
        /// Updates the font of the text element to a font compatible with the current language (based on the language font overrides of the EasyTalkGameState).
        /// </summary>
        private void UpdateFont()
        {
            DialogueDisplay parentDisplay = DialogueDisplay.GetParentDialogueDisplay(this.gameObject);

            if (parentDisplay != null && parentDisplay.DialogueSettings != null && parentDisplay.DialogueSettings.LanguageFontOverrides != null)
            {
                LanguageFontOverride fontOverride = null;

                if (languageFontOverrides != null)
                {
                    fontOverride = languageFontOverrides.GetOverrideForLanguage(EasyTalkGameState.Instance.Language);
                }
                else
                {
                    EasyTalkDialogueSettings dialogueSettings = parentDisplay.DialogueSettings;
                    if (dialogueSettings != null)
                    {
                        LanguageFontOverrides fontOverrides = dialogueSettings.LanguageFontOverrides;

                        if (fontOverrides != null) 
                        {
                            languageFontOverrides = fontOverrides;
                            fontOverride = languageFontOverrides.GetOverrideForLanguage(EasyTalkGameState.Instance.Language);
                        }
                    }
                }

                if (fontOverride != null)
                {
                    #if TEXTMESHPRO_INSTALLED
                        textMeshProText.font = fontOverride.tmpFont;
                    #else
                        text.font = fontOverride.font;
                    #endif
                }
            }
        }

        /// <summary>
        /// Updates the text element to a translated version of the original text if a translation can be found for the current EasyTalkGameState language.
        /// </summary>
        private void UpdateText()
        {
            Translation translation = library.GetTranslation(originalText, EasyTalkGameState.Instance.Language);
            if (translation != null)
            {
                Text.text = translation.text;
            }
        }

        /// <summary>
        /// Sets the text value of the text component. The text passed to this method should be in the default/original language and will be translated automatically
        /// by this method if possible.
        /// </summary>
        /// <param name="text">The new text value.</param>
        public void SetText(string text)
        {
            Text.text = text;
            originalText = text;
            UpdateText();
        }

    #if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// Gets the text component used by the AutoTranslate.
        /// </summary>
        public TMP_Text Text
        {
            get { return textMeshProText; }
        }
    #else
        public Text Text
        {
            get { return text; }
        }
    #endif
    }
}