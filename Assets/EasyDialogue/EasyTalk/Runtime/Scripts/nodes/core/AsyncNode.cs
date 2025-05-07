using EasyTalk.Controller;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Core
{
    /// <summary>
    /// Defines the functions which are implemented by asynchronous nodes, which are nodes that require external processing prior to dialogue flow continuation.
    /// </summary>
    public interface AsyncNode
    {
        /// <summary>
        /// Hands execution over to the NodeHandler, which should send a signal for the node to be handled externally.
        /// </summary>
        /// <param name="nodeHandler">The NodeHandler processing the node.</param>
        /// <param name="nodeValues">A mapping of input/output and node IDs to values attributed to those IDs.</param>
        /// <param name="convoOwner">The current conversation owner.</param>
        public void Execute(NodeHandler nodeHandler, Dictionary<int, object> nodeValues, GameObject convoOwner = null);

        /// <summary>
        /// Must be called whenever a node is finished being processed. The node should typically let the NodeHandler know that it has finished execution in this method.
        /// </summary>
        public void ExecutionCompleted();

        /// <summary>
        /// Returns whether or not the node has finished being processed/used.
        /// </summary>
        /// <returns></returns>
        public bool IsExecutionComplete();

        /// <summary>
        /// Returns whether or not the node can be skipped before processing has been completed.
        /// </summary>
        /// <returns></returns>
        public bool IsSkippable();

        /// <summary>
        /// Interrupts the execution of the node, if it is skippable.
        /// </summary>
        public void Interrupt();

        /// <summary>
        /// Returns the AsyncCompletion mode of the node.
        /// </summary>
        public AsyncCompletionMode AsyncCompletionMode { get; }

        /// <summary>
        /// Returns whether or not the node is being processed along the normal dialogue flow path, rather than back/forward value propagation.
        /// </summary>
        public bool IsExecutingFromDialogueFlow { get; set; }

        /// <summary>
        /// Used to reset an async node after it has completed execution.
        /// </summary>
        public void Reset();
    }

    /// <summary>
    /// Defines various asynchronous completion modes which indicate how the async node is to be processed.
    /// </summary>
    public enum AsyncCompletionMode
    {
        PROCEED_TO_NEXT, //Go to next node without processing the current node again or propagating values
        REPROCESS_CURRENT, //Reprocess the current node, propagating values and proceeding to the next node when complete
        PROPAGATE_ONLY //Propagate the values of the current node forward to any value-attached nodes
    }
}
