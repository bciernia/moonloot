using System.Collections.Generic;
using UnityEngine;

public class EnemyStateManager : Singleton<EnemyStateManager>, ISaveable
{
    private HashSet<string> deadEnemies = new();

    public bool IsEnemyDead(string id)
    {
        return deadEnemies.Contains(id);
    }

    public void MarkEnemyDead(string id)
    {
        deadEnemies.Add(id);
    }

    public void Save()
    {
        ES3.Save("dead_enemies", new List<string>(deadEnemies));
    }

    public void Load()
    {
        if (!ES3.KeyExists("dead_enemies"))
            return;

        var list = ES3.Load<List<string>>("dead_enemies");
        deadEnemies = new HashSet<string>(list);
    }
}