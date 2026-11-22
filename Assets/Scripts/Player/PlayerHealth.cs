using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable, IHealable, IShieldable, IHealth
{
    [Header("Configuration")]
    [SerializeField] private PlayerStatsSO _playerStats;

    private PlayerAnimations _playerAnimations;

    public float CurrentHealth => _playerStats != null ? _playerStats.HP : 0f;

    private Coroutine _shieldCoroutine;
    
    private void Awake()
    {
        _playerAnimations = GetComponent<PlayerAnimations>();
    }
    
    private void Start()
    {
        if (_playerStats.HP <= 0)
        {
            PlayerDead();
        }

        PrepareStatistics();
        StartCoroutine(HealthRegenerationRoutine());
    }

    private void PrepareStatistics()
    {
        // PlayerStatisticsManager.Instance.SetLevel(_playerStats.Level);
        Player.Instance.PlayerAttack.RecalculateDamage();
        // PlayerStatisticsManager.Instance.SetDamage(_playerStats.TotalDamage);
        PlayerStatisticsManager.Instance.SetPhysicalResistance(_playerStats.GetPhysicalReductionPercent());
        // PlayerStatisticsManager.Instance.SetMagicResistance(_playerStats.GetMagicReductionPercent());
        
        RefreshResistanceUI();
    }
    
    public void TakeDamage(float amount, Transform damageSourceType,  DamageType type)
    {
        if (_playerStats.HP <= 0) return;
        
        var afterArmor = ApplyResistance(amount, type);
        var bonusReduction = Mathf.Clamp01(
            _playerStats.GetBonusValue(BonusType.DamageReduction));
        var reducedDamage = afterArmor * _playerStats.ShieldResistance *
                            (1f - bonusReduction);
        
        _playerStats.HP -= reducedDamage;
        FloatingTextManager.Instance.ShowDamageText(reducedDamage, transform);
        
        FeelManager.Instance.PlayDamage();
        
        if (_playerStats.HP <= 0f)
        {
            _playerStats.HP = 0;
            PlayerDead();    
        }
    }

    private void PlayerDead()    
    {
        HordeManager.Instance.CleanupEnemies();
        HordeManager.Instance.StopNight();
        SoundManager.Instance.PlayDeathMusic();
        _playerAnimations.SetDeathAnimation();
        var points = PointsManager.Instance.GetCurrentScore();
        DeathScreenManager.Instance.ShowDeathScreen(points);
    }

    public void RestoreHealth(float amount)
    {
        if (amount <= 0f || _playerStats.HP <= 0f)
            return;

        var previousHealth = _playerStats.HP;
        _playerStats.HP = Mathf.Min(
            _playerStats.HP + amount,
            _playerStats.GetMaxHp());
        var restoredAmount = _playerStats.HP - previousHealth;

        if (restoredAmount > 0f && FloatingTextManager.Instance != null)
            FloatingTextManager.Instance.ShowHealText(restoredAmount, transform);
    }

    private IEnumerator HealthRegenerationRoutine()
    {
        var accumulatedHealing = 0f;

        while (true)
        {
            // Scaled time prevents regeneration while the game is paused.
            yield return new WaitForSeconds(1f);

            if (_playerStats.HP <= 0f || _playerStats.HP >= _playerStats.GetMaxHp())
            {
                accumulatedHealing = 0f;
                continue;
            }

            accumulatedHealing += _playerStats.GetHealthRegenerationPerSecond();
            var wholeHealingPoints = Mathf.Floor(accumulatedHealing);
            if (wholeHealingPoints < 1f)
                continue;

            accumulatedHealing -= wholeHealingPoints;
            RestoreHealth(wholeHealingPoints);
        }
    }

    public void RestoreMana(float amount)
    {
        _playerStats.MP = Mathf.Min(_playerStats.MP + amount, _playerStats.GetMaxMp());
        FloatingTextManager.Instance.ShowManaText(amount, transform);
    }
    
    public bool CanRestoreHealth()
    {
        return _playerStats.HP > 0 && _playerStats.HP < _playerStats.GetMaxHp();
    }
    
    public bool CanRestoreMana()
    {
        return _playerStats.MP > 0 && _playerStats.MP < _playerStats.GetMaxMp();
    }

    public void ReduceDamage(float amount, float duration, GameObject effect)
    {
        if(_shieldCoroutine != null) StopCoroutine(_shieldCoroutine);

        _shieldCoroutine = StartCoroutine(ShieldCoroutine(amount, duration, effect));
    }

    private IEnumerator ShieldCoroutine(float amount, float duration, GameObject effect)
    {
        SetPlayerDmgReduction(amount);
        var currentEffect = Instantiate(effect, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Quaternion.identity, transform);
        yield return new WaitForSeconds(duration);
        SetPlayerDmgReduction(1f);
        Destroy(currentEffect);
    }
    
    private float ApplyResistance(float damage, DamageType type)
    {
        if (type == DamageType.True)
            return damage;

        var resistance = _playerStats.PhysicalResistance;
        var multiplier = 100f / (100f + resistance * 2f);

        var reduced = Mathf.Floor(damage * multiplier);

        return Mathf.Max(reduced, 1f);
    }

    private void SetPlayerDmgReduction(float amount)
    {
        _playerStats.ShieldResistance = amount;

        PlayerStatisticsManager.Instance.SetPhysicalResistance(
            _playerStats.GetPhysicalReductionPercent());

        RefreshDamageReductionUI();
    }
    
    public void ClampHealth()
    {
        _playerStats.HP = Mathf.Min(_playerStats.HP, _playerStats.GetMaxHp());
    }
    
    public void RefreshResistanceUI()
    {
        PlayerStatisticsManager.Instance.SetPhysicalResistance(
            _playerStats.GetPhysicalReductionPercent());

        PlayerStatisticsManager.Instance.SetMoveSpeed(_playerStats.GetMoveSpeedMultiplier());

        RefreshDamageReductionUI();
        
        ClampHealth();
    }

    public void RefreshDamageReductionUI()
    {
        if (PlayerStatisticsManager.Instance == null)
            return;

        var bonusReduction = Mathf.Clamp01(
            _playerStats.GetBonusValue(BonusType.DamageReduction));
        var totalMultiplier = _playerStats.ShieldResistance * (1f - bonusReduction);
        var totalReductionPercent = (1f - totalMultiplier) * 100f;

        PlayerStatisticsManager.Instance.SetShieldReductionPercent(
            totalReductionPercent);
    }

    public float CurrentHealthPoints => CurrentHealth;
    public bool IsAlive => CurrentHealth > 0f;
}
