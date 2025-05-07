using EasyTalk.Character;
using EasyTalk.Settings;
using UnityEngine;

namespace EasyTalk.Display
{
    /// <summary>
    /// An animatable sprite panel which supports setting the animated image based on a character configured in the Character Library.
    /// </summary>
    public class CharacterSpritePanel : AnimatableSpritePanel
    {
        /// <summary>
        /// The image mode to use. In PORTRAYAL mode, the chosen sprite(s) will be retrieved from a character's configured portrayals. In ICON mode, the sprite(s) come from the character's configured icons.
        /// </summary>
        [SerializeField]
        private CharacterSpriteMode spriteMode = CharacterSpriteMode.PORTRAYAL;

        /// <summary>
        /// Whether or not the panel's image should be updated when a null or empty image ID is used.
        /// </summary>
        [SerializeField]
        private bool showWhenInvalid = true;

        /// <summary>
        /// The character library being used.
        /// </summary>
        private CharacterLibrary characterLibrary;

        /// <summary>
        /// The name of the character currently being shown on the panel.
        /// </summary>
        private string currentCharacterName = null;

        /// <inheritdoc/>
        private void Awake()
        {
            Init();
            HideImmediately();
        }

        /// <inheritdoc/>
        public override void Init()
        {
            base.Init();

            InitializeCharacterLibrary();
        }

        /// <summary>
        /// Sets the character library from the Dialogue Registry.
        /// </summary>
        private void InitializeCharacterLibrary()
        {
            if (characterLibrary == null)
            {
                DialogueDisplay parentDisplay = DialogueDisplay.GetParentDialogueDisplay(this.gameObject);

                if (parentDisplay != null)
                {
                    EasyTalkDialogueSettings settings = parentDisplay.DialogueSettings;

                    if (settings != null && settings.DialogueRegistry != null)
                    {
                        characterLibrary = settings.DialogueRegistry.CharacterLibrary;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Character Library.
        /// </summary>
        public CharacterLibrary CharacterLibrary
        {
            get 
            {
                InitializeCharacterLibrary();
                return this.characterLibrary;
            }
        }

        /// <summary>
        /// Gets the name of the character currently being displayed.
        /// </summary>
        public string CurrentCharacterName
        {
            get { return this.currentCharacterName; }
        }


        /// <summary>
        /// Given a character name, an animatable image will be retrieved from the character library for that character and displayed on the panel. If the panel is set to ICON mode, the image will be pulled
        /// from the configured icons of the character. If the panel is in PORTRAYAL mode, then the image will be pulled from portrayals instead. An image ID can also be specified to use a specific image configured for the character. IF no image ID
        /// is specified, the first image in their list of icons/portrayals will be used.
        /// </summary>
        /// <param name="characterName">The name of the character to display.</param>
        /// <param name="imageId">An optional image ID for the image to retrieve from the character library.</param>
        /// <returns>Returns true if an image was found and set successfully; otherwise false.</returns>
        public bool SetImageOnPanel(string characterName, string imageId = null)
        {
            CharacterDefinition charDef = CharacterLibrary.GetCharacterDefinition(characterName);

            if (charDef != null)
            {
                AnimatableDisplayImage img;

                if (spriteMode == CharacterSpriteMode.PORTRAYAL)
                {
                    if(imageId == null || imageId.Length == 0)
                    {
                        if(showWhenInvalid && charDef.PortrayalSprites.Count > 0)
                        {
                            img = charDef.PortrayalSprites[0];
                        }
                        else
                        {
                            img = null;
                        }
                    }
                    else
                    {
                        img = charDef.GetPortrayalSprite(imageId);
                    }
                }
                else
                {
                    if (imageId == null || imageId.Length == 0)
                    {
                        if (showWhenInvalid && charDef.IconsSprites.Count > 0)
                        {
                            img = charDef.IconsSprites[0];
                        }
                        else
                        {
                            img = null;
                        }
                    }
                    else
                    {
                        img = charDef.GetIconSprite(imageId);
                    }
                }

                if (img != null)
                {
                    this.currentCharacterName = characterName;
                    SetImageOnPanel(img);
                    return true;
                }
                else
                {
                    if (this.debugEnabled)
                    {
                        Debug.LogWarning("No icon/image configuration with an ID of '" + imageId + "' exists for the character named '" + characterName + "' in the Character Library being used.");
                    }
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Defines the character sprite modes for a character sprite panel.
    /// </summary>
    public enum CharacterSpriteMode
    {
        ICON, PORTRAYAL
    }
}
