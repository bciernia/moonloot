using System;
using UnityEngine;

public class QuestCompletion : MonoBehaviour
{
    [SerializeField] private Quest quest;
    [SerializeField] private string objective;

    public Quest Quest { get; private set; }

    private void Awake()
    {
        Quest = quest;
    }

    //Used in dialogue nodes
    public void CompleteObjective()
    {
        var questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
        questList.CompleteObjective(quest, objective);
    }
    
    public bool IsObjectiveCompleted(string objectiveName)
    {
        var questStatus = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
        return questStatus.IsObjectiveCompleted(quest, objectiveName);
    }
}
