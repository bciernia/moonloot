using System;
using UnityEngine;

public class GameMenuManager : MonoBehaviour
{
    private void OnEnable()
    {
        PauseManager.Instance.RequestPause();
    }

    private void OnDisable()
    {
        PauseManager.Instance.ReleasePause();
    }
}
