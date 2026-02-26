using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable, IHealable
{
    [Header("Configuration")]
    [SerializeField] private PlayerStatsSO _playerStats;

    private PlayerAnimations _playerAnimations;

    public float CurrentHealth { get; private set; }
    
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
    }

    public void TakeDamage(float amount)
    {
        if (_playerStats.HP <= 0) return;

        _playerStats.HP -= amount;
        DamageManager.Instance.ShowDamageText(amount, transform);
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
        _playerStats.HP = Mathf.Min(_playerStats.HP + amount, _playerStats.MaxHP);
    }

    public void RestoreMana(float amount)
    {
        _playerStats.MP = Mathf.Min(_playerStats.MP + amount, _playerStats.MaxMP);
    }
    
    public bool CanRestoreHealth()
    {
        return _playerStats.HP > 0 && _playerStats.HP < _playerStats.MaxHP;
    }
    
    public bool CanRestoreMana()
    {
        return _playerStats.MP > 0 && _playerStats.MP < _playerStats.MaxMP;
    }
}
