using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public List<RespawnPoint> respawnPoints;

    [Header("Config")]
    public int minActiveSpawns = 2;
    public int maxActiveSpawns = 4;

    [Header("Quest requirement")]
    public string requiredQuestID;

    private void Start()
    {
        var questJournal = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestJournal>();
        
        if (questJournal.IsQuestCompleted(requiredQuestID))
            return;

        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        int spawnCount = Random.Range(minActiveSpawns, maxActiveSpawns + 1);

        var shuffled = respawnPoints
            .OrderBy(x => Random.value)
            .Take(spawnCount);

        foreach (var point in shuffled)
        {
            point.Spawn();
        }
    }
}