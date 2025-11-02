using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Inventory.NewInventory.Model
{
    public abstract class ReadableItemSO : ItemSO
    {
        [field: SerializeField]
        public string Title { get; set; }
        
        [field: SerializeField] 
        public string Author { get; set; }
    }
}