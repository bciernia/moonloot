using EasyTalk.Nodes.Core;
using System;
using UnityEngine;

namespace EasyTalk.Nodes.Flow
{
    /// <summary>
    /// A node for jumping to another Dialogue asset.
    /// </summary>
    [Serializable]
    public class GotoNode : Node, DialogueFlowNode
    {
        /// <summary>
        /// The Dialogue to jump to.
        /// </summary>
        [SerializeField]
        private Dialogue dialogue;

        /// <summary>
        /// The entry point ID to jump to.
        /// </summary>
        [SerializeField]
        private string entryId;

        /// <summary>
        /// Creates a new GotoNode.
        /// </summary>
        public GotoNode()
        {
            this.name = "GOTO";
            this.nodeType = NodeType.GOTO;
        }

        /// <inheritdoc>
        public NodeConnection GetFlowInput()
        {
            return FindFlowInputs()[0];
        }

        /// <inheritdoc>
        public NodeConnection GetFlowOutput()
        {
            return null;
        }

        /// <summary>
        /// Gets or sets the Dialogue to jump to.
        /// </summary>
        public Dialogue Dialogue
        {
            get { return this.dialogue; }
            set { this.dialogue = value; }
        }

        /// <summary>
        /// Gets or sets the entry point to jump to.
        /// </summary>
        public string EntryID
        {
            get { return this.entryId; }
            set { this.entryId = value; }
        }
    }
}
