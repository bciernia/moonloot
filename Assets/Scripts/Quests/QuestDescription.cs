using TMPro;
using UnityEngine;

public class QuestDescription : MonoBehaviour
{
    public void SetQuestDescription(QuestStatus questStatus)
    {
        QuestInfoManager.Instance.SetQuestDetails(questStatus);        
    }
}