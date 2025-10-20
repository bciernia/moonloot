using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    [SerializeField] private List<Quest> _quest;

    private QuestList _questList;

    private void Awake()
    {
        _questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
    }

    public Quest GetQuest(int questIndex = 0)
    {
        return _quest[questIndex];
    }

//Used in dialogues in Trigger Script nodes
    public void GiveQuest(int id = 0)
    {
        _questList.AddQuest(_quest[id]);
    }
    
//Used in dialogues in Trigger Script nodes
    public bool HasPlayerQuestInQuestList(int id = 0) => _questList.HasPlayerQuest(_quest[id]);
}
