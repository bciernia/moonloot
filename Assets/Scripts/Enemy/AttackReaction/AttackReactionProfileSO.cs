using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonloot/Attack Reaction Profile", fileName = "AttackReactionProfile_")]
public class AttackReactionProfileSO : ScriptableObject
{
    [SerializeField]
    private List<AttackReactionEntry> _effects = new();

    public IReadOnlyList<AttackReactionEntry> Effects => _effects;

    [SerializeField]
    private int weight = 1;

    public int Weight => weight;
}