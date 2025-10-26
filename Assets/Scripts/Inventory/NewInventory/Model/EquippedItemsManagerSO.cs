using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquippedItems_")]
public class EquippedItemsManagerSO : ScriptableObject
{
    public List<InventoryItem> test;
    public InventoryItem _weapon;
    public InventoryItem _armor;
}