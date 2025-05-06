using EasyTalk.Controller;
using EasyTalk.Settings;
using System.Collections.Generic;

namespace EasyTalk.Display
{
    /// <summary>
    /// This class implements logic which helps to manage animatable character portrayals/portrait displays during dialogue playback.
    /// </summary>
    public class CharacterDisplay : DialogueListener
    {
        /// <summary>
        /// A List of character sprite panels which will be managed by the display.
        /// </summary>
        private List<CharacterSpritePanel> characterPanels = new List<CharacterSpritePanel>();

        /// <summary>
        /// Whether the display is initialized.
        /// </summary>
        private bool initialized = false;

        /// <inheritdoc/>
        private void Awake()
        {
            if (!initialized)
            {
                Init();
            }
        }

        /// <inheritdoc/>
        private void Init()
        {
            FindCharacterPanels();
            initialized = true;
        }

        /// <summary>
        /// Finds all of the character sprite panels which are children of this display and adds them to the characterPanels List to be managed.
        /// </summary>
        private void FindCharacterPanels()
        {
            characterPanels.Clear();

            CharacterSpritePanel[] spritePanels = GetComponentsInChildren<CharacterSpritePanel>(true);
            foreach(CharacterSpritePanel panel in spritePanels)
            {
                characterPanels.Add(panel);
            }
        }

        /// <summary>
        /// When the dialogue exists, the character display will hide all of the visible characters' displays.
        /// </summary>
        /// <param name="exitPointName">The name of the exist point for the dialogue.</param>
        public override void OnDialogueExited(string exitPointName)
        {
            base.OnDialogueExited(exitPointName);

            HideAll();
        }

        /// <summary>
        /// Hides all managed character panels.
        /// </summary>
        public void HideAll()
        {
            foreach (CharacterSpritePanel panel in characterPanels)
            {
                panel.Hide();
            }
        }

        /// <summary>
        /// Returns whether or not any of the managed character panels is portraying the character who has the specified name.
        /// </summary>
        /// <param name="characterName">The name of the character to check.</param>
        /// <returns>Whether the character is being displayed on any of the managed character panels.</returns>
        public bool IsShowingCharacter(string characterName)
        {
            foreach (CharacterSpritePanel panel in characterPanels)
            {
                if (panel.CurrentCharacterName != null && panel.CurrentCharacterName.Equals(characterName) && !panel.IsHidden)
                {
                    return true;
                }
            }

            return false;
        }
    }
}