using System;
using UnityEngine;

public class QuestCompletion : MonoBehaviour
{
    [SerializeField] private Quest quest;

    public Quest Quest { get; private set; }

    private void Awake()
    {
        Quest = quest;
    }

    //Used in dialogue nodes
    public void CompleteObjective(string objectiveNumber)
    {
        var questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
        questList.CompleteObjective(quest, objectiveNumber);
    }
    
    public bool IsObjectiveCompleted(string objectiveName)
    {
        var questStatus = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
        return questStatus.IsObjectiveCompleted(quest, objectiveName);
    }
}
