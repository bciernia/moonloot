using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private PlayerInput _playerInput;
    
    private void Awake()
    {
        _playerInput = GetComponentInParent<PlayerInput>();
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
    [SerializeField] private Image _healthBarEq;
    [SerializeField] private Image _manaBarEq;
    // [SerializeField] private Image _staminaBar;
    [SerializeField] private TextMeshProUGUI _healthTMPEq;
    [SerializeField] private TextMeshProUGUI _manaTMPEq;
    
    [Header("Quest Panel")] 
    [SerializeField] private GameObject _questPanel;

    [Header("Skills Panel")] 
    [SerializeField] private GameObject _skillsPanel;
    
    [Header("Main menu Panel")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _optionsPanel;
    
    [Header("Enemy info")]
    [SerializeField] private GameObject _enemyInfoPanel;
    [SerializeField] private TextMeshProUGUI _enemyName;
    [SerializeField] private Image _enemyHealthBar;
    
    private void Update()
    {
        UpdatePlayerUI();
    }
    
    private void OnEnable()
    {
        var map = _playerInput.actions;

        map["Statistics"].performed += _ => OpenClosePanel(_statsPanel);
        map["Equipment"].performed += _ => OpenClosePanel(_equipmentPanel);
        map["Journal"].performed += _ => OpenClosePanel(_questPanel);
        map["Main menu"].performed += _ => HandleEscapePressed();
        map["Skills"].performed += _ => OpenClosePanel(_skillsPanel);
    }

    private void OnDisable()
    {
        var map = _playerInput.actions;

        map["Statistics"].performed -= _ => OpenClosePanel(_statsPanel);
        map["Equipment"].performed -= _ => OpenClosePanel(_equipmentPanel);
        map["Journal"].performed -= _ => OpenClosePanel(_questPanel);
        map["Main menu"].performed -= _ => HandleEscapePressed();
        map["Skills"].performed -= _ => OpenClosePanel(_skillsPanel);
    }
    
    private void UpdatePlayerUI()
    {
        _healthBar.fillAmount = Mathf.Lerp(_healthBar.fillAmount, _playerStatsSo.HP / _playerStatsSo.MaxHP,
            10f * Time.deltaTime);
        
        _manaBar.fillAmount = Mathf.Lerp(_manaBar.fillAmount, _playerStatsSo.MP / _playerStatsSo.MaxMP,
            10f * Time.deltaTime);
        
        _healthTMP.text = $"{_playerStatsSo.HP}/{_playerStatsSo.MaxHP}";
        _manaTMP.text = $"{_playerStatsSo.MP}/{_playerStatsSo.MaxMP}";
        
        _healthBarEq.fillAmount = Mathf.Lerp(_healthBarEq.fillAmount, _playerStatsSo.HP / _playerStatsSo.MaxHP,
            10f * Time.deltaTime);
        
        _manaBarEq.fillAmount = Mathf.Lerp(_manaBarEq.fillAmount, _playerStatsSo.MP / _playerStatsSo.MaxMP,
            10f * Time.deltaTime);
        
        _healthTMPEq.text = $"{_playerStatsSo.HP}/{_playerStatsSo.MaxHP}";
        _manaTMPEq.text = $"{_playerStatsSo.MP}/{_playerStatsSo.MaxMP}";
    }
    
    private void OpenClosePanel(GameObject panel)
    {
        panel.SetActive(!panel.activeSelf);
    }
    
    private void HandleEscapePressed()
    {
        if (TryCloseAnyPanel())
            return;

        OpenClosePanel(_mainMenuPanel);
    }

    private bool TryCloseAnyPanel()
    {
        GameObject[] panels = { _statsPanel, _equipmentPanel, _questPanel, _skillsPanel, _optionsPanel };

        foreach (var panel in panels)
        {
            if (panel.activeSelf)
            {
                panel.SetActive(false);
                return true;
            }
        }

        return false;
    }
}
