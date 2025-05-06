using EasyTalk.Nodes.Core;
using System;
using UnityEngine;

namespace EasyTalk.Nodes.Common
{
    /// <summary>
    /// A node which provides an entry point into a dialogue.
    /// </summary>
    [Serializable]
    public class EntryNode : Node, DialogueFlowNode
    {
        /// <summary>
        /// The unique name of the entry point.
        /// </summary>
        [SerializeField]
        private string entryPointName = "";

        /// <summary>
        /// Creates a new EntryNode.
        /// </summary>
        public EntryNode()
        {
            this.name = "ENTRY";
            this.nodeType = NodeType.ENTRY;
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowInput()
        {
            return null;
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowOutput()
        {
            return Outputs[0];
        }

        /// <summary>
        /// Gets or sets the name of the entry point.
        /// </summary>
        public string EntryPointName
        {
            get { return entryPointName; }
            set { entryPointName = value; }
        }
    }
}