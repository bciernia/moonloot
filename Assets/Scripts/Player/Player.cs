using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Configuration")] [SerializeField]
    private PlayerStatsSO _playerStats;

    [Header("Test")] 
    [SerializeField] private ItemHealthPotion ItemHealthPotion;
    [SerializeField] private ItemManaPotion ItemManaPotion;
    
    public PlayerHealth PlayerHealth { get; private set; }
    public PlayerMana PlayerMana { get; private set; }
    
    public PlayerStatsSO PlayerStats => _playerStats;
    private PlayerAnimations _playerAnimations;
    
    private void Awake()
    {
        PlayerHealth = GetComponent<PlayerHealth>();
        PlayerMana = GetComponent<PlayerMana>();
        _playerAnimations = GetComponent<PlayerAnimations>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            var isUsed = ItemHealthPotion.UseItem();
            if (isUsed)
            {
                Debug.Log("Used");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Y))
        {
            var isUsed = ItemManaPotion.UseItem();
            if (isUsed)
            {
                Debug.Log("Mana used");
            }
        }
    }

    public void ResetPlayer()
    {
        _playerStats.ResetPlayerStats();
        _playerAnimations.ResetPlayer();
    }
}
