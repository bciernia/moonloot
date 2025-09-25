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
    [Header("Skill Config")]
    public List<SkillEntry> skills = new List<SkillEntry>();
    public GameObject user;

    [Header("UI References")]
    [SerializeField] private Image QSkillImage;
    [SerializeField] private Image ESkillImage;

    [Header("Effect Prefabs")]
    [SerializeField] private GameObject EffectsContainer;
    [SerializeField] private GameObject EffectPrefab;

    private List<ActiveEffect> activeEffects = new List<ActiveEffect>();

    private PlayerActions _actions;

    protected override void Awake()
    {
        base.Awake();
        _actions = new PlayerActions();

        if (skills.Count > 0) QSkillImage.sprite = skills[0].skill.Icon;
        if (skills.Count > 1) ESkillImage.sprite = skills[1].skill.Icon;
    }

    private void OnEnable()
    {
        _actions.Enable();

        _actions.Skills.SkillQ.performed += ctx => TryActivateSkill(0);
        _actions.Skills.SkillE.performed += ctx => TryActivateSkill(1);
    }

    private void OnDisable()
    {
        _actions.Disable();
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

    private void TryActivateSkill(int index)
    {
        if (index < 0 || index >= skills.Count) return;

        var entry = skills[index];
        if (!entry.skill.IsInUse) return;
        if (entry.state != SkillState.ready) return;

        entry.skill.Activate(user);
        entry.state = SkillState.active;
        entry.activeTimer = entry.skill.ActiveTime;

        if (entry.skill.HasEffectIcon)
        {
            var effect = CreateEffectPrefabForSkill(entry.skill);
            activeEffects.Add(new ActiveEffect { Skill = entry, EffectObject = effect });
        }
    }

    private void Update()
    {
        foreach (var entry in skills)
        {
            switch (entry.state)
            {
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
