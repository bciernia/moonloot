namespace EasyTalk.Nodes.Core
{
    /// <summary>
    /// An interface which defines methods implemented by dialogue flow nodes, that is, nodes which contibute to the overall flow path of a dialogue.
    /// </summary>
    public interface DialogueFlowNode
    {
        /// <summary>
        /// Returns the dialogue flow input for the node.
        /// </summary>
        /// <returns>The dialogue flow input for the node.</returns>
        public NodeConnection GetFlowInput();

        /// <summary>
        /// Returns the dialogue flow output for the node.
        /// </summary>
        /// <returns>The dialogue flow output for the node.</returns>
        public NodeConnection GetFlowOutput();
    }
}
