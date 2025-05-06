using EasyTalk.Nodes.Core;
using System;
using UnityEngine;

namespace EasyTalk.Nodes.Flow
{
    /// <summary>
    /// A node which allows the dialogue flow to jump into a point and continue after jumping from a jump-out node.
    /// </summary>
    [Serializable]
    public class JumpInNode : Node, DialogueFlowNode
    {
        /// <summary>
        /// The unique name or key attribtued to the jump-in node.
        /// </summary>
        [SerializeField]
        private string key = "123";

        /// <summary>
        /// Creates a new JumpInNode.
        /// </summary>
        public JumpInNode()
        {
            this.name = "JUMPIN";
            this.nodeType = NodeType.JUMPIN;
        }

        /// <summary>
        /// Gets or sets the name or jump key attributed to this node.
        /// </summary>
        public string Key
        {
            get { return this.key; }
            set { this.key = value; }
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
    }
}
