using UnityEngine;

public class Skill : IdentifiableSO
{
    public string Name;
    [TextArea] public string Description;
    [TextArea] public string NpcDescription;

    public float Cooldown;
    public float ActiveTime;

    public float HealthCost;
    public float ManaCost;

    public Sprite Icon;

    public Effect Effect;

    public AudioClip SFX;

    public TargetType TargetingType;
    public float Radius;

    public Color RangeIndicatorMinColor;
    public Color RangeIndicatorMaxColor;

    public virtual bool Activate(GameObject user)
    {
        if (Effect != null)
            Effect.Apply(user, this);

        return true;
    }
}