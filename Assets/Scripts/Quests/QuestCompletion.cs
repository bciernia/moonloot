using System;
using UnityEngine;

public class QuestCompletion : MonoBehaviour
{
    [SerializeField] private Quest quest;

    public Quest Quest { get; private set; }

    public delegate void ObjectiveCompletedHandler(string objectiveId);
    public event ObjectiveCompletedHandler OnObjectiveCompleted;
    
    private void Awake()
    {
        Quest = quest;
    }

    //Used in dialogue nodes
    public void CompleteObjective(string objectiveName)
    {
        var questList = GetPlayerQuestList();
        questList.CompleteObjective(quest, objectiveName);
        OnObjectiveCompleted?.Invoke(objectiveName);
    }
    
    public bool IsObjectiveCompleted(string objectiveName)
    {
        var questList = GetPlayerQuestList();
        return questList.IsObjectiveCompleted(quest, objectiveName);
    }

    private QuestList GetPlayerQuestList()
    {
        return GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
    }
}
