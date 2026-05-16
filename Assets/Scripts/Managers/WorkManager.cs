using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class WorkManager : Singleton<WorkManager>
{
    private float _timer;
    private WorkerPoint[] _points;

    [SerializeField] private ItemParameterSO damageBonusParameter;

    [SerializeField] private InventoryItem potion;
    
    private int _usedBlacksmithUpgrades;
    private const int UpgradeDamageBonus = 2;
    private const int UpgradeCostStep = 50;
    
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

    public void ProcessWorkersAfterNight()
    {
        ProcessScavengers();
        ProcessAlchemists();

        _usedBlacksmithUpgrades = 0;
    }

    private ChestInteraction GetWorkerChest(WorkerJob job)
    {
        var workerChest = FindObjectsOfType<WorkerChest>()
            .FirstOrDefault(c => c.workerJob == job);

        return workerChest?.GetComponent<ChestInteraction>();
    }

    private InventoryRuntime TryGetChestInventory(WorkerJob job)
    {
        var chest = GetWorkerChest(job);

        if (chest == null)
        {
            Debug.LogWarning("Worker chest not found!");
            return null;
        }

        return chest.GetRuntimeInventory();
    }
    
    public void UpgradeItem(int itemIndex)
    {
        if (!CanUpgrade())
        {
            FloatingTextManager.Instance.ShowWarningText(
                "No upgrades remaining!",
                Player.Instance.transform
            );

            return;
        }

        var inventory = InventoryController.Instance.inventoryData;

        if (inventory == null)
            return;

        if (itemIndex < 0 || itemIndex >= inventory.inventoryItems.Count)
            return;

        var item = inventory.inventoryItems[itemIndex];

        if (item.IsEmpty)
            return;

        if (item.item is not WeaponItemSO)
            return;

        var cost = GetUpgradeCost(item);

        if (!InventoryController.Instance.ChangeGoldAmount(-cost))
        {
            FloatingTextManager.Instance.ShowWarningText(
                "Not enough gold!",
                Player.Instance.transform
            );

            return;
        }

        UpgradeWeapon(ref item);

        inventory.inventoryItems[itemIndex] = item;

        inventory.NotifyInventoryUpdated();

        _usedBlacksmithUpgrades++;

        Debug.Log(
            $"Upgrade used: {_usedBlacksmithUpgrades}/" +
            $"{GetWorkersCount(WorkerJob.Blacksmith)}"
        );
    }
    
    public int GetRemainingUpgrades()
    {
        var smithCount = GetWorkersCount(WorkerJob.Blacksmith);

        return Mathf.Max(
            0,
            smithCount - _usedBlacksmithUpgrades
        );
    }
    
    public bool CanUpgrade()
    {
        return GetRemainingUpgrades() > 0;
    }
    
    private void UpgradeWeapon(ref InventoryItem item)
    {
        item.itemState ??= new List<ItemParameter>();

        var found = false;

        for (var i = 0; i < item.itemState.Count; i++)
        {
            if (item.itemState[i].itemParameter == damageBonusParameter)
            {
                var param = item.itemState[i];

                param.value += UpgradeDamageBonus;

                item.itemState[i] = param;

                found = true;
                break;
            }
        }

        if (!found)
        {
            item.itemState.Add(new ItemParameter()
            {
                itemParameter = damageBonusParameter,
                value = UpgradeDamageBonus
            });
        }

        Debug.Log(
            $"{item.item.Name} upgraded by +{UpgradeDamageBonus} damage"
        );
    }
    
    public int GetUpgradeLevel(InventoryItem item)
    {
        if (item.itemState == null)
            return 0;

        var damageParam = item.itemState.FirstOrDefault(
            x => x.itemParameter == damageBonusParameter
        );

        if (damageParam.itemParameter == null)
            return 0;

        return Mathf.RoundToInt(
            damageParam.value / UpgradeDamageBonus
        );
    }
    
    public int GetUpgradeCost(InventoryItem item)
    {
        var nextLevel = GetUpgradeLevel(item) + 1;

        return nextLevel * UpgradeCostStep;
    }
    
    public int GetNextUpgradeBonus()
    {
        return UpgradeDamageBonus;
    }

    private void ProcessAlchemists()
    {
        var alchemistsCount = GetWorkersCount(WorkerJob.Alchemist);

        if (alchemistsCount <= 0)
            return;

        var inventory = TryGetChestInventory(WorkerJob.Alchemist);

        if (inventory == null)
            return;
        
        foreach (var item in inventory.Items)
        {
            if (!item.IsEmpty)
                continue;

            inventory.AddItem(potion, alchemistsCount);
            break;
        }

        inventory.NotifyInventoryUpdated();    
    }

    private void ProcessScavengers()
    {
        var count = GetWorkersCount(WorkerJob.Scavenger);
        var gold = Random.Range(10, 20) * count;
        InventoryController.Instance.ChangeGoldAmount(gold);
        
        Debug.Log($"Scavengers found {gold} gold");
    }

    private int GetWorkersCount(WorkerJob job) => WorldManager.Instance.RescuedNpcs.Count(npc => npc.IsWorker && npc.CurrentJob == job);

    public bool TryAssignWorker(VillageNpcRuntime npc, WorkerJob newJob)
    {
        Debug.Log($"{npc.Name} | {npc.RuntimeID} | {npc.CurrentJob}");
        
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