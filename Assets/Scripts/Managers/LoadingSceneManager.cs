using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneManager : MonoBehaviour
{
#pragma warning disable UDR0001
    public static LoadingSceneManager Instance;
#pragma warning restore UDR0001

    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image progressBar;
    [SerializeField] private float lerpSpeed = 3f;        
    [SerializeField] private float minDisplayTime = 0.5f; 

    private float loadedValue;       

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async void LoadScene(string sceneName, bool setPlayerInSpawnPoint = false)
    {
        loadedValue = 0f;
        progressBar.fillAmount = 0f;
        loadingScreen.SetActive(true);

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
        
        SoundManager.Instance.FindMapForSoundManager();
        SoundManager.Instance.PlayMusic(sceneName);
        CombatManager.Instance.ClearCombat();

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
        progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, loadedValue, lerpSpeed * Time.deltaTime);
    }

    private void OnEnable()
    {
        var cycle = FindObjectOfType<DayNightCycle>();
        if (cycle != null)
            cycle.HordeAttack += HandleNightStarted;
    }

    private void OnDisable()
    {
        var cycle = FindObjectOfType<DayNightCycle>();
        if (cycle != null)
            cycle.HordeAttack -= HandleNightStarted;
    }
    
    private void HandleNightStarted()
    {
        LoadScene("Forest", true);        
    }
}