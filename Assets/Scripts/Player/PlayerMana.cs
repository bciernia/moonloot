using System;
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            UseMana(2f);
        }
    }
    
    public void UseMana(float amount)
    {
        _playerStats.MP = Mathf.Max(_playerStats.MP -= amount, 0f);
        CurrentMana = _playerStats.MP;
    }
}