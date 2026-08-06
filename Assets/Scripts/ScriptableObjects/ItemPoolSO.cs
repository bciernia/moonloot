using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Pools/Item Pool")]
public class ItemPoolSO : ScriptableObject
{
    [SerializeField]
    private List<ItemSpawnData> _items = new();

    public GameObject GetRandomItem(int currentNight = 1)
    {
        var availableItems =
            _items.Where(item =>
                    currentNight >= item.Item.MinNight &&
                    currentNight <= item.Item.MaxNight)
                .ToList();

        if (availableItems.Count == 0)
        {
            Debug.LogWarning(
                $"ItemPool '{name}' has no available items for night {currentNight}.");

            return null;
        }

        var totalWeight = availableItems.Sum(item =>
            GetSpawnWeight(item, currentNight));

        var randomWeight =
            RNGManager.Instance.GetRandomFloat(0f, totalWeight);

        foreach (var item in availableItems)
        {
            randomWeight -= GetSpawnWeight(item, currentNight);

            if (randomWeight <= 0f)
                return item.Item.ItemToDrop;
        }

        return availableItems[^1].Item.ItemToDrop;
    }
    
    private float GetSpawnWeight(
        ItemSpawnData item,
        int currentNight)
    {
        return item.Weight *
               GetRarityMultiplier(item.Item.Rarity) *
               GetNightMultiplier(
                   item.Item.Rarity,
                   currentNight);
    }
    
    private float GetRarityMultiplier(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => 1f,
            ItemRarity.Uncommon => 0.8f,
            ItemRarity.Rare => 0.6f,
            ItemRarity.Epic => 0.4f,
            ItemRarity.Legendary => 0.2f,
            _ => 1f
        };
    }
    
    private float GetNightMultiplier(
        ItemRarity rarity,
        int night)
    {
        var progress =
            Mathf.Clamp01((night - 1) / 30f);

        return rarity switch
        {
            ItemRarity.Common =>
                1f,

            ItemRarity.Uncommon =>
                Mathf.Lerp(1f, 1.3f, progress),

            ItemRarity.Rare =>
                Mathf.Lerp(1f, 2f, progress),

            ItemRarity.Epic =>
                Mathf.Lerp(1f, 3f, progress),

            ItemRarity.Legendary =>
                Mathf.Lerp(1f, 5f, progress),

            _ => 1f
        };
    }
}

[System.Serializable]
public class ItemSpawnData
{
    public ItemSO Item;

    [Min(1)]
    public int Weight = 10;
}