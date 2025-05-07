using EasyTalk.Nodes.Core;
using System;

namespace EasyTalk.Nodes.Flow
{
    /// <summary>
    /// A node which allows a random dialogue flow path to be chosen when reached.
    /// </summary>
    [Serializable]
    public class RandomNode : ListNode, DialogueFlowNode
    {
        /// <summary>
        /// Creates a new RandomNode.
        /// </summary>
        public RandomNode()
        {
            this.name = "RAND";
            this.nodeType = NodeType.RANDOM;
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
                int index = UnityEngine.Random.Range(0,Outputs.Count);
                return Outputs[index];
            }

            return null;
        }
    }

    /// <summary>
    /// A dialogue flow path item for a random node.
    /// </summary>
    [Serializable]
    public class RandomItem : ListItem
    {

    }
}
