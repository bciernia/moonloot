using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomUpgrade", menuName = "Moonloot/Tavern/Room Upgrade")]
public class RoomUpgradeSO : ScriptableObject
{
    public string Id;
    
    public string Name;
    [TextArea]
    public string Description;

    public int Cost;
    public int RequiredWorkers;

    public BonusType BonusType;
    public float Value;
    
    [SerializeField]
    private List<TavernEffect> _effects = new();

    public IReadOnlyList<TavernEffect> Effects => _effects;
}