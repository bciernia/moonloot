using System;
using TMPro;
using UnityEngine;

public class QuestItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _title;

    private QuestDescriptionn _questDescriptionn;

    private void Awake()
    {
        _questDescriptionn = GetComponent<QuestDescriptionn>();
    }

    private QuestStatuss Statuss { get; set; }
    
    public void Setup(QuestStatuss statuss)
    {
        Statuss = statuss;
        _title.text = statuss.GetQuest().GetTitle();
        if (statuss.GetCompletedCount() == statuss.GetQuest().GetObjectivesCount())
        {
            _title.fontStyle = FontStyles.Strikethrough;
        }
    }

    public void SetQuestDescription()
    {
        _questDescriptionn.SetQuestDescription(Statuss);
    }

    public QuestStatuss GetQuestStatus()
    {
        return Statuss;
    }
}
