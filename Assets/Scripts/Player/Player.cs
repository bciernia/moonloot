using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Configuration")] [SerializeField]
    private PlayerStatsSO _playerStats;

    public PlayerStatsSO PlayerStats => _playerStats;

    private PlayerAnimations _playerAnimations;
    
    private void Awake()
    {
        _playerAnimations = GetComponent<PlayerAnimations>();
    }

    public void ResetPlayer()
    {
        _playerStats.ResetPlayerStats();
        _playerAnimations.ResetPlayer();
    }
}
