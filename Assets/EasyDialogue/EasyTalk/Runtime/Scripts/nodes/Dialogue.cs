using System.Collections.Generic;
using UnityEngine;
using System;
using EasyTalk.Nodes.Core;
using EasyTalk.Localization;

namespace EasyTalk.Nodes
{
    /// <summary>
    /// Defines the structure for an EasyTalk Dialogue asset.
    /// </summary>
    [CreateAssetMenu(fileName = "Dialogue", menuName = "EasyTalk/Dialogue", order = 1)]
    [Serializable]
    public class Dialogue : ScriptableObject
    {
        /// <summary>
        /// A string indicating the current version of EasyTalk in use.
        /// </summary>
        public static string VERSION = "1.7.6";

        /// <summary>
        /// The version number of the dialogue asset.
        /// </summary>
        [SerializeField]
        private string version = VERSION;

        /// <summary>
        /// The List of nodes in the Dialogue.
        /// </summary>
        [SerializeReference]
        private List<Node> nodes = new List<Node>();

        /// <summary>
        /// The library of translations for the Dialogue.
        /// </summary>
        [SerializeField]
        private TranslationLibrary translationLibrary;

        /// <summary>
        /// The maximum ID used in the dialogue.
        /// </summary>
        [SerializeField]
        private int maxID = 0;

        /// <summary>
        /// Gets or sets the version number of the dialogue.
        /// </summary>
        public string Version
        {
            get { return version; }
            set { version = value; }
        }

        /// <summary>
        /// Gets or sets the List of nodes in the dialogue.
        /// </summary>
        public List<Node> Nodes
        {
            get { return nodes; }
            set { nodes = value; }
        }

        /// <summary>
        /// Gets or sets the maximum ID used in the Dialogue.
        /// </summary>
        public int MaxID 
        { 
            get { return maxID; } 
            set { this.maxID = value; } 
        }

        /// <summary>
        /// Gets or sets the translation library to use for the Dialogue.
        /// </summary>
        public TranslationLibrary TranslationLibrary 
        { 
            get { return translationLibrary; } 
            set { this.translationLibrary = value; } 
        }

        /// <summary>
        /// Returns the center point for the Dialogue.
        /// </summary>
        /// <returns>The center location of the Dialogue (the average of all of the node positions).</returns>
        public Vector2 GetCenter()
        {
            Vector2 center = Vector2.zero;

            foreach(Node node in nodes)
            {
                center += new Vector2(node.XPosition, node.YPosition);
            }

            return center / nodes.Count;
        }
    }
}
