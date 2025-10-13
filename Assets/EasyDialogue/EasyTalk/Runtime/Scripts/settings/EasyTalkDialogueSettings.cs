using EasyTalk.Localization;
using UnityEngine;

namespace EasyTalk.Settings
{
    /// <summary>
    /// EasyTalk settings used during runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "EasyTalk Dialogue Settings", menuName = "EasyTalk/Settings/EasyTalk Dialogue Settings", order = 7)]
    public class EasyTalkDialogueSettings : ScriptableObject
    {
        /// <summary>
        /// The set of localizable languages to support.
        /// </summary>
        [Tooltip("The collection of supported localizable languages.")]
        [SerializeField]
        protected LocalizableLanguageSet localizableLanguages;

        /// <summary>
        /// The set of font overrides for localizable languages.
        /// </summary>
        [Tooltip("Overrides for fonts. These are used to switch fonts whenever the localized language is set to a language not supported by the current font.")]
        [SerializeField]
        protected LanguageFontOverrides languageFontOverrides;

        /// <summary>
        /// The set of language inputs used for text character inputs for each language when using a text input display.
        /// </summary>
        [Tooltip("Defines the character sets which are made available per-language when entering text on a text input display using a gamepad.")]
        [SerializeField]
        protected LanguageInputs languageInputs;

        /*[SerializeField]
        protected AISettings aiSettings;*/

        /// <summary>
        /// The translation evaluation mode to use.
        /// </summary>
        [Tooltip("Determines when the translation step is applied to conversation lines and options." +
                "\nTRANSLATE_BEFORE_VARIABLE_EVALUATION: Translates text before resolving variable values within the text. " +
                "Given a variable called 'numApples' used in the text, 'I have (@numApples)' this exact text would be looked for as a match in" +
                "the dialogue's localization table." +
                "\nTRANSLATE_AFTER_VARIABLE_EVALUATION: Translates text after resolving variable values within the text. " +
                "Given a variable called 'numApples' used in the text, where numApples current value is 3, 'I have (@numApples)' would be replaced by " +
                "'I have 3 apples' when looking for a match in the dialogue's localization table.")]
        [SerializeField]
        protected TranslationEvaluationMode translationEvalMode;

        /// <summary>
        /// The Dialogue Registry to use.
        /// </summary>
        [SerializeField]
        protected DialogueRegistry dialogueRegistry;

        /// <summary>
        /// Gets the set of supported localizable languages.
        /// </summary>
        public LocalizableLanguageSet LocalizableLanguageSet { get { return localizableLanguages; } }

        /// <summary>
        /// Gets the set of font overrides for localizable languages.
        /// </summary>
        public LanguageFontOverrides LanguageFontOverrides { get { return languageFontOverrides; } }

        /// <summary>
        /// Gets the set of language input characters defining which text characters are available for text input displays on a per-language basis.
        /// </summary>
        public LanguageInputs LanguageInputs { get { return languageInputs; } }

        /// <summary>
        /// Gets the translation evaluation mode for determining when to apply translations to dialogue text and options during dialogue playback.
        /// </summary>
        public TranslationEvaluationMode TranslationEvaluationMode { get { return translationEvalMode; } }

        /// <summary>
        /// Gets the active Dialogue Registry.
        /// </summary>
        public DialogueRegistry DialogueRegistry { get { return dialogueRegistry; } }

        //public AISettings AISettings { get { return aiSettings; } }
    }
}