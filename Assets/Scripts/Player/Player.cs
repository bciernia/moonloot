using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Configuration")] [SerializeField]
    private PlayerStatsSO _playerStats;

    public PlayerHealth PlayerHealth { get; private set; }
    public PlayerMana PlayerMana { get; private set; }
    public PlayerAttack PlayerAttack { get; private set; }

    public string areaTransitionName;
    
    public PlayerStatsSO PlayerStats => _playerStats;
    private PlayerAnimations _playerAnimations;
    private PlayerInput _playerInput;
    
    private void Awake()
    {
        PlayerHealth = GetComponent<PlayerHealth>();
        PlayerMana = GetComponent<PlayerMana>();
        PlayerAttack = GetComponent<PlayerAttack>();
        _playerAnimations = GetComponent<PlayerAnimations>();
        _playerInput = GetComponent<PlayerInput>();

        foreach (var map in _playerInput.actions.actionMaps)
        {
            map.Enable();
        }
    }

    public void ResetPlayer()
    {
        _playerStats.ResetPlayerStats();
        _playerAnimations.ResetPlayer();
    }
}
