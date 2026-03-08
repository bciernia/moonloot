using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private const string MAIN_MENU = "MainMenu";
    
    public void StartGame()
    {
        SceneManager.LoadScene("Town", LoadSceneMode.Single);
    }

    public void StartArena()
    {
        SceneManager.LoadScene("SceneArena", LoadSceneMode.Single);
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
        SceneManager.LoadScene("MainMenu");
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
            MainMenuSoundManager.Instance.PlayMainMenuMusic();
        }
    }
}
