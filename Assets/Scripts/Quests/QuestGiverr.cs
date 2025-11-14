using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestGiverr : MonoBehaviour
{
    [SerializeField] private List<Quest> _quest;
    [SerializeField] private List<QuestSO> _questSO;

    private QuestList _questList;
    private QuestJournal _questJournal;

    private void Awake()
    {
        _questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
        _questJournal = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestJournal>();
    }

    public Quest GetQuest(int questIndex = 0)
    {
        return _quest[questIndex];
    }

//Used in dialogues in Trigger Script nodes
    public void GiveQuest(int id = 0)
    {
        _questList.AddQuest(_quest[id]);
        _questJournal.AddQuest(_questSO[id], "A");
    }
    
//Used in dialogues in Trigger Script nodes
    public bool HasPlayerQuestInQuestList(int id = 0) => _questList.HasPlayerQuest(_quest[id]);
}
