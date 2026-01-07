using EasyTalk.Character;
using EasyTalk.Localization;
using EasyTalk.Nodes.Core;
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
        /// Defines a list of node types which are allowed to be translated.
        /// </summary>
        [Tooltip("The translation system will attempt to lookup translations for certain fields in the node types added to this list " +
            "if using an alternate language from the source/default language.")]
        [SerializeField]
        [NonReorderable]
        private List<NodeType> translatedNodeTypes = new List<NodeType>() { NodeType.CONVO, NodeType.OPTION, NodeType.APPEND };

        /// <summary>
        /// Whether or not a single translation library should be used rather than each dialogue asset's specific library.
        /// </summary>
        [Tooltip("When set to true, a comprehensive translation library will be used from the registry to perform translations " +
            "instead of using the translation library directly from each dialogue asset.")]
        [SerializeField]
        private bool useSingleTranslationLibrary = false;

        /// <summary>
        /// The translation library to use when using a single library to perform translations.
        /// </summary>
        [Tooltip("The translation library to use when using a single library to perform translations.")]
        [SerializeField]
        private TranslationLibrary translationLibrary = null;

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
        /// Gets or sets the list of node types which are allowed to be translated.
        /// </summary>
        public List<NodeType> TranslatedNodeTypes
        {
            get { return translatedNodeTypes; }
            set { this.translatedNodeTypes = value; }
        }

        /// <summary>
        /// Gets or sets whether a single translation library should be used, rather than the translation library of an individual dialogue.
        /// </summary>
        public bool UseSingleTranslationLibrary
        {
            get { return useSingleTranslationLibrary; }
            set { this.useSingleTranslationLibrary = value; }
        }

        /// <summary>
        /// Gets or sets the translation library to use when the registry is set to use a single translation library.
        /// </summary>
        public TranslationLibrary TranslationLibrary
        {
            get { return translationLibrary; }
            set { this.translationLibrary = value; }
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
