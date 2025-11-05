using System;
using EZCameraShake;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Configuration")]
    [SerializeField] private PlayerStatsSO _playerStats;

    private PlayerAnimations _playerAnimations;

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
    }

    public void TakeDamage(float amount)
    {
        if (_playerStats.HP <= 0) return;

        var reducedDamage = Math.Max(amount - _playerStats.DamageResistance, 1); 
        
        _playerStats.HP -= reducedDamage;
        DamageManager.Instance.ShowDamageText(reducedDamage, transform);
        CameraShaker.Instance.ShakeOnce(1f, 1f, 0f, .43f);
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
        _playerStats.HP += amount;
        _playerStats.HP = Mathf.Min(_playerStats.HP, _playerStats.MaxHP);
    }

    public void RestoreMana(float amount)
    {
        _playerStats.MP += amount;
        _playerStats.MP = Mathf.Min(_playerStats.MP, _playerStats.MaxMP);
    }
    
    public bool CanRestoreHealth()
    {
        return _playerStats.HP > 0 && _playerStats.HP < _playerStats.MaxHP;
    }
    
    public bool CanRestoreMana()
    {
        return _playerStats.MP > 0 && _playerStats.MP < _playerStats.MaxMP;
    }

    public float GetPlayerCurrentHealth()
    {
        return _playerStats.HP;
    }
}
