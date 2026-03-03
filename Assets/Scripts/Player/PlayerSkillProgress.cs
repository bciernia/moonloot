using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerSkillProgress : MonoBehaviour
{
    public List<Skill> unlockedSkills = new List<Skill>();

    public event Action OnSkillsChanged;

    [SerializeField] public Skill skill;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            UnlockSkill(skill);
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
}

