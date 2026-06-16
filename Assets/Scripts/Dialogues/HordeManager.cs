using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MoreMountains.Feedbacks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class HordeManager : Singleton<HordeManager>, ISaveable
{
    [Header("Horde Settings")]
    public int currentHorde = 1;
    public int enemiesPerHorde = 1;
    public int enemiesIncreasePerHorde = 1;

    [Header("Enemies")]
    [SerializeField] private List<GameObject> normalEnemies;
    [SerializeField] private List<GameObject> eliteEnemies;
    [SerializeField] private List<GameObject> bossEnemies;
    [SerializeField] private GameObject corruptedVillager;
    
    [SerializeField] private HordeConfigSO hordeConfig;
    private string _previousScene;
    private int _aliveEnemies = 0;
    private HordeObjective _currentObjective;

    [Header("Defend objective object")]
    [SerializeField] private GameObject defendPrefab;
    [SerializeField] private float defendMultiplier = 0.5f;
    
    [Header("ExitPrefab")]
    [SerializeField] private GameObject exitPrefab;
    
    [Header("Villagers")]
    [SerializeField] public List<VillageNpcData> workerPool;
    
    [SerializeField] private NpcDatabase _npcDatabase;
    
    [SerializeField] private NightDatabaseSO _nightDatabase;
    
    [Header("Obelisk Objective")]
    [SerializeField] private GameObject obeliskPrefab;

    private int _activatedObelisks;
    private int _spawnedObelisks;

    public NightLocationSO CurrentNightLocation { get; private set; }
    
    public HordeData PreparedData { get; private set; }
    public HordeMutation PreparedMutation { get; private set; }
    
    public Transform DefendTarget { get; private set; }
    public HordeObjective CurrentObjective => _currentObjective;
    public bool IsHeroNight => NightCycleStep == 3;

    private GameObject _defendTarget;
    private bool _defendActive = false;
    private const float _defendDuration = 60f;
    private const int _maxAliveEnemies = 5;

    private HordeMutation _currentMutation;
    private bool _bossSpawned = false;
    private int _rescuedNpcCount = 0;
    
    private Coroutine _nightRoutine;
    private bool _isNightRunning;
    
    private bool _bossAlive;
    
    private bool _hordePrepared;

    private GameObject _spawnedExit;
    
    private bool _objectiveCompleted;
    
    private int _aliveTrees;

    public MoonData CurrentMoon { get; set; }
    
    public VillageNpcRuntime SelectedNpc { get; set; }
    
    private List<VillageNpcRuntime> _spawnedNpcsThisRun = new();

    [Header("Nythera night")] [SerializeField]
    private GameObject mimicChestPrefab;
    
    public int NightCycleStep { get; private set; } = 1;
    
    public VillageNpcRuntime CurrentHeroNpc { get; private set; }
    
    public int CurrentObjectiveProgress { get; private set; }
    
    public int CurrentObjectiveTarget =>
        CurrentMoon != null
            ? CurrentMoon.RequiredAmount
            : 0;
    
    private NightLocationSO _lastNightLocation;
    
    public static Action OnHordeStarted;
    public static Action<int> OnHordeFinished;
    public Action<int, int> OnObjectiveProgressChanged;
    public static Action<Transform> OnExitSpawned;
    public static Action OnExitRemoved;
    
    private void Start()
    {
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.OnInventoryChanged += RefreshObjective;
        }
        
        CorruptedVillager.OnCorruptedVillagerKilled += RefreshObjective;
    }

    private void OnDisable()
    {
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.OnInventoryChanged -= RefreshObjective;
        }
        
        CorruptedVillager.OnCorruptedVillagerKilled -= RefreshObjective;
    }
    
    private void SavePreviousScene()
    {
        _previousScene = SceneManager.GetActiveScene().name;
    }
    
    public void PrepareHorde()
    {
        if (_hordePrepared)
        {
            Debug.Log("Horde is prepared");
            return;
        }
        CurrentMoon = MoonManager.Instance.CurrentMoon;
        
        _hordePrepared = true;
        
        PreparedData = hordeConfig.GetHorde(currentHorde - 1);
        PreparedMutation = GetRandomMutation();

        GenerateNightLocation();
    }
    
    private void GenerateNightLocation()
    {
        var pool = NightCycleStep == 3
            ? _nightDatabase.HeroNights
            : _nightDatabase.NormalNights;
            
        if (pool == null || pool.Count == 0)
        {
            CurrentNightLocation = null;
            return;
        }
        
        var availableLocations = pool
            .Where(x => x != _lastNightLocation)
            .ToList();

        if (availableLocations.Count == 0)
        {
            availableLocations = pool.ToList();
        }

        CurrentNightLocation =
            availableLocations[Random.Range(0, availableLocations.Count)];
        
        _lastNightLocation = CurrentNightLocation;

        Debug.Log($"SELECTED NIGHT: {CurrentNightLocation.name}");
        Debug.Log($"SCENE TO LOAD: {CurrentNightLocation.SceneName}");
    }

    public void StartHorde()
    {
        StopNight();
        
        SavePreviousScene();

        Debug.Log($"Starting Horde {currentHorde}");

        if (CurrentNightLocation == null)
        {
            Debug.LogError("No night location selected!");
            return;
        }

        Debug.Log($"LOADING SCENE: {CurrentNightLocation.SceneName}");

        LoadingSceneManager.Instance.LoadScene(
            CurrentNightLocation.SceneName,
            true
        );

        StartCoroutine(WaitForSceneAndSpawn());
        SoundManager.Instance.PlayCombatMusic(1f);
    }

    private System.Collections.IEnumerator WaitForSceneAndSpawn()
    {
        yield return null;

        yield return new WaitUntil(() =>
            FindObjectsOfType<EnemySpawner>().Length > 0 ||
            FindObjectsOfType<EnemyObjectiveSpawner>().Length > 0
        );

        yield return new WaitForSeconds(0.2f);
        
        LootSpawnManager.Instance.SpawnAll();
        SpawnObjectiveItems();
        SpawnNPC();
        SpawnHorde();
    }
    
    public void CleanupEnemies()
    {
        var enemies = FindObjectsOfType<EnemyStatistics>();

        foreach (var enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
    }
    
    public void OnPlayerExit()
    {
        SoundManager.Instance.StopCombatMusic();

        StopNight();

        CleanupEnemies();
        
        CompleteHorde();
    }
    
    private void SpawnHorde()
    {
        var spawners = FindObjectsOfType<EnemySpawner>();

        if (spawners.Length == 0)
        {
            Debug.LogWarning("No EnemySpawners found!");
        }

        _objectiveCompleted = false;
        CurrentObjectiveProgress = 0;

        OnObjectiveProgressChanged?.Invoke(
            CurrentObjectiveProgress,
            CurrentObjectiveTarget
        );

        _rescuedNpcCount = 0;
        
        var data = PreparedData;
        // _currentObjective = data.objective;
        _currentObjective = CurrentNightLocation.IsBossArena
            ? HordeObjective.BossArena
            : HordeObjective.NightExploration;
        
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
                _nightRoutine = StartCoroutine(StartNightExploration(spawners, data));
                break;
            
            case HordeObjective.BossArena:
                _nightRoutine = StartCoroutine(StartBossArena(spawners, data));
                break;
            
            case HordeObjective.KillAll:
            default:
                StartCoroutine(SpawnHordeRoutine(spawners, data));
                break;
        }
        
        OnHordeStarted?.Invoke();
    }

    private IEnumerator StartBossArena(
        EnemySpawner[] spawners,
        HordeData data)
    {
        _isNightRunning = true;
        _bossAlive = false;

        Debug.Log("Boss Arena Started");

        _aliveEnemies = 0;

        SpawnBoss(data);

        var spawnTimer = 0f;
        var spawnInterval = 4f;

        while (_isNightRunning)
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnInterval)
            {
                if (_aliveEnemies < 20)
                {
                    SpawnEnemyNearPlayer(spawners, data);
                }

                spawnTimer = 0f;
            }

            yield return null;
        }
    }
    
    private void SpawnObjectiveItems()
    {
        if (CurrentNightLocation != null && CurrentNightLocation.IsBossArena)
            return;
        
        if (CurrentMoon == null)
            return;

        switch (CurrentMoon.ObjectiveType)
        {
            case MoonObjectiveType.CollectKnowledge:
            {
                if (CurrentMoon.RequiredItem.item == null)
                    return;

                LootSpawnManager.Instance.SpawnObjectiveItems(
                    CurrentMoon.RequiredItem.item.ItemToDrop,
                    CurrentObjectiveTarget
                );

                break;
            }

            case MoonObjectiveType.DestroyCorruptedTrees:
                SpawnCorruptedVillagers();
                break;
            
            case MoonObjectiveType.ActivateObelisks:
                SpawnObelisks();
                break;
            
            case MoonObjectiveType.FindMimics:
                SpawnMimicChests();
                break;
        }
    }

    private void SpawnMimicChests()
    {
        var spawners = FindObjectsOfType<EnemyObjectiveSpawner>().ToList();

        Shuffle(spawners);

        var count = Mathf.Min(6, spawners.Count);
        
        var mimicCount = Mathf.Min(CurrentObjectiveTarget, count);

        var mimicIndexes = new List<int>();

        while (mimicIndexes.Count < mimicCount)
        {
            var random = Random.Range(0, count);

            if (!mimicIndexes.Contains(random))
            {
                mimicIndexes.Add(random);
            }
        }

        for (var i = 0; i < count; i++)
        {
            var chest = Instantiate(
                mimicChestPrefab,
                spawners[i].transform.position,
                Quaternion.identity
            );

            var mimicChest = chest.GetComponent<MimicChest>();

            if (mimicChest != null)
            {
                mimicChest.Initialize(
                    mimicIndexes.Contains(i)
                );
            }
        }

        Debug.Log(
            $"Spawned {count} chests | Mimics: {CurrentObjectiveTarget}"
        );
    }

    private void GenerateHeroNight()
    {
        var rescued = WorldManager.Instance.RescuedNpcs;

        var available = _npcDatabase.NpcDatas
            .Where(npcData =>
                rescued.All(r => r.Data != npcData))
            .ToList();

        if (available.Count == 0)
        {
            CurrentHeroNpc = null;
            return;
        }

        var randomNpc = available[Random.Range(0, available.Count)];

        CurrentHeroNpc = new VillageNpcRuntime(randomNpc);
    }
    
    private void AdvanceNightCycle()
    {
        NightCycleStep++;

        if (NightCycleStep > 3)
        {
            NightCycleStep = 1;
            CurrentHeroNpc = null;
        }

        if (NightCycleStep == 3)
        {
            GenerateHeroNight();
        }
    }

    #region NightExploration
    private IEnumerator StartNightExploration(EnemySpawner[] spawners, HordeData data)
    {
        _isNightRunning = true;

        var spawnTimer = 0f;
        var spawnInterval = 3f;

        var elapsed = 0f;
        var speedIncreaseTimer = 0f;

        _aliveEnemies = 0;

        // SpawnExit();
        while (_isNightRunning)
        {
            elapsed += Time.deltaTime;
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnInterval)
            {
                PointsManager.Instance.AddScore(3);
                if (_aliveEnemies < 50)
                {
                    SpawnEnemyNearPlayer(spawners, data);
                }

                spawnTimer = 0f;
            }

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

        Debug.Log("Night Exploration Stopped");
    }
    
    public void StopNight()
    {
        _isNightRunning = false;

        if (_nightRoutine != null)
        {
            StopCoroutine(_nightRoutine);
            _nightRoutine = null;
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

        if (_spawnedExit != null) return;
        
        _spawnedExit = Instantiate(exitPrefab, chosen.transform.position, Quaternion.identity);

        OnExitSpawned?.Invoke(_spawnedExit.transform);
        
        Debug.Log("Exit spawned!");
    }

    private void RemoveExit()
    {
        if (_spawnedExit == null) return;

        OnExitRemoved?.Invoke();
        
        Destroy(_spawnedExit);

        _spawnedExit = null;

        Debug.Log("Exit removed!");
    }
    
    private void SpawnBoss(HordeData data)
    {
        if (_bossAlive)
            return;

        var spawners = FindObjectsOfType<BossSpawner>();

        if (spawners == null || spawners.Length == 0)
        {
            Debug.LogWarning("No BossSpawner found in scene!");
            return;
        }

        var randomSpawner = spawners[Random.Range(0, spawners.Length)];

        var prefab = GetRandomEnemy(bossEnemies);

        var bossGO = Instantiate(
            prefab,
            randomSpawner.transform.position,
            randomSpawner.transform.rotation
        );

        SetupEnemy(bossGO, data, bossEnemies);

        _bossAlive = true;

        var stats = bossGO.GetComponent<EnemyStatistics>();

        if (stats != null)
        {
            NPCInfoManager.Instance.ShowNpcInfo(stats);
        }
    }
    
    public void SpawnMimicEnemy(Vector3 position)
    {
        var pool = GetEnemyPool();

        var prefab = GetRandomEnemy(pool);

        var enemyGO = Instantiate(
            prefab,
            position,
            Quaternion.identity
        );

        SetupEnemy(
            enemyGO,
            PreparedData,
            pool
        );
    }
    
    private void SpawnEnemyNearPlayer(EnemySpawner[] spawners, HordeData data)
    {
        if (spawners != null && spawners.Length > 0)
        {
            var spawner = spawners[Random.Range(0, spawners.Length)];

            var pool = GetEnemyPool();
            var prefab = GetRandomEnemy(pool);

            var enemyGO = Instantiate(
                prefab,
                spawner.spawnPoint.position,
                Quaternion.identity
            );

            SetupEnemy(enemyGO, data, pool);

            return;
        }
        
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
    
    private RescueNpc SpawnHeroNpc()
    {
        var spawners = FindObjectsOfType<NpcSpawner>();

        if (spawners.Length == 0)
        {
            Debug.LogWarning("No NPC spawners found!");
            return null;
        }

        if (SelectedNpc == null)
        {
            Debug.LogWarning("No hero selected!");
            return null;
        }

        var chosenSpawner = spawners[Random.Range(0, spawners.Length)];

        var npcGO = Instantiate(
            SelectedNpc.Data.Character,
            chosenSpawner.spawnPoint.position,
            Quaternion.identity
        );

        var rescue = npcGO.GetComponent<RescueNpc>();

        if (rescue != null)
        {
            rescue.SetRuntime(SelectedNpc);
        }

        Debug.Log($"Boss reward hero spawned: {SelectedNpc.Name}");

        return rescue;
    }
    
    private void SpawnExitNear(Vector3 position)
    {
        var offset = Random.insideUnitSphere * 3f;

        offset.y = 0f;

        var spawnPos = position + offset;

        if (NavMesh.SamplePosition(
                spawnPos,
                out NavMeshHit hit,
                5f,
                NavMesh.AllAreas))
        {
            Instantiate(
                exitPrefab,
                hit.position,
                Quaternion.identity
            );

            Debug.Log("Exit spawned near hero NPC");
        }
        else
        {
            Instantiate(
                exitPrefab,
                position,
                Quaternion.identity
            );

            Debug.LogWarning("Fallback exit spawn used");
        }
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

            var hordeMultiplier = GetHordeMultiplier();
            
            stats.ApplyHordeScaling(
                data.hpMultiplier * hordeMultiplier * CurrentMoon.EnemyHealthMultiplier,
                data.damageMultiplier * hordeMultiplier * CurrentMoon.EnemyDamageMultiplier,
                1f * hordeMultiplier * CurrentMoon.EnemySpeedMultiplier,
                pool == eliteEnemies,
                pool == bossEnemies
            );

            ApplyMutation(stats);
        }

        _aliveEnemies++;
    }
    
    private List<GameObject> GetEnemyPool()
    {
        return Random.value < CurrentMoon.EliteChanceBonus
            ? normalEnemies
            : eliteEnemies;
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
    
    public void SetRescuedNPC(VillageNpcRuntime npc)
    {
        _rescuedNpcCount++;
        
        PointsManager.Instance.AddScore(100);
        WorldManager.Instance.AddNpc(npc);
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
        var goldForEnemy = GetGoldForEnemy(isBoss, isElite);
        
        if (_currentObjective == HordeObjective.BossArena && isBoss)
        {
            _bossAlive = false;

            CombatStatsManager.Instance.BossEnemiesKilled++;

            PointsManager.Instance.AddScore(1000);
            StartCoroutine(FinalKillSequence());

            Debug.Log("Boss defeated!");

            var hero = SpawnHeroNpc();

            if (hero != null)
            {
                SpawnExit();
            }

            return;
        }
        
        if (_currentObjective != HordeObjective.BossArena &&
            CurrentMoon.ObjectiveType == MoonObjectiveType.KilledEnemies)
        {
            AddObjectiveProgress(1);
        }
        
        if (isElite)
        {
            CombatStatsManager.Instance.EliteEnemiesKilled++;
            PointsManager.Instance.AddScore(100);
        }
        else
        {
            CombatStatsManager.Instance.NormalEnemiesKilled++;
            PointsManager.Instance.AddScore(50);
        }
        
        CombatStatsManager.Instance.GoldEarned += goldForEnemy;
        
        FloatingTextManager.Instance.ShowGoldText(goldForEnemy, Player.Instance.transform);
        
        if (_currentObjective == HordeObjective.DefendObject) return;
        
        _aliveEnemies--;

        Debug.Log($"Enemy killed. Remaining: {_aliveEnemies}");

        if (_aliveEnemies <= 0)
        {
            StartCoroutine(FinalKillSequence());
            Debug.Log("All enemies defeated!");
        }
    }

    private int GetGoldForEnemy(bool isBoss, bool isElite)
    {
        if (isBoss)
            return (int)Mathf.Ceil(RNGManager.Instance.GetRandomInt(50, 100) * CurrentMoon.GoldMultiplier);
        if (isElite)
            return (int)Mathf.Ceil(RNGManager.Instance.GetRandomInt(10, 20) * CurrentMoon.GoldMultiplier);
            
        return (int)Mathf.Ceil(RNGManager.Instance.GetRandomInt(4, 8) * CurrentMoon.GoldMultiplier);
    }
    
    private GameObject GetRandomEnemy(List<GameObject> enemyList)
    {
        return enemyList[Random.Range(0, enemyList.Count)];
    }

    private void CompleteHorde()
    {
        SoundManager.Instance.PlayWinMusic();
        
        Debug.Log($"Horde {currentHorde} completed");
        AdvanceNightCycle();
        _hordePrepared = false;

        if (NightCycleStep == 1)
        {
            CurrentHeroNpc = null;
        }
        
        InventoryController.Instance.ChangeGoldAmount(hordeConfig.GetHorde(currentHorde - 1).goldReward + CombatStatsManager.Instance.GoldEarned);
        currentHorde++;
        enemiesPerHorde += enemiesIncreasePerHorde;
        PointsManager.Instance.AddScore(100);

        if (currentHorde > 9)
        {
            var points = PointsManager.Instance.GetCurrentScore();
            DeathScreenManager.Instance.ShowWinScreen(points);
            return;
        }
        
        OnHordeFinished?.Invoke(currentHorde - 1);
    }

    private void FailHorde()
    {
        Debug.Log("Player died - Game Over");
        StopNight();
        _hordePrepared = false;

        // TODO: Game Over screen
        // SceneManager.LoadScene("MainMenu");
        ReturnToPreviousScene();
    }

    public void ReturnToPreviousScene()
    {
        SoundManager.Instance.StopCombatMusic();
        
        if (string.IsNullOrEmpty(_previousScene))
        {
            Debug.LogWarning("No previous scene saved!");
            return;
        }

        LoadingSceneManager.Instance.LoadScene(_previousScene, true);
        
        var cycle = FindObjectOfType<DayNightCycle>();
        if (cycle != null)
            cycle.ResetCycle();

        ToastrPanelManager.Instance.Show("SAVING");
        SaveLoadManager.Instance.Save();
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
                // stats.Damage *= 1.5f;
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
        var spawners = FindObjectsOfType<NpcSpawner>().ToList();

        var isBossArena = CurrentNightLocation != null &&
                          CurrentNightLocation.IsBossArena;
        
        if (spawners.Count == 0)
        {
            Debug.LogWarning("No NpcSpawners found!");
        }

        Shuffle(spawners);

        var npcsToSpawn = new List<VillageNpcRuntime>();

        if (!isBossArena && SelectedNpc != null)
        {
            Debug.LogError("NPC SPAWNED");
            //npcsToSpawn.Add(SelectedNpc);
        }
        else
        {
            Debug.LogWarning("No hero selected or is in boss arena!");
        }

        var workerCount = isBossArena
            ? 0
            : Random.Range(1, 3);
        
        var availableWorkers = workerPool
            .Where(w => !WorldManager.Instance.RescuedNpcs.Any(r => r.Data == w))
            .ToList();

        Shuffle(availableWorkers);

        for (var i = 0; i < workerCount && i < availableWorkers.Count; i++)
        {
            npcsToSpawn.Add(new VillageNpcRuntime(availableWorkers[i]));
        }

        for (var i = 0; i < npcsToSpawn.Count; i++)
        {
            if (i >= spawners.Count)
            {
                Debug.LogWarning("Not enough NPC spawners!");
                break;
            }

            var npc = npcsToSpawn[i];
            var chosen = spawners[i];

            var npcGO = Instantiate(
                npc.Data.Character,
                chosen.spawnPoint.position,
                Quaternion.identity
            );

            var rescue = npcGO.GetComponent<RescueNpc>();

            if (rescue != null)
            {
                rescue.SetRuntime(npc);
            }

            Debug.Log($"Spawned NPC: {npc.Name} | Type: {npc.Data.Type}");
        }

        _spawnedNpcsThisRun = npcsToSpawn;
    }
    
    public int NpcRescuedCount() => _rescuedNpcCount;
    
    private float GetHordeMultiplier()
    {
        return 1f + (currentHorde - 1) * 0.02f;
    }
    
    public void AddObjectiveProgress(int amount = 1)
    {
        var oldProgress = CurrentObjectiveProgress;
        
        CurrentObjectiveProgress += amount;

        CheckObjectiveComplete(oldProgress);
    }
    
    private void RefreshObjective()
    {
        if (CurrentMoon == null) return;
        if (_currentObjective == HordeObjective.BossArena) return;
        
        CheckObjectiveComplete();
    }
    
    private void CheckObjectiveComplete(int oldProgress = -1)
    {
        if (CurrentMoon == null)
            return;

        // SPRAWDZIĆ CZY JAK SIE ZBUDUJE, ODPALI, POGRA, KUPI RZECZY W SKLEPIE, zarobi kase i zrestartuje, to czy zawsze zostaje to samo
            // i dołożyć losowanie startowej broni
        
        switch (CurrentMoon.ObjectiveType)
        {
            case MoonObjectiveType.KilledEnemies:
            {
                CurrentObjectiveProgress = Mathf.Clamp(
                    CurrentObjectiveProgress,
                    0,
                    CurrentObjectiveTarget
                );

                break;
            }

            case MoonObjectiveType.CollectKnowledge:
            {
                var amount =
                    InventoryController.Instance.GetItemCount(
                        CurrentMoon.RequiredItem
                    );

                CurrentObjectiveProgress = amount;

                break;
            }
            
            case MoonObjectiveType.DestroyCorruptedTrees:
            {
                CurrentObjectiveProgress = _aliveTrees - FindObjectsOfType<CorruptedVillager>().Length;
                break;
            }
            case MoonObjectiveType.ActivateObelisks:
                CurrentObjectiveProgress = _activatedObelisks;
                break;
            
            
            case MoonObjectiveType.FindMimics:
                //Nic nie dajemy bo progres jest robiony w MimicChest.cs
            default:
                break;
        }

        if (oldProgress != CurrentObjectiveProgress)
        {
            OnObjectiveProgressChanged?.Invoke(
                CurrentObjectiveProgress,
                CurrentObjectiveTarget
            );
        }

        var completed =
            CurrentObjectiveProgress >= CurrentObjectiveTarget;

        if (completed && !_objectiveCompleted)
        {
            _objectiveCompleted = true;
            SpawnExit();
        }
        else if (!completed && _objectiveCompleted)
        {
            _objectiveCompleted = false;
            RemoveExit();
            CurrentObjectiveProgress = 0;
        }
    }
    
    private void SpawnCorruptedVillagers()
    {
        var spawners = FindObjectsOfType<EnemyObjectiveSpawner>().ToList();
        
        Shuffle(spawners);

        var count = Mathf.Min(3, spawners.Count);

        _aliveTrees = count;

        for (var i = 0; i < count; i++)
        {
            var enemy = Instantiate(
                corruptedVillager,
                spawners[i].transform.position,
                Quaternion.identity
            );
            
            enemy.GetComponent<EnemyStatistics>()?.Initialize();
        }
    }
    
    private void Shuffle<T>(List<T> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
    
    private void SpawnObelisks()
    {
        var spawners = FindObjectsOfType<EnemyObjectiveSpawner>().ToList();

        Shuffle(spawners);

        var count = Mathf.Min(3, spawners.Count);

        _spawnedObelisks = count;
        _activatedObelisks = 0;

        for (var i = 0; i < count; i++)
        {
            Instantiate(
                obeliskPrefab,
                spawners[i].transform.position,
                Quaternion.identity
            );
        }
    }
    
    public void OnObeliskActivated()
    {
        _activatedObelisks++;

        AddObjectiveProgress(1);

        Debug.Log($"Activated obelisks: {_activatedObelisks}/{_spawnedObelisks}");
    }

    public bool IsBossAlive() => _bossAlive;

    #region Save/Load

    public void Save()
    {
        var settings = SaveLoadManager.Instance.GetSettings();
        
        ES3.Save("currentHorde", currentHorde, settings);
        ES3.Save("nightCycleStep", NightCycleStep, settings);
        ES3.Save("enemiesPerHorde", enemiesPerHorde, settings);
    }

    public void Load()
    {
        if(ES3.KeyExists("currentHorde"))
            currentHorde = ES3.Load<int>("currentHorde");
        if(ES3.KeyExists("nightCycleStep"))
            NightCycleStep = ES3.Load<int>("nightCycleStep");
        if(ES3.KeyExists("enemiesPerHorde"))
            enemiesPerHorde = ES3.Load<int>("enemiesPerHorde");
        
        if (NightCycleStep == 3)
        {
            GenerateHeroNight();
        }
    }
    
    #endregion

}