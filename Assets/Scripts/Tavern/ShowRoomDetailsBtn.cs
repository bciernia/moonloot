using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowRoomDetailsBtn : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _roomName;
    [SerializeField] private Image _roomIcon;

    private TavernRoomSO _room;
    private RoomUpgradeSO _upgrade;
    private TavernUpgradeUI _tavernUpgradeUI;

    public void Setup(TavernRoomSO room, TavernUpgradeUI tavernUpgradeUI)
    {
        _room = room;
        _tavernUpgradeUI = tavernUpgradeUI;

        _roomName.text = room.RoomType.ToString();
        _roomIcon.sprite = room.Icon;
    }

    public void Setup(RoomUpgradeSO upgrade, TavernUpgradeUI ui)
    {
        _upgrade = upgrade;
        _tavernUpgradeUI = ui;
        _roomName.text = upgrade.Name;
    }

    public void ShowDetails()
    {
        if (_room != null)
        {
            _tavernUpgradeUI.ShowRoomDetails(_room);
            return;
        }

        if (_upgrade != null)
        {
            _tavernUpgradeUI.ShowUpgradeDetails(_upgrade);
        }
    }
}