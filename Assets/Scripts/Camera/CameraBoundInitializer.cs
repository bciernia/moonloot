using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraBoundInitializer : MonoBehaviour
{
    private CinemachineConfiner2D _cinemachineConfiner2D;

    private void Awake()
    {
        _cinemachineConfiner2D = GetComponent<CinemachineConfiner2D>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject cameraBound = GameObject.FindGameObjectWithTag("CameraBound");

        if (cameraBound != null)
        {
            _cinemachineConfiner2D.BoundingShape2D = cameraBound.GetComponent<Collider2D>();

            _cinemachineConfiner2D.InvalidateCache();
        }
        else
        {
            Debug.LogWarning("CameraBound not found in scene: " + scene.name);
        }
    }
}
