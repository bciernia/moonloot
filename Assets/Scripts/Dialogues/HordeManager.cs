using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField] private float defendMultiplier = 0.5f;
    
    public HordeData PreparedData { get; private set; }
    public HordeMutation PreparedMutation { get; private set; }
    
    public Transform DefendTarget { get; private set; }
    public HordeObjective CurrentObjective => _currentObjective;

    private GameObject _defendTarget;
    private bool _defendActive = false;
    private const float _defendDuration = 60f;
    private const int _maxAliveEnemies = 5;

    private HordeMutation _currentMutation;

    public static Action OnHordeStarted;
    public static Action<int> OnHordeFinished;

    private void SavePreviousScene()
    {
        _previousScene = SceneManager.GetActiveScene().name;
    }
    
    public void PrepareHorde()
    {
        PreparedData = hordeConfig.GetHorde(currentHorde - 1);
        PreparedMutation = GetRandomMutation();

        Debug.Log($"Prepared Horde {currentHorde} | {PreparedData.objective} | {PreparedMutation}");
    }

    public void StartHorde()
    {
        SavePreviousScene();
        
        Debug.Log($"Starting Horde {currentHorde}");
        CombatStatsManager.Instance.ResetStats(Player.Instance.transform);
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

        var data = PreparedData;
        _currentObjective = data.objective;
        _currentMutation = PreparedMutation;

        _aliveEnemies = 0;
        
        Debug.Log($"Objective: {_currentObjective.ToString()}");

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
        
        OnHordeStarted?.Invoke();
    }

    #region EliteHunt
    private IEnumerator StartEliteHunt(EnemySpawner[] spawners, HordeData data)
    {
        Debug.Log("Elite Hunt Started");

        data.normalEnemies = 0;
        data.eliteEnemies = Mathf.Max(5, data.eliteEnemies * 2);

        yield return StartCoroutine(SpawnHordeRoutine(spawners, data));
    }
    #endregion
    
    #region DefendObjective 
    
    private IEnumerator StartDefendObject(EnemySpawner[] spawners, HordeData data)
    {
        Debug.Log("Defend Objective Started");

        var defendTargetPosition = FindObjectOfType<DefendTargetSpawner>();
        
        _defendTarget = Instantiate(defendPrefab, defendTargetPosition.spawnPoint.position, Quaternion.identity);
        DefendTarget = _defendTarget.transform;

        _defendActive = true;
        _aliveEnemies = 0;

        var timer = 0f;
        var spawnTimer = 0f;
        var nextSpawnTime = RNGManager.Instance.GetRandomNumberFromRange(1, 3);

        while (_defendActive)
        {
            timer += Time.deltaTime;
            spawnTimer += Time.deltaTime;

            Debug.Log($"Zostało: {_defendDuration - timer}");

            if (timer >= _defendDuration)
            {
                Debug.Log("Defend Success!");
                _defendActive = false;
                CompleteHorde();
                yield break;
            }

            if (spawnTimer >= nextSpawnTime)
            {
                if (_aliveEnemies < _maxAliveEnemies)
                {
                    SpawnOneEnemy(spawners, data);
                }

                spawnTimer = 0f;
                nextSpawnTime = RNGManager.Instance.GetRandomNumberFromRange(3, 6);
            }

            yield return null;
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

        var brain = enemyGO.GetComponent<EnemyBrain>();

        if (brain != null)
        {
            if (_currentObjective == HordeObjective.DefendObject && DefendTarget != null)
            {
                brain.SetTarget(DefendTarget);
            }
        }
        
        var stats = enemyGO.GetComponent<EnemyStatistics>();

        if (stats != null)
        {
            stats.DetectRange = 99999;

            var finalHpMultiplier = data.hpMultiplier;
            if (_currentObjective == HordeObjective.DefendObject)
            {
                finalHpMultiplier *= defendMultiplier;
            }

            stats.Initialize();

            stats.ApplyHordeScaling(
                finalHpMultiplier,
                data.damageMultiplier,
                1f,
                pool == eliteEnemies,
                pool == bossEnemies
            );

            ApplyMutation(stats);
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

                stats.Initialize();
                
                stats.ApplyHordeScaling(
                    data.hpMultiplier,
                    data.damageMultiplier,
                    1f,
                    isElite,
                    isBoss
                );
                
                ApplyMutation(stats);
            }
            _aliveEnemies++;

            yield return new WaitForSeconds(1f);
        }
    }
    
    #endregion

    public void OnEnemyKilled()
    {
        if (_currentObjective == HordeObjective.DefendObject) return;
        
        _aliveEnemies--;
        CombatStatsManager.Instance.EnemiesKilled++;

        Debug.Log($"Enemy killed. Remaining: {_aliveEnemies}");

        if (_aliveEnemies <= 0)
        {
            StartCoroutine(FinalKillSequence());
            Debug.Log("All enemies defeated!");
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
        OnHordeFinished?.Invoke(currentHorde - 1);
        enemiesPerHorde += enemiesIncreasePerHorde;
    }

    private void FailHorde()
    {
        Debug.Log("Player died - Game Over");

        // TODO: Game Over screen
        // SceneManager.LoadScene("MainMenu");
        ReturnToPreviousScene();
    }

    public void ReturnToPreviousScene()
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

    public int GetRemainEnemies() => _aliveEnemies;

    private HordeMutation GetRandomMutation() => EnumUtils.GetRandomEnum<HordeMutation>();

    private void ApplyMutation(EnemyStatistics stats)
    {
        Debug.Log($"⚠ Mutation active: {_currentMutation}");
        switch (_currentMutation)
        {
            case HordeMutation.StrongEnemies:
                stats.MaxHP *= 1.5f;
                stats.RestoreHealth(stats.MaxHP);
                break;

            case HordeMutation.FastEnemies:
                stats.Speed *= 1.5f;
                stats.ChaseSpeed *= 1.5f;
                break;

            case HordeMutation.BrutalEnemies:
                stats.Damage *= 1.5f;
                break;
            case HordeMutation.None:
            default:
                break;
        }
    }
    
    private IEnumerator FinalKillSequence()
    {
        yield return StartCoroutine(FinalKillSlowMo());

        CompleteHorde();
    }
    
    private IEnumerator FinalKillSlowMo()
    {
        var originalTimeScale = Time.timeScale;
        var originalFixedDelta = Time.fixedDeltaTime;

        Time.timeScale = 0.2f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(1f);

        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDelta;
        
        yield return new WaitForSecondsRealtime(3f);
    }
}