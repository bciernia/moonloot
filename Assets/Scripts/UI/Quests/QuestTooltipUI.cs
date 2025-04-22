using TMPro;
using UnityEngine;

public class QuestTooltipUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private Transform _objectiveContainer;
    [SerializeField] private GameObject _objectivePrefab;
    [SerializeField] private GameObject _objectiveInCompletePrefab;
    [SerializeField] private TextMeshProUGUI _rewardText;
    
    public void Setup(QuestStatus status)
    {
        var quest = status.GetQuest();
        _title.text = quest.GetTitle();
        _objectiveContainer.DetachChildren();
        foreach (var objective in quest.GetObjectives())
        {
            var prefab = _objectiveInCompletePrefab;
            if (status.IsObjectiveComplete(objective.reference))
            {
                prefab = _objectivePrefab;
            }
            var objectiveInstance = Instantiate(prefab, _objectiveContainer);
            var objectiveText = objectiveInstance.GetComponentInChildren<TextMeshProUGUI>();
            objectiveText.text = objective.description;
        }

        _rewardText.text = GetRewardText(quest);
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
            rewardText += reward.item.Name;
        }

        if (rewardText == "")
        {
            rewardText = "No reward";
        }

        rewardText += ".";
        return rewardText;
    }
}
