using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuManager : MonoBehaviour
{
    private void OnEnable()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu") return;
        PauseManager.Instance.RequestPause();
    }

    private void OnDisable()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu") return;
        PauseManager.Instance.ReleasePause();
    }
}