using System;
using UnityEngine;

public class SkillsManager : MonoBehaviour
{
    public Skill skill;
    private float cooldownTime;
    private float activeTime;

    enum SkillState
    {
        ready,
        active,
        cooldown,
    }

    private SkillState skillState = SkillState.ready;

    public KeyCode key;

    private void Update()
    {
        switch (skillState)
        {
            case SkillState.ready:
                if (Input.GetKeyDown(key))
                {
                    skill.Activate();
                    skillState = SkillState.active;
                    activeTime = skill.ActiveTime;
                }
                break;
            case SkillState.active:
                if (activeTime > 0)
                {
                    activeTime -= Time.deltaTime;
                }
                else
                {
                    skillState = SkillState.cooldown;
                    cooldownTime = skill.Cooldown;
                }
                break;
            case SkillState.cooldown:
                if (cooldownTime > 0)
                {
                    cooldownTime -= Time.deltaTime;
                }
                else
                {
                    skillState = SkillState.ready;
                }
                break;
        }
    }
}
