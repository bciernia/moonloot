using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HordeConfig", menuName = "Horde/Horde Config")]
public class HordeConfigSO : ScriptableObject
{
    public List<HordeData> hordes;
    
    [Header("Scaling")]
    public float enemyGrowth = 1.2f;
    public float eliteRatioGrowth = 0.02f;
    public float hpGrowth = 0.12f;
    public float damageGrowth = 0.08f;
    public float goldGrowth = 1.15f;

    [Header("Limits")]
    public int maxEnemies = 200;
    public float maxHpMultiplier = 10f;
    public float maxDamageMultiplier = 5f;

    public HordeData GetHorde(int index)
    {
        if (index < hordes.Count)
            return hordes[index];

        return GenerateHorde(index);
    }
    
    private HordeData GenerateHorde(int index)
    {
        var hordeNumber = index + 1;

        var totalEnemies = Mathf.RoundToInt(
            Mathf.Min(5 * Mathf.Pow(enemyGrowth, hordeNumber), maxEnemies)
        );

        var eliteCount = Mathf.FloorToInt(totalEnemies * Mathf.Min(0.1f + hordeNumber * eliteRatioGrowth, 0.5f));
        var bossCount = hordeNumber % 5 == 0 ? 1 : 0;

        var normalCount = totalEnemies - eliteCount - bossCount;

        var hpMultiplier = Mathf.Min(1f + hordeNumber * hpGrowth, maxHpMultiplier);
        var damageMultiplier = Mathf.Min(1f + hordeNumber * damageGrowth, maxDamageMultiplier);

        var gold = Mathf.RoundToInt(100 * Mathf.Pow(goldGrowth, hordeNumber));

        return new HordeData
        {
            hordeNumber = hordeNumber,
            normalEnemies = normalCount,
            eliteEnemies = eliteCount,
            bossEnemies = bossCount,
            hpMultiplier = hpMultiplier,
            damageMultiplier = damageMultiplier,
            goldReward = gold
        };
    }
}

[Serializable]
public class HordeData
{
    [Header("Horde Info")]
    public int hordeNumber;

    [Header("Enemies Count")]
    public int normalEnemies;
    public int eliteEnemies;
    public int bossEnemies;

    [Header("Rewards")]
    public int goldReward;

    [Header("Scaling")]
    [Range(0.5f, 5f)] public float hpMultiplier = 1f;
    [Range(0.5f, 5f)] public float damageMultiplier = 1f;
}