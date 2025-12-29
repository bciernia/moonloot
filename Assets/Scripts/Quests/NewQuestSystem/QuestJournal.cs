using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class QuestJournal : MonoBehaviour, ISaveable
{
    private List<QuestStatus> _questStatuses = new List<QuestStatus>();

    public event Action onUpdate;

    public void AddQuest(QuestSO quest, string entryKey)
    {
        if (IsQuestInQuestList(quest)) return;
        var newQuestStatus = new QuestStatus(quest);
        _questStatuses.Add(newQuestStatus);
        onUpdate?.Invoke();
        var initEntry = quest.GetEntry(entryKey);
        newQuestStatus.AddQuestEntry(initEntry);
    }
    
    private bool IsQuestInQuestList(QuestSO quest) => GetQuestStatus(quest) != null;

    public IEnumerable<QuestStatus> GetQuestStatuses() => _questStatuses;

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

        if (questStatus != null)
        {
            var questEntry = quest.GetEntry(journalEntry);
            questStatus.AddQuestEntry(questEntry);
            questStatus.FinishQuest();

            if (shouldGiveReward)
            {
                GiveReward(quest);
            }
        }
        
        onUpdate?.Invoke();
    }
    
    private void GiveReward(QuestSO quest)
    {
        foreach (var reward in quest.GetRewards())
        {
            InventoryController.Instance.AddItem(new InventoryItem() { item = reward.item.item, quantity = reward.item.quantity});
            //TODO sprawdzić czy są miejsca w eq, jak nie ma to wyrzucić przedmioty na ziemię.
        }
    }
        
    [CanBeNull]
    private QuestStatus GetQuestStatus(QuestSO quest)
    {
        foreach (var status in _questStatuses)
        {
            if (status.GetQuest() == quest) return status;
        }

        return null;
    }
    
    public bool HasPlayerQuest(QuestSO quest) => _questStatuses.Any(status => status.GetQuest().GetQuestTitle() == quest.GetQuestTitle());

    public bool IsQuestEntryInJournal(QuestSO quest, string entryKey)
    {
        var questEntry = quest.GetEntry(entryKey);
        var questStatus = GetQuestStatus(quest);

        if (questStatus == null) return false;
        
        return questStatus.HasQuestEntry(questEntry);
    }

    public bool IsQuestCompleted(QuestSO quest)
    {
        var questStatus = GetQuestStatus(quest);
        
        return questStatus != null && GetQuestStatus(quest)!.IsQuestCompleted;
    }

    public void Save()
    {
        ES3.Save("player_quest_statuses", _questStatuses);
    }

    public void Load()
    {
        _questStatuses = ES3.Load("player_quest_statuses", _questStatuses);
        onUpdate?.Invoke();
    }
}