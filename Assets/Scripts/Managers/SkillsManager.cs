using System;
using System.Collections.Generic;
using UnityEngine;

public enum SkillState
{
    ready,
    active,
    cooldown,
}

[Serializable]
public class SkillEntry
{
    public Skill skill;
    public KeyCode key;

    [NonSerialized] public float cooldownTimer;
    [NonSerialized] public float activeTimer;
    [NonSerialized] public SkillState state = SkillState.ready;
}

public class SkillsManager : MonoBehaviour
{
    public List<SkillEntry> skills = new List<SkillEntry>();

    public GameObject user;

    private void Update()
    {
        foreach (var entry in skills)
        {
            if(!entry.skill.IsInUse)
                continue;
            
            switch (entry.state)
            {
                case SkillState.ready:
                    if (Input.GetKeyDown(entry.key))
                    {
                        entry.skill.Activate(user);
                        entry.state = SkillState.active;
                        entry.activeTimer = entry.skill.ActiveTime;
                    }
                    break;

                case SkillState.active:
                    if (entry.activeTimer > 0)
                    {
                        entry.activeTimer -= Time.deltaTime;
                    }
                    else
                    {
                        entry.state = SkillState.cooldown;
                        entry.cooldownTimer = entry.skill.Cooldown;
                    }
                    break;

                case SkillState.cooldown:
                    if (entry.cooldownTimer > 0)
                    {
                        entry.cooldownTimer -= Time.deltaTime;
                    }
                    else
                    {
                        entry.state = SkillState.ready;
                    }
                    break;
            }
        }
    }
}

