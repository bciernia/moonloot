using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldManager : Singleton<WorldManager>, ISaveable
{
    private List<VillageNpcRuntime> _rescuedNPCs = new();
    private List<WorkerSaveData> _loadedWorkers;
    private Dictionary<VillageNpcRuntime, string> _npcPlacements = new();

    public IReadOnlyList<VillageNpcRuntime> RescuedNpcs => _rescuedNPCs;

    [SerializeField] private NpcDatabase _npcDatabase;

    private HashSet<string> _rescuedNpcIDs = new();
    private QuestGiverIdentityPools _questGiverIdentityPools;
    private bool _questGiverIdentityPoolsLoaded;

    [System.Serializable]
    private class QuestGiverIdentityPools
    {
        public List<string> firstNames;
        public List<string> lastNames;
        public List<string> professions;
    }
    
    public void AddNpc(VillageNpcRuntime npc)
    {
        var existingNpc = _rescuedNPCs
            .Find(x => x.RuntimeID == npc.RuntimeID);

        if (existingNpc != null)
        {
            npc.CurrentJob = existingNpc.CurrentJob;
            return;
        }
        
        if (_loadedWorkers != null)
        {
            var savedData = _loadedWorkers
                .FirstOrDefault(x => x.RuntimeID == npc.RuntimeID);

            if (savedData != null)
            {
                npc.Name = savedData.NpcName;
                npc.Profession = savedData.Profession;
                npc.CurrentJob = savedData.CurrentJob;
                npc.IsAssignedToRoom = savedData.IsAssignedToRoom;
                npc.AssignedRoomSlotId = savedData.AssignedRoomSlotId;
                npc.AssignedQuestId = savedData.AssignedQuestId;
                npc.QuestGiverName = savedData.QuestGiverName;
                npc.QuestGiverProfession = savedData.QuestGiverProfession;
            }
        }

        AssignRuntimeWorkerIdentityIfNeeded(npc);
        AssignRandomQuestIfAvailable(npc);

        _rescuedNPCs.Add(npc);
        _rescuedNpcIDs.Add(npc.RuntimeID);

        Debug.Log(
            $"Added NPC: {npc.Name} | " +
            $"{npc.RuntimeID} | " +
            $"{npc.CurrentJob}");
    }
    
    public void AssignPlacesIfNeeded()
    {
        var points = FindObjectsOfType<NPCSpawnPoint>();

        RebuildOccupiedPoints(points);

        foreach (var npc in _rescuedNPCs)
        {
            if (_npcPlacements.ContainsKey(npc)) continue;

            foreach (var point in points)
            {
                if (point.IsOccupied) continue;

                point.IsOccupied = true;
                _npcPlacements[npc] = point.ID;
                break;
            }
        }
    }
    
    public void SpawnNPCs()
    {
        var points = FindObjectsOfType<NPCSpawnPoint>();

        foreach (var (npc, pointID) in _npcPlacements)
        {
            var existingWorker =
                FindObjectsByType<WorkerNpcController>(
                        FindObjectsSortMode.None)
                    .FirstOrDefault(x => x.GetRuntime() == npc);

            if (existingWorker != null)
            {
                continue;
            }
            
            if (npc.IsAssignedToRoom)
            {
                Debug.Log(
                    $"Skipping {npc.Name} because assigned to room");
                continue;
            }
            
            if (npc.IsWorker)
            {
                StartCoroutine(SpawnWorkerNextFrame(npc));
                continue;
            }

            var point = System.Array.Find(points, p => p.ID == pointID);

            if (point == null || point.transform.childCount > 0) continue;

            var goHero = Instantiate(npc.Data.Character, point.transform.position, Quaternion.identity, point.transform);
            point.ConfigurePatrol(goHero);

            var rescueHero = goHero.GetComponent<RescueNpc>();
            if (rescueHero != null)
                rescueHero.SetRuntime(npc);

            BindQuestGiver(goHero, npc);
        }
    }
    
    public void SpawnWorker(VillageNpcRuntime npc)
    {
        StartCoroutine(SpawnWorkerNextFrame(npc));
    }
    
    private IEnumerator SpawnWorkerNextFrame(
        VillageNpcRuntime npc)
    {
        yield return null;

        var spawnPoint =
            WorkManager.Instance.GetSpawnPointForWorker(
                npc.CurrentJob);

        if (spawnPoint == null)
        {
            Debug.LogWarning(
                $"No WorkerPoint found for {npc.Name}");
            yield break;
        }

        var go = Instantiate(
            npc.Data.Character,
            spawnPoint.transform.position,
            Quaternion.identity);

        spawnPoint.ConfigurePatrol(go);

        var rescue = go.GetComponent<RescueNpc>();

        if (rescue != null)
            rescue.SetRuntime(npc);

        var worker = go.GetComponent<WorkerNpcController>();

        if (worker != null)
            worker.SetRuntime(npc);

        BindQuestGiver(go, npc);
    }

    private static void BindQuestGiver(GameObject npcObject, VillageNpcRuntime runtime)
    {
        var statUpgrade = npcObject.GetComponentInChildren<NPCStatUpgrade>(true);
        if (statUpgrade != null)
            statUpgrade.SetRuntimeNpc(runtime);

        var questGiver = npcObject.GetComponentInChildren<QuestGiver>(true);
        if (questGiver != null)
        {
            questGiver.SetRuntimeNpc(runtime);
        }
        else if (runtime != null && !string.IsNullOrWhiteSpace(runtime.AssignedQuestId))
        {
            Debug.LogWarning(
                $"NPC '{runtime.Name}' has an assigned quest but its character prefab has no QuestGiver component.",
                npcObject);
        }
    }

    private void AssignRandomQuestIfAvailable(VillageNpcRuntime npc)
    {
        if (npc == null)
            return;

        if (string.IsNullOrWhiteSpace(npc.AssignedQuestId))
        {
            var unavailableIds = new HashSet<string>(
                _rescuedNPCs
                    .Where(existing => existing != null && !string.IsNullOrWhiteSpace(existing.AssignedQuestId))
                    .Select(existing => existing.AssignedQuestId));

            var player = GameObject.FindGameObjectWithTag("Player");
            var journal = player != null ? player.GetComponent<QuestJournal>() : null;
            if (journal != null)
            {
                foreach (var status in journal.GetQuestStatuses())
                {
                    if (status != null && !string.IsNullOrWhiteSpace(status._questID))
                        unavailableIds.Add(status._questID);
                }
            }

            var availableQuests = QuestDatabase.GetRescuedNpcQuestPool()
                .Where(quest => quest != null && !unavailableIds.Contains(quest.Id))
                .ToList();

            if (availableQuests.Count == 0)
            {
                Debug.LogWarning(
                    $"No unused quest is available in the rescued NPC quest pool for {npc.Name}.",
                    this);
                return;
            }

            var selectedQuest = availableQuests[Random.Range(0, availableQuests.Count)];
            npc.AssignedQuestId = selectedQuest.Id;
            Debug.Log($"Assigned quest '{selectedQuest.GetQuestTitle()}' to rescued NPC {npc.Name}.", this);
        }

        if (QuestDatabase.Get(npc.AssignedQuestId) != null)
            AssignQuestGiverIdentityIfNeeded(npc);
    }

    private void AssignQuestGiverIdentityIfNeeded(VillageNpcRuntime npc)
    {
        LoadQuestGiverIdentityPools();

        if (string.IsNullOrWhiteSpace(npc.QuestGiverName))
            npc.QuestGiverName = npc.IsWorker
                ? npc.Name
                : GenerateUniqueQuestGiverName(npc);

        if (string.IsNullOrWhiteSpace(npc.QuestGiverProfession))
            npc.QuestGiverProfession = npc.IsWorker
                ? npc.Profession
                : GetRandomIdentityValue(
                    _questGiverIdentityPools?.professions,
                    "Keeper of Questionable Relics");
    }

    private void AssignRuntimeWorkerIdentityIfNeeded(VillageNpcRuntime npc)
    {
        if (npc == null || npc.Data == null || !npc.IsWorker)
            return;

        var needsName = string.IsNullOrWhiteSpace(npc.Name) || npc.Name == npc.Data.Name;
        var needsProfession = string.IsNullOrWhiteSpace(npc.Profession) || npc.Profession == npc.Data.Profession;
        if (!needsName && !needsProfession)
            return;

        LoadQuestGiverIdentityPools();

        if (needsName)
            npc.Name = GenerateUniqueRuntimeWorkerName(npc);

        if (needsProfession)
            npc.Profession = GetRandomIdentityValue(
                _questGiverIdentityPools?.professions,
                string.IsNullOrWhiteSpace(npc.Data.Profession) ? "Worker" : npc.Data.Profession);
    }

    private string GenerateUniqueRuntimeWorkerName(VillageNpcRuntime npc)
    {
        var firstNames = _questGiverIdentityPools?.firstNames;
        var lastNames = _questGiverIdentityPools?.lastNames;
        if (firstNames == null || firstNames.Count == 0 || lastNames == null || lastNames.Count == 0)
            return npc.Data.Name;

        var usedNames = new HashSet<string>(
            _rescuedNPCs
                .Where(existing => existing != null && existing != npc)
                .Select(existing => existing.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name)));

        var availableNames =
            (from firstName in firstNames
             where !string.IsNullOrWhiteSpace(firstName)
             from lastName in lastNames
             where !string.IsNullOrWhiteSpace(lastName)
             let fullName = $"{firstName.Trim()} {lastName.Trim()}"
             where !usedNames.Contains(fullName)
             select fullName).ToList();

        return availableNames.Count > 0
            ? availableNames[Random.Range(0, availableNames.Count)]
            : $"{firstNames[Random.Range(0, firstNames.Count)]} {lastNames[Random.Range(0, lastNames.Count)]}";
    }

    private void LoadQuestGiverIdentityPools()
    {
        if (_questGiverIdentityPoolsLoaded)
            return;

        _questGiverIdentityPoolsLoaded = true;
        var json = Resources.Load<TextAsset>("QuestGiverIdentityPool");
        if (json == null)
        {
            Debug.LogWarning("QuestGiverIdentityPool.json was not found in a Resources folder.", this);
            return;
        }

        _questGiverIdentityPools = JsonUtility.FromJson<QuestGiverIdentityPools>(json.text);
    }

    private string GenerateUniqueQuestGiverName(VillageNpcRuntime npc)
    {
        var firstNames = _questGiverIdentityPools?.firstNames;
        var lastNames = _questGiverIdentityPools?.lastNames;
        if (firstNames == null || firstNames.Count == 0 || lastNames == null || lastNames.Count == 0)
            return npc.Name;

        var usedNames = new HashSet<string>(
            _rescuedNPCs
                .Where(existing => existing != null && existing != npc)
                .Select(existing => existing.QuestGiverName)
                .Where(name => !string.IsNullOrWhiteSpace(name)));

        var availableNames =
            (from firstName in firstNames
             where !string.IsNullOrWhiteSpace(firstName)
             from lastName in lastNames
             where !string.IsNullOrWhiteSpace(lastName)
             let fullName = $"{firstName.Trim()} {lastName.Trim()}"
             where !usedNames.Contains(fullName)
             select fullName).ToList();

        return availableNames.Count > 0
            ? availableNames[Random.Range(0, availableNames.Count)]
            : $"{firstNames[Random.Range(0, firstNames.Count)]} {lastNames[Random.Range(0, lastNames.Count)]}";
    }

    private static string GetRandomIdentityValue(List<string> values, string fallback)
    {
        var availableValues = values?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();

        return availableValues != null && availableValues.Count > 0
            ? availableValues[Random.Range(0, availableValues.Count)].Trim()
            : fallback;
    }

    public void ResetSpawnPoints()
    {
        var points = FindObjectsOfType<NPCSpawnPoint>();

        foreach (var point in points)
        {
            point.IsOccupied = false;
        }
    }
    
    private void RebuildOccupiedPoints(NPCSpawnPoint[] points)
    {
        foreach (var point in points)
        {
            point.IsOccupied = false;
        }

        foreach (var (_, pointID) in _npcPlacements)
        {
            var point = System.Array.Find(points, p => p.ID == pointID);
            if (point != null)
            {
                point.IsOccupied = true;
            }
        }
    }
    
    private VillageNpcData GetNpcData(
        string dataId,
        string npcName,
        string profession)
    {
        if (!string.IsNullOrWhiteSpace(dataId))
        {
            var workerById = HordeManager.Instance.workerPool
                .FirstOrDefault(x => x != null && x.name == dataId);

            if (workerById != null)
                return workerById as VillageNpcData;

            var npcById = _npcDatabase.NpcDatas
                .FirstOrDefault(x => x != null && x.name == dataId);

            if (npcById != null)
                return npcById;
        }

        var worker = HordeManager.Instance.workerPool
            .FirstOrDefault(x =>
                x.Name == npcName &&
                x.Profession == profession);

        if (worker != null)
            return worker as VillageNpcData;

        var npcData = _npcDatabase.NpcDatas
            .FirstOrDefault(x =>
                x.Name == npcName &&
                x.Profession == profession);

        if (npcData != null)
            return npcData;

        // Older saves stored placeholder worker names. Preserve those saves
        // after replacing the placeholders with each worker's real identity.
        if (profession == "Worker")
        {
            var legacyAssetName = string.IsNullOrWhiteSpace(npcName)
                ? null
                : npcName.Trim().ToLowerInvariant() switch
                {
                    "first worker" => "VillageNPC_Worker1",
                    "second worker" => "VillageNPC_Worker2",
                    "third worker" => "VillageNPC_Worker3",
                    "fourth worker" => "VillageNPC_Worker4",
                    "fifth worker" => "VillageNPC_Worker5",
                    "sixth worker" => "VillageNPC_Worker6",
                    "seventh worker" => "VillageNPC_Worker7",
                    "eighth worker" => "VillageNPC_Worker8",
                    "ninth villager" => "VillageNPC_Worker9",
                    _ => null
                };

            if (!string.IsNullOrEmpty(legacyAssetName))
            {
                npcData = _npcDatabase.NpcDatas
                    .FirstOrDefault(x => x != null && x.name == legacyAssetName);

                if (npcData != null)
                    return npcData;

                npcData = HordeManager.Instance.workerPool
                    .FirstOrDefault(x => x != null && x.name == legacyAssetName);
            }
        }

        return npcData;
    }
    
    public List<VillageNpcRuntime> GetFreeWorkers(int count)
    {
        return _rescuedNPCs
            .Where(x =>
                x.IsWorker &&
                !x.IsAssignedToRoom)
            .Take(count)
            .ToList();
    }
    
    public void HideWorker(VillageNpcRuntime npc)
    {
        var worker = FindObjectsByType<WorkerNpcController>(
                FindObjectsSortMode.None)
            .FirstOrDefault(x => x.GetRuntime() == npc);

        if (worker != null)
        {
            worker.transform.position = new Vector3(
                9999f,
                9999f,
                9999f);
        }
    }

    public void ShowWorker(VillageNpcRuntime npc)
    {
        var worker = FindObjectsByType<WorkerNpcController>(
                FindObjectsSortMode.None)
            .FirstOrDefault(x => x.GetRuntime() == npc);

        if (worker == null)
        {
            return;
        }

        var spawnPoint = FindObjectsByType<NPCSpawnPoint>(
                FindObjectsSortMode.None)
            .FirstOrDefault(x =>
                x.ID == _npcPlacements[npc]);

        if (spawnPoint != null)
        {
            worker.transform.position =
                spawnPoint.transform.position;
        }
    }
    
    public bool HasEnoughFreeWorkers(int count)
    {
        return _rescuedNPCs.Count(x =>
            x.IsWorker &&
            !x.IsAssignedToRoom) >= count;
    }
    
    #region Save/Load
    public void Save()
    {
        var workers = new List<WorkerSaveData>();

        foreach (var npc in _rescuedNPCs)
        {
            workers.Add(new WorkerSaveData
            {
                RuntimeID = npc.RuntimeID,
                NpcDataId = npc.DataId,
                NpcName = npc.Name,
                Profession = npc.Profession,
                CurrentJob = npc.CurrentJob,
                GrantedSkillId = npc.GrantedSkill?.Id,
                AssignedQuestId = npc.AssignedQuestId,
                QuestGiverName = npc.QuestGiverName,
                QuestGiverProfession = npc.QuestGiverProfession,
                IsAssignedToRoom = npc.IsAssignedToRoom,
                AssignedRoomSlotId = npc.AssignedRoomSlotId,
            });
        }

        var settings = SaveLoadManager.Instance.GetSettings();
        
        ES3.Save("workers", workers, settings);
    }

    public void Load()
    {
        var settings = SaveLoadManager.Instance.GetSettings();
        if (!ES3.KeyExists("workers", settings))
            return;

        var savedNpcs =
            ES3.Load<List<WorkerSaveData>>("workers", settings);

        _rescuedNPCs.Clear();
        _npcPlacements.Clear();
        _loadedWorkers = savedNpcs;
        var needsSaveMigration = false;
        
        foreach (var data in savedNpcs)
        {
            Debug.Log(
                $"{data.NpcName} | " +
                $"Assigned: {data.IsAssignedToRoom} | " +
                $"Room: {data.AssignedRoomSlotId}");
            
            var npcData = GetNpcData(data.NpcDataId, data.NpcName, data.Profession);
            
            if (npcData == null)
                continue;

            var runtime = new VillageNpcRuntime(npcData);

            runtime.RuntimeID = data.RuntimeID;
            runtime.Name = data.NpcName;
            runtime.Profession = data.Profession;
            if (string.IsNullOrWhiteSpace(data.NpcDataId))
            {
                needsSaveMigration = true;

                // Older saves used the ScriptableObject's fixed identity.
                // Give those workers a runtime identity once and persist it.
                if (runtime.IsWorker)
                {
                    runtime.Name = npcData.Name;
                    runtime.Profession = npcData.Profession;
                    AssignRuntimeWorkerIdentityIfNeeded(runtime);
                    runtime.QuestGiverName = runtime.Name;
                    runtime.QuestGiverProfession = runtime.Profession;
                }
            }
            runtime.CurrentJob = data.CurrentJob;
            runtime.IsAssignedToRoom = data.IsAssignedToRoom;
            runtime.AssignedRoomSlotId = data.AssignedRoomSlotId;
            runtime.AssignedQuestId = data.AssignedQuestId;
            runtime.QuestGiverName = data.QuestGiverName;
            runtime.QuestGiverProfession = data.QuestGiverProfession;
            
            if (!string.IsNullOrEmpty(data.GrantedSkillId))
            {
                runtime.GrantedSkill =
                    SkillDatabase.Get(data.GrantedSkillId);
            }

            _rescuedNPCs.Add(runtime);
        }

        foreach (var npc in _rescuedNPCs)
        {
            AssignRandomQuestIfAvailable(npc);
        }

        if (needsSaveMigration)
            Save();

        AssignPlacesIfNeeded();
        SpawnNPCs();
        
        NPCManager.Instance.ReapplyBonuses();
        Debug.Log($"Loaded NPC count: {_rescuedNPCs.Count}");
    }
    #endregion
}
