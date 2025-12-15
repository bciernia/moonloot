using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestStatus
{
    [SerializeField] private QuestSO _quest;
    [SerializeField] private List<string> _questEntries = new List<string>();
    [SerializeField] public bool IsQuestCompleted { get; private set; }
    
    public QuestStatus(QuestSO quest)
    {
        _quest = quest;
    }

    public QuestSO GetQuest() => _quest;

    public void AddQuestEntry(string journalEntry)
    {
        if(!_questEntries.Contains(journalEntry))
            _questEntries.Add(journalEntry);
    }

    public bool HasQuestEntry(string journalEntry) => _questEntries.Contains(journalEntry);

    public void FinishQuest() => IsQuestCompleted = true;

    public List<string> GetQuestEntries => _questEntries;
}