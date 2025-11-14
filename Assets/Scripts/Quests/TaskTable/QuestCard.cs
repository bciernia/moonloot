using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestCard : MonoBehaviour
{
    private QuestSO questData;
    public TextMeshProUGUI titleText;
    public Button button;

    public void Setup(QuestSO quest, System.Action<QuestSO> onClickCallback)
    {
        questData = quest;
        titleText.text = quest.GetQuestTitle();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickCallback?.Invoke(questData));
    }
    
    public bool HasQuest(QuestSO quest)
    {
        return questData == quest;
    }
}