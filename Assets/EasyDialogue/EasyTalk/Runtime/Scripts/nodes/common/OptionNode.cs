using EasyTalk.Nodes.Core;
using System;
using UnityEngine;

namespace EasyTalk.Nodes.Common
{
    /// <summary>
    /// A node which allows dialogue options to be presented to the player.
    /// </summary>
    [Serializable]
    public class OptionNode : ListNode, DialogueFlowNode
    {
        /// <summary>
        /// Creates a new OptionNode.
        /// </summary>
        public OptionNode()
        {
            this.name = "OPTION";
            this.nodeType = NodeType.OPTION;
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowInput()
        {
            return FindFlowInputs()[0];
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowOutput()
        {
            return FindFlowOutputs()[0];
        }
    }

    /// <summary>
    /// Defines a dialogue option item.
    /// </summary>
    [Serializable]
    public class OptionItem : ListItem
    {
        /// <summary>
        /// The text of the dialogue option.
        /// </summary>
        [SerializeField]
        public string text;

        /// <summary>
        /// Creates a new OptionItem.
        /// </summary>
        public OptionItem() { }

        /// <summary>
        /// Creates a new OptionItem with the provided text value.
        /// </summary>
        /// <param name="text">The text of the dialogue option.</param>
        public OptionItem(string text)
        {
            this.text = text;
        }
    }
}