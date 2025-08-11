using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestCard : MonoBehaviour
{
    private Quest questData;
    public TextMeshProUGUI titleText;
    public Button button;

    public void Setup(Quest quest, System.Action<Quest> onClickCallback)
    {
        questData = quest;
        titleText.text = quest.GetTitle();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickCallback?.Invoke(questData));
    }
    
    public bool HasQuest(Quest quest)
    {
        return questData == quest;
    }
}