using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class QuestList : MonoBehaviour
{
    private readonly List<QuestStatuss> _statuses = new List<QuestStatuss>();

    public event Action onUpdate;
    public delegate void ObjectiveCompletedHandler(Quest quest, string objectiveId);
    public event ObjectiveCompletedHandler OnObjectiveCompleted;
    
    public void AddQuest(Quest quest)
    {
        if (IsQuestInQuestList(quest)) return;
        var newStatus = new QuestStatuss(quest);
        _statuses.Add(newStatus);
        onUpdate?.Invoke();
    }

    public bool IsQuestInQuestList(Quest quest) => GetQuestStatus(quest) != null;

    public IEnumerable<QuestStatuss> GetStatuses()
    {
        return _statuses;
    }

    public void CompleteObjective(Quest quest, string objective)
    {
        var status = GetQuestStatus(quest);
        
        if (status != null)
        {
            status.CompleteObjective(objective);
            OnObjectiveCompleted?.Invoke(quest, objective);
            if (status.IsComplete())
            {
                GiveReward(quest);
            }
        }
        
        onUpdate?.Invoke();
    }

    private void GiveReward(Quest quest)
    {
        foreach (var reward in quest.GetRewards())
        {
            InventoryController.Instance.AddItem(new InventoryItem() { item = reward.item.item, quantity = reward.item.quantity});
            //TODO sprawdzić czy są miejsca w eq, jak nie ma to wyrzucić przedmioty na ziemię.
        }
    }

    public bool IsObjectiveCompleted(Quest quest, string objective)
    {
        var questStatus = GetQuestStatus(quest);

        return questStatus.IsObjectiveComplete(objective);
    }

    public bool IsQuestCompleted(Quest quest)
    {
        var questStatus = GetQuestStatus(quest);
        return questStatus != null && questStatus.IsComplete();
    }
    
    [CanBeNull]
    private QuestStatuss GetQuestStatus(Quest quest)
    {
        foreach (var status in _statuses)
        {
            if (status.GetQuest() == quest) return status;
        }

        return null;
    }

    public bool HasPlayerQuest(Quest quest) => _statuses.Any(status => status.GetQuest().GetTitle() == quest.GetTitle());
}