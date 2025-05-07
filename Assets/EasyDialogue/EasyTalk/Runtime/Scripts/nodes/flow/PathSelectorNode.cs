using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Flow
{
    /// <summary>
    /// A node which allows a dialogue flow path to be chosen based on the index of the path set on the node.
    /// </summary>
    [Serializable]
    public class PathSelectorNode : ListNode, DialogueFlowNode
    {
        /// <summary>
        /// The string value of the index.
        /// </summary>
        [SerializeField]
        private string indexValue = "0";

        /// <summary>
        /// The index of the dialogue flow path which should be proceeded down.
        /// </summary>
        private int index = 0;

        /// <summary>
        /// Creates a new PathSelectorNode.
        /// </summary>
        public PathSelectorNode()
        {
            this.name = "PATH SELECT";
            this.nodeType = NodeType.PATH_SELECT;
        }

        /// <summary>
        /// Gets or sets the index of the dialogue flow path to proceed down.
        /// </summary>
        public string Index 
        { 
            get { return indexValue; } 
            set { indexValue = value; }
        } 

        /// <summary>
        /// Determines the index of the dialogoue flow path to use.
        /// </summary>
        /// <param name="nodeValues">A collection of node and connection IDs to their corresponding values.</param>
        public void FindIndex(Dictionary<int, object> nodeValues)
        {
            if (Inputs[1].AttachedIDs.Count > 0)
            {
                int incomingId = Inputs[1].AttachedIDs[0];

                if(nodeValues.ContainsKey(incomingId))
                {
                    index = Convert.ToInt32(nodeValues[incomingId]);
                }
            }
            else
            {
                int.TryParse(indexValue, out index);
            }

            index = Mathf.Min(index, Items.Count - 1);
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowInput()
        {
            return FindFlowInputs()[0];
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowOutput()
        {
            List<NodeConnection> outgoingConnections = FindFlowOutputs();
            if(index < outgoingConnections.Count)
            {
                return outgoingConnections[index];
            }

            return outgoingConnections[0];
        }
    }

    /// <summary>
    ///  A dialogue flow path item for a path selector node.
    /// </summary>
    public class PathSelectorListItem : ListItem { }
}