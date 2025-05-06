using EasyTalk.Controller;
using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Utility
{
    /// <summary>
    /// A node which is used to show Dialogue Panels and Character portrayals during dialogue playback.
    /// </summary>
    [Serializable]
    public class ShowNode : ListNode, DialogueFlowNode, AsyncNode
    {
        /// <summary>
        /// Whether or not execution is completed on the node.
        /// </summary>
        [NonSerialized]
        private bool isExecutionComplete = false;

        /// <summary>
        /// The NodeHandler used to process the node.
        /// </summary>
        private NodeHandler nodeHandler = null;

        /// <summary>
        /// Indicates whether the node is being executed along the normal dialogue flow path, rather than via back/forward value propagation.
        /// </summary>
        [NonSerialized]
        private bool isExecutingFromDialogueFlow = false;

        /// <summary>
        /// Creates a new Show node.
        /// </summary>
        public ShowNode()
        {
            this.name = "SHOW";
            this.nodeType = NodeType.SHOW;
        }

        /// <inheritdoc/>
        public AsyncCompletionMode AsyncCompletionMode 
        {
            get { return AsyncCompletionMode.PROCEED_TO_NEXT; }
        }

        /// <inheritdoc/>
        public bool IsExecutingFromDialogueFlow
        {
            get { return isExecutingFromDialogueFlow; }
            set { this.isExecutingFromDialogueFlow = value; }
        }

        /// <inheritdoc>
        public void Reset()
        {
            this.isExecutionComplete = false;
        }

        /// <inheritdoc>
        public void Execute(NodeHandler nodeHandler, Dictionary<int, object> nodeValues, GameObject convoOwner = null)
        {
            //Send signal to listeners so that node execution gets handled by the active Dialogue Display
            this.nodeHandler = nodeHandler;
            nodeHandler.Listener.OnExecuteAsyncNode(this);
        }

        /// <inheritdoc/>
        public void ExecutionCompleted()
        {
            this.isExecutionComplete = true;
            nodeHandler.ExecutionCompleted(this);
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
        public void Interrupt()
        {
            throw new System.Exception("Cannot interrupt Show Node since it is not interruptable.");
        }

        /// <inheritdoc/>
        public bool IsExecutionComplete()
        {
            return this.isExecutionComplete;
        }

        /// <inheritdoc/>
        public bool IsSkippable()
        {
            return false;
        }
    }

    /// <summary>
    /// Defines the structure of an item within a Show node for showing a Dialogue Panel or a character portrayal.
    /// </summary>
    [Serializable]
    public class ShowNodeItem : ListItem
    {
        /// <summary>
        /// The mode to use, either DISPLAY or CHARACTER.
        /// </summary>
        [SerializeField]
        private ShowMode mode;

        /// <summary>
        /// The name of the character to show (if applicable).
        /// </summary>
        [SerializeField]
        private string characterName;

        /// <summary>
        /// The ID of the character's portrayal to show (if applicable).
        /// </summary>
        [SerializeField]
        private string imageId;

        /// <summary>
        /// Whether or not the display ID should be overridden for showing a character portrayal (if applicable).
        /// </summary>
        [SerializeField]
        private bool overrideDisplayId;

        /// <summary>
        /// The Display ID of the display to show (if applicable).
        /// </summary>
        [SerializeField]
        private string displayId;

        /// <summary>
        /// Whether or not the item is expanded in the node (used for editor UI).
        /// </summary>
        [SerializeField]
        private bool isExpanded;

        /// <summary>
        /// Creates a new ShowNodeItem.
        /// </summary>
        public ShowNodeItem() { }

        /// <summary>
        /// Creates a new ShowNodeItem.
        /// </summary>
        /// <param name="showMode">The ShowMode to use.</param>
        /// <param name="characterName">The name of the character to show.</param>
        /// <param name="imageId">The ID of the character portrayal to show.</param>
        /// <param name="overrideDisplayId">Whether the display ID is overridden.</param>
        /// <param name="displayId">The Display ID of the display to show.</param>
        public ShowNodeItem(ShowMode showMode, string characterName, string imageId, bool overrideDisplayId, string displayId)
        {
            this.mode = showMode;
            this.characterName = characterName;
            this.imageId = imageId;
            this.overrideDisplayId = overrideDisplayId;
            this.displayId = displayId;
        }

        /// <summary>
        /// Gets or sets the ID of the character's portrayal to show.
        /// </summary>
        public string CharacterName
        {
            get { return this.characterName; }
            set { this.characterName = value; }
        }

        /// <summary>
        /// Gets or sets the mode to use.
        /// </summary>
        public ShowMode ShowMode
        {
            get { return this.mode; }
            set { this.mode = value; }
        }

        /// <summary>
        /// Gets or sets the character portrayal image to use.
        /// </summary>
        public string ImageID
        {
            get { return this.imageId; }
            set { this.imageId = value; }
        }

        /// <summary>
        /// Gets or sets whether the display ID to use for character portrayals should override the default target ID configured in the character library.
        /// </summary>
        public bool OverrideDisplayID
        {
            get { return this.overrideDisplayId; }
            set { this.overrideDisplayId = value; }
        }

        /// <summary>
        /// Gets or sets the Display ID to show.
        /// </summary>
        public string DisplayID
        {
            get { return this.displayId; }
            set { this.displayId = value; }
        }

        /// <summary>
        /// Gets or sets whether the item is in an expanded state in the node editor UI.
        /// </summary>
        public bool IsExpanded
        {
            get { return this.isExpanded; }
            set { this.isExpanded = value; }
        }
    }

    /// <summary>
    /// Defines modes for showable items, such as characters or Dialogue Panels (displays).
    /// </summary>
    [Serializable]
    public enum ShowMode
    {
        CHARACTER, DISPLAY
    }
}
