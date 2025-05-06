using EasyTalk.Controller;
using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Logic
{
    /// <summary>
    /// A conditional node which compares two numeric values, outputs the result of the comparison, and continues down the dialogue flow path attributed to the result.
    /// </summary>
    [Serializable]
    public class CompareNumbersNode : Node, DialogueFlowNode, ConditionalNode, FunctionalNode
    {
        /// <summary>
        /// The type of comparison to perform.
        /// </summary>
        [SerializeField]
        private NumberComparisonType comparisonType;

        /// <summary>
        /// The first value to use in the comparison.
        /// </summary>
        [SerializeField]
        private string valueA;

        /// <summary>
        /// The second value to use in the comparison.
        /// </summary>
        [SerializeField]
        private string valueB;

        /// <summary>
        /// Creates a new CompareNumbersNode.
        /// </summary>
        public CompareNumbersNode()
        {
            this.name = "COMPARE NUMBERS";
            this.nodeType = NodeType.NUMBER_COMPARE;
        }

        /// <summary>
        /// Gets or sets the type of comparison this node should perform (from a string equivalent to a NumberComparisonType toString() value).
        /// </summary>
        public string ComparisonTypeString
        {
            get { return comparisonType.ToString(); }
            set { Enum.TryParse<NumberComparisonType>(value, out comparisonType);  }
        }

        /// <summary>
        /// Gets or sets the type of comparison this node should perform.
        /// </summary>
        public NumberComparisonType ComparisonType
        {
            get { return comparisonType; }
            set { comparisonType = value; }
        }

        /// <summary>
        /// Gets or sets the first numeric value to use in the comparison.
        /// </summary>
        public string ValueA
        {
            get { return this.valueA; }
            set { this.valueA = value; }
        }

        /// <summary>
        /// Gets or sets the second numeric value to use in the comparison.
        /// </summary>
        public string ValueB
        {
            get { return this.valueB; }
            set { this.valueB = value; }
        }

        /// <inheritdoc/>
        public bool DetermineAndStoreValue(NodeHandler nodeHandler, Dictionary<int, object> nodeValues, GameObject convoOwner = null)
        {
            float numberA = 0.0f;
            float numberB = 0.0f;

            if (Inputs[1].AttachedIDs.Count > 0)
            {
                object valueObj = nodeValues[Inputs[1].AttachedIDs[0]];
                numberA = Convert.ToSingle(valueObj);
            }
            else
            {
                string valueAString = nodeHandler.ReplaceVariablesInString(valueA);
                float.TryParse(valueAString, out numberA);
            }

            if (Inputs[2].AttachedIDs.Count > 0)
            {
                object valueObj = nodeValues[Inputs[2].AttachedIDs[0]];
                numberB = Convert.ToSingle(valueObj);
            }
            else
            {
                string valueBString = nodeHandler.ReplaceVariablesInString(valueB);
                float.TryParse(valueBString, out numberB);
            }

            NodeConnection boolOutput = FindOutputOfType(InputOutputType.BOOL);

            switch (comparisonType)
            {
                case NumberComparisonType.LESS_THAN: 
                    nodeValues.TryAdd(boolOutput.ID, numberA < numberB);
                    nodeValues.TryAdd(this.ID, numberA < numberB);
                    break;
                case NumberComparisonType.LESS_THAN_OR_EQUAL: 
                    nodeValues.TryAdd(boolOutput.ID, numberA <= numberB);
                    nodeValues.TryAdd(this.ID, numberA <= numberB);
                    break;
                case NumberComparisonType.GREATER_THAN: 
                    nodeValues.TryAdd(boolOutput.ID, numberA > numberB);
                    nodeValues.TryAdd(this.ID, numberA > numberB);
                    break;
                case NumberComparisonType.GREATER_THAN_OR_EQUAL: 
                    nodeValues.TryAdd(boolOutput.ID, numberA >= numberB);
                    nodeValues.TryAdd(this.ID, numberA >= numberB);
                    break;
                case NumberComparisonType.EQUAL_TO: 
                    nodeValues.TryAdd(boolOutput.ID, numberA == numberB);
                    nodeValues.TryAdd(this.ID, numberA == numberB);
                    break;
                case NumberComparisonType.NOT_EQUAL: 
                    nodeValues.TryAdd(boolOutput.ID, numberA != numberB);
                    nodeValues.TryAdd(this.ID, numberA != numberB);
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
    }

    /// <summary>
    /// An enumeration defining different types of comparisons which can be performed on numeric types.
    /// </summary>
    public enum NumberComparisonType { LESS_THAN, GREATER_THAN, EQUAL_TO, LESS_THAN_OR_EQUAL, GREATER_THAN_OR_EQUAL, NOT_EQUAL }
}

