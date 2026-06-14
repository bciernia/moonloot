using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    [SerializeField] private ES3SlotManager slotManager;

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
                format = ES3.Format.JSON
            };
        }
    }

    public ES3Settings GetSettings() =>  CurrentSettings;

    public void CreateNewSaveForNewGame()
    {
        var slotName = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        var slot =
            slotManager.CreateNewSlot(slotName);

        ES3SlotManager.selectedSlotPath =
            slot.GetSlotPath();

        // ES3.DeleteFile(
            // ES3SlotManager.selectedSlotPath);

        ES3.Save(
            "SceneName",
            "Base", CurrentSettings);
        
        Debug.Log(
            "Slot path: " +
            ES3SlotManager.selectedSlotPath
        );

        Debug.Log(
            "File exists: " +
            ES3.FileExists(
                ES3SlotManager.selectedSlotPath
            )
        );

        ES3.Save(
            "CurrentNight",
            1, CurrentSettings);
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(
                ES3SlotManager.selectedSlotPath))
            return;

        var saveables = FindObjectsOfType<MonoBehaviour>(true);

        foreach (var mono in saveables)
        {
            if (mono is ISaveable saveable)
            {
                saveable.Save();
            }
        }

        ES3.Save(
            "CurrentNight",
            HordeManager.Instance.currentHorde, CurrentSettings);

        ES3.Save(
            "LastSavedAt",
            System.DateTime.Now.ToBinary(), CurrentSettings);
    }

    public async void Load()
    {
        if (string.IsNullOrEmpty(
                ES3SlotManager.selectedSlotPath))
            return;

        var sceneName =
            ES3.Load<string>(
                "SceneName");

        await LoadingSceneManager.Instance.LoadScene(
            sceneName);

        var saveables =
            FindObjectsByType<MonoBehaviour>(
                    FindObjectsSortMode.None)
                .OfType<ISaveable>();

        foreach (var saveable in saveables)
        {
            saveable.Load();
        }
    }
}