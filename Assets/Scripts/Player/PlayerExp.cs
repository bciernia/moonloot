using System;
using UnityEngine;

public class PlayerExp : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PlayerStatsSO _playerStats;

    public static event Action<int> OnLevelUp;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            AddExp(30f);
        }
    }

    public void AddExp(float amount)
    {
        _playerStats.Exp += amount;
        while (_playerStats.Exp >= _playerStats.NextLevelExp)
        {
            _playerStats.Exp -= _playerStats.NextLevelExp;

            NextLevel();
        }
    }

    private void NextLevel()
    {
        _playerStats.Level++;
        var currentExpRequired = _playerStats.NextLevelExp;
        var newNextLevelExp = Mathf.Round(currentExpRequired + _playerStats.NextLevelExp * (_playerStats.ExpMultiplier / 100f));
        _playerStats.NextLevelExp = newNextLevelExp;
        
        Debug.Log(
            $"LEVEL UP! Level {_playerStats.Level}");
        
        OnLevelUp?.Invoke(_playerStats.Level);
    }
}