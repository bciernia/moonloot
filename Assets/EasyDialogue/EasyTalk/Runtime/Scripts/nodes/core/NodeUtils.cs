using System.Collections.Generic;

namespace EasyTalk.Nodes.Core
{
    /// <summary>
    /// A utility class for performing common functions on dialogue nodes, such as creating and handling IDs for nodes and their components.
    /// </summary>
    public class NodeUtils
    {
        /// <summary>
        /// The current ID value used for nodes and node components.
        /// </summary>
        private static int currentId = 1;

        /// <summary>
        /// Increments the current ID and returns the new ID.
        /// </summary>
        /// <returns>The next ID.</returns>
        public static int NextID()
        {
            currentId++;
            return currentId;
        }

        /// <summary>
        /// Returns the current ID.
        /// </summary>
        /// <returns>The current ID.</returns>
        public static int CurrentID()
        {
            return currentId;
        }

        /// <summary>
        /// Sets the NodeUtils class current ID to the specified ID value.
        /// </summary>
        /// <param name="id">The ID value to use.</param>
        public static void SetCurrentID(int id)
        {
            currentId = id;
        }

        /// <summary>
        /// Generates a new set of IDs for each node in the provided List and all of its connections.
        /// </summary>
        /// <param name="nodes">The List of Nodes to generate new IDs for.</param>
        public static void GenerateNewIDs(List<Node> nodes)
        {
            Dictionary<int, int> idDictionary = new Dictionary<int, int>();
            NodeUtils.NextID();

            //Establish new IDs for each input/output connection
            foreach (Node node in nodes)
            {
                //Reset the ID of the node.
                node.ID = NodeUtils.NextID();

                foreach (NodeConnection connection in node.Inputs)
                {
                    if (!idDictionary.ContainsKey(connection.ID))
                    {
                        int newId = NodeUtils.NextID();
                        idDictionary.Add(connection.ID, newId);
                        connection.ID = newId;
                    }
                }

                foreach (NodeConnection connection in node.Outputs)
                {
                    if (!idDictionary.ContainsKey(connection.ID))
                    {
                        int newId = NodeUtils.NextID();
                        idDictionary.Add(connection.ID, newId);
                        connection.ID = newId;
                    }
                }
            }

            //For each input/output, delete any attached IDs which have not been detected as available inputs/outputs in the current graph
            //Also modify each ID if it has been updated to match the new ID
            foreach (Node node in nodes)
            {
                foreach (NodeConnection connection in node.Inputs)
                {
                    for (int i = 0; i < connection.AttachedIDs.Count; i++)
                    {
                        int attachedId = connection.AttachedIDs[i];

                        if (!idDictionary.ContainsKey(attachedId))
                        {
                            //Remove the ID
                            connection.AttachedIDs.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            //Update the ID
                            connection.AttachedIDs[i] = idDictionary[attachedId];
                        }
                    }
                }

                foreach (NodeConnection connection in node.Outputs)
                {
                    for (int i = 0; i < connection.AttachedIDs.Count; i++)
                    {
                        int attachedId = connection.AttachedIDs[i];

                        if (!idDictionary.ContainsKey(attachedId))
                        {
                            //Remove the ID
                            connection.AttachedIDs.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            //Update the ID
                            connection.AttachedIDs[i] = idDictionary[attachedId];
                        }
                    }
                }
            }
        }
    }
}