using System;
using UnityEngine;

namespace EasyTalk.Nodes.Variable
{
    /// <summary>
    /// Defines the structure used for global dialogue variables.
    /// </summary>
    [Serializable]
    public class GlobalNodeVariable
    {
        /// <summary>
        /// The name of the variable.
        /// </summary>
        [SerializeField]
        private string variableName;

        /// <summary>
        /// The type of the variable.
        /// </summary>
        [SerializeField]
        private GlobalVariableType variableType;

        /// <summary>
        /// The initial value (as a string) of the variable.
        /// </summary>
        [SerializeField]
        private string initialValue;

        /// <summary>
        /// Gets or sets the name of the variable.
        /// </summary>
        public string VariableName
        {
            get { return variableName; }
            set { variableName = value; }
        }

        /// <summary>
        /// Gets or sets the type of the variable.
        /// </summary>
        public GlobalVariableType VariableType
        {
            get { return variableType; }
            set { variableType = value; }
        }

        /// <summary>
        /// Gets or sets the initial value of the variable.
        /// </summary>
        public string InitialValue
        {
            get { return initialValue; }
            set { initialValue = value; }
        }

        /// <summary>
        /// Returns the true type of the variable (int, float, string, or bool).
        /// </summary>
        /// <returns>The true Type of the variable./returns>
        public Type GetTrueType()
        {
            switch (variableType)
            {
                case GlobalVariableType.STRING: return typeof(string);
                case GlobalVariableType.INT: return typeof(int);
                case GlobalVariableType.FLOAT: return typeof(float);
                case GlobalVariableType.BOOL: return typeof(bool);
            }

            return typeof(object);
        }
    }

    /// <summary>
    /// An enum defining global variable types.
    /// </summary>
    public enum GlobalVariableType { STRING, INT, FLOAT, BOOL }
}
