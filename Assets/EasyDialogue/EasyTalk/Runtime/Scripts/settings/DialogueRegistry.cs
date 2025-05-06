using EasyTalk.Character;
using EasyTalk.Nodes.Variable;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Settings
{
    /// <summary>
    /// The DialogueRegistry class is used for store and configure information which may be used across dialogue assets and in the editor, such as global variables and character libraries.
    /// </summary>
    [CreateAssetMenu(fileName = "EasyTalk Dialogue Registry", menuName = "EasyTalk/Dialogue Registry", order = 8)]
    public class DialogueRegistry : ScriptableObject
    {
        /// <summary>
        /// The character library used by the registry.
        /// </summary>
        [SerializeField]
        private CharacterLibrary characterLibrary;

        /// <summary>
        /// The List of global variables defined in the registry.
        /// </summary>
        [SerializeField]
        private List<GlobalNodeVariable> variables = new List<GlobalNodeVariable>();

        /// <summary>
        /// Gets or sets the character library to use.
        /// </summary>
        public CharacterLibrary CharacterLibrary
        {
            get { return characterLibrary; }
            set { this.characterLibrary = value; }
        }

        /// <summary>
        /// Gets or sets the List of global variables to use.
        /// </summary>
        public List<GlobalNodeVariable> GlobalVariables
        {
            get { return variables; }
            set { this.variables = value; }
        }

        /// <summary>
        /// Finds and returns the index of the global variable with the specified name, if it exists.
        /// </summary>
        /// <param name="varName">The name of the variable to retrieve.</param>
        /// <returns>The index of the specified global variable within the registry's global variable list. If the variable can't be found, this method returns -1.</returns>
        public int FindVariable(string varName)
        {
            int foundIdx = -1;

            for (int i = 0; i < GlobalVariables.Count; i++)
            {
                GlobalNodeVariable globalVariable = GlobalVariables[i];

                if (globalVariable.VariableName.Equals(varName))
                {
                    foundIdx = i;
                    break;

                }
            }

            return foundIdx;
        }
    }
}
