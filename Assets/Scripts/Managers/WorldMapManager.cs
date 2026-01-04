using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldMapManager : Singleton<WorldMapManager>
{
    private Scene Scene { get; set; }

    private const string WORLD_MAP_NAME = "WorldMap";
    
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
        GameManager.Instance.SetMode(scene.name == WORLD_MAP_NAME ? GameMode.WorldMap : GameMode.Location);
    }
}
