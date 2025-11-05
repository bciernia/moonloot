using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Item/Edible", fileName = "Edible_")]
public class EdibleItemSO : ItemSO, IDestroyableItem, IItemAction
{
    public string ActionName => "Consume";

    [SerializeField] private float HealthValue;
    [SerializeField] private float ManaValue;

    [field: SerializeField] public AudioClip actionSfx { get; private set; }

    public override string GetStatsDescription() => $"Health: {HealthValue} \nMana: {ManaValue} \n";
    
    public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
    {
        var restoredStats = false;
        
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
}

public interface IDestroyableItem
{
    
}

public interface IItemAction
{
    public string ActionName { get; }

    public AudioClip actionSfx { get; }

    bool PerformAction(GameObject character, List<ItemParameter> itemParameters);
}

