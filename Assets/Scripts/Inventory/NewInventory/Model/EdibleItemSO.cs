using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Edible", fileName = "Edible_")]
public class EdibleItemSO : ItemSO, IDestroyableItem, IItemAction, IAsyncItemAction
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
            if (effect == null ||
                (!effect.UntilReturnToBase && effect.Duration <= 0f))
                continue;

            var value = IsPercentageBonus(effect.Type)
                ? $"{effect.Value:+0.#;-0.#;0}%"
                : $"{effect.Value:+0.#;-0.#;0}";

            var duration = effect.UntilReturnToBase
                ? "until return to Base"
                : $"{effect.Duration:0.#}s";

            description += $"{effect.Type}: {value} ({duration})\n";
        }

        return description;
    }
    
    public bool PerformAction(GameObject character, InventoryItem inventoryItem, bool isUsingItem = false, string slotName = "")
    {
        if (!isUsingItem)
        {
            var slotIndex = slotName == "QuickSlot1" ? 5 : 6;
            
            return QuickItemManager.Instance.SetQuickItem(
                this,
                inventoryItem.itemState,
                inventoryItem.quantity,
                slotIndex);
        }

        // Items with a confirmation flow must be used through PerformActionAsync.
        return false;
    }

    public void PerformActionAsync(
        GameObject character,
        InventoryItem inventoryItem,
        Action<bool> completed)
    {
        var player = GameManager.Instance.Player;
        var runFoodBonuses = GetRunFoodBonuses();

        if (runFoodBonuses.Count > 0 && player.HasRunFoodBonuses)
        {
            if (ConfirmationManager.Instance == null)
            {
                Debug.LogError("Cannot replace run food bonuses because ConfirmationManager is missing.");
                completed?.Invoke(false);
                return;
            }

            ConfirmationManager.Instance.ShowConfirmation(
                BuildRunFoodConfirmation(player, runFoodBonuses),
                accepted =>
                {
                    if (!accepted)
                    {
                        completed?.Invoke(false);
                        return;
                    }

                    completed?.Invoke(ConsumeItem(player, runFoodBonuses));
                });
            return;
        }

        completed?.Invoke(ConsumeItem(player, runFoodBonuses));
    }

    private bool ConsumeItem(Player player, List<StatBonus> runFoodBonuses)
    {
        var health = player.PlayerHealth;

        if (health.CanRestoreHealth() && HealthValue > 0)
        {
            health.RestoreHealth(HealthValue);
        }

        if (health.CanRestoreMana() && ManaValue > 0)
        {
            health.RestoreMana(ManaValue);
        }

        if (runFoodBonuses.Count > 0)
        {
            player.ApplyRunFoodBonuses(runFoodBonuses);
        }

        foreach (var effect in _timedEffects)
        {
            if (effect == null || effect.UntilReturnToBase || effect.Duration <= 0f)
                continue;

            player.ApplyTimedStatBonus(
                new StatBonus { Type = effect.Type, Value = effect.Value },
                effect.Duration);
        }

        return true;
    }

    private List<StatBonus> GetRunFoodBonuses()
    {
        var valuesByType = new Dictionary<BonusType, float>();

        foreach (var effect in _timedEffects)
        {
            if (effect == null || !effect.UntilReturnToBase)
                continue;

            if (!valuesByType.TryGetValue(effect.Type, out var currentValue) ||
                effect.Value > currentValue)
            {
                valuesByType[effect.Type] = effect.Value;
            }
        }

        var bonuses = new List<StatBonus>();
        foreach (var pair in valuesByType)
        {
            bonuses.Add(new StatBonus
            {
                Type = pair.Key,
                Value = pair.Value
            });
        }

        return bonuses;
    }

    private string BuildRunFoodConfirmation(
        Player player,
        List<StatBonus> incomingBonuses)
    {
        var currentBonuses = player.GetRunFoodBonuses();
        var resultingBonuses = new Dictionary<BonusType, float>(currentBonuses);
        var lines = new StringBuilder();

        foreach (var bonus in incomingBonuses)
        {
            if (!resultingBonuses.TryGetValue(bonus.Type, out var currentValue))
            {
                resultingBonuses[bonus.Type] = bonus.Value;
                continue;
            }

            resultingBonuses[bonus.Type] = Mathf.Max(currentValue, bonus.Value);
        }

        lines.AppendLine("Masz już bonusy z potraw na ten run:");
        AppendFoodBonuses(lines, currentBonuses);

        lines.AppendLine();
        lines.AppendLine($"Nowa potrawa: {Name}");
        foreach (var bonus in incomingBonuses)
            lines.AppendLine($"{FormatFoodBonus(bonus.Type, bonus.Value)}");

        lines.AppendLine();
        lines.AppendLine("Po zjedzeniu zostanie zachowana najwyższa wartość każdego bonusu:");
        AppendFoodBonuses(lines, resultingBonuses);

        lines.AppendLine();
        lines.Append("Czy na pewno chcesz zjeść tę potrawę?");
        return lines.ToString();
    }

    private void AppendFoodBonuses(
        StringBuilder lines,
        Dictionary<BonusType, float> bonuses)
    {
        var types = new List<BonusType>(bonuses.Keys);
        types.Sort();

        foreach (var type in types)
            lines.AppendLine(FormatFoodBonus(type, bonuses[type]));
    }

    private string FormatFoodBonus(BonusType type, float value)
    {
        var formattedValue = value.ToString("+0.#;-0.#;0");
        if (IsPercentageBonus(type))
            formattedValue += "%";

        var displayName = type switch
        {
            BonusType.MoveSpeed => "Move Speed",
            BonusType.CritChance => "Critical Chance",
            BonusType.CritMultiplier => "Critical Damage",
            BonusType.MaxHp => "Maximum HP",
            BonusType.MaxMp => "Maximum Mana",
            BonusType.AttackCooldownReduction => "Attack Speed",
            BonusType.DamageReduction => "Damage Reduction",
            _ => type.ToString()
        };

        return $"{displayName}: {formattedValue}";
    }

    private bool IsPercentageBonus(BonusType type)
    {
        return type == BonusType.Damage ||
               type == BonusType.MoveSpeed ||
               type == BonusType.CritChance ||
               type == BonusType.AttackCooldownReduction ||
               type == BonusType.DamageReduction;
    }
    
    public void Unequip(GameObject character)
    {
        var quickItemManager = character.transform.parent.GetComponentInChildren<QuickItemManager>();
        quickItemManager.SetQuickItem(null, null, 0, 5);
    }
}

public interface IAsyncItemAction
{
    void PerformActionAsync(
        GameObject character,
        InventoryItem inventoryItem,
        Action<bool> completed);
}

[System.Serializable]
public class TimedItemStatEffect
{
    public BonusType Type;
    public float Value;
    public float Duration = 5f;
    public bool UntilReturnToBase;
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

