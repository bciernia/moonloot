using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestList : MonoBehaviour
{
    private List<QuestStatus> _statuses = new List<QuestStatus>();

    public event Action onUpdate;
    
    public void AddQuest(Quest quest)
    {
        if (HasQuest(quest)) return;
        var newStatus = new QuestStatus(quest);
        _statuses.Add(newStatus);
        onUpdate?.Invoke();
    }

    private bool HasQuest(Quest quest) => GetQuestStatus(quest) != null;

    public IEnumerable<QuestStatus> GetStatuses()
    {
        return _statuses;
    }

    public void CompleteObjective(Quest quest, string objective)
    {
        var status = GetQuestStatus(quest);
        status.CompleteObjective(objective);
        if (status.IsComplete())
        {
            GiveReward(quest);
        }
        onUpdate?.Invoke();
    }

    private void GiveReward(Quest quest)
    {
        foreach (var reward in quest.GetRewards())
        {
            Inventory.Instance.AddItem(reward.item, reward.number);
            //TODO sprawdzić czy są miejsca w eq, jak nie ma to wyrzucić przedmioty na ziemię.
        }
    }

    private QuestStatus GetQuestStatus(Quest quest)
    {
        foreach (var status in _statuses)
        {
            if (status.GetQuest() == quest) return status;
        }

        return null;
    }
}