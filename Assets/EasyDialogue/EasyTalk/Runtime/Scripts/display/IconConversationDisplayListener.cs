using UnityEngine;

namespace EasyTalk.Display
{
    /// <summary>
    /// A conversation display listener used to hide the icon panel whenever the conversation display is reset.
    /// </summary>
    public class IconConversationDisplayListener : ConversationDisplayListener
    {
        /// <summary>
        /// The icon panel.
        /// </summary>
        [SerializeField]
        private CharacterSpritePanel iconPanel;

        /// <inheritdoc/>
        public override void OnReset()
        {
            base.OnReset();
            iconPanel.Hide();
        }
    }
}
