using EasyTalk.Controller;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Core
{
    /// <summary>
    /// An interface which defines methods for nodes which output one or more values.
    /// </summary>
    public interface FunctionalNode
    {
        /// <summary>
        /// Determines the value of this node and stores it in the provided Dictionary of node and connection IDs to values.
        /// </summary>
        /// <param name="nodeHandler">The node handler being used.</param>
        /// <param name="nodeValues">A mapping between node or connection IDs and the values attributed to them.</param>
        /// <param name="convoOwner">The GameObject which on which the dialogue logic is currently running.</param>
        /// <returns>Returns true if the value was determined and stored successfully. IF the value could not be determined (perhaps 
        /// due to needing to await feedback or other processing), this method returns false.</returns>
        public bool DetermineAndStoreValue(NodeHandler nodeHandler, Dictionary<int, object> nodeValues, GameObject convoOwner = null);

        /// <summary>
        /// Returns whether the node is dependent on values coming into input connections.
        /// </summary>
        /// <returns>Whether the node has dependencies.</returns>
        public bool HasDependencies();

        /// <summary>
        /// Returns a List of IDs for output connections that this node is dependent on in order to evaluate itself and determine the value to store.
        /// </summary>
        /// <returns>The List of output connections that this node is dependent on.</returns>
        public List<int> GetDependencyOutputIDs();
    }
}
