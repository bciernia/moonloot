using UnityEngine;

public class Skill : ScriptableObject
{
    public string Name;
    [TextArea] public string Description;

    public float Cooldown;
    public float ActiveTime;

    public float HealthCost;
    public float ManaCost;

    public Sprite Icon;
    public bool IsInUse;

    public Effect Effect;

    public virtual void Activate(GameObject user)
    {
        if (Effect != null)
            Effect.Apply(user);
    }
}