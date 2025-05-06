using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Localization
{
    /// <summary>
    /// A set of localizable languages.
    /// </summary>
    [CreateAssetMenu(fileName = "Localizable Language Set", menuName = "EasyTalk/Localization/Localizable Language Set", order = 4)]
    [Serializable]
    public class LocalizableLanguageSet : ScriptableObject
    {
        /// <summary>
        /// The list of localizable languages.
        /// </summary>
        [SerializeField]
        private List<LocalizableLanguage> languages = new List<LocalizableLanguage>();

        /// <summary>
        /// Gets or sets the list of localizable languages.
        /// </summary>
        public List<LocalizableLanguage> Languages
        {
            get { return languages; }
            set { languages = value; }
        }

        /// <summary>
        /// Finds and returns the LocalizableLanguage matching the specified language name or ISO-639 code, if available in the language set.
        /// </summary>
        /// <param name="languageCodeOrName">The English, native, or alternative name for the language to retrieve, or the ISO-639 code.</param>
        /// <returns>The LocalizableLanguage for the language specified, or null if none exists in the set.</returns>
        public LocalizableLanguage GetLanguage(string languageCodeOrName)
        {
            foreach (LocalizableLanguage language in languages)
            {
                if (language.LanguageCode.ToLower().Equals(languageCodeOrName.ToLower()))
                {
                    return language;
                }

                if (language.EnglishName.ToLower().Equals(languageCodeOrName.ToLower()))
                {
                    return language;
                }

                if (language.NativeName.Equals(languageCodeOrName))
                {
                    return language;
                }

                if (language.AltName.ToLower().Equals(languageCodeOrName.ToLower()))
                {
                    return language;
                }
            }

            return null;
        }
    }
}