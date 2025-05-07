using System;
using System.Collections.Generic;

#if TEXTMESHPRO_INSTALLED
using TMPro;
#endif

using UnityEngine;

namespace EasyTalk.Localization
{
    /// <summary>
    /// This class is used to store information about the fonts to use for various languages.
    /// </summary>
    [CreateAssetMenu(fileName = "Language Font Overrides", menuName = "EasyTalk/Localization/Language Font Overrides", order = 3)]
    [Serializable]
    public class LanguageFontOverrides : ScriptableObject
    {
        /// <summary>
        /// The List of language font overrides in this collection.
        /// </summary>
        [SerializeField]
        public List<LanguageFontOverride> overrides = new List<LanguageFontOverride>();

        /// <summary>
        /// Checks the list of language font overrides for the specified language, and if found, returns the found language font override.
        /// </summary>
        /// <param name="language">The language for the language font override to retrieve.</param>
        /// <returns>The language font override for the specified language, or null if one can't be found.</returns>
        public LanguageFontOverride GetOverrideForLanguage(string language)
        {
            foreach(LanguageFontOverride fontOverride in overrides)
            {
                if(fontOverride.languageCode.ToLower().Equals(language.ToLower()) || fontOverride.languageName.ToLower().Equals(language.ToLower()))
                {
                    return fontOverride;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// A class used to map a language to a particular font.
    /// </summary>
    [Serializable]
    public class LanguageFontOverride
    {
        /// <summary>
        /// The name of the language.
        /// </summary>
        [SerializeField]
        public string languageName;

        /// <summary>
        /// The ISO-639 language code for the language.
        /// </summary>
        [SerializeField]
        public string languageCode;

        /// <summary>
        /// The regular font to use for the language.
        /// </summary>
        [SerializeField]
        public Font font;

    #if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// The TextMeshPro font to use for the language.
        /// </summary>
        [SerializeField]
        public TMP_FontAsset tmpFont;
#endif

        /// <summary>
        /// The minimum font size to use when displaying the language.
        /// </summary>
        [SerializeField]
        public float minFontSize = 12.0f;

        /// <summary>
        /// The maximum font size to use when display the language.
        /// </summary>
        [SerializeField]
        public float maxFontSize = 32.0f;
    }
}
