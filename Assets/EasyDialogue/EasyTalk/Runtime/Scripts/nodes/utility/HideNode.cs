using EasyTalk.Controller;
using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Utility
{
    /// <summary>
    /// A node which is used to hide  Dialogue Panels and Character portrayals during dialogue playback.
    /// </summary>
    public class HideNode : ListNode, DialogueFlowNode, AsyncNode
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
        private bool isExecutingFromDialogueFlow = true;

        /// <summary>
        /// Creates a new Hide node.
        /// </summary>
        public HideNode()
        {
            this.name = "HIDE";
            this.nodeType = NodeType.HIDE;
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

        /// <inheritdoc/>
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
            throw new System.Exception("Cannot interrupt Hide Node since it is not interruptable.");
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
    /// Defines the structure of an item within a Hide node for showing a Dialogue Panel or a character portrayal.
    /// </summary>
    [Serializable]
    public class HideNodeItem : ListItem
    {
        /// <summary>
        /// The mode to use, either DISPLAY or CHARACTER.
        /// </summary>
        [SerializeField]
        private HideMode mode;

        /// <summary>
        /// The name of the character to hide (if applicable).
        /// </summary>
        [SerializeField]
        private string characterName;

        /// <summary>
        /// The Display ID of the display to hide (if applicable).
        /// </summary>
        [SerializeField]
        private string displayId;

        /// <summary>
        /// Whether or not the item is expanded in the node (used for editor UI).
        /// </summary>
        [SerializeField]
        private bool isExpanded;

        /// <summary>
        /// Creates a new HideNodeItem.
        /// </summary>
        public HideNodeItem() { }

        /// <summary>
        /// Creates a new HideNodeItem.
        /// </summary>
        /// <param name="hideMode">The HideMode to use.</param>
        /// <param name="characterName">The name of the character to hide.</param>
        /// <param name="displayId">The name of the display (DIalogue Panel) to hide.</param>
        public HideNodeItem(HideMode hideMode, string characterName, string displayId)
        {
            this.mode = hideMode;
            this.characterName = characterName;
            this.displayId = displayId;
        }

        /// <summary>
        /// Gets or sets the name of the character to hide.
        /// </summary>
        public string CharacterName
        {
            get { return this.characterName; }
            set { this.characterName = value; }
        }

        /// <summary>
        /// Gets or sets the HideMode.
        /// </summary>
        public HideMode HideMode
        {
            get { return this.mode; }
            set { this.mode = value; }
        }

        /// <summary>
        /// Gets or sets the Display ID of the display (Dialogue Panel) to hide.
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
    /// Defines modes for hideable items, such as characters or Dialogue Panels (displays).
    /// </summary>
    [Serializable]
    public enum HideMode
    {
        CHARACTER, DISPLAY
    }
}
