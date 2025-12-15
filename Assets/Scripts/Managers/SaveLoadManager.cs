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


    private void Save()
    {
        var saveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>();

        foreach (var s in saveables)
        {
            s.Save();
        }
    }

    public void Load()
    {
        var saveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>();
        
        foreach (var s in saveables)
        {
            s.Load();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Load();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
