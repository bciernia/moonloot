using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestInfoManager : Singleton<QuestInfoManager>
{
    [Header("Quest description ui elements")]
    [SerializeField] public TextMeshProUGUI Title;
    [SerializeField] public TextMeshProUGUI Description;
    [SerializeField] public TextMeshProUGUI Reward;
    [SerializeField] public GameObject _objectivePrefab;
    [SerializeField] private GameObject _objectiveInCompletePrefab;
    [SerializeField] public Transform ObjectiveContainer;
    [SerializeField] public GameObject QuestDescriptionPanel;

    private QuestList _questList;

    private QuestStatuss Statuss { get; set; }
    
    void Start()
    {
        _questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
        _questList.onUpdate += RedrawObjectives;
        RedrawObjectives();
        QuestDescriptionPanel.SetActive(false);
    }

    private void RedrawObjectives()
    {
        if (Statuss == null) return;
        
        transform.DetachChildren();
        SetQuestDetails(Statuss);
    }
    
    public void SetQuestDetails(QuestStatuss questStatuss)
    {
        QuestDescriptionPanel.SetActive(true);

        Statuss = questStatuss;
        
        var quest = questStatuss.GetQuest();
        
        Title.text = quest.GetTitle();
        Description.text = quest.GetDescription();
        Reward.text = GetRewardText(quest);
        
        ObjectiveContainer.DetachChildren();
        
        foreach (var objective in quest.GetObjectives())
        {
            bool isComplete = questStatuss.IsObjectiveComplete(objective.reference);

            if (isComplete)
            {
                var objectiveInstance = Instantiate(_objectivePrefab, ObjectiveContainer);
                var objectiveText = objectiveInstance.GetComponentInChildren<TextMeshProUGUI>();
                objectiveText.text = objective.description;
            }
            else
            {
                var objectiveInstance = Instantiate(_objectiveInCompletePrefab, ObjectiveContainer);
                var objectiveText = objectiveInstance.GetComponentInChildren<TextMeshProUGUI>();
                objectiveText.text = objective.description;
                break;
            }
        }
    }
    
    private string GetRewardText(Quest quest)
    {
        var rewardText = "";
        foreach (var reward in quest.GetRewards())
        {
            if (rewardText != "")
            {
                rewardText += ", ";
            }

            if (reward.number > 1)
            {
                rewardText += reward.number + " ";
            }
            rewardText += reward.item.item.Name;
        }

        if (rewardText == "")
        {
            rewardText = "No reward";
        }

        rewardText += ".";
        return rewardText;
    }
}
