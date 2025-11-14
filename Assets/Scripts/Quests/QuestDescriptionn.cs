using TMPro;
using UnityEngine;

public class QuestDescriptionn : MonoBehaviour
{
    public void SetQuestDescription(QuestStatuss questStatuss)
    {
        QuestInfoManager.Instance.SetQuestDetails(questStatuss);        
    }
}