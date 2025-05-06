using System;
using UnityEngine;

namespace EasyTalk.Localization
{
    /// <summary>
    /// The LocalizableLanguage class is used to represent and store information pertaining to a specific localizable language.
    /// </summary>
    [Serializable]
    public class LocalizableLanguage
    {
        /// <summary>
        /// The English name for the language.
        /// </summary>
        [SerializeField]
        private string englishName;

        /// <summary>
        /// The native name for the language.
        /// </summary>
        [SerializeField]
        private string nativeName;

        /// <summary>
        /// An alternative name for the language.
        /// </summary>
        [SerializeField]
        private string altName;

        /// <summary>
        /// The ISO-639 language code for the language.
        /// </summary>
        [SerializeField]
        private string languageCode;

        /// <summary>
        /// Gets or sets the English name.
        /// </summary>
        public string EnglishName
        {
            get { return englishName; }
            set { englishName = value; }
        }

        /// <summary>
        /// Gets or sets the native name.
        /// </summary>
        public string NativeName
        {
            get { return nativeName; }
            set { nativeName = value; }
        }

        /// <summary>
        /// Gets or sets the alternative name.
        /// </summary>
        public string AltName
        {
            get { return altName; }
            set { altName = value; }
        }

        /// <summary>
        /// Gets or sets the ISO-639 language code.
        /// </summary>
        public string LanguageCode
        {
            get { return languageCode; }
            set { languageCode = value; }
        }
    }
}
