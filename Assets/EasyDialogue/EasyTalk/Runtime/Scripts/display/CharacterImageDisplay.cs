using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EasyTalk.Display
{
    /// <summary>
    /// A UI component used to display a character image.
    /// </summary>
    public class CharacterImageDisplay : ConversationDisplayListener
    {
        /// <summary>
        /// The Image component used to display the character image.
        /// </summary>
        [Tooltip("An image component used to display the character icon.")]
        [SerializeField]
        private Image characterIconImage;

        /// <summary>
        /// A List of Character Image configurations mapping character names to sprites to use for each character.
        /// </summary>
        [SerializeField]
        private List<CharacterImage> characterImages = new List<CharacterImage>();

        /// <summary>
        /// Searches for an image component to use on the GameObject if one isn't set for the character image.
        /// </summary>
        private void Awake()
        {
            if(characterIconImage == null)
            {
                characterIconImage = GetComponent<Image>();
            }
        }

        /// <summary>
        /// Updates the character image sprite to show the sprite for the current character.
        /// </summary>
        /// <param name="characterName">The derived/translated name of the character a sprite should be shown for (ignored in this method).</param>
        /// <param name="sourceName">The original/source name of the character a sprite should be shown for.</param>
        public override void OnCharacterNameUpdated(string characterName, string sourceName)
        {
            base.OnCharacterNameUpdated(characterName, sourceName);

            if (characterIconImage != null)
            {
                characterIconImage.enabled = true;
                CharacterImage defaultIcon = null;

                bool iconSet = false;
                foreach (CharacterImage icon in characterImages)
                {
                    if (icon.characterName.ToLower().Equals(sourceName.ToLower()))
                    {
                        characterIconImage.sprite = icon.sprite;
                        iconSet = true;
                        break;
                    }
                    else if (icon.characterName.ToLower().Equals("default"))
                    {
                        defaultIcon = icon;
                    }
                }

                if (!iconSet)
                {
                    if (defaultIcon != null)
                    {
                        characterIconImage.sprite = defaultIcon.sprite;
                    }
                    else
                    {
                        characterIconImage.enabled = false;
                        characterIconImage.sprite = null;
                    }
                }
            }
        }

        /// <summary>
        /// Disables the character image when reset.
        /// </summary>
        public override void OnReset()
        {
            base.OnReset();

            if (characterIconImage != null)
            {
                characterIconImage.enabled = false;
            }
        }
    }

    /// <summary>
    /// Configuration class for mapping a character name to a sprite.
    /// </summary>
    [Serializable]
    public class CharacterImage
    {
        /// <summary>
        /// The character name.
        /// </summary>
        [SerializeField]
        public string characterName;

        /// <summary>
        /// The sprite to use for the character.
        /// </summary>
        [SerializeField]
        public Sprite sprite;
    }
}
