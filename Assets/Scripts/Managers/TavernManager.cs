using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TavernManager : Singleton<TavernManager>, ISaveable
{
    private const string SaveKey = "TavernRooms";
    private const string UnlockedHeroesSaveKey = "TavernUnlockedHeroes";
    [SerializeField] private PlayerStatsSO _playerStats;

    [SerializeField] private List<TavernRoomSO> _rooms = new();
    [SerializeField] private List<TavernRoomSlot> _roomsPosition = new();
    [SerializeField]
    private List<TavernRoomData> _roomsData = new();
    private List<string> _unlockedHeroIds = new();
    
    public IReadOnlyList<TavernRoomData> RoomDatas => _roomsData;
    public IReadOnlyList<string> UnlockedHeroIds => _unlockedHeroIds;

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
        if (scene.name != "Base_Demo")
        {
            return;
        }
        
        RefreshSlots();
        RespawnRooms();
        SpawnUnlockedHeroes();
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
        SpawnUnlockedHeroes();
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

        foreach (var requirement in room.RequiredItems)
        {
            if (requirement == null || requirement.Item == null || requirement.Amount <= 0)
                continue;

            var ownedAmount = InventoryController.Instance.GetItemCount(new InventoryItem { item = requirement.Item });
            if (ownedAmount < requirement.Amount)
            {
                ConfirmationManager.Instance.ShowInformation(
                    $"Need {requirement.Amount} {requirement.Item.Name} ({ownedAmount}/{requirement.Amount}).");
                return false;
            }
        }

        return true;
    }

    public bool TryBuyRoom(TavernRoomSO room, int slotId)
    {
        if (!CheckIfCanBuyRoom(room, slotId))
            return false;

        if (!InventoryController.Instance.ChangeGoldAmount(-room.Cost))
            return false;

        foreach (var requirement in room.RequiredItems)
        {
            if (requirement == null || requirement.Item == null || requirement.Amount <= 0)
                continue;

            InventoryController.Instance.inventoryData.RemoveItemByName(
                requirement.Item.Name,
                requirement.Amount);
        }

        InventoryController.Instance.inventoryData.NotifyInventoryUpdated();

        var freeWorkers =
            WorldManager.Instance.GetFreeWorkers(
                room.RequiredWorkers);

        foreach (var worker in freeWorkers)
        {
            worker.IsAssignedToRoom = true;
            worker.AssignedRoomSlotId = slotId;

            WorldManager.Instance.HideWorker(worker);
        }

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
        
        TavernBonusManager.Instance.Rebuild();
        return true;
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

        var roomOwners = spawnedRoom.GetComponentsInChildren<RoomOwner>(true);

        foreach (var roomOwner in roomOwners)
        {
            roomOwner.SetRoomSlotId(roomData.RoomSlotId);
        }

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
        
        TavernBonusManager.Instance.Rebuild();
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
        var roomData = GetRoomBySlot(slotId);

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

        var unlockedHero = upgrade.HeroToUnlock != null &&
                           AddUnlockedHero(upgrade.HeroToUnlock);

        ReapplyRoomBonuses();
        
        Save();

        TavernBonusManager.Instance.Rebuild();

        if (unlockedHero)
        {
            SpawnUnlockedHeroes();

            if (ConfirmationManager.Instance != null)
            {
                ConfirmationManager.Instance.ShowInformation(
                    $"Hero unlocked: {upgrade.HeroToUnlock.Name}");
            }
        }

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

    public bool IsHeroUnlocked(NPCHero hero)
    {
        if (hero == null)
        {
            return false;
        }

        return _unlockedHeroIds.Contains(hero.StableId);
    }

    public IReadOnlyList<NPCHero> GetUnlockedHeroes()
    {
        var heroesById = new Dictionary<string, NPCHero>();

        foreach (var room in _rooms)
        {
            if (room == null)
            {
                continue;
            }

            foreach (var upgrade in room.Upgrades)
            {
                var hero = upgrade?.HeroToUnlock;
                if (hero == null || string.IsNullOrEmpty(hero.StableId))
                {
                    continue;
                }

                if (!heroesById.ContainsKey(hero.StableId))
                {
                    heroesById.Add(hero.StableId, hero);
                }
            }
        }

        var unlockedHeroes = new List<NPCHero>();
        foreach (var heroId in _unlockedHeroIds)
        {
            if (heroesById.TryGetValue(heroId, out var hero))
            {
                unlockedHeroes.Add(hero);
            }
        }

        return unlockedHeroes;
    }

    private void SpawnUnlockedHeroes()
    {
        var heroes = GetUnlockedHeroes();
        if (heroes.Count == 0)
        {
            return;
        }

        var positions = FindObjectsByType<HeroPosition>(FindObjectsSortMode.None)
            .OrderBy(position => position.name)
            .ToArray();

        if (positions.Length == 0)
        {
            Debug.LogWarning("Unlocked heroes exist, but no HeroPosition was found in the scene.");
            return;
        }

        var spawnedHeroIds = new HashSet<string>(
            positions
                .Where(position => position.SpawnedHero != null)
                .Select(position => position.SpawnedHero.StableId));

        foreach (var hero in heroes)
        {
            if (spawnedHeroIds.Contains(hero.StableId))
            {
                continue;
            }

            if (hero.Character == null)
            {
                Debug.LogWarning($"Unlocked hero '{hero.Name}' has no Character prefab assigned.");
                continue;
            }

            var position = positions.FirstOrDefault(
                candidate => !candidate.IsOccupied && candidate.transform.childCount == 0);

            if (position == null)
            {
                Debug.LogWarning($"No free HeroPosition is available for unlocked hero '{hero.Name}'.");
                return;
            }

            if (position.TrySpawn(hero))
            {
                spawnedHeroIds.Add(hero.StableId);
            }
        }
    }

    private bool AddUnlockedHero(NPCHero hero)
    {
        if (hero == null)
        {
            return false;
        }

        var heroId = hero.StableId;
        if (string.IsNullOrEmpty(heroId) || _unlockedHeroIds.Contains(heroId))
        {
            return false;
        }

        _unlockedHeroIds.Add(heroId);
        Debug.Log($"Unlocked hero: {hero.name} ({heroId})");
        return true;
    }

    private bool RebuildHeroUnlocksFromPurchasedUpgrades()
    {
        var changed = false;

        foreach (var roomData in _roomsData)
        {
            if (roomData.PurchasedUpgrades == null)
            {
                continue;
            }

            var room = GetRoom(roomData.RoomId);
            if (room == null)
            {
                continue;
            }

            foreach (var upgradeId in roomData.PurchasedUpgrades)
            {
                var upgrade = room.Upgrades.FirstOrDefault(x => x.Id == upgradeId);
                if (upgrade != null && upgrade.HeroToUnlock != null)
                {
                    changed |= AddUnlockedHero(upgrade.HeroToUnlock);
                }
            }
        }

        return changed;
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
        ES3.Save(UnlockedHeroesSaveKey, _unlockedHeroIds, settings);
    }

    public void Load()
    {
        var settings = SaveLoadManager.Instance.GetSettings();

        _roomsData = ES3.KeyExists(SaveKey, settings)
            ? ES3.Load<List<TavernRoomData>>(SaveKey, settings)
            : new List<TavernRoomData>();

        if (_roomsData == null)
        {
            _roomsData = new List<TavernRoomData>();
        }

        _unlockedHeroIds = ES3.KeyExists(UnlockedHeroesSaveKey, settings)
            ? ES3.Load<List<string>>(UnlockedHeroesSaveKey, settings)
            : new List<string>();

        if (_unlockedHeroIds == null)
        {
            _unlockedHeroIds = new List<string>();
        }

        var migratedHeroUnlocks = RebuildHeroUnlocksFromPurchasedUpgrades();
        if (migratedHeroUnlocks)
        {
            Save();
        }

        foreach (var roomData in _roomsData)
        {
            SpawnRoom(roomData);
        }

        SpawnUnlockedHeroes();

        ReapplyRoomBonuses();
        
        TavernBonusManager.Instance.Rebuild();
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
