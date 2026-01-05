using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

//Used in dialogue nodes, must be public
public class QuestService : MonoBehaviour
{
    [SerializeField] private List<QuestSO> _questToEntries = new List<QuestSO>();
    
    private readonly List<QuestSO> _questList = new List<QuestSO>();
    
    private QuestJournal _questJournal;
    private QuestGiver _questGiver;

    private void Awake()
    {
        if (TryGetComponent<QuestGiver>(out var questGiver))
        {
            _questGiver = questGiver;
        }
        
        _questJournal = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestJournal>();
        TryFillQuestList();
    }

    private void TryFillQuestList() 
    {
        if (TryGetComponent<QuestGiver>(out var questGiver))
            _questList.AddRange(questGiver.GetQuests());

        if (_questToEntries != null && _questToEntries.Count > 0)
            _questList.AddRange(_questToEntries);
    }
    
    //Used in dialogue nodes, must be public
    public void AddEntry(int questIndex, string entryKey)
    {
        AddDialogueFlag(entryKey);
        _questJournal.AddEntryToJournal(_questList[questIndex], entryKey);
    }

    //Used in dialogue nodes, must be public
    public void AddDialogueFlag(string flag)
    {
        DialogueFlagsManager.Instance.AddFlagToHashSet(flag.Trim());
    }
    
    //Used in dialogue nodes, must be public
    public void AddEntryAndFinishQuest(int questIndex, string entryKey, bool shouldGiveReward = true)
    {
        AddDialogueFlag(entryKey);
        _questJournal.AddEntryToJournalAndFinishQuest(_questList[questIndex], entryKey, shouldGiveReward);        
    }

    //Used in dialogue nodes, must be public
    public void GiveQuest(int questIndex, string entryKey)
    {
        _questGiver.GiveQuest(questIndex, entryKey);
    }

    public bool IsDialogueFlagInHashSet(string entryKey) => DialogueFlagsManager.Instance.IsFlagInHashSet(entryKey.Trim());
    
    public bool IsEntryInJournal(int questIndex, string entryKey)
    {
        return _questJournal.IsQuestEntryInJournal(_questList[questIndex], entryKey);
    }

    public bool IsQuestCompleted(int questIndex)
    {
        return _questJournal.IsQuestCompleted(_questList[questIndex]);
    }

    public bool IsQuestInQuestJournal(int questIndex) => _questJournal.HasPlayerQuest(_questList[questIndex]);
    
    public bool HasPlayerQuestItem(int itemIndex, int quantity)
    {
        var questItem = GetQuestItem(itemIndex);
        return questItem != null && InventoryController.Instance.HasUserQuestItem(questItem.item.item.Name, quantity);
    }
    
    public void TryRemoveQuestItems(int itemIndex, int quantity)
    {
        var questItem = GetQuestItem(itemIndex);
        if (questItem == null) return;
        
        InventoryController.Instance.TryRemoveQuestItems(questItem.item.item.Name, quantity);
    }

    [CanBeNull]
    private QuestSO.QuestItem GetQuestItem(int itemIndex)
    {
        var questItems = _questList[itemIndex].GetQuestItems().ToList();
        return questItems.Count == 0 ? null : questItems[itemIndex];
    }
}