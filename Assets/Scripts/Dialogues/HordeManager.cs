using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HordeManager : Singleton<HordeManager>
{
    [Header("Horde Settings")]
    public int currentHorde = 1;
    public int enemiesPerHorde = 1;
    public int enemiesIncreasePerHorde = 1;

    [Header("Enemies")] [SerializeField] private List<GameObject> enemyPrefabs;
    
    private string _previousScene;
    private int _aliveEnemies = 0;
    
    public void SavePreviousScene()
    {
        _previousScene = SceneManager.GetActiveScene().name;
    }

    public void StartHorde()
    {
        SavePreviousScene();

        Debug.Log($"Starting Horde {currentHorde}");

        LoadingSceneManager.Instance.LoadScene("Forest", true);

        StartCoroutine(WaitForSceneAndSpawn());
    }
    
    private System.Collections.IEnumerator WaitForSceneAndSpawn()
    {
        yield return null; // 1 frame

        // czekamy aż scena się załaduje
        while (FindObjectsOfType<EnemySpawner>().Length == 0)
            yield return null;

        SpawnHorde();
    }
    
    private void SpawnHorde()
    {
        var spawners = FindObjectsOfType<EnemySpawner>();

        if (spawners.Length == 0)
        {
            Debug.LogWarning("No EnemySpawners found!");
            return;
        }

        _aliveEnemies = 0;

        for (var i = 0; i < enemiesPerHorde; i++)
        {
            var spawner = spawners[Random.Range(0, spawners.Length)];
            var enemyPrefab = GetRandomEnemy();
            var enemy= Instantiate(enemyPrefab, spawner.spawnPoint.position, Quaternion.identity);

            Debug.Log(enemy.GetComponent<EnemyBrain>().EnemyID);
            
            _aliveEnemies++;
        }

        Debug.Log($"Spawned {_aliveEnemies} enemies");
    }
    
    public void OnEnemyKilled()
    {
        _aliveEnemies--;

        Debug.Log($"Enemy killed. Remaining: {_aliveEnemies}");

        if (_aliveEnemies <= 0)
        {
            Debug.Log("All enemies defeated!");
            CompleteHorde();
        }
    }
    
    private GameObject GetRandomEnemy()
    {
        return enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
    }

    public void CompleteHorde()
    {
        Debug.Log($"Horde {currentHorde} completed");

        currentHorde++;

        enemiesPerHorde += enemiesIncreasePerHorde;

        ReturnToPreviousScene();
    }

    public void FailHorde()
    {
        Debug.Log("Player died - Game Over");

        // TODO: Game Over screen
        SceneManager.LoadScene("MainMenu");
    }

    private void ReturnToPreviousScene()
    {
        if (string.IsNullOrEmpty(_previousScene))
        {
            Debug.LogWarning("No previous scene saved!");
            return;
        }

        LoadingSceneManager.Instance.LoadScene(_previousScene, true);

        // reset dnia
        var cycle = FindObjectOfType<DayNightCycle>();
        if (cycle != null)
            cycle.ResetCycle();
    }

    // do spawnerów
    public int GetEnemyCount()
    {
        return enemiesPerHorde;
    }
}