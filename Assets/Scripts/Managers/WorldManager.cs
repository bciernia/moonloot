using System.Collections.Generic;
using UnityEngine;

public class WorldManager : Singleton<WorldManager>
{
    private List<NPCData> _rescuedNPCs = new();
    private Dictionary<NPCData, string> _npcPlacements = new();

    public IReadOnlyList<NPCData> RescuedNpcs => _rescuedNPCs;

    private HashSet<string> _rescuedNpcIDs = new();
    
    public void AddNpc(VillageNpcRuntime npc)
    {
        if (_rescuedNPCs.Contains(npc.Data)) return;
        
        _rescuedNPCs.Add(npc.Data);
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
            var point = System.Array.Find(points, p => p.ID == pointID);

            if (point == null || point.transform.childCount > 0) continue;

            Instantiate(npc.Character, point.transform.position, Quaternion.identity, point.transform);
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
