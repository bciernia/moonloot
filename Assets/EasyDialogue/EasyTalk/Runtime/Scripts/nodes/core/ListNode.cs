using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Nodes.Core
{
    /// <summary>
    /// A node which allows for a flexible number of items to be contained within it.
    /// </summary>
    [Serializable]
    public class ListNode : Node
    {
        /// <summary>
        /// The items in the node.
        /// </summary>
        [SerializeReference]
        protected List<ListItem> items = new List<ListItem>();

        /// <summary>
        /// Creates a new ListNode.
        /// </summary>
        public ListNode()
        {
            this.name = "LIST";
            this.nodeType = NodeType.LIST;
        }

        /// <summary>
        /// Gets or sets the List of items in this node.
        /// </summary>
        [SerializeField]
        public List<ListItem> Items
        {
            get { return items; }
            set { items = value; }
        }

        /// <summary>
        /// Adds the specified item to this node's List of items.
        /// </summary>
        /// <param name="item">The ListItem to add.</param>
        public virtual void AddItem(ListItem item)
        {
            this.items.Add(item);
        }

        /// <summary>
        /// Removes the specified item from this node's List of items.
        /// </summary>
        /// <param name="item">The ListItem to remove.</param>
        public void RemoveItem(ListItem item)
        {
            this.items.Remove(item);
        }

        /// <summary>
        /// Removes the item at the specified index from this node.
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
        public void RemoveItem(int index)
        {
            if (items.Count > index)
            {
                items.RemoveAt(index);
            }
        }
        
        /// <summary>
        /// Removes all items from the node.
        /// </summary>
        public void RemoveAllItems()
        {
            this.items.Clear();
        }
    }

    /// <summary>
    /// The abstract base class for all item types which contained within dynamic list type nodes.
    /// </summary>
    [Serializable]
    public abstract class ListItem
    {
        public ListItem() { }
    }
}