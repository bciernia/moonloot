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
    
    [Header("Task table")]
    [SerializeField] private GameObject _taskTablePanel;
    
    [Header("Save/Load panel")]
    [SerializeField] private GameObject _savePanel;
    [SerializeField] private GameObject _loadPanel;
    
    [Header("Player Game UI")]
    [SerializeField] private GameObject _mainGamePanel;
    [SerializeField] private GameObject _equippedPanel;

    [Header("Day Night Timer")] 
    [SerializeField] private GameObject _dayNightContainer;
    [SerializeField] private Image _dayNightTimerImage;
    [SerializeField] private Sprite[] _timeSprites;
    [SerializeField] private float _transitionDuration = 0.5f;

    [Header("Info panel")] 
    [SerializeField] private TextMeshProUGUI _nightNumber;

    [Header("Day night panels")] 
    [SerializeField] private GameObject _startNightPanel;
    [SerializeField] private GameObject _nightSummaryPanel;
    
    [Header("Horde info")]
    [SerializeField] private GameObject _hordeInfoContainer;
    [SerializeField] private GameObject _hordeInfo;
    [SerializeField] private TextMeshProUGUI _objectiveText;
    [SerializeField] private TextMeshProUGUI _mutationText;

    private float _waveTime;
    private bool _isTimerActive;
    
    private TextMeshProUGUI _enemyTMP;
    private TextMeshProUGUI _timerTMP;
    private TextMeshProUGUI _defendHpTMP;

    private float _clockTimer = 0f;
    private bool _isTransition = false;
    private float _mainFrameDuration;
    private int _currentClockIndex = 0;
    private DayNightCycle _dayNightCycle;
    
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
        _dayNightCycle = FindAnyObjectByType<DayNightCycle>();
        _dayNightCycle.OnDayStarted += ResetClock;
        _dayNightCycle.HordeAttack += ShowStartNightPanel;
        HordeManager.OnHordeStarted += SetupHordeUI;
        HordeManager.OnHordeFinished += ShowSummaryPanel;
        CacheInputActions();
        EnableInputActions();
        RegisterInputCallbacks();

        UpdateKeyTexts();
        UpdatePlayerUI(force: true);
        InitializeClock();
    }

    private void OnDestroy()
    {
        UnregisterInputCallbacks();
        HordeManager.OnHordeStarted -= SetupHordeUI;
        HordeManager.OnHordeFinished -= ShowSummaryPanel;
    }

    private void OnEnable()
    {
#pragma warning disable UDR0004
        GameManager.OnGameModeChanged += OnGameModeChanged;
#pragma warning restore UDR0004
        HordeManager.OnHordeFinished += UpdateNightUI;
    }

    private void OnDisable()
    {
        GameManager.OnGameModeChanged -= OnGameModeChanged;
        HordeManager.OnHordeFinished -= UpdateNightUI;
    }

    private void OnGameModeChanged(GameMode mode)
    {
        var isLocation = mode == GameMode.Location;
        _mainGamePanel.SetActive(isLocation);
        _equippedPanel.SetActive(isLocation);
        _dayNightTimerImage.gameObject.SetActive(isLocation);
        CloseAllPanels();
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
        UpdateClock();
        UpdateHordeUI();
    }

    #region UI Update
    private void UpdatePlayerUI(bool force = false)
    {
        if (_playerStatsSo == null) return;

        _displayedHp = Mathf.MoveTowards(_displayedHp, _playerStatsSo.HP, 20f * Time.deltaTime);
        _displayedMp = Mathf.MoveTowards(_displayedMp, _playerStatsSo.MP, 20f * Time.deltaTime);

        var hpRatio = _displayedHp / _playerStatsSo.MaxHP;
        var mpRatio = _displayedMp / _playerStatsSo.MaxMP;

        _healthBar.fillAmount = hpRatio;
        _manaBar.fillAmount = mpRatio;
        _healthBarEq.fillAmount = hpRatio;
        _manaBarEq.fillAmount = mpRatio;

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
        var isPanelActive = _gameMenu.activeSelf;

        _gameMenu.SetActive(!isPanelActive);

        //Tylko kiedy wchodzimy do panelu ze skillami
        if (tabIndex == 2)
        {
            foreach(var btn in FindObjectsOfType<UISkillBtn>())
            {
                btn.RefreshState();
            }
        }
        
        if (!isPanelActive)
        {
            TabMenuManager.Instance.SwitchToTab(tabIndex);
            PauseManager.Instance.RequestPause();
        }
        else
        {
            PauseManager.Instance.ReleasePause();
        }
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
        GameObject[] panels = { _gameMenu, _optionsPanel, _taskTablePanel, _loadPanel, _savePanel, _mainMenuPanel };

        foreach (var panel in panels)
        {
            if (!panel.activeSelf) continue;
            
            panel.SetActive(false);

            if (panel == _gameMenu || panel == _taskTablePanel)
                PauseManager.Instance.ReleasePause();

            if (panel == _optionsPanel)
                UpdateKeyTexts();

            return true;
        }

        return false;
    }
    private void CloseAllPanels()
    {
        GameObject[] panels = { _gameMenu, _optionsPanel, _taskTablePanel, _loadPanel, _savePanel, _mainMenuPanel };

        foreach (var panel in panels)
        {
            if (panel.activeSelf)
            {
                panel.SetActive(false);
            }
        }
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
    
    #region Day/Night Timer
    private void InitializeClock()
    {
        if (_dayNightCycle == null || _timeSprites.Length == 0)
            return;

        var spriteCount = _timeSprites.Length;

        var mainFrames = (spriteCount + 1) / 2;
        var transitions = spriteCount / 2;

        var totalTransitionTime = transitions * _transitionDuration;

        _mainFrameDuration = (_dayNightCycle.dayDuration - totalTransitionTime) / mainFrames;

        _currentClockIndex = 0;
        _clockTimer = 0f;
        _isTransition = false;

        _dayNightTimerImage.sprite = _timeSprites[0];
    }
    
    private void UpdateClock()
    {
        if (_timeSprites.Length == 0)
            return;

        if (GameManager.Instance.CurrentMode == GameMode.MainMenu)
            return;
        
        _clockTimer += Time.deltaTime;

        if (!_isTransition)
        {
            if (_clockTimer >= _mainFrameDuration)
            {
                _clockTimer = 0f;

                if (_currentClockIndex + 1 < _timeSprites.Length)
                {
                    _currentClockIndex++; 
                    _dayNightTimerImage.sprite = _timeSprites[_currentClockIndex];

                    _isTransition = true;
                }
            }
        }
        else
        {
            if (_clockTimer >= _transitionDuration)
            {
                _clockTimer = 0f;

                if (_currentClockIndex + 1 < _timeSprites.Length)
                {
                    _currentClockIndex++; 
                    _dayNightTimerImage.sprite = _timeSprites[_currentClockIndex];
                }

                _isTransition = false;
            }
        }
    }
    
    private void ResetClock()
    {
        _currentClockIndex = 0;
        _clockTimer = 0f;
        _isTransition = false;

        _dayNightTimerImage.sprite = _timeSprites[0];
    }
    #endregion

    private void UpdateNightUI(int night)
    {
        _nightNumber.text = $"{night}";
    }

    private void ShowSummaryPanel(int night)
    {
        _nightSummaryPanel.SetActive(true);
        PauseManager.Instance.RequestPause();
        ShowSummary();
    }
    
    public void OnReturnToDayClicked()
    {
        _nightSummaryPanel.SetActive(false);
        PauseManager.Instance.ReleasePause();
        _dayNightContainer.SetActive(true);
        _hordeInfoContainer.SetActive(false);
        
        HordeManager.Instance.ReturnToPreviousScene();
    }

    private void ShowStartNightPanel()
    {
        HordeManager.Instance.PrepareHorde();

        UpdateStartNightUI();

        _startNightPanel.SetActive(true);
        PauseManager.Instance.RequestPause();
    }
    
    private void UpdateStartNightUI()
    {
        var data = HordeManager.Instance.PreparedData;
        var mutation = HordeManager.Instance.PreparedMutation;

        _objectiveText.text = GetObjectiveDescription(data.objective);
        _mutationText.text = GetMutationDescription(mutation);
    }
    
    private string GetObjectiveDescription(HordeObjective obj)
    {
        return obj switch
        {
            HordeObjective.KillAll => "Kill all enemies",
            HordeObjective.DefendObject => "Defend the crystal",
            HordeObjective.EliteHunt => "Hunt elite enemies",
            _ => obj.ToString()
        };
    }
    
    private string GetMutationDescription(HordeMutation obj)
    {
        return obj switch
        {
            HordeMutation.BrutalEnemies => "Enemies attacks deal more damage",
            HordeMutation.FastEnemies => "Enemies will be very fast",
            HordeMutation.StrongEnemies => "Enemies are very tough",
            _ => "No mutations"
        };
    }
    
    public void OnStartNightClicked()
    {
        _startNightPanel.SetActive(false);
        PauseManager.Instance.ReleasePause();
        HordeManager.Instance.StartHorde();
    }

    private void SetupHordeUI()
    {
        _dayNightContainer.SetActive(false);
        _hordeInfoContainer.SetActive(true);

        ClearHordeUI();

        var objective = HordeManager.Instance.CurrentObjective;

        switch (objective)
        {
            case HordeObjective.KillAll:
            case HordeObjective.EliteHunt:
                CreateEnemyCounter();
                break;

            case HordeObjective.DefendObject:
                CreateDefendUI();
                break;
        }
    }
    
    private void ClearHordeUI()
    {
        foreach (Transform child in _hordeInfoContainer.transform)
        {
            Destroy(child.gameObject);
        }

        _enemyTMP = null;
        _timerTMP = null;
        _defendHpTMP = null;
    }
    
    private void CreateEnemyCounter()
    {
        var obj = Instantiate(_hordeInfo, _hordeInfoContainer.transform);
        _enemyTMP = obj.GetComponentInChildren<TextMeshProUGUI>();

        _enemyTMP.text = "Enemies: 0";
    }
    
    private void CreateDefendUI()
    {
        var timerObj = Instantiate(_hordeInfo, _hordeInfoContainer.transform);
        _timerTMP = timerObj.GetComponentInChildren<TextMeshProUGUI>();

        _waveTime = 60f;
        _isTimerActive = true;

        var hpObj = Instantiate(_hordeInfo, _hordeInfoContainer.transform);
        _defendHpTMP = hpObj.GetComponentInChildren<TextMeshProUGUI>();
    }
    
    private void UpdateHordeUI()
    {
        if (!_hordeInfoContainer.activeSelf) return;

        if (_enemyTMP != null)
        {
            var alive = HordeManager.Instance.GetRemainEnemies();
            _enemyTMP.text = $"Enemies: {alive}";
        }

        if (_isTimerActive && _timerTMP != null)
        {
            _waveTime -= Time.deltaTime;

            if (_waveTime < 0f)
                _waveTime = 0f;

            var m = Mathf.FloorToInt(_waveTime / 60f);
            var s = Mathf.FloorToInt(_waveTime % 60f);

            _timerTMP.text = $"{m:00}:{s:00}";
        }

        if (_defendHpTMP != null && HordeManager.Instance.DefendTarget != null)
        {
            var stats = HordeManager.Instance.DefendTarget.GetComponent<DefendTarget>();

            if (stats != null)
            {
                var percent = stats.currentHp / stats.maxHp * 100f;
                _defendHpTMP.text = $"HP: {percent:0}%";
            }
        }
    }
    
    public void ShowSummary()
    {
        var stats = CombatStatsManager.Instance;

        Debug.Log($"Damage: {stats.DamageDealt}");
        Debug.Log($"Enemies killed : {stats.EnemiesKilled}");
        Debug.Log($"Distance: {Mathf.RoundToInt(stats.DistanceTraveled)}m");
        
        // _damageText.text = $"Damage: {stats.DamageDealt}";
        // _killsText.text = $"Kills: {stats.EnemiesKilled}";
        // _distanceText.text = $"Distance: {Mathf.RoundToInt(stats.DistanceTraveled)}m";
    }
}
