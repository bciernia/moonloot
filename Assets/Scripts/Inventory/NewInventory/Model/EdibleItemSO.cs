using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu]
public class EdibleItemSO : ItemSO, IDestroyableItem, IItemAction
{
    public string ActionName => "Consume";

    [SerializeField] private float HealthValue;

    [field: SerializeField] public AudioClip actionSfx { get; private set; }

    public override string GetStatsDescription() => $"Health: {HealthValue} \n";
    
    public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
    {
        if (GameManager.Instance.Player.PlayerHealth.CanRestoreHealth())
        {
            GameManager.Instance.Player.PlayerHealth.RestoreHealth(HealthValue);
            return true;
        }

        return false;
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

