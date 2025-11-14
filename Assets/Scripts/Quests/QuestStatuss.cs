using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Update = Unity.VisualScripting.Update;

public class QuestStatuss
{
    private Quest _quest;
    private List<string> _completedObjectives = new List<string>();

    public QuestStatuss(Quest quest)
    {
        _quest = quest;
    }
    
    public Quest GetQuest()
    {
        return _quest;
    }

    public int GetCompletedCount()
    {
        return _completedObjectives.Count;
    }

    public bool IsObjectiveComplete(string objective)
    {
        return _completedObjectives.Contains(objective);
    }

    public void CompleteObjective(string objective)
    {
        if (_quest.HasObjective(objective))
        {
            _completedObjectives.Add(objective);
        }
    }

    public bool IsComplete()
    {
        foreach (var objective in _quest.GetObjectives())
        {
            if (!_completedObjectives.Contains(objective.reference))
            {
                return false;
            }
        }
        return true;
    }
}
