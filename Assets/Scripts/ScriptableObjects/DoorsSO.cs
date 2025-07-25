using UnityEngine;

[CreateAssetMenu(fileName = "Door_")]
public class DoorsSO : ScriptableObject
{
    public ItemSO KeyToOpen;
    public Sprite ClosedDoorSprite;
    public Sprite OpenedDoorSprite;
    public string ConnectedRoom;
}