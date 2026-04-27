using System;
using System.Collections.Generic;
using Inventory.NewInventory.Model;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyLoot : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float _expDrop;
    [SerializeField] private DropItem[] _dropItems;

    private List<DropItem> Items { get; set; }
    private List<DropItem> ItemsFromParent { get; set; }
    public float ExpDrop => _expDrop;

    [SerializeField] private float dropRadius = 1.0f; 
    [SerializeField] private float minDistanceBetweenDrops = 0.3f;
    [SerializeField] private int maxAttempts = 10; 
    [SerializeField] private LayerMask dropItemMask;
    
    private void Start()
    {
        Items = new List<DropItem>();
        AddDropItemsToList(_dropItems);
    }

    private void AddDropItemsToList(DropItem[] dropItems)
    {
        foreach (var item in dropItems)
        {
            var prob = Random.Range(0f, 100f);
            if (prob <= item.DropChance)
            {
                Items.Add(item);
            }
        }
    }
    
    public void AddDropItemsToListFromParent(DropItem[] dropItems)
    {
        ItemsFromParent = new List<DropItem>();
        foreach (var item in dropItems)
        {
            var prob = Random.Range(0f, 100f);
            if (prob <= item.DropChance)
            {
                ItemsFromParent.Add(item);
            }
        }
    }

    public void DropItems()
    {
        if (Items == null) return;
        
        foreach (var item in Items)
        {
            if (!item.ItemToDrop) continue;

            for (var i = 0; i < item.Quantity; i++)
            {
                var dropPosition = FindFreeDropPosition();
                var drop = Instantiate(item.ItemToDrop, transform.position, Quaternion.identity);

                var mover = drop.GetComponent<LootDropMover>();
                if (mover != null)
                {
                    mover.MoveToPosition(dropPosition, Random.Range(0.25f, 0.5f));
                }
                else
                {
                    drop.transform.position = dropPosition;
                }

                var itemSo = item.ItemToDrop.GetComponent<ItemInteraction>().GetInventoryItem();

                if (itemSo is GoldItemSO)
                {
                    
                }
            }
        }

        if (ItemsFromParent == null) return;
        
        foreach (var item in ItemsFromParent)
        {
            if (!item.ItemToDrop) continue;

            for (var i = 0; i < item.Quantity; i++)
            {
                var dropPosition = FindFreeDropPosition();
                var drop = Instantiate(item.ItemToDrop, transform.position, Quaternion.identity);

                var mover = drop.GetComponent<LootDropMover>();
                if (mover != null)
                {
                    mover.MoveToPosition(dropPosition, Random.Range(0.25f, 0.5f));
                }
                else
                {
                    drop.transform.position = dropPosition;
                }

                var itemSo = item.ItemToDrop.GetComponent<ItemInteraction>().GetInventoryItem();

                if (itemSo is GoldItemSO)
                {
                    
                }
            }
        }
    }
    
    private Vector3 FindFreeDropPosition()
    {
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var randomOffset = Random.insideUnitCircle * dropRadius;
            var testPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

            var hit = Physics2D.OverlapCircle(testPos, minDistanceBetweenDrops, dropItemMask);
            if (hit == null)
            {
                return testPos; 
            }
        }

        return transform.position;
    }

    public bool IsLootEmpty() => Items.Count == 0;
}

[Serializable]
public class DropItem
{
    [Header("Config")]
    public string Name;
    public GameObject ItemToDrop;
    public int Quantity;

    [Header("Drop Chance")]
    public float DropChance;
    public bool PickedItem { get; set; }
}
