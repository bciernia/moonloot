using EasyTalk.Controller;
using EasyTalk.Nodes.Common;
using UnityEngine;
using EasyTalk.Nodes.Core;

namespace EasyTalk.Display
{
    /// <summary>
    /// A display for showing character icons during dialogue playback.
    /// </summary>
    public class IconDisplay : DialogueListener
    {
        /// <summary>
        /// The icon panel, which contains the logic for handling sprite animations, characters, etc..
        /// </summary>
        [SerializeField]
        private CharacterSpritePanel iconPanel;

        /// <summary>
        /// The character currently being displayed.
        /// </summary>
        private string currentCharacter = null;

        /// <summary>
        /// The icon currently being displayed.
        /// </summary>
        private string currentIcon = null;

        /// <summary>
        /// Called when the node changes. If the new node is a conversation node, attempt to set the image on the icon panel for the speaking character.
        /// </summary>
        /// <param name="node">The active dialogue node.</param>
        public override void OnNodeChanged(Node node)
        {
            base.OnNodeChanged(node);

            //If the node is a conversation node, check the icon settings to determine which icon should be shown.
            if(node is ConversationNode)
            {
                ConversationNode convoNode = node as ConversationNode;
                this.currentCharacter = convoNode.CharacterName;
                this.currentIcon = convoNode.Icon;

                if (iconPanel.SetImageOnPanel(convoNode.CharacterName, convoNode.Icon))
                {
                    iconPanel.Show();
                }
                else
                {
                    iconPanel.HideImmediately();
                }
            }
        }

        /// <summary>
        /// Called when a line of dialogue is to be displayed. Attempts to set the image on the icon panel based on the character who is speaking. If no image can be set for the character, the icon panel is hidden.
        /// </summary>
        /// <param name="conversationLine">The line of dialogue being shown.</param>
        public override void OnDisplayLine(ConversationLine conversationLine)
        {
            base.OnDisplayLine(conversationLine);

            //If the character name changed, or an icon is specified which is different than the one currently in use, update the icon.
            if(currentCharacter != null && conversationLine.OriginalCharacterName != null && !currentCharacter.Equals(conversationLine.OriginalCharacterName))
            {
                this.currentCharacter = conversationLine.OriginalCharacterName;
                this.currentIcon = conversationLine.IconID;

                if(!iconPanel.SetImageOnPanel(conversationLine.OriginalCharacterName, conversationLine.IconID))
                {
                    iconPanel.HideImmediately();
                }
            }
        }
    }
}
