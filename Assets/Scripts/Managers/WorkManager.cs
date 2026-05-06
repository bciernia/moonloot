using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorkManager : Singleton<WorkManager>
{
    private float _timer;
    private WorkerPoint[] _points;

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void RefreshPoints()
    {
        _points = FindObjectsOfType<WorkerPoint>()
            .Where(p => p != null)
            .ToArray();
    }

    private void Update()
    {
        if (!LoadingSceneManager.Instance.IsSceneTown())
            return;
        
        _timer += Time.deltaTime;

        if (_timer >= 1f)
        {
            Tick();
            _timer = 0f;
        }
    }

    private void Tick()
    {
        foreach (var npc in WorldManager.Instance.RescuedNpcs)
        {
            if (!npc.IsWorker) continue;

            switch (npc.CurrentJob)
            {
                case WorkerJob.FoodProduction:
                    ProduceFood(npc);
                    break;

                case WorkerJob.Lumber:
                    ProduceWood(npc);
                    break;
                
                case WorkerJob.None:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void ProduceFood(VillageNpcRuntime npc)
    {
        Debug.Log($"{npc.Name} produkuje jedzenie");
    }

    private void ProduceWood(VillageNpcRuntime npc)
    {
        Debug.Log($"{npc.Name} produkuje drewno");
    }

    public bool TryAssignWorker(VillageNpcRuntime npc, WorkerJob newJob)
    {
        EnsurePoints();
        
        if (!npc.IsWorker)
            return false;

        ReleaseCurrentPoint(npc);

        if (newJob == WorkerJob.None)
        {
            MoveNpcToFreePoint(npc);
            npc.CurrentJob = newJob;
            return true;
        }

        var freePoint = _points
            .FirstOrDefault(p => p.JobType == newJob && !p.IsOccupied);

        if (freePoint == null)
        {
            Debug.Log($"Brak miejsca dla {newJob}");
            return false;
        }

        freePoint.IsOccupied = true;

        MoveNpcToPoint(npc, freePoint);

        npc.CurrentJob = newJob;

        return true;
    }

    private void MoveNpcToPoint(VillageNpcRuntime npc, WorkerPoint point)
    {
        var npcGO = FindNpc(npc);

        if (npcGO == null) return;

        npcGO.transform.position = point.transform.position;
    }

    private void MoveNpcToFreePoint(VillageNpcRuntime npc)
    {
        var freePoint = _points
            .FirstOrDefault(p => p.JobType == WorkerJob.None && !p.IsOccupied);

        if (freePoint == null)
        {
            Debug.LogWarning("No free worker point!");
            return;
        }

        freePoint.IsOccupied = true;

        MoveNpcToPoint(npc, freePoint);
    }

    private GameObject FindNpc(VillageNpcRuntime npc)
    {
        var all = FindObjectsOfType<RescueNpc>();

        foreach (var r in all)
        {
            if (r.GetRuntime() == npc)
                return r.gameObject;
        }

        return null;
    }

    private void ReleaseCurrentPoint(VillageNpcRuntime npc)
    {
        var npcGO = FindNpc(npc);

        if (npcGO == null) return;

        var point = _points.FirstOrDefault(p =>
            Vector3.Distance(p.transform.position, npcGO.transform.position) < 0.1f);

        if (point != null)
        {
            point.IsOccupied = false;
        }
    }
    
    private void EnsurePoints()
    {
        if (_points == null || _points.Length == 0)
        {
            RefreshPoints();
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!LoadingSceneManager.Instance.IsSceneTown())
            return;

        StartCoroutine(RefreshNextFrame());
    }

    private IEnumerator RefreshNextFrame()
    {
        yield return null;
        RefreshPoints();
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}