using EasyTalk.Controller;
using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Logic
{
    /// <summary>
    /// A node which chooses a value and sends it to its value output based on its current boolean value/input.
    /// </summary>
    [Serializable]
    public class ConditionalValueNode : Node, DialogueFlowNode, FunctionalNode
    {
        /// <summary>
        /// The type of value this node outputs.
        /// </summary>
        [SerializeField]
        private ValueOutputType valueOutputType;

        /// <summary>
        /// The value to output when the node evaluates to true.
        /// </summary>
        [SerializeField]
        private string trueValue;

        /// <summary>
        /// The value to output when the node evaluates to false.
        /// </summary>
        [SerializeField]
        private string falseValue;

        /// <summary>
        /// The boolean value of the node.
        /// </summary>
        [SerializeField]
        private bool boolValue;
        
        /// <summary>
        /// Creates a new ConditionalValueNode.
        /// </summary>
        public ConditionalValueNode()
        {
            this.name = "CONDITIONAL VALUE";
            this.nodeType = NodeType.CONDITIONAL_VALUE;
        }

        /// <inheritdoc/>
        public bool DetermineAndStoreValue(NodeHandler nodeHandler, Dictionary<int, object> nodeValues, GameObject convoOwner = null)
        {

            NodeConnection inputConnection = Inputs[1];
            bool currentBoolValue = true;
            if (inputConnection.AttachedIDs.Count > 0) 
            {
                object incomingBoolValue = nodeValues[inputConnection.AttachedIDs[0]];
                currentBoolValue = Convert.ToBoolean(incomingBoolValue);
            }
            else
            {
                currentBoolValue = boolValue;
            }

            object value = null;

            if(currentBoolValue)
            {
                if (Inputs[2].AttachedIDs.Count > 0)
                {
                    value = nodeValues[Inputs[2].AttachedIDs[0]];
                }
                else
                {
                    string trueValueString = nodeHandler.ReplaceVariablesInString(trueValue);
                    value = DetermineValue(trueValueString);
                }
            }
            else
            {
                if (Inputs[3].AttachedIDs.Count > 0)
                {
                    value = nodeValues[Inputs[3].AttachedIDs[0]];
                }
                else
                {
                    string falseValueString = nodeHandler.ReplaceVariablesInString(falseValue);
                    value = DetermineValue(falseValueString);
                }
            }

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
                    //value = nodeHandler.TranslateText(value as string);
                    nodeValues.TryAdd(FindOutputOfType(InputOutputType.STRING).ID, value);
                    break;
            }

            return true;
        }

        /// <summary>
        /// Determines and returns the appropriate type of value for the string value provided based on the type of value this node is set to output.
        /// </summary>
        /// <param name="valueString">The string to get a value for.</param>
        /// <returns>The value this node should output.</returns>
        private object DetermineValue(string valueString)
        {
            object value = null;

            switch (valueOutputType)
            {
                case EasyTalk.Nodes.Logic.ValueOutputType.INT:
                    value = Convert.ToInt32(valueString);
                    break;
                case EasyTalk.Nodes.Logic.ValueOutputType.FLOAT:
                    value = Convert.ToSingle(valueString);
                    break;
                case EasyTalk.Nodes.Logic.ValueOutputType.BOOL:
                    value = Convert.ToBoolean(valueString);
                    break;
                case EasyTalk.Nodes.Logic.ValueOutputType.STRING:
                    value = valueString;
                    break;
            }

            return value;
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

        /// <summary>
        /// Gets or sets the value which will be output when the node evaluates to true.
        /// </summary>
        public string TrueValue
        {
            get { return trueValue; }
            set { trueValue = value; }
        }

        /// <summary>
        /// Gets or sets the value which will be output when the node evaluates to false.
        /// </summary>
        public string FalseValue
        {
            get { return falseValue; }
            set { falseValue = value; }
        }

        /// <summary>
        /// Gets or sets the boolean value the node is set to.
        /// </summary>
        public bool BoolValue
        {
            get { return this.boolValue; }
            set { this.boolValue = value; }
        }

        /// <summary>
        /// Gets or sets the type of value this node outputs (from a string equivalent to a ValueOutputType toString() value).
        /// </summary>
        public string ValueOutputType
        {
            get { return this.valueOutputType.ToString(); }
            set { this.valueOutputType = (ValueOutputType)Enum.Parse(typeof(ValueOutputType), value); }
        }
    }
}
