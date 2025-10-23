using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Inventory.NewInventory.Model
{
    [CreateAssetMenu]
    public class BookItemSO : ItemSO
    {
        [field: SerializeField] 
        public string BookTitle { get; set; }
        
        [field: SerializeField] 
        public string Author { get; set; }
        
        [field: SerializeField]
        public List<string> PagesAndContent { get; set; }
    }
}