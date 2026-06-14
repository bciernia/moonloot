using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private const string DEMO_START_SCENE = "Base";
    private const string MAIN_MENU = "MainMenu";
    [SerializeField] private GameObject[] _buttonsMenu;
    
    private GameObject _loadPanel;

    public void StartGame()
    {
        var gameRoot = GameObject.FindWithTag("GameRoot");
        var player = GameObject.FindWithTag("Player");

        if (gameRoot != null) Destroy(gameRoot);
        if (player != null) Destroy(player);
        
        // ES3.DeleteFile();
        
        SaveLoadManager.Instance.CreateNewSaveForNewGame();
        LoadingSceneManager.Instance.StartNewGame(DEMO_START_SCENE);
        ChangeButtonsVisibility(false);
    }

    public void ExitGame()
    {
        Application.Quit();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void GoToMainMenu()
    {
        LoadingSceneManager.Instance.LoadScene("MainMenu");
        
        Destroy(GameObject.FindWithTag("GameRoot"));
        Destroy(GameObject.FindWithTag("Player"));
    }
    
    private void OnEnable()
    {
#pragma warning disable UDR0005
        SceneManager.sceneLoaded += OnSceneLoaded;
#pragma warning restore UDR0005
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMode(scene.name == MAIN_MENU ? GameMode.MainMenu : GameMode.Location);
        }
        else
        {
            ChangeButtonsVisibility(true);
            // MainMenuSoundManager.Instance.PlayMainMenuMusic();
        }
    }

    private void ChangeButtonsVisibility(bool shouldBeVisible)
    {
        foreach (var btn in _buttonsMenu)
        {
            btn.gameObject.SetActive(shouldBeVisible);
        }
    }
    
    public void SetActiveLoadPanel(bool isActive)
    {
        if (_loadPanel == null)
        {
            FindLoadPanel();
        }

        if (_loadPanel != null)
        {
            _loadPanel.SetActive(isActive);
        }
    }
    
    private void FindLoadPanel()
    {
        var transforms =
            FindObjectsByType<Transform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (var t in transforms)
        {
            if (!t.CompareTag("LoadSlots"))
                continue;

            _loadPanel = t.gameObject;
            return;
        }

        Debug.LogError("LoadPanel not found!");
    }
}
