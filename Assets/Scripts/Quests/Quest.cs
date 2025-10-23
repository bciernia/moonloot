using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "Quest")]
public class Quest : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField][TextArea] private string _description;
    [SerializeField] private List<Reward> _rewards = new List<Reward>();
    [SerializeField] private List<Objective> _objectives = new List<Objective>();
    [SerializeField] private ItemSO _itemInQuest;
    [SerializeField] private List<Quest> _relatedQuests = new List<Quest>();
    
    [System.Serializable]
    public class Reward
    {
        [Min(1)]
        public int number;
        public InventoryItem item;
    }

    [System.Serializable]
    public class Objective
    {
        public string reference;
        public string description;
    }
    
    public string GetTitle()
    {
        return _name ?? name;
    }
    
    public string GetDescription()
    {
        return _description;
    }

    public int GetObjectivesCount() => _objectives.Count;

    public IEnumerable<Objective> GetObjectives() => _objectives;

    public IEnumerable<Reward> GetRewards() => _rewards;

    public IEnumerable<Quest> GetRelatedQuests() =>  _relatedQuests;

    public string GetQuestItemName() => _itemInQuest.Name;

    public string GetRewardDescription()
    {
        var sb = new StringBuilder();
        foreach (var reward in _rewards)
        {
            if(reward.number > 1) sb.Append($"{reward.number} ");
            sb.Append($"{reward.item.item.Name}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public bool HasObjective(string objectiveRef)
    {
        foreach (var objective in _objectives)
        {
            if (objective.reference == objectiveRef) return true;
        }

        return false;
    }
}
