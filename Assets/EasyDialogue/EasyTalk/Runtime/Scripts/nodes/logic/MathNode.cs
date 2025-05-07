using EasyTalk.Controller;
using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Logic
{
    /// <summary>
    /// A node which performs a mathematical operation and sends the result to a numeric value output.
    /// </summary>
    [Serializable]
    public class MathNode : Node, DialogueFlowNode, FunctionalNode
    {
        /// <summary>
        /// The mathematical operation to perform.
        /// </summary>
        [SerializeField]
        private MathOperation operation;

        /// <summary>
        /// The first value to use in the math operation.
        /// </summary>
        [SerializeField]
        private string valueA;

        /// <summary>
        /// The second value to use in the math operation.
        /// </summary>
        [SerializeField]
        private string valueB;

        /// <summary>
        /// Creates a new MathNode.
        /// </summary>
        public MathNode()
        {
            this.name = "MATH";
            this.nodeType = NodeType.MATH;
        }

        /// <summary>
        /// Gets or sets the math operation used by the node (from a string equivalent to a MathOperation toString() value).
        /// </summary>
        public string MathOperationString
        {
            get { return this.operation.ToString(); }
            set { Enum.TryParse<MathOperation>(value, out operation); }
        }

        /// <summary>
        /// Gets or sets the math operation used by the node.
        /// </summary>
        public MathOperation MathOperation
        {
            get { return this.operation; }
            set { operation = value; }
        }

        /// <summary>
        /// Gets or sets the first value to use in the math operation.
        /// </summary>
        public string ValueA
        {
            get { return this.valueA; }
            set { this.valueA = value; }
        }

        /// <summary>
        /// Gets or sets the second value to use in the math operation.
        /// </summary>
        public string ValueB 
        { 
            get { return this.valueB; } 
            set { this.valueB = value; } 
        }

        /// <inheritdoc/>
        public bool DetermineAndStoreValue(NodeHandler nodeHandler, Dictionary<int, object> nodeValues, GameObject convoOwner = null)
        {
            float floatA = 0.0f;
            float floatB = 0.0f;

            if (Inputs[1].AttachedIDs.Count > 0)
            {
                object valueObj = nodeValues[Inputs[1].AttachedIDs[0]];
                floatA = Convert.ToSingle(valueObj);
            }
            else
            {
                string valueAAString = nodeHandler.ReplaceVariablesInString(valueA);
                float.TryParse(valueAAString, out floatA);
            }

            if (Inputs[2].AttachedIDs.Count > 0)
            {
                object valueObj = nodeValues[Inputs[2].AttachedIDs[0]];
                floatB = Convert.ToSingle(valueObj);
            }
            else
            {
                string valueBString = nodeHandler.ReplaceVariablesInString(valueB);
                float.TryParse(valueBString, out floatB);
            }

            NodeConnection resultConn = FindOutputOfType(InputOutputType.NUMBER);

            switch(operation)
            {
                case MathOperation.ADD: 
                    nodeValues.TryAdd(resultConn.ID, floatA + floatB); break;
                case MathOperation.SUBTRACT: 
                    nodeValues.TryAdd(resultConn.ID, floatA - floatB); break;
                case MathOperation.MULTIPLY: 
                    nodeValues.TryAdd(resultConn.ID, floatA * floatB); break;
                case MathOperation.DIVIDE: 
                    nodeValues.TryAdd(resultConn.ID, floatA / floatB); break;
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

    /// <summary>
    /// An enumeration defining the types of mathematical operations which can be performed by a math node.
    /// </summary>
    public enum MathOperation { ADD, SUBTRACT, MULTIPLY, DIVIDE }
}