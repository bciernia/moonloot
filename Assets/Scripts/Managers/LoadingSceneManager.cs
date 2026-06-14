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

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public async void StartNewGame(string sceneName)
    {
        await LoadScene(sceneName, true);

        var cycle = FindObjectOfType<DayNightCycle>();
        if (cycle != null)
            cycle.ResetCycle();
    }

    public async Task LoadScene(string sceneName, bool setPlayerInSpawnPoint = false)
    {
        if (PauseManager.Instance)
        {
            PauseManager.Instance.ClearAllPauses();
        }
        
        PersistentMenuManager.Instance.HideAllPersistentPanels();
        
        loadedValue = 0f;
        progressBar.fillAmount = 0f;
        loadingScreen.SetActive(true);

        // SaveGame();

        var timer = 0f;

        var scene = SceneManager.LoadSceneAsync(sceneName);

        scene.allowSceneActivation = false;

        while (!scene.isDone)
        {
            timer += Time.deltaTime;

            loadedValue = Mathf.Clamp01(scene.progress / 0.9f);

            if (scene.progress >= 0.9f && timer >= minDisplayTime)
            {
                loadedValue = 1f;
                scene.allowSceneActivation = true;
            }

            await Task.Yield();
        }

        if (setPlayerInSpawnPoint)
        {
            SetPlayerToSpawnPoint();
        }

        TryFindDayNightCycle();
        CombatManager.Instance.ClearCombat();
        SoundManager.Instance.FindMapForSoundManager();
        SoundManager.Instance.PlayMusic(sceneName);

        if (sceneName == "MainMenu")
        {
            var cycle = FindObjectOfType<DayNightCycle>();
            SoundManager.Instance.StopCombatMusic();
            if (cycle != null)
                cycle.ResetCycle();
        }

        await Task.Delay(500);       
        
        loadingScreen.SetActive(false);
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
        if (IsInMainMenu())
            return;
        
        TryFindDayNightCycle();

        WorldManager.Instance.ResetSpawnPoints();
        WorldManager.Instance.AssignPlacesIfNeeded();
        WorldManager.Instance.SpawnNPCs();

        if (IsSceneBase())
        {
            SoundManager.Instance.StopCombatMusic();
            ExecuteFunctionsForMainTownScene();
        }
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

        Debug.Log("Subscribed to DayNightCycle");
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


    public bool IsSceneBase() => SceneManager.GetActiveScene().name == "Base";
    public bool IsInMainMenu() => SceneManager.GetActiveScene().name == "MainMenu";

    public async void LoadMainMenu()
    {
        await LoadScene("MainMenu");
    }
}