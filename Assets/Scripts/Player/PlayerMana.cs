using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PlayerStatsSO _playerStats;

    public float CurrentMana { get; private set; }

    private void Start()
    {
        CurrentMana = _playerStats.MP;
    }
    
    public void UseMana(float amount)
    {
        _playerStats.MP = Mathf.Max(_playerStats.MP -= amount, 0f);
        CurrentMana = _playerStats.MP;
    }

    public void RecoverMana(float amount)
    {
        _playerStats.MP += amount;
        _playerStats.MP = Mathf.Min(_playerStats.MP, _playerStats.MaxMP);
    }

    
    public bool CanRecoverMana()
    {
        return _playerStats.MP < _playerStats.MaxMP;
    }
}