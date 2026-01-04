using UnityEngine;

public class StartGamePanel : MonoBehaviour
{
    void Start()
    {
        PauseManager.Instance.RequestPause();
    }
}
