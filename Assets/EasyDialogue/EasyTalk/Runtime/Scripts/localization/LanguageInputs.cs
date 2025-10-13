using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Localization
{
    /// <summary>
    /// This class is used to store the configuration for input characters (used by text input displays) for various languages.
    /// </summary>
    [CreateAssetMenu(fileName = "Language Inputs", menuName = "EasyTalk/Localization/Language Inputs", order = 4)]
    [SerializeField]
    public class LanguageInputs : ScriptableObject
    {
        /// <summary>
        /// A list containing input character sets for various languages.
        /// </summary>
        [SerializeField]
        private List<LanguageInputCharacterSet> characterSet = new List<LanguageInputCharacterSet>();

        /// <summary>
        /// Gets the List of input characters sets for all languages. 
        /// </summary>
        public List<LanguageInputCharacterSet> CharacterSet { get { return characterSet; } }
    }
}
