using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerExp : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PlayerStats _playerStats;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            AddExp(100f);
        }
    }

    private void AddExp(float amount)
    {
        _playerStats.Exp += amount;
        if (_playerStats.Exp >= _playerStats.NextLevelExp)
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
    }
}