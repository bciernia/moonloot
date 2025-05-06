using EasyTalk.Nodes.Core;
using System;
using UnityEngine;

namespace EasyTalk.Nodes.Flow
{
    /// <summary>
    /// A node which pauses dialogue playback when encountered.
    /// </summary>
    [Serializable]
    public class PauseNode : Node, DialogueFlowNode
    {
        /// <summary>
        /// A string to pass to callback methods when the pause node is encountered.
        /// </summary>
        [SerializeField]
        private string signalId = "signal";

        /// <summary>
        /// Creates a new Pause node.
        /// </summary>
        public PauseNode()
        {
            this.name = "PAUSE";
            this.nodeType = NodeType.PAUSE;
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

        /// <summary>
        /// Gets or sets the signal string for the Pause node.
        /// </summary>
        public string Signal
        {
            get { return signalId; }
            set { signalId = value; }
        }
    }
}
