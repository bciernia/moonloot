using EasyTalk.Nodes.Core;
using System;
using UnityEngine;

namespace EasyTalk.Nodes.Common
{
    /// <summary>
    /// A node which is used to store and handle lines of dialogue.
    /// </summary>
    [Serializable]
    public class ConversationNode : ListNode, DialogueFlowNode
    {
        /// <summary>
        /// The name of the character presumed to be speaking.
        /// </summary>
        [SerializeField]
        private string characterName = "N/A";

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private string icon;

        /// <summary>
        /// Creates a new ConversationNode.
        /// </summary>
        public ConversationNode()
        {
            this.name = "CONVERSATION";
            this.nodeType = NodeType.CONVO;
        }

        /// <summary>
        /// Gets or sets the character name to use.
        /// </summary>
        public string CharacterName
        {
            get { return characterName; }
            set { characterName = value; }
        }

        /// <summary>
        /// Gets or sets the icon ID for the icon to be shown when the first line of dialogue in the node is played.
        /// </summary>
        public string Icon
        {
            get { return this.icon; }
            set { this.icon = value; }
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowInput()
        {
            return Inputs[0];
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowOutput()
        {
            return Outputs[0];
        }
    }

    /// <summary>
    /// An item representing a line of dialogue in a conversation node.
    /// </summary>
    [Serializable]
    public class ConversationItem : ListItem
    {
        /// <summary>
        /// The text of the line of dialogue.
        /// </summary>
        [SerializeField]
        private string text;

        /// <summary>
        /// The audio clip attributed to the line of dialogue.
        /// </summary>
        [SerializeField]
        private AudioClip audioClip;

        /// <summary>
        /// The audio clip file attributed to the line of dialogue.
        /// </summary>
        [SerializeField]
        private string audioClipFile;

        /// <summary>
        /// The asset ID of the audio file attributed to the line of dialogue.
        /// </summary>
        [SerializeField]
        private int audioAssetID;

        /// <summary>
        /// Creates a new ConversationItem.
        /// </summary>
        public ConversationItem() { }

        /// <summary>
        /// Creates a new ConversationItem with the provided values.
        /// </summary>
        /// <param name="text">The text of the line of dialogue.</param>
        /// <param name="audioClip">The audio clip attributed to the line of dialogue.</param>
        /// <param name="audioClipFile">The filepath to the audio file.</param>
        public ConversationItem(string text, AudioClip audioClip, string audioClipFile)
        {
            this.text = text;
            this.audioClip = audioClip;
            this.audioClipFile = audioClipFile;

            if (audioClip != null)
            {
                this.audioAssetID = audioClip.GetInstanceID();
            }
        }

        /// <summary>
        /// Gets or sets the text of the line of dialogue.
        /// </summary>
        public string Text
        {
            get { return this.text; }
            set { this.text = value; }
        }

        /// <summary>
        /// Gets or sets the audio clip attributed to the line of dialogue.
        /// </summary>
        public AudioClip AudioClip
        {
            get { return this.audioClip; }
            set { this.audioClip = value; }
        }

        /// <summary>
        /// Gets or sets the asset ID of the audio clip attributed to the line of dialogue.
        /// </summary>
        public int AudioAssetID
        {
            get { return this.audioAssetID; }
            set { this.audioAssetID = value; }
        }

        /// <summary>
        /// Gets or sets the audio clip file path attributed to the line of dialogue.
        /// </summary>
        public string AudioClipFile
        {
            get { return this.audioClipFile; }
            set { this.audioClipFile = value; }
        }
    }
}