using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tavern/Room")]
public class TavernRoomSO : ScriptableObject
{
    public string RoomId;
    public int SlotId;
    public TavernRoomType RoomType;
    public Sprite Icon;
    [TextArea]
    public string Description;
    
    public int Cost;
    public int RequiredWorkers;
    
    public GameObject RoomPrefab;
    
    [SerializeField] private List<RoomUpgradeSO> _upgrades = new();
    public IReadOnlyList<RoomUpgradeSO> Upgrades => _upgrades;

    public WorkerJob WorkerJob;
    public int WorkerCapacity;
}