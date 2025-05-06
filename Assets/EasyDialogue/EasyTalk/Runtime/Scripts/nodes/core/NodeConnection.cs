using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Core
{
    /// <summary>
    /// The NodeConnection class is used to define a connection point (an input or output) for a node.
    /// </summary>
    [Serializable]
    public class NodeConnection
    {
        /// <summary>
        /// The ID of the connection.
        /// </summary>
        [SerializeField]
        protected int id = NodeUtils.NextID();

        /// <summary>
        /// A List of IDs for connections which are attached to this connection.
        /// </summary>
        [SerializeField]
        protected List<int> attachedIds = new List<int>();

        /// <summary>
        /// The ID of the node this connection belongs to.
        /// </summary>
        [SerializeField]
        protected int ownerId = NodeUtils.NextID();

        /// <summary>
        /// The type of the connection.
        /// </summary>
        [SerializeField]
        protected InputOutputType connectionType = InputOutputType.ANY;

        /// <summary>
        /// Creates a new NodeConnection.
        /// </summary>
        public NodeConnection() { }

        /// <summary>
        /// Creates a new NodeConnection of the specified type.
        /// </summary>
        /// <param name="ownerId">The ID of the node which this connection belongs to.</param>
        /// <param name="connectionType">The type of the node connection.</param>
        public NodeConnection(int ownerId, InputOutputType connectionType)
        {
            this.ownerId = ownerId;
            this.connectionType = connectionType;
        }

        /// <summary>
        /// Gets or sets the ID of the connection.
        /// </summary>
        public int ID
        {
            get { return id; }
            set { this.id = value; }
        }

        /// <summary>
        /// Gets or sets the List of IDs for connections which are attached to this connection.
        /// </summary>
        public List<int> AttachedIDs
        {
            get { return attachedIds; }
            set { attachedIds = value; }
        }

        /// <summary>
        /// Gets or sets the ID of the node this connection belongs to.
        /// </summary>
        public int OwnerID
        {
            get { return ownerId; }
            set { this.ownerId = value; }
        }

        /// <summary>
        /// Gets or sets the connection type for the connection.
        /// </summary>
        public InputOutputType ConnectionType
        {
            get { return connectionType; }
            set { connectionType = value; }
        }

        /// <summary>
        /// Gets or sets the connection type for the connection from a string.
        /// </summary>
        public string ConnectionTypeString
        {
            get { return this.connectionType.ToString(); }
            set { Enum.TryParse<InputOutputType>(value, out connectionType); }
        }

        /// <summary>
        /// Returns true if the connection is a dialogue flow type connection, which includes the DIALGOUE_FLOW, DIALOGUE_TRUE_FLOW, and DIALOGUE_FALSE_FLOW types.
        /// </summary>
        /// <returns>Whether the connection is a dialogue flow type connection.</returns>
        public bool IsDialogueFlowConnection()
        {
            return this.ConnectionType == InputOutputType.DIALGOUE_FLOW || 
                this.ConnectionType == InputOutputType.DIALOGUE_TRUE_FLOW || 
                this.connectionType == InputOutputType.DIALOGUE_FALSE_FLOW;
        }
    }
}