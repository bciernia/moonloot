using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

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
    private HordeObjective _currentObjective;

    [Header("Defend objective object")]
    [SerializeField] private GameObject defendPrefab;

    private GameObject _defendTarget;
    private bool _defendActive = false;
    private readonly float _defendDuration = 60f;
    private readonly int _maxAliveEnemies = 10;
    
    private void SavePreviousScene()
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
        _currentObjective = data.objective;
        _aliveEnemies = 0;

        Debug.Log($"Objective: {_currentObjective.ToString()}");

        _currentObjective = HordeObjective.DefendObject;
        
        switch (_currentObjective)
        {
            case HordeObjective.DefendObject:
                StartCoroutine(StartDefendObject(spawners, data));
                break;
            
            case HordeObjective.EliteHunt:
                StartCoroutine(StartEliteHunt(spawners, data));
                break;
            
            case HordeObjective.KillAll:
            default:
                StartCoroutine(SpawnHordeRoutine(spawners, data));
                break;
        }
    }

    #region EliteHunt
    private System.Collections.IEnumerator StartEliteHunt(EnemySpawner[] spawners, HordeData data)
    {
        Debug.Log("Elite Hunt Started");

        data.normalEnemies = 0;
        data.eliteEnemies = Mathf.Max(5, data.eliteEnemies * 2);

        yield return StartCoroutine(SpawnHordeRoutine(spawners, data));
    }
    #endregion
    
    #region DefendObjective 
    
    private System.Collections.IEnumerator StartDefendObject(EnemySpawner[] spawners, HordeData data)
    {
        Debug.Log("Defend Objective Started");

        var defendTargetPosition = FindObjectOfType<DefendTargetSpawner>();
        
        _defendTarget = Instantiate(defendPrefab, defendTargetPosition.spawnPoint.position, Quaternion.identity);

        _defendActive = true;
        _aliveEnemies = 0;

        var timer = 0f;

        while (_defendActive)
        {
            timer += Time.deltaTime;

            if (timer >= _defendDuration)
            {
                Debug.Log("Defend Success!");
                _defendActive = false;
                CompleteHorde();
                yield break;
            }

            if (_aliveEnemies < _maxAliveEnemies)
            {
                SpawnOneEnemy(spawners, data);
            }

            yield return new WaitForSeconds(1f);
        }
    }    
    
    private void SpawnOneEnemy(EnemySpawner[] spawners, HordeData data)
    {
        var spawner = spawners[Random.Range(0, spawners.Length)];

        List<GameObject> pool;

        var roll = Random.value;

        if (roll < 0.7f)
            pool = normalEnemies;
        else if (roll < 0.95f)
            pool = eliteEnemies;
        else
            pool = bossEnemies;

        var prefab = GetRandomEnemy(pool);

        var enemyGO = Instantiate(prefab, spawner.spawnPoint.position, Quaternion.identity);

        var stats = enemyGO.GetComponent<EnemyStatistics>();

        if (stats != null)
        {
            stats.DetectRange = 99999;

            stats.ApplyHordeScaling(
                data.hpMultiplier,
                data.damageMultiplier,
                1f,
                pool == eliteEnemies,
                pool == bossEnemies
            );
        }

        _aliveEnemies++;
    }
    
    public void FailDefendObjective()
    {
        if (!_defendActive) return;

        Debug.Log("Defend Failed!");

        _defendActive = false;

        FailHorde();
    }
    
    #endregion
    
    #region SpawnHordeRoutine
    private System.Collections.IEnumerator SpawnHordeRoutine(EnemySpawner[] spawners, HordeData data)
    {
        // NORMAL
        yield return StartCoroutine(SpawnGroupRoutine(data.normalEnemies, spawners, false, false, data, normalEnemies));

        // ELITE
        yield return StartCoroutine(SpawnGroupRoutine(data.eliteEnemies, spawners, true, false, data, eliteEnemies));

        // BOSS
        yield return StartCoroutine(SpawnGroupRoutine(data.bossEnemies, spawners, false, true, data, bossEnemies));

        Debug.Log("All enemies spawned");
    }
    
    #endregion

    #region SpawnGroupRoutine
    private System.Collections.IEnumerator SpawnGroupRoutine(
        int count,
        EnemySpawner[] spawners,
        bool isElite,
        bool isBoss,
        HordeData data,
        List<GameObject> enemies)
    {
        for (var i = 0; i < count; i++)
        {
            var spawner = spawners[Random.Range(0, spawners.Length)];
            var prefab = GetRandomEnemy(enemies);

            var enemyGO = Instantiate(prefab, spawner.spawnPoint.position, Quaternion.identity);

            var stats = enemyGO.GetComponent<EnemyStatistics>();

            if (stats != null)
            {
                stats.DetectRange = 99999;

                stats.ApplyHordeScaling(
                    data.hpMultiplier,
                    data.damageMultiplier,
                    1f,
                    isElite,
                    isBoss
                );
            }

            _aliveEnemies++;

            yield return new WaitForSeconds(1f);
        }
    }
    
    #endregion

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

    private void FailHorde()
    {
        Debug.Log("Player died - Game Over");

        // TODO: Game Over screen
        // SceneManager.LoadScene("MainMenu");
        ReturnToPreviousScene();
    }

    private void ReturnToPreviousScene()
    {
        if (string.IsNullOrEmpty(_previousScene))
        {
            Debug.LogWarning("No previous scene saved!");
            return;
        }

        LoadingSceneManager.Instance.LoadScene(_previousScene, true);

        var cycle = FindObjectOfType<DayNightCycle>();
        if (cycle != null)
            cycle.ResetCycle();
    }

    public int GetEnemyCount()
    {
        return enemiesPerHorde;
    }
}