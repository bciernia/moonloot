using UnityEngine;

[CreateAssetMenu(
    fileName = "FrenzyUnbound_",
    menuName = "Skill/FrenzyUnbound")]
public class FrenzyUnbound : Skill
{
    [Header("Attack Speed")]
    [Range(0.1f, 1f)]
    public float AttackCooldownMultiplier = 0.5f;

    public override bool Activate(GameObject user)
    {
        if (user == null)
            return false;

        var hp = user.GetComponent<PlayerHealth>();

        if (hp != null &&
            hp.CurrentHealth <= HealthCost)
        {
            Debug.Log("Hp is too low");
            FloatingTextManager.Instance.ShowWarningText("HP is too low", user.transform);
            return false;
        }
        
        hp?.TakeDamage(
            HealthCost,
            null,
            DamageType.True);

        var duration =
            PlayerSkillManager.Instance.GetSkillStat(
                this,
                SkillStatType.Duration,
                ActiveTime
            );

        var cooldownMultiplier =
            PlayerSkillManager.Instance.GetSkillStat(
                this,
                SkillStatType.Cooldown,
                AttackCooldownMultiplier
            );

        var playerAttack =
            user.GetComponent<PlayerAttack>();

        if (playerAttack != null)
        {
            playerAttack.ApplyAttackCooldownMultiplier(
                cooldownMultiplier,
                duration);
        }

        return true;
    }

    public override string GetDescription()
    {
        var duration = PlayerSkillManager.Instance.GetSkillStat(this, SkillStatType.Duration, ActiveTime);
        var cooldownMultiplier = PlayerSkillManager.Instance.GetSkillStat(this, SkillStatType.Cooldown, AttackCooldownMultiplier);
        var cooldownReduction = (1f - cooldownMultiplier) * 100f;
        
        return $"Enter Udros’s battle trance at a cost of {HealthCost:0.#} Health.\n\nReduce the time between your attacks by {cooldownReduction:0.#}% for {duration:0.#} seconds, letting you press the advantage before the trance fades.";
    }
}
