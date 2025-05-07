using EasyTalk.Controller;
using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Variable
{
    /// <summary>
    /// A node for setting variable values.
    /// </summary>
    [Serializable]
    public class SetVariableNode : Node, DialogueFlowNode, FunctionalNode
    {
        /// <summary>
        /// The name of the variable to set the value of.
        /// </summary>
        [SerializeField]
        private string variableName;

        /// <summary>
        /// The value to set.
        /// </summary>
        [SerializeField]
        private string variableValue;

        /// <summary>
        /// Creates a new SetVariableNode.
        /// </summary>
        public SetVariableNode()
        {
            this.name = "SET VARIABLE";
            this.nodeType = NodeType.SET_VARIABLE_VALUE;
        }

        /// <summary>
        /// Gets or sets the name of the variable to set.
        /// </summary>
        public string VariableName
        {
            get { return this.variableName; }
            set { this.variableName = value; }
        }

        /// <summary>
        /// Gets or sets the value to set on the variable.
        /// </summary>
        public string VariableValue 
        { 
            get { return this.variableValue; }
            set { this.variableValue = value; }
        }

        /// <inheritdoc/>
        public bool DetermineAndStoreValue(NodeHandler nodeHandler, Dictionary<int, object> nodeValues, GameObject convoOwner = null)
        {
            //This node just needs to set a new variable value. No value outputs need to be stored.
            NodeVariable variable = nodeHandler.GetVariable(variableName);

            if (variable != null)
            {
                object newValue = null;

                if(variable.variableType == typeof(int))
                {
                    int intValue = 0;
                    float floatValue = 0;

                    if (Inputs[1].AttachedIDs.Count > 0)
                    {
                        if (float.TryParse(nodeValues[Inputs[1].AttachedIDs[0]].ToString(), out floatValue))
                        {
                            intValue = (int)floatValue;
                        }
                    }
                    else
                    {
                        string variableValueString = nodeHandler.ReplaceVariablesInString(variableValue);
                        if (float.TryParse(variableValueString, out floatValue))
                        {
                            intValue = (int)floatValue;
                        }
                    }

                    newValue = intValue;
                }
                else if(variable.variableType == typeof(string))
                {
                    string stringValue = "";

                    if (Inputs[1].AttachedIDs.Count > 0)
                    {
                        stringValue = nodeValues[Inputs[1].AttachedIDs[0]].ToString();
                    }
                    else
                    {
                        if (variableValue != null)
                        {
                            //string variableValueString = nodeHandler.TranslateText(variableValue);
                            string variableValueString = nodeHandler.ReplaceVariablesInString(variableValue);
                            stringValue = variableValueString;
                        }
                    }

                    newValue = stringValue;
                }
                else if(variable.variableType == typeof(bool))
                {
                    bool boolValue = true;

                    if (Inputs[1].AttachedIDs.Count > 0)
                    {
                        bool.TryParse(nodeValues[Inputs[1].AttachedIDs[0]].ToString(), out boolValue);
                    }
                    else
                    {
                        string variableValueString = nodeHandler.ReplaceVariablesInString(variableValue);
                        bool.TryParse(variableValueString, out boolValue);
                    }

                    newValue = boolValue;
                }
                else if(variable.variableType == typeof(float))
                {
                    float floatValue = 0.0f;

                    if (Inputs[1].AttachedIDs.Count > 0)
                    {
                        float.TryParse(nodeValues[Inputs[1].AttachedIDs[0]].ToString(), out floatValue);
                    }
                    else
                    {
                        string variableValueString = nodeHandler.ReplaceVariablesInString(variableValue);
                        float.TryParse(variableValueString, out floatValue);
                    }
                    
                    newValue = floatValue;
                }

                nodeHandler.SetVariableValue(variableName, newValue);
            }

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
}