using EasyTalk.Controller;
using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Logic
{
    /// <summary>
    /// A conditional node which compares two string values, outputs the result of the comparison, and continues down the dialogue flow path attributed to the result.
    /// </summary>
    [Serializable]
    public class CompareStringsNode : Node, DialogueFlowNode, ConditionalNode, FunctionalNode
    {
        /// <summary>
        /// The type of string comparison to perform.
        /// </summary>
        [SerializeField]
        private StringComparisonType comparisonType;

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
        /// Creates a new CompareStringNode.
        /// </summary>
        public CompareStringsNode()
        {
            this.name = "COMPARE STRINGS";
            this.nodeType = NodeType.STRING_COMPARE;
        }

        /// <summary>
        /// Gets or sets the type of comparison this node should perform (from a string equivalent to a StringComparisonType toString() value).
        /// </summary>
        public string ComparisonTypeString
        {
            get { return comparisonType.ToString(); }
            set { Enum.TryParse<StringComparisonType>(value, out comparisonType); }
        }

        /// <summary>
        /// Gets or sets the string comparison type to perform.
        /// </summary>
        public StringComparisonType ComparisonType
        {
            get { return comparisonType; }
            set { comparisonType = value; }
        }

        /// <summary>
        /// Gets or sets the first value to use in the comparison.
        /// </summary>
        public string ValueA
        {
            get { return this.valueA; }
            set { this.valueA = value; }
        }

        /// <summary>
        /// Gets or sets the second value to use in the comparison.
        /// </summary>
        public string ValueB
        {
            get { return this.valueB; }
            set { this.valueB = value; }
        }

        /// <inheritdoc/>
        public bool DetermineAndStoreValue(NodeHandler nodeHandler, Dictionary<int, object> nodeValues, GameObject convoOwner = null)
        {
            string stringA = "";
            string stringB = "";

            if (Inputs[1].AttachedIDs.Count > 0)
            {
                stringA = nodeValues[Inputs[1].AttachedIDs[0]].ToString();
            }
            else if(valueA != null)
            {
                stringA = nodeHandler.ReplaceVariablesInString(valueA);
            }

            if (Inputs[2].AttachedIDs.Count > 0)
            {
                stringB = nodeValues[Inputs[2].AttachedIDs[0]].ToString();
            }
            else if(valueB != null)
            {
                stringB = nodeHandler.ReplaceVariablesInString(valueB);
            }

            NodeConnection boolOutput = FindOutputOfType(InputOutputType.BOOL);

            switch(comparisonType)
            {
                case StringComparisonType.BEFORE: 
                    nodeValues.TryAdd(boolOutput.ID, stringA.CompareTo(stringB) < 0);
                    nodeValues.TryAdd(this.ID, stringA.CompareTo(stringB) < 0);
                    break;
                case StringComparisonType.AFTER: 
                    nodeValues.TryAdd(boolOutput.ID, stringA.CompareTo(stringB) > 0);
                    nodeValues.TryAdd(this.ID, stringA.CompareTo(stringB) > 0);
                    break;
                case StringComparisonType.EQUAL: 
                    nodeValues.TryAdd(boolOutput.ID, stringA.Equals(stringB));
                    nodeValues.TryAdd(this.ID, stringA.Equals(stringB));
                    break;
                case StringComparisonType.NOT_EQUAL: 
                    nodeValues.TryAdd(boolOutput.ID, !stringA.Equals(stringB));
                    nodeValues.TryAdd(this.ID, !stringA.Equals(stringB));
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
            if(conn.AttachedIDs.Count > 0)
            {
                return conn;
            }

            return GetFalseOutput();
        }

        /// <inheritdoc/>
        public NodeConnection GetOutput(bool value)
        {
            if(value) { return GetTrueOutput(); }

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
    /// An enumeration which defines types of string comparisons which can be performed by a 'compare strings' node.
    /// </summary>
    public enum StringComparisonType { EQUAL, NOT_EQUAL, AFTER, BEFORE }
}