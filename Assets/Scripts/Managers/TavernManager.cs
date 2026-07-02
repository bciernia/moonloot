using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TavernManager : Singleton<TavernManager>, ISaveable
{
    private const string SaveKey = "TavernRooms";
    [SerializeField] private PlayerStatsSO _playerStats;

    [SerializeField] private List<TavernRoomSO> _rooms = new();
    [SerializeField] private List<TavernRoomSlot> _roomsPosition = new();
    [SerializeField]
    private List<TavernRoomData> _roomsData = new();

    public IReadOnlyList<TavernRoomSO> Rooms => _rooms;
    
    private readonly Dictionary<int, GameObject> _spawnedRooms = new();
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Base")
        {
            return;
        }
        
        RefreshSlots();
        RespawnRooms();
    }
    
    public bool IsRoomUnlocked(string roomId)
    {
        return GetRoomData(roomId) != null;
    }

    public void UnlockRoom(string roomId, int slotId)
    {
        _roomsData.Add(new TavernRoomData()
        {
            RoomId = roomId,
            RoomSlotId = slotId,
            Level = 1,
            AssignedNpcCount = 0
        });
        
        SpawnRoom(_roomsData[^1]);
    }

    public TavernRoomSO GetRoom(string roomId)
    {
        foreach (var room in _rooms)
        {
            if (room.RoomId == roomId)
            {
                return room;
            }
        }

        return null;
    }

    public TavernRoomData GetRoomData(string roomId)
    {
        foreach (var roomData in _roomsData)
        {
            if (roomData.RoomId == roomId)
            {
                return roomData;
            }
        }

        return null;
    }

    public IReadOnlyList<TavernRoomData> GetUnlockedRooms()
    {
        return _roomsData;
    }

    public bool CheckIfCanBuyRoom(TavernRoomSO room, int slotId)
    {
        if (HasRoomType(room.RoomType))
        {
            ConfirmationManager.Instance.ShowInformation("You already have this room type.");
            return false;
        }

        if (IsSlotOccupied(slotId))
        {
            ConfirmationManager.Instance.ShowInformation("This room is occupied.");
            return false;
        }
        
        if (GetRoomSlot(slotId) == null)
        {
            ConfirmationManager.Instance.ShowInformation("Invalid room slot.");
            return false;
        }
        
        if (!InventoryController.Instance.HasPlayerEnoughGold(room.Cost))
        {
            ConfirmationManager.Instance.ShowInformation("Not enough lunars to unlock this room.");
            return false;
        }
        
        if (!WorldManager.Instance.HasEnoughFreeWorkers(room.RequiredWorkers))
        {
            ConfirmationManager.Instance.ShowInformation($"Need {room.RequiredWorkers} free workers.");

            return false;
        }

        return true;
    }

    public void TryBuyRoom(TavernRoomSO room, int slotId)
    {
        var freeWorkers =
            WorldManager.Instance.GetFreeWorkers(
                room.RequiredWorkers);

        foreach (var worker in freeWorkers)
        {
            worker.IsAssignedToRoom = true;
            worker.AssignedRoomSlotId = slotId;

            WorldManager.Instance.HideWorker(worker);
        }

        InventoryController.Instance.ChangeGoldAmount(room.Cost);
        
        WorldManager.Instance.Save();
        
        UnlockRoom(room.RoomId, slotId);
        
        var roomData = GetRoomBySlot(slotId);

        if (roomData != null)
        {
            foreach (var worker in freeWorkers)
            {
                roomData.AssignedWorkers.Add(
                    worker.RuntimeID);
            }
        }
        
        Save();
    }
    
    private void SpawnRoom(TavernRoomData roomData)
    {
        if (_spawnedRooms.TryGetValue(roomData.RoomSlotId, out var roomObject))
        {
            if (roomObject != null)
            {
                return;
            }

            _spawnedRooms.Remove(roomData.RoomSlotId);
        }
        
        var room = GetRoom(roomData.RoomId);

        if (room == null)
        {
            return;
        }

        var slot = GetRoomSlot(roomData.RoomSlotId);

        if (slot == null)
        {
            return;
        }

        var spawnedRoom = Instantiate(
            room.RoomPrefab,
            slot.SpawnPoint.position,
            slot.SpawnPoint.rotation,
            slot.SpawnPoint);

        _spawnedRooms[roomData.RoomSlotId] = spawnedRoom;
    }
    
    public void RemoveRoom(int slotId)
    {
        var roomData = GetRoomBySlot(slotId);

        if (roomData == null)
        {
            return;
        }

        foreach (var workerId in roomData.AssignedWorkers.ToList())
        {
            var npc = WorldManager.Instance.RescuedNpcs
                .FirstOrDefault(x => x.RuntimeID == workerId);

            if (npc == null)
            {
                continue;
            }

            RemoveWorkerFromRoom(npc);
            
            if (WorkManager.Instance.FindNpc(npc) == null)
            {
                WorldManager.Instance.SpawnWorker(npc);
            }

            WorkManager.Instance.TryAssignWorker(
                npc,
                WorkerJob.None);
        }
        
        _roomsData.Remove(roomData);

        if (_spawnedRooms.TryGetValue(slotId, out GameObject roomObject))
        {
            Destroy(roomObject);
            _spawnedRooms.Remove(slotId);
        }
        
        ReapplyRoomBonuses();

        Save();
    }
    
    public int GetAssignedWorkersCount(int slotId)
    {
        var roomData = GetRoomBySlot(slotId);

        return roomData?.AssignedWorkers.Count ?? 0;
    }
    
    private TavernRoomSlot GetRoomSlot(int slotId)
    {
        foreach (var slot in _roomsPosition)
        {
            if (slot.SlotId == slotId)
            {
                return slot;
            }
        }

        return null;
    }
    
    public bool HasRoomType(TavernRoomType roomType)
    {
        foreach (var roomData in _roomsData)
        {
            TavernRoomSO room = GetRoom(roomData.RoomId);

            if (room == null)
            {
                continue;
            }

            if (room.RoomType == roomType)
            {
                return true;
            }
        }

        return false;
    }
    
    public TavernRoomData GetRoomByType(TavernRoomType roomType)
    {
        foreach (var roomData in _roomsData)
        {
            TavernRoomSO room = GetRoom(roomData.RoomId);

            if (room == null)
            {
                continue;
            }

            if (room.RoomType == roomType)
            {
                return roomData;
            }
        }

        return null;
    }
    
    public bool IsSlotOccupied(int slotId)
    {
        foreach (var roomData in _roomsData)
        {
            if (roomData.RoomSlotId == slotId)
            {
                return true;
            }
        }

        return false;
    }
    
    public TavernRoomData GetRoomBySlot(int slotId)
    {
        foreach (var roomData in _roomsData)
        {
            if (roomData.RoomSlotId == slotId)
            {
                return roomData;
            }
        }

        return null;
    }
    
    public TavernRoomSlot GetSlot(int slotId)
    {
        foreach (var slot in _roomsPosition)
        {
            if (slot.SlotId == slotId)
            {
                return slot;
            }
        }

        return null;
    }
    
    public bool BuyUpgrade(int slotId, RoomUpgradeSO upgrade)
    {
        TavernRoomData roomData = GetRoomBySlot(slotId);

        if (roomData == null)
        {
            return false;
        }

        if (roomData.PurchasedUpgrades.Contains(upgrade.Id))
        {
            return false;
        }

        if (!InventoryController.Instance.ChangeGoldAmount(upgrade.Cost))
        {
            FloatingTextManager.Instance.ShowWarningText(
                "Not enough gold.",
                transform);

            return false;
        }

        roomData.PurchasedUpgrades.Add(upgrade.Id);

        ReapplyRoomBonuses();
        
        Save();

        return true;
    }
    
    public bool HasUpgrade(int slotId, string upgradeId)
    {
        var roomData = GetRoomBySlot(slotId);

        if (roomData == null)
        {
            return false;
        }

        return roomData.PurchasedUpgrades.Contains(upgradeId);
    }
    
    public void ReapplyRoomBonuses()
    {
        _playerStats.ResetTavernBonuses();

        foreach (var roomData in _roomsData)
        {
            TavernRoomSO room = GetRoom(roomData.RoomId);

            if (room == null)
            {
                continue;
            }

            foreach (var purchasedUpgradeId in roomData.PurchasedUpgrades)
            {
                var upgrade = room.Upgrades.FirstOrDefault(
                    x => x.Id == purchasedUpgradeId);

                if (upgrade == null)
                {
                    continue;
                }

                _playerStats.AddTavernBonus(
                    new StatBonus
                    {
                        Type = upgrade.BonusType,
                        Value = upgrade.Value
                    });
            }
        }

        Player.Instance.PlayerAttack.RecalculateDamage();
    }
    
    public void RespawnRooms()
    {
        _spawnedRooms.Clear();

        foreach (var roomData in _roomsData)
        {
            SpawnRoom(roomData);
        }
    }
    
    public void RefreshSlots()
    {
        _roomsPosition.Clear();

        var markers = FindObjectsByType<TavernRoomSlotMarker>(
            FindObjectsSortMode.None);

        foreach (var marker in markers)
        {
            _roomsPosition.Add(new TavernRoomSlot
            {
                SlotId = marker.SlotId,
                SpawnPoint = marker.transform
            });
        }
    }
    
    public int GetWorkerCapacity(WorkerJob job)
    {
        var capacity = 0;

        foreach (var roomData in _roomsData)
        {
            var room = GetRoom(roomData.RoomId);

            if (room == null)
                continue;

            if (room.WorkerJob != job)
                continue;

            capacity += room.WorkerCapacity;
        }

        return capacity;
    }
    
    public bool AssignWorkerToRoom(
        VillageNpcRuntime npc,
        int slotId)
    {
        var roomData = GetRoomBySlot(slotId);

        if (roomData == null)
        {
            return false;
        }

        if (roomData.AssignedWorkers.Contains(npc.RuntimeID))
        {
            return false;
        }

        roomData.AssignedWorkers.Add(npc.RuntimeID);

        npc.IsAssignedToRoom = true;
        npc.AssignedRoomSlotId = slotId;

        Save();

        return true;
    }
    
    public void RemoveWorkerFromRoom(VillageNpcRuntime npc)
    {
        if (!npc.IsAssignedToRoom)
        {
            return;
        }

        var roomData = GetRoomBySlot(
            npc.AssignedRoomSlotId);

        if (roomData != null)
        {
            roomData.AssignedWorkers.Remove(
                npc.RuntimeID);
        }

        npc.IsAssignedToRoom = false;
        npc.AssignedRoomSlotId = -1;

        Save();
        WorldManager.Instance.Save();
    }
    
    #region Save/Load
    public void Save()
    {
        var settings = SaveLoadManager.Instance.GetSettings();
        
        ES3.Save(SaveKey, _roomsData, settings);
    }

    public void Load()
    {
        var settings = SaveLoadManager.Instance.GetSettings();

        if (!ES3.KeyExists(SaveKey, settings))
        {
            _roomsData = new List<TavernRoomData>();
            return;
        }

        _roomsData = ES3.Load<List<TavernRoomData>>(
            SaveKey,
            settings);

        foreach (var roomData in _roomsData)
        {
            SpawnRoom(roomData);
        }

        ReapplyRoomBonuses();
    }
    #endregion
}

[System.Serializable]
public class TavernRoomData
{
    public string RoomId;
    public int RoomSlotId;

    public int Level;
    public int AssignedNpcCount;
    
    public List<string> PurchasedUpgrades = new();
    public List<string> AssignedWorkers = new();
}

[System.Serializable]
public class TavernRoomSlot
{
    public int SlotId;
    public Transform SpawnPoint;
}