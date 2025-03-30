using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item")]
public class InventoryItem : ScriptableObject
{
    [Header("Config")]
    public string Id = Guid.NewGuid().ToString();
    public string Name;
    public Sprite Icon;
    [TextArea] public string Description;

    [Header("Info")]
    public ItemTypes ItemType;
    public bool IsConsumable;
    public bool IsStackable;
    public int MaxStack;

    [HideInInspector] public int Quantity;

    public InventoryItem CopyItem()
    {
        var instance = Instantiate(this);
        return instance;
    }

    public virtual bool UseItem()
    {
        return true;
    }

    public virtual void EquipItem()
    {
        
    }

    public virtual void RemoveItem()
    {
        
    }
}
