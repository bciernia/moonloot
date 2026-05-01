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
    
    public bool TryUseMana(float amount)
    {
        if (CurrentMana < amount)
        {
            Debug.Log(CurrentMana);
            Debug.Log(amount);
            
            FloatingTextManager.Instance.ShowWarningText("Not enough mana!", transform);
            
            return false;
        }
        
        _playerStats.MP = Mathf.Max(_playerStats.MP -= amount, 0f);
        CurrentMana = _playerStats.MP;
        return true;
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