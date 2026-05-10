using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Healing", menuName = "Effects/Healing")]
public class HealingEffect : Effect
{
    [SerializeField] private float healPerTick;

    protected override void OnTick(GameObject target)
    {
        var active = target
            .GetComponents<ActiveDmgOverTime>()
            .FirstOrDefault(e => e.Effect == this);

        if (active == null)
            return;

        var skill = active.SourceSkill;

        var finalHeal = healPerTick;
        
        if (skill != null)
        {
            finalHeal =
                PlayerSkillManager.Instance.GetSkillStat(
                    skill,
                    SkillStatType.HealAmount,
                    healPerTick
                );
        }
        
        var health = target.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.RestoreHealth(finalHeal);
        }
    }
}