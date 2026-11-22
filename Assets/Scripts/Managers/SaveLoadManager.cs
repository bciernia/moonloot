using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    public const int MaxSaveSlots = 15;
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

    public bool CreateNewSaveForNewGame(
        OutfitItemSO startingHeroOutfit = null,
        string startingSceneName = "START_MAP")
    {
        if (slotManager == null)
        {
            Debug.LogError("Cannot create a save: the slot manager is not assigned.", this);
            return false;
        }

        if (ES3.DirectoryExists(slotManager.slotDirectory) &&
            ES3.GetFiles(slotManager.slotDirectory).Count(file =>
                file.EndsWith(slotManager.slotExtension, StringComparison.OrdinalIgnoreCase)) >= MaxSaveSlots)
            return false;

        var slotName = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var baseName = slotName;
        var suffix = 1;
        while (ES3.FileExists(slotManager.GetSlotPath(slotName)))
            slotName = $"{baseName}_{suffix++}";

        var slot =
            slotManager.CreateNewSlot(slotName);

        ES3SlotManager.selectedSlotPath =
            slot.GetSlotPath();

        // InventorySO is a shared runtime asset and may still hold values from
        // the previously loaded save. A genuinely new game starts with no crystals.
        InventoryController.ResetCrystalBalancesForNewGame();

        ES3.Save("playerInventory_aullaCrystals", 0, CurrentSettings);
        ES3.Save("playerInventory_udrosCrystals", 0, CurrentSettings);

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.InitializeForNewSave();

        // ES3.DeleteFile(
            // ES3SlotManager.selectedSlotPath);

        ES3.Save(
            "SceneName",
            startingSceneName, CurrentSettings);
        
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
        return true;
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

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.BeginLoadingSeenTutorials();

        await LoadingSceneManager.Instance.LoadScene(
            sceneName,
            true);

        if (HordeManager.Instance != null && ES3.KeyExists("CurrentNight", settings))
            HordeManager.Instance.currentHorde = ES3.Load<int>("CurrentNight", settings);

        var saveables = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<ISaveable>()
            .OrderBy(GetLoadPriority)
            .ToArray();

        foreach (var saveable in saveables)
        {
            saveable.Load();
        }

        MigrateLegacyEquippedOutfitIfNeeded();

        if (Player.Instance != null && Player.Instance.PlayerAttack != null)
            Player.Instance.PlayerAttack.RecalculateDamage();
    }

    private static int GetLoadPriority(ISaveable saveable)
    {
        if (saveable is Player) return 0;
        if (saveable is InventoryController) return 1;
        if (saveable is OutfitManager) return 2;
        if (saveable is PlayerSkillManager) return 3;
        if (saveable is SkillsManager) return 4;
        if (saveable is NPCManager) return 5;
        if (saveable is TavernManager) return 6;
        if (saveable is WorldManager) return 7;
        return 8;
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
