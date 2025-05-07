using EasyTalk.Controller;
using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Logic
{
    /// <summary>
    /// A node which performs a boolean logic operation, outputs the boolean result, and continues down the dialogue flow path attributed to that result.
    /// </summary>
    [Serializable]
    public class LogicNode : Node, DialogueFlowNode, ConditionalNode, FunctionalNode
    {
        /// <summary>
        /// The logic operation to use.
        /// </summary>
        [SerializeField]
        private LogicOperation logicOperation = LogicOperation.AND;

        /// <summary>
        /// Creates a new LogicNode.
        /// </summary>
        public LogicNode()
        {
            this.name = "BOOL LOGIC";
            this.nodeType = NodeType.BOOL_LOGIC;
        }

        /// <summary>
        /// Gets or sets the logic operation to use.
        /// </summary>
        public LogicOperation LogicMode
        {
            get { return this.logicOperation; }
            set { this.logicOperation = value; }
        }

        /// <summary>
        /// Gets or sets the logic operation to use (from a string equivalent to a LogicOperation toString() value).
        /// </summary>
        [SerializeField]
        public string LogicOperationString
        {
            get { return this.logicOperation.ToString(); }
            set { Enum.TryParse<LogicOperation>(value, out logicOperation); }
        }

        /// <summary>
        /// Returns the logic operation used by the node.
        /// </summary>
        /// <returns>The logic operation used by this node.</returns>
        public LogicOperation GetLogicMode()
        {
            return this.logicOperation;
        }

        /// <summary>
        /// Sets the logic operation used by the node.
        /// </summary>
        /// <param name="operation">The logic operation to use.</param>
        public void SetLogicMode(LogicOperation operation)
        {
            this.logicOperation = operation;
        }

        /// <inheritdoc/>
        public bool DetermineAndStoreValue(NodeHandler nodeHandler, Dictionary<int, object> nodeValues, GameObject convoOwner = null)
        {
            bool valueA = true;
            bool valueB = true;

            if (Inputs[1].AttachedIDs.Count > 0)
            {
                valueA = (bool)nodeValues[Inputs[1].AttachedIDs[0]];
            }

            if (Inputs[2].AttachedIDs.Count > 0)
            {
                valueB = (bool)nodeValues[Inputs[2].AttachedIDs[0]];
            }

            NodeConnection boolOutput = FindOutputOfType(InputOutputType.BOOL);

            switch (this.logicOperation)
            {
                case LogicOperation.AND: 
                    nodeValues.TryAdd(boolOutput.ID, valueA && valueB);
                    nodeValues.TryAdd(this.ID, valueA && valueB);
                    break;
                case LogicOperation.OR: 
                    nodeValues.TryAdd(boolOutput.ID, valueA || valueB);
                    nodeValues.TryAdd(this.ID, valueA || valueB);
                    break;
                case LogicOperation.NOT: 
                    nodeValues.TryAdd(boolOutput.ID, !valueA);
                    nodeValues.TryAdd(this.ID, !valueA);
                    break;
                case LogicOperation.XOR: 
                    nodeValues.TryAdd(boolOutput.ID, valueA ^ valueB);
                    nodeValues.TryAdd(this.ID, valueA ^ valueB);
                    break;
                case LogicOperation.IS_TRUE: 
                    nodeValues.TryAdd(boolOutput.ID, valueA);
                    nodeValues.TryAdd(this.ID, valueA);
                    break;
                case LogicOperation.IS_FALSE: 
                    nodeValues.TryAdd(boolOutput.ID, !valueA);
                    nodeValues.TryAdd(this.ID, !valueA);
                    break;
            }

            return true;
        }

        /// <inheritdoc/>
        public List<int> GetDependencyOutputIDs()
        {
            return FindDependencyOutputIDs();
        }

        /// <inheritdoc/>
        public NodeConnection GetFalseOutput()
        {
            return FindOutputOfType(InputOutputType.DIALOGUE_FALSE_FLOW);
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowInput()
        {
            return FindFlowInputs()[0];
        }

        /// <inheritdoc/>
        public NodeConnection GetFlowOutput()
        {
            NodeConnection conn = GetTrueOutput();
            if (conn.AttachedIDs.Count > 0)
            {
                return conn;
            }

            return GetFalseOutput();
        }

        /// <inheritdoc/>
        public NodeConnection GetOutput(bool value)
        {
            if (value) { return GetTrueOutput(); }

            return GetFalseOutput();
        }

        /// <inheritdoc/>
        public NodeConnection GetTrueOutput()
        {
            return FindOutputOfType(InputOutputType.DIALOGUE_TRUE_FLOW);
        }

        /// <inheritdoc/>
        public bool HasDependencies()
        {
            return FindDependencyOutputIDs().Count > 0;
        }

        /// <summary>
        /// An enumeration defining the types of boolean logic operations which can be performed by a logic node.
        /// </summary>
        [Serializable]
        public enum LogicOperation
        {
            AND, OR, XOR, NOT, IS_TRUE, IS_FALSE
        }
    }
}
