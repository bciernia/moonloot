using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Core
{
    /// <summary>
    /// An interface which defines methods for conditional nodes, that is, nodes which output a boolean value based on a condition, and choose an output path to
    /// continue the dialogue flow based on that boolean value.
    /// </summary>
    public interface ConditionalNode
    {
        /// <summary>
        /// Returns the dialogue flow output connection for when the result of the node's condition is true.
        /// </summary>
        /// <returns>The dialogue flow output connection for when the result of the node's condition is true.</returns>
        public NodeConnection GetTrueOutput();

        /// <summary>
        /// Returns the dialogue flow output connection for when the result of the node's condition is false.
        /// </summary>
        /// <returns>The dialogue flow output connection for when the result of the node's condition is false.</returns>
        public NodeConnection GetFalseOutput();

        /// <summary>
        /// Returns the dialogue flow output which corresponds to the specified boolean value.
        /// </summary>
        /// <param name="value">The value to retrieve a dialogue flow output for.</param>
        /// <returns>The dialogue flow output which corresponds to the specified boolean value.</returns>
        public NodeConnection GetOutput(bool value);
    }
}
