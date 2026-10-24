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

    public void CreateNewSaveForNewGame(OutfitItemSO startingHeroOutfit = null)
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
            "Base_Demo", CurrentSettings);
        
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

        ES3.Save(
            OutfitManager.HeroOutfitSaveKey,
            OutfitManager.GetOutfitSaveId(startingHeroOutfit),
            CurrentSettings);
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(
                ES3SlotManager.selectedSlotPath))
            return;

        var settings = CurrentSettings;
        if (settings == null)
            return;

        ES3.Save("SceneName", SceneManager.GetActiveScene().name, settings);

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

        var settings = CurrentSettings;
        if (settings == null || !ES3.KeyExists("SceneName", settings))
        {
            Debug.LogWarning("The selected save has no saved scene name.", this);
            return;
        }

        var sceneName = ES3.Load<string>("SceneName", settings);

        await LoadingSceneManager.Instance.LoadScene(
            sceneName,
            true);

        var saveables =
            FindObjectsByType<MonoBehaviour>(
                    FindObjectsSortMode.None)
                .OfType<ISaveable>();

        foreach (var saveable in saveables)
        {
            saveable.Load();
        }

        MigrateLegacyEquippedOutfitIfNeeded();
    }

    private void MigrateLegacyEquippedOutfitIfNeeded()
    {
        var settings = CurrentSettings;
        if (settings == null || ES3.KeyExists(OutfitManager.HeroOutfitSaveKey, settings))
            return;

        var equippedItemsManager = EquippedItemsManager.Instance;
        var equippedItems = equippedItemsManager != null ? equippedItemsManager.EquippedItems : null;
        if (equippedItems == null || equippedItems.Count <= 2 ||
            equippedItems[2].item is not OutfitItemSO legacyOutfit)
            return;

        OutfitManager.ApplyHeroOutfit(legacyOutfit);
        equippedItems[2] = InventoryItem.GetEmptyItem();
        equippedItemsManager.InitializeEquippedSlots();

        ES3.Save(OutfitManager.HeroOutfitSaveKey, OutfitManager.GetOutfitSaveId(legacyOutfit), settings);
        ES3.Save("playerInventory_equippedItems", equippedItems, settings);
    }

}
