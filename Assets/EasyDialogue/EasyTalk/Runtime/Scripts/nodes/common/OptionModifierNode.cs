using EasyTalk.Controller;
using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Common
{
    /// <summary>
    /// A node which allows presented dialogue options to be modified during runtime.
    /// </summary>
    [Serializable]
    public class OptionModifierNode : Node, FunctionalNode
    {
        /// <summary>
        /// Whether the option should be displayed.
        /// </summary>
        [SerializeField]
        private bool isDisplayed = true;

        /// <summary>
        /// Whether the option should be selectable.
        /// </summary>
        [SerializeField]
        private bool isSelectable = true;

        /// <summary>
        /// Creates a new OptionModifierNode.
        /// </summary>
        public OptionModifierNode()
        {
            this.name = "OPTION_FILTER";
            this.nodeType = NodeType.OPTION_MOD;
        }

        /// <summary>
        /// Gets or sets whether the option(s) this modifier node is connected to should be displayed.
        /// </summary>
        public bool IsDisplayed
        {
            get { return this.isDisplayed; }
            set { this.isDisplayed = value; }
        }

        /// <summary>
        /// Gets or sets whether the option(s) this modifier node is connected to should be selectable.
        /// </summary>
        public bool IsSelectable
        {
            get { return this.isSelectable; }
            set { this.isSelectable = value; }
        }

        /// <inheritdoc/>
        public bool DetermineAndStoreValue(NodeHandler nodeHandler, Dictionary<int, object> nodeValues, GameObject convoOwner = null)
        {
            OptionModifier modifier = new OptionModifier();

            if(Inputs[0].AttachedIDs.Count > 0)
            {
                string textValue = null;

                object incomingTextValue = nodeValues[Inputs[0].AttachedIDs[0]];
                if(incomingTextValue != null)
                {
                    textValue = incomingTextValue.ToString();
                }

                modifier.text = textValue;
            }

            if (Inputs[1].AttachedIDs.Count > 0)
            {
                bool displayedValue = true;

                object incomingDisplayedValue = nodeValues[Inputs[1].AttachedIDs[0]];
                if(incomingDisplayedValue != null)
                {
                    bool.TryParse(incomingDisplayedValue.ToString(), out displayedValue);
                }

                modifier.displayed = displayedValue;
            }
            else
            {
                modifier.displayed = IsDisplayed;
            }

            if (Inputs[2].AttachedIDs.Count > 0)
            {
                bool selectableValue = true;

                object incomingSelectableValue = nodeValues[Inputs[2].AttachedIDs[0]];
                if (incomingSelectableValue != null)
                {
                    bool.TryParse(incomingSelectableValue.ToString(), out selectableValue);
                }

                modifier.selectable = selectableValue;
            }
            else
            {
                modifier.selectable = isSelectable;
            }

            NodeConnection output = Outputs[0];
            nodeValues.TryAdd(output.ID, modifier);

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

    /// <summary>
    /// Defines the values set by an option modifier.
    /// </summary>
    public class OptionModifier
    {
        /// <summary>
        /// The text to set on an option.
        /// </summary>
        public string text = null;

        /// <summary>
        /// Whether an option should be displayed.
        /// </summary>
        public bool displayed;

        /// <summary>
        /// Whether an option should be selectable.
        /// </summary>
        public bool selectable;
    }
}