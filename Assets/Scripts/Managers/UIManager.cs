using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private PlayerActions _actions;

    private void Awake()
    {
        _actions = new PlayerActions();
    }
    
    [Header("Stats")]
    [SerializeField] private PlayerStatsSO _playerStatsSo;
    
    [Header("Bars")]
    [SerializeField] private Image _healthBar;
    [SerializeField] private Image _manaBar;
    // [SerializeField] private Image _staminaBar;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI _healthTMP;
    [SerializeField] private TextMeshProUGUI _manaTMP;
    // [SerializeField] private TextMeshProUGUI _staminaTMP;

    [Header("Stats Panel")] 
    [SerializeField] private GameObject _statsPanel;
    [SerializeField] private TextMeshProUGUI _statsLevelTMP;
    [SerializeField] private TextMeshProUGUI _statsDamageTMP;

    [Header("Equipment Panel")] 
    [SerializeField] private GameObject _equipmentPanel;
    
    [Header("Enemy info")]
    [SerializeField] private GameObject _enemyInfoPanel;
    [SerializeField] private TextMeshProUGUI _enemyName;
    [SerializeField] private Image _enemyHealthBar;
    
    private void Update()
    {
        UpdatePlayerUI();
    }

    private void OpenCloseStatsPanel()
    {
        _statsPanel.SetActive(!_statsPanel.activeSelf);
        if (_statsPanel.activeSelf)
        {
            UpdateStatsPanel();
        }
    }

    private void UpdatePlayerUI()
    {
        _healthBar.fillAmount = Mathf.Lerp(_healthBar.fillAmount, _playerStatsSo.HP / _playerStatsSo.MaxHP,
            10f * Time.deltaTime);
        
        _manaBar.fillAmount = Mathf.Lerp(_manaBar.fillAmount, _playerStatsSo.MP / _playerStatsSo.MaxMP,
            10f * Time.deltaTime);
        
        _healthTMP.text = $"{_playerStatsSo.HP}/{_playerStatsSo.MaxHP}";
        _manaTMP.text = $"{_playerStatsSo.MP}/{_playerStatsSo.MaxMP}";
    }

    private void UpdateStatsPanel()
    {
        _statsLevelTMP.text = _playerStatsSo.Level.ToString();
        _statsDamageTMP.text = _playerStatsSo.TotalDamage.ToString(CultureInfo.InvariantCulture);
    }
    
    private void OnEnable()
    {
        _actions.UI.OpenCloseStatsPanel.performed += _ => OpenCloseStatsPanel();
        // _actions.UI.OpenCloseEquipmentPanel.performed += _ => OpenCloseEquipmentPanel();
        _actions.Enable();
    }

    private void OnDisable()
    {
        _actions.UI.OpenCloseStatsPanel.performed -= _ => OpenCloseStatsPanel();
        //_actions.UI.OpenCloseEquipmentPanel.performed -= _ => OpenCloseEquipmentPanel();
        _actions.Disable();
    }
}
