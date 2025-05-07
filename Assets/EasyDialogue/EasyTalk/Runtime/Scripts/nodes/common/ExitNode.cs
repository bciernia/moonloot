using EasyTalk.Nodes.Core;
using System;
using UnityEngine;

namespace EasyTalk.Nodes.Common
{
    /// <summary>
    /// A node which provides an exit point for a dialogue.
    /// </summary>
    [Serializable]
    public class ExitNode : Node, DialogueFlowNode
    {
        /// <summary>
        /// The unique name of the exit point.
        /// </summary>
        [SerializeField]
        private string exitPointName = "";

        /// <summary>
        /// Creates a new ExitNode.
        /// </summary>
        public ExitNode()
        {
            this.name = "EXIT";
            this.nodeType = NodeType.EXIT;
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowInput()
        {
            return Inputs[0];
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowOutput()
        {
            return null;
        }

        /// <summary>
        /// Gets or sets the name of the exit point.
        /// </summary>
        public string ExitPointName
        {
            get { return this.exitPointName; }
            set { this.exitPointName = value; }
        }
    }
}