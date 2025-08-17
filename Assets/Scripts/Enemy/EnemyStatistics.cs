using System;
using UnityEngine;

public class EnemyStatistics : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private EnemyStatsSO _enemyStats;
    [SerializeField] public CharacterType CharacterType = CharacterType.Enemy;
    
    public string Name { get; private set; }
    public string Description { get; private set; }
    public float CurrentHP { get; private set; }
    public float MaxHP { get; private set; }
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
    private Rigidbody2D _rb2D;

    private void Awake()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        _enemyBrain = GetComponent<EnemyBrain>();
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _enemySelector = GetComponent<EnemySelector>();
        _rb2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Name = _enemyStats.Name;
        Description = _enemyStats.Description;
        CurrentHP = _enemyStats.MaxHP;
        MaxHP = _enemyStats.MaxHP;
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
            // _circleCollider.enabled = false;
            _rb2D.bodyType = RigidbodyType2D.Static;
            
            //TODO po otrzymaniu obrażen, zwiększyć na kilka sekund chase range innych postaci
        }
        else
        {
            DamageManager.Instance.ShowDamageText(amount, transform);
            _enemyAnimator.SetDamagedAnimation();
        }
    }
}
