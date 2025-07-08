using UnityEngine;

namespace Inventory.NewInventory.Model
{
    [CreateAssetMenu]
    public class GoldItemSO : ItemSO
    {
        [field: SerializeField] public int Amount;
    }
}