using TMPro;
using UnityEngine;

public class QuestItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _progress;

    private QuestStatus Status { get; set; }
    
    public void Setup(QuestStatus status)
    {
        Status = status;
        _title.text = status.GetQuest().GetTitle();
        _progress.text = $"{status.GetCompletedCount()} / {status.GetQuest().GetObjectivesCount()}";
    }

    public QuestStatus GetQuestStatus()
    {
        return Status;
    }
}
