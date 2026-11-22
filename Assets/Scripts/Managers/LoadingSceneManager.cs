using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneManager : Singleton<LoadingSceneManager>
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image progressBar;
    [SerializeField] private float lerpSpeed = 3f;
    [SerializeField] private float minDisplayTime = 0.5f;
    [SerializeField] private RectTransform[] _moons;

    [Header("Frame rate limits")]
    [SerializeField, Min(1)] private int mainMenuMaxFrameRate = 60;
    [SerializeField, Min(1)] private int loadingScreenMaxFrameRate = 60;
    [SerializeField, Min(1)] private int gameplayMaxFrameRate = 150;

    private readonly float[] moonSpeeds =
    {
        10f,
        -15f,
        20f,
        -25f,
        30f
    };
    
    private float loadedValue;
    private DayNightCycle _currentCycle;
    private bool _skipWorkerProcessingOnNextTownArrival;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
            return;

        ApplyFrameRateForScene(SceneManager.GetActiveScene().name);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public async void StartNewGame(string sceneName)
    {
        await LoadScene(sceneName, true);

        // var cycle = FindObjectOfType<DayNightCycle>();
        // if (cycle != null)
        //     cycle.ResetCycle();
    }

    public async Task LoadScene(string sceneName, bool setPlayerInSpawnPoint = false)
    {
        if (PauseManager.Instance)
        {
            PauseManager.Instance.ClearAllPauses();
        }
        
        PersistentMenuManager.Instance.HideAllPersistentPanels();

        SetFrameRateLimit(loadingScreenMaxFrameRate);
        
        loadedValue = 0f;
        progressBar.fillAmount = 0f;
        loadingScreen.SetActive(true);
        try
        {
            // SaveGame();

            var timer = 0f;
            var scene = SceneManager.LoadSceneAsync(sceneName);
            if (scene == null)
                throw new InvalidOperationException($"Could not start loading scene '{sceneName}'.");

            scene.allowSceneActivation = false;

            while (!scene.isDone)
            {
                timer += Time.unscaledDeltaTime;

                loadedValue = Mathf.Clamp01(scene.progress / 0.9f);

                if (scene.progress >= 0.9f && timer >= minDisplayTime)
                {
                    loadedValue = 1f;
                    scene.allowSceneActivation = true;
                }

                await Task.Yield();
            }

            if (setPlayerInSpawnPoint)
                SetPlayerToSpawnPoint();

            TryFindDayNightCycle();
            CombatManager.Instance.ClearCombat();
            SoundManager.Instance.FindMapForSoundManager();
            SoundManager.Instance.PlayMusic(sceneName);

            if (sceneName == "MainMenu")
            {
                var cycle = FindObjectOfType<DayNightCycle>();
                SoundManager.Instance.StopCombatMusic();
                // if (cycle != null)
                //     cycle.ResetCycle();
            }

            await Task.Delay(500);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            throw;
        }
        finally
        {
            loadedValue = 1f;
            if (loadingScreen != null)
                loadingScreen.SetActive(false);
            ApplyFrameRateForScene(SceneManager.GetActiveScene().name);
        }
    }

    private void SetPlayerToSpawnPoint()
    {
        var spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
        var player = GameObject.FindGameObjectWithTag("Player");

        if (spawnPoint != null && player != null)
        {
            player.transform.position = spawnPoint.transform.position;
        }
        else
        {
            Debug.LogWarning("SpawnPoint or Player not found!");
        }
    }

    private void Update()
    {
        progressBar.fillAmount =
            Mathf.Lerp(
                progressBar.fillAmount,
                loadedValue,
                lerpSpeed * Time.deltaTime
            );

        for (var i = 0; i < _moons.Length; i++)
        {
            _moons[i].Rotate(
                0,
                0,
                moonSpeeds[i] * Time.deltaTime
            );
        }
    }

    private void OnEnable()
    {
        TryFindDayNightCycle();
    }

    /*
    private void OnDisable()
    {
        var cycle = FindObjectOfType<DayNightCycle>();
        if (cycle != null)
            cycle.HordeAttack -= HandleNightStarted;
    }
*/
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (loadingScreen != null && loadingScreen.activeSelf)
            SetFrameRateLimit(loadingScreenMaxFrameRate);
        else
            ApplyFrameRateForScene(scene.name);

        if (IsInMainMenu())
            return;
        
        TryFindDayNightCycle();

        WorldManager.Instance.ResetSpawnPoints();
        WorldManager.Instance.AssignPlacesIfNeeded();
        WorldManager.Instance.SpawnNPCs();

        Player.Instance?
            .GetComponent<PlayerMovement>()
            ?.ResetTemporaryMovementModifiers();

        if ((scene.name == "Base_Demo" || scene.name == "START_MAP") &&
            MainMenuManager.SelectedHeroForNewGame != null)
        {
            // Reapply the fresh-game reset after the destination scene has
            // created its managers. The InventorySO asset is shared at runtime
            // and can otherwise retain values from a previous play session.
            InventoryController.ResetCrystalBalancesForNewGame();

            var selectedHero = MainMenuManager.SelectedHeroForNewGame;
            if (Player.Instance != null)
            {
                Player.Instance.ResetForNewGame();
                Player.Instance.ApplyStartingHeroStats(selectedHero);
                OutfitManager.ApplyHeroOutfit(selectedHero.Outfit);
            }
            else
                Debug.LogWarning("Selected hero setup: Player is missing while loading the new game scene.");

            var playerSkillManager = PlayerSkillManager.Instance;
            var skillsManager = SkillsManager.Instance;
            var baseSkill = playerSkillManager != null
                ? playerSkillManager.DefaultStartingSkill
                : null;

            playerSkillManager?.ConfigureForNewHero(selectedHero.StartingProfessionSkill);
            if (skillsManager != null)
                skillsManager.ConfigureStartingSkills(baseSkill, selectedHero.StartingProfessionSkill);

            // Persist the new hero's initial unlocks and Q/E setup immediately.
            playerSkillManager?.Save();
            skillsManager?.Save();

            MainMenuManager.ClearSelectedHeroForNewGame();
        }

        if (IsSceneBase())
        {
            SoundManager.Instance.StopCombatMusic();
            if (_skipWorkerProcessingOnNextTownArrival)
                _skipWorkerProcessingOnNextTownArrival = false;
            else
                ExecuteFunctionsForMainTownScene();
        }
    }

    public async void ReturnFromTutorialToVillage()
    {
        _skipWorkerProcessingOnNextTownArrival = true;
        await LoadScene("Base_Demo", true);
        SaveLoadManager.Instance?.Save();
    }

    private void ExecuteFunctionsForMainTownScene()
    {
        WorkManager.Instance.ProcessWorkersAfterNight();
    }

    private void TryFindDayNightCycle()
    {
        var cycle = FindObjectOfType<DayNightCycle>();

        if (cycle == null)
            return;

        _currentCycle = cycle;
    }

    private void ApplyFrameRateForScene(string sceneName)
    {
        SetFrameRateLimit(
            sceneName == "MainMenu"
                ? mainMenuMaxFrameRate
                : gameplayMaxFrameRate);
    }

    private static void SetFrameRateLimit(int frameRate)
    {
        // Disable VSync so targetFrameRate controls the actual frame cap.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Mathf.Max(1, frameRate);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /*
    private void HandleNightStarted()
    {
        if (DialogueManager.DialogueInProgress)
        {
            DialogueManager.Instance.EndDialogue();
        }

        HordeManager.Instance.StartHorde();
    }
    */

    private void SaveGame()
    {
        var saveables = FindObjectsOfType<MonoBehaviour>(true);

        foreach (var mono in saveables)
        {
            if (mono is ISaveable saveable)
            {
                saveable.Save();
            }
        }
    }


    public bool IsSceneBase() => SceneManager.GetActiveScene().name == "Base_Demo";
    public bool IsInMainMenu() => SceneManager.GetActiveScene().name == "MainMenu";

    public async void LoadMainMenu()
    {
        await LoadScene("MainMenu");
    }
}
