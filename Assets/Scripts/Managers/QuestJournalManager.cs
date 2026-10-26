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
        var player = GameObject.FindGameObjectWithTag("Player");
        _questJournal = player != null ? player.GetComponent<QuestJournal>() : null;
        if (_questJournal == null)
        {
            Debug.LogWarning("QuestJournalManager could not find QuestJournal on the Player.", this);
            return;
        }

        if (JournalEntryPrefab == null || JournalContainer == null)
        {
            Debug.LogWarning("Assign JournalEntryPrefab and JournalContainer on QuestJournalManager.", this);
            return;
        }

        _questJournal.onUpdate += RedrawQuest;
        if (StartQuest != null)
            _questJournal.AddQuest(StartQuest, StartEntry);

        RedrawQuest();
    }

    private void OnDestroy()
    {
        if (_questJournal != null)
            _questJournal.onUpdate -= RedrawQuest;
    }
    
    private void RedrawQuest()
    {
        if (_questJournal == null || JournalContainer == null || JournalEntryPrefab == null)
            return;

        for (var i = JournalContainer.childCount - 1; i >= 0; i--)
            Destroy(JournalContainer.GetChild(i).gameObject);

        foreach (var questStatus in _questJournal.GetQuestStatuses())
        {
            if (questStatus == null || questStatus.GetQuest() == null)
                continue;

            var entryInstance = Instantiate(JournalEntryPrefab, JournalContainer);
            var questElement = entryInstance.GetComponent<QuestElementUI>();
            if (questElement == null)
            {
                Debug.LogError("JournalEntryPrefab must have a QuestElementUI component.", entryInstance);
                Destroy(entryInstance);
                continue;
            }

            questElement.Setup(questStatus);
        }
    }

    public void SetQuestDetails(QuestStatus questStatus)
    {
        // The journal container now displays the quest cards themselves.
        // Keep the selected status for callers that still select a card, but
        // do not replace the list with text entries.
        QuestStatus = questStatus;
    }
}
