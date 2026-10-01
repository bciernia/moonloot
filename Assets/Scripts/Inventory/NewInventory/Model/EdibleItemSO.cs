using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Edible", fileName = "Edible_")]
public class EdibleItemSO : ItemSO, IDestroyableItem, IItemAction
{
    public string ActionName => "Consume";

    [SerializeField] private float HealthValue;
    [SerializeField] private float ManaValue;
    [SerializeField] private List<TimedItemStatEffect> _timedEffects = new();

    [field: SerializeField] public AudioClip actionSfx { get; private set; }

    public override string GetStatsDescription()
    {
        var description = $"Health: {HealthValue} \nMana: {ManaValue} \n";

        foreach (var effect in _timedEffects)
        {
            if (effect == null || effect.Duration <= 0f)
                continue;

            var value = IsPercentageBonus(effect.Type)
                ? $"{effect.Value:+0.#;-0.#;0}%"
                : $"{effect.Value:+0.#;-0.#;0}";

            description += $"{effect.Type}: {value} ({effect.Duration:0.#}s)\n";
        }

        return description;
    }
    
    public bool PerformAction(GameObject character, InventoryItem inventoryItem, bool isUsingItem = false, string slotName = "")
    {
        var restoredStats = false;

        if (!isUsingItem)
        {
            var slotIndex = slotName == "QuickSlot1" ? 5 : 6;
            
            QuickItemManager.Instance.SetQuickItem(this, inventoryItem.itemState, inventoryItem.quantity, slotIndex);
            return true;
        }
        
        if (GameManager.Instance.Player.PlayerHealth.CanRestoreHealth() && HealthValue > 0)
        {
            GameManager.Instance.Player.PlayerHealth.RestoreHealth(HealthValue);
            restoredStats = true;
        }
        
        if (GameManager.Instance.Player.PlayerHealth.CanRestoreMana() && ManaValue > 0)
        {
            GameManager.Instance.Player.PlayerHealth.RestoreMana(ManaValue);
            restoredStats = true;
        }

        foreach (var effect in _timedEffects)
        {
            if (effect == null || effect.Duration <= 0f)
                continue;

            GameManager.Instance.Player.ApplyTimedStatBonus(
                new StatBonus
                {
                    Type = effect.Type,
                    Value = effect.Value
                },
                effect.Duration);

            restoredStats = true;
        }

        return restoredStats;
    }

    private bool IsPercentageBonus(BonusType type)
    {
        return type == BonusType.Damage ||
               type == BonusType.MoveSpeed ||
               type == BonusType.CritChance ||
               type == BonusType.AttackCooldownReduction;
    }
    
    public void Unequip(GameObject character)
    {
        var quickItemManager = character.transform.parent.GetComponentInChildren<QuickItemManager>();
        quickItemManager.SetQuickItem(null, null, 0, 5);
    }
}

[System.Serializable]
public class TimedItemStatEffect
{
    public BonusType Type;
    public float Value;
    public float Duration = 5f;
}

public interface IDestroyableItem
{
    
}

public interface IItemAction
{
    public string ActionName { get; }

    public AudioClip actionSfx { get; }

    bool PerformAction(GameObject character, InventoryItem inventoryItem, bool isUsingItem = false, string slotName = "");
}

