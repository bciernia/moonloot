using EasyTalk.Nodes.Core;
using System;
using UnityEngine;

namespace EasyTalk.Nodes.Flow
{
    /// <summary>
    /// A node which waits for a period of time before allowing dialogue flow to continue.
    /// </summary>
    [SerializeField]
    public class WaitNode : Node, DialogueFlowNode
    {
        /// <summary>
        /// The string value for the amount of time to wait, in seconds.
        /// </summary>
        [SerializeField]
        private string waitTime = "1.0";

        /// <summary>
        /// Creates a new WaitNode.
        /// </summary>
        public WaitNode()
        {
            this.name = "WAIT";
            this.nodeType = NodeType.WAIT;
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
        /// Gets or sets the string value for the wait time (in seconds).
        /// </summary>
        public string WaitTime
        {
            get { return waitTime; }
            set { waitTime = value; }
        }

        /// <summary>
        /// Gets the amount of time to wait (in seconds).
        /// </summary>
        /// <returns>The amount of time to wait (in seconds).</returns>
        public float GetWaitTime()
        {
            if (waitTime == null || waitTime.Length == 0)
            {
                return 0.0f;
            }
            else
            {
                return Convert.ToSingle(waitTime);
            }
        }
    }
}
