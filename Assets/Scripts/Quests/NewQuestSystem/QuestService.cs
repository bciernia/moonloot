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
        _questGiver = GetComponent<QuestGiver>() ?? GetComponentInChildren<QuestGiver>(true);
        _questJournal = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestJournal>();
        TryFillQuestList();
    }

    private void TryFillQuestList() 
    {
        _questList.Clear();

        if (_questGiver != null)
        {
            foreach (var quest in _questGiver.GetQuests())
            {
                if (quest != null && !_questList.Contains(quest))
                    _questList.Add(quest);
            }
        }

        if (_questToEntries != null && _questToEntries.Count > 0)
        {
            foreach (var quest in _questToEntries)
            {
                if (quest != null && !_questList.Contains(quest))
                    _questList.Add(quest);
            }
        }
    }

    public void RefreshQuestList()
    {
        _questGiver ??= GetComponent<QuestGiver>() ??
                       GetComponentInParent<QuestGiver>() ??
                       GetComponentInChildren<QuestGiver>(true);
        TryFillQuestList();
    }
    
    //Used in dialogue nodes, must be public
    public void AddEntry(int questIndex, string entryKey)
    {
        var quest = GetQuestAt(questIndex);
        if (quest == null) return;
        AddDialogueFlag(entryKey);
        _questJournal.AddEntryToJournal(quest, entryKey);
    }

    //Used in dialogue nodes, must be public
    public void AddDialogueFlag(string flag)
    {
        DialogueFlagsManager.Instance.AddFlagToHashSet(flag.Trim());
    }
    
    //Used in dialogue nodes, must be public
    public void AddEntryAndFinishQuest(int questIndex, string entryKey, bool shouldGiveReward = true)
    {
        var quest = GetQuestAt(questIndex);
        if (quest == null) return;
        AddDialogueFlag(entryKey);
        _questJournal.AddEntryToJournalAndFinishQuest(quest, entryKey, shouldGiveReward);        
    }

    public void FinishQuest(int questIndex)
    {
        var quest = GetQuestAt(questIndex);
        if (quest != null)
            _questJournal.AddEntryToJournalAndFinishQuest(quest, "", true);
    }

    //Used in dialogue nodes, must be public
    public void GiveQuest(int questIndex, string entryKey = "")
    {
        var quest = GetQuestAt(questIndex);
        if (quest == null)
            return;

        if (_questGiver != null && _questGiver.GetQuest(questIndex) == quest)
            _questGiver.GiveQuest(questIndex, entryKey);
        else
            _questJournal.AddQuest(
                quest,
                entryKey,
                _questGiver != null ? _questGiver.GiverID : gameObject.name,
                _questGiver != null ? _questGiver.GiverDisplayName : gameObject.name,
                _questGiver != null ? _questGiver.GiverPortrait : GetComponentInChildren<SpriteRenderer>(true)?.sprite,
                _questGiver != null ? _questGiver.GiverProfession : string.Empty);
    }

    // Use as EasyTalk dialogue conditions/actions for NPC quest turn-ins.
    public bool CanTurnInQuest(int questIndex)
    {
        var quest = GetQuestAt(questIndex);
        return quest != null && _questJournal.CanTurnInQuest(
            quest, _questGiver != null ? _questGiver.GiverID : gameObject.name);
    }

    public bool TurnInQuest(int questIndex)
    {
        var quest = GetQuestAt(questIndex);
        return quest != null && _questJournal.TurnInQuest(
            quest, _questGiver != null ? _questGiver.GiverID : gameObject.name);
    }

    public bool IsDialogueFlagInHashSet(string entryKey) => DialogueFlagsManager.Instance.IsFlagInHashSet(entryKey.Trim());
    
    public bool IsEntryInJournal(int questIndex, string entryKey)
    {
        var quest = GetQuestAt(questIndex);
        return quest != null && _questJournal.IsQuestEntryInJournal(quest, entryKey);
    }

    public bool IsQuestCompleted(int questIndex)
    {
        var quest = GetQuestAt(questIndex);
        return quest != null && _questJournal.IsQuestCompleted(quest);
    }

    public bool IsQuestInQuestJournal(int questIndex)
    {
        var quest = GetQuestAt(questIndex);
        return quest != null && _questJournal.HasPlayerQuest(quest);
    }

    // Boolean helpers for EasyTalk dialogue conditions.
    public bool RequiresBringingItems(int questIndex)
    {
        var quest = GetQuestAt(questIndex);
        if (quest == null)
            return false;

        if (quest.GetQuestItems().Any(questItem =>
                questItem.item.item != null &&
                (questItem.number > 0 || questItem.item.quantity > 0)))
            return true;

        return HasObjectiveType(quest, QuestObjectiveType.CollectItem);
    }

    public bool RequiresKillingEnemies(int questIndex) =>
        HasObjectiveType(GetQuestAt(questIndex), QuestObjectiveType.KillEnemy);

    public bool RequiresCompletingMap(int questIndex) =>
        HasObjectiveType(GetQuestAt(questIndex), QuestObjectiveType.CompleteMap);

    private static bool HasObjectiveType(QuestSO quest, QuestObjectiveType objectiveType)
    {
        return quest != null && quest.GetObjectives().Any(objective =>
            objective != null && objective.type == objectiveType);
    }
    
    public bool HasPlayerQuestItem(int itemIndex, int quantity)
    {
        var questItem = GetQuestItem(itemIndex);
        return questItem != null && InventoryController.Instance.HasUserQuestItem(questItem.item.item.Name, quantity);
    }
    
    public void TryRemoveQuestItems(int itemIndex)
    {
        var questItem = GetQuestItem(itemIndex);
        if (questItem == null) return;
        
        InventoryController.Instance.TryRemoveQuestItems(questItem.item.item.Name, questItem.item.quantity);
    }

    [CanBeNull]
    private QuestSO.QuestItem GetQuestItem(int itemIndex)
    {
        var quest = _questGiver != null && _questGiver.AssignedQuest != null
            ? _questGiver.AssignedQuest
            : itemIndex >= 0 && itemIndex < _questList.Count ? _questList[itemIndex] : null;
        if (quest == null)
            return null;

        var questItems = quest.GetQuestItems().ToList();
        return itemIndex >= 0 && itemIndex < questItems.Count ? questItems[itemIndex] : null;
    }

    private QuestSO GetQuestAt(int index)
    {
        if (_questGiver != null && _questGiver.AssignedQuest != null)
            return index == 0 ? _questGiver.AssignedQuest : null;

        return index >= 0 && index < _questList.Count ? _questList[index] : null;
    }
}
