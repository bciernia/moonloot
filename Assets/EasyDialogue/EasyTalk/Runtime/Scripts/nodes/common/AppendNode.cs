using UnityEngine;
using EasyTalk.Nodes.Core;
using EasyTalk.Controller;
using System.Collections.Generic;

namespace EasyTalk.Nodes.Common
{
    /// <summary>
    /// A node for appending text to the current conversation.
    /// </summary>
    public class AppendNode : Node, DialogueFlowNode
    {
        /// <summary>
        /// The text to append.
        /// </summary>
        [SerializeField]
        private string text = "";

        /// <summary>
        /// The audio clip to play.
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
        /// Creates a new AppendNode.
        /// </summary>
        public AppendNode() 
        {
            this.name = "APPEND";
            this.nodeType = NodeType.APPEND;
        }

        /// <summary>
        /// Gets or sets the text to append.
        /// </summary>
        public string Text 
        { 
            get { return text; } 
            set { text = value; } 
        }

        /// <summary>
        /// Gets or sets the audio clip to play.
        /// </summary>
        public AudioClip AudioClip
        {
            get { return this.audioClip; }
            set { this.audioClip = value; }
        }

        /// <summary>
        /// Gets or sets the asset ID of the audio clip attributed to the text.
        /// </summary>
        public int AudioAssetID
        {
            get { return this.audioAssetID; }
            set { this.audioAssetID = value; }
        }

        /// <summary>
        /// Gets or sets the audio clip file path attributed to the text.
        /// </summary>
        public string AudioClipFile
        {
            get { return this.audioClipFile; }
            set { this.audioClipFile = value; }
        }

        /// <inheritdoc>
        public NodeConnection GetFlowInput()
        {
            return Inputs[0];
        }

        /// <inheritdoc>
        public NodeConnection GetFlowOutput()
        {
            return Outputs[0];
        }
    }
}
