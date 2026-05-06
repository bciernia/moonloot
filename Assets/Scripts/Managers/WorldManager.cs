using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : Singleton<WorldManager>
{
    private List<VillageNpcRuntime> _rescuedNPCs = new();
    private Dictionary<VillageNpcRuntime, string> _npcPlacements = new();

    public IReadOnlyList<VillageNpcRuntime> RescuedNpcs => _rescuedNPCs;

    private HashSet<string> _rescuedNpcIDs = new();
    
    public void AddNpc(VillageNpcRuntime npc)
    {
        if (_rescuedNPCs.Contains(npc)) return;
        
        _rescuedNPCs.Add(npc);
        _rescuedNpcIDs.Add(npc.RuntimeID);
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
            if (npc.IsWorker)
            {
                var go = Instantiate(npc.Data.Character, Vector3.zero, Quaternion.identity);

                var rescue = go.GetComponent<RescueNpc>();
                if (rescue != null)
                    rescue.SetRuntime(npc);

                var worker = go.GetComponent<WorkerNpcController>();
                if (worker != null)
                    worker.SetRuntime(npc);

                if (LoadingSceneManager.Instance.IsSceneTown())
                {
                    StartCoroutine(AssignNextFrame(npc));
                }
                
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
    
    private IEnumerator AssignNextFrame(VillageNpcRuntime npc)
    {
        yield return null;

        if (WorkManager.Instance != null)
        {
            WorkManager.Instance.TryAssignWorker(npc, npc.CurrentJob);
        }
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
}
