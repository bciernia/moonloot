using System.Collections.Generic;

public class QuestStatus
{
    private QuestSO _quest;

    private List<string> _questEntries = new List<string>();
    
    public bool IsQuestCompleted { get; private set; }
    
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