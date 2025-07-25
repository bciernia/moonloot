using UnityEngine;

public class Skill : ScriptableObject
{
    public string Name;
    public float Cooldown;
    public float ActiveTime;
    public float HealthCost;
    public float ManaCost;
    public Sprite Icon;

    public virtual void Activate() {}
}
