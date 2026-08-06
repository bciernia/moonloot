using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemSO : ScriptableObject
{
    [field: SerializeField] 
    public bool IsStackable { get; set; }
    public int Id => GetInstanceID();

    [field: SerializeField] 
    public int MaxStackSize { get; set; } = 1;
    
    [field: SerializeField] 
    public string Name { get; set; }
    
    [field: TextArea]
    [field: SerializeField] 
    public string Description { get; set; }
    
    [field: SerializeField] 
    public Sprite Image { get; set; }
    
    [field: SerializeField]
    public List<ItemParameter> DefaultParametersList { get; set; }
    
    [field: SerializeField]
    public GameObject ItemToDrop { get; set; }

    [field: SerializeField]
    public ItemType ItemType { get; private set; } = ItemType.Junk;

    [field: SerializeField] public int MinNight { get; set; } = 1;
    [field: SerializeField] public int MaxNight { get; set; } = 9999;
    [field: SerializeField] public int Weight { get; set; } = 10;
    [field: SerializeField] public ItemRarity Rarity { get; set; }
    [field: SerializeField] public GameObject Prefab { get; set; }

    public virtual string GetStatsDescription() => "";

    public int BuyPrice = 0;
    public int SellPrice = 0;
}

[Serializable]
public struct ItemParameter : IEquatable<ItemParameter>
{
    public ItemParameterSO itemParameter;
    public float value;

    public bool Equals(ItemParameter other)
    {
        return other.itemParameter == itemParameter;
    }
}