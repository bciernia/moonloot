using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats_", menuName = "Enemy Stats")]
public class EnemyStatsSO : ScriptableObject
{
    [Header("Configuration")]
    public float MaxHP;
    public float Exp;
    public float AttackRange;
    public float DetectRange;
    public bool IsMelee;
    public float Damage;
    public float TimeBetweenAttack;
    public float Speed;
    
    [Header("Chasing")]
    public float ChaseSpeed;
    public float StopRange;
}
