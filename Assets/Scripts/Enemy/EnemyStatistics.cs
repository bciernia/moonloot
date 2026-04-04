using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class EnemyStatistics : MonoBehaviour, IDamageable, IHealable, IRootable, IConfusionable
{
    [Header("Config")]
    [SerializeField] private EnemyStatsSO _enemyStats;
    [SerializeField] public CharacterType CharacterType = CharacterType.Enemy;
    
    // public event Action OnEnemyDied;
    
    public string Name { get; private set; }
    public string Description { get; private set; }
    public float CurrentHP { get; private set; }
    public float MaxHP { get; set; }
    public float ExpForEnemy { get; private set; }
    public bool IsMelee { get; private set; }
    public float AttackRange { get; set; }
    public float DetectRange { get; set; }
    public float Damage { get; set; }
    public float TimeBetweenAttacks { get; set; }
    public float Speed { get; set; }
    public float ChaseSpeed { get; set; }
    public float StopRange { get; private set; }
    public float SpecialAttackTimeInterval { get; set; }
    public float MaxAttackTimeInterval { get; private set; }
    public List<GameObject> SpecialAttacks { get; private set; }
    public bool IsBoss { get; private set; }
    public bool IsElite { get; private set; }

    public Effect Effect { get; private set; }
    public float EffectChance { get; private set; }
    
    public List<AudioClip> IdleSounds { get; private set; }
    public List<AudioClip> MoveSounds {get; private set;  }
    public List<AudioClip> DmgSounds {get; private set;  }
    public List<AudioClip> AttackSounds {get; private set; }
    public List<AudioClip> DeathSounds {get; private set; }
    
    private CircleCollider2D _circleCollider;
    private EnemyBrain _enemyBrain;
    private EnemyAnimator _enemyAnimator;
    private EnemySelector _enemySelector;
    private Rigidbody2D _rb2D;
    private EnemyLoot _enemyLoot;
    private EnemySounds _enemySounds;
    private KnockBack _knockBack;
    
    public Action<EnemyStatistics> OnDeath;

    public bool _isRooted;
    public bool _isConfused;
    private Coroutine _rootCoroutine;
    private Coroutine _confusionCoroutine;
    
    private bool _initialized = false;

    private MMF_Player _feelEffects;
    
    private void Awake()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        _enemyBrain = GetComponent<EnemyBrain>();
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _enemySelector = GetComponent<EnemySelector>();
        _rb2D = GetComponent<Rigidbody2D>();
        _enemyLoot = GetComponent<EnemyLoot>();
        _enemySounds = GetComponent<EnemySounds>();
        _knockBack = GetComponent<KnockBack>();
        _feelEffects = GetComponent<MMF_Player>();
    }
    
    public void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        
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
        SpecialAttackTimeInterval = _enemyStats.SpecialAttackTimeInterval;
        MaxAttackTimeInterval = _enemyStats.MaxAttackTimeInterval;
        SpecialAttacks = _enemyStats.SpecialAttacks;
        IsBoss = _enemyStats.IsBoss;
        IsElite = _enemyStats.IsElite;
        Effect = _enemyStats.Effect;
        EffectChance = _enemyStats.EffectChance;
        IdleSounds = _enemyStats.IdleSounds;
        MoveSounds = _enemyStats.MoveSounds;
        DmgSounds = _enemyStats.DmgSounds;
        AttackSounds = _enemyStats.AttackSounds;
        DeathSounds = _enemyStats.DeathSounds;
    }

    public void TakeDamage(float amount, Transform damageSourceTransform, DamageType type = DamageType.Physical)
    {
        CurrentHP = Mathf.Max(CurrentHP - amount, 0);
        DamageManager.Instance.ShowDamageText(amount, transform);

        if (!_isRooted && damageSourceTransform != null)
        {
            _knockBack.GetKnockedBack(damageSourceTransform, 5f);
        }
        
        if (CurrentHP <= 0)
        {
            // _enemySelector.NoSelectionCallback();
            _enemySounds?.Die();
            _enemyAnimator.TryFlipSpriteX();
            _enemyAnimator.SetDeadAnimation();

            _enemyBrain.enabled = false;
            // _circleCollider.enabled = false;
            _rb2D.bodyType = RigidbodyType2D.Static;
            EnemyStateManager.Instance.MarkEnemyDead(_enemyBrain.EnemyID);
            _enemyLoot.DropItems();
            OnDeath?.Invoke(this);
            HordeManager.Instance.OnEnemyKilled();
            StartCoroutine(HandleDeathAnimation());
            //TODO po otrzymaniu obrażen, zwiększyć na kilka sekund chase range innych postaci
        }
        else
        {
            _feelEffects?.PlayFeedbacks();
            _enemySounds?.Hit();
            _enemyAnimator.SetDamagedAnimation();
        }
    }

    private IEnumerator HandleDeathAnimation()
    {
        const float deathAnimLength = 1f;
        yield return new WaitForSeconds(deathAnimLength);

        if (!IsBoss)
        {
            Destroy(gameObject);
        }
    }

    public void RestoreHealth(float amount)
    {
        CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
    }


    public void ApplyRoot(float duration, GameObject effect)
    {
        if (_isRooted)
        {
            if (_rootCoroutine != null)
                StopCoroutine(_rootCoroutine);
        }

        _rootCoroutine = StartCoroutine(RootRoutine(duration, effect));
    }
    
    private IEnumerator RootRoutine(float duration, GameObject effect)
    {
        _isRooted = true;

        var _originalSpeed = Speed;
        var _originalChaseSpeed = ChaseSpeed;
        
        var currentEffect = Instantiate(effect, new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z), Quaternion.identity, transform);
        
        Speed = 0f;
        ChaseSpeed = 0f;

        if (_rb2D != null)
            _rb2D.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(duration);

        Speed = _originalSpeed;
        ChaseSpeed = _originalChaseSpeed;

        _isRooted = false;
        Destroy(currentEffect);
    }

    public void ApplyConfusion(float duration, GameObject effect)
    {
        if (_isConfused)
        {
            if (_confusionCoroutine != null)
                StopCoroutine(_confusionCoroutine);
        }

        _confusionCoroutine = StartCoroutine(ConfusionRoutine(duration, effect));
    }
    
    private IEnumerator ConfusionRoutine(float duration, GameObject effect)
    {
        _isConfused = true;
        var currentEffect = Instantiate(effect, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Quaternion.identity, transform);

        var decisionDetectPlayer = GetComponent<DecisionDetectPlayer>();
        
        decisionDetectPlayer.playerMask = LayerMask.GetMask("Environment");

        yield return new WaitForSeconds(duration);

        decisionDetectPlayer.playerMask = LayerMask.GetMask("Player");
        _isConfused = false;
        Destroy(currentEffect);
    }
    
    public void ApplyHordeScaling(float hpMultiplier, float damageMultiplier, float speedMultiplier, bool isEliteOverride = false, bool isBossOverride = false)
    {
        MaxHP *= hpMultiplier;
        CurrentHP = MaxHP;

        Damage *= damageMultiplier;

        Speed *= speedMultiplier;
        ChaseSpeed *= speedMultiplier;

        if (isEliteOverride)
            IsElite = true;

        if (isBossOverride)
            IsBoss = true;

        // ELITE ENEMY IS BIGGER THAN OTHER
        // BOSS IS MUCH BIGGER
        if (IsElite)
        {
            transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        }
        else if (IsBoss)
        {
            transform.localScale = new Vector3(1.4f, 1.4f, 1f);
        }
        
        
    }
}
