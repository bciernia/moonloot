using System.Collections.Generic;
using UnityEngine;

public class RoomOwner : MonoBehaviour
{
    [Header("Crafting")]
    [SerializeField] private List<CraftingRecipeSO> _recipes;

    public IReadOnlyList<CraftingRecipeSO> Recipes => _recipes;
    
    public void OpenRoomPanel()
    {
        RoomOwnerPanelManager.Instance.Show(this);
    }
}