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
                npc.CurrentJob = savedData.CurrentJob;
                npc.IsAssignedToRoom = savedData.IsAssignedToRoom;
                npc.AssignedRoomSlotId = savedData.AssignedRoomSlotId;
            }
        }

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
            
            if (npc.IsWorker)
            {
                StartCoroutine(SpawnWorkerNextFrame(npc));
                continue;
            }

            var point = System.Array.Find(points, p => p.ID == pointID);

            if (point == null || point.transform.childCount > 0) continue;

            var goHero = Instantiate(npc.Data.Character, point.transform.position, Quaternion.identity, point.transform);

            var rescueHero = goHero.GetComponent<RescueNpc>();
            if (rescueHero != null)
                rescueHero.SetRuntime(npc);
        }
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

        var rescue = go.GetComponent<RescueNpc>();

        if (rescue != null)
            rescue.SetRuntime(npc);

        var worker = go.GetComponent<WorkerNpcController>();

        if (worker != null)
            worker.SetRuntime(npc);
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
        string npcName,
        string profession)
    {
        var worker = HordeManager.Instance.workerPool
            .FirstOrDefault(x =>
                x.Name == npcName &&
                x.Profession == profession);

        if (worker != null)
            return worker as VillageNpcData;

        return _npcDatabase.NpcDatas
            .FirstOrDefault(x =>
                x.Name == npcName &&
                x.Profession == profession);
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
    
    #region Save/Load
    public void Save()
    {
        var workers = new List<WorkerSaveData>();

        foreach (var npc in _rescuedNPCs)
        {
            workers.Add(new WorkerSaveData
            {
                RuntimeID = npc.RuntimeID,
                NpcName = npc.Name,
                Profession = npc.Profession,
                CurrentJob = npc.CurrentJob,
                GrantedSkillId = npc.GrantedSkill?.Id,
                IsAssignedToRoom = npc.IsAssignedToRoom,
                AssignedRoomSlotId = npc.AssignedRoomSlotId,
            });
        }

        var settings = SaveLoadManager.Instance.GetSettings();
        
        ES3.Save("workers", workers, settings);
    }

    public void Load()
    {
        if (!ES3.KeyExists("workers"))
            return;

        var savedNpcs =
            ES3.Load<List<WorkerSaveData>>("workers");

        _rescuedNPCs.Clear();
        _npcPlacements.Clear();
        
        foreach (var data in savedNpcs)
        {
            var npcData = GetNpcData(data.NpcName, data.Profession);
            
            if (npcData == null)
                continue;

            var runtime = new VillageNpcRuntime(npcData);

            runtime.RuntimeID = data.RuntimeID;
            runtime.CurrentJob = data.CurrentJob;
            runtime.IsAssignedToRoom = data.IsAssignedToRoom;
            runtime.AssignedRoomSlotId = data.AssignedRoomSlotId;
            
            if (!string.IsNullOrEmpty(data.GrantedSkillId))
            {
                runtime.GrantedSkill =
                    SkillDatabase.Get(data.GrantedSkillId);
            }

            _rescuedNPCs.Add(runtime);
        }

        AssignPlacesIfNeeded();
        SpawnNPCs();
        
        NPCManager.Instance.ReapplyBonuses();
        Debug.Log($"Loaded NPC count: {_rescuedNPCs.Count}");
    }
    #endregion
}
