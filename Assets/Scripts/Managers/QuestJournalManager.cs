using TMPro;
using UnityEngine;

public class QuestJournalManager : Singleton<QuestJournalManager>
{
    [SerializeField] public GameObject JournalEntryPrefab;
    [SerializeField] public Transform JournalContainer;

    [SerializeField] public QuestSO StartQuest;
    [SerializeField] public string StartEntry;
    
    private QuestJournal _questJournal;
    
    private QuestStatus QuestStatus { get; set; }

    private void Start()
    {
        _questJournal = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestJournal>();
        _questJournal.onUpdate += RedrawQuest;
        RedrawQuest();
        
        _questJournal.AddQuest(StartQuest, StartEntry);
    }
    
    private void RedrawQuest()
    {
        if (QuestStatus == null) return;
        SetQuestDetails(QuestStatus);
    }

    public void SetQuestDetails(QuestStatus questStatus)
    {
        QuestStatus = questStatus;

        JournalContainer.DetachChildren();

        foreach (var entry in questStatus.GetQuestEntries)
        {
            var entryInstance = Instantiate(JournalEntryPrefab, JournalContainer);
            var entryText = entryInstance.GetComponent<TextMeshProUGUI>();
            entryText.text = entry;
        }
    }
}