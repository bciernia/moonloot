using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UIManager : MonoBehaviour
{
    private PlayerInput _playerInput;

    [Header("Stats")]
    [SerializeField] private PlayerStatsSO _playerStatsSo;

    [Header("Bars")]
    [SerializeField] private Image _healthBar;
    [SerializeField] private Image _levelBar;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI _healthTMP;
    [SerializeField] private TextMeshProUGUI _levelTMP;

    [Header("Equipment Panel")]
    [SerializeField] private Image _healthBarEq;
    [SerializeField] private Image _manaBarEq;
    [SerializeField] private TextMeshProUGUI _healthTMPEq;
    [SerializeField] private TextMeshProUGUI _manaTMPEq;
    
    [Header("Skill buttons")]
    [SerializeField] private TextMeshProUGUI _skill1KeyText;
    [SerializeField] private TextMeshProUGUI _skill2KeyText;
    [SerializeField] private TextMeshProUGUI _quickItem1KeyText;
    [SerializeField] private TextMeshProUGUI _quickItem2KeyText;

    [Header("Interaction button")]
    [SerializeField] private TextMeshProUGUI _interactionKeyText;

    [Header("Tab menu manager")]
    [SerializeField] private GameObject _gameMenu;
    
    [Header("Task table")]
    [SerializeField] private GameObject _taskTablePanel;
    
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
    
    [Header("Boss night panel")]
    [SerializeField] private GameObject _bossFightPanel;
    [SerializeField] private TextMeshProUGUI _bossNameTMP;
    [SerializeField] private TextMeshProUGUI _bossDescriptionTMP;
    [SerializeField] private Image _bossImage;
    [SerializeField] private Button _bossNightButton;

    [Header("Night summary panel")] 
    [SerializeField] private GameObject _bonusesSummaryPanel;
    [SerializeField] private GameObject _levelSummaryPanel;
    [SerializeField] private GameObject _summaryInfoObject;
    [SerializeField] private GameObject _exitSummaryBtn;
    
    [Header("Horde info")]
    [SerializeField] private GameObject _hordeInfoContainer;
    [SerializeField] private GameObject _waveInfoContainer;
    [SerializeField] private GameObject _endlessWaveInfoContainer;
    [SerializeField] private GameObject _endlessWaveInfoTMP;
    [SerializeField] private TextMeshProUGUI _enemyTMP;
    [SerializeField] private TextMeshProUGUI _waveNumberTMP;
    [SerializeField] private TextMeshProUGUI _nextWaveTMP;
    [SerializeField] private TextMeshProUGUI _timerTMP;
    [SerializeField] private TextMeshProUGUI _objectiveTMP;

    [Header("Choose npc buttons")] 
    [SerializeField] private NpcDatabase _npcDatabase;
    [SerializeField] private Transform _npcContainer;
    [SerializeField] private GameObject _npcButtonPrefab;
    
    [Header("Night selection")]
    [SerializeField] private TextMeshProUGUI _nightTitle;
    [SerializeField] private TextMeshProUGUI _nightObjective;
    [SerializeField] private Image _nightObjectiveImage;
    [SerializeField] private TextMeshProUGUI _mutation;
    [SerializeField] private Image _mutationImage;
    [SerializeField] private TextMeshProUGUI _locationName;
    [SerializeField] private GameObject _nightRewardContainer;
    [SerializeField] private RewardElement _nightRewardPrefab;

    [SerializeField] private Image _nightPreviewImage;

    [SerializeField] private Button _startNightButton;

    [Header("Moon objective")]
    [SerializeField] private Image _moonObjectiveImageFinished;
    [SerializeField] private TextMeshProUGUI _moonObjectiveText;
    
    [Header("Minimap")]
    [SerializeField] private GameObject _minimapContainer;
    
    private float _waveTime;
    private bool _isTimerActive;

    private float _clockTimer = 0f;
    private bool _isTransition = false;
    private float _mainFrameDuration;
    private int _currentClockIndex = 0;
    private DayNightCycle _dayNightCycle;
    
    private InputAction _equipmentAction;
    private InputAction _skillsAction;
    private InputAction _journalAction;
    private InputAction _menuAction;

    private float _displayedHp;
    private float _displayedExp;
    private VillageNpcRuntime _selectedNPC;

    private bool _portalSpawned;
    
    private Coroutine _objectiveFlashRoutine;
    
    private GameObject MainMenuPanel =>
        PersistentMenuManager.Instance.MainMenuPanel;

    private GameObject OptionsPanel =>
        PersistentMenuManager.Instance.OptionsPanel;

    private GameObject LoadPanel =>
        PersistentMenuManager.Instance.LoadPanel;
    
    private int _lastLevel;

    private void Awake()
    {
        _playerInput = FindAnyObjectByType<PlayerInput>();

        if (_playerStatsSo != null)
        {
            _displayedHp = _playerStatsSo.HP;
            _displayedExp = _playerStatsSo.Exp;
        }
        _lastLevel = _playerStatsSo.Level;
    }
    
    private void Start()
    {
        _dayNightCycle = FindAnyObjectByType<DayNightCycle>();
        _dayNightCycle.OnDayStarted += ResetClock;
        _dayNightCycle.HordeAttack += ShowStartNightPanel;
        HordeManager.OnHordeStarted += SetupHordeUI;
        HordeManager.OnHordeFinished += ShowSummaryPanel;
        HordeManager.OnExitSpawned += OnPortalSpawned;
        HordeManager.OnExitRemoved += OnPortalRemoved;
        SceneManager.sceneLoaded += OnSceneLoaded;
        HordeManager.OnEndlessStarted += ShowEndlessUI;

        
        CacheInputActions();
        EnableInputActions();
        RegisterInputCallbacks();

        UpdateKeyTexts();
        UpdatePlayerUI(force: true);
        
        //_playerStatsSo.ResetBonuses();
        UpdateStatsPanel();
        InitializeClock();

        RefreshMoonObjective();

        HordeManager.Instance.OnObjectiveProgressChanged += UpdateMoonObjectiveUI;
        
        RefreshMinimapVisibility();
        RefreshHordeInfoVisibility();
    }

    private void ShowEndlessUI()
    {
        _waveNumberTMP.text = "ENDLESS";
        _nextWaveTMP.text = "";
    }

    private void OnPortalSpawned(Transform exitTransform)
    {
        _portalSpawned = true;

        UpdateMoonObjectiveUI(
            HordeManager.Instance.CurrentObjectiveProgress,
            HordeManager.Instance.CurrentObjectiveTarget
        );
    }

    private void OnPortalRemoved()
    {
        _portalSpawned = false;

        RefreshMoonObjective();
    }

    private void OnDestroy()
    {
        UnregisterInputCallbacks();
        HordeManager.OnHordeStarted -= SetupHordeUI;
        HordeManager.OnHordeFinished -= ShowSummaryPanel;
        HordeManager.OnExitSpawned -= OnPortalSpawned;
        HordeManager.OnExitRemoved -= OnPortalRemoved;
        HordeManager.OnEndlessStarted -= ShowEndlessUI;

        if (HordeManager.Instance != null)
        {
            HordeManager.Instance.OnObjectiveProgressChanged -= UpdateMoonObjectiveUI;
        }
    }

    private void OnEnable()
    {
#pragma warning disable UDR0004
        GameManager.OnGameModeChanged += OnGameModeChanged;
#pragma warning restore UDR0004
        HordeManager.OnHordeFinished += UpdateNightUI;
        PlayerExp.OnLevelUp += HandleLevelUp;

    }

    private void OnDisable()
    {
        GameManager.OnGameModeChanged -= OnGameModeChanged;
        HordeManager.OnHordeFinished -= UpdateNightUI;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerExp.OnLevelUp -= HandleLevelUp;

    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        RefreshMinimapVisibility();
        RefreshHordeInfoVisibility();
        
        if (GameManager.Instance.CurrentMode == GameMode.MainMenu)
        {
            HidePersistentGameplayUI();
        }
    }

    private void HidePersistentGameplayUI()
    {
        _hordeInfoContainer.SetActive(false);
        _nightSummaryPanel.SetActive(false);
        _startNightPanel.SetActive(false);
    }
    
    private void HandleLevelUp(int obj)
    {
        _displayedExp = 0;
    }

    private void OnGameModeChanged(GameMode mode)
    {
        var isLocation = mode == GameMode.Location;
        
        _mainGamePanel.SetActive(isLocation);
        _equippedPanel.SetActive(isLocation);
        _dayNightTimerImage.gameObject.SetActive(isLocation);
        
        RefreshMinimapVisibility();
        RefreshHordeInfoVisibility();
        
        CloseAllPanels();
    }

    #region Input System Setup
    private void CacheInputActions()
    {
        var map = _playerInput.actions;
        _equipmentAction = map["Equipment"];
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
        if (_skillsAction != null) _skillsAction.performed += OnSkillsPressed;
        if (_journalAction != null) _journalAction.performed += OnJournalPressed;
        if (_menuAction != null) _menuAction.performed += OnMenuPressed;
    }

    private void UnregisterInputCallbacks()
    {
        if (_equipmentAction != null) _equipmentAction.performed -= OnEquipmentPressed;
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
        
        if (_gameMenu.activeSelf)
        {
            UpdateStatsPanel();
        }
    }

    #region UI Update
    private void UpdatePlayerUI(bool force = false)
    {
        if (_playerStatsSo == null) return;

        if (_playerStatsSo.Level > _lastLevel)
        {
            _displayedExp = _playerStatsSo.Exp;
            _lastLevel = _playerStatsSo.Level;
        }
        
        _displayedHp = Mathf.MoveTowards(_displayedHp, _playerStatsSo.HP, 100f * Time.unscaledDeltaTime);
        _displayedExp = Mathf.MoveTowards(_displayedExp, _playerStatsSo.Exp, 100f * Time.unscaledDeltaTime);

        var maxHpWithBonuses = _playerStatsSo.GetMaxHp();
        
        var hpRatio = _displayedHp / maxHpWithBonuses;
        var expRatio = _displayedExp / _playerStatsSo.NextLevelExp;
        
        _healthBar.fillAmount = hpRatio;
        _healthBarEq.fillAmount = hpRatio;
        _levelBar.fillAmount = expRatio;
        
        _healthTMP.text =
            $"{FormatNumber(_displayedHp)}/{FormatNumber(maxHpWithBonuses)}";

        _healthTMPEq.text =
            $"{FormatNumber(_displayedHp)}/{FormatNumber(maxHpWithBonuses)}";
        
        _levelTMP.text =
            $"Lv. {_playerStatsSo.Level} | {FormatNumber(_displayedExp)}/{FormatNumber(_playerStatsSo.NextLevelExp)}";
    }
    #endregion
    
    private string FormatNumber(float value)
    {
        if (value >= 1_000_000_000)
            return $"{value / 1_000_000_000f:0.#}B";

        if (value >= 1_000_000)
            return $"{value / 1_000_000f:0.#}M";

        if (value >= 1_000)
            return $"{value / 1_000f:0.#}K";

        return Mathf.RoundToInt(value).ToString();
    }

    #region Input Callbacks
    private void OnEquipmentPressed(InputAction.CallbackContext ctx) => OpenCloseEquipmentPanel(0);
    private void OnSkillsPressed(InputAction.CallbackContext ctx) => OpenCloseTabPanel(1);
    private void OnJournalPressed(InputAction.CallbackContext ctx) => OpenCloseTabPanel(2);
    private void OnMenuPressed(InputAction.CallbackContext ctx) => HandleEscapePressed();
    #endregion

    #region Menu & Panels
    private void OpenCloseEquipmentPanel(int tabIndex)
    {
        if (ShopManager.Instance != null && ShopManager.Instance.ShopPanel != null)
            ShopManager.Instance.ShopPanel.SetActive(false);

        InventoryController.Instance.ResetSelectedItem();
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
            
        OpenClosePanel(MainMenuPanel);
    }

    private bool TryCloseAnyPanel()
    {
        GameObject[] panels = { _gameMenu, OptionsPanel, _taskTablePanel, LoadPanel, MainMenuPanel };

        foreach (var panel in panels)
        {
            if (!panel.activeSelf) continue;
            
            panel.SetActive(false);

            if (panel == _gameMenu || panel == _taskTablePanel)
                PauseManager.Instance.ReleasePause();

            if (panel == OptionsPanel)
                UpdateKeyTexts();

            return true;
        }

        return false;
    }
    private void CloseAllPanels()
    {
        GameObject[] panels = { _gameMenu, OptionsPanel, _taskTablePanel, LoadPanel, MainMenuPanel };

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
        _quickItem1KeyText.text = map["QuickItem1"]?.GetBindingDisplayString().ToUpperInvariant();
        _quickItem2KeyText.text = map["QuickItem2"]?.GetBindingDisplayString().ToUpperInvariant();
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
        if (NpcRescuedCount() > 0) ApplyNpcBonuses();
        PauseManager.Instance.RequestPause();
        _exitSummaryBtn.SetActive(false);
        StartCoroutine(ShowSummary());
    }

    private void ApplyNpcBonuses()
    {
        NPCManager.Instance.ApplyNPC(_selectedNPC);
        UpdateStatsPanel();
    }
    
    public void OnReturnToDayClicked()
    {
        _selectedNPC = null;

        _portalSpawned = false;

        _nightSummaryPanel.SetActive(false);

        PauseManager.Instance.ReleasePause();

        _dayNightContainer.SetActive(true);
        _hordeInfoContainer.SetActive(false);
        _waveInfoContainer.SetActive(true);
        _endlessWaveInfoContainer.SetActive(false);
        HordeManager.Instance.ReturnToPreviousScene();
    }

    private void ShowStartNightPanel()
    {
        HordeManager.Instance.PrepareHorde();

        SetupNightPanel();

        _startNightPanel.SetActive(true);

        PauseManager.Instance.RequestPause();
    }

    public void ShowStartBossNightPanel(NightLocationSO nightLocationSo)
    {
        HordeManager.Instance.PrepareBossFight(nightLocationSo);

        SetupBossNightPanel(nightLocationSo.BossMoon);

        _bossFightPanel.SetActive(true);

        PauseManager.Instance.RequestPause();
    }
    
    private void StartSelectedNight(VillageNpcRuntime npc)
    {
        CombatStatsManager.Instance.ResetStats(Player.Instance.transform);

        if (npc != null)
        {
            SelectNPC(npc);
        }

        _startNightPanel.SetActive(false);
        _bossFightPanel.SetActive(false);

        if (HordeManager.Instance.CurrentNightLocation.IsBossArena)
        {
            HordeManager.Instance.StartBossFight();
            return;
        }
        
        PauseManager.Instance.ReleasePause();
        HordeManager.Instance.StartHorde();
    }
    
    public void OnStartNightClicked(VillageNpcRuntime chosenNpc)
    {
        SelectNPC(chosenNpc);

        // WorldManager.Instance.AddNpc(chosenNpc);
        
        //TODO Animation + sound
        
        _startNightPanel.SetActive(false);
        PauseManager.Instance.ReleasePause();
        HordeManager.Instance.StartHorde();
        
        // StartCoroutine(StartNightWithDelay());
    }
    
    private IEnumerator StartNightWithDelay()
    {
        //TODO jak będzie animacja to wtedy zwiększyć ten czas
        yield return new WaitForSecondsRealtime(0.0f);

        _startNightPanel.SetActive(false);
        PauseManager.Instance.ReleasePause();
        HordeManager.Instance.StartHorde();
    }
    
    private void SetupHordeInfoUI()
    {
        _waveInfoContainer.SetActive(true);
        _endlessWaveInfoContainer.SetActive(false);
        
        _waveNumberTMP.text = "Wave: 1";
        _nextWaveTMP.text = "Next wave in: 90";

        var moon = MoonManager.Instance.CurrentMoon;

        if (moon != null)
        {
            _objectiveTMP.text =
                $"{moon.ObjectiveText}: {HordeManager.Instance.CurrentObjectiveProgress}/{HordeManager.Instance.CurrentObjectiveTarget}";
        }

        _isTimerActive = false;
    }

    private void SetupHordeUI()
    {
        RefreshMoonObjective();

        _moonObjectiveImageFinished.gameObject.SetActive(false);
        _dayNightContainer.SetActive(false);

        SetupHordeInfoUI();
    }
    
    private void UpdateHordeUI()
    {
        if (!_hordeInfoContainer.activeSelf)
            return;

        if (_waveNumberTMP != null)
        {
            if (HordeManager.Instance.IsEndlessStarted)
            {
                _waveInfoContainer.SetActive(false);
                _endlessWaveInfoContainer.SetActive(true);
            }
            else
            {
                _waveNumberTMP.text =
                    $"Wave: {HordeManager.Instance.GetCurrentWave()}";
            }
        }

        if (_nextWaveTMP != null)
        {
            if (HordeManager.Instance.IsEndlessStarted)
            {
                _nextWaveTMP.text = "";
            }
            else
            {
                var time = Mathf.CeilToInt(
                    HordeManager.Instance.GetTimeToNextWave()
                );

                _nextWaveTMP.text = time.ToString();
            }
        }
        
        if (_enemyTMP != null)
        {
            var alive = HordeManager.Instance.GetRemainEnemies();
            _enemyTMP.text = $"Enemies: {alive}";
        }
        
        var moon = MoonManager.Instance.CurrentMoon;

        if (moon != null)
        {
            _objectiveTMP.text =
                $"{moon.ObjectiveText}: {HordeManager.Instance.CurrentObjectiveProgress}/{HordeManager.Instance.CurrentObjectiveTarget}";
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
    }
    
    private IEnumerator ShowNormalNightSummary()
    {
        var stats = CombatStatsManager.Instance;

        CreateSummaryText("Night survived!", _bonusesSummaryPanel.transform);

        yield return new WaitForSecondsRealtime(.75f);

        CreateSummaryText($"Dealt damage: {stats.DamageDealt:0}", _levelSummaryPanel.transform);
        yield return new WaitForSecondsRealtime(.75f);

        CreateSummaryText($"Earned gold: {stats.GoldEarned}", _levelSummaryPanel.transform);
        yield return new WaitForSecondsRealtime(.75f);

        CreateSummaryText($"Defeated minions: {stats.NormalEnemiesKilled}", _levelSummaryPanel.transform);
        yield return new WaitForSecondsRealtime(.75f);

        CreateSummaryText($"Defeated elites: {stats.EliteEnemiesKilled}", _levelSummaryPanel.transform);
        yield return new WaitForSecondsRealtime(.75f);

        CreateSummaryText($"Defeated bosses: {stats.BossEnemiesKilled}", _levelSummaryPanel.transform);
        yield return new WaitForSecondsRealtime(.75f);

        CreateSummaryText($"Distance traveled: {Mathf.RoundToInt(stats.DistanceTraveled)}m", _levelSummaryPanel.transform);

        yield return new WaitForSecondsRealtime(.75f);

        _exitSummaryBtn.SetActive(true);
    }
    
    private IEnumerator ShowSummary()
    {
        foreach (Transform child in _bonusesSummaryPanel.transform)
            Destroy(child.gameObject);

        foreach (Transform child in _levelSummaryPanel.transform)
            Destroy(child.gameObject);
        
        if (_selectedNPC == null)
        {
            yield return ShowNormalNightSummary();
            yield break;
        }
        
        var stats = CombatStatsManager.Instance;

        switch (_selectedNPC.Data)
        {
            case NPCStat statNpc:
                yield return ShowNpcStatSummary(statNpc);
                break;
            
            case NPCMerchant merchantNpc:
                ShowNpcMerchantSummary(merchantNpc);
                break;
            
            case NPCHero heroNpc:
                ShowNpcHeroSummary(heroNpc);
                break;
            
            default:
                Debug.LogWarning("Unknown NPC type");
                break;
        }
            
        CreateSummaryText($"Dealt damage: {stats.DamageDealt:0}", _levelSummaryPanel.transform);
        yield return new WaitForSecondsRealtime(.75f);
        CreateSummaryText($"Earned gold: {stats.GoldEarned}", _levelSummaryPanel.transform);
        yield return new WaitForSecondsRealtime(.75f);
        CreateSummaryText($"Defeated minions: {stats.NormalEnemiesKilled}", _levelSummaryPanel.transform);
        yield return new WaitForSecondsRealtime(.75f);
        CreateSummaryText($"Defeated elites: {stats.EliteEnemiesKilled}", _levelSummaryPanel.transform);
        yield return new WaitForSecondsRealtime(.75f);
        CreateSummaryText($"Defeated bosses: {stats.BossEnemiesKilled}", _levelSummaryPanel.transform);
        yield return new WaitForSecondsRealtime(.75f);
        CreateSummaryText($"Distance traveled: {Mathf.RoundToInt(stats.DistanceTraveled)}m", _levelSummaryPanel.transform);
        yield return new WaitForSecondsRealtime(.75f);
        
        _exitSummaryBtn.SetActive(true);
    }

    private void ShowNpcMerchantSummary(NPCMerchant merchantNpc)
    {
        var obj = Instantiate(_summaryInfoObject, _bonusesSummaryPanel.transform);
        var tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = $"Saved {merchantNpc.Name}";
    }

    private void ShowNpcHeroSummary(NPCHero heroNpc)
    {
        var obj = Instantiate(_summaryInfoObject, _bonusesSummaryPanel.transform);
        var tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = $"Saved {heroNpc.Name}";   
    }

    private IEnumerator ShowNpcStatSummary(NPCStat npc)
    {
        if (_selectedNPC == null || _selectedNPC.UpgradeLevels[0]?.Bonuses == null) yield break;

        if (NpcRescuedCount() == 0)
        {
            CreateSummaryText("You did not find the peasant.", _bonusesSummaryPanel.transform, BonusType.Damage);
            yield return new WaitForSecondsRealtime(.75f);

        }
        else
        {
            foreach (var bonus in _selectedNPC.UpgradeLevels[0].Bonuses)
            {
                CreateSummaryText("New villager saved!", _bonusesSummaryPanel.transform);
                var text = FormatBonus(bonus);
                CreateSummaryText(text, _bonusesSummaryPanel.transform, bonus.Type);
                yield return new WaitForSecondsRealtime(.75f);
            }
        }
    }

    private int NpcRescuedCount() => HordeManager.Instance.NpcRescuedCount();

    private List<VillageNpcRuntime> GetRandomNPCs(int count)
    {
        var result = new List<VillageNpcRuntime>();

        var rescued = WorldManager.Instance.RescuedNpcs;

        var list = _npcDatabase.NpcDatas
            .Where(npcData => rescued.All(r => r.Data != npcData))
            .ToList();

        if (list.Count == 0)
        {
            Debug.Log("No more NPCs to spawn");
            return result;
        }

        for (var i = 0; i < list.Count; i++)
        {
            var temp = list[i];
            var randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }

        for (var i = 0; i < count && i < list.Count; i++)
        {
            result.Add(new VillageNpcRuntime(list[i]));
        }

        return result;
    }
    
    private void ClearNPCButtons()
    {
        for (var i = _npcContainer.childCount - 1; i >= 0; i--)
        {
            var child = _npcContainer.GetChild(i);

            if (child != null)
                Destroy(child.gameObject);
        }
    }
    
    private void SpawnNPCButtons()
    {
        if (!LoadingSceneManager.Instance.IsSceneBase())
            return;

        if (_npcContainer == null) return;
            
        ClearNPCButtons();

        var npcs = GetRandomNPCs(3);

        if (npcs.Count == 0)
        {
            Debug.Log("All npcs collected");
            return;
        }

        foreach (var npc in npcs)
        {
            var obj = Instantiate(_npcButtonPrefab, _npcContainer);
            var btn = obj.GetComponent<NPCSelectionButton>();

            btn.Setup(npc, this);
            
            var button = obj.GetComponent<Button>();
            button.onClick.AddListener(() => btn.OnClick(npc));
        }
    }
    private void SetupNightPanel()
    {
        var location = HordeManager.Instance.CurrentNightLocation;

        if (location == null)
            return;

        _nightTitle.text = $"NIGHT {HordeManager.Instance.currentHorde}";
        _nightPreviewImage.sprite = location.PreviewImage;
        _nightObjective.text = HordeManager.Instance.CurrentMoon.ObjectiveTextLong;
        _nightObjectiveImage.sprite = HordeManager.Instance.CurrentMoon.Image;
        _mutation.text = HordeManager.Instance.PreparedMutation.Description;
        _mutationImage.sprite = HordeManager.Instance.PreparedMutation.Icon;
        _locationName.text = location.SceneName;
        SetupNightRewards();
        
        //TODO
        // CreateNightRewards(location.Rewards);

        _startNightButton.onClick.RemoveAllListeners();

        _startNightButton.onClick.AddListener(() =>
        {
            StartSelectedNight(
                HordeManager.Instance.IsHeroNight
                    ? HordeManager.Instance.CurrentHeroNpc
                    : null
            );
        });
    }
    
    private void SetupBossNightPanel(MoonData bossMoon)
    {
        var location = HordeManager.Instance.CurrentNightLocation;
        
        if (location == null)
            return;

        _bossNameTMP.text = bossMoon.DisplayName;
        _bossImage.sprite = bossMoon.Image;
        _bossDescriptionTMP.text = bossMoon.Description;
        
        _bossNightButton.onClick.RemoveAllListeners();

        _bossNightButton.onClick.AddListener(() =>
        {
            StartSelectedNight(
                HordeManager.Instance.IsHeroNight
                    ? HordeManager.Instance.CurrentHeroNpc
                    : null
            );
        });
    }
    
    private void SetupNightRewards()
    {
        ClearNightRewards();

        var rewards = HordeManager.Instance.PreparedRewards;

        foreach (var reward in rewards)
        {
            var obj = Instantiate(
                _nightRewardPrefab,
                _nightRewardContainer.transform
            );

            obj.Setup(
                reward.Item.Image,
                reward.Amount
            );
        }
    }
    
    private void CreateNightRewards(List<NightReward> rewards)
    {
        ClearNightRewards();

        foreach (var rewardData in rewards)
        {
            var reward = Instantiate(
                _nightRewardPrefab,
                _nightRewardContainer.transform
            );

            reward.Setup(
                rewardData.Item.Image,
                rewardData.Amount
            );
        }
    }
    
    private void ClearNightRewards()
    {
        foreach (Transform child in _nightRewardContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }
    
    private void CreateNightReward(Sprite sprite, int amount)
    {
        var reward = Instantiate(
            _nightRewardPrefab,
            _nightRewardContainer.transform
        );

        reward.Setup(sprite, amount);
    }
    
    private void SelectNPC(VillageNpcRuntime npc)
    {
        _selectedNPC = npc;
        HordeManager.Instance.SelectedNpc = npc;
    }
    
    private void UpdateStatsPanel()
    {
        if (_playerStatsSo == null) return;

        var player = Player.Instance;
        var movement = player.GetComponent<PlayerMovement>();

        PlayerStatisticsManager.Instance.SetDamage(_playerStatsSo.TotalDamage);

        var finalSpeed = movement.GetFinalSpeed();
        PlayerStatisticsManager.Instance.SetMoveSpeed(finalSpeed);

        var attackCooldownReduction =
            _playerStatsSo.GetBonusValue(
                BonusType.AttackCooldownReduction);

        PlayerStatisticsManager.Instance.SetAttackCooldownPercent(
            attackCooldownReduction * 100f);
        
        SetCriticalStatistics();
        
        Player.Instance.PlayerAttack.RecalculateDamage();
    }

    private void SetCriticalStatistics()
    {
        var critRaw = _playerStatsSo.GetBonusValue(BonusType.CritChance);
        var crit = Mathf.Max(0f, critRaw) * 100f;
        PlayerStatisticsManager.Instance.SetCritChance(Mathf.Min(crit, 100f));

        var critMultiplier = _playerStatsSo.GetCritMultiplier();
        PlayerStatisticsManager.Instance.SetCritMultiplier(critMultiplier);
    }
    
    private string FormatBonus(StatBonus bonus)
    {
        return bonus.Type switch
        {
            BonusType.Damage => $"+{bonus.Value:0}% Damage",
            BonusType.MoveSpeed => $"+{bonus.Value:0}% Move Speed",
            BonusType.CritChance => $"+{bonus.Value:0}% Crit Chance",
            BonusType.CritMultiplier => $"+{bonus.Value:0} Crit Multiplier",

            BonusType.MaxHp => $"+{bonus.Value:0} Max HP",
            BonusType.MaxMp => $"+{bonus.Value:0} Max Mana",
            BonusType.AttackCooldownReduction => $"+{bonus.Value:0}% Attack Speed",
            _ => bonus.Type.ToString()
        };
    }
    
    private void CreateSummaryText(string text, Transform parent, BonusType? bonusType = null)
    {
        var obj = Instantiate(_summaryInfoObject, parent);
        var tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.color = bonusType switch
        {
            BonusType.MaxHp => Color.green,
            BonusType.Damage => Color.red,
            BonusType.MoveSpeed => Color.cyan,
            _ => Color.white
        };
    }

    #region Moon objective UI

    private void RefreshMoonObjective()
    {
        var moon = MoonManager.Instance.CurrentMoon;

        if (moon == null)
            return;

        UpdateMoonObjectiveUI(
            HordeManager.Instance.CurrentObjectiveProgress,
            HordeManager.Instance.CurrentObjectiveTarget
        );
    }
    
    private void UpdateMoonObjectiveUI(int current, int target)
    {
        string newText;

        if (HordeManager.Instance.CurrentObjective ==
            HordeObjective.BossArena)
        {
            newText = HordeManager.Instance.IsBossAlive()
                ? "Kill the boss"
                : "Find the portal";
        }
        else if (_portalSpawned)
        {
            newText = "Find the portal";
            _moonObjectiveImageFinished.gameObject.SetActive(true);
        }
        else if (current >= target)
        {
            newText = "Find the portal";
            _moonObjectiveImageFinished.gameObject.SetActive(true);
        }
        else
        {
            var moon = MoonManager.Instance.CurrentMoon;

            if (moon == null)
                return;

            newText =
                $"{moon.ObjectiveText}: {current}/{target}";
        }

        if (_moonObjectiveText.text != newText)
        {
            _moonObjectiveText.text = newText;
            NotifyObjectiveChanged();
        }
    }
    
    private void NotifyObjectiveChanged()
    {
        if (_objectiveFlashRoutine != null)
        {
            StopCoroutine(_objectiveFlashRoutine);
        }

        _objectiveFlashRoutine = StartCoroutine(ObjectiveFlashRoutine());
    }

    private IEnumerator ObjectiveFlashRoutine()
    {
        var originalColor = _moonObjectiveText.color;

        var flashColor = Color.yellow;

        var duration = 0.15f;
        var timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            var t = timer / duration;

            _moonObjectiveText.color = Color.Lerp(
                originalColor,
                flashColor,
                t
            );

            yield return null;
        }

        timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            var t = timer / duration;

            _moonObjectiveText.color = Color.Lerp(
                flashColor,
                originalColor,
                t
            );

            yield return null;
        }

        _moonObjectiveText.color = originalColor;
    }
    
    #endregion

    private void RefreshHordeInfoVisibility()
    {
        if (_hordeInfoContainer == null)
            return;

        var isLocation = !LoadingSceneManager.Instance.IsSceneBase() && !LoadingSceneManager.Instance.IsInMainMenu();
        var isBossFight = HordeManager.Instance.NightStartType == NightStartType.Boss;

        var shouldShow = isLocation && !isBossFight;

        _hordeInfoContainer.SetActive(shouldShow);
    }
    
    #region Minimap

    private void RefreshMinimapVisibility()
    {
        if (_minimapContainer == null)
            return;

        var shouldShow =
            !LoadingSceneManager.Instance.IsSceneBase()
            && !LoadingSceneManager.Instance.IsInMainMenu();

        _minimapContainer.SetActive(shouldShow);
    }

    #endregion
}
