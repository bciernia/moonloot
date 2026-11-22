using TMPro;
using UnityEngine;

public class QuestTooltipUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private Transform _objectiveContainer;
    [SerializeField] private GameObject _objectivePrefab;
    [SerializeField] private GameObject _objectiveInCompletePrefab;
    [SerializeField] private TextMeshProUGUI _rewardText;
    
    public void Setup(QuestStatuss statuss)
    {
        var quest = statuss.GetQuest();
        _title.text = quest.GetTitle();
        _objectiveContainer.DetachChildren();
        foreach (var objective in quest.GetObjectives())
        {
            var prefab = _objectiveInCompletePrefab;
            if (statuss.IsObjectiveComplete(objective.reference))
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
            rewardText += GetLocalizedItemName(reward.item.item);
        }

        if (rewardText == "")
        {
            rewardText = "No reward";
        }

        rewardText += ".";
        return LocalizationManager.Instance != null
            ? LocalizationManager.Instance.Translate(rewardText)
            : rewardText;
    }

    private static string GetLocalizedItemName(ItemSO item)
    {
        return item != null && LocalizationManager.Instance != null
            ? item.GetLocalizedName(LocalizationManager.Instance.CurrentLanguage)
            : item != null ? item.Name : string.Empty;
    }
}
