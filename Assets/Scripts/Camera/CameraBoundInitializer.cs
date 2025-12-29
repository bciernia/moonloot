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

#pragma warning disable CS0618 // Type or member is obsolete
        SceneManager.sceneLoaded += OnSceneLoaded;
#pragma warning restore CS0618 // Type or member is obsolete
    }

    private void OnDestroy()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        SceneManager.sceneLoaded -= OnSceneLoaded;
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Obsolete("Invalidate cache obsolete")]
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var cameraBound = GameObject.FindGameObjectWithTag("CameraBound");

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
