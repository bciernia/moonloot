using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private GameObject corruptedVillager;
    
    private EnemyPoolSO CurrentEnemyPool =>
        CurrentNightLocation.EnemyPool;
    
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
    [SerializeField] private GameObject rescueCagePrefab;
    
    [SerializeField] private NpcDatabase _npcDatabase;
    [SerializeField] private MutationDatabase _mutationDatabase;
    
    [SerializeField] private NightDatabaseSO _nightDatabase;
    
    [Header("Obelisk Objective")]
    [SerializeField] private GameObject obeliskPrefab;
    
    [SerializeField]
    private GameObject searchChestPrefab;

    [SerializeField]
    private GameObject killChestPrefab;
    
    [Header("Wave count")]
    [SerializeField] private int wavesCount = 5;
    [SerializeField] private float timeBetweenWaves = 90f;
    [SerializeField] private int baseEnemiesPerWave = 25;
    [SerializeField] private int additionalEnemiesPerWave = 5;
    [SerializeField] private float spawnDelay = 0.5f;
    
    [Header("Elite chances")]
    [SerializeField] private float eliteChanceStart = 0f;
    [SerializeField] private float eliteChanceIncreasePerWave = 0.08f;
    [SerializeField] private float eliteChanceMax = 0.6f;
    
    [Header("Endless")]
    [SerializeField] private float endlessSpawnInterval = 2f;
    [SerializeField] private float endlessMinSpawnInterval = 0.25f;
    [SerializeField] private float endlessDifficultyIncreaseTime = 30f;
    [SerializeField] private float endlessDifficultyIncrease = 0.15f;
    [SerializeField] private float endlessSpeedIncrease = 0.08f;
    [SerializeField] private float endlessEliteChanceIncrease = 0.05f;
    [SerializeField] private float endlessEliteChanceMax = 0.9f;
    [SerializeField] private int endlessMaxAliveEnemies = 50;
    [SerializeField] private float bossToEndlessTime = 90f;
    [SerializeField] private float endlessSpawnIntervalDecrease = 0.15f;
    
    private int _endlessStage;
    private float _endlessTime;
    private float _bossToEndlessTimer;
    private bool _endlessStarted;
    
    private float _timeToNextWave;
    private int _currentWave;
    private bool _isWaveActive;
    private bool _isWaitingForNextWave;
    
    private int _activatedObelisks;
    private int _spawnedObelisks;
    private NightStartType _nightStartType;
    
    public NightLocationSO CurrentNightLocation { get; private set; }
    
    public HordeData PreparedData { get; private set; }
    public MutationData PreparedMutation { get; private set; }
    
    public Transform DefendTarget { get; private set; }
    public HordeObjective CurrentObjective => _currentObjective;
    public bool IsHeroNight => NightCycleStep == 3;

    private GameObject _defendTarget;
    private bool _defendActive = false;
    private const float _defendDuration = 60f;
    private const int _maxAliveEnemies = 5;

    private HordeMutation _currentMutationType;
    private bool _bossSpawned = false;
    private int _rescuedNpcCount = 0;
    
    private Coroutine _nightRoutine;
    private bool _isNightRunning;
    
    private bool _bossAlive;
    
    private bool _hordePrepared;

    private GameObject _spawnedExit;
    private bool _isExitSpawned = false;
    
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
    
    private List<NightReward> _preparedRewards = new();

    public IReadOnlyList<NightReward> PreparedRewards => _preparedRewards;
    public NightStartType NightStartType => _nightStartType;
    
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
    
    private IEnumerator HordeWaveRoutine(
        EnemySpawner[] spawners,
        HordeData data)
    {
        _isNightRunning = true;
        _currentWave = 1;
        _endlessStarted = false;

        while (_currentWave <= wavesCount)
        {
            if (_currentWave < wavesCount)
            {
                _timeToNextWave = timeBetweenWaves;

                StartCoroutine(SpawnWave(spawners, data));

                yield return StartCoroutine(WaveTimer());

                _currentWave++;

                continue;
            }

            yield return StartCoroutine(SpawnBossWave(spawners, data));

            yield return StartCoroutine(BossToEndlessTimer(spawners));

            yield break;
        }
    }
    private IEnumerator SpawnBossWave(
        EnemySpawner[] spawners,
        HordeData data)
    {
        Debug.Log("BOSS WAVE STARTED");

        // Boss
        SpawnBoss(data);

        // Od razu po pojawieniu się bossa uruchamiamy timer Endless
        StartCoroutine(BossToEndlessTimer(spawners));

        // Przeciwnicy towarzyszący bossowi
        var enemiesToSpawn = baseEnemiesPerWave +
                             (_currentWave - 1) * additionalEnemiesPerWave;

        enemiesToSpawn = Mathf.RoundToInt(enemiesToSpawn * 0.5f);

        for (var i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemyNearPlayer(spawners, data);

            yield return new WaitForSeconds(spawnDelay);
        }
    }
    
    private IEnumerator BossToEndlessTimer(EnemySpawner[] spawners)
    {
        _bossToEndlessTimer = bossToEndlessTime;

        Debug.Log(
            $"Boss spawned. Endless mode starts in {_bossToEndlessTimer} seconds."
        );

        while (_bossToEndlessTimer > 0f && !_endlessStarted)
        {
            _bossToEndlessTimer -= Time.deltaTime;

            yield return null;
        }

        if (_endlessStarted)
            yield break;

        _bossToEndlessTimer = 0f;

        StartEndlessMode(spawners);
    }
    
    private void PrepareRewards()
    {
        _preparedRewards.Clear();

        var location = CurrentNightLocation;

        if (location == null || location.Rewards == null || location.Rewards.Count == 0)
            return;

        var rewardCount = GetRewardCount(currentHorde);

        var availableRewards = location.Rewards
            .OrderBy(_ => Random.value)
            .ToList();

        for (var i = 0; i < rewardCount && i < availableRewards.Count; i++)
        {
            _preparedRewards.Add(availableRewards[i]);
        }
    }
    
    private int GetRewardCount(int night)
    {
        return Mathf.CeilToInt(night / 5f);
    }
    
    private void StartEndlessMode(EnemySpawner[] spawners)
    {
        if (_endlessStarted)
            return;

        _endlessStarted = true;

        Debug.Log("BOSS TIMER ENDED - ENDLESS MODE STARTED!");

        _isNightRunning = true;
        _endlessStage = 0;
        _endlessTime = 0f;

        StartCoroutine(EndlessRoutine(spawners));
    }
    
    private IEnumerator WaveTimer()
    {
        while (_timeToNextWave > 0f)
        {
            _timeToNextWave -= Time.deltaTime;

            yield return null;
        }

        _timeToNextWave = 0f;
    }
    
    private IEnumerator SpawnWave(
        EnemySpawner[] spawners,
        HordeData data)
    {
        var enemiesToSpawn =
            baseEnemiesPerWave +
            (_currentWave - 1) * additionalEnemiesPerWave;

        for (var i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemyNearPlayer(spawners, data);

            yield return new WaitForSeconds(spawnDelay);
        }
    }
    
    private IEnumerator WaitForNextWave()
    {
        _isWaitingForNextWave = true;

        var timer = timeBetweenWaves;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            // Później:
            // HordeTimerUI.Instance.SetTime(timer);

            yield return null;
        }

        _isWaitingForNextWave = false;
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
        PrepareRewards();
    }
    
    public void PrepareBossFight(NightLocationSO bossLocation)
    {
        StopNight();

        CurrentMoon = bossLocation.BossMoon;
        CurrentNightLocation = bossLocation;

        PreparedData = hordeConfig.GetHorde(currentHorde - 1);

        _preparedRewards.Clear();
        _hordePrepared = true;

        _nightStartType = NightStartType.Boss;
    }
    
    private void GrantRewards()
    {
        foreach (var reward in _preparedRewards)
        {
            GiveReward(reward);
        }
    }
    
    private void GiveReward(NightReward reward)
    {
        InventoryController.Instance.AddItem( new InventoryItem()
        {
            item = reward.Item,
            quantity = reward.Amount
        });
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

        // CurrentNightLocation = availableLocations[Random.Range(0, availableLocations.Count)];
        CurrentNightLocation = availableLocations[0];
        
        _lastNightLocation = CurrentNightLocation;

        Debug.Log($"SELECTED NIGHT: {CurrentNightLocation.name}");
        Debug.Log($"SCENE TO LOAD: {CurrentNightLocation.SceneName}");
    }
    
    private bool ShouldSpawnElite()
    {
        var chance = eliteChanceStart +
                     (_currentWave - 1) * eliteChanceIncreasePerWave;

        chance = Mathf.Min(chance, eliteChanceMax);

        chance += CurrentMoon.EliteChanceBonus;

        return Random.value < chance;
    }

    public void StartHorde()
    {
        StopNight();
        
        SavePreviousScene();

        Debug.Log($"Starting Horde {currentHorde}");
        _isExitSpawned = false;
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
        SoundManager.Instance.PlayCombatMusic();
    }
    
    public void StartBossFight()
    {
        StopNight();

        SavePreviousScene();

        if (CurrentNightLocation == null)
        {
            Debug.LogError("No night location selected!");
            return;
        }

        Debug.Log($"Starting Boss Fight: {CurrentNightLocation.SceneName}");

        LoadingSceneManager.Instance.LoadScene(
            CurrentNightLocation.SceneName,
            true
        );

        StartCoroutine(WaitForBossScene());
    
        SoundManager.Instance.PlayCombatMusic();
    }
    
    private IEnumerator WaitForBossScene()
    {
        yield return null;

        yield return new WaitUntil(() =>
            FindObjectsOfType<BossSpawner>().Length > 0
        );
        
        yield return new WaitForSeconds(0.2f);
        
        LootSpawnManager.Instance.SpawnAll(
            CurrentNightLocation,
            currentHorde
        );

        StartBossFightRoutine();
    }
    
    private void StartBossFightRoutine()
    {
        _isNightRunning = true;

        _bossAlive = false;

        CurrentObjectiveProgress = 0;
        
        SpawnBoss(PreparedData);
    }

    private IEnumerator WaitForSceneAndSpawn()
    {
        yield return null;

        yield return new WaitUntil(() =>
            FindObjectsOfType<EnemySpawner>().Length > 0 ||
            FindObjectsOfType<EnemyObjectiveSpawner>().Length > 0
        );

        yield return new WaitForSeconds(0.2f);
        
        LootSpawnManager.Instance.SpawnAll(CurrentNightLocation, currentHorde);
        SpawnObjectiveItems();
        SpawnSpecialChests();
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
        
        _currentMutationType = PreparedMutation.Mutation;

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

    #region NightExploration STARE
    private IEnumerator StartNightExploration(
        EnemySpawner[] spawners,
        HordeData data)
    {
        _aliveEnemies = 0;

        yield return StartCoroutine(
            HordeWaveRoutine(spawners, data)
        );

        Debug.Log("Night Exploration Stopped");
    }
    private IEnumerator StartNightExplorationOLD(EnemySpawner[] spawners, HordeData data)
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
        _isExitSpawned = true;
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

        var prefab = GetRandomEnemy(CurrentEnemyPool.BossEnemies);

        var bossGO = Instantiate(
            prefab,
            randomSpawner.transform.position,
            randomSpawner.transform.rotation
        );

        SetupEnemy(bossGO, data, CurrentEnemyPool.BossEnemies);

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
                pool == CurrentEnemyPool.EliteEnemies,
                pool == CurrentEnemyPool.BossEnemies
            );
            
            ApplyMutation(stats);
            ApplyMoonObjectiveEffects(stats);
            ApplyMoonHitEffects(stats);
            ApplyMoonAttackEffects(stats);
        }

        _aliveEnemies++;
    }
    
    private void SpawnSpecialChests()
    {
        var spawners = FindObjectsOfType<ChestSpawner>().ToList();

        if (spawners.Count < 2)
        {
            Debug.LogWarning("Need at least 2 ChestSpawners!");
            return;
        }

        Shuffle(spawners);

        Instantiate(
            searchChestPrefab,
            spawners[0].spawnPoint.position,
            Quaternion.identity);

        Instantiate(
            killChestPrefab,
            spawners[1].spawnPoint.position,
            Quaternion.identity);
    }
    
    private DeathEffectProfileSO GetRandomDeathProfile(
        IReadOnlyList<DeathEffectProfileSO> profiles)
    {
        var totalWeight = profiles.Sum(x => x.Weight);

        var roll = RNGManager.Instance.GetRandomInt(0, totalWeight);

        var current = 0;

        foreach (var profile in profiles)
        {
            current += profile.Weight;

            if (roll < current)
                return profile;
        }

        return profiles[^1];
    }

    private void ApplyMoonObjectiveEffects(
        EnemyStatistics stats)
    {
        if (CurrentMoon == null)
            return;

        var profiles = CurrentMoon.EnemyDeathProfiles;

        if (profiles == null || profiles.Count == 0)
            return;

        var profile = GetRandomDeathProfile(profiles);

        stats.SetDeathEffects(
            profile.Effects,
            profile.DamageMultiplier);
    }
    
    private AttackReactionProfileSO GetRandomAttackReactionProfile(
        IReadOnlyList<AttackReactionProfileSO> profiles)
    {
        var totalWeight = profiles.Sum(x => x.Weight);

        var roll = RNGManager.Instance.GetRandomInt(0, totalWeight);

        var current = 0;

        foreach (var profile in profiles)
        {
            current += profile.Weight;

            if (roll < current)
                return profile;
        }

        return profiles[^1];
    }

    private List<GameObject> GetEnemyPool()
    {
        return ShouldSpawnElite()
            ? CurrentEnemyPool.EliteEnemies
            : CurrentEnemyPool.NormalEnemies;
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
            pool = CurrentEnemyPool.NormalEnemies;
        else if (roll < 0.95f)
            pool = CurrentEnemyPool.EliteEnemies;
        else
            pool = CurrentEnemyPool.BossEnemies;

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
                pool == CurrentEnemyPool.EliteEnemies,
                pool == CurrentEnemyPool.BossEnemies
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
        yield return StartCoroutine(SpawnGroupRoutine(data.normalEnemies, spawners, false, false, data, CurrentEnemyPool.NormalEnemies));

        // ELITE
        yield return StartCoroutine(SpawnGroupRoutine(data.eliteEnemies, spawners, true, false, data, CurrentEnemyPool.EliteEnemies));

        // BOSS
        yield return StartCoroutine(SpawnGroupRoutine(data.bossEnemies, spawners, false, true, data, CurrentEnemyPool.BossEnemies));

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
    
    private IEnumerator EndlessRoutine(EnemySpawner[] spawners)
    {
        while (_isNightRunning)
        {
            _endlessTime += Time.deltaTime;

            _endlessStage = Mathf.FloorToInt(
                _endlessTime / endlessDifficultyIncreaseTime
            );

            var spawnInterval = Mathf.Max(
                endlessMinSpawnInterval,
                endlessSpawnInterval -
                _endlessStage * endlessSpawnIntervalDecrease
            );

            if (_aliveEnemies < endlessMaxAliveEnemies)
            {
                SpawnEndlessEnemy(spawners);

                yield return new WaitForSeconds(spawnInterval);
            }
            else
            {
                yield return null;
            }
        }
    }
    
    private List<GameObject> GetEndlessEnemyPool()
    {
        var chance = eliteChanceStart +
                     _endlessStage * endlessEliteChanceIncrease;

        chance = Mathf.Min(
            chance,
            endlessEliteChanceMax
        );

        if (CurrentMoon != null)
        {
            chance += CurrentMoon.EliteChanceBonus;
        }

        return Random.value < chance
            ? CurrentEnemyPool.EliteEnemies
            : CurrentEnemyPool.NormalEnemies;
    }
    
    private void SpawnEndlessEnemy(EnemySpawner[] spawners)
    {
        if (spawners == null || spawners.Length == 0)
        {
            Debug.LogWarning("No EnemySpawners available for endless mode!");
            return;
        }

        var pool = GetEndlessEnemyPool();
        var prefab = GetRandomEnemy(pool);

        var spawner = spawners[
            Random.Range(0, spawners.Length)
        ];

        var enemyGO = Instantiate(
            prefab,
            spawner.spawnPoint.position,
            Quaternion.identity
        );

        var stats = enemyGO.GetComponent<EnemyStatistics>();

        if (stats != null)
        {
            stats.DetectRange = 9999999;
            stats.Initialize();

            var difficultyMultiplier =
                1f + _endlessStage * endlessDifficultyIncrease;

            var speedMultiplier =
                1f + _endlessStage * endlessSpeedIncrease;

            stats.ApplyHordeScaling(
                PreparedData.hpMultiplier *
                GetHordeMultiplier() *
                difficultyMultiplier,

                PreparedData.damageMultiplier *
                GetHordeMultiplier() *
                difficultyMultiplier,

                GetHordeMultiplier() *
                speedMultiplier,

                pool == CurrentEnemyPool.EliteEnemies,
                false
            );

            ApplyMutation(stats);
            ApplyMoonObjectiveEffects(stats);
            ApplyMoonHitEffects(stats);
            ApplyMoonAttackEffects(stats);
        }

        _aliveEnemies++;
    }
    
    public void OnEnemyKilled(bool isElite, bool isBoss)
    {
        var goldForEnemy = GetGoldForEnemy(isBoss, isElite);

        if (isBoss)
        {
            _bossAlive = false;
            
            CombatStatsManager.Instance.BossEnemiesKilled++;

            PointsManager.Instance.AddScore(1000);

            Debug.Log("Boss defeated!");

            StartCoroutine(FinalKillSlowMo());

            if (!_isExitSpawned)
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

        FloatingTextManager.Instance.ShowGoldText(
            goldForEnemy,
            Player.Instance.transform
        );

        if (_currentObjective == HordeObjective.DefendObject)
            return;

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
        
        GrantRewards();
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

    public int GetRemainEnemies() => _aliveEnemies;

    private MutationData GetRandomMutation() => _mutationDatabase.Mutations[Random.Range(0, _mutationDatabase.Mutations.Count)];

    private void ApplyMutation(EnemyStatistics stats)
    {
        switch (_currentMutationType)
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
                stats.Damage *= 1.25f;
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
                
                var cage = Instantiate(
                    rescueCagePrefab,
                    npcGO.transform.position,
                    Quaternion.identity);

                cage.GetComponent<RescueCage>().Initialize(rescue);
            }

            Debug.Log($"Spawned NPC: {npc.Name} | Type: {npc.Data.Type}");
        }

        _spawnedNpcsThisRun = npcsToSpawn;
    }
    
    public int NpcRescuedCount() => _rescuedNpcCount;
    
    public float CurrentHordeMultiplier { get; private set; }
    
    private float GetHordeMultiplier()
    {
        var multiplier = 1f + (currentHorde - 1) * 0.08f;

        CurrentHordeMultiplier = multiplier;

        return multiplier;
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
            if (!_isExitSpawned)
            {
                SpawnExit();
            }
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
    
    private HitReactionProfileSO GetRandomHitReactionProfile(
        IReadOnlyList<HitReactionProfileSO> profiles)
    {
        var totalWeight = profiles.Sum(x => x.Weight);

        var roll = RNGManager.Instance.GetRandomInt(0, totalWeight);

        var current = 0;

        foreach (var profile in profiles)
        {
            current += profile.Weight;

            if (roll < current)
                return profile;
        }

        return profiles[^1];
    }
    
    private void ApplyMoonHitEffects(
        EnemyStatistics stats)
    {
        if (CurrentMoon == null)
            return;

        var profiles = CurrentMoon.EnemyHitProfiles;

        if (profiles == null || profiles.Count == 0)
            return;

        var profile = GetRandomHitReactionProfile(profiles);

        stats.SetHitReactions(profile.Effects);
    }
    
    private void ApplyMoonAttackEffects(
        EnemyStatistics stats)
    {
        if (CurrentMoon == null)
            return;

        var profiles = CurrentMoon.EnemyAttackProfiles;

        if (profiles == null || profiles.Count == 0)
            return;

        var profile = GetRandomAttackReactionProfile(profiles);

        stats.SetAttackReactions(profile.Effects);
    }

    public bool IsBossAlive() => _bossAlive;

    public int GetCurrentHordeNumber() => currentHorde;
    
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

    public float GetTimeToNextWave() => _timeToNextWave;

    public string GetCurrentWave() => _currentWave.ToString();

    public void SpawnEnemyFromBoss(Vector3 position)
    {
        if (CurrentNightLocation == null)
            return;

        var pool = CurrentNightLocation.EnemyPool;

        if (pool == null || pool.NormalEnemies == null || pool.NormalEnemies.Count == 0)
        {
            Debug.LogWarning("No normal enemies configured for current night location.");
            return;
        }

        var prefab = GetRandomEnemy(pool.NormalEnemies);

        var enemyGO = Instantiate(
            prefab,
            position,
            Quaternion.identity
        );

        SetupEnemy(
            enemyGO,
            PreparedData,
            pool.NormalEnemies
        );
    }

    public void SpawnBossElite(Vector3 position)
    {
        if (CurrentNightLocation == null)
            return;

        var pool = CurrentNightLocation.EnemyPool.EliteEnemies;

        if (pool == null)
            return;

        var enemyGO = Instantiate(
            pool[0],
            position,
            Quaternion.identity
        );

        SetupEnemy(
            enemyGO,
            PreparedData,
            pool
        );
    }
}