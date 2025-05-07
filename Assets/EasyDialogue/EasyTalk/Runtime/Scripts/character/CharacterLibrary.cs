using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Character
{
    /// <summary>
    /// The Character Library allows for characters to be defined, including information such as names, icons, and portrayal images/spritesets.
    /// </summary>
    [CreateAssetMenu(fileName = "Character Library", menuName = "EasyTalk/Settings/Character Library", order = 11)]
    public class CharacterLibrary : ScriptableObject
    {
        /// <summary>
        /// The List of Character Definitions containing information about each character which has been configured in the library.
        /// </summary>
        [SerializeField]
        private List<CharacterDefinition> characters = new List<CharacterDefinition>();

        /// <summary>
        /// Gets or sets the List of characters in the character library.
        /// </summary>
        public List<CharacterDefinition> Characters
        {
            get { return this.characters; }
            set { this.characters = value; }
        }

        /// <summary>
        /// Gets the CharacterDefinition for the character with the specified name, if there is a matching character with that name in the library.
        /// </summary>
        /// <param name="name">The name of the character to retrieve configuration information for.</param>
        /// <returns>The CharacterDefinition for the specified character, if that character exists; otherwise this method returns null.</returns>
        public CharacterDefinition GetCharacterDefinition(string name)
        {
            foreach (CharacterDefinition character in this.characters)
            {
                if (character.CharacterName.Equals(name))
                {
                    return character;
                }
            }

            return null;
        }
    }
}
