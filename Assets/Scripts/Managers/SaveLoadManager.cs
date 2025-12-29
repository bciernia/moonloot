using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            Save();
            Debug.Log("Game saved");
        }
    }
    
    private ES3Settings CurrentSettings
    {
        get
        {
            if (string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
            {
                Debug.LogWarning("No save slot selected!");
                return null;
            }

            return new ES3Settings(ES3SlotManager.selectedSlotPath)
            {
                format = ES3.Format.JSON,
            };
        }
    }

    public void Save()
    {
        if(string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath)) return;

        ES3.Save("SceneName", SceneManager.GetActiveScene().name);
        
        var saveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>();

        Debug.Log($"Save to {ES3SlotManager.selectedSlotPath}");
        
        foreach (var s in saveables)
        {
            s.Save();
        }

        ES3SlotManager.selectedSlotPath = null;
    }

    public void Load()
    {
        Debug.Log($"Loaded: {ES3SlotManager.selectedSlotPath}"); 

        if (string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath)) return;

        var sceneName = ES3.Load<string>("SceneName", CurrentSettings);

        StartCoroutine(LoadSceneAndApplySave(sceneName));
    }
    
    private IEnumerator LoadSceneAndApplySave(string sceneName)
    {
        var asyncOp = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncOp.isDone)
        {
            yield return null;
        }

        var saveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>();
        foreach (var s in saveables)
        {
            s.Load();
        }

        Debug.Log($"Loaded scene {sceneName} with save {ES3SlotManager.selectedSlotPath}");

        ES3SlotManager.selectedSlotPath = null;
    }

    // protected override void Awake()
    // {
    //     base.Awake();
    //     SceneManager.sceneLoaded += OnSceneLoaded;
    // }
    //
    // private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     Load();
    // }
    //
    // private void OnDestroy()
    // {
    //     SceneManager.sceneLoaded -= OnSceneLoaded;
    // }
}
