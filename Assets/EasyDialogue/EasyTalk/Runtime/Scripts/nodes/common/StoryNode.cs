using EasyTalk.Nodes.Core;
using System;
using UnityEngine;

namespace EasyTalk.Nodes.Common
{
    /// <summary>
    /// A node used for writing story and implementing custom logic.
    /// </summary>
    [Serializable]
    public class StoryNode : Node, DialogueFlowNode
    {
        /// <summary>
        /// The story summary.
        /// </summary>
        [SerializeField]
        private string summary = "Story summary goes here...";

        /// <summary>
        /// Creates a new StoryNode.
        /// </summary>
        public StoryNode()
        {
            this.name = "STORY";
            this.nodeType = NodeType.STORY;
        }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        public string Summary
        {
            get { return this.summary; }
            set { this.summary = value; }
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
}