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

    public void CreateNewSaveForNewGame()
    {
        var slotName =
            System.DateTime.Now.ToString(
                "yyyyMMdd_HHmmss");

        var slot =
            slotManager.CreateNewSlot(slotName);

        ES3SlotManager.selectedSlotPath =
            slot.GetSlotPath();

        ES3.DeleteFile(
            ES3SlotManager.selectedSlotPath);

        ES3.Save(
            "SceneName",
            "Base",
            CurrentSettings);

        ES3.Save(
            "CreatedAt",
            System.DateTime.Now.ToBinary(),
            CurrentSettings);

        ES3.Save(
            "CurrentNight",
            1,
            CurrentSettings);

        ES3.Save(
            "LastSavedAt",
            System.DateTime.Now.ToBinary(),
            CurrentSettings);
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(
                ES3SlotManager.selectedSlotPath))
            return;

        var saveables =
            FindObjectsByType<MonoBehaviour>(
                    FindObjectsSortMode.None)
                .OfType<ISaveable>();

        foreach (var saveable in saveables)
        {
            saveable.Save();
        }

        ES3.Save(
            "CurrentNight",
            HordeManager.Instance.currentHorde,
            CurrentSettings);

        ES3.Save(
            "LastSavedAt",
            System.DateTime.Now.ToBinary(),
            CurrentSettings);
    }

    public void Load()
    {
        if (string.IsNullOrEmpty(
                ES3SlotManager.selectedSlotPath))
            return;

        var sceneName =
            ES3.Load<string>(
                "SceneName",
                CurrentSettings);

        StartCoroutine(
            LoadSceneAndApplySave(sceneName));
    }

    private IEnumerator LoadSceneAndApplySave(
        string sceneName)
    {
        var asyncOp =
            SceneManager.LoadSceneAsync(sceneName);

        while (!asyncOp.isDone)
        {
            yield return null;
        }

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