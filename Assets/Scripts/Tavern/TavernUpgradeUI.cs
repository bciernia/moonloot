using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TavernUpgradeUI : MonoBehaviour
{
    [SerializeField] private Transform _tavernOverviewPoint;
    [SerializeField] private GameObject _tavernOverviewPanel;
    [SerializeField] private GameObject _roomUpgradePanel;
    
    [SerializeField] private GameObject _addRoomPanel;
    [SerializeField] private GameObject _upgradeRoomPanel;
    [SerializeField] private RectTransform _addRoomBtnsContentPanel;
    [SerializeField] private RectTransform _upgradeRoomBtnsContentPanel;
    [SerializeField] private GameObject _roomBtnPrefab;
    [SerializeField] private GameObject _roomDescriptionPanel;
    [SerializeField] private TextMeshProUGUI _roomDescriptionText;
    
    [SerializeField] private GameObject _createButton;
    [SerializeField] private GameObject _upgradeButton;
    [SerializeField] private GameObject _removeButton;
    
    private readonly List<GameObject> _spawnedRoomButtons = new();
    private int _selectedSlotId;
    private GameObject _currentRoomPreview;
    private TavernRoomSO _selectedRoom;
    private RoomUpgradeSO _selectedUpgrade;
    
    public void FocusRoom(TavernRoomSpot roomCameraPoint)
    {
        _selectedSlotId = roomCameraPoint.SlotId;

        _selectedRoom = null;

        _roomDescriptionPanel.SetActive(false);

        ClearRoomPreview();

        StartCoroutine(FocusRoomCoroutine(roomCameraPoint.CameraPoint));
    }

    private IEnumerator FocusRoomCoroutine(Transform roomCameraPoint)
    {
        _tavernOverviewPanel.SetActive(false);

        CameraFocusManager.Instance.SetFocus(roomCameraPoint);

        yield return new WaitUntil(() =>
            CameraFocusManager.Instance.IsAtTarget());

        ClearRoomButtons();

        var roomData = TavernManager.Instance.GetRoomBySlot(_selectedSlotId);

        _addRoomPanel.SetActive(roomData == null);
        _upgradeRoomPanel.SetActive(roomData != null);

        if (roomData == null)
        {
            RefreshAddRoomButtons();
        }
        else
        {
            RefreshUpgradeButtons(roomData);
        }

        _roomUpgradePanel.SetActive(true);
    }
    
    public void GoBackToTavernOverview()
    {
        StartCoroutine(GoBackCoroutine());
    }

    private IEnumerator GoBackCoroutine()
    {
        _roomDescriptionPanel.SetActive(false);
        _roomUpgradePanel.SetActive(false);

        ClearRoomPreview();

        CameraFocusManager.Instance.SetFocus(
            _tavernOverviewPoint,
            18f);

        yield return new WaitUntil(() =>
            CameraFocusManager.Instance.IsAtTarget());

        _tavernOverviewPanel.SetActive(true);
    }
    
    private void RefreshAddRoomButtons()
    {
        _selectedRoom = null;

        _roomDescriptionPanel.SetActive(false);

        ClearRoomPreview();

        foreach (var button in _spawnedRoomButtons)
        {
            Destroy(button);
        }

        _spawnedRoomButtons.Clear();


        foreach (var room in TavernManager.Instance.Rooms)
        {
            if (room.SlotId != _selectedSlotId)
            {
                continue;
            }

            if (TavernManager.Instance.HasRoomType(room.RoomType))
            {
                continue;
            }

            var roomButton = Instantiate(_roomBtnPrefab, _addRoomBtnsContentPanel);

            var roomBtn = roomButton.GetComponent<ShowRoomDetailsBtn>();

            roomBtn.Setup(room, this);

            _spawnedRoomButtons.Add(roomButton);
        }
    }
    public void ShowRoomDetails(TavernRoomSO room)
    {
        _selectedRoom = room;

        _roomDescriptionPanel.SetActive(true);

        _roomDescriptionText.text = room.Description;

        RefreshRoomActions();

        ShowRoomPreview(room);
    }
    
    private void RefreshRoomActions()
    {
        TavernRoomData roomData = TavernManager.Instance.GetRoomBySlot(_selectedSlotId);

        if (roomData == null)
        {
            _createButton.SetActive(true);
            _upgradeButton.SetActive(false);
            _removeButton.SetActive(false);
            return;
        }

        _createButton.SetActive(false);
        _upgradeButton.SetActive(true);
        _removeButton.SetActive(true);
    }
    
    private void ClearRoomPreview()
    {
        if (_currentRoomPreview == null)
        {
            return;
        }

        Destroy(_currentRoomPreview);
        _currentRoomPreview = null;
    }
    
    private void ShowRoomPreview(TavernRoomSO room)
    {
        ClearRoomPreview();

        TavernRoomSlot slot =
            TavernManager.Instance.GetSlot(_selectedSlotId);

        if (slot == null)
        {
            return;
        }

        _currentRoomPreview = Instantiate(
            room.RoomPrefab,
            slot.SpawnPoint.position,
            slot.SpawnPoint.rotation,
            slot.SpawnPoint);
    }
    
    public void CreateRoom()
    {
        if (_selectedRoom == null)
        {
            return;
        }

        if (!TavernManager.Instance.TryBuyRoom(_selectedRoom, _selectedSlotId))
        {
            return;
        }

        ClearRoomPreview();
        
        _selectedRoom = null;
        _roomDescriptionPanel.SetActive(false);
        
        _addRoomPanel.SetActive(false);
        _upgradeRoomPanel.SetActive(true);

        var roomData = TavernManager.Instance.GetRoomBySlot(_selectedSlotId);

        if (roomData != null)
        {
            RefreshUpgradeButtons(roomData);
        }

        RefreshRoomActions();
    }
    
    public void RemoveRoom()
    {
        TavernManager.Instance.RemoveRoom(_selectedSlotId);

        _selectedRoom = null;

        ClearRoomPreview();

        _roomDescriptionPanel.SetActive(false);

        _upgradeRoomPanel.SetActive(false);
        _addRoomPanel.SetActive(true);

        RefreshAddRoomButtons();
    }
    
    private void ClearRoomButtons()
    {
        foreach (var button in _spawnedRoomButtons)
        {
            Destroy(button);
        }

        _spawnedRoomButtons.Clear();
    }
    
    private void RefreshUpgradeButtons(TavernRoomData roomData)
    {
        ClearRoomButtons();

        TavernRoomSO room =
            TavernManager.Instance.GetRoom(roomData.RoomId);

        if (room == null)
        {
            return;
        }

        foreach (var upgrade in room.Upgrades)
        {
            if (TavernManager.Instance.HasUpgrade(
                    roomData.RoomSlotId,
                    upgrade.Id))
            {
                continue;
            }
            
            var roomButton = Instantiate(
                _roomBtnPrefab,
                _upgradeRoomBtnsContentPanel);

            var roomBtn = roomButton.GetComponent<ShowRoomDetailsBtn>();

            roomBtn.Setup(upgrade, this);

            _spawnedRoomButtons.Add(roomButton);
        }
    }

    public void BuyUpgrade()
    {
        if (_selectedUpgrade == null)
        {
            return;
        }

        if (!TavernManager.Instance.BuyUpgrade(
                _selectedSlotId,
                _selectedUpgrade))
        {
            return;
        }

        _selectedUpgrade = null;

        _roomDescriptionPanel.SetActive(false);

        var roomData = TavernManager.Instance.GetRoomBySlot(_selectedSlotId);

        if (roomData != null)
        {
            RefreshUpgradeButtons(roomData);
        }
    }
    
    public void ShowUpgradeDetails(RoomUpgradeSO upgrade)
    {
        _selectedUpgrade = upgrade;

        _roomDescriptionPanel.SetActive(true);

        _roomDescriptionText.text = upgrade.Description;

        _createButton.SetActive(false);
        _upgradeButton.SetActive(true);
        _removeButton.SetActive(true);
    }
}

