using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Core
{
    /// <summary>
    /// A base implementation of a dialogue node, from which all other nodes are derived.
    /// </summary>
    [Serializable]
    public class Node
    {
        /// <summary>
        /// The ID of the node.
        /// </summary>
        [SerializeField]
        protected int nodeId = NodeUtils.NextID();

        /// <summary>
        /// The name of the node.
        /// </summary>
        [SerializeField]
        protected string name;

        /// <summary>
        /// The type of the node.
        /// </summary>
        [SerializeField]
        protected NodeType nodeType;

        /// <summary>
        /// A List of input connections of the node.
        /// </summary>
        [SerializeField]
        protected List<NodeConnection> inputs = new List<NodeConnection>();

        /// <summary>
        /// A List of output connections of the node.
        /// </summary>
        [SerializeField]
        protected List<NodeConnection> outputs = new List<NodeConnection>();

        /// <summary>
        /// The width of the node.
        /// </summary>
        [SerializeField]
        private float width = 400.0f;

        /// <summary>
        /// The height of the node.
        /// </summary>
        [SerializeField]
        private float height = 300.0f;

        /// <summary>
        /// The X position of the node.
        /// </summary>
        [SerializeField]
        private float xPosition = 0.0f;

        /// <summary>
        /// The Y position of the node.
        /// </summary>
        [SerializeField]
        private float yPosition = 0.0f;

        /// <summary>
        /// Gets or sets the node ID.
        /// </summary>
        public int ID
        {
            get { return nodeId; }
            set { this.nodeId = value; }
        }

        /// <summary>
        /// Gets or sets the node type string.
        /// </summary>
        public string NodeTypeString
        {
            get { return this.nodeType.ToString(); }
            set
            {
                Enum.TryParse<NodeType>(value, out nodeType);
            }
        }

        /// <summary>
        /// Gets or sets the X position of the node.
        /// </summary>
        public float XPosition
        {
            get { return xPosition; }
            set { this.xPosition = value; }
        }

        /// <summary>
        /// Gets or sets the Y position of the node.
        /// </summary>
        public float YPosition
        {
            get { return yPosition; }
            set { this.yPosition = value; }
        }

        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>
        /// Gets or sets the width of the node.
        /// </summary>
        public float Width
        {
            get { return this.width; }
            set { this.width = value; }
        }

        /// <summary>
        /// Gets or sets the height of the node.
        /// </summary>
        public float Height
        {
            get { return this.height; }
            set { this.height = value; }
        }

        /// <summary>
        /// Gets or sets the node type.
        /// </summary>
        public NodeType NodeType
        {
            get { return this.nodeType; }
            set { this.nodeType = value; }
        }

        /// <summary>
        /// Gets or sets the List of input connections of the node.
        /// </summary>
        public List<NodeConnection> Inputs
        {
            get { return this.inputs; }
            set { this.inputs = value; }
        }

        /// <summary>
        /// Gets or sets the List of output connections of the node.
        /// </summary>
        public List<NodeConnection> Outputs
        {
            get { return this.outputs; }
            set { this.outputs = value; }
        }

        /// <summary>
        /// Adds the provided input connection to the node's inputs.
        /// </summary>
        /// <param name="input">The input connection to add.</param>
        public void AddInput(NodeConnection input)
        {
            this.inputs.Add(input);
        }

        /// <summary>
        /// Adds the provided output connection to the node's outputs.
        /// </summary>
        /// <param name="output">The output connection to add.</param>
        public void AddOutput(NodeConnection output)
        {
            this.outputs.Add(output);
        }

        /// <summary>
        /// Adds a new input connection of the specified type to the node's inputs.
        /// </summary>
        /// <param name="connectionType">The type of input to add.</param>
        /// <returns>The newly created input connection.</returns>
        public NodeConnection AddInput(InputOutputType connectionType)
        {
            NodeConnection conn = new NodeConnection(this.ID, connectionType);
            inputs.Add(conn);
            return conn;
        }

        /// <summary>
        /// Adds a new output connection of the specified type to the node's outputs.
        /// </summary>
        /// <param name="connectionType">The type of output to add.</param>
        /// <returns>The newly created output connection.</returns>
        public NodeConnection AddOutput(InputOutputType connectionType)
        {
            NodeConnection conn = new NodeConnection(this.ID, connectionType);
            outputs.Add(conn);
            return conn;
        }

        /// <summary>
        /// Finds the first output which matches the specified output type.
        /// </summary>
        /// <param name="outputType">The output type to retrieve an output for.</param>
        /// <returns>The first output connection which matches the specified output type, or null if none exists.</returns>
        public NodeConnection FindOutputOfType(InputOutputType outputType)
        {
            foreach(NodeConnection output in Outputs)
            {
                if(output.ConnectionType == outputType)
                {
                    return output;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the first input which matches the specified input type.
        /// </summary>
        /// <param name="inputType">The input type to retrieve an input for.</param>
        /// <returns>The first input connection which matches the specified input type, or null if none exists.</returns>
        public NodeConnection FindInputOfType(InputOutputType inputType)
        {
            foreach(NodeConnection input in Inputs)
            {
                if(input.ConnectionType == inputType)
                {
                    return input;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns true if this node has inputs which are connected to other node outputs.
        /// </summary>
        /// <returns>Whether this node has inputs connected to other nodes.</returns>
        public bool HasConnectedInputs()
        {
            for(int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i].AttachedIDs.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if this node has outputs which are connected to other node inputs.
        /// </summary>
        /// <returns>Whether this node has outputs connected to the other nodes.</returns>
        public bool HasConnectedOutputs()
        {
            for(int i = 0; i < outputs.Count; i++)
            {
                if (outputs[i].AttachedIDs.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds and returns a List of all inputs which are dialogue flow inputs.
        /// </summary>
        /// <returns>All inputs which are dialogue flow inputs.</returns>
        protected List<NodeConnection> FindFlowInputs()
        {
            List<NodeConnection> conns = new List<NodeConnection>();

            foreach(NodeConnection conn in Inputs)
            {
                if(conn.IsDialogueFlowConnection())
                {
                    conns.Add(conn);
                }
            }

            return conns;
        }

        /// <summary>
        /// Finds and returns a List of all outputs which are dialogue flow outputs.
        /// </summary>
        /// <returns>All outputs which are dialogue flow outputs.</returns>
        protected List<NodeConnection> FindFlowOutputs()
        {
            List<NodeConnection> conns = new List<NodeConnection>();

            foreach(NodeConnection conn in Outputs)
            {
                if(conn.IsDialogueFlowConnection())
                {
                    conns.Add(conn);
                }
            }

            return conns;
        }

        /// <summary>
        /// Finds and returns a List of all output IDs which are connected to this node's value (non-dialogue-flow) inputs.
        /// </summary>
        /// <returns>The IDs of output connections which the node is dependent upon.</returns>
        public List<int> FindDependencyOutputIDs()
        {
            List<int> dependencyOutputIds = new List<int>();

            foreach (NodeConnection conn in Inputs)
            {
                if (!conn.IsDialogueFlowConnection() && conn.AttachedIDs.Count > 0)
                {
                    dependencyOutputIds.Add(conn.AttachedIDs[0]);
                }
            }

            return dependencyOutputIds;
        }

        /// <summary>
        /// Returns a JSON string representation of the node.
        /// </summary>
        /// <returns>A JSON representation of the node.</returns>
        public string GetJSON()
        {
            return JsonUtility.ToJson(this);
        }

        /// <summary>
        /// Deserializes a Node from the specified JSON string.
        /// </summary>
        /// <param name="json">A JSON string representation for a Node.</param>
        /// <returns>A Node deserialized from the provided JSON string.</returns>
        public static Node Deserialize(string json)
        {
            Node node = JsonUtility.FromJson<Node>(json);
            return node;
        }
    }
}