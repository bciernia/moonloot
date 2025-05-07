using System.Collections.Generic;
using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    [SerializeField] private List<Quest> _quest;

    //Used in dialogues in Trigger Script nodes
    public void GiveQuest(int id = 0)
    {
        var questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
        questList.AddQuest(_quest[id]);
    }
}
