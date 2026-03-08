using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSkillProgress : MonoBehaviour, ISaveable
{
    public List<Skill> unlockedSkills = new List<Skill>();

    public event Action OnSkillsChanged;

    [SerializeField] private Skill Skill;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            UnlockSkill(Skill);
        }
    }

    public bool IsUnlocked(Skill skill)
    {
        return unlockedSkills.Contains(skill);
    }

    public void UnlockSkill(Skill skill)
    {
        if (!unlockedSkills.Contains(skill))
            unlockedSkills.Add(skill);

        OnSkillsChanged?.Invoke();
    }

    public void Save()
    {
        var ids = unlockedSkills.Select(skill => skill.Id).ToList();
        ES3.Save("player_unlocked_skills", ids);
    }

    public void Load()
    {
        if (!ES3.KeyExists("player_unlocked_skills"))
            return;

        var ids = ES3.Load<List<string>>("player_unlocked_skills");

        unlockedSkills.Clear();

        foreach (var skill in ids.Select(id => SkillDatabase.Get(id)).Where(skill => skill != null))
        {
            unlockedSkills.Add(skill);
        }
    }
}

