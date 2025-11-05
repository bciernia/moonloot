using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class QuestCompletion : MonoBehaviour
{
    [SerializeField] public List<Quest> quest;

    // private void Awake()
    // {
    //     quest = new List<Quest>();
    // }

    //Used in dialogue nodes
    public void CompleteObjective(int questIndex, string objectiveName)
    {
        var questList = GetPlayerQuestList();
        questList.CompleteObjective(quest[questIndex], objectiveName);
    }
    
    public bool IsObjectiveCompleted(int questIndex, string objectiveName)
    {
        var questList = GetPlayerQuestList();
        return questList.IsObjectiveCompleted(quest[questIndex], objectiveName);
    }

    public bool IsQuestCompleted(int questIndex)
    {
        var questList = GetPlayerQuestList();
        
        return questList.IsQuestCompleted(quest[questIndex]);
    }

    private QuestList GetPlayerQuestList()
    {
        return GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
    }
}
