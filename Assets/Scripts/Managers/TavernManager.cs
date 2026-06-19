using System.Collections.Generic;
using UnityEngine;

public class TavernManager : Singleton<TavernManager>, ISaveable
{
    private const string SaveKey = "TavernRooms";

    [SerializeField] private List<TavernRoomSO> _rooms = new();
    [SerializeField] private List<TavernRoomSlot> _roomsPosition = new();

    [SerializeField]
    private List<TavernRoomData> _roomsData = new();

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

    public bool TryBuyRoom(TavernRoomSO room, int slotId)
    {
        if (HasRoomType(room.RoomType))
        {
            FloatingTextManager.Instance.ShowWarningText("You already have this room type.", transform);
            return false;
        }

        if (IsSlotOccupied(slotId))
        {
            FloatingTextManager.Instance.ShowWarningText("This room is occupied.", transform);
            return false;
        }
        
        if (GetRoomSlot(slotId) == null)
        {
            FloatingTextManager.Instance.ShowErrorText("Invalid room slot.", transform);
            return false;
        }
        
        if (!InventoryController.Instance.ChangeGoldAmount(room.Cost))
        {
            FloatingTextManager.Instance.ShowWarningText("Not enough gold to unlock this room.", transform);
            return false;
        }

        UnlockRoom(room.RoomId, slotId);
        Save();
        return true;
    }
    
    private void SpawnRoom(TavernRoomData roomData)
    {
        TavernRoomSO room = GetRoom(roomData.RoomId);

        if (room == null)
        {
            return;
        }

        TavernRoomSlot slot = GetRoomSlot(roomData.RoomSlotId);

        if (slot == null)
        {
            return;
        }

        Instantiate(
            room.RoomPrefab,
            slot.SpawnPoint.position,
            slot.SpawnPoint.rotation,
            slot.SpawnPoint);
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

    public void Save()
    {
        ES3.Save(SaveKey, _roomsData);
    }

    public void Load()
    {
        if (!ES3.KeyExists(SaveKey))
        {
            _roomsData = new List<TavernRoomData>();
            return;
        }

        _roomsData = ES3.Load<List<TavernRoomData>>(SaveKey);

        foreach (var roomData in _roomsData)
        {
            SpawnRoom(roomData);
        }
    }
}

[System.Serializable]
public class TavernRoomData
{
    public string RoomId;
    public int RoomSlotId;

    public int Level;
    public int AssignedNpcCount;
}

[System.Serializable]
public class TavernRoomSlot
{
    public int SlotId;
    public Transform SpawnPoint;
}