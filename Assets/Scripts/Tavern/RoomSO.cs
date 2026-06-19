using UnityEngine;

[CreateAssetMenu(menuName = "Tavern/Room")]
public class TavernRoomSO : ScriptableObject
{
    public string RoomId;
    public int RoomSlotId;
    public TavernRoomType RoomType;
    
    public int Cost;

    public GameObject RoomPrefab;
}