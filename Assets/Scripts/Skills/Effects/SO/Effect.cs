using UnityEngine;

public abstract class Effect : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    [TextArea] public string Description;

    public float Duration;
    public float TickInterval = 1f;
    public GameObject VisualPrefab;

    protected abstract void OnTick(GameObject target);

    public void Apply(GameObject target)
    {
        var uiObj = StatusEffectUIManager.Instance.CreateEffectUI(this, target);

        var existing = target.GetComponent<ActiveDmgOverTime>();
        ActiveDmgOverTime activeEffect;

        if (existing != null)
        {
            existing.ResetEffect();
            activeEffect = existing; 
        }
        else
        {
            activeEffect = target.AddComponent<ActiveDmgOverTime>();
            activeEffect.InitializeEffect(this, target, uiObj, OnTick);
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