using System;
using System.Collections.Generic;
using System.Net.Mime;
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

internal class ActiveEffect 
{
    public SkillEntry Skill;
    public GameObject EffectObject;
}

public class SkillsManager : Singleton<SkillsManager>
{
    public List<SkillEntry> skills = new List<SkillEntry>();

    public GameObject user;

    [SerializeField] private Image QSkillImage;
    [SerializeField] private Image ESkillImage;

    [SerializeField] private GameObject EffectsContainer;
    [SerializeField] private GameObject EffectPrefab;

    private List<ActiveEffect> activeEffects = new List<ActiveEffect>();
    
    protected override void Awake()
    {
        base.Awake();
        QSkillImage.sprite = skills[0].skill.Icon;
        ESkillImage.sprite = skills[1].skill.Icon;
    }
    
    private GameObject CreateEffectPrefabForSkill(Skill skill)
    {
        var instance = Instantiate(EffectPrefab, EffectsContainer.transform);
    
        var tooltipTrigger = instance.GetComponent<TooltipTrigger>();
        var image = instance.GetComponent<Image>();
    
        tooltipTrigger.header = skill.Name;
        tooltipTrigger.content = skill.Description;
        image.sprite = skill.Icon;

        return instance;
    }
    
    private void Update()
    {
        foreach (var entry in skills)
        {
            if(!entry.skill.IsInUse)
                continue;

            if (Input.GetKeyDown(KeyCode.P))
            {
            }
            
            switch (entry.state)
            {
                case SkillState.ready:
                    if (Input.GetKeyDown(entry.key))
                    {
                        entry.skill.Activate(user);
                        entry.state = SkillState.active;
                        entry.activeTimer = entry.skill.ActiveTime;

                        if (entry.skill.HasEffectIcon)
                        {
                            var effect = CreateEffectPrefabForSkill(entry.skill);
                            activeEffects.Add(new ActiveEffect { Skill = entry, EffectObject = effect });
                        }
                    }
                    break;

                case SkillState.active:
                    if (entry.activeTimer > 0)
                    {
                        entry.activeTimer -= Time.deltaTime;
                        
                        if (entry.skill.ActiveTime > 0)
                        {
                            var progress = entry.activeTimer / entry.skill.ActiveTime;
                            entry.cooldownImage.fillAmount = progress;
                        }
                        
                    }
                    else
                    {
                        entry.state = SkillState.cooldown;
                        entry.cooldownTimer = entry.skill.Cooldown;
                        
                        var toRemove = activeEffects.Find(e => e.Skill == entry);
                        if (toRemove != null)
                        {
                            activeEffects.Remove(toRemove);
                            Destroy(toRemove.EffectObject);
                        }
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

