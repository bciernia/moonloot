using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Pools/Obligatory Item Pool")]
public class ObligatoryItemPoolSO : ScriptableObject
{
    [SerializeField]
    private List<ObligatoryItemSpawnData> _items = new();

    public List<ObligatoryItemSpawnData> GetAvailableItems(int currentNight)
    {
        return _items
            .Where(item =>
                currentNight >= item.MinNight &&
                currentNight <= item.MaxNight)
            .ToList();
    }
}

[System.Serializable]
public class ObligatoryItemSpawnData
{
    public ItemSO Item;

    [Min(1)]
    public int Amount = 1;

    [Min(1)]
    public int MinNight = 1;

    [Min(1)]
    public int MaxNight = 9999;
}