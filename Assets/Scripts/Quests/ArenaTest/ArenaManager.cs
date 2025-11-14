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

    private class EnemyRef
    {
        public GameObject GameObject;
        public EnemyStatistics Stats;
    }

    private readonly List<EnemyRef> _spawnedEnemies = new();
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
        int enemiesCount = difficultyLevel switch
        {
            DifficultyLevel.Easy => 2,
            DifficultyLevel.Medium => 3,
            DifficultyLevel.Hard => 4,
            _ => 2
        };

        for (int i = 0; i < enemiesCount; i++)
        {
            var spawnPoint = spawnPoints[i % spawnPoints.Count];
            var enemyPrefab = enemiesToSpawn[Random.Range(0, enemiesToSpawn.Count)];

            var enemy = Instantiate(enemyPrefab, spawnPoint.transform.position, Quaternion.identity);
            var stats = enemy.GetComponent<EnemyStatistics>();
            _spawnedEnemies.Add(new EnemyRef { GameObject = enemy, Stats = stats });
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

        _spawnedEnemies.RemoveAll(e => e.GameObject == null);

        bool allDead = true;
        foreach (var enemy in _spawnedEnemies)
        {
            if (enemy.Stats != null && enemy.Stats.CurrentHP > 0)
            {
                allDead = false;
                break;
            }
        }

        if (allDead && _activeDoor != null)
        {
            Destroy(_activeDoor);
            _arenaStarted = false;
            Debug.Log("All enemies defeated! Arena door opened!");
        }
    }
}
