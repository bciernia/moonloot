using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
        if(!TavernBonusManager.Instance) return;
        
        var potionCount =
            TavernBonusManager.Instance.GetValue(
                TavernEffectType.FreePotion);

        if (potionCount <= 0)
            return;

        var inventory =
            TryGetChestInventory(WorkerJob.Alchemist);

        if (inventory == null)
            return;

        inventory.AddItem(potion, potionCount);

        inventory.NotifyInventoryUpdated();
    }

    private void ProcessScavengers()
    {
        if (!TavernBonusManager.Instance) return;
        
        var scavengers =
            TavernBonusManager.Instance.GetValue(
                TavernEffectType.Scavengers);

        if (scavengers <= 0)
            return;
        
        var gold = Random.Range(10, 20) * scavengers;
        InventoryController.Instance.ChangeGoldAmount(gold);
    }

    private int GetWorkersCount(WorkerJob job) => WorldManager.Instance.RescuedNpcs.Count(npc => npc.IsWorker && npc.CurrentJob == job);

    public bool TryAssignWorker(
        VillageNpcRuntime npc,
        int roomSlotId,
        WorkerJob job)
    {
        var assignedCount =
            WorldManager.Instance.RescuedNpcs.Count(
                x => x.CurrentJob == job);

        var maxWorkers =
            TavernManager.Instance.GetWorkerCapacity(job);

        if (assignedCount >= maxWorkers)
        {
            return false;
        }

        TavernManager.Instance.AssignWorkerToRoom(
            npc,
            roomSlotId);

        npc.CurrentJob = job;

        HideNpc(npc);

        return true;
    }
    
    public WorkerPoint GetSpawnPointForWorker(
        WorkerJob job)
    {
        EnsurePoints();

        var point = _points.FirstOrDefault(
            p => p.JobType == job &&
                 !p.IsOccupied);

        if (point != null)
        {
            point.IsOccupied = true;
        }

        return point;
    }
    
    public bool TryAssignWorker(
        VillageNpcRuntime npc,
        WorkerJob newJob)
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
            .FirstOrDefault(
                p => p.JobType == newJob &&
                     !p.IsOccupied);

        if (freePoint == null)
            return false;

        freePoint.IsOccupied = true;

        MoveNpcToPoint(npc, freePoint);

        npc.CurrentJob = newJob;

        return true;
    }
    
    private void HideNpc(VillageNpcRuntime npc)
    {
        var npcGO = FindNpc(npc);

        if (npcGO == null)
        {
            return;
        }

        npcGO.SetActive(false);
    }
    
    public void RemoveWorkerFromRoom(
        VillageNpcRuntime npc)
    {
        npc.CurrentJob = WorkerJob.None;

        ShowNpc(npc);
    }
    
    private void ShowNpc(VillageNpcRuntime npc)
    {
        var npcGO = FindNpc(npc);

        if (npcGO == null)
        {
            return;
        }

        npcGO.SetActive(true);
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

    public GameObject FindNpc(VillageNpcRuntime npc)
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
        if (!LoadingSceneManager.Instance.IsSceneBase())
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