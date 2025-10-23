using System;
using UnityEngine;

namespace EasyTalk.Localization
{
    /// <summary>
    /// Contains information about input characters made available for a specific language (to be used in text input displays).
    /// </summary>
    [Serializable]
    public class LanguageInputCharacterSet
    {
        /// <summary>
        /// The language code the character set is for.
        /// </summary>
        [SerializeField]
        private string languageCode = "en";

        /// <summary>
        /// The array of characters available for the language during text input.
        /// </summary>
        [SerializeField]
        private char[] characters = null;

        /// <summary>
        /// Gets or sets the language code for the character set.
        /// </summary>
        public string LanguageCode 
        { 
            get { return languageCode; } 
            set { languageCode = value; }
        }

        /// <summary>
        /// Gets the array of characters available as input characters for the language.
        /// </summary>
        public char[] Characters { get { return characters; } }
    }
}
