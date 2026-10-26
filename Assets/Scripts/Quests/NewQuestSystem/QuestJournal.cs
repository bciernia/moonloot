using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class QuestJournal : MonoBehaviour, ISaveable
{
    private const string TaskBoardSourceID = "task_board";

    private List<QuestStatus> _questStatuses = new List<QuestStatus>();

    [SerializeField] private AudioClip _questNotificationSound;

    public event Action onUpdate;
    public event Action<QuestStatus> OnQuestReadyToTurnIn;
    public event Action<QuestStatus> OnQuestTurnedIn;

    private void OnEnable()
    {
        EnemyEvents.EnemyKilled += HandleEnemyKilled;
        QuestObjectiveEvents.ItemCollected += HandleItemCollected;
        QuestObjectiveEvents.MapCompleted += HandleMapCompleted;
    }

    private void OnDisable()
    {
        EnemyEvents.EnemyKilled -= HandleEnemyKilled;
        QuestObjectiveEvents.ItemCollected -= HandleItemCollected;
        QuestObjectiveEvents.MapCompleted -= HandleMapCompleted;
    }

    public bool AddQuest(
        QuestSO quest,
        string entryKey,
        string giverID = "",
        string giverName = "",
        Sprite giverPortrait = null,
        string giverProfession = "")
    {
        if (quest == null || IsQuestInQuestList(quest)) return false;

        var newQuestStatus = new QuestStatus(quest, giverID, giverName, giverPortrait, giverProfession);
        var initEntry = quest.GetEntry(entryKey);
        newQuestStatus.AddQuestEntry(initEntry);
        _questStatuses.Add(newQuestStatus);
        onUpdate?.Invoke();
        return true;
    }
    
    private bool IsQuestInQuestList(QuestSO quest) => GetQuestStatus(quest.Id) != null;

    public IEnumerable<QuestStatus> GetQuestStatuses() => _questStatuses;

    public QuestStatus GetQuestStatus(QuestSO quest) => quest == null ? null : GetQuestStatus(quest.Id);

    public void AddEntryToJournal(QuestSO quest, string entryKey)
    {
        var questStatus = GetQuestStatus(quest);
        var entry = quest.GetEntry(entryKey);

        if (questStatus != null)
        {
            questStatus.AddQuestEntry(entry);
        }
        
        onUpdate?.Invoke();
    }
    
    public void AddEntryToJournalAndFinishQuest(QuestSO quest, string journalEntry, bool shouldGiveReward)
    {
        var questStatus = GetQuestStatus(quest);

        if (questStatus != null && !questStatus.IsQuestCompleted)
        {
            var questEntry = quest.GetEntry(journalEntry);
            questStatus.AddQuestEntry(questEntry);
            questStatus.FinishQuest();

            if (shouldGiveReward)
            {
                GiveReward(quest);
            }
            else
            {
                RecordCompletedQuestForNpcDialogue(quest, questStatus);
            }

            OnQuestTurnedIn?.Invoke(questStatus);
            ShowQuestNotification($"Quest completed: {quest.GetQuestTitle()}");
        }
        
        onUpdate?.Invoke();
    }
    
    private void GiveReward(QuestSO quest)
    {
        RecordCompletedQuestForNpcDialogue(quest, GetQuestStatus(quest));

        if (InventoryController.Instance == null)
        {
            Debug.LogWarning($"Cannot give rewards for quest {quest.GetQuestTitle()}: InventoryController was not found.", this);
            return;
        }

        foreach (var reward in quest.GetRewards())
        {
            if (reward == null || reward.item.IsEmpty || reward.item.item == null)
                continue;

            var quantity = reward.item.quantity > 0 ? reward.item.quantity : Mathf.Max(1, reward.number);
            var remaining = InventoryController.Instance.AddItem(
                new InventoryItem { item = reward.item.item, quantity = quantity, itemState = reward.item.itemState });

            if (remaining > 0 && FloatingTextManager.Instance != null)
                FloatingTextManager.Instance.ShowWarningText("Not enough inventory space for all quest rewards.", transform);
        }
    }

    private static void RecordCompletedQuestForNpcDialogue(QuestSO quest, QuestStatus status)
    {
        if (quest == null || HordeManager.Instance == null)
            return;

        HordeManager.Instance.RecordQuestCompleted(
            quest.GetQuestTitle(),
            status != null ? status.GiverName : string.Empty);
    }

    public bool CanTurnInQuest(QuestSO quest, string giverID = null)
    {
        var status = GetQuestStatus(quest);
        if (status == null || !status.IsReadyToTurnIn)
            return false;

        return string.IsNullOrWhiteSpace(giverID) ||
               string.IsNullOrWhiteSpace(status.GiverID) ||
               status.GiverID == TaskBoardSourceID ||
               status.GiverID == giverID;
    }

    public bool TurnInQuest(QuestSO quest, string giverID = null)
    {
        if (!CanTurnInQuest(quest, giverID))
            return false;

        var status = GetQuestStatus(quest);
        status.FinishQuest();
        GiveReward(quest);
        onUpdate?.Invoke();
        OnQuestTurnedIn?.Invoke(status);
        ShowQuestNotification($"Quest completed: {quest.GetQuestTitle()}");
        return true;
    }

    private void HandleEnemyKilled(EnemyStatistics enemy)
    {
        if (enemy == null || enemy.Stats == null)
            return;

        UpdateObjectiveProgress((objective, _) =>
            objective.type == QuestObjectiveType.KillEnemy && objective.enemyType == enemy.Stats);
    }

    private void HandleItemCollected(ItemSO item, int quantity)
    {
        UpdateObjectiveProgress((objective, _) =>
            objective.type == QuestObjectiveType.CollectItem && objective.item == item, quantity);
    }

    private void HandleMapCompleted(string sceneName)
    {
        UpdateObjectiveProgress((objective, _) =>
            objective.type == QuestObjectiveType.CompleteMap &&
            string.Equals(objective.sceneName?.Trim(), sceneName, System.StringComparison.OrdinalIgnoreCase));
    }

    private void UpdateObjectiveProgress(
        System.Func<QuestSO.QuestObjective, int, bool> objectiveMatches,
        int amount = 1)
    {
        var changed = false;

        foreach (var status in _questStatuses)
        {
            if (status.IsQuestCompleted)
                continue;

            var quest = status.GetQuest();
            if (quest == null)
                continue;

            var objectives = quest.GetObjectives();
            var wasReadyToTurnIn = status.IsReadyToTurnIn;
            var questChanged = false;

            for (var i = 0; i < objectives.Count; i++)
            {
                if (objectiveMatches(objectives[i], i))
                    questChanged |= status.AddObjectiveProgress(i, amount);
            }

            if (!questChanged)
                continue;

            changed = true;

            if (!wasReadyToTurnIn && status.IsReadyToTurnIn)
            {
                if (status.GiverID == TaskBoardSourceID)
                {
                    status.FinishQuest();
                    GiveReward(quest);
                    OnQuestTurnedIn?.Invoke(status);
                    ShowQuestNotification($"Quest completed: {quest.GetQuestTitle()}");
                }
                else
                {
                    OnQuestReadyToTurnIn?.Invoke(status);
                    var giverText = string.IsNullOrWhiteSpace(status.GiverName)
                        ? string.Empty
                        : $" Return to {status.GiverName}.";
                    ShowQuestNotification($"Quest objectives complete: {quest.GetQuestTitle()}.{giverText}");
                }
            }
        }

        if (changed)
            onUpdate?.Invoke();
    }

    private void ShowQuestNotification(string message)
    {
        if (ToastrPanelManager.Instance != null)
            ToastrPanelManager.Instance.Show(message);

        if (_questNotificationSound != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySound(_questNotificationSound);
    }
        
    [CanBeNull]
    private QuestStatus GetQuestStatus(string questId)
    {
        return _questStatuses.FirstOrDefault(status => status._questID == questId);
    }
    
    public bool HasPlayerQuest(QuestSO quest) => _questStatuses.Any(status => status._questID == quest.Id);

    public bool IsQuestEntryInJournal(QuestSO quest, string entryKey)
    {
        var questEntry = quest.GetEntry(entryKey);
        var questStatus = GetQuestStatus(quest);

        if (questStatus == null) return false;
        
        return questStatus.HasQuestEntry(questEntry);
    }

    public bool IsQuestCompleted(QuestSO quest) => IsQuestCompleted(quest.Id);
    
    public bool IsQuestCompleted(string questId)
    {
        var questStatus = GetQuestStatus(questId);
        
        return questStatus != null && questStatus.IsQuestCompleted;
    }

    public void Save()
    {
        var saveData = new List<QuestSaveData>();

        foreach (var questStatus in _questStatuses)
        {
            saveData.Add(new QuestSaveData
            {
                questID = questStatus._questID,
                entries = questStatus.GetQuestEntries,
                objectiveProgress = questStatus.GetObjectiveProgressValues(),
                giverID = questStatus.GiverID,
                giverName = questStatus.GiverName,
                giverProfession = questStatus.GiverProfession,
                giverPortraitName = questStatus.GiverPortraitName,
                completed = questStatus.IsQuestCompleted
            });
        }
        
        var settings = SaveLoadManager.Instance.GetSettings();
        
        ES3.Save("player_quest_statuses", saveData, settings);
    }

    public void Load()
    {
        var settings = SaveLoadManager.Instance != null
            ? SaveLoadManager.Instance.GetSettings()
            : null;
        if (settings == null || !ES3.KeyExists("player_quest_statuses", settings)) return;

        var saveData = ES3.Load<List<QuestSaveData>>("player_quest_statuses", settings);

        _questStatuses.Clear();

        foreach (var questSaveData in saveData)
        {
            var quest = QuestDatabase.Get(questSaveData.questID);
            if (quest == null)
            {
                Debug.LogWarning($"Skipping saved quest with missing data: {questSaveData.questID}", this);
                continue;
            }

            var savedPortrait = FindSavedPortrait(questSaveData.giverPortraitName);
            var questStatus = new QuestStatus(
                quest,
                questSaveData.giverID,
                questSaveData.giverName,
                savedPortrait,
                questSaveData.giverProfession);

            if (questSaveData.entries != null)
            {
                foreach (var entry in questSaveData.entries)
                    questStatus.AddQuestEntry(entry);
            }

            questStatus.RestoreProgress(questSaveData.objectiveProgress);
            
            if(questSaveData.completed) questStatus.FinishQuest();
            
            _questStatuses.Add(questStatus);
        }

        onUpdate?.Invoke();
    }

    private static Sprite FindSavedPortrait(string spriteName)
    {
        if (string.IsNullOrWhiteSpace(spriteName))
            return null;

        foreach (var sprite in Resources.FindObjectsOfTypeAll<Sprite>())
        {
            if (sprite != null && sprite.name == spriteName)
                return sprite;
        }

        return null;
    }
}
