using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArenaManager : Singleton<ArenaManager>
{
    [Header("Arena Configuration")]
    [SerializeField] private List<GameObject> spawnPoints;
    [SerializeField] private List<GameObject> enemiesToSpawn;
    [SerializeField] private GameObject arenaDoor;
    [SerializeField] private Transform arenaDoorPosition;

    private List<GameObject> _spawnedEnemies = new();
    private GameObject _activeDoor;
    private bool _arenaStarted;

    public void StartArena(DifficultyLevel difficultyLevel)
    {
        if (_arenaStarted) return;
        _arenaStarted = true;
        
        SpawnEnemiesByDifficulty(difficultyLevel);
        SpawnArenaDoor();
    }

    private void SpawnEnemiesByDifficulty(DifficultyLevel difficultyLevel)
    {
        var enemiesCount = 0;

        switch (difficultyLevel)
        {
            case DifficultyLevel.Easy:
                enemiesCount = 2;
                break;
            case DifficultyLevel.Medium:
                enemiesCount = 3;
                break;
            case DifficultyLevel.Hard:
                enemiesCount = 4;
                break;
            default:
                enemiesCount = 2;
                break;
        }

        for (var i = 0; i < enemiesCount; i++)
        {
            var spawnPoint = spawnPoints[i % spawnPoints.Count];
            var enemyPrefab = enemiesToSpawn[Random.Range(0, enemiesToSpawn.Count)];

            var enemy = Instantiate(enemyPrefab, spawnPoint.transform.position, Quaternion.identity);
            _spawnedEnemies.Add(enemy);
        }

        Debug.Log($"Arena started with {enemiesCount} enemies.");
    }

    private void SpawnArenaDoor()
    {
        if (arenaDoor == null) return;
        _activeDoor = Instantiate(arenaDoor, arenaDoorPosition.position, Quaternion.identity);
    }

    private void Update()
    {
        if (!_arenaStarted) return;

        _spawnedEnemies = _spawnedEnemies.Where(e => e != null).ToList();

        bool allDead = _spawnedEnemies.All(enemy =>
        {
            var stats = enemy.GetComponent<EnemyStatistics>();
            return stats == null || stats.CurrentHP <= 0;
        });

        if (allDead && _activeDoor != null)
        {
            Destroy(_activeDoor);
            _arenaStarted = false;
            Debug.Log("All enemies defeated! Arena door opened!");
        }
    }
}
