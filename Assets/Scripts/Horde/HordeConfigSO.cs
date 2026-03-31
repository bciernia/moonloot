using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HordeConfig", menuName = "Horde/Horde Config")]
public class HordeConfigSO : ScriptableObject
{
    public List<HordeData> hordes;

    public HordeData GetHorde(int index)
    {
        if (index < 0 || index >= hordes.Count)
        {
            Debug.LogWarning("Horde index out of range!");
            return hordes[^1]; 
        }

        return hordes[index];
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