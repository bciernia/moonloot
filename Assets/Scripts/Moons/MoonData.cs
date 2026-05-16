using System;
using UnityEngine;

[Serializable]
public class MoonData
{
    public MoonType Type;

    public string DisplayName;

    public Color EveningColor;
    public Color NightColor;
    public Sprite MoonSprite;
    [TextArea]
    public string Description;
    
    [Header("Gameplay")]
    public MoonObjectiveType ObjectiveType;
    public int RequiredAmount = 10;
    [TextArea]
    public string ObjectiveText;

    public float EnemyHealthMultiplier = 1f;
    public float EnemyDamageMultiplier = 1f;
    public float EnemySpeedMultiplier = 1f;

    public float LootMultiplier = 1f;

    public float GoldMultiplier = 1f;

    [Range(0,1)]
    public float EliteChanceBonus = 0.75f;
    
    [SerializeField] private InventoryItem requiredItem;

    public InventoryItem RequiredItem => requiredItem;
}