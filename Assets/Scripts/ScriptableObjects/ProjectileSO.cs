using UnityEngine;

[CreateAssetMenu(fileName = "Projectile_", menuName = "Projectile")]
public class ProjectileSO : ScriptableObject
{
    public float Damage;
    public float Speed;
    public float ManaCost;
    public float KnockBackForce;
    public bool IsAOE;
    [Header("Effect modifiers")]
    [field: SerializeField] public Effect Effect;
    [field: SerializeField] public float EffectChance;
}
