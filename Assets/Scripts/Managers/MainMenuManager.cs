using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("SceneForest", LoadSceneMode.Single);
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
    
}
