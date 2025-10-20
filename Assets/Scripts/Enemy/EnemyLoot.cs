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

    public List<DropItem> Items { get; private set; }
    public float ExpDrop => _expDrop;

    [SerializeField] private float dropRadius = 1.0f; 
    [SerializeField] private float minDistanceBetweenDrops = 0.3f;
    [SerializeField] private int maxAttempts = 10; 
    [SerializeField] private LayerMask dropItemMask;
    
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

    public void DropItems()
    {
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
                
                var goldItem = item.ItemToDrop as GoldItemSO;
                if (goldItem != null)
                {
                    var pickup = drop.GetComponent<GoldPickup>();
                    if (pickup != null)
                    {
                        pickup.Amount = Random.Range(goldItem.MinAmount, goldItem.MaxAmount);
                    }
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
