using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.AI;
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
    
    [Header("ExitPrefab")]
    [SerializeField] private GameObject exitPrefab;
    
    public HordeData PreparedData { get; private set; }
    public HordeMutation PreparedMutation { get; private set; }
    
    public Transform DefendTarget { get; private set; }
    public HordeObjective CurrentObjective => _currentObjective;

    private GameObject _defendTarget;
    private bool _defendActive = false;
    private const float _defendDuration = 60f;
    private const int _maxAliveEnemies = 5;

    private HordeMutation _currentMutation;
    private bool _bossSpawned = false;
    private bool _rescuedNPC = false;
    
    public VillageNpcRuntime SelectedNpc { get; set; }
    
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

        LootSpawnManager.Instance.SpawnAll();
        SpawnNPC();
        SpawnHorde();
        
    }
    
    public void OnPlayerExit()
    {
        Debug.Log("Player exited");

        Debug.Log(_rescuedNPC ? "NPC rescued!" : "No NPC found");

        StopAllCoroutines();
        CompleteHorde();
    }
    
    private void SpawnHorde()
    {
        var spawners = FindObjectsOfType<EnemySpawner>();

        if (spawners.Length == 0)
        {
            Debug.LogWarning("No EnemySpawners found!");
            return;
        }

        _rescuedNPC = false;
        
        var data = PreparedData;
        // _currentObjective = data.objective;
        _currentObjective = HordeObjective.NightExploration;
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
            
            case HordeObjective.NightExploration:
                StartCoroutine(StartNightExploration(spawners, data));
                break;
            
            case HordeObjective.KillAll:
            default:
                StartCoroutine(SpawnHordeRoutine(spawners, data));
                break;
        }
        
        OnHordeStarted?.Invoke();
    }

    #region NightExploration
    private IEnumerator StartNightExploration(EnemySpawner[] spawners, HordeData data)
    {
        Debug.Log("Night Exploration Started");

        var spawnTimer = 0f;
        var spawnInterval = 3f;

        var elapsed = 0f;
        var speedIncreaseTimer = 0f;

        _aliveEnemies = 0;

        SpawnExit(); 

        while (true) 
        {
            elapsed += Time.deltaTime;
            spawnTimer += Time.deltaTime;

            // spawn enemy
            if (spawnTimer >= spawnInterval)
            {
                if (_aliveEnemies < 50)
                {
                    SpawnEnemyNearPlayer(spawners, data);
                }

                spawnTimer = 0f;
            }

            // scaling po 2 minutach
            if (elapsed >= 120f)
            {
                speedIncreaseTimer += Time.deltaTime;

                if (speedIncreaseTimer >= 5f)
                {
                    IncreaseEnemiesSpeed();
                    speedIncreaseTimer = 0f;
                }
            }

            yield return null;
        }
    }
    
    private void IncreaseEnemiesSpeed()
    {
        Debug.Log("Enemies speed increased!");
        var enemies = FindObjectsOfType<EnemyStatistics>();

        foreach (var e in enemies)
        {
            e.Speed *= 1.1f;
            e.ChaseSpeed *= 1.1f;
        }
    }
    
    private void SpawnExit()
    {
        var exitSpawners = FindObjectsOfType<ExitSpawner>();

        if (exitSpawners.Length == 0)
        {
            Debug.LogWarning("No ExitSpawners found!");
            return;
        }

        var chosen = exitSpawners[Random.Range(0, exitSpawners.Length)];

        Instantiate(exitPrefab, chosen.transform.position, Quaternion.identity);

        Debug.Log("Exit spawned!");
    }
    
    private void SpawnBossNearPlayer(HordeData data)
    {
        Debug.Log("BOSS SPAWNED");

        var playerPos = Player.Instance.transform.position;

        var circle = Random.insideUnitCircle.normalized * Random.Range(12f, 18f);
        var spawnPos = playerPos + new Vector3(circle.x, 0, circle.y);

        var prefab = GetRandomEnemy(bossEnemies);
        var bossGO = Instantiate(prefab, spawnPos, Quaternion.identity);

        SetupEnemy(bossGO, data, bossEnemies);

        var stats = bossGO.GetComponent<EnemyStatistics>();
        if (stats != null)
        {
            NPCInfoManager.Instance.ShowNpcInfo(stats);
        }
    }
    
    private void SpawnEnemyNearPlayer(EnemySpawner[] spawners, HordeData data)
    {
        var playerPos = Player.Instance.transform.position;

        var minDistance = 12f;
        var maxDistance = 20f;
        var maxAttempts = 10;

        for (var i = 0; i < maxAttempts; i++)
        {
            var rawPos = GetRandomSpawnPosition(playerPos, minDistance, maxDistance);

            if (IsValidSpawnPosition(rawPos, playerPos, out Vector3 finalPos))
            {
                var pool = GetEnemyPool();
                var prefab = GetRandomEnemy(pool);

                var enemyGO = Instantiate(prefab, finalPos, Quaternion.identity);
                SetupEnemy(enemyGO, data, pool);
                return;
            }
        }

        Debug.LogWarning("Nie znaleziono dobrej pozycji spawnu");
    }
    
    private Vector3 GetRandomSpawnPosition(Vector3 center, float minDist, float maxDist)
    {
        var distance = Random.Range(minDist, maxDist);
        var dir = Random.insideUnitCircle.normalized;

        return center + new Vector3(dir.x, 0, dir.y) * distance;
    }
    
    private bool IsValidSpawnPosition(Vector3 pos, Vector3 playerPos, out Vector3 finalPos)
    {
        finalPos = pos;

        if (Vector3.Distance(pos, playerPos) < 10f)
            return false;

        if (Physics.CheckSphere(pos, 0.5f, LayerMask.GetMask("Obstacle")))
            return false;

        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            finalPos = hit.position;
            return true;
        }

        return false;
    }
    
    private void SetupEnemy(GameObject enemyGO, HordeData data, List<GameObject> pool)
    {
        var stats = enemyGO.GetComponent<EnemyStatistics>();

        if (stats != null)
        {
            stats.DetectRange = 9999999;
            stats.Initialize();

            stats.ApplyHordeScaling(
                data.hpMultiplier,
                data.damageMultiplier,
                1f,
                pool == eliteEnemies,
                pool == bossEnemies
            );

            ApplyMutation(stats);
        }

        _aliveEnemies++;
    }
    
    private List<GameObject> GetEnemyPool()
    {
        var roll = Random.value;

        return roll switch
        {
            < 0.7f => normalEnemies,
            _ => eliteEnemies,
        };
    }
    #endregion

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
        var nextSpawnTime = RNGManager.Instance.GetRandomInt(1, 3);

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
                nextSpawnTime = RNGManager.Instance.GetRandomInt(3, 6);
            }

            yield return null;
        }
    }
    
    public void SetRescuedNPC()
    {
        _rescuedNPC = true;
        PointsManager.Instance.AddScore(100);

        if (SelectedNpc != null)
        {
            WorldManager.Instance.AddNpc(SelectedNpc);
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
            stats.DetectRange = 9999999;

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
                stats.DetectRange = 9999999;

                stats.Initialize();
                
                stats.ApplyHordeScaling(
                    data.hpMultiplier,
                    data.damageMultiplier,
                    1f,
                    isElite,
                    isBoss
                );

                if (isBoss)
                {
                    NPCInfoManager.Instance.ShowNpcInfo(stats);
                }
                
                ApplyMutation(stats);
            }
            _aliveEnemies++;

            yield return new WaitForSeconds(1f);
        }
    }
    
    #endregion

    public void OnEnemyKilled(bool isElite, bool isBoss)
    {
        if (isBoss)
        {
            CombatStatsManager.Instance.BossEnemiesKilled++;
            CombatStatsManager.Instance.GoldEarned += RNGManager.Instance.GetRandomInt(50, 100);
            PointsManager.Instance.AddScore(3);
        }
        else if (isElite)
        {
            CombatStatsManager.Instance.EliteEnemiesKilled++;
            CombatStatsManager.Instance.GoldEarned += RNGManager.Instance.GetRandomInt(10, 20);
            PointsManager.Instance.AddScore(10);
        }
        else
        {
            CombatStatsManager.Instance.NormalEnemiesKilled++;
            CombatStatsManager.Instance.GoldEarned += RNGManager.Instance.GetRandomInt(2, 5);
            PointsManager.Instance.AddScore(100);
        }
        
        if (_currentObjective == HordeObjective.DefendObject) return;
        
        _aliveEnemies--;

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

        InventoryController.Instance.ChangeGoldAmount(hordeConfig.GetHorde(currentHorde - 1).goldReward + CombatStatsManager.Instance.GoldEarned);
        currentHorde++;
        OnHordeFinished?.Invoke(currentHorde - 1);
        enemiesPerHorde += enemiesIncreasePerHorde;
        PointsManager.Instance.AddScore(100);
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
        Debug.Log($"Mutation active: {_currentMutation}");
        switch (_currentMutation)
        {
            case HordeMutation.StrongEnemies:
                stats.MaxHP *= 1.5f;
                stats.RestoreHealthForEliteEnemy(stats.MaxHP);
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
        //TODO Slow motion zrobić dla pokonania objectivu którym jest pokonanie wszystkich przeciwników
        yield return StartCoroutine(FinalKillSlowMo());

        // CompleteHorde();
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
    
    private void SpawnNPC()
    {
        if (SelectedNpc == null)
        {
            Debug.LogWarning("No NPC selected!");
            return;
        }

        var spawners = FindObjectsOfType<NpcSpawner>();

        if (spawners.Length == 0)
        {
            Debug.LogWarning("No NpcSpawners found!");
            return;
        }

        var chosen = spawners[Random.Range(0, spawners.Length)];

        var npcGO = Instantiate(
            SelectedNpc.Data.Character,
            chosen.spawnPoint.position,
            Quaternion.identity
        );

        Debug.Log($"Spawned NPC: {SelectedNpc.Name}");
    }
    
    public bool IsNpcRescued() => _rescuedNPC;
}