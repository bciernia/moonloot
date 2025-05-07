using EasyTalk.Nodes.Core;
using System;

namespace EasyTalk.Nodes.Flow
{
    /// <summary>
    /// A node which allows and ordered sequence of dialogue flow paths to be followed in order each time the node is processed.
    /// </summary>
    [Serializable]
    public class SequenceNode : ListNode, DialogueFlowNode
    {
        /// <summary>
        /// The index of the current dialogue flow path to follow.
        /// </summary>
        private int currentIdx = 0;

        /// <summary>
        /// Creates a new SequenceNode.
        /// </summary>
        public SequenceNode()
        {
            this.name = "SEQ";
            this.nodeType = NodeType.SEQUENCE;
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowInput()
        {
            return Inputs[0];
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowOutput()
        {
            if(Outputs.Count > 0)
            {
                int outputIdx = currentIdx;

                if (currentIdx < Outputs.Count - 1)
                {
                    currentIdx++;
                } 
                else
                {
                    currentIdx = 0;
                }

                return Outputs[outputIdx];
            }

            return null;
        }
    }

    /// <summary>
    /// A dialogue flow path item for a sequence node.
    /// </summary>
    [Serializable]
    public class SequenceItem : ListItem
    {

    }
}