using EasyTalk.Controller;
using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Logic
{
    /// <summary>
    /// A node used to dynamically build a string during dialgoue playback.
    /// </summary>
    [Serializable]
    public class BuildStringNode : ListNode, DialogueFlowNode, FunctionalNode
    {
        /// <summary>
        /// Creates a new BuildStringNode.
        /// </summary>
        public BuildStringNode()
        {
            this.name = "BUILD STRING";
            this.nodeType = NodeType.BUILD_STRING;
        }

        /// <inheritdoc/>
        public bool DetermineAndStoreValue(NodeHandler nodeHandler, Dictionary<int, object> nodeValues, GameObject convoOwner = null)
        {
            string value = "";

            for(int i = 0; i < items.Count; i++)
            {
                if (Inputs[i+1].AttachedIDs.Count > 0)
                {
                    object inputValue = nodeValues[Inputs[i + 1].AttachedIDs[0]];

                    if (inputValue != null) 
                    {
                        value += inputValue.ToString();
                    }
                }
                else
                {
                    string itemText = ((StringItem)items[i]).text;
                    //itemText = nodeHandler.TranslateText(itemText);
                    itemText = nodeHandler.ReplaceVariablesInString(itemText);

                    if (itemText != null)
                    {
                        value += itemText;
                    }
                }
            }

            NodeConnection stringOutput = FindOutputOfType(InputOutputType.STRING);
            nodeValues.TryAdd(stringOutput.ID, value);
            return true;
        }

        /// <inheritdoc/>
        public List<int> GetDependencyOutputIDs()
        {
            return FindDependencyOutputIDs();
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

        /// <inheritdoc/>
        public bool HasDependencies()
        {
            return FindDependencyOutputIDs().Count > 0;
        }
    }

    /// <summary>
    /// A string value item used in the 'build string' type node.
    /// </summary>
    [Serializable]
    public class StringItem : ListItem
    {
        /// <summary>
        /// The text of the string.
        /// </summary>
        [SerializeField]
        public string text;

        /// <summary>
        /// Creates a new StringItem.
        /// </summary>
        public StringItem() { }

        /// <summary>
        /// Creates a new StringItem with the specified value.
        /// </summary>
        /// <param name="text">The string value.</param>
        public StringItem(string text)
        {
            this.text = text;
        }
    }
}