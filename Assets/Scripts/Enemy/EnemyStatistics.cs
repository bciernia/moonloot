using System;
using UnityEngine;

public class EnemyStatistics : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private EnemyStatsSO _enemyStats;

    public float CurrentHP { get; private set; }
    public float ExpForEnemy { get; private set; }
    public bool IsMelee { get; private set; }
    public float AttackRange { get; set; }
    public float DetectRange { get; set; }
    public float Damage { get; set; }
    public float TimeBetweenAttacks { get; set; }
    public float Speed { get; set; }
    public float ChaseSpeed { get; set; }
    public float StopRange { get; private set; }
    
    private CircleCollider2D _circleCollider;
    private EnemyBrain _enemyBrain;
    private EnemyAnimator _enemyAnimator;
    private EnemySelector _enemySelector;

    private void Awake()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        _enemyBrain = GetComponent<EnemyBrain>();
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _enemySelector = GetComponent<EnemySelector>();
    }

    private void Start()
    {
        CurrentHP = _enemyStats.MaxHP;
        ExpForEnemy = _enemyStats.Exp;
        IsMelee = _enemyStats.IsMelee;
        AttackRange = _enemyStats.AttackRange;
        DetectRange = _enemyStats.DetectRange;
        Damage = _enemyStats.Damage;
        TimeBetweenAttacks = _enemyStats.TimeBetweenAttack;
        Speed = _enemyStats.Speed;
        ChaseSpeed = _enemyStats.ChaseSpeed;
        StopRange = _enemyStats.StopRange;
    }

    public void TakeDamage(float amount)
    {
        CurrentHP -= amount;

        if (CurrentHP <= 0)
        {
            // _enemySelector.NoSelectionCallback();
            
            _enemyAnimator.TryFlipSpriteX();
            _enemyAnimator.SetDeadAnimation();
            
            _enemyBrain.enabled = false;
            _circleCollider.enabled = false;
            
            //TODO po otrzymaniu obrażen, zwiększyć na kilka sekund chase range innych postaci
            
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
        else
        {
            DamageManager.Instance.ShowDamageText(amount, transform);
            _enemyAnimator.SetDamagedAnimation();
        }
    }
}
