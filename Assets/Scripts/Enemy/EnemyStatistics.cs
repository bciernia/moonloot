using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyStatistics : MonoBehaviour, IDamageable, IHealable, IRootable, IConfusionable, IHealth
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
    
    private GameObject _currentExplosionRange;
    
    private CircleCollider2D _circleCollider;
    private EnemyBrain _enemyBrain;
    private EnemyAnimator _enemyAnimator;
    private EnemySelector _enemySelector;
    private Rigidbody2D _rb2D;
    private EnemyLoot _enemyLoot;
    private EnemySounds _enemySounds;
    private KnockBack _knockBack;
    
    public Action<EnemyStatistics> OnDeath;
    public Action OnHit;

    private bool _isFrozen;
    public bool _isRooted;
    public bool _isConfused;
    private Coroutine _rootCoroutine;
    private Coroutine _confusionCoroutine;
    private Coroutine _freezeCoroutine;

    private bool _initialized = false;

    private MMF_Player _feelEffects;
    
    private const float DeathAnimationLength = 2f;
    
    public bool ShouldRunAway { get; private set; }
    
    private bool _willExplode;
    
    private List<DeathEffectEntry> _deathEffects;
    private DeathEffectEntry _selectedDeathEffect;
    
    private float _deathEffectDamageMultiplier = 1f;
    
    private List<HitReactionEntry> _hitReactions = new();
    private readonly List<AttackReactionEntry> _attackReactions = new();
    
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
        
        var hp = RandomizeStat(_enemyStats.MinHP, _enemyStats.MaxHP);
        var dmg = RandomizeStat(_enemyStats.MinDamage, _enemyStats.MaxDamage);
        
        Name = _enemyStats.Name;
        Description = _enemyStats.Description;
        CurrentHP = hp;
        MaxHP = hp;
        ExpForEnemy = _enemyStats.Exp;
        IsMelee = _enemyStats.IsMelee;
        AttackRange = _enemyStats.AttackRange;
        DetectRange = _enemyStats.DetectRange;
        Damage = dmg;
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
        
        _deathEffects = new List<DeathEffectEntry>(_enemyStats.DeathEffects);
    }

    private float RandomizeStat(float min, float max) => RNGManager.Instance.GetRandomFloat(min, max);

    public void TakeDamage(float amount, Transform damageSourceTransform, DamageType type = DamageType.Physical)
    {
        if (CurrentHP <= 0) return;
        
        CurrentHP = Mathf.Max(CurrentHP - amount, 0);
        FloatingTextManager.Instance.ShowDamageText(amount, transform);

        if (_enemyBrain != null && damageSourceTransform != null && damageSourceTransform.CompareTag("Player"))
        {
            _enemyBrain.ForceTarget(damageSourceTransform, 3f);
        } 
        
        if (!_isRooted && !_isFrozen && damageSourceTransform != null && _knockBack != null)
        {
            _knockBack.GetKnockedBack(damageSourceTransform, 5f);
        }
        
        TryTriggerHitReaction(damageSourceTransform);

        if (CurrentHP <= 0)
        {
            // _enemySelector.NoSelectionCallback();
            _enemySounds?.Die();
            _enemyAnimator?.TryFlipSpriteX();
            _enemyAnimator?.SetDeadAnimation();
            Speed = 0f;
            ChaseSpeed = 0f;
            
            _enemyBrain.enabled = false;
            _circleCollider.enabled = false;
            _rb2D.linearVelocity = Vector2.zero;
            _rb2D.angularVelocity = 0f;
            _rb2D.simulated = false;
            
            //TODO używane przy zapisywaniu martwych miedzy scenami
            //EnemyStateManager.Instance.MarkEnemyDead(_enemyBrain.EnemyID);
            TryTriggerDeathEffect();
            _enemyLoot.DropItems();
            OnDeath?.Invoke(this);
            EnemyEvents.RaiseEnemyKilled(this);
            
            Player.Instance.PlayerExp.AddExp(Mathf.RoundToInt(ExpForEnemy * HordeManager.Instance.CurrentHordeMultiplier));

            if (IsBoss)
            {
                NPCInfoManager.Instance.HideNpcInfo();
            }

            if (_enemyAnimator == null)
            {
                Destroy(gameObject);
            }
            
            HordeManager.Instance.OnEnemyKilled(IsElite, IsBoss);
            StartCoroutine(HandleDeathAnimation());
            //TODO po otrzymaniu obrażen, zwiększyć na kilka sekund chase range innych postaci
        }
        else
        {
            ShouldRunAway = true;
  
            _feelEffects?.PlayFeedbacks();
            _enemySounds?.Hit();
            _enemyAnimator?.SetDamagedAnimation();
        }
    }

    private void TryTriggerHitReaction(Transform attacker)
    {
        if (CurrentHP <= 0)
            return;

        if (_hitReactions == null || _hitReactions.Count == 0)
            return;

        var roll = RNGManager.Instance.GetRandomFloat(0f, 100f);

        var accumulated = 0f;

        foreach (var reaction in _hitReactions)
        {
            accumulated += reaction.Chance;

            if (roll <= accumulated)
            {
                TriggerHitReaction(
                    reaction,
                    attacker);

                return;
            }
        }
    }

    private void TriggerHitReaction(
        HitReactionEntry reaction,
        Transform attacker)
    {
        switch (reaction.Effect)
        {
            case HitReactionType.Teleport:
                TriggerTeleport(attacker, reaction.Distance);
                break;
        }
    }

    private void TriggerTeleport(Transform attacker, float distance)
    {
        if (attacker == null)
            return;

        var data =
            HitReactionDatabase.Instance.Get(HitReactionType.Teleport);

        if (data == null)
            return;
        
        var startPos = transform.position;

        if (data.StartEffectPrefab != null)
        {
            Destroy(
                Instantiate(
                    data.StartEffectPrefab,
                    startPos,
                    Quaternion.identity),
                5f);
        }
        
        if (data.Sound != null)
        {
            AudioSource.PlayClipAtPoint(
                data.Sound,
                startPos);
        }
        
        var direction = (transform.position - attacker.position).normalized;
        var targetPos = transform.position + direction * distance;
        transform.position = targetPos;
    }

    private void TryTriggerDeathEffect()
    {
        _willExplode = false;
        _selectedDeathEffect = null;

        if (_deathEffects == null || _deathEffects.Count == 0)
            return;

        var roll = RNGManager.Instance.GetRandomFloat(0f, 100f);

        var accumulated = 0f;

        foreach (var effect in _deathEffects)
        {
            accumulated += effect.Chance;

            if (roll <= accumulated)
            {
                if (effect.Effect == DeathEffectType.None)
                {
                    _selectedDeathEffect = null;
                    _willExplode = false;
                    return;
                }
                
                _selectedDeathEffect = effect;
                _willExplode = true;

                ShowExplosionRange();

                return;
            }
        }
    }
    
    private void ShowExplosionRange()
    {
        var data =
            DeathEffectDatabase.Instance.Get(
                _selectedDeathEffect.Effect);
        
        if (data.RangePrefab == null)
            return;

        _currentExplosionRange = Instantiate(
            data.RangePrefab,
            transform.position,
            Quaternion.identity,
            transform);

        var colors =
            DeathEffectColors.GetColors(
                _selectedDeathEffect.Effect);

        _currentExplosionRange
            .GetComponent<DeathEffectParticleColor>()
            ?.SetColors(colors.min, colors.max);

        var ps =
            _currentExplosionRange
                .GetComponentInChildren<ParticleSystem>();

        if (ps != null)
        {
            var shape = ps.shape;
            shape.radius = _selectedDeathEffect.Radius;
        }
    }
    
    private void TriggerExplosion()
    {
        var data =
            DeathEffectDatabase.Instance.Get(
                _selectedDeathEffect.Effect);
        
        if (data.EffectPrefab != null)
        {
            var effect = Instantiate(
                data.EffectPrefab,
                transform.position,
                Quaternion.identity);

            var colors =
                DeathEffectColors.GetColors(
                    _selectedDeathEffect.Effect);

            effect.GetComponent<DeathEffectParticleColor>()
                ?.SetColors(colors.min, colors.max);

            effect.GetComponentInChildren<ExplosionParticles>()
                ?.SetRadius(_selectedDeathEffect.Radius);

            effect.transform.localScale =
                Vector3.one * _selectedDeathEffect.Radius;
        }

        var hits = Physics2D.OverlapCircleAll(
            transform.position,
            _selectedDeathEffect.Radius);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player"))
                continue;

            _feelEffects?.PlayFeedbacks();

            var nightMultiplier = HordeManager.Instance.CurrentHordeMultiplier;

            var damage =
                RNGManager.Instance.GetRandomFloat(
                    Damage * 1.2f,
                    Damage * 1.5f)
                * nightMultiplier
                * _deathEffectDamageMultiplier;

            SoundManager.Instance.PlaySound(data.Sound);
            
            hit.GetComponent<IDamageable>()
                ?.TakeDamage(
                    damage,
                    transform);
        }
    }

    public void SetDeathEffects(IReadOnlyList<DeathEffectEntry> effects, float damageMultiplier)
    {
        ClearDeathEffects();

        _deathEffectDamageMultiplier = damageMultiplier;

        foreach (var effect in effects)
        {
            AddDeathEffect(
                effect.Effect,
                effect.Chance,
                effect.Radius);
        }
    }
    
    private IEnumerator HandleDeathAnimation()
    {
        yield return new WaitForSeconds(DeathAnimationLength);

        if (_currentExplosionRange != null)
        {
            Destroy(_currentExplosionRange);
        }

        if (_willExplode)
        {
            TriggerDeathEffect();
        }

        yield return new WaitForSeconds(0.2f);
        
        if (!IsBoss)
        {
            Destroy(gameObject);
        }
    }
    
    private void TriggerDeathEffect()
    {
        if (_selectedDeathEffect == null)
            return;

        switch (_selectedDeathEffect.Effect)
        {
            case DeathEffectType.Explosion:
                TriggerExplosion();
                break;

            case DeathEffectType.IceNova:
                TriggerIceNova();
                break;
            
            case DeathEffectType.PoisonCloud:
                TriggerPoisonCloud();
                break;
        }
    }

    private void TriggerPoisonCloud()
    {
        var data =
            DeathEffectDatabase.Instance.Get(
                _selectedDeathEffect.Effect);
        
        if (data.EffectPrefab != null)
        {
            var effect = Instantiate(
                data.EffectPrefab,
                transform.position,
                Quaternion.identity);

            var colors =
                DeathEffectColors.GetColors(
                    _selectedDeathEffect.Effect);

            effect.GetComponent<DeathEffectParticleColor>()
                ?.SetColors(colors.min, colors.max);

            effect.GetComponentInChildren<ExplosionParticles>()
                ?.SetRadius(_selectedDeathEffect.Radius);

            effect.transform.localScale =
                Vector3.one * _selectedDeathEffect.Radius;
        }

        var hits = Physics2D.OverlapCircleAll(
            transform.position,
            _selectedDeathEffect.Radius);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player"))
                continue;

            _feelEffects?.PlayFeedbacks();

            var nightMultiplier = HordeManager.Instance.CurrentHordeMultiplier;

            var damage =
                RNGManager.Instance.GetRandomFloat(
                    Damage * 1.2f,
                    Damage * 1.5f)
                * nightMultiplier
                * _deathEffectDamageMultiplier;

            hit.GetComponent<IDamageable>()
                ?.TakeDamage(
                    damage,
                    transform);

            SoundManager.Instance.PlaySound(data.Sound);
            
            data.PoisonEffect
                ?.Apply(
                    hit.gameObject,
                    null,
                    100f);
        }    }

    private void TriggerIceNova()
    {
        var data =
            DeathEffectDatabase.Instance.Get(
                _selectedDeathEffect.Effect);
        
        if (data.EffectPrefab != null)
        {
            var effect = Instantiate(
                data.EffectPrefab,
                transform.position,
                Quaternion.identity);

            var colors =
                DeathEffectColors.GetColors(
                    _selectedDeathEffect.Effect);

            effect.GetComponent<DeathEffectParticleColor>()
                ?.SetColors(colors.min, colors.max);

            effect.GetComponentInChildren<ExplosionParticles>()
                ?.SetRadius(_selectedDeathEffect.Radius);

            effect.transform.localScale =
                Vector3.one * _selectedDeathEffect.Radius;
        }

        var hits = Physics2D.OverlapCircleAll(
            transform.position,
            _selectedDeathEffect.Radius);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player"))
                continue;

            _feelEffects?.PlayFeedbacks();

            var nightMultiplier = HordeManager.Instance.CurrentHordeMultiplier;

            var damage =
                RNGManager.Instance.GetRandomFloat(
                    Damage * 1.2f,
                    Damage * 1.5f)
                * nightMultiplier
                * _deathEffectDamageMultiplier;

            hit.GetComponent<IDamageable>()
                ?.TakeDamage(
                    damage,
                    transform);
            
            SoundManager.Instance.PlaySound(data.Sound);

            data.FreezingEffect
                ?.Apply(
                    hit.gameObject,
                    null,
                    100f);
        }
    }

    private void ClearDeathEffects()
    {
        _deathEffects.Clear();
    }

    public void RestoreHealth(float amount)
    {
        CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
        FloatingTextManager.Instance.ShowHealText(amount, transform);
    }

    public void RestoreHealthForEliteEnemy(float amount)
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
    }

    public float CurrentHealthPoints => CurrentHP;
    public bool IsAlive => CurrentHP > 0f;
    
    public void StopRunningAway()
    {
        ShouldRunAway = false;
    }
    
    public void ApplyFreeze(float duration, GameObject effect)
    {
        if (_freezeCoroutine != null)
            StopCoroutine(_freezeCoroutine);

        _freezeCoroutine = StartCoroutine(
            FreezeRoutine(duration, effect));
    }
    
    private IEnumerator FreezeRoutine(
        float duration,
        GameObject effect)
    {
        _isFrozen = true;

        var originalSpeed = Speed;
        var originalChaseSpeed = ChaseSpeed;

        var currentEffect = Instantiate(
            effect,
            transform.position + Vector3.down * 0.5f,
            Quaternion.identity,
            transform);

        Speed = 0f;
        ChaseSpeed = 0f;

        if (_rb2D != null)
            _rb2D.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(duration);

        Speed = originalSpeed;
        ChaseSpeed = originalChaseSpeed;

        _isFrozen = false;

        if (currentEffect != null)
            Destroy(currentEffect);
    }
    
    private void AddDeathEffect(
        DeathEffectType effect,
        float chance,
        float radius)
    {
        if (_deathEffects == null)
        {
            _deathEffects = new List<DeathEffectEntry>();
        }

        _deathEffects.Add(new DeathEffectEntry
        {
            Effect = effect,
            Chance = chance,
            Radius = radius,
        });
    }
    
    private void ClearHitReactions()
    {
        _hitReactions.Clear();
    }

    private void AddHitReaction(
        HitReactionType effect,
        float chance,
        float duration,
        float distance)
    {
        _hitReactions.Add(new HitReactionEntry
        {
            Effect = effect,
            Chance = chance,
            Duration = duration,
            Distance = distance
        });
    }

    public void SetHitReactions(IReadOnlyList<HitReactionEntry> reactions)
    {
        ClearHitReactions();

        foreach (var reaction in reactions)
        {
            AddHitReaction(
                reaction.Effect,
                reaction.Chance,
                reaction.Duration,
                reaction.Distance);
        }
    }
    
    public void SetAttackReactions(IReadOnlyList<AttackReactionEntry> reactions)
    {
        _attackReactions.Clear();

        foreach (var reaction in reactions)
        {
            _attackReactions.Add(new AttackReactionEntry
            {
                Effect = reaction.Effect,
                Chance = reaction.Chance,
                Value = reaction.Value
            });
        }
    }
    
    public void TryTriggerAttackReaction(float damageDealt)
    {
        if (_attackReactions == null || _attackReactions.Count == 0)
            return;

        var roll = RNGManager.Instance.GetRandomFloat(0f, 100f);

        var accumulated = 0f;

        foreach (var reaction in _attackReactions)
        {
            accumulated += reaction.Chance;

            if (roll <= accumulated)
            {
                TriggerAttackReaction(
                    reaction,
                    damageDealt);

                return;
            }
        }
    }
    
    private void TriggerAttackReaction(
        AttackReactionEntry reaction,
        float damageDealt)
    {
        switch (reaction.Effect)
        {
            case AttackReactionType.Heal:
                RestoreHealth(Mathf.Round(damageDealt));
                break;
        }
    }
}
