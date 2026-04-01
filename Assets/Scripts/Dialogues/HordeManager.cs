using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HordeManager : Singleton<HordeManager>
{
    [Header("Horde Settings")]
    public int currentHorde = 1;
    public int enemiesPerHorde = 1;
    public int enemiesIncreasePerHorde = 1;

    [Header("Enemies")]
    [SerializeField] private List<GameObject> normalEnemies;
    [SerializeField] private List<GameObject> eliteEnemies;
    [SerializeField] private List<GameObject> bossEnemies;
    
    [SerializeField] private HordeConfigSO hordeConfig;
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
        yield return null;

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

        var data = hordeConfig.GetHorde(currentHorde - 1);

        _aliveEnemies = 0;

        // NORMAL
        SpawnGroup(data.normalEnemies, spawners, false, false, data, normalEnemies);

        // ELITE
        SpawnGroup(data.eliteEnemies, spawners, true, false, data, eliteEnemies);

        // BOSS
        SpawnGroup(data.bossEnemies, spawners, false, true, data, bossEnemies);

        Debug.Log($"Spawned {_aliveEnemies} enemies");
    }
    
    private void SpawnGroup(int count, EnemySpawner[] spawners, bool isElite, bool isBoss, HordeData data, List<GameObject> enemies)
    {
        for (var i = 0; i < count; i++)
        {
            var spawner = spawners[Random.Range(0, spawners.Length)];
            var prefab = GetRandomEnemy(enemies);

            var enemyGO = Instantiate(prefab, spawner.spawnPoint.position, Quaternion.identity);

            var stats = enemyGO.GetComponent<EnemyStatistics>();

            stats.DetectRange = 99999;
            
            if (stats != null)
            {
                stats.ApplyHordeScaling(
                    data.hpMultiplier,
                    data.damageMultiplier,
                    1f,
                    isElite,
                    isBoss
                );
            }

            _aliveEnemies++;
        }
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
    
    private GameObject GetRandomEnemy(List<GameObject> enemyList)
    {
        return enemyList[Random.Range(0, enemyList.Count)];
    }

    private void CompleteHorde()
    {
        Debug.Log($"Horde {currentHorde} completed");
        InventoryController.Instance.ChangeGoldAmount(hordeConfig.GetHorde(currentHorde - 1).goldReward);

        currentHorde++;
        enemiesPerHorde += enemiesIncreasePerHorde;
        
        //TODO Timescale = 0f
        //TODO Show ui with going back
        
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