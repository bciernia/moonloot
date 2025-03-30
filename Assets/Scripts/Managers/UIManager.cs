using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private PlayerStatsSO _playerStatsSo;
    
    [Header("Bars")]
    [SerializeField] private Image _healthBar;
    [SerializeField] private Image _manaBar;
    [SerializeField] private Image _staminaBar;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI _levelTMP;
    [SerializeField] private TextMeshProUGUI _healthTMP;
    [SerializeField] private TextMeshProUGUI _manaTMP;
    [SerializeField] private TextMeshProUGUI _staminaTMP;

    private void Update()
    {
        UpdatePlayerUI();
    }

    private void UpdatePlayerUI()
    {
        _healthBar.fillAmount = Mathf.Lerp(_healthBar.fillAmount, _playerStatsSo.HP / _playerStatsSo.MaxHP,
            10f * Time.deltaTime);
        
        _manaBar.fillAmount = Mathf.Lerp(_manaBar.fillAmount, _playerStatsSo.MP / _playerStatsSo.MaxMP,
            10f * Time.deltaTime);
        
        _staminaBar.fillAmount = Mathf.Lerp(_staminaBar.fillAmount, _playerStatsSo.Stamina / _playerStatsSo.MaxStamina,
            10f * Time.deltaTime);
        
        _healthTMP.text = $"{_playerStatsSo.HP}/{_playerStatsSo.MaxHP}";
        _manaTMP.text = $"{_playerStatsSo.MP}/{_playerStatsSo.MaxMP}";
        _staminaTMP.text = $"{_playerStatsSo.Stamina}/{_playerStatsSo.MaxStamina}";
    }
}
