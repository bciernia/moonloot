using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetMainCameraToUI : MonoBehaviour
{
    private Canvas _canvas;
    
    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
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
        AssignMainCamera();
    }

    private void Start()
    {
        AssignMainCamera();
    }

    private void AssignMainCamera()
    {
        Camera mainCam = Camera.main;
        if (_canvas != null && mainCam != null)
        {
            _canvas.worldCamera = mainCam;
        }
    }
}
