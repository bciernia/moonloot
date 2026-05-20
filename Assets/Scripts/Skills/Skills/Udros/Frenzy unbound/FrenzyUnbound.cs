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
}