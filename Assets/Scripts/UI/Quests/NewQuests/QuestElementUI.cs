using TMPro;
using UnityEngine;

public class QuestElementUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _title;

    private QuestDescription _questDescription;

    private void Awake()
    {
        _questDescription = GetComponent<QuestDescription>();
    }
    
    private QuestStatus QuestStatus { get; set; }

    public void Setup(QuestStatus questStatus)
    {
        QuestStatus = questStatus;
        _title.text = questStatus.GetQuest().GetQuestTitle();
        if (questStatus.IsQuestCompleted)
        {
            _title.color = Color.forestGreen;
        }
    }

    public void SetQuestDescription()
    {
        _questDescription.SetQuestDescription(QuestStatus);
    }
}