using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "Quest")]
public class Quest : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private string _description;
    [SerializeField] private List<Reward> _rewards = new List<Reward>();
    [SerializeField] private List<Objective> _objectives = new List<Objective>();
    
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

    public int GetObjectivesCount()
    {
        return _objectives.Count;
    }

    public IEnumerable<Objective> GetObjectives()
    {
        return _objectives;
    }

    public IEnumerable<Reward> GetRewards()
    {
        return _rewards;
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
