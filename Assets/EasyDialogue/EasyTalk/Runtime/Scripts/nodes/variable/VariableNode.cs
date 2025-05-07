using EasyTalk.Controller;
using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Variable
{
    /// <summary>
    /// A node for defining a variable.
    /// </summary>
    [Serializable]
    public class VariableNode : Node, FunctionalNode
    {
        /// <summary>
        /// The name of the variable.
        /// </summary>
        [SerializeField]
        private string variableName;

        /// <summary>
        /// The initial value of the variable.
        /// </summary>
        [SerializeField]
        private string variableValue;

        /// <summary>
        /// Whether the variable value should be reset to its original value when its dialogue is entered.
        /// </summary>
        [SerializeField]
        private bool resetOnEntry = true;

        /// <summary>
        /// Whether the variable is a global variable.
        /// </summary>
        [SerializeField]
        private bool isGlobal = false;

        /// <summary>
        /// Creates a new VariableNode.
        /// </summary>
        public VariableNode()
        {
            this.name = "VARIABLE";
        }

        /// <summary>
        /// Gets or sets the name of the variable.
        /// </summary>
        public string VariableName
        {
            get { return this.variableName; }
            set { this.variableName = value; }
        }

        /// <summary>
        /// Gets or sets the initial value of the variable.
        /// </summary>
        public string VariableValue 
        { 
            get { return this.variableValue; } 
            set { this.variableValue = value; } 
        }

        /// <summary>
        /// Gets or sets whether the variable should be reset whenever its dialogue is entered.
        /// </summary>
        public bool ResetOnEntry
        {
            get { return this.resetOnEntry; }
            set { this.resetOnEntry = value; }
        }

        /// <summary>
        /// Gets or sets whether the variable is a global variable.
        /// </summary>
        public bool IsGlobal
        {
            get { return this.isGlobal; }
            set { this.isGlobal = value; }
        }

        /// <inheritdoc/>
        public bool DetermineAndStoreValue(NodeHandler nodeHandler, Dictionary<int, object> nodeValues, GameObject convoOwner = null)
        {
            NodeVariable nodeVariable = nodeHandler.GetVariable(variableName);

            if(nodeVariable != null)
            {
                NodeConnection output = Outputs[0];

                nodeValues.TryAdd(output.ID, nodeVariable.currentValue);
            }

            return true;
        }

        /// <inheritdoc/>
        public List<int> GetDependencyOutputIDs()
        {
            return FindDependencyOutputIDs();
        }

        /// <inheritdoc/>
        public bool HasDependencies()
        {
            return FindDependencyOutputIDs().Count > 0;
        }
    }
}
