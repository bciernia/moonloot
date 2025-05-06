using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Variable
{
    /// <summary>
    /// The NodeVariable class is used to store information about a dialogue variable, such as its type, name, initial value, and current value.
    /// </summary
    [Serializable]
    public class NodeVariable
    {
        /// <summary>
        /// The variable type.
        /// </summary>
        [SerializeField]
        public Type variableType;

        /// <summary>
        /// The variable name.
        /// </summary>
        [SerializeField]
        public string variableName;

        /// <summary>
        /// The initial value of the variable.
        /// </summary>
        [SerializeField]
        public object initialValue;

        /// <summary>
        /// The current value of the variable.
        /// </summary>
        [SerializeField]
        public object currentValue;

        /// <summary>
        /// Whether the variable should be reset to its initial value each time the dialogue is entered.
        /// </summary>
        [SerializeField]
        public bool resetOnEntry;

        /// <summary>
        /// Whether the variable is a global variable or not.
        /// </summary>
        [SerializeField]
        public bool isGlobal;

        /// <summary>
        /// Strips all variable references from the provided string and returns a new string.
        /// </summary>
        /// <param name="text">The string to remove variable references from.</param>
        /// <returns>A modified version of the provided string with all references to variables removed.</returns>
        public static string RemoveVariables(string text)
        {
            string newText = text;
            int startIndex = 0;
            int endIndex = 0;

            while ((startIndex = newText.IndexOf("(@", startIndex)) != -1 &&
                (endIndex = newText.IndexOf(')', startIndex + 2)) != -1)
            {
                newText = newText.Substring(0, startIndex) + newText.Substring(endIndex + 1);
                startIndex = endIndex;
            }

            return newText;
        }

        /// <summary>
        /// Takes each variable in the provided string and replaces it with a numeric variable reference. The nameReplacementMap is populated with the new variable reference
        /// names as the keys, and the old variable reference names as the values. This method is useful for translations of text which contain variable references since automatic
        /// translation could potentially rename variable references; nameing the variables numerically prevents this and allows the original variable names to be swapped back
        /// into the string later.
        /// </summary>
        /// <param name="text">The string to index variable references for.</param>
        /// <param name="nameReplacementMap">A Dictionary which maps the new variable reference names to the old names.</param>
        /// <returns>A modified version of the original string with indexed variable references.</returns>
        public static string IndexVariablesNames(string text, Dictionary<string, string> nameReplacementMap)
        {
            string newText = text;
            int startIndex = 0;
            int endIndex = 0;

            string passString = "*";

            while (startIndex < newText.Length &&
                (startIndex = newText.IndexOf("(@", startIndex)) != -1 &&
                startIndex + 2 < newText.Length &&
                (endIndex = newText.IndexOf(')', startIndex + 2)) != -1)
            {
                string oldVariableName = newText.Substring(startIndex + 2, endIndex - startIndex - 2);
                string newVariableName = "(@" + passString + ")";
                newText = newText.Substring(0, startIndex) + newVariableName + newText.Substring(endIndex + 1);
                nameReplacementMap.Add(newVariableName, oldVariableName);
                startIndex = endIndex;

                passString += "*";
            }

            return newText;
        }

        /// <summary>
        /// Given a string, this method replaces the names of each variable reference in the string with the value attributed to the key of the same name in the provided
        /// nameReplacementMap.
        /// </summary>
        /// <param name="text">The string to replace variable name references in.</param>
        /// <param name="nameReplacementMap">A mapping of current variable names to replacement names.</param>
        /// <returns>A modified version of the original string where the original variable name references have been replaced based upon the values in the nameReplacementMap.</returns>
        public static string ReplaceVariablesNames(string text, Dictionary<string, string> nameReplacementMap)
        {
            string newText = text;

            foreach (string currentName in nameReplacementMap.Keys)
            {
                if (newText.Contains(currentName))
                {
                    newText = newText.Replace(currentName, "(@" + nameReplacementMap[currentName] + ")");
                }
            }

            return newText;
        }
    }
}