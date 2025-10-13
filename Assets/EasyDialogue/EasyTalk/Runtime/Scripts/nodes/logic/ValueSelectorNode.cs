using EasyTalk.Controller;
using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Logic
{
    /// <summary>
    /// A node for selecting and outputting a value based on a selected index.
    /// </summary>
    [Serializable]
    public class ValueSelectorNode : ListNode, DialogueFlowNode, FunctionalNode
    {
        /// <summary>
        /// The type of value to output.
        /// </summary>
        [SerializeField]
        private ValueOutputType valueOutputType;

        /// <summary>
        /// The string value of the index.
        /// </summary>
        [SerializeField]
        private string indexValue = "0";

        /// <summary>
        /// The index of the value to output.
        /// </summary>
        private int index = 0;

        /// <summary>
        /// Creates a new ValueSelectorNode.
        /// </summary>
        public ValueSelectorNode()
        {
            this.name = "VALUE SELECT";
            this.nodeType = NodeType.VALUE_SELECT;
        }

        /// <summary>
        /// Gets or sets the index of the value to choose.
        /// </summary>
        public string Index
        {
            get { return indexValue; }
            set { indexValue = value; }
        }

        /// <summary>
        /// Gets or sets the value output type of the node.
        /// </summary>
        public string ValueOutputType
        {
            get { return this.valueOutputType.ToString(); }
            set { this.valueOutputType = (ValueOutputType)Enum.Parse(typeof(ValueOutputType), value); }
        }

        /// <inheritdoc/>
        public bool DetermineAndStoreValue(NodeHandler nodeHandler, Dictionary<int, object> nodeValues, GameObject convoOwner = null)
        {
            FindIndex(nodeValues);
            NodeConnection inputConnection = Inputs[index + 2];
            if (inputConnection.AttachedIDs.Count > 0)
            {
                object value = nodeValues[inputConnection.AttachedIDs[0]];

                switch (valueOutputType)
                {
                    case EasyTalk.Nodes.Logic.ValueOutputType.INT:
                        nodeValues.TryAdd(FindOutputOfType(InputOutputType.INT).ID, value);
                        break;
                    case EasyTalk.Nodes.Logic.ValueOutputType.FLOAT:
                        nodeValues.TryAdd(FindOutputOfType(InputOutputType.FLOAT).ID, value);
                        break;
                    case EasyTalk.Nodes.Logic.ValueOutputType.BOOL:
                        nodeValues.TryAdd(FindOutputOfType(InputOutputType.BOOL).ID, value);
                        break;
                    case EasyTalk.Nodes.Logic.ValueOutputType.STRING:
                        nodeValues.TryAdd(FindOutputOfType(InputOutputType.STRING).ID, value);
                        break;
                }
            }
            else
            {
                ValueSelectorListItem item = Items[index] as ValueSelectorListItem;
                object value = null;
                string valueString = nodeHandler.ReplaceVariablesInString(item.Value);

                switch (valueOutputType)
                {
                    case EasyTalk.Nodes.Logic.ValueOutputType.INT:
                        value = Convert.ToInt32(valueString);
                        nodeValues.TryAdd(FindOutputOfType(InputOutputType.INT).ID, value);
                        break;
                    case EasyTalk.Nodes.Logic.ValueOutputType.FLOAT:
                        value = Convert.ToSingle(valueString);
                        nodeValues.TryAdd(FindOutputOfType(InputOutputType.FLOAT).ID, value);
                        break;
                    case EasyTalk.Nodes.Logic.ValueOutputType.BOOL:
                        value = Convert.ToBoolean(valueString);
                        nodeValues.TryAdd(FindOutputOfType(InputOutputType.BOOL).ID, value);
                        break;
                    case EasyTalk.Nodes.Logic.ValueOutputType.STRING:
                        //value = nodeHandler.TranslateText(valueString);
                        nodeValues.TryAdd(FindOutputOfType(InputOutputType.STRING).ID, valueString);
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines the index value to use when selecting a value to output.
        /// </summary>
        /// <param name="nodeValues">A collection of node and connection IDs to their corresponding values.</param>
        public void FindIndex(Dictionary<int, object> nodeValues)
        {
            if (Inputs[1].AttachedIDs.Count > 0)
            {
                int incomingId = Inputs[1].AttachedIDs[0];

                if (nodeValues.ContainsKey(incomingId))
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
        public List<int> GetDependencyOutputIDs()
        {
            return FindDependencyOutputIDs();
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowInput()
        {
            return FindInputOfType(InputOutputType.DIALGOUE_FLOW);
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowOutput()
        {
            return FindOutputOfType(InputOutputType.DIALGOUE_FLOW);
        }

        /// <inheritdoc/>
        public bool HasDependencies()
        {
            return FindDependencyOutputIDs().Count > 0;
        }
    }

    /// <summary>
    /// An enumeration defining the types of values which a value selector node can output.
    /// </summary>
    public enum ValueOutputType { INT, FLOAT, BOOL, STRING }

    /// <summary>
    /// Defines an item used in a value selector node.
    /// </summary>
    [Serializable]
    public class ValueSelectorListItem : ListItem 
    {
        /// <summary>
        /// The value of the item.
        /// </summary>
        [SerializeField]
        private string value = "";

        /// <summary>
        /// Creates a new ValueSelectorListItem.
        /// </summary>
        public ValueSelectorListItem() : base() { }

        /// <summary>
        /// Creates a new ValueSelectorListItem with the provided value.
        /// </summary>
        /// <param name="value">The value of the item.</param>
        public ValueSelectorListItem(string value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets or sets the string value of the item.
        /// </summary>
        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
}
