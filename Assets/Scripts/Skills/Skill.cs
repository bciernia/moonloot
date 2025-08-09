using System.ComponentModel;
using UnityEngine;

public class Skill : ScriptableObject
{
    public string Name;
    [TextArea]
    public string Description;
    public float Cooldown;
    public float ActiveTime;
    public float HealthCost;
    public float ManaCost;
    public Sprite Icon;
    public bool IsInUse;
    public GameObject SkillBtn;
    public bool HasEffectIcon = true;

    public virtual void Activate(GameObject user) {}
}
