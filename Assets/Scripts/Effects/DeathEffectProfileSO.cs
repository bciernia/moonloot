using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonloot/Death Effect Profile", fileName = "DeathEffectProfile_")]
public class DeathEffectProfileSO : ScriptableObject
{
    [SerializeField]
    private List<DeathEffectEntry> _effects = new();

    public IReadOnlyList<DeathEffectEntry> Effects => _effects;
    
    [SerializeField]
    private int weight = 1;
    [SerializeField]
    private float damageMultiplier = 1f;
    
    public int Weight => weight;

    public float DamageMultiplier => damageMultiplier;
}