using EasyTalk.Editor.Nodes;
using EasyTalk.Localization;
using EasyTalk.Settings;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static EasyTalk.Editor.Utils.ETFileManager;

namespace EasyTalk.Editor.Settings
{
    [CreateAssetMenu(fileName = "EasyTalk Editor Settings", menuName = "EasyTalk/Settings/EasyTalk Editor Settings", order = 7)]
    public class EasyTalkEditorSettings : ScriptableObject
    {
        /// <summary>
        /// The amount of time to wait (in milliseconds) when loading the node editor from a stored editor session.
        /// </summary>
        [Tooltip("The amount of time to wait (in milliseconds) when loading the node editor from a stored editor session.")]
        [SerializeField]
        public int uiReloadSessionDelay = 1000;

        /// <summary>
        /// The amount of time to wait (in milliseconds) when loading a Dialogue asset into the node editor from a file.
        /// </summary>
        [Tooltip("The amount of time to wait (in milliseconds) when loading a Dialogue asset into the node editor from a file.")]
        [SerializeField]
        public int dialogueLoadDelay = 100;

        /// <summary>
        /// The default volumne to use in the node editor when playing back audio for lines of dialogue.
        /// </summary>
        [Tooltip("The default volumne to use in the node editor when playing back audio for lines of dialogue.")]
        [SerializeField]
        public float defaultVolume = 50.0f;

        /// <summary>
        /// The autosave mode to use.
        /// </summary>
        [Tooltip("The autosave mode to use.")]
        [SerializeField]
        public AutosaveMode autosaveMode = AutosaveMode.NONE;

        /// <summary>
        /// The amount of time to wait between autosaving changes if in TIMED mode. (setting this too low will make Unity slow).
        /// </summary>
        [Tooltip("The amount of time to wait between autosaving changes if in TIMED mode. (setting this too low will make Unity slow).")]
        [SerializeField]
        public int autosaveDelayMs = 30000;

        /// <summary>
        /// The stylesheets to use for the node editor.
        /// </summary>
        [Tooltip("The stylesheets to use for the node editor.")]
        [SerializeField]
        public List<StyleSheet> stylesheets = new List<StyleSheet>();

        /// <summary>
        /// The source language being used to write dialogue in the node editor (this should be an ISO-639 language code).
        /// </summary>
        [Tooltip("The source language being used to write dialogue in the node editor (this should be an ISO-639 language code).")]
        [SerializeField]
        public string defaultOriginalLanguage = "en";

        /// <summary>
        /// The default font to use in the node editor when a language font override can't be found for the current language being used.
        /// </summary>
        [Tooltip("The default font to use in the node editor when a language font override can't be found for the current language being used.")]
        [SerializeField]
        public Font defaultFont;

        /// <summary>
        /// The font to use in the node editor (when a language font override exists for the language currently in use).
        /// </summary>
        [Tooltip("The font to use in the node editor (when a language font override exists for the language currently in use).")]
        [SerializeField]
        public Font localizedFont;

        /// <summary>
        /// The collection of information about languages which are localizable in EasyTalk.
        /// </summary>
        [Tooltip("The collection of information about languages which are localizable in EasyTalk.")]
        [SerializeField]
        public LocalizableLanguageSet localizableLanguages;

        /// <summary>
        /// The set of language font overrides which defines which fonts to use for various languages which are not supported by the default font.
        /// </summary>
        [Tooltip("The set of language font overrides which defines which fonts to use for various languages which are not supported by the default font.")]
        [SerializeField]
        public LanguageFontOverrides languageFontOverrides;

        /// <summary>
        /// The list of languages which the current project is supposed to support.
        /// </summary>
        [Tooltip("The list of languages which the current project is supposed to support.")]
        [SerializeField]
        public List<string> defaultLocalizationLanguages = new List<string>();

        /// <summary>
        /// The project ID of the Google Cloud project to use when using the Google Cloud Translation API.
        /// </summary>
        [Tooltip("The project ID of the Google Cloud project to use when using the Google Cloud Translation API.")]
        [SerializeField]
        public string googleTranslateProjectId = null;

        /// <summary>
        /// When this is true, EasyTalk will automatically search for TMP installation in Assets/TextMesh Pro/ and add or remove the scripting 
        /// define symbol for TEXTMESHPRO_INSTALLED based on whether it is detected at that location or not.
        /// </summary>
        [Tooltip("If set to true, EasyTalk will periodically automatically search for a TextMeshPro installation in 'Assets/TextMesh Pro/ and add or remove a scripting define" +
            "symbol for TEXTMESHPRO_INSTALLED based on whether it was found. If TextMeshPro is installed in Packages instead of Assets, this should be set to false and the " +
            "TEXTMESHPRO_INSTALLED scripting define symbol should be added to the Project Settings manually if you want to use TextMeshPro with EasyTalk.")]
        [SerializeField]
        public bool autoDetectTMP = true;

        [SerializeField]
        public DialogueRegistry dialogueRegistry;

        /// <summary>
        /// If there is a font override for the current language being used in the editor, the localized font is returned, otherwise this method returns the default font.
        /// </summary>
        /// <returns>The font to use for the node editor UI.</returns>
        public Font GetCurrentFont()
        {
            if(localizedFont == null)
            {
                return defaultFont;
            }
            else
            {
                return localizedFont;
            }
        }
    }
}
