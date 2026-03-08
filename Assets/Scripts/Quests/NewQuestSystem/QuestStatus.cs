using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestStatus
{
    [SerializeField] public string _questID;
    [SerializeField] private List<string> _questEntries = new List<string>();
    [SerializeField] public bool IsQuestCompleted { get; private set; } = false;
    
    private QuestSO _quest;
    
    public QuestStatus(QuestSO quest)
    {
        _questID = quest.Id;
        _quest = quest;
    }

    public QuestSO GetQuest()
    {
        if (_quest == null)
            _quest = QuestDatabase.Get(_questID);

        return _quest;
    }
    public void AddQuestEntry(string journalEntry)
    {
        if(!_questEntries.Contains(journalEntry))
            _questEntries.Add(journalEntry);
    }

    public bool HasQuestEntry(string journalEntry) => _questEntries.Contains(journalEntry);

    public void FinishQuest() => IsQuestCompleted = true;

    public List<string> GetQuestEntries => _questEntries;
}