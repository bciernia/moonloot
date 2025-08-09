using System;
using TMPro;
using UnityEngine;

public class QuestItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _title;

    private QuestDescription _questDescription;

    private void Awake()
    {
        _questDescription = GetComponent<QuestDescription>();
    }

    private QuestStatus Status { get; set; }
    
    public void Setup(QuestStatus status)
    {
        Status = status;
        _title.text = status.GetQuest().GetTitle();
        if (status.GetCompletedCount() == status.GetQuest().GetObjectivesCount())
        {
            _title.fontStyle = FontStyles.Strikethrough;
        }
    }

    public void SetQuestDescription()
    {
        _questDescription.SetQuestDescription(Status);
    }

    public QuestStatus GetQuestStatus()
    {
        return Status;
    }
}
