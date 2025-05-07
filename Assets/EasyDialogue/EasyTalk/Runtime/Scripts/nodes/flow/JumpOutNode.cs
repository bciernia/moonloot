using EasyTalk.Nodes.Core;
using System;
using UnityEngine;

namespace EasyTalk.Nodes.Flow
{
    /// <summary>
    /// A node whice allows the dialogue flow to jump from one point to a jump-in node.
    /// </summary>
    [Serializable]
    public class JumpOutNode : Node, DialogueFlowNode
    {
        /// <summary>
        /// The name or key of the jump-in node that this node should jump to.
        /// </summary>
        [SerializeField]
        private string key = "123";

        /// <summary>
        /// Creates a new JumpOutNode.
        /// </summary>
        public JumpOutNode()
        {
            this.name = "JUMPOUT";
            this.nodeType = NodeType.JUMPOUT;
        }

        /// <summary>
        /// Gets or sets the name or jump key of the jump-in node which should be jumped to when this node is reached.
        /// </summary>
        public string Key
        {
            get { return this.key; }
            set { this.key = value; }
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowInput()
        {
            return Outputs[0];
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowOutput()
        {
            return null;
        }
    }
}
