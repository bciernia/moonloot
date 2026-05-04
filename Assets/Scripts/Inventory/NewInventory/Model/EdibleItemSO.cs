using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Edible", fileName = "Edible_")]
public class EdibleItemSO : ItemSO, IDestroyableItem, IItemAction
{
    public string ActionName => "Consume";

    [SerializeField] private float HealthValue;
    [SerializeField] private float ManaValue;

    [field: SerializeField] public AudioClip actionSfx { get; private set; }

    public override string GetStatsDescription() => $"Health: {HealthValue} \nMana: {ManaValue} \n";
    
    public bool PerformAction(GameObject character, InventoryItem inventoryItem, bool isUsingItem = false, string slotName = "")
    {
        var restoredStats = false;

        if (!isUsingItem)
        {
            var slotIndex = slotName == "QuickSlot1" ? 5 : 6;
            
            QuickItemManager.Instance.SetQuickItem(this, inventoryItem.itemState, inventoryItem.quantity, slotIndex);
            return true;
        }
        
        if (GameManager.Instance.Player.PlayerHealth.CanRestoreHealth())
        {
            GameManager.Instance.Player.PlayerHealth.RestoreHealth(HealthValue);
            restoredStats = true;
        }
        
        if (GameManager.Instance.Player.PlayerHealth.CanRestoreMana())
        {
            GameManager.Instance.Player.PlayerHealth.RestoreMana(ManaValue);
            restoredStats = true;
        }

        return restoredStats;
    }
    
    public void Unequip(GameObject character)
    {
        var armorSystem = character.transform.parent.GetComponentInChildren<QuickItemManager>();
        armorSystem.SetQuickItem(null, null, 0, 5);
    }
}

public interface IDestroyableItem
{
    
}

public interface IItemAction
{
    public string ActionName { get; }

    public AudioClip actionSfx { get; }

    bool PerformAction(GameObject character, InventoryItem inventoryItem, bool isUsingItem = false, string slotName = "");
}

