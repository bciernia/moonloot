using System;
using System.Collections.Generic;
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
        ES3.Save("player_unlocked_skills", unlockedSkills);
    }

    public void Load()
    {
        if (ES3.KeyExists("player_unlocked_skills"))
        {
            unlockedSkills = ES3.Load<List<Skill>>("player_unlocked_skills");
        }
    }
}

