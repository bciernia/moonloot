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
        if (status.GetCompletedCount() == status.GetQuest().GetObjectivesCount())
        {
            _title.fontStyle = FontStyles.Strikethrough;
            _progress.fontStyle = FontStyles.Strikethrough;
        }
    }

    public QuestStatus GetQuestStatus()
    {
        return Status;
    }
}
