using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    menuName = "Moonloot/Hit Reaction Profile",
    fileName = "HitReactionProfile_")]
public class HitReactionProfileSO : ScriptableObject
{
    [SerializeField] public List<HitReactionEntry> _effects = new();

    public IReadOnlyList<HitReactionEntry> Effects => _effects;

    [SerializeField]
    private int weight = 1;
    public int Weight => weight;
}