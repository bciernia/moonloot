using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Configuration")] [SerializeField]
    private PlayerStatsSO _playerStats;

    public PlayerHealth PlayerHealth { get; private set; }
    public PlayerMana PlayerMana { get; private set; }
    public PlayerAttack PlayerAttack { get; private set; }

    public static Player instance;
    public string areaTransitionName;
    
    public PlayerStatsSO PlayerStats => _playerStats;
    private PlayerAnimations _playerAnimations;
    
    private void Awake()
    {
        PlayerHealth = GetComponent<PlayerHealth>();
        PlayerMana = GetComponent<PlayerMana>();
        PlayerAttack = GetComponent<PlayerAttack>();
        _playerAnimations = GetComponent<PlayerAnimations>();
    }

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void ResetPlayer()
    {
        _playerStats.ResetPlayerStats();
        _playerAnimations.ResetPlayer();
    }
}
