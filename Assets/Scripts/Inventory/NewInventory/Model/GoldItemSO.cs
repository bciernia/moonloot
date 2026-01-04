using UnityEngine;

namespace Inventory.NewInventory.Model
{
    [CreateAssetMenu(menuName = "Item/Gold", fileName = "Gold_")]
    public class GoldItemSO : ItemSO
    {
        [field: SerializeField] public int Amount;
        [field: SerializeField] public MoneyType MoneyType;
    }
}