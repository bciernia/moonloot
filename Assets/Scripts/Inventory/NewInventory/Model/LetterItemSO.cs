using UnityEngine;

namespace Inventory.NewInventory.Model
{
    [CreateAssetMenu(menuName = "Item/Letter", fileName = "Letter_")]
    public class LetterItemSO : ReadableItemSO
    {
        [field: SerializeField]
        [TextArea]
        public string Content { get; set; }

        public string GetLetterContent() => $"{Title}\n\n{Content}\n\n{Author}\n\n\n";
    }
}