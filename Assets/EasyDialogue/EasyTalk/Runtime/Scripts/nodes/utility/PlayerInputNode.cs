using System.Collections.Generic;
using UnityEngine;
using EasyTalk.Nodes.Core;
using EasyTalk.Controller;
using System;

namespace EasyTalk.Nodes.Utility
{
    /// <summary>
    /// A node for getting text input from the player.
    /// </summary>
    public class PlayerInputNode : Node, DialogueFlowNode, FunctionalNode, AsyncNode
    {
        /// <summary>
        /// The hint text to show to the player.
        /// </summary>
        [SerializeField]
        private string hintText = "Enter Response...";

        /// <summary>
        /// The text obtained from the player.
        /// </summary>
        private string inputText = "";

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
        /// Creates a new PlayerInput node.
        /// </summary>
        public PlayerInputNode()
        {
            this.name = "PLAYER INPUT";
            this.nodeType = NodeType.PLAYER_INPUT;
        }

        /// <summary>
        /// Gets or sets the hint text for the node.
        /// </summary>
        public string HintText
        {
            get { return this.hintText; }
            set { this.hintText = value; }
        }

        /// <summary>
        /// Gets or sets the text which was input by the player.
        /// </summary>
        public string InputText 
        { 
            get { return this.inputText; } 
            set { this.inputText = value; }
        }

        /// <inheritdoc/>
        public bool DetermineAndStoreValue(NodeHandler nodeHandler, Dictionary<int, object> nodeValues, GameObject convoOwner = null)
        {
            //Tell the node handler to display and retrieve input from the player, then set the entered value.
            if (!IsExecutionComplete())
            {
                return false;
            }
            else
            {
                NodeConnection valueOutput = FindOutputOfType(InputOutputType.STRING);
                nodeValues.TryAdd(valueOutput.ID, inputText);
                nodeValues.TryAdd(this.ID, inputText);
                this.inputText = "";
                return true;
            }
        }

        /// <inheritdoc/>
        public List<int> GetDependencyOutputIDs()
        {
            return FindDependencyOutputIDs();
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
        public bool HasDependencies()
        {
            return false;
        }

        /// <inheritdoc>
        public void Reset()
        {
            this.isExecutionComplete = false;
        }

        /// <inheritdoc/>
        public void Execute(NodeHandler nodeHandler, Dictionary<int, object> nodeValues, GameObject convoOwner = null)
        {
            //Send signal to listeners so that node execution gets handled by UI interaction
            this.nodeHandler = nodeHandler;
            nodeHandler.Listener.OnExecuteAsyncNode(this);
        }

        /// <inheritdoc/>
        public void ExecutionCompleted()
        {
            this.isExecutionComplete = true;

            //Tell the node handler to continue
            nodeHandler.ExecutionCompleted(this);
        }

        /// <inheritdoc/>
        public bool IsExecutionComplete()
        {
            return isExecutionComplete;
        }

        /// <inheritdoc/>
        public bool IsSkippable()
        {
            return false;
        }

        /// <inheritdoc/>
        public void Interrupt()
        {
            throw new System.Exception("Cannot interrupt PlayerInputNode since it is not interruptable.");
        }

        /// <inheritdoc/>
        public AsyncCompletionMode AsyncCompletionMode
        {
            get { return AsyncCompletionMode.REPROCESS_CURRENT; }
        }

        /// <inheritdoc/>
        public bool IsExecutingFromDialogueFlow
        {
            get { return isExecutingFromDialogueFlow; }
            set { this.isExecutingFromDialogueFlow = value; }
        }
    }
}
