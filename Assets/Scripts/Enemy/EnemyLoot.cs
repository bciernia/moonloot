using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyLoot : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float _expDrop;
    [SerializeField] private DropItem[] _dropItems;

    public List<DropItem> Items { get; private set; }
    public float ExpDrop => _expDrop;

    private void Start()
    {
        LoadDropItems();
    }

    private void LoadDropItems()
    {
        Items = new List<DropItem>();
        foreach (var item in _dropItems)
        {
            var prob = Random.Range(0f, 100f);
            if (prob <= item.DropChance)
            {
                Items.Add(item);
            }
        }
    }

    public bool IsLootEmpty() => Items.Count == 0;
}

[Serializable]
public class DropItem
{
    [Header("Config")]
    public string Name;
    public InventoryItem Item;
    public int Quantity;

    [Header("Drop Chance")]
    public float DropChance;
    public bool PickedItem { get; set; }
}
