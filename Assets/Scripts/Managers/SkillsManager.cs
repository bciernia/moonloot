using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public Image cooldownImage;

    [NonSerialized] public float cooldownTimer;
    [NonSerialized] public float activeTimer;
    [NonSerialized] public SkillState state = SkillState.ready;
}

public class SkillsManager : Singleton<SkillsManager>
{
    public List<SkillEntry> skills = new List<SkillEntry>();

    public GameObject user;

    [SerializeField] private Image QSkillImage;
    [SerializeField] private Image ESkillImage;

    protected override void Awake()
    {
        base.Awake();
        QSkillImage.sprite = skills[0].skill.Icon;
        ESkillImage.sprite = skills[1].skill.Icon;
    }

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
                        entry.cooldownImage.fillAmount = 0f;
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
                        
                        if (entry.skill.Cooldown > 0)
                        {
                            var progress = 1f - (entry.cooldownTimer / entry.skill.Cooldown);
                            entry.cooldownImage.fillAmount = progress;
                        }
                    }
                    else
                    {
                        entry.state = SkillState.ready;
                        entry.cooldownImage.fillAmount = 1f;
                    }
                    break;
            }
        }
    }
}

