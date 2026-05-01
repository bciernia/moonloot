using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable, IHealable, IShieldable, IHealth
{
    [Header("Configuration")]
    [SerializeField] private PlayerStatsSO _playerStats;

    private PlayerAnimations _playerAnimations;

    public float CurrentHealth { get; private set; }

    private Coroutine _shieldCoroutine;
    
    private void Awake()
    {
        _playerAnimations = GetComponent<PlayerAnimations>();
    }
    
    private void Start()
    {
        CurrentHealth = _playerStats.HP;
        
        if (_playerStats.HP <= 0)
        {
            PlayerDead();
        }

        PrepareStatistics();
    }

    private void PrepareStatistics()
    {
        // PlayerStatisticsManager.Instance.SetLevel(_playerStats.Level);
        Player.Instance.PlayerAttack.RecalculateDamage();
        // PlayerStatisticsManager.Instance.SetDamage(_playerStats.TotalDamage);
        PlayerStatisticsManager.Instance.SetPhysicalResistance(_playerStats.GetPhysicalReductionPercent());
        // PlayerStatisticsManager.Instance.SetMagicResistance(_playerStats.GetMagicReductionPercent());
        
        var shieldReductionPercent = (1f - _playerStats.ShieldResistance) * 100f;
        PlayerStatisticsManager.Instance.SetShieldReductionPercent(shieldReductionPercent);
        RefreshResistanceUI();
    }
    
    public void TakeDamage(float amount, Transform damageSourceType,  DamageType type)
    {
        if (_playerStats.HP <= 0) return;
        
        var afterArmor = ApplyResistance(amount, type);
        var reducedDamage = afterArmor * _playerStats.ShieldResistance;
        
        _playerStats.HP -= reducedDamage;
        FloatingTextManager.Instance.ShowDamageText(reducedDamage, transform);
        if (_playerStats.HP <= 0f)
        {
            _playerStats.HP = 0;
            PlayerDead();    
        }
    }

    private void PlayerDead()    
    {
        _playerAnimations.SetDeathAnimation();
    }

    public void RestoreHealth(float amount)
    {
        _playerStats.HP = Mathf.Min(_playerStats.HP + amount, _playerStats.GetMaxHp());
        FloatingTextManager.Instance.ShowHealText(amount, transform);
    }

    public void RestoreMana(float amount)
    {
        _playerStats.MP = Mathf.Min(_playerStats.MP + amount, _playerStats.GetMaxMp());
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

        var shieldReductionPercent = (1f - amount) * 100f;
        PlayerStatisticsManager.Instance.SetShieldReductionPercent(shieldReductionPercent);
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

        var shieldReductionPercent = (1f - _playerStats.ShieldResistance) * 100f;

        PlayerStatisticsManager.Instance.SetShieldReductionPercent(shieldReductionPercent);
        
        ClampHealth();
    }

    public float CurrentHealthPoints => CurrentHealth;
    public bool IsAlive => CurrentHealth > 0f;
}