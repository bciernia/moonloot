using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private PlayerInput _playerInput;
    private bool _dialogueActive;
    
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
    [SerializeField] private TextMeshProUGUI _statsLevelTMP;
    [SerializeField] private TextMeshProUGUI _statsDamageTMP;

    [Header("Equipment Panel")] 
    [SerializeField] private Image _healthBarEq;
    [SerializeField] private Image _manaBarEq;
    // [SerializeField] private Image _staminaBar;
    [SerializeField] private TextMeshProUGUI _healthTMPEq;
    [SerializeField] private TextMeshProUGUI _manaTMPEq;
    
    [Header("Main menu Panel")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _optionsPanel;
    
    [Header("Enemy info")]
    [SerializeField] private GameObject _enemyInfoPanel;
    [SerializeField] private TextMeshProUGUI _enemyName;
    [SerializeField] private Image _enemyHealthBar;

    [Header("Skill buttons")] 
    [SerializeField] private TextMeshProUGUI _skill1KeyText;
    [SerializeField] private TextMeshProUGUI _skill2KeyText;
    
    [Header("Interaction button")]
    [SerializeField] private TextMeshProUGUI _interactionKeyText;
    
    [Header("Tab menu manager")]
    [SerializeField] private TabMenuManager _tabMenuManager;

    [Header("Tab menu manager")]
    [SerializeField] private GameObject _gameMenu;
    
    private void Start()
    {
        UpdateKeyTexts();
    }
    
    private void Update()
    {
        UpdatePlayerUI();
    }
    
    private void OnEnable()
    {
        var map = _playerInput.actions;

        map["Equipment"].performed += _ => OpenCloseTabPanel(0);
        map["Statistics"].performed += _ => OpenCloseTabPanel(1);
        map["Skills"].performed += _ => OpenCloseTabPanel(2);
        map["Journal"].performed += _ => OpenCloseTabPanel(3);
        map["Main menu"].performed += _ => HandleEscapePressed();
    }

    private void OnDisable()
    {
        var map = _playerInput.actions;
        
        map["Equipment"].performed -= _ => OpenCloseTabPanel(0);
        map["Statistics"].performed -= _ => OpenCloseTabPanel(1);
        map["Skills"].performed -= _ => OpenCloseTabPanel(2);
        map["Journal"].performed -= _ => OpenCloseTabPanel(3);
        map["Main menu"].performed -= _ => HandleEscapePressed();
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

    private void OpenCloseTabPanel(int tabIndex)
    {
        var isPanelActive = _gameMenu.activeSelf;

        if (isPanelActive)
        {
            _gameMenu.SetActive(false);
        }
        else
        {
            _gameMenu.SetActive(true);
            _tabMenuManager.SwitchToTab(tabIndex);    
        }
    }
    
    private void OpenClosePanel(GameObject panel)
    {
        panel.SetActive(!panel.activeSelf);
    }
    
    private void HandleEscapePressed()
    {
        //Przerwanie dialogu za pomocą Escape wyświetlało Main menu
        if (DialogueManager.Instance.IsInDialogue())
            return;
        
        if (TryCloseAnyPanel())
            return;

        OpenClosePanel(_mainMenuPanel);
    }

    private bool TryCloseAnyPanel()
    {
        GameObject[] panels = { _gameMenu, _optionsPanel };

        foreach (var panel in panels)
        {
            if (panel.name == _optionsPanel.name)
            {
                UpdateKeyTexts();
            }
            
            if (panel.activeSelf)
            {
                panel.SetActive(false);
                return true;
            }
        }

        return false;
    }


    private void UpdateKeyTexts()
    {
        if (_playerInput == null) return;

        var map = _playerInput.actions;
        var skill1 = map["Skill1"];
        var skill2 = map["Skill2"];
        var interaction = map["Interaction"];

        if (skill1 != null)
            _skill1KeyText.text = skill1.GetBindingDisplayString().ToUpper();
        if (skill2 != null)
            _skill2KeyText.text = skill2.GetBindingDisplayString().ToUpper();
        if (interaction != null)
            _interactionKeyText.text = interaction.GetBindingDisplayString().ToUpper();
    }
}
