using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private PlayerInput _playerInput;

    [Header("Stats")]
    [SerializeField] private PlayerStatsSO _playerStatsSo;

    [Header("Bars")]
    [SerializeField] private Image _healthBar;
    [SerializeField] private Image _manaBar;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI _healthTMP;
    [SerializeField] private TextMeshProUGUI _manaTMP;

    [Header("Stats Panel")]
    [SerializeField] private TextMeshProUGUI _statsLevelTMP;
    [SerializeField] private TextMeshProUGUI _statsDamageTMP;

    [Header("Equipment Panel")]
    [SerializeField] private Image _healthBarEq;
    [SerializeField] private Image _manaBarEq;
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
    [SerializeField] private GameObject _gameMenu;

    private InputAction _equipmentAction;
    private InputAction _statsAction;
    private InputAction _skillsAction;
    private InputAction _journalAction;
    private InputAction _menuAction;

    private float _displayedHp;
    private float _displayedMp;

    private void Awake()
    {
        _playerInput = FindAnyObjectByType<PlayerInput>();

        if (_playerStatsSo != null)
        {
            _displayedHp = _playerStatsSo.HP;
            _displayedMp = _playerStatsSo.MP;
        }
    }

    private void Start()
    {
        CacheInputActions();
        EnableInputActions();
        RegisterInputCallbacks();

        UpdateKeyTexts();
        UpdatePlayerUI(force: true);
    }

    private void OnDestroy()
    {
        UnregisterInputCallbacks();
    }

    #region Input System Setup
    private void CacheInputActions()
    {
        var map = _playerInput.actions;
        _equipmentAction = map["Equipment"];
        _statsAction = map["Statistics"];
        _skillsAction = map["Skills"];
        _journalAction = map["Journal"];
        _menuAction = map["Main menu"];
    }

    private void EnableInputActions()
    {
        if (!_playerInput.actions.enabled)
            _playerInput.actions.Enable();
    }

    private void RegisterInputCallbacks()
    {
        if (_equipmentAction != null) _equipmentAction.performed += OnEquipmentPressed;
        if (_statsAction != null) _statsAction.performed += OnStatsPressed;
        if (_skillsAction != null) _skillsAction.performed += OnSkillsPressed;
        if (_journalAction != null) _journalAction.performed += OnJournalPressed;
        if (_menuAction != null) _menuAction.performed += OnMenuPressed;
    }

    private void UnregisterInputCallbacks()
    {
        if (_equipmentAction != null) _equipmentAction.performed -= OnEquipmentPressed;
        if (_statsAction != null) _statsAction.performed -= OnStatsPressed;
        if (_skillsAction != null) _skillsAction.performed -= OnSkillsPressed;
        if (_journalAction != null) _journalAction.performed -= OnJournalPressed;
        if (_menuAction != null) _menuAction.performed -= OnMenuPressed;
    }
    #endregion

    private void Update()
    {
        UpdatePlayerUI();
    }

    #region UI Update
    private void UpdatePlayerUI(bool force = false)
    {
        if (_playerStatsSo == null) return;

        // Płynna animacja wyświetlanej wartości
        _displayedHp = Mathf.MoveTowards(_displayedHp, _playerStatsSo.HP, 20f * Time.deltaTime);
        _displayedMp = Mathf.MoveTowards(_displayedMp, _playerStatsSo.MP, 20f * Time.deltaTime);

        float hpRatio = _displayedHp / _playerStatsSo.MaxHP;
        float mpRatio = _displayedMp / _playerStatsSo.MaxMP;

        // Aktualizacja pasków HP/MP
        _healthBar.fillAmount = hpRatio;
        _manaBar.fillAmount = mpRatio;
        _healthBarEq.fillAmount = hpRatio;
        _manaBarEq.fillAmount = mpRatio;

        // Aktualizacja tekstów
        _healthTMP.text = $"{(int)_displayedHp}/{_playerStatsSo.MaxHP}";
        _manaTMP.text = $"{(int)_displayedMp}/{_playerStatsSo.MaxMP}";
        _healthTMPEq.text = $"{(int)_displayedHp}/{_playerStatsSo.MaxHP}";
        _manaTMPEq.text = $"{(int)_displayedMp}/{_playerStatsSo.MaxMP}";
    }
    #endregion

    #region Input Callbacks
    private void OnEquipmentPressed(InputAction.CallbackContext ctx) => OpenCloseEquipmentPanel(0);
    private void OnStatsPressed(InputAction.CallbackContext ctx) => OpenCloseTabPanel(1);
    private void OnSkillsPressed(InputAction.CallbackContext ctx) => OpenCloseTabPanel(2);
    private void OnJournalPressed(InputAction.CallbackContext ctx) => OpenCloseTabPanel(3);
    private void OnMenuPressed(InputAction.CallbackContext ctx) => HandleEscapePressed();
    #endregion

    #region Menu & Panels
    private void OpenCloseEquipmentPanel(int tabIndex)
    {
        if (ShopManager.Instance != null && ShopManager.Instance.ShopPanel != null)
            ShopManager.Instance.ShopPanel.SetActive(false);

        OpenCloseTabPanel(tabIndex);
    }

    private void OpenCloseTabPanel(int tabIndex)
    {
        bool isPanelActive = _gameMenu.activeSelf;

        _gameMenu.SetActive(!isPanelActive);
        Time.timeScale = isPanelActive ? 1f : 0f;

        if (!isPanelActive)
            TabMenuManager.Instance.SwitchToTab(tabIndex);
    }

    private void HandleEscapePressed()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue())
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
            if (panel.activeSelf)
            {
                panel.SetActive(false);
                if (panel == _optionsPanel)
                    UpdateKeyTexts();
                return true;
            }
        }

        return false;
    }

    private void OpenClosePanel(GameObject panel)
    {
        panel.SetActive(!panel.activeSelf);
    }
    #endregion

    private void UpdateKeyTexts()
    {
        if (_playerInput == null) return;

        var map = _playerInput.actions;
        _skill1KeyText.text = map["Skill1"]?.GetBindingDisplayString().ToUpperInvariant();
        _skill2KeyText.text = map["Skill2"]?.GetBindingDisplayString().ToUpperInvariant();
        _interactionKeyText.text = map["Interaction"]?.GetBindingDisplayString().ToUpperInvariant();
    }
}
