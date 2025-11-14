using UnityEngine;

public class QuestDescription : MonoBehaviour
{
    public void SetQuestDescription(QuestStatus questStatus)
    {
        QuestJournalManager.Instance.SetQuestDetails(questStatus);
    }
}