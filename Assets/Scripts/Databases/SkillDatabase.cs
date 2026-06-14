using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillDatabase : Singleton<SkillDatabase>
{
    [SerializeField] private List<Skill> skills;

    private static Dictionary<string, Skill> _skills;

    protected override void Awake()
    {
        _skills = skills.ToDictionary(s => s.Id, s => s);
    }

    public static Skill Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        
        if (_skills.TryGetValue(id, out var skill))
            return skill;

        Debug.LogError($"Skill with ID {id} not found");
        return null;
    }
}