using System.Collections.Generic;
using UnityEngine;

public class RoomOwner : MonoBehaviour
{
    [Header("Crafting")]
    [SerializeField] private List<CraftingRecipeSO> _recipes;

    private int _roomSlotId = -1;

    public IReadOnlyList<CraftingRecipeSO> Recipes => _recipes;
    public int RoomSlotId => _roomSlotId;
    public string RoomOwnerName => gameObject.name; 

    public void SetRoomSlotId(int slotId)
    {
        _roomSlotId = slotId;
    }
    
    public void OpenRoomPanel()
    {
        RoomOwnerPanelManager.Instance.Show(this);
    }
}
