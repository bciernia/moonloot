using System.Linq;
using UnityEngine;

public abstract class Effect : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    [TextArea] public string Description;

    public float Duration;
    public float TickInterval = 1f;
    public GameObject VisualPrefab;

    [Range(0, 100)] public int BasicChanceForHit;

    protected abstract void OnTick(GameObject target);

    public void Apply(GameObject target, Skill sourceSkill = null, float hitChance = 0)
    {
        var uiObj = target.CompareTag("Player") ? StatusEffectUIManager.Instance.CreateEffectUI(this) : null;

        var effectHitChance = BasicChanceForHit + hitChance;
        
        if (!RNGManager.Instance.MakeSkillCheck(effectHitChance)) return;
        
        var existingEffects =
            target.GetComponents<ActiveDmgOverTime>();
        
        var existing = existingEffects
            .FirstOrDefault(e => e.Effect == this);
        
        ActiveDmgOverTime activeEffect;

        if (existing != null)
        {
            existing.ResetEffect();
            activeEffect = existing; 
        }
        else
        {
            activeEffect = target.AddComponent<ActiveDmgOverTime>();
            activeEffect.InitializeEffect(this, sourceSkill, target, uiObj, OnTick);
        }

        if (VisualPrefab != null)
        {
            GameObject visual;

            if (activeEffect.VisualObject == null)
            {
                visual = Instantiate(VisualPrefab, target.transform.position, Quaternion.identity);
                visual.transform.SetParent(target.transform);
                activeEffect.VisualObject = visual;
            }
            else
            {
                visual = activeEffect.VisualObject; 
            }

            var controller = visual.GetComponent<SkillVisualEffectController>();
            if (controller != null)
            {
                controller.Duration = Duration;
                controller.RestartAnimation();
            }
        }
    }
}