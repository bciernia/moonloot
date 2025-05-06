using EasyTalk.Controller;
using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Variable
{
    /// <summary>
    /// A node which allows a variable value to be retrieved and passed into another node's value input.
    /// </summary>
    [Serializable]
    public class GetVariableNode : Node, DialogueFlowNode, FunctionalNode
    {
        /// <summary>
        /// The name of the variable to retrieve a value for.
        /// </summary>
        [SerializeField]
        private string variableName;

        /// <summary>
        /// Creates a new GetVariableNode.
        /// </summary>
        public GetVariableNode()
        {
            this.name = "GET VARIABLE";
            this.nodeType = NodeType.GET_VARIABLE_VALUE;
        }

        /// <summary>
        /// Gets or sets the name of the variable this node retrieves the value of.
        /// </summary>
        public string VariableName 
        { 
            get { return this.variableName; } 
            set { this.variableName = value; }
        }

        /// <inheritdoc/>
        public bool DetermineAndStoreValue(NodeHandler nodeHandler, Dictionary<int, object> nodeValues, GameObject convoOwner = null)
        {
            NodeVariable variable = nodeHandler.GetVariable(variableName);
            if(variable != null)
            {
                NodeConnection valueConn = null;

                if (variable.variableType == typeof(int))
                {
                    valueConn = FindOutputOfType(InputOutputType.INT);
                }
                else if (variable.variableType == typeof(string))
                {
                    valueConn = FindOutputOfType(InputOutputType.STRING);
                }
                else if (variable.variableType == typeof(float))
                {
                    valueConn = FindOutputOfType(InputOutputType.FLOAT);
                }
                else if (variable.variableType == typeof(bool))
                {
                    valueConn = FindOutputOfType(InputOutputType.BOOL);
                }

                if (valueConn != null)
                {
                    nodeValues.TryAdd(valueConn.ID, variable.currentValue);
                }
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